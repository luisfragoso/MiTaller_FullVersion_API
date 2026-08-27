using Microsoft.IdentityModel.Tokens;
using MiTaller.Models.Auth;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MiTaller.Services
{
    public class JwtService
    {
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(BaseIdentityUser user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("UserType", user.UserType.ToString()) // Agregar el UserType al token
            };

            // El claim de admin de plataforma solo se otorga a la cuenta fija configurada
            // en Admin:UserId. No hay ningún endpoint ni flujo que pueda asignarlo a otra
            // cuenta - se establece una sola vez a mano (ver scripts/seed-admin.sql).
            var adminUserId = _configuration["Admin:UserId"];
            var isPlatformAdmin = !string.IsNullOrEmpty(adminUserId)
                && Guid.TryParse(adminUserId, out var adminGuid)
                && adminGuid == user.Id;

            if (isPlatformAdmin)
            {
                claims.Add(new Claim("IsPlatformAdmin", "true"));
            }

            // Sesión de admin más corta dado el radio de daño de un token filtrado.
            var expires = isPlatformAdmin ? DateTime.Now.AddMinutes(45) : DateTime.Now.AddHours(2);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Issuer"],
                claims: claims,
                expires: expires,
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

}
