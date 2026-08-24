using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiTaller.Models.Accident
{
    public class Accident
    {
        [Key]
        public int Id { get; set; }
        public Guid CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public Customer.Customer Customer { get; set; }
        public string Plates { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
