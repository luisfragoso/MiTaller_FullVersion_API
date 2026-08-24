namespace MiTaller.DTO.RoadsideAssistance
{
    public class PostRoadsideAssistance
    {
        public Guid WorkshopId { get; set; }
        public Guid CustomerId { get; set; }
        public int VehicleId { get; set; }
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
