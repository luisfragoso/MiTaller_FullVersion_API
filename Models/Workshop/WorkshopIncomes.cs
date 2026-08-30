using System.ComponentModel.DataAnnotations;
using MiTaller.Models.Audit;

namespace MiTaller.Models.Workshop
{
    public class WorkshopIncomes : INotAudited
    {
        [Key]
        public int Id { get; set; }
        public Guid WorkshopId { get; set; }
        public WorkshopServices? WorkshopServices { get; set; }
        // Null when this income isn't linked to a workshop service (the
        // "Otro" option) - CustomDescription is used for display instead.
        public int? WorkshopServiceId { get; set; }
        public string? CustomDescription { get; set; }
        public float Amount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
