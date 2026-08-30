using MiTaller.DTO.Vehicle;
using MiTaller.Models.Notification;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiTaller.DTO.Appointment
{
    public class AppointmentResponseDto
    {

        public int Id { get; set; }
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public Guid WorkshopId { get; set; }
        public string WorkshopName {  get; set; } = string.Empty;
        public VehicleResponseDto Vehicle {  get; set; }
        public DateTime Date { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string AppointmentType { get; set; } = string.Empty;
        public NotificationType NotificationType { get; set; }
        public byte[]? Image { get; set; }
    }
}
