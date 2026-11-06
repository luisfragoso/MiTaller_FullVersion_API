using System.ComponentModel.DataAnnotations;

namespace MiTaller.DTO.Accident
{
    public class PostAccidentDto
    {
        public Guid CustomerId { get; set; }
        public int? VehicleId { get; set; }
        public string Plates { get; set; } = string.Empty;
        public string? Kind { get; set; }
        public bool HasInjured { get; set; }
        [MaxLength(300)]
        public string? Notes { get; set; }
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
    }
}
