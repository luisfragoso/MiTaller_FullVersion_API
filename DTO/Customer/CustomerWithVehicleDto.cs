using MiTaller.DTO.Vehicle;

namespace MiTaller.DTO.Customer
{
    public class CustomerWithVehicleDto
    {
        public Guid CustomerId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public byte[]? ProfileImage { get; set; }
    }
}
