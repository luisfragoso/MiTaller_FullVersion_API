using System.ComponentModel.DataAnnotations.Schema;

namespace MiTaller.Models.Workshop
{
    public class WorkshopBill
    {
        public int Id { get; set; }
        public Guid WorkshopId { get; set; }
        [ForeignKey("WorkshopId")]
        public Workshop Workshop { get; set; }
        public string Description { get; set; } = string.Empty;
        public float Amount { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
