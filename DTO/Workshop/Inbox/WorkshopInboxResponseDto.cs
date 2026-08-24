using MiTaller.DTO.Vehicle;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiTaller.DTO.Workshop.Inbox
{
    public class WorkshopInboxResponseDto
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhoneNumber { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public VehicleResponseDto Vehicle { get; set; }
        public string ParentModelType { get; set; } = string.Empty;
        public int ParentModelId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
