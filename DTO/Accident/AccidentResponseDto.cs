namespace MiTaller.DTO.Accident
{
    public class AccidentResponseDto
    {
        public int Id { get; set; }
        public Guid CustomerId { get; set; }
        public string Plates { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int? VehicleId { get; set; }
        public string VehicleName { get; set; } = string.Empty;
        public string Kind { get; set; } = "other";
        public bool HasInjured { get; set; }
        public string? Notes { get; set; }
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
        public int NotifiedContacts { get; set; }
    }
}
