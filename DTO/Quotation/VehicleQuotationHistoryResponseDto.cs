using MiTaller.DTO.Vehicle;
using MiTaller.DTO.Workshop.Services;

namespace MiTaller.DTO.Quotation
{
    public class VehicleQuotationHistoryResponseDto
    {
        public int QuotationId { get; set; }
        public string Description { get; set; } = string.Empty;
        public float PriceOfLabor { get; set; } = 0;
        public float PriceOfSpareParts { get; set; } = 0;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
