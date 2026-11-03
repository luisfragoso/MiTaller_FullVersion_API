using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiTaller.Attributes;
using MiTaller.Data;
using MiTaller.DTO;
using MiTaller.DTO.Workshop.Roadside;
using MiTaller.Models.Workshop;

namespace MiTaller.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WorkshopRoadsideController : ControllerBase
    {
        private const int MaxRadiusKm = 200;
        private readonly DataContext _context;

        public WorkshopRoadsideController(DataContext context)
        {
            _context = context;
        }

        [HttpGet("{workshopId}")]
        [AuthorizeEmployee("Administrador")]
        [OwnWorkshop]
        public async Task<ActionResult<RoadsideSettingsDto>> Get(Guid workshopId)
        {
            var settings = await _context.WorkshopRoadsideSettings.FindAsync(workshopId);
            return Ok(settings == null
                ? new RoadsideSettingsDto()
                : new RoadsideSettingsDto
                {
                    OffersRoadsideAssistance = settings.OffersRoadsideAssistance,
                    CoverageRadiusKm = settings.CoverageRadiusKm,
                    Available24h = settings.Available24h,
                    EmergencyPhone = settings.EmergencyPhone,
                });
        }

        [HttpPut("{workshopId}")]
        [AuthorizeEmployee("Administrador")]
        [OwnWorkshop]
        public async Task<ActionResult> Save(Guid workshopId, [FromBody] RoadsideSettingsDto model)
        {
            if (model.CoverageRadiusKm < 1 || model.CoverageRadiusKm > MaxRadiusKm)
                return BadRequest("invalid-radius");

            var phone = new string((model.EmergencyPhone ?? string.Empty).Where(char.IsDigit).ToArray());
            if (phone.Length != 0 && phone.Length != 10)
                return BadRequest("invalid-phone");

            var settings = await _context.WorkshopRoadsideSettings.FindAsync(workshopId);
            if (settings == null)
            {
                settings = new WorkshopRoadsideSettings { WorkshopId = workshopId };
                _context.WorkshopRoadsideSettings.Add(settings);
            }
            settings.OffersRoadsideAssistance = model.OffersRoadsideAssistance;
            settings.CoverageRadiusKm = model.CoverageRadiusKm;
            settings.Available24h = model.Available24h;
            settings.EmergencyPhone = phone;
            settings.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return Ok("roadside-settings-saved");
        }

        /// <summary>Workshops that offer roadside assistance and whose coverage reaches the customer, nearest first.
        /// Anonymous on purpose: someone stuck on the road may not have an account yet.</summary>
        [HttpPost("nearby")]
        [AllowAnonymous]
        public async Task<ActionResult<List<RoadsideWorkshopDto>>> Nearby([FromBody] LatitudeLongitude position)
        {
            var offering = await _context.WorkshopRoadsideSettings
                .Where(r => r.OffersRoadsideAssistance
                            && !r.Workshop.IsDeleted
                            && r.Workshop.Latitude.HasValue
                            && r.Workshop.Longitude.HasValue)
                .Select(r => new
                {
                    r.WorkshopId,
                    r.Workshop.WorkshopName,
                    HasImage = r.Workshop.ProfileImage != null,
                    Lat = r.Workshop.Latitude!.Value,
                    Lng = r.Workshop.Longitude!.Value,
                    r.CoverageRadiusKm,
                    r.Available24h,
                    r.EmergencyPhone,
                    r.Workshop.PhoneNumber,
                })
                .ToListAsync();

            var inRange = offering
                .Select(w => new { w, Distance = DistanceKm(position.Latitude, position.Longitude, w.Lat, w.Lng) })
                .Where(x => x.Distance <= x.w.CoverageRadiusKm)
                .OrderBy(x => x.Distance)
                .Take(30)
                .ToList();

            var ids = inRange.Select(x => x.w.WorkshopId).ToList();
            var rates = await _context.Reviews
                .Where(r => ids.Contains(r.WorkshopId))
                .GroupBy(r => r.WorkshopId)
                .Select(g => new { g.Key, Avg = g.Average(r => r.Rate) })
                .ToDictionaryAsync(g => g.Key, g => (float)Math.Round(g.Avg, 1));
            var addresses = await _context.WorkshopAddresses
                .Where(a => ids.Contains(a.WorkshopId) && !a.IsDeleted)
                .Select(a => new { a.WorkshopId, Suburb = a.Suburb.Name, Town = a.Suburb.Town.Name })
                .ToListAsync();

            return Ok(inRange.Select(x =>
            {
                var address = addresses.FirstOrDefault(a => a.WorkshopId == x.w.WorkshopId);
                return new RoadsideWorkshopDto
                {
                    WorkshopId = x.w.WorkshopId,
                    WorkshopName = x.w.WorkshopName,
                    HasProfileImage = x.w.HasImage,
                    DistanceKm = Math.Round(x.Distance, 1),
                    CoverageRadiusKm = x.w.CoverageRadiusKm,
                    Available24h = x.w.Available24h,
                    // Stored as "5512345678_workshop" so a customer can reuse the same number
                    Phone = string.IsNullOrEmpty(x.w.EmergencyPhone)
                        ? (x.w.PhoneNumber ?? string.Empty).Split('_')[0]
                        : x.w.EmergencyPhone,
                    Address = address == null ? null : $"{address.Suburb}, {address.Town}",
                    ReviewAverageRate = rates.TryGetValue(x.w.WorkshopId, out var rate) ? rate : 0,
                };
            }).ToList());
        }

        private static double DistanceKm(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371;
            double ToRad(double deg) => deg * Math.PI / 180;
            var dLat = ToRad(lat2 - lat1);
            var dLon = ToRad(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        }
    }
}
