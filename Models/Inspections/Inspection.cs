using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MiTaller.Models.Workshop;
using MiTaller.Models.Customer;
using MiTaller.Models.Vehicle;

namespace MiTaller.Models.Inspections
{
    /// <summary>
    /// A generic (template-driven) vehicle inspection, used by any vehicle
    /// format that isn't "Automóvil"/"Motocicleta" (those keep using
    /// WorkshopVehicleInspection/WorkshopMotocycleInspection unchanged).
    /// </summary>
    public class Inspection
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TemplateId { get; set; }
        [ForeignKey("TemplateId")]
        public InspectionTemplate? Template { get; set; }

        [Required]
        public Guid WorkshopId { get; set; }
        [ForeignKey("WorkshopId")]
        public Workshop.Workshop Workshop { get; set; }

        [Required]
        public Guid CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public Customer.Customer Customer { get; set; }

        [Required]
        public int VehicleId { get; set; }
        [ForeignKey("VehicleId")]
        public Vehicle.Vehicle Vehicle { get; set; }

        public DateTime InspectionDate { get; set; } = DateTime.Now;

        public string Status { get; set; } = "Pendiente";

        public bool IsActive { get; set; } = true;

        public string? GeneralObservations { get; set; } = string.Empty;

        public List<InspectionResultItem> Results { get; set; } = new();
        public List<InspectionSectionComment> SectionComments { get; set; } = new();
        public List<InspectionFile> Files { get; set; } = new();
    }
}
