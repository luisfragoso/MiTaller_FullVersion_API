using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MiTaller.Models.Customer
{
    public class CustomerWorkshops
    {
        [Key]
        public int Id { get; set; }

        public Guid CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; } = null!;

        public Guid WorkshopId { get; set; }
        [ForeignKey("WorkshopId")]
        public virtual Workshop.Workshop Workshop { get; set; } = null!;
    }
}
