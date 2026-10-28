namespace MiTaller.DTO.Subscription
{
    public class SubscriptionStatusDto
    {
        public string Plan { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool InTrial { get; set; }
        public DateTime TrialEndsAt { get; set; }
        public int TrialDaysLeft { get; set; }
        public string? BillingPeriod { get; set; }
        public DateTime? CurrentPeriodEnd { get; set; }
        public DateTime? GraceEndsAt { get; set; }
        // Prices before IVA plus the totals actually charged.
        public decimal MonthlyPrice { get; set; }
        public decimal MonthlyTotal { get; set; }
        public decimal AnnualPrice { get; set; }
        public decimal AnnualTotal { get; set; }
        public decimal IvaRate { get; set; }
        public int FreeMaxEmployees { get; set; }
        public int FreeMaxVehiclesPerMonth { get; set; }
        public int Employees { get; set; }
        public int VehiclesThisMonth { get; set; }
    }
}
