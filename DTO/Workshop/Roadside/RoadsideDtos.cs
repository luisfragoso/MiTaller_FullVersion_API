namespace MiTaller.DTO.Workshop.Roadside
{
    public class RoadsideSettingsDto
    {
        public bool OffersRoadsideAssistance { get; set; }
        public int CoverageRadiusKm { get; set; } = 20;
        public bool Available24h { get; set; }
        public string EmergencyPhone { get; set; } = string.Empty;
    }

    public class RoadsideWorkshopDto
    {
        public Guid WorkshopId { get; set; }
        public string WorkshopName { get; set; } = string.Empty;
        public bool HasProfileImage { get; set; }
        public double DistanceKm { get; set; }
        public int CoverageRadiusKm { get; set; }
        public bool Available24h { get; set; }
        // Emergency phone if set, otherwise the workshop's own number
        public string Phone { get; set; } = string.Empty;
        public string? Address { get; set; }
        public float ReviewAverageRate { get; set; }
    }
}
