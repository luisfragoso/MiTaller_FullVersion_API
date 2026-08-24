namespace MiTaller.DTO.Workshop.Note
{
    public class PostWorkshopNoteDto
    {
        public Guid WorkshopId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
