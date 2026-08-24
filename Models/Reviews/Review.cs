using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MiTaller.Models.Reviews
{
    public class Review
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public Guid CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public Customer.Customer Customer { get; set; }

        [Required]
        public Guid WorkshopId { get; set; }
        [ForeignKey("WorkshopId")]
        public Workshop.Workshop Workshop { get; set; }

        [Required]
        [StringLength(500, ErrorMessage = "El comentario no puede tener más de 500 caracteres.")]
        public string Comment { get; set; } = string.Empty;

        [Required]
        [Range(0, 5, ErrorMessage = "La calificación debe estar entre 0 y 5.")]
        public float Rate { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;
    }
}
