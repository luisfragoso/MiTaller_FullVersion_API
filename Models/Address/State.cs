using System.ComponentModel.DataAnnotations;

namespace MiTaller.Models.Address
{
    public class State
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
