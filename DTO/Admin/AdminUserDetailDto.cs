namespace MiTaller.DTO.Admin
{
    public class AdminUserDetailDto
    {
        public Guid Id { get; set; }
        public string UserType { get; set; } = string.Empty; // "Customer" | "Workshop"
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public string? Address { get; set; }

        // Solo se llena para clientes.
        public List<AdminVehicleSummaryDto> Vehicles { get; set; } = new();
        public List<AdminQuotationSummaryDto> Quotations { get; set; } = new();
        public List<AdminAppointmentSummaryDto> Appointments { get; set; } = new();
        public List<AdminWorkshopSummaryDto> VisitedWorkshops { get; set; } = new();
        public List<AdminTagSummaryDto> Tags { get; set; } = new();

        // Solo se llena para talleres.
        public List<AdminWorkshopServiceSummaryDto> WorkshopServices { get; set; } = new();
        public List<AdminCustomerSummaryDto> LinkedCustomers { get; set; } = new();
        public List<AdminReviewSummaryDto> Reviews { get; set; } = new();
        public double? AverageRating { get; set; }
    }

    public class AdminWorkshopSummaryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class AdminCustomerSummaryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class AdminReviewSummaryDto
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public float Rate { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime Date { get; set; }
    }

    public class AdminTagSummaryDto
    {
        public int Id { get; set; }
        public string Value { get; set; } = string.Empty;
        public string HexColor { get; set; } = string.Empty;
        public string WorkshopName { get; set; } = string.Empty;
    }

    public class AdminVehicleSummaryDto
    {
        public int Id { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Plates { get; set; } = string.Empty;
        public string Year { get; set; } = string.Empty;
    }

    public class AdminQuotationSummaryDto
    {
        public int Id { get; set; }
        public string WorkshopName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public double Total { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AdminAppointmentSummaryDto
    {
        public int Id { get; set; }
        public string WorkshopName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
    }

    public class AdminWorkshopServiceSummaryDto
    {
        public int Id { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public double Price { get; set; }
    }
}
