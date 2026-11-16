using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiTaller.Models.Inspections
{
    public class InspectionTemplateSection
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TemplateId { get; set; }
        [ForeignKey("TemplateId")]
        public InspectionTemplate? Template { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;
        // English display name; null falls back to Name (custom services, rows added later).
        public string? NameEn { get; set; }

        public int Order { get; set; }

        public List<InspectionTemplateItem> Items { get; set; } = new();
    }
}
