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
                var vehicleName = $"{(vehicle.BrandId == -1 ? vehicle.OtherBrand : vehicle.Brand?.Name)} {(vehicle.VehicleModelId == -1 ? vehicle.OtherVehicleModel : vehicle.VehicleModel?.Model)} ({vehicle.Plates})";
                var contacts = await _context.EmergencyContacts
                    .Where(e => e.CustomerId == owner.Id && !e.IsDeleted && e.MustBeNotified)
                    .ToListAsync();

                foreach (var contact in contacts)
                {
                    try
                    {
                        await _emailSender.SendEmailAsync(contact.Email, $"🚨 Alerta de Emergencia - {owner.FullName}",
                            AlertEmail(contact.FullName, owner.FullName, vehicleName, accident));
                        sent++;
                    }
                    catch (Exception)
                    {
                        // One bad address must not stop the others
                    }

                    await NotifyIfRegistered(contact.Email, "Accidente reportado",
                        $"Reportaron un posible accidente del vehículo {vehicleName} de {owner.FullName}.", "AccidentReported");
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
                    Title = "Reportaron un accidente de tu vehículo",
                    Content = $"Alguien reportó un posible accidente de tu {vehicleName}. Si estás bien, avísalo en la app para tranquilizar a tus contactos.",
                    Event = "AccidentOwnerAlert",
                });
                try
                {
                    await _emailSender.SendEmailAsync(owner.Email, "🚨 Reportaron un accidente de tu vehículo, ¿estás bien?",
                        OwnerEmail(owner.FullName, vehicleName, contacts.Count, accident, FalseAlarmUrl(alert.Token)));
                }
                catch (Exception)
                {
                    // The in-app notice still reaches the owner
                }
            }
            return sent;
        }

        private async Task NotifyIfRegistered(string email, string title, string content, string eventName)
        {
            var registeredUser = await _context.Customers
                .FirstOrDefaultAsync(u => u.Email == email && u.UserType == UserType.Customer && !u.IsDeleted);
            if (registeredUser == null) return;
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
            if (alert == null) return Page("Enlace no válido", "<p>Este enlace no es válido o ya expiró.</p>");
            if (alert.FalseAlarmAt != null) return Page("Ya avisamos a tus contactos", "<p>Tus contactos de emergencia ya recibieron el aviso de que fue una falsa alarma.</p>");
            return Page("¿Estás bien?", $@"<p>Reportaron un posible accidente de tu <strong>{WebUtility.HtmlEncode(alert.VehicleName)}</strong> el {alert.CreatedAt:dd/MM/yyyy} a las {alert.CreatedAt:HH:mm}.</p>
<p>Si estás bien, confirma y avisaremos a tus contactos de emergencia que fue una falsa alarma.</p>
<form method=""post""><button type=""submit"" style=""background:#2e7d32;color:#fff;border:0;border-radius:10px;padding:14px 22px;font-size:16px;font-weight:bold;cursor:pointer;"">Estoy bien, fue un error</button></form>");
        }

        [HttpPost("false-alarm/{token}")]
        [AllowAnonymous]
        public async Task<ContentResult> FalseAlarmConfirm(string token)
        {
            var alert = await _context.AccidentOwnerAlerts.Include(a => a.Accident).FirstOrDefaultAsync(a => a.Token == token);
            if (alert == null) return Page("Enlace no válido", "<p>Este enlace no es válido o ya expiró.</p>");
            await SendFalseAlarm(alert);
            return Page("Listo, qué bueno que estás bien", "<p>Avisamos a tus contactos de emergencia que fue una falsa alarma.</p>");
        }

        private async Task SendFalseAlarm(AccidentOwnerAlert alert)
        {
            if (alert.FalseAlarmAt != null) return;
            alert.FalseAlarmAt = DateTime.Now;

            var owner = await _context.Customers.FirstAsync(c => c.Id == alert.OwnerId);
            var contacts = await _context.EmergencyContacts
                .Where(e => e.CustomerId == owner.Id && !e.IsDeleted && e.MustBeNotified)
                .ToListAsync();
            foreach (var contact in contacts)
            {
                try
                {
                    await _emailSender.SendEmailAsync(contact.Email, $"✅ Falsa alarma - {owner.FullName} está bien",
                        FalseAlarmEmail(contact.FullName, owner.FullName, alert));
                }
                catch (Exception)
                {
                    // One bad address must not stop the others
                }
                await NotifyIfRegistered(contact.Email, "Falsa alarma",
                    $"{owner.FullName} confirmó que está bien: el accidente reportado de su {alert.VehicleName} fue un error.", "AccidentFalseAlarm");
            }
            await _context.SaveChangesAsync();
        }

        private static ContentResult Page(string title, string body) => new()
        {
            ContentType = "text/html; charset=utf-8",
            Content = $@"<!DOCTYPE html>
<html lang=""es""><head><meta charset=""UTF-8"" /><meta name=""viewport"" content=""width=device-width, initial-scale=1"" /><title>Mi Taller</title></head>
<body style=""margin:0;font-family:Arial,sans-serif;background:#f5f5f5;"">
  <div style=""background:#f52222;padding:18px 0;text-align:center;color:#fff;font-size:22px;font-weight:bold;"">Mi Taller</div>
  <div style=""max-width:480px;margin:24px auto;background:#fff;border-radius:14px;padding:24px;color:#333;font-size:16px;line-height:1.5;"">
    <h2 style=""margin-top:0;"">{WebUtility.HtmlEncode(title)}</h2>
    {body}
  </div>
</body></html>",
        };

        private static string E(string? s) => WebUtility.HtmlEncode(s ?? string.Empty);

        private static string ReportDetails(Accident a)
        {
            var location = a.Latitude != null && a.Longitude != null
                ? $@"📍 <strong>Ubicación del reporte:</strong> <a href=""https://www.google.com/maps?q={E(a.Latitude)},{E(a.Longitude)}"" target=""_blank"" style=""color:#1a73e8;font-weight:bold;"">Ver en Google Maps</a><br />"
                : "";
            return $@"<p style=""line-height:1.8;"">{location}🕓 <strong>Fecha y hora del reporte:</strong> {a.CreatedAt:dd/MM/yyyy - HH:mm}</p>";
        }

        private static string AlertEmail(string contactName, string ownerName, string vehicleName, Accident a) => EmailShell($@"
    <p><strong>Estimado(a) {E(contactName)},</strong></p>
    <p>Eres el contacto de emergencia registrado por <strong>{E(ownerName)}</strong>. Una persona reportó desde la app Mi Taller un posible accidente de su vehículo <strong>{E(vehicleName)}</strong>.</p>
    {ReportDetails(a)}
    <p>Este mensaje no necesariamente indica una situación grave, pero te recomendamos contactar a <strong>{E(ownerName)}</strong> para confirmar su estado.</p>");

        private static string OwnerEmail(string ownerName, string vehicleName, int contacts, Accident a, string falseAlarmUrl) => EmailShell($@"
    <p><strong>Hola {E(ownerName)},</strong></p>
    <p>Una persona reportó desde la app Mi Taller un posible accidente de tu vehículo <strong>{E(vehicleName)}</strong>.</p>
    {ReportDetails(a)}
    <p>{(contacts == 0
        ? "No tienes contactos de emergencia que reciban avisos. Puedes agregarlos en la app, en tu perfil."
        : contacts == 1 ? "Ya avisamos a tu contacto de emergencia." : $"Ya avisamos a tus {contacts} contactos de emergencia.")}</p>
    {(contacts == 0 ? "" : $@"<p>Si estás bien y fue un error, avísales:</p>
    <p><a href=""{E(falseAlarmUrl)}"" target=""_blank"" style=""display:inline-block;background:#2e7d32;color:#fff;text-decoration:none;border-radius:10px;padding:14px 22px;font-weight:bold;"">Estoy bien, fue un error</a></p>")}");

        private static string FalseAlarmEmail(string contactName, string ownerName, AccidentOwnerAlert alert) => EmailShell($@"
    <p><strong>Estimado(a) {E(contactName)},</strong></p>
    <p><strong>{E(ownerName)}</strong> confirmó que está bien: el posible accidente de su vehículo <strong>{E(alert.VehicleName)}</strong> que te avisamos el {alert.CreatedAt:dd/MM/yyyy} a las {alert.CreatedAt:HH:mm} fue una falsa alarma.</p>
    <p>Gracias por estar al pendiente.</p>");

        private static string EmailShell(string body) => $@"<!DOCTYPE html>
<html lang=""es"">
<head><meta charset=""UTF-8"" /><title>Mi Taller</title></head>
<body style=""margin:0;font-family:Arial,sans-serif;background-color:#ffffff;"">
  <div style=""background-color:#f52222;padding:20px 0;text-align:center;"">
    <h1 style=""margin:0;color:white;""><span style=""font-weight:bold;"">MiTaller</span> <span style=""color:black;"">Financiero</span></h1>
  </div>
  <div style=""padding:30px 40px;color:#333;font-size:16px;"">{body}
    <p>MiTaller Financiero – Conectando movilidad y seguridad</p>
  </div>
  <div style=""padding:20px 30px;font-size:12px;color:#555;"">
    <p><strong>AVISO DE CONFIDENCIALIDAD.</strong> Este correo y la información contenida o adjunta al mismo es privada y confidencial y va dirigida exclusivamente a su destinatario. Mi Taller Financiero informa a quien pueda haber recibido este correo por error que contiene información confidencial cuyo uso, copia, reproducción o distribución está expresamente prohibida. Si no eres el destinatario del mismo y recibes este correo por error, te pedimos pongas en conocimiento al emisor y procedas a la eliminación sin copiarlo, imprimirlo o utilizarlo de ningún modo.</p>
    <p><strong>CONFIDENTIALITY WARNING.</strong> This message and the information contained in or attached to it are private and confidential and intended exclusively for the addressee. If you are not an intended recipient of this E-mail, please notify the sender, delete it and do not read, act upon, print, disclose, copy, retain or redistribute any portion of this E-mail.</p>
  </div>
</body>
</html>";
    }
}
