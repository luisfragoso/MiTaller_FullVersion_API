using System.ComponentModel.DataAnnotations.Schema;

namespace MiTaller.Models.Address
{
    public class WorkshopAddress
    {
        public int Id { get; set; }
        public Guid WorkshopId { get; set; }
        [ForeignKey("WorkshopId")]
        public Workshop.Workshop Workshop { get; set; }
        public int SuburbId { get; set; }
        [ForeignKey("SuburbId")]
        public Suburb Suburb { get; set; }
        public string Street { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
