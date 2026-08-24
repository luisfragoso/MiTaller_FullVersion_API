using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MiTaller.DTO.Inspections
{
    public class VehicleInspectionHistoryResponseDto
    {
        public int VehicleInspectionHistoryId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Folio { get; set; } = string.Empty;
        public byte[]? File { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
