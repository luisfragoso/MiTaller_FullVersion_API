using MiTaller.Models.Vehicle;
using MiTaller.Models.Audit;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiTaller.Models.Inspections
{
    public class VehicleInspectionDetailHistory : INotAudited
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("VehicleInspectionId")]
        public int VehicleInspectionId { get; set; }
        public WorkshopVehicleInspection VehicleInspection { get; set; }
        public string Title { get; set; } = string.Empty;
        public bool IsCompleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? CompletedAt { get; set; } = null;
    }
}
