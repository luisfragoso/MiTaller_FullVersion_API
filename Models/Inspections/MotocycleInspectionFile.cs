using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MiTaller.Models.Inspections
{
    public class MotocycleInspectionFile
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int WorkshopMotocycleInspectionId { get; set; }
        [ForeignKey("WorkshopMotocycleInspectionId")]
        public WorkshopMotocycleInspection WorkshopMotocycleInspection { get; set; }

        [Required]
        public string FileName { get; set; } = string.Empty;

        [Required]
        public string FileType { get; set; } = string.Empty; // "image/png", "application/pdf", etc.

        [Required]
        public byte[] FileData { get; set; } = Array.Empty<byte>();
    }
}
