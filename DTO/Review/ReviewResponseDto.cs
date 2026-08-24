namespace MiTaller.DTO.Review
{
    public class ReviewResponseDto
    {
        public float AverageRate { get; set; }
        public int TotalReviews { get; set; }
        public Dictionary<int, int> StarCounts { get; set; }
        public List<WorkshopReviewDto> WorkshopReviews { get; set; }
    }
}
