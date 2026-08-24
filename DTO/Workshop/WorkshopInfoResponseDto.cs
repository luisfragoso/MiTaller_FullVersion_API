using MiTaller.DTO.Address;

namespace MiTaller.DTO.Workshop
{
    public class WorkshopInfoResponseDto
    {
        public Guid WorkshopId { get; set; }
        public string AssociateName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Landline { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string WorkshopName {  get; set; } = string.Empty;
        public string Type {  get; set; } = string.Empty;
        public byte[]? ProfileImage { get; set; }
        public string? Latitude { get; set; } = string.Empty;
        public string? Longitude { get; set; } = string.Empty;
        public AccountAddressDto? Address { get; set; }
        public string? OneLineAddress { get; set; }
        public float ReviewAverageRate { get; set; } = 0;
    }
}
