using MiTaller.Models.Domain;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiTaller.Models.Customer
{
    public class CustomerFile
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public Customer Customer { get; set; } = null!;

        [Required]
        public FileModel File { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.Now;
    }
}
