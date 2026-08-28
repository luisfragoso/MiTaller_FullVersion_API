using Microsoft.AspNetCore.Identity;

namespace MiTaller.Models.Auth
{
    public abstract class BaseIdentityUser : IdentityUser<Guid>
    {
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public bool IsPurged { get; set; } = false;
        public DateTime? PurgedAt { get; set; }
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
        Employee,
        // Cuenta única de administrador de plataforma - ver Models/Auth/Admin.cs y
        // scripts/seed-admin (nunca se crea por ningún endpoint público).
        Admin
    }
}
