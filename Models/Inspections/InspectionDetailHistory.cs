using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MiTaller.Models.Audit;

namespace MiTaller.Models.Inspections
{
    /// <summary>
    /// Customer-facing progress timeline of an Inspection (not the InspectionHistory audit).
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
