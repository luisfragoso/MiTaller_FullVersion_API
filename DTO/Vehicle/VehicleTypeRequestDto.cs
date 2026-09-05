using System.ComponentModel.DataAnnotations;

namespace MiTaller.DTO.Vehicle
{
    /// <summary>Body for POST/PUT api/vehicle/types.</summary>
    public class VehicleTypeRequestDto
    {
        [Required]
        public int VehicleVersionId { get; set; }

        [Required]
        public string Type { get; set; } = string.Empty;
    }
}
