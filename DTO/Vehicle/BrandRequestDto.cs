using System.ComponentModel.DataAnnotations;

namespace MiTaller.DTO.Vehicle
{
    /// <summary>
    /// Body for POST/PUT api/vehicle/brands. Type is the vehicle format the brand
    /// belongs to (e.g. "Automovil", "Motocicleta", "Yate", "Lancha", "Bote").
    /// </summary>
    public class BrandRequestDto
    {
        [Required]
        public string Type { get; set; } = string.Empty;

        [Required]
        public string Name { get; set; } = string.Empty;
    }
}
