using System.ComponentModel.DataAnnotations;

namespace MiTaller.DTO.Review
{
    public class PostReviewDto
    {
        [Required]
        public Guid CustomerId { get; set; }

        [Required]
        public Guid WorkshopId { get; set; }

        [Required]
        [StringLength(500, ErrorMessage = "El comentario no puede tener más de 500 caracteres.")]
        public string Comment { get; set; } = string.Empty;

        [Required]
        [Range(0, 5, ErrorMessage = "La calificación debe estar entre 0 y 5.")]
        public float Rate { get; set; }
    }
}
