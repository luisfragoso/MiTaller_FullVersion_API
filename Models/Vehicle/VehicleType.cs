using MiTaller.Models.Domain;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MiTaller.Models.Vehicle
{
    public class VehicleType
    {
        [Key]
        public int Id { get; set; } // -1 = Other
        public int? VehicleVersionId { get; set; }
        [ForeignKey("VehicleVersionId")]
        public VehicleVersion? VehicleVersion { get; set; }
        public string Type { get; set; } = string.Empty;
    }
}
