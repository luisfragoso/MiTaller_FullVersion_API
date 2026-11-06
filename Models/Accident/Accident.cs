using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiTaller.Models.Accident
{
    public class Accident
    {
        [Key]
        public int Id { get; set; }
        // Who reported it (usually a witness, not the vehicle owner)
        public Guid CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public Customer.Customer Customer { get; set; }
        public string Plates { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
        // Emails sent to the owners' emergency contacts; internal, never returned to the reporter
        public int NotifiedContacts { get; set; }
    }
}
