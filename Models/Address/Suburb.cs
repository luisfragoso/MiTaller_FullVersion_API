using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiTaller.Models.Address
{
    public class Suburb
    {
        [Key]
        public int Id { get; set; }
        public int TownId { get; set; }
        [ForeignKey ("TownId")]
        public Town Town {  get; set; }
        public string Zipcode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
