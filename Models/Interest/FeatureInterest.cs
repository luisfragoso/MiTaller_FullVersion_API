using System.ComponentModel.DataAnnotations;
using MiTaller.Models.Audit;

namespace MiTaller.Models.Interest
{
    /// A customer asked to be told when a not-yet-available feature (e.g. "credit") launches.
    public class FeatureInterest : INotAudited
    {
        [Key]
        public int Id { get; set; }

        public Guid CustomerId { get; set; }

        [MaxLength(40)]
        public string Feature { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Set once the customer has been told the feature is live
        public DateTime? NotifiedAt { get; set; }
    }
}
