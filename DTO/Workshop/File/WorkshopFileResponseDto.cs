namespace MiTaller.DTO.Workshop.File
{
    public class WorkshopFileResponseDto
    {
        public Guid FileId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public byte[] FileData { get; set; }
        public string UploadedAt { get; set; } = string.Empty;
    }
}
