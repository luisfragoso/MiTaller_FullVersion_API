using MiTaller.Models.Auth;

namespace MiTaller.DTO.Auth
{
    public class VerifyResetCodeDto
    {
        public string LoginIdentifier { get; set; } = string.Empty;
        public UserType UserType { get; set; }
        public string Code { get; set; } = string.Empty;
    }
}
