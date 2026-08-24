namespace MiTaller.DTO.Accident
{
    public class PostAccidentDto
    {
        public Guid CustomerId { get; set; }
        public string Plates { get; set; } = string.Empty;
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
    }
}
