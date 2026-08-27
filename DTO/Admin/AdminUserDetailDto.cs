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

        // Solo se llena para clientes.
        public List<AdminVehicleSummaryDto> Vehicles { get; set; } = new();
        public List<AdminQuotationSummaryDto> Quotations { get; set; } = new();
        public List<AdminAppointmentSummaryDto> Appointments { get; set; } = new();

        // Solo se llena para talleres.
        public List<AdminWorkshopServiceSummaryDto> WorkshopServices { get; set; } = new();
        public int LinkedCustomersCount { get; set; }
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
