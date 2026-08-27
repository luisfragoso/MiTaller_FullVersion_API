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
            [FromQuery] string? query, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
        {
            var customersQuery = _context.Customers.AsQueryable();
            var workshopsQuery = _context.Workshops.AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                customersQuery = customersQuery.Where(c =>
                    c.FullName.Contains(query) || c.Email.Contains(query) || c.PhoneNumber.Contains(query));
                workshopsQuery = workshopsQuery.Where(w =>
                    w.WorkshopName.Contains(query) || w.AssociateFullName.Contains(query) ||
                    w.Email.Contains(query) || w.PhoneNumber.Contains(query));
            }

            var customers = await customersQuery
                .Select(c => new AdminUserListItemDto
                {
                    Id = c.Id,
                    UserType = "Customer",
                    Name = c.FullName,
                    Email = c.Email ?? string.Empty,
                    Phone = c.PhoneNumber,
                    CreatedAt = c.CreatedAt,
                    IsDeleted = c.IsDeleted,
                })
                .ToListAsync();

            var workshops = await workshopsQuery
                .Select(w => new AdminUserListItemDto
                {
                    Id = w.Id,
                    UserType = "Workshop",
                    Name = w.WorkshopName,
                    Email = w.Email ?? string.Empty,
                    Phone = w.PhoneNumber,
                    CreatedAt = w.CreatedAt,
                    IsDeleted = w.IsDeleted,
                })
                .ToListAsync();

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
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == id);
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

                return Ok(dto);
            }

            var workshop = await _context.Workshops.FirstOrDefaultAsync(w => w.Id == id);
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

                dto.LinkedCustomersCount = await _context.WorkshopCustomers
                    .CountAsync(wc => wc.WorkshopId == id);

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
