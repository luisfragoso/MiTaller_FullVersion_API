namespace MiTaller.DTO.Inspections
{
    public class PostInspectionDetailDto
    {
        public int InspectionId { get; set; }
        public string Title { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
