namespace MiTaller.DTO.Workshop.File
{
    public class WorkshopFileResponseDto
    {
        public Guid FileId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        // Not sent in the list; the file itself comes from download-file/{fileId}
        public long Size { get; set; }
        public string UploadedAt { get; set; } = string.Empty;
    }
}
