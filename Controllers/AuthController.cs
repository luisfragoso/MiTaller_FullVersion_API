using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

namespace MiTaller.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<BaseIdentityUser> _userManager;
        private readonly JwtService _jwtService;
        private readonly IEmailSender _emailSender;
        private readonly DataContext _context;

        public AuthController(UserManager<BaseIdentityUser> userManager, JwtService jwtService, IEmailSender emailSender, DataContext dataContext)
        {
            _userManager = userManager;
            _jwtService = jwtService;
            _emailSender = emailSender;
            _context = dataContext;
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
                    return Unauthorized("not-found");
                }

                if (user.IsDeleted)
                {
                    return Unauthorized("user-deleted");
                }

                if (!await _userManager.CheckPasswordAsync(user, model.Password))
                {
                    return Unauthorized("wrong-password");
                }

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
                    UserType = UserType.Customer
                };

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
                    Title = "Cuenta creada exitosamente",
                    Content = "¡Tu cuenta ha sido creada con éxito! Ahora puedes acceder a todas las funcionalidades de tu cuenta.",
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
                    Title = "Cuenta creada exitosamente",
                    Content = "¡Tu cuenta ha sido creada con éxito! Ahora puedes acceder a todas las funcionalidades de tu cuenta.",
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

            var verificationCode = new Random().Next(100000, 999999).ToString();
            //var verificationCode = "123456";
            user.EmailVerificationCode = verificationCode;
            user.EmailVerificationExpires = DateTime.UtcNow.AddMinutes(15);

            await _userManager.UpdateAsync(user);

            var htmlBody = $@"
                <!DOCTYPE html>
                <html lang=""es"">
                    <head>
                    <meta charset=""UTF-8"" />
                    <title>Verificación MiTaller</title>
                    </head>
                    <body style=""margin: 0; font-family: Arial, sans-serif; background-color: #ffffff;"">
                    <div style=""background-color: #f52222; padding: 20px 0; text-align: center;"">
                        <h1 style=""margin: 0; color: white;"">
                        <span style=""font-weight: bold;"">MiTaller</span> <span style=""color: black;"">Financiero</span>
                        </h1>
                    </div>

                    <div style=""padding: 30px; text-align: center; color: #333;"">
                        <h2 style=""margin-top: 0;"">Hola</h2>
                        <p style=""font-size: 18px;"">Este es tu código de verificación para acceder a la app MiTaller.</p>
                        <p style=""font-size: 32px; font-weight: bold; color: #f52222;"">{verificationCode}</p>
                        <p style=""margin-top: 20px;"">Este código expirará en 15 minutos.</p>
                        <p style=""margin-top: 30px;"">¡Saludos, gracias por tu descarga!</p>
                    </div>

                    <div style=""padding: 20px 30px; font-size: 12px; color: #555;"">
                        <p><strong>AVISO DE CONFIDENCIALIDAD.</strong> Este correo y la información contenida o adjunta al mismo es privada y confidencial y va dirigida exclusivamente a su destinatario. Mi Taller Financiero informa a quien pueda haber recibido este correo por error que contiene información confidencial cuyo uso, copia, reproducción o distribución está expresamente prohibida. Si no eres el destinatario del mismo y recibes este correo por error, te pedimos pongas en conocimiento al emisor y procedas a la eliminación sin copiarlo, imprimirlo o utilizarlo de ningún modo.</p>
                        <p><strong>CONFIDENTIALITY WARNING.</strong> This message and the information contained in or attached to it are private and confidential and intended exclusively for the addressee. Comisiónn Federal de Electricidad informs whomit may receive it in error that it contains privileged information and its use, copy, reproduction or distributions is prohibited.  If you are not an intended recipient of this E-mail, please notify the sender, delete it and do not read, act upon, print, disclose, copy, retain or redistribute any portion of this E-mail.</p>
                    </div>
                    </body>
                </html>
                ";

            await _emailSender.SendEmailAsync(user.Email, "Verificación de Correo", htmlBody);
            
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
                return BadRequest("incorrect-code");
            }

            user.EmailConfirmed = true;
            user.EmailVerificationCode = null;
            user.EmailVerificationExpires = null;

            await _userManager.UpdateAsync(user);

            return Ok("user-verified");
        }

        [HttpPost("resend-email-verification")]
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

            if (user.EmailVerificationCode != null && user.EmailVerificationExpires > DateTime.UtcNow)
            {
                var htmlBody = $@"
                <!DOCTYPE html>
                <html lang=""es"">
                    <head>
                    <meta charset=""UTF-8"" />
                    <title>Verificación MiTaller</title>
                    </head>
                    <body style=""margin: 0; font-family: Arial, sans-serif; background-color: #ffffff;"">
                    <div style=""background-color: #f52222; padding: 20px 0; text-align: center;"">
                        <h1 style=""margin: 0; color: white;"">
                        <span style=""font-weight: bold;"">MiTaller</span> <span style=""color: black;"">Financiero</span>
                        </h1>
                    </div>

                    <div style=""padding: 30px; text-align: center; color: #333;"">
                        <h2 style=""margin-top: 0;"">Hola</h2>
                        <p style=""font-size: 18px;"">Este es tu código de verificación para acceder a la app MiTaller.</p>
                        <p style=""font-size: 32px; font-weight: bold; color: #f52222;"">{user.EmailVerificationCode}</p>
                        <p style=""margin-top: 20px;"">Este código expirará en 15 minutos.</p>
                        <p style=""margin-top: 30px;"">¡Saludos, gracias por tu descarga!</p>
                    </div>

                    <div style=""padding: 20px 30px; font-size: 12px; color: #555;"">
                        <p><strong>AVISO DE CONFIDENCIALIDAD.</strong> Este correo y la información contenida o adjunta al mismo es privada y confidencial y va dirigida exclusivamente a su destinatario. Mi Taller Financiero informa a quien pueda haber recibido este correo por error que contiene información confidencial cuyo uso, copia, reproducción o distribución está expresamente prohibida. Si no eres el destinatario del mismo y recibes este correo por error, te pedimos pongas en conocimiento al emisor y procedas a la eliminación sin copiarlo, imprimirlo o utilizarlo de ningún modo.</p>
                        <p><strong>CONFIDENTIALITY WARNING.</strong> This message and the information contained in or attached to it are private and confidential and intended exclusively for the addressee. Comisiónn Federal de Electricidad informs whomit may receive it in error that it contains privileged information and its use, copy, reproduction or distributions is prohibited.  If you are not an intended recipient of this E-mail, please notify the sender, delete it and do not read, act upon, print, disclose, copy, retain or redistribute any portion of this E-mail.</p>
                    </div>
                    </body>
                </html>
                ";

                await _emailSender.SendEmailAsync(user.Email, "Verificación de Correo", htmlBody);

                return Ok("email-sent");
            }

            var verificationCode = new Random().Next(100000, 999999).ToString();
            //var verificationCode = "123456";
            user.EmailVerificationCode = verificationCode;
            user.EmailVerificationExpires = DateTime.UtcNow.AddMinutes(15);

            await _userManager.UpdateAsync(user);

            var htmlBody2 = $@"
                <!DOCTYPE html>
                <html lang=""es"">
                    <head>
                    <meta charset=""UTF-8"" />
                    <title>Verificación MiTaller</title>
                    </head>
                    <body style=""margin: 0; font-family: Arial, sans-serif; background-color: #ffffff;"">
                    <div style=""background-color: #f52222; padding: 20px 0; text-align: center;"">
                        <h1 style=""margin: 0; color: white;"">
                        <span style=""font-weight: bold;"">MiTaller</span> <span style=""color: black;"">Financiero</span>
                        </h1>
                    </div>

                    <div style=""padding: 30px; text-align: center; color: #333;"">
                        <h2 style=""margin-top: 0;"">Hola</h2>
                        <p style=""font-size: 18px;"">Este es tu código de verificación para acceder a la app MiTaller.</p>
                        <p style=""font-size: 32px; font-weight: bold; color: #f52222;"">{verificationCode}</p>
                        <p style=""margin-top: 20px;"">Este código expirará en 15 minutos.</p>
                        <p style=""margin-top: 30px;"">¡Saludos, gracias por tu descarga!</p>
                    </div>

                    <div style=""padding: 20px 30px; font-size: 12px; color: #555;"">
                        <p><strong>AVISO DE CONFIDENCIALIDAD.</strong> Este correo y la información contenida o adjunta al mismo es privada y confidencial y va dirigida exclusivamente a su destinatario. Mi Taller Financiero informa a quien pueda haber recibido este correo por error que contiene información confidencial cuyo uso, copia, reproducción o distribución está expresamente prohibida. Si no eres el destinatario del mismo y recibes este correo por error, te pedimos pongas en conocimiento al emisor y procedas a la eliminación sin copiarlo, imprimirlo o utilizarlo de ningún modo.</p>
                        <p><strong>CONFIDENTIALITY WARNING.</strong> This message and the information contained in or attached to it are private and confidential and intended exclusively for the addressee. Comisiónn Federal de Electricidad informs whomit may receive it in error that it contains privileged information and its use, copy, reproduction or distributions is prohibited.  If you are not an intended recipient of this E-mail, please notify the sender, delete it and do not read, act upon, print, disclose, copy, retain or redistribute any portion of this E-mail.</p>
                    </div>
                    </body>
                </html>
                ";

            await _emailSender.SendEmailAsync(user.Email, "Verificación de Correo", htmlBody2);


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
                    return Ok("password-updated");
                }

                return BadRequest("unkwnown-error");
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPost("forgot-password")]
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
                    return NotFound("not-found");
                }

                var resetCode = new Random().Next(100000, 999999).ToString();
                //var resetCode = "123456";
                user.PasswordResetCode = resetCode;
                user.PasswordResetCodeExpires = DateTime.UtcNow.AddMinutes(15);

                await _userManager.UpdateAsync(user);

                var htmlBody = $@"
                    <!DOCTYPE html>
                    <html lang=""es"">
                      <head>
                        <meta charset=""UTF-8"" />
                        <title>Recuperación MiTaller</title>
                      </head>
                      <body style=""margin: 0; font-family: Arial, sans-serif; background-color: #ffffff;"">
                        <div style=""background-color: #f52222; padding: 20px 0; text-align: center;"">
                          <h1 style=""margin: 0; color: white;"">
                            <span style=""font-weight: bold;"">MiTaller</span> <span style=""color: black;"">Financiero</span>
                          </h1>
                        </div>

                        <div style=""padding: 30px; text-align: center; color: #333;"">
                          <h2 style=""margin-top: 0;"">Hola</h2>
                          <p style=""font-size: 18px;"">
                            Este es tu código de verificación para recuperar tu contraseña y que<br/>
                            puedas acceder a la app MiTaller.
                          </p>
                          <p style=""font-size: 32px; font-weight: bold; color: #f52222;"">{resetCode}</p>
                          <p style=""margin-top: 20px;"">Este código expirará el 15 minutos.</p>
                          <p style=""margin-top: 30px;"">¡Saludos, gracias por tu descarga!</p>
                        </div>

                        <div style=""padding: 20px 30px; font-size: 12px; color: #555;"">
                        <p><strong>AVISO DE CONFIDENCIALIDAD.</strong> Este correo y la información contenida o adjunta al mismo es privada y confidencial y va dirigida exclusivamente a su destinatario. Mi Taller Financiero informa a quien pueda haber recibido este correo por error que contiene información confidencial cuyo uso, copia, reproducción o distribución está expresamente prohibida. Si no eres el destinatario del mismo y recibes este correo por error, te pedimos pongas en conocimiento al emisor y procedas a la eliminación sin copiarlo, imprimirlo o utilizarlo de ningún modo.</p>
                        <p><strong>CONFIDENTIALITY WARNING.</strong> This message and the information contained in or attached to it are private and confidential and intended exclusively for the addressee. Comisiónn Federal de Electricidad informs whomit may receive it in error that it contains privileged information and its use, copy, reproduction or distributions is prohibited.  If you are not an intended recipient of this E-mail, please notify the sender, delete it and do not read, act upon, print, disclose, copy, retain or redistribute any portion of this E-mail.</p>
                    </div>
                      </body>
                    </html>
                    ";

                await _emailSender.SendEmailAsync(user.Email, "Recuperación de Contraseña", htmlBody);

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
                    return NotFound("not-found");
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
                    return BadRequest("incorrect-code");
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
                    return NotFound("not-found");
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
                    return BadRequest("incorrect-code");
                }

                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, resetToken, model.NewPassword);

                if (!result.Succeeded)
                {
                    return BadRequest("unkwnown-error");
                }

                user.PasswordResetCode = null;
                user.PasswordResetCodeExpires = null;

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
                return NotFound("not-found");
            }

            if (user.PasswordResetCode != null && user.PasswordResetCodeExpires > DateTime.UtcNow)
            {
                var htmlBody = $@"
                    <!DOCTYPE html>
                    <html lang=""es"">
                      <head>
                        <meta charset=""UTF-8"" />
                        <title>Recuperación MiTaller</title>
                      </head>
                      <body style=""margin: 0; font-family: Arial, sans-serif; background-color: #ffffff;"">
                        <div style=""background-color: #f52222; padding: 20px 0; text-align: center;"">
                          <h1 style=""margin: 0; color: white;"">
                            <span style=""font-weight: bold;"">MiTaller</span> <span style=""color: black;"">Financiero</span>
                          </h1>
                        </div>

                        <div style=""padding: 30px; text-align: center; color: #333;"">
                          <h2 style=""margin-top: 0;"">Hola</h2>
                          <p style=""font-size: 18px;"">
                            Este es tu código de verificación para recuperar tu contraseña y que<br/>
                            puedas acceder a la app MiTaller.
                          </p>
                          <p style=""font-size: 32px; font-weight: bold; color: #f52222;"">{user.PasswordResetCode}</p>
                          <p style=""margin-top: 20px;"">Este código expirará el 15 minutos.</p>
                          <p style=""margin-top: 30px;"">¡Saludos, gracias por tu descarga!</p>
                        </div>

                        <div style=""padding: 20px 30px; font-size: 12px; color: #555;"">
                            <p><strong>AVISO DE CONFIDENCIALIDAD.</strong> Este correo y la información contenida o adjunta al mismo es privada y confidencial y va dirigida exclusivamente a su destinatario. Mi Taller Financiero informa a quien pueda haber recibido este correo por error que contiene información confidencial cuyo uso, copia, reproducción o distribución está expresamente prohibida. Si no eres el destinatario del mismo y recibes este correo por error, te pedimos pongas en conocimiento al emisor y procedas a la eliminación sin copiarlo, imprimirlo o utilizarlo de ningún modo.</p>
                            <p><strong>CONFIDENTIALITY WARNING.</strong> This message and the information contained in or attached to it are private and confidential and intended exclusively for the addressee. Comisiónn Federal de Electricidad informs whomit may receive it in error that it contains privileged information and its use, copy, reproduction or distributions is prohibited.  If you are not an intended recipient of this E-mail, please notify the sender, delete it and do not read, act upon, print, disclose, copy, retain or redistribute any portion of this E-mail.</p>
                        </div>
                      </body>
                    </html>
                    ";

                await _emailSender.SendEmailAsync(user.Email, "Recuperación de Contraseña", htmlBody);

                return Ok("email-sent");
            }

            var resetCode = new Random().Next(100000, 999999).ToString();
            //var resetCode = "123456";
            user.PasswordResetCode = resetCode;
            user.PasswordResetCodeExpires = DateTime.UtcNow.AddMinutes(15);

            await _userManager.UpdateAsync(user);


            var htmlBody2 = $@"
                    <!DOCTYPE html>
                    <html lang=""es"">
                      <head>
                        <meta charset=""UTF-8"" />
                        <title>Recuperación MiTaller</title>
                      </head>
                      <body style=""margin: 0; font-family: Arial, sans-serif; background-color: #ffffff;"">
                        <div style=""background-color: #f52222; padding: 20px 0; text-align: center;"">
                          <h1 style=""margin: 0; color: white;"">
                            <span style=""font-weight: bold;"">MiTaller</span> <span style=""color: black;"">Financiero</span>
                          </h1>
                        </div>

                        <div style=""padding: 30px; text-align: center; color: #333;"">
                          <h2 style=""margin-top: 0;"">Hola</h2>
                          <p style=""font-size: 18px;"">
                            Este es tu código de verificación para recuperar tu contraseña y que<br/>
                            puedas acceder a la app MiTaller.
                          </p>
                          <p style=""font-size: 32px; font-weight: bold; color: #f52222;"">{resetCode}</p>
                          <p style=""margin-top: 20px;"">Este código expirará el 15 minutos.</p>
                          <p style=""margin-top: 30px;"">¡Saludos, gracias por tu descarga!</p>
                        </div>

                        <div style=""padding: 20px 30px; font-size: 12px; color: #555;"">
                        <p><strong>AVISO DE CONFIDENCIALIDAD.</strong> Este correo y la información contenida o adjunta al mismo es privada y confidencial y va dirigida exclusivamente a su destinatario. Mi Taller Financiero informa a quien pueda haber recibido este correo por error que contiene información confidencial cuyo uso, copia, reproducción o distribución está expresamente prohibida. Si no eres el destinatario del mismo y recibes este correo por error, te pedimos pongas en conocimiento al emisor y procedas a la eliminación sin copiarlo, imprimirlo o utilizarlo de ningún modo.</p>
                        <p><strong>CONFIDENTIALITY WARNING.</strong> This message and the information contained in or attached to it are private and confidential and intended exclusively for the addressee. Comisiónn Federal de Electricidad informs whomit may receive it in error that it contains privileged information and its use, copy, reproduction or distributions is prohibited.  If you are not an intended recipient of this E-mail, please notify the sender, delete it and do not read, act upon, print, disclose, copy, retain or redistribute any portion of this E-mail.</p>
                    </div>
                      </body>
                    </html>
                    ";

            await _emailSender.SendEmailAsync(user.Email, "Recuperación de Contraseña", htmlBody2);


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
