namespace MiTaller.DTO.Review
{
    public class WorkshopReviewDto
    {
        public int Id { get; set; }
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public float Rate { get; set; }
        public string Date { get; set; } = string.Empty;
    }
}
