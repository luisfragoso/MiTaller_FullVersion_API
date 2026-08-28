using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiTaller.Data;
using MiTaller.DTO.Admin;
using MiTaller.DTO.Pager;
using MiTaller.Models.Audit;
using MiTaller.Models.Auth;
using MiTaller.Models.Customer;
using MiTaller.Models.Workshop;

namespace MiTaller.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "PlatformAdmin")]
    [EnableCors("AdminPortal")]
    public class AdminController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly UserManager<BaseIdentityUser> _userManager;

        public AdminController(DataContext context, UserManager<BaseIdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet("whoami")]
        public ActionResult WhoAmI()
        {
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var id = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return Ok(new { id, email });
        }

        [HttpGet("users")]
        public async Task<ActionResult<PagerWithCountResponseDto<AdminUserListItemDto>>> GetUsers(
            [FromQuery] string? query, [FromQuery] string? userType, [FromQuery] bool deletedOnly = false,
            [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
        {
            var includeCustomers = userType == null || userType == "Customer";
            var includeWorkshops = userType == null || userType == "Workshop";

            var customersQuery = _context.Customers
                .Where(c => !c.IsPurged && c.IsDeleted == deletedOnly)
                .AsQueryable();
            var workshopsQuery = _context.Workshops
                .Where(w => !w.IsPurged && w.IsDeleted == deletedOnly)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                customersQuery = customersQuery.Where(c =>
                    c.FullName.Contains(query) || c.Email.Contains(query) || c.PhoneNumber.Contains(query));
                workshopsQuery = workshopsQuery.Where(w =>
                    w.WorkshopName.Contains(query) || w.AssociateFullName.Contains(query) ||
                    w.Email.Contains(query) || w.PhoneNumber.Contains(query));
            }

            var customers = includeCustomers
                ? await customersQuery
                    .Select(c => new AdminUserListItemDto
                    {
                        Id = c.Id,
                        UserType = "Customer",
                        Name = c.FullName,
                        Email = c.Email ?? string.Empty,
                        Phone = c.PhoneNumber,
                        CreatedAt = c.CreatedAt,
                        IsDeleted = c.IsDeleted,
                        DeletedAt = c.DeletedAt,
                        LastLoginAt = c.LastLoginAt,
                    })
                    .ToListAsync()
                : new List<AdminUserListItemDto>();

            var workshops = includeWorkshops
                ? await workshopsQuery
                    .Select(w => new AdminUserListItemDto
                    {
                        Id = w.Id,
                        UserType = "Workshop",
                        Name = w.WorkshopName,
                        Email = w.Email ?? string.Empty,
                        Phone = w.PhoneNumber,
                        CreatedAt = w.CreatedAt,
                        IsDeleted = w.IsDeleted,
                        DeletedAt = w.DeletedAt,
                        LastLoginAt = w.LastLoginAt,
                    })
                    .ToListAsync()
                : new List<AdminUserListItemDto>();

            // Combinados y paginados en memoria: los volúmenes de talleres/clientes hoy
            // no justifican un UNION a nivel SQL; revisar si esto crece mucho.
            var combined = customers.Concat(workshops)
                .OrderByDescending(u => u.CreatedAt)
                .ToList();

            var totalElements = combined.Count;
            var totalPages = (int)Math.Ceiling((double)totalElements / pageSize);
            var page = combined.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            return Ok(new PagerWithCountResponseDto<AdminUserListItemDto>
            {
                CurrentPage = pageNumber,
                TotalPages = totalPages,
                TotalElements = totalElements,
                Elements = page,
            });
        }

        [HttpGet("users/{id}/detail")]
        public async Task<ActionResult<AdminUserDetailDto>> GetUserDetail(Guid id)
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == id && !c.IsPurged);
            if (customer != null)
            {
                var dto = new AdminUserDetailDto
                {
                    Id = customer.Id,
                    UserType = "Customer",
                    Name = customer.FullName,
                    Email = customer.Email ?? string.Empty,
                    Phone = customer.PhoneNumber,
                    CreatedAt = customer.CreatedAt,
                    EmailConfirmed = customer.EmailConfirmed,
                    IsDeleted = customer.IsDeleted,
                    DeletedAt = customer.DeletedAt,
                    LastLoginAt = customer.LastLoginAt,
                };

                dto.Vehicles = await _context.Vehicles
                    .Where(v => v.CustomerId == id && !v.IsDeleted)
                    .Select(v => new AdminVehicleSummaryDto
                    {
                        Id = v.Id,
                        Brand = v.BrandId == -1 ? (v.OtherBrand ?? "") : v.Brand.Name,
                        Model = v.VehicleModelId == -1 ? (v.OtherVehicleModel ?? "") : v.VehicleModel.Model,
                        Plates = v.Plates,
                        Year = v.Year,
                    })
                    .ToListAsync();

                dto.Quotations = await _context.Quotations
                    .Where(q => q.CustomerId == id)
                    .Include(q => q.Workshop)
                    .OrderByDescending(q => q.CreatedAt)
                    .Select(q => new AdminQuotationSummaryDto
                    {
                        Id = q.Id,
                        WorkshopName = q.Workshop != null ? q.Workshop.WorkshopName : "",
                        Status = q.Status,
                        Total = q.PriceOfLabor + q.PriceOfSpareParts,
                        CreatedAt = q.CreatedAt,
                    })
                    .ToListAsync();

                dto.Appointments = await _context.Appointments
                    .Where(a => a.CustomerId == id)
                    .Include(a => a.Workshop)
                    .OrderByDescending(a => a.Date)
                    .Select(a => new AdminAppointmentSummaryDto
                    {
                        Id = a.Id,
                        WorkshopName = a.Workshop != null ? a.Workshop.WorkshopName : "",
                        Date = a.Date,
                    })
                    .ToListAsync();

                var customerAddress = await _context.CustomerAddresses
                    .Where(a => a.CustomerId == id && !a.IsDeleted)
                    .Include(a => a.Suburb).ThenInclude(s => s.Town).ThenInclude(t => t.State)
                    .OrderByDescending(a => a.CreatedAt)
                    .FirstOrDefaultAsync();
                dto.Address = customerAddress != null
                    ? $"{customerAddress.Street}, {customerAddress.Suburb.Name}, " +
                      $"{customerAddress.Suburb.Town.Name}, {customerAddress.Suburb.Town.State.Name}, " +
                      $"CP {customerAddress.Suburb.Zipcode}"
                    : null;

                dto.VisitedWorkshops = await _context.WorkshopCustomers
                    .Where(wc => wc.CustomerId == id)
                    .Include(wc => wc.Workshop)
                    .Select(wc => new AdminWorkshopSummaryDto
                    {
                        Id = wc.WorkshopId,
                        Name = wc.Workshop.WorkshopName,
                    })
                    .Distinct()
                    .ToListAsync();

                dto.Tags = await _context.CustomerAssociatedTags
                    .Where(t => t.CustomerId == id)
                    .Include(t => t.Tag)
                    .Include(t => t.Workshop)
                    .Select(t => new AdminTagSummaryDto
                    {
                        Id = t.TagId,
                        Value = t.Tag.Value,
                        HexColor = t.Tag.HexColor,
                        WorkshopName = t.Workshop.WorkshopName,
                    })
                    .ToListAsync();

                return Ok(dto);
            }

            var workshop = await _context.Workshops.FirstOrDefaultAsync(w => w.Id == id && !w.IsPurged);
            if (workshop != null)
            {
                var dto = new AdminUserDetailDto
                {
                    Id = workshop.Id,
                    UserType = "Workshop",
                    Name = workshop.WorkshopName,
                    Email = workshop.Email ?? string.Empty,
                    Phone = workshop.PhoneNumber,
                    CreatedAt = workshop.CreatedAt,
                    EmailConfirmed = workshop.EmailConfirmed,
                    IsDeleted = workshop.IsDeleted,
                    DeletedAt = workshop.DeletedAt,
                    LastLoginAt = workshop.LastLoginAt,
                };

                dto.WorkshopServices = await _context.WorkshopServices
                    .Where(ws => ws.WorkshopId == id && !ws.IsDeleted)
                    .Include(ws => ws.Service)
                    .Select(ws => new AdminWorkshopServiceSummaryDto
                    {
                        Id = ws.Id,
                        ServiceName = ws.Service.Name,
                        Price = ws.Price,
                    })
                    .ToListAsync();

                dto.LinkedCustomers = await _context.WorkshopCustomers
                    .Where(wc => wc.WorkshopId == id)
                    .Include(wc => wc.Customer)
                    .Select(wc => new AdminCustomerSummaryDto
                    {
                        Id = wc.CustomerId,
                        Name = wc.Customer.FullName,
                        Email = wc.Customer.Email ?? string.Empty,
                    })
                    .ToListAsync();

                dto.Reviews = await _context.Reviews
                    .Where(r => r.WorkshopId == id)
                    .Include(r => r.Customer)
                    .OrderByDescending(r => r.Date)
                    .Select(r => new AdminReviewSummaryDto
                    {
                        Id = r.Id,
                        CustomerName = r.Customer.FullName,
                        Rate = r.Rate,
                        Comment = r.Comment,
                        Date = r.Date,
                    })
                    .ToListAsync();

                dto.AverageRating = dto.Reviews.Count > 0
                    ? dto.Reviews.Average(r => r.Rate)
                    : null;

                var workshopAddress = await _context.WorkshopAddresses
                    .Where(a => a.WorkshopId == id && !a.IsDeleted)
                    .Include(a => a.Suburb).ThenInclude(s => s.Town).ThenInclude(t => t.State)
                    .OrderByDescending(a => a.CreatedAt)
                    .FirstOrDefaultAsync();
                dto.Address = workshopAddress != null
                    ? $"{workshopAddress.Street}, {workshopAddress.Suburb.Name}, " +
                      $"{workshopAddress.Suburb.Town.Name}, {workshopAddress.Suburb.Town.State.Name}, " +
                      $"CP {workshopAddress.Suburb.Zipcode}"
                    : null;

                return Ok(dto);
            }

            return NotFound("not-found");
        }

        [HttpPost("users/{id}/reset-password")]
        public async Task<ActionResult> ResetPassword(Guid id, [FromBody] AdminResetPasswordRequestDto model)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound("not-found");
            }

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, resetToken, model.NewPassword);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors.Select(e => e.Description));
            }

            return Ok("password-reset");
        }

        [HttpPut("users/{id}")]
        public async Task<ActionResult> UpdateUser(Guid id, [FromBody] AdminUpdateUserRequestDto model)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound("not-found");
            }

            var duplicate = await _userManager.Users
                .AnyAsync(u => u.Id != id && u.UserType == user.UserType &&
                    (u.Email == model.Email || (model.Phone != null && u.NormalizedPhoneNumber == model.Phone)));
            if (duplicate)
            {
                return BadRequest("Ya existe otro usuario con ese correo o teléfono.");
            }

            if (user is Customer customer)
            {
                customer.FullName = model.Name;
                customer.Email = model.Email;
                if (model.Phone != null)
                {
                    customer.NormalizedPhoneNumber = model.Phone;
                    customer.PhoneNumber = model.Phone + "_customer";
                }
            }
            else if (user is Workshop workshop)
            {
                workshop.WorkshopName = model.Name;
                workshop.Email = model.Email;
                if (model.Phone != null)
                {
                    workshop.NormalizedPhoneNumber = model.Phone;
                    workshop.PhoneNumber = model.Phone + "_workshop";
                }
            }
            else
            {
                return BadRequest("Este tipo de usuario no se puede editar desde aquí.");
            }

            user.UpdatedAt = DateTime.Now;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors.Select(e => e.Description));
            }

            return Ok("user-updated");
        }

        [HttpPost("users/{id}/soft-delete")]
        public async Task<ActionResult> SoftDeleteUser(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound("not-found");
            }

            user.IsDeleted = true;
            user.DeletedAt = DateTime.Now;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors.Select(e => e.Description));
            }

            return Ok("user-deleted");
        }

        [HttpPost("users/{id}/reactivate")]
        public async Task<ActionResult> ReactivateUser(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound("not-found");
            }

            user.IsDeleted = false;
            user.DeletedAt = null;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors.Select(e => e.Description));
            }

            return Ok("user-reactivated");
        }

        // "Purgar" no borra ninguna fila ni historial: solo oculta el registro
        // para siempre en el admin. Se eligió así porque casi todas las
        // relaciones (vehículos, cotizaciones, reseñas, etc.) están
        // configuradas como Restrict en la base de datos - un borrado físico
        // real requeriría cascadear manualmente docenas de tablas y sería
        // irreversible sobre datos de producción.
        [HttpPost("users/{id}/purge")]
        public async Task<ActionResult> PurgeUser(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound("not-found");
            }

            if (!user.IsDeleted)
            {
                return BadRequest("El usuario debe estar eliminado antes de poder purgarse.");
            }

            user.IsPurged = true;
            user.PurgedAt = DateTime.Now;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors.Select(e => e.Description));
            }

            return Ok("user-purged");
        }

        [HttpGet("audit-log")]
        public async Task<ActionResult<PagerWithCountResponseDto<AuditLogEntryDto>>> GetAuditLog(
            [FromQuery] AuditLogFilterRequestDto filter)
        {
            var query = _context.AuditLogs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.EntityName))
                query = query.Where(a => a.EntityName == filter.EntityName);

            if (!string.IsNullOrWhiteSpace(filter.EntityId))
                query = query.Where(a => a.EntityId == filter.EntityId);

            if (filter.ChangedByUserId.HasValue)
                query = query.Where(a => a.ChangedByUserId == filter.ChangedByUserId);

            if (filter.From.HasValue)
                query = query.Where(a => a.ChangedAt >= filter.From.Value);

            if (filter.To.HasValue)
                query = query.Where(a => a.ChangedAt <= filter.To.Value);

            var totalElements = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalElements / filter.PageSize);

            var entries = await query
                .OrderByDescending(a => a.ChangedAt)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(a => new AuditLogEntryDto
                {
                    Id = a.Id,
                    EntityName = a.EntityName,
                    EntityId = a.EntityId,
                    PropertyName = a.PropertyName,
                    OldValue = a.OldValue,
                    NewValue = a.NewValue,
                    ChangeType = a.ChangeType.ToString(),
                    ChangedAt = a.ChangedAt,
                    ChangedByUserId = a.ChangedByUserId,
                })
                .ToListAsync();

            return Ok(new PagerWithCountResponseDto<AuditLogEntryDto>
            {
                CurrentPage = filter.PageNumber,
                TotalPages = totalPages,
                TotalElements = totalElements,
                Elements = entries,
            });
        }
    }
}
