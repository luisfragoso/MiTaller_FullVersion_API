using System.ComponentModel.DataAnnotations;

namespace MiTaller.DTO.Vehicle
{
    /// <summary>Body for POST/PUT api/vehicle/versions.</summary>
    public class VehicleVersionRequestDto
    {
        [Required]
        public int VehicleModelId { get; set; }

        [Required]
        public string Version { get; set; } = string.Empty;
    }
}
