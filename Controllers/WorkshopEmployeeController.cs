using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiTaller.Attributes;
using MiTaller.Data;
using MiTaller.DTO.Workshop.Employee;
using MiTaller.Models.Auth;
using MiTaller.Models.Notification;
using MiTaller.Models.Workshop;
using MiTaller.Services;
using System.Security.Claims;

namespace MiTaller.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WorkshopEmployeeController : ControllerBase
    {
        private readonly UserManager<BaseIdentityUser> _userManager;
        private readonly DataContext _context;
        private readonly IEmailSender _emailSender;

        public WorkshopEmployeeController(UserManager<BaseIdentityUser> userManager, DataContext context, IEmailSender emailSender)
        {
            _userManager = userManager;
            _context = context;
            _emailSender = emailSender;
        }
        [HttpPost("register-employee")]
        [AuthorizeEmployee("Administrador")]
        public async Task<ActionResult> RegisterEmployee(PostWorkshopEmployeeDto model)
        {
            try
            {
                // Validar que Permissions sea uno de los valores permitidos
                var validPermissions = new[] { "Ninguno", "Administrador", "Registrar vehículos" };
                if (!validPermissions.Contains(model.Permissions))
                {
                    return BadRequest("invalid-permissions");
                }

                var workshop = await _context.Workshops
                    .Where(w => w.Id == model.WorkshopId && !w.IsDeleted)
                    .FirstOrDefaultAsync();

                if (workshop == null)
                {
                    return NotFound("not-found");
                }

                // Normalize phone number
                var normalizedPhone = model.PhoneNumber?.Trim().Replace(" ", "").Replace("-", "");

                // Validate that email/phone are not already used in another WorkshopEmployee or Workshop user
                var existingEmployee = await _context.WorkshopEmployees
                    .Where(we => !we.IsDeleted && 
                        (we.Email.ToLower() == model.Email.ToLower() || 
                         we.PhoneNumber.Replace(" ", "").Replace("-", "") == normalizedPhone))
                    .FirstOrDefaultAsync();

                if (existingEmployee != null)
                {
                    return BadRequest("employee-already-registered");
                }

                var existingWorkshopUser = await _userManager.Users
                    .Where(u => u.UserType == UserType.Workshop && !u.IsDeleted &&
                        (u.Email.ToLower() == model.Email.ToLower() ||
                         u.NormalizedPhoneNumber.Replace(" ", "").Replace("-", "") == normalizedPhone))
                    .FirstOrDefaultAsync();

                if (existingWorkshopUser != null)
                {
                    return BadRequest("employee-already-registered");
                }

                // Create Employee identity user
                var newEmployeeUser = new Employee
                {
                    Id = Guid.NewGuid(),
                    UserName = model.Email + "_employee",
                    FullName = model.FullName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber + "_employee",
                    NormalizedPhoneNumber = normalizedPhone,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = null,
                    DeletedAt = null,
                    IsDeleted = false,
                    UserType = UserType.Employee
                };

                var result = await _userManager.CreateAsync(newEmployeeUser, "Password123!");
                if (!result.Succeeded)
                {
                    return BadRequest("unknown-error");
                }

                // Create WorkshopEmployees record
                var employee = new WorkshopEmployees
                {
                    WorkshopId = workshop.Id,
                    EmployeeId = newEmployeeUser.Id,
                    FullName = model.FullName,
                    PhoneNumber = model.PhoneNumber,
                    Email = model.Email,
                    Salary = model.Salary,
                    Role = model.Role,
                    Permissions = model.Permissions,
                    IsDeleted = false,
                };

                await _context.WorkshopEmployees.AddAsync(employee);

                // Send welcome email
                var subject = "¡Bienvenid@ a Mi taller!";
                var htmlBody = $@"
                    <!DOCTYPE html>
                    <html lang=""es"">
                      <head>
                        <meta charset=""UTF-8"" />
                        <title>Bienvenida MiTaller</title>
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
                            Hola <strong>{newEmployeeUser.FullName}</strong>. Te damos la bienvenida a MiTaller. Tu cuenta ha sido creada por el taller <strong>{workshop.WorkshopName}</strong>.
                          </p>
                          <p style=""font-size: 18px; margin-top: 20px;"">
                            Para comenzar a usar tu cuenta, descarga la aplicación y<br />
                            restablece tu contraseña
                          </p>
                          <p style=""margin-top: 30px;"">¡Saludos, gracias por tu descarga!</p>
                        </div>

                        <div style=""padding: 20px 30px; font-size: 12px; color: #555;"">
                            <p><strong>AVISO DE CONFIDENCIALIDAD.</strong> Este correo y la información contenida o adjunta al mismo es privada y confidencial y va dirigida exclusivamente a su destinatario. Mi Taller Financiero informa a quien pueda haber recibido este correo por error que contiene información confidencial cuyo uso, copia, reproducción o distribución está expresamente prohibida. Si no eres el destinatario del mismo y recibes este correo por error, te pedimos pongas en conocimiento al emisor y procedas a la eliminación sin copiarlo, imprimirlo o utilizarlo de ningún modo.</p>
                            <p><strong>CONFIDENTIALITY WARNING.</strong> This message and the information contained in or attached to it are private and confidential and intended exclusively for the addressee. Comisiónn Federal de Electricidad informs whomit may receive it in error that it contains privileged information and its use, copy, reproduction or distributions is prohibited.  If you are not an intended recipient of this E-mail, please notify the sender, delete it and do not read, act upon, print, disclose, copy, retain or redistribute any portion of this E-mail.</p>
                        </div>
                      </body>
                    </html>
                    ";

                await _emailSender.SendEmailAsync(newEmployeeUser.Email, subject, htmlBody);

                // Create NotificationSettings
                var notificationSettings = new NotificationSettings
                {
                    UserId = newEmployeeUser.Id,
                    UserType = UserType.Employee,
                };

                await _context.NotificationSettings.AddAsync(notificationSettings);
                await _context.SaveChangesAsync();

                return Ok("employee-created");
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpGet("get-employees-by/{workshopId}")]
        public async Task<ActionResult<ICollection<WorkshopEmployeeResponseDto>>> GetEmployeesByWorkshop(Guid workshopId)
        {
            try
            {
                var workshop = await _context.Workshops
                    .Where(w => w.Id == workshopId && !w.IsDeleted)
                    .FirstOrDefaultAsync();

                if (workshop == null)
                {
                    return NotFound("not-found");
                }

                var employeesDto = new List<WorkshopEmployeeResponseDto>();

                var employees = await _context.WorkshopEmployees
                    .Where(e => e.WorkshopId == workshopId && !e.IsDeleted)
                    .ToListAsync();

                if (employees == null)
                {
                    return NotFound("not-found");
                }

                foreach (var model in employees)
                {
                    var newEmployee = new WorkshopEmployeeResponseDto
                    {
                        Id = model.Id,
                        FullName = model.FullName,
                        PhoneNumber = model.PhoneNumber,
                        Email = model.Email,
                        Salary = model.Salary,
                        Role = model.Role,
                        Permissions = model.Permissions,
                    };
                    employeesDto.Add(newEmployee);
                }

                return Ok(employeesDto);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpGet("get-employee-by/{employeeId}")]
        public async Task<ActionResult<WorkshopEmployeeResponseDto>> GetEmployeeById(Guid employeeId)
        {
            try
            {
                var employee = await _context.WorkshopEmployees
                    .Where(e => e.Id == employeeId && !e.IsDeleted)
                    .FirstOrDefaultAsync();

                if (employee == null)
                {
                    return NotFound("not-found");
                }

                var employeeDto = new WorkshopEmployeeResponseDto
                {
                    Id = employeeId,
                    FullName = employee.FullName,
                    PhoneNumber = employee.PhoneNumber,
                    Email = employee.Email,
                    Salary = employee.Salary,
                    Role = employee.Role,
                    Permissions = employee.Permissions,
                };


                return Ok(employeeDto);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPut("update-employee/{employeeId}")]
        [AuthorizeEmployee("Administrador")]
        public async Task<ActionResult> UpdateEmployee(Guid employeeId, PostWorkshopEmployeeDto model)
        {
            try
            {
                var workshop = await _context.Workshops
                    .Where(w => w.Id == model.WorkshopId && !w.IsDeleted)
                    .FirstOrDefaultAsync();

                if (workshop == null)
                {
                    return NotFound("not-found");
                }

                var workshopEmployee = await _context.WorkshopEmployees
                    .Where(we => we.Id == employeeId && !we.IsDeleted)
                    .FirstOrDefaultAsync();

                if (workshopEmployee == null)
                {
                    return NotFound("not-found");
                }

                // Validar que Permissions sea uno de los valores permitidos
                var validPermissions = new[] { "Ninguno", "Administrador", "Registrar vehículos" };
                if (!validPermissions.Contains(model.Permissions))
                {
                    return BadRequest("invalid-permissions");
                }

                // Normalize phone number
                var normalizedPhone = model.PhoneNumber?.Trim().Replace(" ", "").Replace("-", "");

                // Validate that email/phone are not already used in another WorkshopEmployee (excluding current) or Workshop user
                var existingEmployee = await _context.WorkshopEmployees
                    .Where(we => !we.IsDeleted && 
                        we.Id != employeeId &&
                        (we.Email.ToLower() == model.Email.ToLower() || 
                         we.PhoneNumber.Replace(" ", "").Replace("-", "") == normalizedPhone))
                    .FirstOrDefaultAsync();

                if (existingEmployee != null)
                {
                    return BadRequest("employee-already-registered");
                }

                var existingWorkshopUser = await _userManager.Users
                    .Where(u => u.UserType == UserType.Workshop && !u.IsDeleted &&
                        (u.Email.ToLower() == model.Email.ToLower() ||
                         u.NormalizedPhoneNumber.Replace(" ", "").Replace("-", "") == normalizedPhone))
                    .FirstOrDefaultAsync();

                if (existingWorkshopUser != null)
                {
                    return BadRequest("employee-already-registered");
                }

                // Update WorkshopEmployees
                workshopEmployee.FullName = model.FullName;
                workshopEmployee.PhoneNumber = model.PhoneNumber;
                workshopEmployee.Email = model.Email;
                workshopEmployee.Salary = model.Salary;
                workshopEmployee.Role = model.Role;
                workshopEmployee.Permissions = model.Permissions;

                // Update Employee identity user if it exists
                if (workshopEmployee.EmployeeId.HasValue)
                {
                    var employeeUser = await _userManager.Users
                        .OfType<Employee>()
                        .Where(u => u.Id == workshopEmployee.EmployeeId.Value)
                        .FirstOrDefaultAsync();

                    if (employeeUser != null)
                    {
                        employeeUser.FullName = model.FullName;
                        employeeUser.Email = model.Email;
                        employeeUser.NormalizedPhoneNumber = normalizedPhone;
                        employeeUser.PhoneNumber = model.PhoneNumber + "_employee";
                        employeeUser.UpdatedAt = DateTime.UtcNow;
                        await _userManager.UpdateAsync(employeeUser);
                    }
                }

                await _context.SaveChangesAsync();

                return Ok("employee-updated");
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpDelete("employee/{employeeId}")]
        [AuthorizeEmployee("Administrador")]
        public async Task<ActionResult> DeleteEmployee(Guid employeeId)
        {
            try
            {
                var employee = await _context.WorkshopEmployees
                    .Where(e => e.Id == employeeId && !e.IsDeleted)
                    .FirstOrDefaultAsync();

                if (employee == null)
                {
                    return NotFound("not-found");
                }

                // Get the current user's ID from the JWT claims
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return Unauthorized("unauthorized-user");
                }

                // Check if the user is trying to delete themselves
                if (employee.EmployeeId.HasValue && employee.EmployeeId.Value.ToString() == userId)
                {
                    return BadRequest("cannot-delete-yourself");
                }

                employee.IsDeleted = true;

                await _context.SaveChangesAsync();

                return Ok("employee-deleted");
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }
    }
}
