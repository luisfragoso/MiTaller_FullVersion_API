using System.ComponentModel.DataAnnotations;

namespace MiTaller.Models.Inspections
{
    /// <summary>
    /// Checklist template for a vehicle format (e.g. "Yate"): adding a format is data, not code.
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
