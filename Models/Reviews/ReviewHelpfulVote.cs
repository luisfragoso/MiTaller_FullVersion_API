using System.ComponentModel.DataAnnotations;

namespace MiTaller.Models.Reviews
{
    /// One "Útil" vote per customer and review.
    public class ReviewHelpfulVote
    {
        [Key]
        public int Id { get; set; }
        public int ReviewId { get; set; }
        public Review Review { get; set; }
        public Guid CustomerId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
