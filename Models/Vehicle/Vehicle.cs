using MiTaller.Models.Domain;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiTaller.Models.Vehicle
{
    public class Vehicle
    {
        [Key]
        public int Id { get; set; }

        public Guid CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public Customer.Customer Customer { get; set; }

        public string Year { get; set; } = string.Empty;

        public int BrandId { get; set; } = -1;
        [ForeignKey("BrandId")]
        public Brand Brand { get; set; }
        public string? OtherBrand { get; set; }

        public int VehicleModelId { get; set; } = -1;
        [ForeignKey("VehicleModelId")]
        public VehicleModel VehicleModel { get; set; }
        public string? OtherVehicleModel { get; set; }

        public int VehicleVersionId { get; set; } = -1;
        [ForeignKey("VehicleVersionId")]
        public VehicleVersion VehicleVersion { get; set; }
        public string? OtherVehicleVersion { get; set; }

        public int VehicleTypeId { get; set; } = -1;
        [ForeignKey("VehicleTypeId")]
        public VehicleType VehicleType { get; set; }
        public string? OtherVehicleType { get; set; }

        public string SerialNumber { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Plates { get; set; } = string.Empty;
        public string? RimRubber { get; set; } = string.Empty;
        public string Kms { get; set; } = string.Empty;
        public string VehicleFormat { get; set; } = string.Empty;
        public byte[]? Image { get; set; }
        public bool IsDeleted { get; set; } = false;
    }

}
