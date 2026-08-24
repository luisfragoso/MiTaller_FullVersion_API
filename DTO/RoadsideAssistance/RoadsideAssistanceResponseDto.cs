using MiTaller.DTO.Vehicle;

namespace MiTaller.DTO.RoadsideAssistance
{
    public class RoadsideAssistanceResponseDto
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public VehicleResponseDto Vehicle { get; set; }
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
