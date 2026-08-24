using MiTaller.Models.Auth;

namespace MiTaller.DTO.Auth
{
    public class ConfirmEmailDto
    {
        public Guid Id { get; set; }
        public UserType UserType { get; set; }
    }
}
