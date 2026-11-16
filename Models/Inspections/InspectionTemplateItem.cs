using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiTaller.Models.Inspections
{
    public class InspectionTemplateItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SectionId { get; set; }
        [ForeignKey("SectionId")]
        public InspectionTemplateSection? Section { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;
        // English display name; null falls back to Name (custom services, rows added later).
        public string? NameEn { get; set; }

        public int Order { get; set; }

        public bool AllowNotApplicable { get; set; } = false;
    }
}
