using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiTaller.Models.Domain
{
    public class VehicleModel
    {
        [Key]
        public int Id { get; set; } // -1 = Other
        public int? BrandId { get; set; } // Puede ser null si es "Other"
        [ForeignKey("BrandId")]
        public Brand? Brand { get; set; }
        public string Model { get; set; } = string.Empty;
    }
}
