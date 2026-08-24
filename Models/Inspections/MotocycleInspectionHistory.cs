using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MiTaller.Models.Inspections
{
    public class MotocycleInspectionHistory
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("MotocycleInspectionId")]
        public int MotocycleInspectionId { get; set; }
        public WorkshopMotocycleInspection MotocycleInspection { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Folio { get; set; } = string.Empty;
        public byte[]? File { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
