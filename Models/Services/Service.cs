using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiTaller.Models.Services
{
    public class Service
    {
        [Key]
        public int Id { get; set; }
        public int ServiceCategoryId { get; set; }
        [ForeignKey("ServiceCategoryId")]
        public ServiceCategory ServiceCategory { get; set; }
        public string Name { get; set; } = string.Empty;
        // English display name; null falls back to Name (custom services, rows added later).
        public string? NameEn { get; set; }
        // Null = global catalog; set = custom service visible only to that workshop
        public Guid? WorkshopId { get; set; }
    }
}
