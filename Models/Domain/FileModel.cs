using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MiTaller.Models.Domain
{
    public class FileModel
    {

        [Required]
        public string FileName { get; set; } = string.Empty;

        [Required]
        public string FileType { get; set; } = string.Empty; // "image/png", "application/pdf", etc.

        [Required]
        public byte[] FileData { get; set; } = Array.Empty<byte>(); // Archivo en bytes

        public string FileDescription { get; set; } = string.Empty; // INE, INE FRONTAL
    }
}
