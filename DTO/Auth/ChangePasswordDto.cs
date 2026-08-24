using MiTaller.Models.Auth;

namespace MiTaller.DTO.Auth
{
    public class ChangePasswordDto
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public UserType UserType { get; set; }
    }
}
