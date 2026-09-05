using System.ComponentModel.DataAnnotations;

namespace MiTaller.DTO.Vehicle
{
    /// <summary>Body for POST/PUT api/vehicle/models.</summary>
    public class VehicleModelRequestDto
    {
        [Required]
        public int BrandId { get; set; }

        [Required]
        public string Model { get; set; } = string.Empty;
    }
}
