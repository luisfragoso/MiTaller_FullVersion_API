using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiTaller.Models
{
    public class RoadsideAssistance
    {
        [Key]
        public int Id { get; set; }
        public Guid WorkshopId { get; set; }
        [ForeignKey("CustomerId")]
        public Guid CustomerId { get; set; }
        public Customer.Customer Customer { get; set; }
        [ForeignKey("VehicleId")]
        public int VehicleId { get; set; }
        public Vehicle.Vehicle Vehicle { get; set; }
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
