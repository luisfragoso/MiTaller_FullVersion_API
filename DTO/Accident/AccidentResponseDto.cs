namespace MiTaller.DTO.Accident
{
    /// <summary>Never carries who reported it nor who owns the vehicle.</summary>
    public class AccidentResponseDto
    {
        public int Id { get; set; }
        public string Plates { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
        // Register only: last report of the same plates (by anyone) in the previous 24 h
        public DateTime? PreviousReportAt { get; set; }
    }
}
