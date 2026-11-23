using System.Globalization;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace MiTaller.Services
{
    /// <summary>Credentials live only in server config (MercadoPago__AccessToken / __WebhookSecret), never in code.</summary>
    public class MercadoPagoSettings
    {
        public string AccessToken { get; set; } = string.Empty;
        public string WebhookSecret { get; set; } = string.Empty;
        public string ApiBaseUrl { get; set; } = "https://api.mercadopago.com";
        // Where the shop lands after paying; the platform reads ?pago={id} and confirms it.
        public string ReturnUrl { get; set; } = "https://plataforma.mitaller.io/suscripcion";
    }

    public record MpOrder(
        string Id,
        string Status,
        string? StatusDetail,
        string? ExternalReference,
        decimal TotalPaidAmount,
        string? PaymentId,
        string? PaymentMethodType);

    /// <summary>Checkout Pro through the Orders API: card data stays on Mercado Pago's pages.</summary>
    public class MercadoPagoClient
    {
        private readonly HttpClient _http;
        private readonly MercadoPagoSettings _settings;
        private readonly ILogger<MercadoPagoClient> _logger;

        public MercadoPagoClient(HttpClient http, IOptions<MercadoPagoSettings> settings, ILogger<MercadoPagoClient> logger)
        {
            _http = http;
            _settings = settings.Value;
            _logger = logger;
            _http.BaseAddress = new Uri(_settings.ApiBaseUrl.TrimEnd('/') + "/");
            _http.Timeout = TimeSpan.FromSeconds(20);
        }

        public bool IsConfigured => !string.IsNullOrWhiteSpace(_settings.AccessToken);

        public string ReturnUrl => _settings.ReturnUrl;

        private HttpRequestMessage Request(HttpMethod method, string path)
        {
            var request = new HttpRequestMessage(method, path);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.AccessToken);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return request;
        }

        private static string Money(decimal amount) => amount.ToString("0.00", CultureInfo.InvariantCulture);

        /// <summary>Returns the order id and the checkout_url to send the buyer to, or null on failure.</summary>
        public async Task<(string OrderId, string CheckoutUrl)?> CreateOrderAsync(
            string externalReference, string title, decimal total, string? payerEmail, string returnUrl)
        {
            var body = new Dictionary<string, object?>
            {
                ["type"] = "online",
                ["processing_mode"] = "manual",
                ["total_amount"] = Money(total),
                ["external_reference"] = externalReference,
                ["description"] = title,
                ["items"] = new[] { new { title, unit_price = Money(total), quantity = 1 } },
                ["config"] = new
                {
                    online = new
                    {
                        success_url = returnUrl,
                        failure_url = returnUrl,
                        pending_url = returnUrl,
                        auto_return = "all",
                    },
                },
            };
            if (!string.IsNullOrWhiteSpace(payerEmail)) body["payer"] = new { email = payerEmail };

            using var request = Request(HttpMethod.Post, "v1/orders");
            request.Headers.Add("X-Idempotency-Key", Guid.NewGuid().ToString());
            request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            using var response = await _http.SendAsync(request);
            var text = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Mercado Pago create order failed {Status}: {Body}", (int)response.StatusCode, text);
                return null;
            }
            using var doc = JsonDocument.Parse(text);
            var root = doc.RootElement;
            var id = root.TryGetProperty("id", out var idEl) ? idEl.GetString() : null;
            var url = root.TryGetProperty("checkout_url", out var urlEl) ? urlEl.GetString() : null;
            if (id == null || url == null)
            {
                _logger.LogWarning("Mercado Pago order without id or checkout_url: {Body}", text);
                return null;
            }
            return (id, url);
        }

        public async Task<MpOrder?> GetOrderAsync(string orderId)
        {
            using var request = Request(HttpMethod.Get, $"v1/orders/{Uri.EscapeDataString(orderId)}");
            using var response = await _http.SendAsync(request);
            var text = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Mercado Pago get order {OrderId} failed {Status}: {Body}", orderId, (int)response.StatusCode, text);
                return null;
            }
            using var doc = JsonDocument.Parse(text);
            var root = doc.RootElement;
            string? Str(JsonElement e, string name) =>
                e.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;

            string? paymentId = null, methodType = null;
            if (root.TryGetProperty("transactions", out var tx) && tx.TryGetProperty("payments", out var payments)
                && payments.ValueKind == JsonValueKind.Array && payments.GetArrayLength() > 0)
            {
                var last = payments[payments.GetArrayLength() - 1];
                paymentId = Str(last, "id");
                if (last.TryGetProperty("payment_method", out var pm)) methodType = Str(pm, "type");
            }
            decimal.TryParse(Str(root, "total_paid_amount"), NumberStyles.Number, CultureInfo.InvariantCulture, out var paid);
            return new MpOrder(Str(root, "id") ?? orderId, Str(root, "status") ?? "", Str(root, "status_detail"),
                Str(root, "external_reference"), paid, paymentId, methodType);
        }

        /// <summary>
        /// x-signature is "ts=...,v1=..."; v1 = HMAC-SHA256(secret, "id:{data.id};request-id:{x-request-id};ts:{ts};"),
        /// with an alphanumeric data.id in lower case and missing parts left out of the manifest.
        /// </summary>
        public bool IsValidSignature(string? signatureHeader, string? requestId, string? dataId)
        {
            if (string.IsNullOrWhiteSpace(_settings.WebhookSecret) || string.IsNullOrWhiteSpace(signatureHeader)) return false;
            string? ts = null, v1 = null;
            foreach (var part in signatureHeader.Split(','))
            {
                var kv = part.Split('=', 2, StringSplitOptions.TrimEntries);
                if (kv.Length != 2) continue;
                if (kv[0] == "ts") ts = kv[1];
                else if (kv[0] == "v1") v1 = kv[1];
            }
            if (ts == null || v1 == null) return false;

            var manifest = new StringBuilder();
            if (!string.IsNullOrEmpty(dataId)) manifest.Append($"id:{dataId.ToLowerInvariant()};");
            if (!string.IsNullOrEmpty(requestId)) manifest.Append($"request-id:{requestId};");
            manifest.Append($"ts:{ts};");

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_settings.WebhookSecret));
            var expected = Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(manifest.ToString()))).ToLowerInvariant();
            return CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(expected), Encoding.ASCII.GetBytes(v1.ToLowerInvariant()));
        }
    }
}
