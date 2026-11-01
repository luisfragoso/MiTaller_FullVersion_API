namespace MiTaller.DTO.Workshop.Services
{
    public class PostCustomWorkshopServiceDto
    {
        public Guid WorkshopId { get; set; }
        public int ServiceCategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public float Price { get; set; } = 0;
    }
}
