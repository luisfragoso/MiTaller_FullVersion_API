namespace MiTaller.DTO.Workshop.File
{
    public class PostWorkshopFilesDto
    {
        public Guid WorkshopId { get; set; }
        public ICollection<IFormFile> Files { get; set; } = new List<IFormFile>();
        public List<string> FileDescriptions { get; set; } = new List<string>();
    }
}
