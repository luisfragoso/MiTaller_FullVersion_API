using MiTaller.DTO.Pager;

namespace MiTaller.DTO.Review
{
    public class ReviewPagedResponseDto
    {
        public float AverageRate { get; set; }
        public int TotalReviews { get; set; }
        public int ThisMonthReviews { get; set; }
        public Dictionary<int, int> StarCounts { get; set; }
        public int CurrentPage { get; set; }
        public int MaxPage { get; set; }
        public List<WorkshopReviewDto> WorkshopReviews { get; set; } = new();
    }
}
