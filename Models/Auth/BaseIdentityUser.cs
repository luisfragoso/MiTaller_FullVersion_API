using Microsoft.AspNetCore.Identity;

namespace MiTaller.Models.Auth
{
    public abstract class BaseIdentityUser : IdentityUser<Guid>
    {
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public UserType UserType { get; set; }
        public string NormalizedPhoneNumber { get; set; } = string.Empty;
        public string? PasswordResetCode { get; set; }
        public DateTime? PasswordResetCodeExpires { get; set; }
        public string? EmailVerificationCode { get; set; }
        public DateTime? EmailVerificationExpires { get; set; }
        public string DeviceTokens { get; set; } = string.Empty;
    }

    public enum UserType
    {
        Customer,
        Workshop,
        Employee
    }
}
