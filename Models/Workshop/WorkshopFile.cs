using MiTaller.Models.Domain;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MiTaller.Models.Workshop
{
    public class WorkshopFile
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid WorkshopId { get; set; }

        [ForeignKey("WorkshopId")]
        public Workshop Workshop{ get; set; } = null!;

        [Required]
        public FileModel File { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.Now;
    }
}
