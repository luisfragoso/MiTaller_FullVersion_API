using MiTaller.Models.Auth;

namespace MiTaller.DTO.Auth
{
    public class LoginDto
    {
        public string LoginIdentifier { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public UserType UserType { get; set; }
        public string? DeviceToken { get; set; }
    }
}
