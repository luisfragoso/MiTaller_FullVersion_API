using System.ComponentModel.DataAnnotations;

namespace MiTaller.Models.Inspections
{
    /// <summary>
    /// A configurable inspection checklist template for a given vehicle format
    /// (e.g. "Yate"). Replaces the hardcoded-wide-table pattern used by
    /// WorkshopVehicleInspection/WorkshopMotocycleInspection for any new format,
    /// so adding a format is data, not code.
    /// </summary>
    public class InspectionTemplate
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string VehicleFormat { get; set; } = string.Empty;

        [Required]
        public string Name { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public List<InspectionTemplateSection> Sections { get; set; } = new();
    }
}
