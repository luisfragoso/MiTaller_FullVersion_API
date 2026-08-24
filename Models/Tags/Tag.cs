using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiTaller.Models.Tags
{
    public class Tag
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public Guid WorkshopId { get; set; }

        [ForeignKey("WorkshopId")]
        public Workshop.Workshop Workshop { get; set; }

        [Required]
        public string Value { get; set; } = string.Empty; // Nombre del tag

        public string Description { get; set; } = string.Empty;

        public string HexColor { get; set; } = string.Empty; // Color en formato HEX

        // Relación inversa: un Tag puede estar asociado a varios clientes
        public ICollection<CustomerAssociatedTag> CustomerAssociatedTags { get; set; } = new List<CustomerAssociatedTag>();
    }
}
