using System.ComponentModel.DataAnnotations.Schema;

namespace MiTaller.DTO.Workshop.Services
{
    public class PostWorkshopServiceDto
    {
        public Guid WorkshopId { get; set; }
        public int ServiceId { get; set; }
        public float Price { get; set; } = 0;
    }
}
