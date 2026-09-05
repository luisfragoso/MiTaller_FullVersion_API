using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiTaller.Models.Inspections
{
    public class InspectionResultItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int InspectionId { get; set; }
        [ForeignKey("InspectionId")]
        public Inspection? Inspection { get; set; }

        [Required]
        public int TemplateItemId { get; set; }
        [ForeignKey("TemplateItemId")]
        public InspectionTemplateItem? TemplateItem { get; set; }

        public InspectionItemValue Value { get; set; } = InspectionItemValue.Optimal;
    }
}
