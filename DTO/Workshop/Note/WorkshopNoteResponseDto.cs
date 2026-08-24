using System.ComponentModel.DataAnnotations.Schema;

namespace MiTaller.DTO.Workshop.Note
{
    public class WorkshopNoteResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; } = DateTime.Now;
    }
}
