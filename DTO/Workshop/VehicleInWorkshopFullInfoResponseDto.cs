using MiTaller.DTO.Inspections;
using MiTaller.DTO.Vehicle;

namespace MiTaller.DTO.Workshop
{
    public class VehicleInWorkshopFullInfoResponseDto
    {
        public int WorkshopInspectionId { get; set; }
        public Guid CustomerId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public byte[]? ProfileImage { get; set; }
        public string Status { get; set; } = string.Empty;
        public VehicleResponseDto Vehicle { get; set; }
        public List<VehicleInspectionHistoryResponseDto> InspectionHistory { get; set; }
        public int QuotationCount { get; set; }
    }
}
