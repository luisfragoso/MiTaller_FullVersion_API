namespace MiTaller.DTO.Workshop.Income
{
    public class PostWorkshopIncomeDto
    {
        public Guid WorkshopId { get; set; }
        // <= 0 (or the "Otro" sentinel from the app) means "no linked
        // service" - CustomDescription is required in that case instead.
        public int WorkshopServiceId { get; set; }
        public string? CustomDescription { get; set; }
        public float Amount { get; set; }
    }
}
