using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MiTaller.Models.Audit;

namespace MiTaller.Models.Workshop
{
    public class WorkshopIncomes : INotAudited
    {
        [Key]
        public int Id { get; set; }
        public Guid WorkshopId { get; set; }
        public WorkshopServices WorkshopServices { get; set; }
        public int WorkshopServiceId { get; set; }
        [ForeignKey("WorkshopServiceId")]
        public float Amount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
