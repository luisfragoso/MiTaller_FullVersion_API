using MiTaller.Models.Auth;

namespace MiTaller.DTO.Auth
{
    public class ForgotPasswordRequestDto
    {
        public string LoginIdentifier { get; set; } = string.Empty;
        public UserType UserType { get; set; }
    }
}
