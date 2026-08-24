using System.ComponentModel.DataAnnotations;

namespace MiTaller.DTO.Inspections
{
    public class PostWorkshopVehicleFileDto
    {
        [Required]
        public string FileName { get; set; } = string.Empty;

        [Required]
        public string FileType { get; set; } = string.Empty;

        [Required]
        public byte[] FileData { get; set; } = Array.Empty<byte>();
    }
}
