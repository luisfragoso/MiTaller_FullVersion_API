using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiTaller.Models.Inspections
{
    /// <summary>
    /// Maps a VehicleFormat (e.g. "Yate", "Lancha", "Bote") to the InspectionTemplate
    /// that should be used for it. Makes InspectionTemplate 1:N with VehicleFormat so
    /// several formats (e.g. embarcaciones) can share the same template/sections/items
    /// without duplicating content. InspectionTemplate.VehicleFormat is kept as
    /// historical metadata only — lookups now go through this table instead.
    /// </summary>
    public class InspectionTemplateFormat
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TemplateId { get; set; }
        [ForeignKey("TemplateId")]
        public InspectionTemplate? Template { get; set; }

        [Required]
        public string VehicleFormat { get; set; } = string.Empty;
    }
}
