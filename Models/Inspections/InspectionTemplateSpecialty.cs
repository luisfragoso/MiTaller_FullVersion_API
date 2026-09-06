using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiTaller.Models.Inspections
{
    /// <summary>
    /// Maps a workshop specialty (e.g. "Radiadores", "Eléctrico Automotriz") to
    /// the lightweight InspectionTemplate used when a specialty shop runs a
    /// focused checklist on a vehicle it doesn't need to re-register - the
    /// vehicle keeps its real VehicleFormat (Automóvil/Motocicleta), only the
    /// checklist changes. Parallel to InspectionTemplateFormat but kept in its
    /// own table so specialty keys never leak into GetVehicleTypes (which only
    /// unions Brands.Type with InspectionTemplateFormats).
    /// </summary>
    public class InspectionTemplateSpecialty
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TemplateId { get; set; }
        [ForeignKey("TemplateId")]
        public InspectionTemplate? Template { get; set; }

        [Required]
        public string Specialty { get; set; } = string.Empty;
    }
}
