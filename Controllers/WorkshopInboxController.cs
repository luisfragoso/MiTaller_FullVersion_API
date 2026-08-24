using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiTaller.Data;
using MiTaller.DTO.Pager;
using MiTaller.DTO.Vehicle;
using MiTaller.DTO.Workshop.Inbox;
using MiTaller.DTO.Workshop.Note;
using MiTaller.Models.Auth;
using MiTaller.Models.Notification;
using MiTaller.Models.Workshop;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiTaller.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WorkshopInboxController : ControllerBase
    {
        private readonly DataContext _context;

        public WorkshopInboxController(DataContext context)
        {
            _context = context;
        }

        [HttpGet("{workshopId}")]
        public async Task<ActionResult<ICollection<WorkshopInboxResponseDto>>> GetWorkshopInbox(Guid workshopId)
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

                var workshopInboxes = await _context.WorkshopInbox
                    .Where(w => w.WorkshopId == workshopId)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.Brand)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.VehicleModel)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.VehicleVersion)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.VehicleType)
                    .Include(w => w.Customer)
                    .OrderByDescending(w => w.CreatedAt)
                    .Select(w => new WorkshopInboxResponseDto
                    {
                        Id = w.Id,
                        CustomerName = w.Customer.FullName,
                        CustomerPhoneNumber = w.Customer.NormalizedPhoneNumber,
                        CustomerEmail = w.Customer.Email,
                        Vehicle = new VehicleResponseDto
                        {
                            Id = w.Vehicle.Id,
                            Brand = w.Vehicle.BrandId == -1 ? w.Vehicle.OtherBrand : w.Vehicle.Brand.Name,
                            Model = w.Vehicle.VehicleModelId == -1 ? w.Vehicle.OtherVehicleModel : w.Vehicle.VehicleModel.Model,
                            Version = w.Vehicle.VehicleVersionId == -1 ? w.Vehicle.OtherVehicleVersion : w.Vehicle.VehicleVersion.Version,
                            Type = w.Vehicle.VehicleTypeId == -1 ? w.Vehicle.OtherVehicleType : w.Vehicle.VehicleType.Type,
                            Year = w.Vehicle.Year,
                            SerialNumber = w.Vehicle.SerialNumber,
                            Color = w.Vehicle.Color,
                            Plates = w.Vehicle.Plates,
                            RimRubber = w.Vehicle.RimRubber,
                            Kms = w.Vehicle.Kms,
                            VehicleFormat = w.Vehicle.VehicleFormat,
                            Image = w.Vehicle.Image,
                        },
                        ParentModelType = w.ParentModelType,
                        ParentModelId = w.ParentModelId,
                        Title = w.Title,
                        Details = w.Details,
                        IsRead = w.IsRead,
                        CreatedAt = w.CreatedAt,
                        UpdatedAt = w.UpdatedAt,
                    })
                    .ToListAsync();

                if (workshopInboxes == null)
                {
                    return NotFound("not-found");
                }

                return Ok(workshopInboxes);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPost("{workshopId}/inbox-pager")]
        public async Task<ActionResult<PagerResponseDto<WorkshopInboxResponseDto>>> GetWorkshopInboxPaged(Guid workshopId, [FromBody] PagerBodyDto pager)
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

                var query = _context.WorkshopInbox
                    .Where(w => w.WorkshopId == workshopId)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.Brand)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.VehicleModel)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.VehicleVersion)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.VehicleType)
                    .Include(w => w.Customer)
                    .OrderByDescending(w => w.CreatedAt);

                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalCount / pager.PageSize);

                var pagedInboxes = await query
                    .Skip((pager.PageNumber - 1) * pager.PageSize)
                    .Take(pager.PageSize)
                    .Select(w => new WorkshopInboxResponseDto
                    {
                        Id = w.Id,
                        CustomerName = w.Customer.FullName,
                        CustomerPhoneNumber = w.Customer.NormalizedPhoneNumber,
                        CustomerEmail = w.Customer.Email,
                        Vehicle = new VehicleResponseDto
                        {
                            Id = w.Vehicle.Id,
                            Brand = w.Vehicle.BrandId == -1 ? w.Vehicle.OtherBrand : w.Vehicle.Brand.Name,
                            Model = w.Vehicle.VehicleModelId == -1 ? w.Vehicle.OtherVehicleModel : w.Vehicle.VehicleModel.Model,
                            Version = w.Vehicle.VehicleVersionId == -1 ? w.Vehicle.OtherVehicleVersion : w.Vehicle.VehicleVersion.Version,
                            Type = w.Vehicle.VehicleTypeId == -1 ? w.Vehicle.OtherVehicleType : w.Vehicle.VehicleType.Type,
                            Year = w.Vehicle.Year,
                            SerialNumber = w.Vehicle.SerialNumber,
                            Color = w.Vehicle.Color,
                            Plates = w.Vehicle.Plates,
                            RimRubber = w.Vehicle.RimRubber,
                            Kms = w.Vehicle.Kms,
                            VehicleFormat = w.Vehicle.VehicleFormat,
                            Image = w.Vehicle.Image,
                        },
                        ParentModelType = w.ParentModelType,
                        ParentModelId = w.ParentModelId,
                        Title = w.Title,
                        Details = w.Details,
                        IsRead = w.IsRead,
                        CreatedAt = w.CreatedAt,
                        UpdatedAt = w.UpdatedAt,
                    })
                    .ToListAsync();

                var response = new PagerResponseDto<WorkshopInboxResponseDto>
                {
                    CurrentPage = pager.PageNumber,
                    TotalPages = totalPages,
                    Elements = pagedInboxes
                };

                return Ok(response);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }


        [HttpPut("mark-one-as-read/{workshopInboxId}")]
        public async Task<ActionResult> UpdateOneWorkshopInboxAsViewed(int workshopInboxId)
        {
            try
            {
                var workshopInbox = await _context.WorkshopInbox
                    .Where(n => n.Id == workshopInboxId)
                    .FirstOrDefaultAsync();

                if (workshopInbox == null)
                {
                    return NotFound("not-found");
                }

                workshopInbox.IsRead = true;

                await _context.SaveChangesAsync();

                return Ok("workshop-inbox-updated");
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPut("mark-all-as-read/{workshopId}")]
        public async Task<ActionResult> UpdateAllWorkshopInboxAsViewed(Guid workshopId)
        {
            try
            {
                var workshopInboxes = await _context.WorkshopInbox
                    .Where(n => n.WorkshopId == workshopId)
                    .ToListAsync();

                if (workshopInboxes == null)
                {
                    return NotFound("not-found");
                }

                foreach (var workshopInbox in workshopInboxes)
                {
                    workshopInbox.IsRead = true;
                }

                await _context.SaveChangesAsync();

                return Ok("workshop-inbox-updated");
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }


        [HttpDelete("delete-one/{workshopInboxId}")]
        public async Task<ActionResult> DeleteOneWorkshopInbox(int workshopInboxId)
        {
            try
            {
                var workshopInbox = await _context.WorkshopInbox
                    .Where(n => n.Id == workshopInboxId)
                    .FirstOrDefaultAsync();

                if (workshopInbox == null)
                {
                    return NotFound("not-found");
                }

                _context.WorkshopInbox.Remove(workshopInbox);

                await _context.SaveChangesAsync();

                return Ok("workshop-inbox-deleted");
            }
            catch (Exception)
            {
                return BadRequest("unknown-error");
            }
        }

        [HttpDelete("delete-all/{workshopId}")]
        public async Task<ActionResult> DeleteOneWorkshopInbox(Guid workshopId)
        {
            try
            {
                var workshopInboxes = await _context.WorkshopInbox
                    .Where(n => n.WorkshopId == workshopId)
                    .ToListAsync();

                if (workshopInboxes == null)
                {
                    return NotFound("not-found");
                }

                _context.WorkshopInbox.RemoveRange(workshopInboxes);

                await _context.SaveChangesAsync();

                return Ok("workshop-inbox-deleted");
            }
            catch (Exception)
            {
                return BadRequest("unknown-error");
            }
        }
    }
}
