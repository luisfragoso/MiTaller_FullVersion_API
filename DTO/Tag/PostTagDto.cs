using System.ComponentModel.DataAnnotations;

namespace MiTaller.DTO.Tag
{
    public class PostTagDto
    {

        [Required]
        public string Value { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string HexColor { get; set; } = string.Empty;
    }
}
