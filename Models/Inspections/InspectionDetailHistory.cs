using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MiTaller.Models.Audit;

namespace MiTaller.Models.Inspections
{
    /// <summary>
    /// Customer-facing progress-update timeline for a generic Inspection -
    /// the equivalent of VehicleInspectionDetailHistory (car) /
    /// MotocycleInspectionDetailHistory (moto). Not to be confused with
    /// InspectionHistory, which is a separate (and currently unused)
    /// audit/snapshot concept.
    /// </summary>
    public class InspectionDetailHistory : INotAudited
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int InspectionId { get; set; }
        [ForeignKey(nameof(InspectionId))]
        public Inspection? Inspection { get; set; }

        public string Title { get; set; } = string.Empty;
        public bool IsCompleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? CompletedAt { get; set; } = null;
    }
}
