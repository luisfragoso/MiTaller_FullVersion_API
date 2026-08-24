using MiTaller.Models.Customer;
using MiTaller.Models.Workshop;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiTaller.Models.Address
{
    public class CustomerAddress
    {
        [Key]
        public int Id { get; set; }
        public Guid CustomerId { get; set; }
        [ForeignKey("CustomerId")]

        public Customer.Customer Customer { get; set; }
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
