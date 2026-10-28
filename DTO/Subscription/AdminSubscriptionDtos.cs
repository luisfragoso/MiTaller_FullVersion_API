using MiTaller.Models.Subscription;

namespace MiTaller.DTO.Subscription
{
    public class SubscriptionSettingsDto
    {
        public decimal MonthlyPrice { get; set; }
        public decimal IvaRate { get; set; }
        public int AnnualPaidMonths { get; set; }
        public int TrialDays { get; set; }
        public int GraceDays { get; set; }
        public int FreeMaxEmployees { get; set; }
        public int FreeMaxVehiclesPerMonth { get; set; }
        public DateTime LaunchDate { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class SubscriptionSummaryDto
    {
        public int Total { get; set; }
        public int Trial { get; set; }
        public int Active { get; set; }
        public int PastDue { get; set; }
        public int Free { get; set; }
    }

    public class DiscountSourceDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class PostDiscountSourceDto
    {
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }

    public class DiscountCodeDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public BillingPeriod BillingPeriod { get; set; }
        public DiscountType Type { get; set; }
        public decimal Value { get; set; }
        public int? Charges { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidUntil { get; set; }
        public int? MaxUses { get; set; }
        public int UsedCount { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        // Redemptions per channel the workshops picked.
        public Dictionary<string, int> UsesBySource { get; set; } = new();
    }

    public class PostDiscountCodeDto
    {
        public string Code { get; set; } = string.Empty;
        public BillingPeriod BillingPeriod { get; set; }
        public DiscountType Type { get; set; }
        public decimal Value { get; set; }
        public int? Charges { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidUntil { get; set; }
        public int? MaxUses { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class SubscriptionPaymentDto
    {
        public int Id { get; set; }
        public Guid WorkshopId { get; set; }
        // Only filled in the admin-wide payments list.
        public string? WorkshopName { get; set; }
        // Mercado Pago payment id: the folio the workshop sees on its MP receipt.
        public string? MpPaymentId { get; set; }
        public string BillingPeriod { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
        public decimal Iva { get; set; }
        public decimal Total { get; set; }
        public DateTime? PeriodStart { get; set; }
        public DateTime? PeriodEnd { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AdminWorkshopSubscriptionDto
    {
        public SubscriptionStatusDto Status { get; set; } = new();
        public string PaymentMethod { get; set; } = string.Empty;
        public string? DiscountCode { get; set; }
        public string? DiscountSource { get; set; }
        public List<SubscriptionPaymentDto> Payments { get; set; } = new();
    }

    public class ExtendTrialDto
    {
        public int Days { get; set; }
    }
}
