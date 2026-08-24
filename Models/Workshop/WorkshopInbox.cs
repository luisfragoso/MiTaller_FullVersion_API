using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiTaller.Models.Workshop
{
    public class WorkshopInbox
    {
        [Key]
        public int Id { get; set; }
        public Guid WorkshopId { get; set; }
        [ForeignKey("CustomerId")]
        public Guid CustomerId { get; set; }
        public Customer.Customer Customer { get; set; }
        [ForeignKey("VehicleId")]
        public int VehicleId { get; set; }
        public Vehicle.Vehicle Vehicle { get; set; }
        public string ParentModelType {  get; set; } = string.Empty;
        public int ParentModelId { get; set; }
        public string Title {  get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;  
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

    }
}
