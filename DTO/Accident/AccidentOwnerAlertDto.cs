namespace MiTaller.DTO.Accident
{
    public class AccidentOwnerAlertDto
    {
        public int Id { get; set; }
        public string VehicleName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
        public DateTime? FalseAlarmAt { get; set; }
    }
}
