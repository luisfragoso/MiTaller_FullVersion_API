using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using MiTaller.Models.Domain;
using MiTaller.Attributes;
using MiTaller.Data;
using MiTaller.DTO.Auth;
using MiTaller.DTO.Customer;
using MiTaller.DTO.Workshop;
using MiTaller.Models.Auth;
using MiTaller.Models.Customer;
using MiTaller.Models.Notification;
using MiTaller.Models.Workshop;
using MiTaller.Services;
using System.Security.Claims;

using MiTaller.Services.Localization;

namespace MiTaller.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableRateLimiting(RateLimitPolicies.Auth)]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<BaseIdentityUser> _userManager;
        private readonly JwtService _jwtService;
        private readonly IEmailSender _emailSender;
        private readonly DataContext _context;

        private const int MaxCodeAttempts = 5;
        private static readonly TimeSpan CodeLockout = TimeSpan.FromMinutes(30);

        public AuthController(UserManager<BaseIdentityUser> userManager, JwtService jwtService, IEmailSender emailSender, DataContext dataContext)
        {
            _userManager = userManager;
            _jwtService = jwtService;
            _emailSender = emailSender;
            _context = dataContext;
        }

        private static bool CodeLocked(BaseIdentityUser user) => user.VerificationCodeLockedUntil > DateTime.UtcNow;

        // Unknown accounts answer like a wrong password, after a real hash check so the timing matches too
        private static readonly string DummyPasswordHash =
            new PasswordHasher<BaseIdentityUser>().HashPassword(null!, Guid.NewGuid().ToString());

        // The last allowed wrong guess burns the pending code and refuses new ones for CodeLockout.
        private async Task<string> WrongCodeAsync(BaseIdentityUser user)
        {
            var burned = ++user.VerificationCodeFailedAttempts >= MaxCodeAttempts;
            if (burned)
            {
                user.VerificationCodeFailedAttempts = 0;
                user.VerificationCodeLockedUntil = DateTime.UtcNow.Add(CodeLockout);
                user.PasswordResetCode = null;
                user.PasswordResetCodeExpires = null;
                user.EmailVerificationCode = null;
                user.EmailVerificationExpires = null;
            }
            await _userManager.UpdateAsync(user);
            return burned ? "expired-code" : "incorrect-code";
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginDto model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.LoginIdentifier))
                {
                    return BadRequest("invalid-empty");
                }

                // Determinar si es un email o un número de teléfono
                bool isEmail = model.LoginIdentifier.Contains("@");

                // Buscar el usuario por email o teléfono según el formato
                var user = await _userManager.Users
                    .Where(u => (isEmail ? u.Email == model.LoginIdentifier : u.NormalizedPhoneNumber == model.LoginIdentifier) && u.UserType == model.UserType)
                    .SingleOrDefaultAsync();

                if (user == null)
                {
                    _userManager.PasswordHasher.VerifyHashedPassword(null!, DummyPasswordHash, model.Password ?? string.Empty);
                    return Unauthorized("wrong-password");
                }

                if (await _userManager.IsLockedOutAsync(user))
                {
                    return Unauthorized("locked-out");
                }

                if (!await _userManager.CheckPasswordAsync(user, model.Password))
                {
                    await _userManager.AccessFailedAsync(user);
                    return Unauthorized(await _userManager.IsLockedOutAsync(user) ? "locked-out" : "wrong-password");
                }

                // Only revealed to whoever knows the password
                if (user.IsDeleted)
                {
                    return Unauthorized("user-deleted");
                }

                await _userManager.ResetAccessFailedCountAsync(user);

                // Registrar el token del dispositivo si se proporciona
                if (!string.IsNullOrEmpty(model.DeviceToken))
                {
                    var tokens = string.IsNullOrEmpty(user.DeviceTokens)
                        ? new List<string>()
                        : user.DeviceTokens.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();

                    // Agregar el token solo si no existe ya
                    if (!tokens.Contains(model.DeviceToken))
                    {
                        tokens.Add(model.DeviceToken);
                        user.DeviceTokens = string.Join(",", tokens);
                    }
                }

                user.LastLoginAt = DateTime.Now;
                // The apps send their active language; emails and push to this account follow it
                if (Request.Headers.ContainsKey("Accept-Language")) user.PreferredLanguage = Lang.FromRequest(Request);
                await _userManager.UpdateAsync(user);

                var token = _jwtService.GenerateToken(user);
                string shortId = user.Id.ToString().Substring(0,13);

                var loginResponseDto = new LoginResponseDto
                {
                    Id = user.Id,
                    UserType = user.UserType,
                    ShortId = shortId,
                    Token = token,
                    EmailConfirmed = user.EmailConfirmed,
                };

                // Si es un empleado, agregar información adicional
                if (user.UserType == UserType.Employee)
                {
                    var workshopEmployee = await _context.WorkshopEmployees
                        .Where(we => we.EmployeeId == user.Id && !we.IsDeleted)
                        .FirstOrDefaultAsync();

                    if (workshopEmployee != null)
                    {
                        loginResponseDto.Id = workshopEmployee.WorkshopId;
                        loginResponseDto.ShortId = workshopEmployee.WorkshopId.ToString().Substring(0, 13);
                        loginResponseDto.EmployeeId = workshopEmployee.EmployeeId;
                        loginResponseDto.ShortEmployeeId = workshopEmployee.EmployeeId?.ToString().Substring(0, 13);
                        loginResponseDto.Permissions = workshopEmployee.Permissions;
                        loginResponseDto.EmployeeName = workshopEmployee.FullName;
                    }
                }

                return Ok(loginResponseDto);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPost("create-customer")]
        public async Task<ActionResult> Register([FromBody] PostCustomerDto model)
        {
            try
            {
                var existingUser = await _userManager.Users
                    .AnyAsync(u => (u.NormalizedPhoneNumber == model.PhoneNumber || u.Email == model.Email) && u.UserType == UserType.Customer);

                if (existingUser)
                {
                    return BadRequest("user-already-registered");
                }

                var customer = new Customer
                {
                    PreferredLanguage = Lang.FromRequest(Request),
                    Id = Guid.NewGuid(),
                    UserName = model.Email + "_customer",
                    Email = model.Email,
                    FullName = model.FullName,
                    PhoneNumber = model.PhoneNumber + "_customer",
                    NormalizedPhoneNumber = model.PhoneNumber,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = null,
                    DeletedAt = null,
                    IsDeleted = false,
                    UserType = UserType.Customer,
                    CreatedBy = RecordOrigin.Self,
                };

                customer.CreatedByAccountId = customer.Id;
                var result = await _userManager.CreateAsync(customer, model.Password);

                if (!result.Succeeded)
                {
                    return BadRequest("invalid-password");
                }

                // Creamos el registro de notificationSettings
                var notificationSettings = new NotificationSettings
                {
                    UserId = customer.Id,
                    UserType = UserType.Customer,
                };

                await _context.NotificationSettings.AddAsync(notificationSettings);
                await _context.SaveChangesAsync();


                // Notificación de bienvenida
                var notification = new Notifications
                {
                    UserId = customer.Id,
                    UserType = UserType.Customer,
                    Title = T.Get(Lang.FromRequest(Request), "account.created.title"),
                    Content = T.Get(Lang.FromRequest(Request), "account.created.content"),
                    Event = "AccountCreated"
                };

                await _context.Notifications.AddAsync(notification);
                await _context.SaveChangesAsync();

                return Ok("user-created");

            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
            
        }

        [HttpPost("create-workshop")]
        public async Task<ActionResult> Register([FromBody] PostWorkshopDto model)
        {
            try
            {
                var normalizedPhone = model.PhoneNumber?.Trim().Replace(" ", "").Replace("-", "");

                var existingUser = await _userManager.Users
                    .Where(u => u.UserType == UserType.Workshop)
                    .AnyAsync(u =>
                        (u.NormalizedPhoneNumber.Replace(" ", "").Replace("-", "") == normalizedPhone) ||
                        (u.Email.ToLower() == model.Email.ToLower()));

                if (existingUser)
                {
                    return BadRequest("user-already-registered");
                }

                var workshop = new Workshop
                {
                    PreferredLanguage = Lang.FromRequest(Request),
                    Id = Guid.NewGuid(),
                    UserName = model.Email + "_workshop",
                    Email = model.Email,
                    WorkshopName = model.WorkshopName,
                    PhoneNumber = model.PhoneNumber + "_workshop",
                    NormalizedPhoneNumber = model.PhoneNumber,
                    AssociateFullName = model.AssociateFullName,
                    Type = model.Type,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = null,
                    DeletedAt = null,
                    IsDeleted = false,
                    UserType = UserType.Workshop
                };

                var result = await _userManager.CreateAsync(workshop, model.Password);

                if (!result.Succeeded)
                {
                    return BadRequest("invalid-password");
                }

                // Creamos el registro de notificationSettings
                var notificationSettings = new NotificationSettings
                {
                    UserId = workshop.Id,
                    UserType = UserType.Workshop,
                };

                await _context.NotificationSettings.AddAsync(notificationSettings);
                await _context.SaveChangesAsync();


                // Notificación de bienvenida
                var notification = new Notifications
                {
                    UserId = workshop.Id,
                    UserType = UserType.Workshop,
                    Title = T.Get(Lang.FromRequest(Request), "account.created.title"),
                    Content = T.Get(Lang.FromRequest(Request), "account.created.content"),
                    Event = "AccountCreated"
                };

                await _context.Notifications.AddAsync(notification);
                await _context.SaveChangesAsync();

                return Ok("workshop-created");
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPost("send-email-verification")]
        [EnableRateLimiting(RateLimitPolicies.AuthEmail)]
        public async Task<ActionResult> SendEmailVerification([FromBody] ConfirmEmailDto model)
        {
            var user = await _userManager.Users
                .Where(u => u.Id == model.Id && u.UserType == model.UserType)
                .SingleOrDefaultAsync();

            if (user == null)
            {
                return NotFound("not-found");
            }

            if (user.EmailConfirmed)
            {
                return BadRequest("user-already-verified");
            }

            if (CodeLocked(user))
            {
                return BadRequest("too-many-attempts");
            }

            var verificationCode = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
            //var verificationCode = "123456";
            user.EmailVerificationCode = verificationCode;
            user.EmailVerificationExpires = DateTime.UtcNow.AddMinutes(15);

            await _userManager.UpdateAsync(user);

            await _emailSender.SendEmailAsync(user.Email, T.Get(Lang.FromRequest(Request), "verify.subject"),
                EmailLayout.Code(Lang.FromRequest(Request), T.Get(Lang.FromRequest(Request), "verify.intro"), verificationCode));
            
            return Ok("email-sent");
        }

        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromBody] VerifyConfirmEmailDto model)
        {
            var user = await _userManager.Users
                .Where(u => u.Id == model.Id && !u.IsDeleted)
                .SingleOrDefaultAsync();

            if (user == null)
            {
                return NotFound("not-found");
            }

            if (user.EmailConfirmed)
            {
                return BadRequest("user-already-verified");
            }

            if (user.EmailVerificationCode == null || user.EmailVerificationExpires == null)
            {
                return BadRequest("no-code");
            }

            if (DateTime.UtcNow > user.EmailVerificationExpires)
            {
                return BadRequest("expired-code");
            }

            if (user.EmailVerificationCode != model.Code)
            {
                return BadRequest(await WrongCodeAsync(user));
            }

            user.EmailConfirmed = true;
            user.VerificationCodeFailedAttempts = 0;
            user.EmailVerificationCode = null;
            user.EmailVerificationExpires = null;

            await _userManager.UpdateAsync(user);

            return Ok("user-verified");
        }

        [HttpPost("resend-email-verification")]
        [EnableRateLimiting(RateLimitPolicies.AuthEmail)]
        public async Task<IActionResult> ResendEmailVerification([FromBody] ConfirmEmailDto model)
        {
            var user = await _userManager.Users
                .Where(u => u.Id == model.Id && u.UserType == model.UserType)
                .SingleOrDefaultAsync();

            if (user == null)
            {
                return NotFound("not-found");
            }

            if (user.EmailConfirmed)
            {
                return BadRequest("user-already-verified");
            }

            if (CodeLocked(user))
            {
                return BadRequest("too-many-attempts");
            }

            if (user.EmailVerificationCode != null && user.EmailVerificationExpires > DateTime.UtcNow)
            {
                await _emailSender.SendEmailAsync(user.Email, T.Get(Lang.FromRequest(Request), "verify.subject"),
                    EmailLayout.Code(Lang.FromRequest(Request), T.Get(Lang.FromRequest(Request), "verify.intro"), user.EmailVerificationCode));

                return Ok("email-sent");
            }

            var verificationCode = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
            //var verificationCode = "123456";
            user.EmailVerificationCode = verificationCode;
            user.EmailVerificationExpires = DateTime.UtcNow.AddMinutes(15);

            await _userManager.UpdateAsync(user);

            await _emailSender.SendEmailAsync(user.Email, T.Get(Lang.FromRequest(Request), "verify.subject"),
                EmailLayout.Code(Lang.FromRequest(Request), T.Get(Lang.FromRequest(Request), "verify.intro"), verificationCode));


            return Ok("email-sent");
        }


        [HttpPost("change-password")]
        [Authorize]
        public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordDto model)
        {
            try { 

                // 🔹 Obtener el ID del usuario autenticado desde el Token JWT
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return Unauthorized("unauthorized-user");
                }

                // 🔹 Buscar al usuario autenticado en la base de datos
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound("not-found");
                }

                // 🔹 Intentar cambiar la contraseña (validando la actual)
                var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

                if (result.Succeeded)
                {
                    // The new SecurityStamp revokes every other session; this one keeps working with a fresh token.
                    return Ok(new { message = "password-updated", token = _jwtService.GenerateToken(user) });
                }

                return BadRequest("unkwnown-error");
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPost("forgot-password")]
        [EnableRateLimiting(RateLimitPolicies.AuthEmail)]
        public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.LoginIdentifier))
                {
                    return BadRequest("invalid-empty");
                }

                // Determinar si es un email o un número de teléfono
                bool isEmail = model.LoginIdentifier.Contains("@");

                var user = await _userManager.Users
                    .Where(u => (isEmail ? u.Email == model.LoginIdentifier : u.NormalizedPhoneNumber == model.LoginIdentifier) && u.UserType == model.UserType)
                    .SingleOrDefaultAsync();

                if (user == null)
                {
                    // Same answer as for a real account, so the form can't tell which emails exist
                    return Ok("email-sent");
                }

                if (CodeLocked(user))
                {
                    return BadRequest("too-many-attempts");
                }

                var resetCode = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
                //var resetCode = "123456";
                user.PasswordResetCode = resetCode;
                user.PasswordResetCodeExpires = DateTime.UtcNow.AddMinutes(15);

                await _userManager.UpdateAsync(user);

                await _emailSender.SendEmailAsync(user.Email, T.Get(Lang.FromRequest(Request), "reset.subject"),
                    EmailLayout.Code(Lang.FromRequest(Request), T.Get(Lang.FromRequest(Request), "reset.intro"), resetCode));

                return Ok("email-sent");
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPost("verify-reset-code")]
        public async Task<ActionResult> VerifyResetCode([FromBody] VerifyResetCodeDto model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.LoginIdentifier))
                {
                    return BadRequest("invalid-empty");
                }

                // Determinar si es un email o un número de teléfono
                bool isEmail = model.LoginIdentifier.Contains("@");

                var user = await _userManager.Users
                    .Where(u => (isEmail ? u.Email == model.LoginIdentifier : u.NormalizedPhoneNumber == model.LoginIdentifier) && u.UserType == model.UserType)
                    .SingleOrDefaultAsync();

                if (user == null)
                {
                    // No account looks like a wrong code, not "not-found"
                    return BadRequest("incorrect-code");
                }

                if (user.PasswordResetCode == null || user.PasswordResetCodeExpires == null)
                {
                    return BadRequest("no-code");
                }

                if (DateTime.UtcNow > user.PasswordResetCodeExpires)
                {
                    return BadRequest("expired-code");
                }

                if (user.PasswordResetCode != model.Code)
                {
                    return BadRequest(await WrongCodeAsync(user));
                }

                return Ok("valid-code");
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }

        }

        [HttpPost("reset-password")]
        public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordDto model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.LoginIdentifier))
                {
                    return BadRequest("invalid-empty");
                }

                // Determinar si es un email o un número de teléfono
                bool isEmail = model.LoginIdentifier.Contains("@");

                var user = await _userManager.Users
                    .Where(u => (isEmail ? u.Email == model.LoginIdentifier : u.NormalizedPhoneNumber == model.LoginIdentifier) && u.UserType == model.UserType)
                    .SingleOrDefaultAsync();

                if (user == null)
                {
                    // No account looks like a wrong code, not "not-found"
                    return BadRequest("incorrect-code");
                }
                 
                if (user.PasswordResetCode == null || user.PasswordResetCodeExpires == null)
                {
                    return BadRequest("no-code");
                }

                if (DateTime.UtcNow > user.PasswordResetCodeExpires)
                {
                    return BadRequest("expired-code");
                }

                if (user.PasswordResetCode != model.Code)
                {
                    return BadRequest(await WrongCodeAsync(user));
                }

                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, resetToken, model.NewPassword);

                if (!result.Succeeded)
                {
                    return BadRequest("unkwnown-error");
                }

                user.PasswordResetCode = null;
                user.PasswordResetCodeExpires = null;
                // Receiving the code proves the mailbox, e.g. employees whose first access is this flow.
                user.EmailConfirmed = true;
                user.VerificationCodeFailedAttempts = 0;
                // A completed reset also lifts a login lockout.
                user.AccessFailedCount = 0;
                user.LockoutEnd = null;

                if (!result.Succeeded)
                { 
                    return BadRequest("unkwnown-error"); 
                }

                await _userManager.UpdateAsync(user);

                return Ok("password-reset");
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPost("resend-reset-code")]
        [EnableRateLimiting(RateLimitPolicies.AuthEmail)]
        public async Task<ActionResult> ResendResetCode([FromBody] ForgotPasswordRequestDto model)
        {
            if (string.IsNullOrEmpty(model.LoginIdentifier))
            {
                return BadRequest("invalid-empty");
            }

            // Determinar si es un email o un número de teléfono
            bool isEmail = model.LoginIdentifier.Contains("@");

            var user = await _userManager.Users
                .Where(u => (isEmail ? u.Email == model.LoginIdentifier : u.NormalizedPhoneNumber == model.LoginIdentifier) && u.UserType == model.UserType)
                .SingleOrDefaultAsync();
            if (user == null)
            {
                // Same answer as for a real account, so the form can't tell which emails exist
                return Ok("email-sent");
            }

            if (CodeLocked(user))
            {
                return BadRequest("too-many-attempts");
            }

            if (user.PasswordResetCode != null && user.PasswordResetCodeExpires > DateTime.UtcNow)
            {
                await _emailSender.SendEmailAsync(user.Email, T.Get(Lang.FromRequest(Request), "reset.subject"),
                    EmailLayout.Code(Lang.FromRequest(Request), T.Get(Lang.FromRequest(Request), "reset.intro"), user.PasswordResetCode));

                return Ok("email-sent");
            }

            var resetCode = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
            //var resetCode = "123456";
            user.PasswordResetCode = resetCode;
            user.PasswordResetCodeExpires = DateTime.UtcNow.AddMinutes(15);

            await _userManager.UpdateAsync(user);


            await _emailSender.SendEmailAsync(user.Email, T.Get(Lang.FromRequest(Request), "reset.subject"),
                EmailLayout.Code(Lang.FromRequest(Request), T.Get(Lang.FromRequest(Request), "reset.intro"), resetCode));


            return Ok("email-sent");
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<ActionResult> Logout([FromBody] LogoutDto model)
        {
            try
            {
                // Obtener el ID del usuario autenticado desde el Token JWT
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return Unauthorized("unauthorized-user");
                }

                // Buscar al usuario autenticado en la base de datos
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound("not-found");
                }

                // Eliminar el token del dispositivo si existe
                if (!string.IsNullOrEmpty(model.DeviceToken) && !string.IsNullOrEmpty(user.DeviceTokens))
                {
                    var tokens = user.DeviceTokens.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
                    if (tokens.Contains(model.DeviceToken))
                    {
                        tokens.Remove(model.DeviceToken);
                        user.DeviceTokens = string.Join(",", tokens);
                        await _userManager.UpdateAsync(user);
                    }
                }

                return Ok("logged-out");
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

    }
}
