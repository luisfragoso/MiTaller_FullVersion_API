namespace MiTaller.DTO.Workshop.Services
{
    public class WorkshopServiceResponseDto
    {
        public Guid? WorkshopId { get; set; }
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public float Price { get; set; } = 0;
        public string ServiceType { get; set; } = string.Empty;
        public bool IsCustom { get; set; }
    }
}
