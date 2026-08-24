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
    }
}
