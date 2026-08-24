using System.ComponentModel.DataAnnotations.Schema;

namespace MiTaller.Models.Workshop
{
    public class WorkshopNote
    {
        public int Id { get; set; }
        public Guid WorkshopId { get; set; }
        [ForeignKey("WorkshopId")]
        public Workshop Workshop { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
