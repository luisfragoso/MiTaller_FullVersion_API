namespace MiTaller.DTO.Workshop.Statistics
{
    public class QuotationStatusSummaryDto
    {
        public int Total { get; set; }
        public List<QuotationCountResponseDto> Statuses { get; set; } = new();
    }

    public class QuotationCountResponseDto
    {
        public string Status { get; set; } = string.Empty;
        public float Percentage { get; set; }
        public int Count { get; set; }
    }
}
