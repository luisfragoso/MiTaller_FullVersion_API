using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiTaller.Models.Workshop
{
    public class WorkshopCustomerNotes
    {
        [Key]
        public int Id { get; set; }
        public Guid WorkshopId { get; set; }
        public Guid CustomerId { get; set; }
        public string Note {  get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; }
    }
}
