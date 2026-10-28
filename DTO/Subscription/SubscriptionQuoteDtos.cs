using MiTaller.Models.Subscription;

namespace MiTaller.DTO.Subscription
{
    public class SubscriptionQuoteRequestDto
    {
        public BillingPeriod BillingPeriod { get; set; }
        public string? Code { get; set; }
        // Channel the workshop picked for the code; required when Code is sent.
        public int? DiscountSourceId { get; set; }
    }

    /// <summary>What a payment would cost and cover right now. Nothing is charged or redeemed.</summary>
    public class SubscriptionQuoteDto
    {
        public string BillingPeriod { get; set; } = string.Empty;
        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
        public decimal Iva { get; set; }
        public decimal Total { get; set; }
        public DateTime CoverageStart { get; set; }
        public DateTime CoverageEnd { get; set; }
        public string? Code { get; set; }
        // How the code applies, e.g. "50% en 3 cobros"; null without a valid code.
        public string? CodeDescription { get; set; }
    }
}
