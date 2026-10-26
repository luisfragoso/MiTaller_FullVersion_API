using MiTaller.DTO.Tag;

namespace MiTaller.DTO.Workshop
{
    public class WorkshopCustomersDto
    {
        public Guid CustomerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public byte[]? ProfileImage { get; set; }
        public List<CustomerTagsDto>? Tags { get; set; }

        // Customers can come from appointments/quotations/direct adds without ever being inspected here
        public bool HasInspection => InspectedVehicles.Count > 0;
        public List<WorkshopCustomerInspectedVehicleDto> InspectedVehicles { get; set; } = new();
    }
}
