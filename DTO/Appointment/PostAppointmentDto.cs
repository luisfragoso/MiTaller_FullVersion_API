using MiTaller.Models.Auth;
using MiTaller.Models.Notification;
using System.ComponentModel.DataAnnotations;

namespace MiTaller.DTO.Appointment
{
    public class PostAppointmentDto
    {
        public Guid CustomerId { get; set; }
        public Guid WorkshopId { get; set; }
        public UserType UserType { get; set; }
        public int VehicleId { get; set; }
        public DateTime Date { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        //public string Status { get; set; } = string.Empty;
        public string AppointmentType { get; set; } = string.Empty;
        public NotificationType NotificationType { get; set; }
        public IFormFile? Image { get; set; }
    }
}
