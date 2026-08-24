using MiTaller.Models.Domain;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiTaller.Models.Vehicle
{
    public class VehicleVersion
    {
        [Key]
        public int Id { get; set; } // -1 = Other
        public int? VehicleModelId { get; set; }
        [ForeignKey("VehicleModelId")]
        public VehicleModel? VehicleModel { get; set; }
        public string Version { get; set; } = string.Empty;
    }
}
