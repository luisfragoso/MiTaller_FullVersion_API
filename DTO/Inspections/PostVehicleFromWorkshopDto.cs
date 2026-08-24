using System.ComponentModel.DataAnnotations;

namespace MiTaller.DTO.Inspections
{
    public class PostVehicleFromWorkshopDto
    {
        [Required]
        public string Year { get; set; } = string.Empty;

        [Required]
        public int BrandId { get; set; }
        public string? OtherBrand { get; set; }

        [Required]
        public int VehicleModelId { get; set; }
        public string? OtherVehicleModel { get; set; }

        [Required]
        public int VehicleVersionId { get; set; }
        public string? OtherVehicleVersion { get; set; }

        [Required]
        public int VehicleTypeId { get; set; }
        public string? OtherVehicleType { get; set; }

        public string SerialNumber { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Plates { get; set; } = string.Empty;
        public string RimRubber { get; set; } = string.Empty;
        public string Kms { get; set; } = string.Empty;
        public string VehicleFormat { get; set; } = string.Empty;
        public IFormFile? Image { get; set; }
    }
}
