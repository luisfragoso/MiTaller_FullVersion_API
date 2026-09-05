using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiTaller.Models.Workshop
{
    /// <summary>
    /// Per-workshop toggle for which vehicle formats ("Automóvil", "Motocicleta",
    /// "Yate", ...) a workshop attends. Absence of any row for a workshop means
    /// it hasn't configured this yet — callers should synthesize the legacy
    /// default (Automóvil/Motocicleta enabled) in that case, see WorkshopController.
    /// </summary>
    public class WorkshopVehicleFormat
    {
        [Key]
        public int Id { get; set; }
        public Guid WorkshopId { get; set; }
        [ForeignKey("WorkshopId")]
        public Workshop Workshop { get; set; }
        public string VehicleFormat { get; set; } = string.Empty; // "Automóvil", "Motocicleta", "Yate", ...
        public bool IsEnabled { get; set; } = true;
    }
}
