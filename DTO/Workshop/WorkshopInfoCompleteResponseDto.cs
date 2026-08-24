using MiTaller.DTO.Address;
using MiTaller.DTO.Review;
using MiTaller.DTO.Workshop.Services;
using MiTaller.Models.Workshop;

namespace MiTaller.DTO.Workshop
{
    public class WorkshopInfoCompleteResponseDto
    {
        public Guid WorkshopId { get; set; }
        public string AssociateName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string WorkshopName { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Latitude { get; set; } = string.Empty;
        public string Longitude { get; set; } = string.Empty;
        public AccountAddressDto? Address { get; set; }
        public string? OneLineAddress { get; set; }
        public ReviewResponseDto? Reviews { get; set; }
        public List<WorkshopServiceResponseDto>? Services { get; set; }
        public byte[]? ProfileImage { get; set; }

    }
}
