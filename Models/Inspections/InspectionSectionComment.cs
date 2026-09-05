using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiTaller.Models.Inspections
{
    public class InspectionSectionComment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int InspectionId { get; set; }
        [ForeignKey("InspectionId")]
        public Inspection? Inspection { get; set; }

        [Required]
        public int SectionId { get; set; }
        [ForeignKey("SectionId")]
        public InspectionTemplateSection? Section { get; set; }

        public string? Comment { get; set; } = string.Empty;
    }
}
