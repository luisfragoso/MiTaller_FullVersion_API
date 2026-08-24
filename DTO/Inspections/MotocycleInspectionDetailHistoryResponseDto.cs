namespace MiTaller.DTO.Inspections
{
    public class MotocycleInspectionDetailHistoryResponseDto
    {
        public int MotocycleInspectionDetailHistoryId { get; set; }
        public string Title { get; set; } = string.Empty;
        public bool IsCompleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? CompletedAt { get; set; }
    }
}
