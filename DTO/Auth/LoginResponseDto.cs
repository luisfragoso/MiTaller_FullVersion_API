using MiTaller.Models.Auth;

namespace MiTaller.DTO.Auth
{
    public class LoginResponseDto
    {
        public Guid Id { get; set; }
        public UserType UserType { get; set; }
        public string ShortId { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public bool EmailConfirmed { get; set; }
        public string? Permissions { get; set; }
        public Guid? EmployeeId { get; set; }
        public string? ShortEmployeeId { get; set; }
    }
}
