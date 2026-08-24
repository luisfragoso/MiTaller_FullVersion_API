using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.Design;

namespace MiTaller.Models.Workshop
{
    public class WorkshopServices
    {
        [Key]
        public int Id { get; set; }
        public Guid WorkshopId { get; set; }
        [ForeignKey("WorkshopId")]
        public Workshop Workshop { get; set; }
        public int ServiceId { get; set; }
        [ForeignKey("ServiceId")]
        public Services.Service Service { get; set; }
        public float Price { get; set; } = 0;
        public bool IsDeleted { get; set; } = false;
    }
}
