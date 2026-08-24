namespace MiTaller.DTO.Workshop.Services
{
    public class WorkshopServiceSimpleResponseDto
    {
        public Guid WorkshopId { get; set; }
        public string WorkshopName { get; set; } = string.Empty;
        public List<ServiceSimpleResponseDto> Services { get; set; } = new List<ServiceSimpleResponseDto>();
        public byte[]? Image { get; set; }
    }
    public class ServiceSimpleResponseDto
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public float Price { get; set; } = 0;

    }

}
