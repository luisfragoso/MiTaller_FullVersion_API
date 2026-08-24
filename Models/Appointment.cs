using MiTaller.Models.Notification;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiTaller.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        public Guid CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public Customer.Customer Customer { get; set; }
        public Guid WorkshopId { get; set; }
        [ForeignKey("WorkshopId")]
        public Workshop.Workshop Workshop { get; set; }
        public int VehicleId { get; set; }
        [ForeignKey("VehicleId")]
        public Vehicle.Vehicle Vehicle { get; set; }
        public DateTime Date { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string AppointmentType { get; set; } = string.Empty;
        public NotificationType NotificationType { get; set; }
        public byte[]? Image { get; set; }
    }
}
