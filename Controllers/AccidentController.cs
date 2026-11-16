using System.Net;
using System.Security.Cryptography;
using System.Security.Claims;
using MiTaller.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiTaller.Data;
using MiTaller.DTO.Accident;
using MiTaller.DTO.Pager;
using MiTaller.Models.Accident;
using MiTaller.Models.Auth;
using MiTaller.Models.Notification;

using MiTaller.Services.Localization;

namespace MiTaller.Controllers
{
    /// <summary>
    /// A witness types the plates of a crashed vehicle; the owners' emergency contacts get an email with the
    /// reporter's location. The reporter never learns whether the plates are registered or whose they are.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AccidentController : ControllerBase
    {
        private const int MaxReportsPerHour = 10;
        private static readonly TimeSpan RenotifyAfter = TimeSpan.FromMinutes(30);
        private static readonly TimeSpan PreviousWindow = TimeSpan.FromHours(24);
        private static readonly TimeSpan AlertsWindow = TimeSpan.FromDays(7);

        private readonly DataContext _context;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;

        public AccidentController(DataContext dataContext, IEmailSender emailSender, IConfiguration configuration)
        {
            _context = dataContext;
            _emailSender = emailSender;
            _configuration = configuration;
        }

        private Guid? CallerCustomerId() =>
            Enum.TryParse<UserType>(User.FindFirst("UserType")?.Value, out var type) && type == UserType.Customer
            && Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

        // Letters and digits only, so "N/A" (stored on bikes without plates) can never match anything
        private static string Normalize(string? plates) =>
            new string((plates ?? string.Empty).Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();

        private static AccidentResponseDto ToDto(Accident a, DateTime? previous = null) => new()
        {
            Id = a.Id,
            Plates = a.Plates,
            CreatedAt = a.CreatedAt,
            Latitude = a.Latitude,
            Longitude = a.Longitude,
            PreviousReportAt = previous,
        };

        private async Task<PagerResponseDto<AccidentResponseDto>> Page(IQueryable<Accident> query, PagerBodyDto pager)
        {
            var pageSize = Math.Clamp(pager.PageSize, 1, 50);
            var pageNumber = Math.Max(pager.PageNumber, 1);
            var total = await query.CountAsync();
            var items = await query.OrderByDescending(a => a.CreatedAt)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PagerResponseDto<AccidentResponseDto>
            {
                CurrentPage = pageNumber,
                TotalPages = (int)Math.Ceiling((double)total / pageSize),
                Elements = items.Select(a => ToDto(a)).ToList(),
            };
        }

        /// <summary>Everyone's reports, newest first (main tab in the app).</summary>
        [HttpPost("feed")]
        public async Task<ActionResult<PagerResponseDto<AccidentResponseDto>>> GetFeed([FromBody] PagerBodyDto pager) =>
            Ok(await Page(_context.Accidents, pager));

        /// <summary>The caller's own reports.</summary>
        [HttpPost("pager")]
        public async Task<ActionResult<PagerResponseDto<AccidentResponseDto>>> GetAccidentsPaged([FromBody] PagerBodyDto pager)
        {
            if (CallerCustomerId() is not Guid customerId) return StatusCode(StatusCodes.Status403Forbidden, "customer-only");
            return Ok(await Page(_context.Accidents.Where(a => a.CustomerId == customerId), pager));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AccidentResponseDto>>> GetAccidents()
        {
            if (CallerCustomerId() is not Guid customerId) return StatusCode(StatusCodes.Status403Forbidden, "customer-only");
            var accidents = await _context.Accidents
                .Where(a => a.CustomerId == customerId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
            return Ok(accidents.Select(a => ToDto(a)));
        }

        /// <summary>Last report of these plates in the previous 24 h, so the app can warn before sending.</summary>
        [HttpGet("check/{plates}")]
        public async Task<ActionResult> CheckPlates(string plates)
        {
            var normalized = Normalize(plates);
            var since = DateTime.Now - PreviousWindow;
            var last = await _context.Accidents
                .Where(a => a.Plates == normalized && a.CreatedAt > since)
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => (DateTime?)a.CreatedAt)
                .FirstOrDefaultAsync();
            return Ok(new { plates = normalized, previousReportAt = last });
        }

        [HttpPost("register")]
        [CallerMatches("CustomerId")]
        public async Task<ActionResult> CreateAccident(PostAccidentDto model)
        {
            var plates = Normalize(model.Plates);
            if (plates.Length < 3 || plates.Length > 12) return BadRequest("invalid-plates");

            var now = DateTime.Now;
            var reporterRecent = _context.Accidents.Where(a => a.CustomerId == model.CustomerId && a.CreatedAt > now.AddHours(-1));
            if (await reporterRecent.CountAsync() >= MaxReportsPerHour)
                return StatusCode(StatusCodes.Status429TooManyRequests, "too-many-reports");
            if (await reporterRecent.AnyAsync(a => a.Plates == plates && a.CreatedAt > now - RenotifyAfter))
                return BadRequest("accident-already-reported");

            var previous = await _context.Accidents
                .Where(a => a.Plates == plates && a.CreatedAt > now - PreviousWindow)
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => (DateTime?)a.CreatedAt)
                .FirstOrDefaultAsync();

            var accident = new Accident
            {
                CustomerId = model.CustomerId,
                Plates = plates,
                CreatedAt = now,
                Latitude = model.Latitude,
                Longitude = model.Longitude,
            };
            await _context.Accidents.AddAsync(accident);
            await _context.SaveChangesAsync();

            // Several witnesses reporting the same crash: the contacts get one email, not one per witness
            if (previous == null || previous < now - RenotifyAfter)
            {
                accident.NotifiedContacts = await NotifyOwnersContacts(accident);
                await _context.SaveChangesAsync();
            }

            return Ok(ToDto(accident, previous));
        }

        private async Task<int> NotifyOwnersContacts(Accident accident)
        {
            var vehicles = await _context.Vehicles
                .Where(v => !v.IsDeleted && (v.EmergencyCode == accident.Plates
                    || v.Plates.Replace("-", "").Replace(" ", "").ToUpper() == accident.Plates))
                .Include(v => v.Brand).Include(v => v.VehicleModel).Include(v => v.Customer)
                .ToListAsync();

            var sent = 0;
            foreach (var vehicle in vehicles.Where(v => v.Customer != null && !v.Customer.IsDeleted).DistinctBy(v => v.CustomerId))
            {
                var owner = vehicle.Customer;
                // Contacts have no account: everything about this owner goes out in the owner's language
                var lang = Lang.Of(owner);
                // Bikes carry "N/A" plates: identify them by their emergency code instead
                var identifier = vehicle.Plates == "N/A" && vehicle.EmergencyCode is { Length: > 2 } code
                    ? T.Get(lang, "accident.code", $"{code[..2]}-{code[2..]}")
                    : vehicle.Plates;
                var vehicleName = $"{(vehicle.BrandId == -1 ? vehicle.OtherBrand : vehicle.Brand?.Name)} {(vehicle.VehicleModelId == -1 ? vehicle.OtherVehicleModel : vehicle.VehicleModel?.Model)} ({identifier})";
                var contacts = await _context.EmergencyContacts
                    .Where(e => e.CustomerId == owner.Id && !e.IsDeleted && e.MustBeNotified)
                    .ToListAsync();

                foreach (var contact in contacts)
                {
                    try
                    {
                        await _emailSender.SendEmailAsync(contact.Email, T.Get(lang, "accident.contact.subject", owner.FullName),
                            AlertEmail(lang, contact.FullName, owner.FullName, vehicleName, accident));
                        sent++;
                    }
                    catch (Exception)
                    {
                        // One bad address must not stop the others
                    }

                    await NotifyIfRegistered(contact.Email, l => (T.Get(l, "accident.contact.notif.title"),
                        T.Get(l, "accident.contact.notif.content", vehicleName, owner.FullName)), "AccidentReported");
                }

                // Contacts are alerted right away (the owner may be unconscious); the owner can then call it off
                var alert = new AccidentOwnerAlert
                {
                    AccidentId = accident.Id,
                    OwnerId = owner.Id,
                    VehicleName = vehicleName.Length > 200 ? vehicleName[..200] : vehicleName,
                    Token = RandomNumberGenerator.GetHexString(48, lowercase: true),
                    CreatedAt = accident.CreatedAt,
                };
                await _context.AccidentOwnerAlerts.AddAsync(alert);
                await _context.Notifications.AddAsync(new Notifications
                {
                    UserId = owner.Id,
                    UserType = UserType.Customer,
                    Title = T.Get(lang, "accident.owner.notif.title"),
                    Content = T.Get(lang, "accident.owner.notif.content", vehicleName),
                    Event = "AccidentOwnerAlert",
                });
                try
                {
                    await _emailSender.SendEmailAsync(owner.Email, T.Get(lang, "accident.owner.subject"),
                        OwnerEmail(lang, owner.FullName, vehicleName, contacts.Count, accident, FalseAlarmUrl(alert.Token)));
                }
                catch (Exception)
                {
                    // The in-app notice still reaches the owner
                }
            }
            return sent;
        }

        // A contact who also has a customer account gets the notice in their own language
        private async Task NotifyIfRegistered(string email, Func<string, (string Title, string Content)> build, string eventName)
        {
            var registeredUser = await _context.Customers
                .FirstOrDefaultAsync(u => u.Email == email && u.UserType == UserType.Customer && !u.IsDeleted);
            if (registeredUser == null) return;
            var (title, content) = build(Lang.Of(registeredUser));
            await _context.Notifications.AddAsync(new Notifications
            {
                UserId = registeredUser.Id,
                UserType = UserType.Customer,
                Title = title,
                Content = content,
                Event = eventName,
            });
        }

        private string FalseAlarmUrl(string token)
        {
            var baseUrl = _configuration["PublicApiUrl"] ?? $"{Request.Scheme}://{Request.Host}";
            return $"{baseUrl.TrimEnd('/')}/api/Accident/false-alarm/{token}";
        }

        private static AccidentOwnerAlertDto ToDto(AccidentOwnerAlert a) => new()
        {
            Id = a.Id,
            VehicleName = a.VehicleName,
            CreatedAt = a.CreatedAt,
            Latitude = a.Accident?.Latitude,
            Longitude = a.Accident?.Longitude,
            FalseAlarmAt = a.FalseAlarmAt,
        };

        /// <summary>Reports of the caller's own vehicles in the last 7 days.</summary>
        [HttpGet("alerts")]
        public async Task<ActionResult<IEnumerable<AccidentOwnerAlertDto>>> GetOwnerAlerts()
        {
            if (CallerCustomerId() is not Guid customerId) return StatusCode(StatusCodes.Status403Forbidden, "customer-only");
            var since = DateTime.Now - AlertsWindow;
            var alerts = await _context.AccidentOwnerAlerts.Include(a => a.Accident)
                .Where(a => a.OwnerId == customerId && a.CreatedAt > since)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
            return Ok(alerts.Select(ToDto));
        }

        [HttpPost("alerts/{id:int}/false-alarm")]
        public async Task<ActionResult<AccidentOwnerAlertDto>> MarkFalseAlarm(int id)
        {
            if (CallerCustomerId() is not Guid customerId) return StatusCode(StatusCodes.Status403Forbidden, "customer-only");
            var alert = await _context.AccidentOwnerAlerts.Include(a => a.Accident)
                .FirstOrDefaultAsync(a => a.Id == id && a.OwnerId == customerId);
            if (alert == null) return NotFound("alert-not-found");
            await SendFalseAlarm(alert);
            return Ok(ToDto(alert));
        }

        // The email link only shows a confirmation page: mail scanners open links on their own,
        // and a GET that called off the alert could silence a real accident.
        [HttpGet("false-alarm/{token}")]
        [AllowAnonymous]
        public async Task<ContentResult> FalseAlarmPage(string token)
        {
            var alert = await _context.AccidentOwnerAlerts.FirstOrDefaultAsync(a => a.Token == token);
            if (alert == null) return InvalidLinkPage(Lang.FromRequest(Request));
            var lang = await _context.LanguageOfAsync(alert.OwnerId);
            if (alert.FalseAlarmAt != null)
                return Page(lang, T.Get(lang, "accident.page.already.title"), $"<p>{T.Get(lang, "accident.page.already.body")}</p>");
            return Page(lang, T.Get(lang, "accident.page.ask.title"), $@"<p>{T.Get(lang, "accident.page.ask.bodyHtml", E(alert.VehicleName), alert.CreatedAt.LongDate(lang), alert.CreatedAt.ShortTime(lang))}</p>
<p>{T.Get(lang, "accident.page.ask.confirm")}</p>
<form method=""post""><button type=""submit"" style=""background:#2e7d32;color:#fff;border:0;border-radius:10px;padding:14px 22px;font-size:16px;font-weight:bold;cursor:pointer;"">{T.Get(lang, "accident.imOk")}</button></form>");
        }

        [HttpPost("false-alarm/{token}")]
        [AllowAnonymous]
        public async Task<ContentResult> FalseAlarmConfirm(string token)
        {
            var alert = await _context.AccidentOwnerAlerts.Include(a => a.Accident).FirstOrDefaultAsync(a => a.Token == token);
            if (alert == null) return InvalidLinkPage(Lang.FromRequest(Request));
            await SendFalseAlarm(alert);
            var lang = await _context.LanguageOfAsync(alert.OwnerId);
            return Page(lang, T.Get(lang, "accident.page.done.title"), $"<p>{T.Get(lang, "accident.page.done.body")}</p>");
        }

        private async Task SendFalseAlarm(AccidentOwnerAlert alert)
        {
            if (alert.FalseAlarmAt != null) return;
            alert.FalseAlarmAt = DateTime.Now;

            var owner = await _context.Customers.FirstAsync(c => c.Id == alert.OwnerId);
            var lang = Lang.Of(owner);
            var contacts = await _context.EmergencyContacts
                .Where(e => e.CustomerId == owner.Id && !e.IsDeleted && e.MustBeNotified)
                .ToListAsync();
            foreach (var contact in contacts)
            {
                try
                {
                    await _emailSender.SendEmailAsync(contact.Email, T.Get(lang, "accident.falseAlarm.subject", owner.FullName),
                        FalseAlarmEmail(lang, contact.FullName, owner.FullName, alert));
                }
                catch (Exception)
                {
                    // One bad address must not stop the others
                }
                await NotifyIfRegistered(contact.Email, l => (T.Get(l, "accident.falseAlarm.notif.title"),
                    T.Get(l, "accident.falseAlarm.notif.content", owner.FullName, alert.VehicleName)), "AccidentFalseAlarm");
            }
            await _context.SaveChangesAsync();
        }

        private static ContentResult InvalidLinkPage(string lang) =>
            Page(lang, T.Get(lang, "accident.page.invalid.title"), $"<p>{T.Get(lang, "accident.page.invalid.body")}</p>");

        private static ContentResult Page(string lang, string title, string body) => new()
        {
            ContentType = "text/html; charset=utf-8",
            Content = $@"<!DOCTYPE html>
<html lang=""{lang}""><head><meta charset=""UTF-8"" /><meta name=""viewport"" content=""width=device-width, initial-scale=1"" /><title>MiTaller</title></head>
<body style=""margin:0;font-family:Arial,sans-serif;background:#f5f5f5;"">
  <div style=""background:#f52222;padding:18px 0;text-align:center;color:#fff;font-size:22px;font-weight:bold;"">MiTaller</div>
  <div style=""max-width:480px;margin:24px auto;background:#fff;border-radius:14px;padding:24px;color:#333;font-size:16px;line-height:1.5;"">
    <h2 style=""margin-top:0;"">{E(title)}</h2>
    {body}
  </div>
</body></html>",
        };

        private static string E(string? s) => EmailLayout.E(s);

        private static string ReportDetails(string lang, Accident a)
        {
            var location = a.Latitude != null && a.Longitude != null
                ? $@"{T.Get(lang, "accident.locationHtml")} <a href=""https://www.google.com/maps?q={E(a.Latitude)},{E(a.Longitude)}"" target=""_blank"" style=""color:#1a73e8;font-weight:bold;"">{T.Get(lang, "accident.viewMap")}</a><br />"
                : "";
            return $@"<p style=""line-height:1.8;"">{location}{T.Get(lang, "accident.whenHtml")} {a.CreatedAt.LongDate(lang)} - {a.CreatedAt.ShortTime(lang)}</p>";
        }

        private static string AlertEmail(string lang, string contactName, string ownerName, string vehicleName, Accident a) => EmailLayout.Shell(lang, $@"
    <p><strong>{E(T.Get(lang, "accident.dear", contactName))}</strong></p>
    <p>{T.Get(lang, "accident.contact.introHtml", E(ownerName), E(vehicleName))}</p>
    {ReportDetails(lang, a)}
    <p>{T.Get(lang, "accident.contact.adviceHtml", E(ownerName))}</p>", tagline: true);

        private static string OwnerEmail(string lang, string ownerName, string vehicleName, int contacts, Accident a, string falseAlarmUrl) => EmailLayout.Shell(lang, $@"
    <p><strong>{E(T.Get(lang, "accident.hello", ownerName))}</strong></p>
    <p>{T.Get(lang, "accident.owner.introHtml", E(vehicleName))}</p>
    {ReportDetails(lang, a)}
    <p>{(contacts == 0 ? T.Get(lang, "accident.owner.noContacts")
        : contacts == 1 ? T.Get(lang, "accident.owner.oneContact") : T.Get(lang, "accident.owner.manyContacts", contacts))}</p>
    {(contacts == 0 ? "" : $@"<p>{T.Get(lang, "accident.owner.ifOk")}</p>
    <p><a href=""{E(falseAlarmUrl)}"" target=""_blank"" style=""display:inline-block;background:#2e7d32;color:#fff;text-decoration:none;border-radius:10px;padding:14px 22px;font-weight:bold;"">{T.Get(lang, "accident.imOk")}</a></p>")}", tagline: true);

        private static string FalseAlarmEmail(string lang, string contactName, string ownerName, AccidentOwnerAlert alert) => EmailLayout.Shell(lang, $@"
    <p><strong>{E(T.Get(lang, "accident.dear", contactName))}</strong></p>
    <p>{T.Get(lang, "accident.falseAlarm.bodyHtml", E(ownerName), E(alert.VehicleName), alert.CreatedAt.LongDate(lang), alert.CreatedAt.ShortTime(lang))}</p>
    <p>{T.Get(lang, "accident.falseAlarm.thanks")}</p>", tagline: true);
    }
}
