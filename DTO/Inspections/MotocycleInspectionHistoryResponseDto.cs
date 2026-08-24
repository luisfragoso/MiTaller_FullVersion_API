namespace MiTaller.DTO.Inspections
{
    public class MotocycleInspectionHistoryResponseDto
    {
        public int MotocycleInspectionHistoryId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Folio { get; set; } = string.Empty;
        public byte[]? File { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
