using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiTaller.Models.Workshop
{
    public class WorkshopCustomers
    {
        [Key]
        public int Id { get; set; }

        public Guid WorkshopId { get; set; }
        [ForeignKey("WorkshopId")]
        public virtual Workshop Workshop { get; set; } = null!;

        public Guid CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public virtual Customer.Customer Customer { get; set; } = null!;

    }
}
