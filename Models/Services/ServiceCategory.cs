using System.ComponentModel.DataAnnotations;

namespace MiTaller.Models.Services
{
    public class ServiceCategory
    {
        [Key]
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
