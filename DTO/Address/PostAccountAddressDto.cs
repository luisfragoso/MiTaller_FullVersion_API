using MiTaller.Models.Address;
using MiTaller.Models.Auth;

namespace MiTaller.DTO.Address
{
    public class PostAccountAddressDto
    {
        public Guid Id { get; set; }
        public UserType UserType { get; set; }
        public int SuburbId { get; set; }
        public string Street { get; set; } = string.Empty;

    }
}
