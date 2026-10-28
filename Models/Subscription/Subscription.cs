using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiTaller.Models.Subscription
{
    public enum SubscriptionStatus { Trial, Active, PastDue, Canceled }

    public enum BillingPeriod { Monthly, Annual }

    public enum PaymentMethod { None, AutoCard, Manual }

    public enum DiscountType { Percent, Fixed }

    public enum PaymentStatus { Pending, Approved, Rejected, Refunded }

    /// <summary>Single row (Id = 1) edited from Portal Admin; prices are before IVA.</summary>
    public class SubscriptionSettings
    {
        public int Id { get; set; }
        [Column(TypeName = "decimal(10,2)")]
        public decimal MonthlyPrice { get; set; } = 199m;
        [Column(TypeName = "decimal(5,4)")]
        public decimal IvaRate { get; set; } = 0.16m;
        // Annual price = MonthlyPrice x AnnualPaidMonths for 12 months of service.
        public int AnnualPaidMonths { get; set; } = 10;
        public int TrialDays { get; set; } = 90;
        public int GraceDays { get; set; } = 7;
        public int FreeMaxEmployees { get; set; } = 1;
        public int FreeMaxVehiclesPerMonth { get; set; } = 20;
        // Workshops registered before this date start their trial here instead of at signup.
        public DateTime LaunchDate { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class WorkshopSubscription
    {
        public int Id { get; set; }
        public Guid WorkshopId { get; set; }
        public Workshop.Workshop? Workshop { get; set; }
        public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Trial;
        public BillingPeriod? BillingPeriod { get; set; }
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.None;
        public DateTime TrialEndsAt { get; set; }
        public DateTime? CurrentPeriodEnd { get; set; }
        public DateTime? GraceEndsAt { get; set; }
        [MaxLength(100)]
        public string? MpPreapprovalId { get; set; }
        public int? DiscountCodeId { get; set; }
        public DiscountCode? DiscountCode { get; set; }
        // Charges the code still discounts; null = every charge.
        public int? DiscountChargesRemaining { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }

    public class SubscriptionPayment
    {
        public int Id { get; set; }
        public Guid WorkshopId { get; set; }
        public BillingPeriod BillingPeriod { get; set; }
        public PaymentMethod Method { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        [Column(TypeName = "decimal(10,2)")]
        public decimal Subtotal { get; set; }
        [Column(TypeName = "decimal(10,2)")]
        public decimal Discount { get; set; }
        [Column(TypeName = "decimal(10,2)")]
        public decimal Iva { get; set; }
        [Column(TypeName = "decimal(10,2)")]
        public decimal Total { get; set; }
        [MaxLength(3)]
        public string Currency { get; set; } = "MXN";
        [MaxLength(100)]
        public string? MpPaymentId { get; set; }
        public DateTime? PeriodStart { get; set; }
        public DateTime? PeriodEnd { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>Channel a discount code came from (Distribuidor, Expo...), picked by the workshop from a dropdown.</summary>
    public class DiscountSource
    {
        public int Id { get; set; }
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }

    public class DiscountCode
    {
        public int Id { get; set; }
        [MaxLength(40)]
        public string Code { get; set; } = string.Empty;
        public BillingPeriod BillingPeriod { get; set; }
        public DiscountType Type { get; set; }
        [Column(TypeName = "decimal(10,2)")]
        public decimal Value { get; set; }
        // How many charges it discounts; null = all of them.
        public int? Charges { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidUntil { get; set; }
        public int? MaxUses { get; set; }
        public int UsedCount { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class DiscountRedemption
    {
        public int Id { get; set; }
        public int DiscountCodeId { get; set; }
        public DiscountCode? DiscountCode { get; set; }
        public Guid WorkshopId { get; set; }
        public int DiscountSourceId { get; set; }
        public DiscountSource? DiscountSource { get; set; }
        public DateTime RedeemedAt { get; set; } = DateTime.UtcNow;
    }
}
