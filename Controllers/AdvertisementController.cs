using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiTaller.Data;
using MiTaller.DTO.Advertisement;
using MiTaller.Models.Advertisement;
using MiTaller.Models.Vehicle;

namespace MiTaller.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AdvertisementController : ControllerBase
    {
        private readonly DataContext _context;

        public AdvertisementController(DataContext context)
        {
            _context = context;
        }

        [HttpPost("create")]
        public async Task<ActionResult> CreateAdvertisement(PostAdvertisementDto model)
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

                var advertisement = new Advertisement
                {
                    WorkshopId = workshop.Id,
                    Title = model.Title,
                    Description = model.Description,
                    Url = model.Url,
                    Type = model.Type,
                    StartsAt = model.StartsAt,
                    EndsAt = model.EndsAt,
                    CreatedAt = DateTime.Now
                };

                if (model.Image != null)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await model.Image.CopyToAsync(memoryStream);
                        advertisement.Image = memoryStream.ToArray();
                    }
                }

                await _context.Advertisements.AddAsync(advertisement);
                await _context.SaveChangesAsync();

                return Ok("advertisement-created");
            }
            catch (Exception)
            {
                return BadRequest("unknown-error");
            }
        }

        [HttpGet("get-advertisements")]
        public async Task<ActionResult<ICollection<AdvertisementResponseDto>>> GetAdvertisements()
        {
            try
            {
                var currentDate = DateTime.Now;

                var advertisementsDto = new List<AdvertisementResponseDto>();
                var advertisements = await _context.Advertisements
                    .Where(e => e.StartsAt <= currentDate && e.EndsAt >= currentDate)
                    .OrderByDescending(e => e.CreatedAt)
                    .Take(10)
                    .ToListAsync();

                if (advertisements == null || !advertisements.Any())
                {
                    return NotFound("not-found");
                }

                foreach (var advertisement in advertisements)
                {
                    var adDto = new AdvertisementResponseDto
                    {
                        Id = advertisement.Id,
                        Title = advertisement.Title,
                        Description = advertisement.Description,
                        Image = advertisement.Image,
                        Url = advertisement.Url,
                        Type = advertisement.Type,
                        StartsAt = advertisement.StartsAt,
                        EndsAt = advertisement.EndsAt
                    };
                    advertisementsDto.Add(adDto);
                }

                return Ok(advertisementsDto);
            }
            catch (Exception)
            {
                return BadRequest("unknown-error");
            }
        }

        [HttpGet("get-advertisements-by/{workshopId}")]
        public async Task<ActionResult<ICollection<AdvertisementResponseDto>>> GetAdvertisementsByWorkshop(Guid workshopId)
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

                var currentDate = DateTime.Now;

                var advertisementsDto = new List<AdvertisementResponseDto>();
                var advertisements = await _context.Advertisements
                    .Where(e => e.WorkshopId == workshopId && e.StartsAt <= currentDate && e.EndsAt >= currentDate)
                    .ToListAsync();

                if (advertisements == null || !advertisements.Any())
                {
                    return NotFound("not-found");
                }

                foreach (var advertisement in advertisements)
                {
                    var adDto = new AdvertisementResponseDto
                    {
                        Id = advertisement.Id,
                        Title = advertisement.Title,
                        Description = advertisement.Description,
                        Image = advertisement.Image,
                        Url = advertisement.Url,
                        Type = advertisement.Type,
                        StartsAt = advertisement.StartsAt,
                        EndsAt = advertisement.EndsAt
                    };
                    advertisementsDto.Add(adDto);
                }

                return Ok(advertisementsDto);
            }
            catch (Exception)
            {
                return BadRequest("unknown-error");
            }
        }

        [HttpPut("advertisement/{advertisementId}")]
        public async Task<ActionResult> UpdateAdvertisement(int advertisementId, PostAdvertisementDto model)
        {
            try
            {
                var advertisement = await _context.Advertisements
                    .Where(a => a.Id == advertisementId)
                    .FirstOrDefaultAsync();

                if (advertisement == null)
                {
                    return NotFound("not-found");
                }

                advertisement.Title = model.Title;
                advertisement.Description = model.Description;
                advertisement.Url = model.Url;
                advertisement.Type = model.Type;
                advertisement.StartsAt = model.StartsAt;
                advertisement.EndsAt = model.EndsAt;

                if (model.Image != null)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await model.Image.CopyToAsync(memoryStream);
                        advertisement.Image = memoryStream.ToArray();
                    }
                }

                await _context.SaveChangesAsync();

                return Ok("advertisement-updated");
            }
            catch (Exception)
            {
                return BadRequest("unknown-error");
            }
        }

        [HttpDelete("advertisement/{advertisementId}")]
        public async Task<ActionResult> DeleteAdvertisement(int advertisementId)
        {
            try
            {
                var advertisement = await _context.Advertisements
                    .Where(a => a.Id == advertisementId)
                    .FirstOrDefaultAsync();

                if (advertisement == null)
                {
                    return NotFound("not-found");
                }

                _context.Advertisements.Remove(advertisement);
                await _context.SaveChangesAsync();

                return Ok("advertisement-deleted");
            }
            catch (Exception)
            {
                return BadRequest("unknown-error");
            }
        }
    }
}
