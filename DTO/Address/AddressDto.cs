using MiTaller.Models.Address;

namespace MiTaller.DTO.Address
{
    public class AddressDto
    {
        public string State { get; set; } = string.Empty;
        public string Town { get; set; } = string.Empty;
        public List<Suburb> SuburbList { get; set; } = new List<Suburb>();
    }
}
