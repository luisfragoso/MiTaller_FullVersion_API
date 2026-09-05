using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MiTaller.Models.Audit;

namespace MiTaller.Models.Inspections
{
    /// <summary>
    /// History/audit trail for a generic Inspection. Unlike
    /// VehicleInspectionHistory/VehicleInspectionDetailHistory (car), this
    /// keeps a simplified JSON snapshot of the results instead of a
    /// per-item detail table, since the set of items is dynamic per template.
    /// </summary>
    public class InspectionHistory : INotAudited
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int InspectionId { get; set; }
        [ForeignKey("InspectionId")]
        public Inspection? Inspection { get; set; }

        public DateTime ChangedAt { get; set; } = DateTime.Now;

        // Identifier (NameIdentifier claim) of the user who made the change, when available.
        public string? ChangedBy { get; set; }

        public string Status { get; set; } = "Pendiente";

        // Simplified JSON snapshot of the results at this point in time
        // (sectionId/itemId/value/comments), instead of a per-item detail table.
        public string ResultsSnapshotJson { get; set; } = string.Empty;
    }
}
