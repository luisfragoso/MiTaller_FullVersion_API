using MiTaller.DTO.Vehicle;

namespace MiTaller.DTO.Inspections
{
    /// <summary>
    /// Same shape as VehicleInWorkshopFullInfoResponseDto (DTO/Workshop) but for
    /// the generic (template-driven) Inspections table, used by vehicle formats
    /// other than Automóvil/Motocicleta (e.g. Yate).
    /// </summary>
    public class InspectionInWorkshopFullInfoResponseDto
    {
        public int InspectionId { get; set; }
        public Guid CustomerId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public byte[]? ProfileImage { get; set; }
        public string Status { get; set; } = string.Empty;
        public string VehicleFormat { get; set; } = string.Empty;
        public VehicleResponseDto Vehicle { get; set; }
        public List<VehicleInspectionHistoryResponseDto> InspectionHistory { get; set; } = new();
        public int QuotationCount { get; set; }
    }
}
