using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiTaller.Services;

namespace MiTaller.Controllers
{
    /// <summary>
    /// Mercado Pago notifications. The body is never trusted: after checking the signature the order is
    /// fetched from MP with our token, so a forged call can at most re-read a real order.
    /// </summary>
    [Route("api/payments/mercadopago")]
    [ApiController]
    [AllowAnonymous]
    public class MercadoPagoWebhookController : ControllerBase
    {
        private readonly MercadoPagoClient _mp;
        private readonly SubscriptionService _subscriptions;
        private readonly ILogger<MercadoPagoWebhookController> _logger;

        public MercadoPagoWebhookController(MercadoPagoClient mp, SubscriptionService subscriptions, ILogger<MercadoPagoWebhookController> logger)
        {
            _mp = mp;
            _subscriptions = subscriptions;
            _logger = logger;
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook()
        {
            var type = Request.Query["type"].FirstOrDefault() ?? Request.Query["topic"].FirstOrDefault();
            var dataId = Request.Query["data.id"].FirstOrDefault() ?? Request.Query["id"].FirstOrDefault();
            if (!_mp.IsValidSignature(Request.Headers["x-signature"], Request.Headers["x-request-id"], dataId))
            {
                _logger.LogWarning("Mercado Pago webhook with invalid signature (type {Type}, id {Id})", type, dataId);
                return Unauthorized();
            }
            // Other topics (subscriptions, chargebacks) are acknowledged until they're handled.
            if (type != "order" || string.IsNullOrWhiteSpace(dataId)) return Ok();

            var order = await _mp.GetOrderAsync(dataId);
            if (order == null) return StatusCode(StatusCodes.Status502BadGateway); // MP retries later
            var payment = await _subscriptions.ApplyOrderAsync(order);
            _logger.LogInformation("Mercado Pago order {OrderId} status {Status}/{Detail} -> payment {PaymentId} {PaymentStatus}",
                order.Id, order.Status, order.StatusDetail, payment?.Id, payment?.Status);
            return Ok();
        }
    }
}
