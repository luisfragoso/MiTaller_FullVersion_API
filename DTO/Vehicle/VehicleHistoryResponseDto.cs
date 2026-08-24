using MiTaller.DTO.Inspections;
using MiTaller.DTO.Quotation;

namespace MiTaller.DTO.Vehicle
{
    public class VehicleHistoryResponseDto
    {
        public List<VehicleQuotationHistoryResponseDto>? QuotationHistory { get; set; }
        public List<VehicleInspectionHistoryResponseDto>? InspectionHistory { get; set; }
    }
}
