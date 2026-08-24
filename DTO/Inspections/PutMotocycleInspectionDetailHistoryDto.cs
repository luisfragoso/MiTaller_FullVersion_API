namespace MiTaller.DTO.Inspections
{
    public class PutMotocycleInspectionDetailHistoryDto
    {
        public string Title { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
