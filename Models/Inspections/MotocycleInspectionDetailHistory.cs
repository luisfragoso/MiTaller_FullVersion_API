using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using MiTaller.Models.Audit;

namespace MiTaller.Models.Inspections
{
    public class MotocycleInspectionDetailHistory : INotAudited
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("MotocycleInspectionId")]
        public int MotocycleInspectionId { get; set; }
        public WorkshopMotocycleInspection MotocycleInspection { get; set; }
        public string Title { get; set; } = string.Empty;
        public bool IsCompleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? CompletedAt { get; set; } = null;
    }
}
