using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiTaller.Models.Tags
{
    public class CustomerAssociatedTag
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public Guid WorkshopId { get; set; }
        [ForeignKey("WorkshopId")]
        public Workshop.Workshop Workshop { get; set; }

        [Required]
        public Guid CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public Customer.Customer Customer { get; set; }

        [Required]
        public int TagId { get; set; }
        [ForeignKey("TagId")]
        public Tag Tag { get; set; }
    }
}
