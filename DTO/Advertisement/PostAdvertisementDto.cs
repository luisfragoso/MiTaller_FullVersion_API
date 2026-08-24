namespace MiTaller.DTO.Advertisement
{
    public class PostAdvertisementDto
    {
        public Guid WorkshopId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public IFormFile? Image { get; set; }
        public string Url { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public DateTime? StartsAt { get; set; }
        public DateTime? EndsAt { get; set; }
    }
}
