namespace MiTaller.DTO.Workshop.Income
{
    public class PostWorkshopIncomeDto
    {
        public Guid WorkshopId { get; set; }
        public int WorkshopServiceId { get; set; }
        public float Amount { get; set; }
    }
}
