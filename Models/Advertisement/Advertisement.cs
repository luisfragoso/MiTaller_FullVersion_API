using System.ComponentModel.DataAnnotations;

namespace MiTaller.Models.Advertisement
{
    public class Advertisement
    {
        [Key]
        public int Id { get; set; }
        public Guid WorkshopId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public byte[]? Image { get; set; }
        public string Url { get; set; } = string.Empty;
        public string Type {  get; set; } = string.Empty;
        public DateTime? StartsAt { get; set; }
        public DateTime? EndsAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
