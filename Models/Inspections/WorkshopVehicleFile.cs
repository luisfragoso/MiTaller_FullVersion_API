using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using MiTaller.Models.Vehicle;

namespace MiTaller.Models.Inspections
{
    public class WorkshopVehicleFile
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int WorkshopVehicleInspectionId { get; set; }
        [ForeignKey("WorkshopVehicleInspectionId")]
        public WorkshopVehicleInspection WorkshopVehicleInspection { get; set; }

        [Required]
        public string FileName { get; set; } = string.Empty;

        [Required]
        public string FileType { get; set; } = string.Empty; // "image/png", "application/pdf", etc.

        [Required]
        public byte[] FileData { get; set; } = Array.Empty<byte>();
    }
}
