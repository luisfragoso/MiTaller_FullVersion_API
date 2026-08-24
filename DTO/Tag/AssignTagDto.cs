using System.ComponentModel.DataAnnotations;

namespace MiTaller.DTO.Tag
{
    public class AssignTagDto
    {
        [Required]
        public Guid WorkshopId { get; set; }

        [Required]
        public Guid CustomerId { get; set; }

        [Required]
        public int TagId { get; set; }
    }
}
