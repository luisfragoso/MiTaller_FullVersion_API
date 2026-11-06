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

        // Null when the customer typed plates of a vehicle not in their account
        public int? VehicleId { get; set; }
        public string VehicleName { get; set; } = string.Empty;
        // collision | hit-object | rollover | pedestrian | other
        public string Kind { get; set; } = "other";
        public bool HasInjured { get; set; }
        [MaxLength(300)]
        public string? Notes { get; set; }
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
        public int NotifiedContacts { get; set; }
    }
}
