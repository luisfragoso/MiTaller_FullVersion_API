using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiTaller.Models.Address
{
    public class Town
    {
        [Key]
        public int Id { get; set; }
        public int StateId { get; set; }
        [ForeignKey ("StateId")]
        public State State { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
