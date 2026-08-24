using MiTaller.Models.Auth;

namespace MiTaller.DTO.Auth
{
    public class VerifyConfirmEmailDto
    {
        public Guid Id { get; set; }
        public UserType UserType { get; set; }
        public string Code { get; set; } = string.Empty;
    }
}
