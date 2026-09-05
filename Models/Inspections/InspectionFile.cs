using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiTaller.Models.Inspections
{
    /// <summary>
    /// Same shape as WorkshopVehicleFile/MotocycleInspectionFile: files are
    /// stored inline as bytes in the DB, not as a path on disk.
    /// </summary>
    public class InspectionFile
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int InspectionId { get; set; }
        [ForeignKey("InspectionId")]
        public Inspection? Inspection { get; set; }

        [Required]
        public string FileName { get; set; } = string.Empty;

        [Required]
        public string FileType { get; set; } = string.Empty; // "image/png", "application/pdf", etc.

        [Required]
        public byte[] FileData { get; set; } = Array.Empty<byte>();
    }
}
