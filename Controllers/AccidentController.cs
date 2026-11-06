using System.Net;
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

        private readonly DataContext _context;
        private readonly IEmailSender _emailSender;

        public AccidentController(DataContext dataContext, IEmailSender emailSender)
        {
            _context = dataContext;
            _emailSender = emailSender;
        }

        private Guid? CallerCustomerId() =>
            Enum.TryParse<UserType>(User.FindFirst("UserType")?.Value, out var type) && type == UserType.Customer
            && Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

        private static string Normalize(string? plates) =>
            (plates ?? string.Empty).Replace("-", "").Replace(" ", "").Trim().ToUpperInvariant();

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
                .Where(v => !v.IsDeleted && v.Plates.Replace("-", "").Replace(" ", "").ToUpper() == accident.Plates)
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

                    var registeredUser = await _context.Customers
                        .FirstOrDefaultAsync(u => u.Email == contact.Email && u.UserType == UserType.Customer && !u.IsDeleted);
                    if (registeredUser != null)
                    {
                        await _context.Notifications.AddAsync(new Notifications
                        {
                            UserId = registeredUser.Id,
                            UserType = UserType.Customer,
                            Title = "Accidente reportado",
                            Content = $"Reportaron un posible accidente del vehículo {vehicleName} de {owner.FullName}.",
                            Event = "AccidentReported",
                        });
                    }
                }
            }
            return sent;
        }

        private static string AlertEmail(string contactName, string ownerName, string vehicleName, Accident a)
        {
            static string E(string? s) => WebUtility.HtmlEncode(s ?? string.Empty);
            var location = a.Latitude != null && a.Longitude != null
                ? $@"📍 <strong>Ubicación del reporte:</strong> <a href=""https://www.google.com/maps?q={E(a.Latitude)},{E(a.Longitude)}"" target=""_blank"" style=""color:#1a73e8;font-weight:bold;"">Ver en Google Maps</a><br />"
                : "";

            return $@"<!DOCTYPE html>
<html lang=""es"">
<head><meta charset=""UTF-8"" /><title>Alerta de Emergencia</title></head>
<body style=""margin:0;font-family:Arial,sans-serif;background-color:#ffffff;"">
  <div style=""background-color:#f52222;padding:20px 0;text-align:center;"">
    <h1 style=""margin:0;color:white;""><span style=""font-weight:bold;"">MiTaller</span> <span style=""color:black;"">Financiero</span></h1>
  </div>
  <div style=""padding:30px 40px;color:#333;font-size:16px;"">
    <p><strong>Estimado(a) {E(contactName)},</strong></p>
    <p>Eres el contacto de emergencia registrado por <strong>{E(ownerName)}</strong>. Una persona reportó desde la app Mi Taller un posible accidente de su vehículo <strong>{E(vehicleName)}</strong>.</p>
    <p style=""line-height:1.8;"">{location}🕓 <strong>Fecha y hora del reporte:</strong> {a.CreatedAt:dd/MM/yyyy - HH:mm}</p>
    <p>Este mensaje no necesariamente indica una situación grave, pero te recomendamos contactar a <strong>{E(ownerName)}</strong> para confirmar su estado.</p>
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
}
