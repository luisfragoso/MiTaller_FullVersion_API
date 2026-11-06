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
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AccidentController : ControllerBase
    {
        private static readonly Dictionary<string, string> KindLabels = new()
        {
            ["collision"] = "Choque con otro vehículo",
            ["hit-object"] = "Golpe contra un objeto",
            ["rollover"] = "Volcadura o salida del camino",
            ["pedestrian"] = "Atropello o ciclista involucrado",
            ["other"] = "Otro",
        };

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

        private static AccidentResponseDto ToDto(Accident a) => new()
        {
            Id = a.Id,
            CustomerId = a.CustomerId,
            Plates = a.Plates,
            CreatedAt = a.CreatedAt,
            VehicleId = a.VehicleId,
            VehicleName = a.VehicleName,
            Kind = a.Kind,
            HasInjured = a.HasInjured,
            Notes = a.Notes,
            Latitude = a.Latitude,
            Longitude = a.Longitude,
            NotifiedContacts = a.NotifiedContacts,
        };

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AccidentResponseDto>>> GetAccidents()
        {
            if (CallerCustomerId() is not Guid customerId) return StatusCode(StatusCodes.Status403Forbidden, "customer-only");

            var accidents = await _context.Accidents
                .Where(a => a.CustomerId == customerId)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();
            return Ok(accidents.Select(ToDto));
        }

        [HttpPost("pager")]
        public async Task<ActionResult<PagerResponseDto<AccidentResponseDto>>> GetAccidentsPaged([FromBody] PagerBodyDto pager)
        {
            if (CallerCustomerId() is not Guid customerId) return StatusCode(StatusCodes.Status403Forbidden, "customer-only");

            var pageSize = Math.Clamp(pager.PageSize, 1, 50);
            var pageNumber = Math.Max(pager.PageNumber, 1);
            var query = _context.Accidents
                .Where(a => a.CustomerId == customerId)
                .OrderByDescending(e => e.CreatedAt);

            var totalCount = await query.CountAsync();
            var paged = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return Ok(new PagerResponseDto<AccidentResponseDto>
            {
                CurrentPage = pageNumber,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                Elements = paged.Select(ToDto).ToList(),
            });
        }

        [HttpGet("{accidentId}")]
        public async Task<ActionResult<AccidentResponseDto>> GetAccidentsById(int accidentId)
        {
            if (CallerCustomerId() is not Guid customerId) return StatusCode(StatusCodes.Status403Forbidden, "customer-only");

            var accident = await _context.Accidents
                .FirstOrDefaultAsync(a => a.Id == accidentId && a.CustomerId == customerId);
            return accident == null ? NotFound("not-found") : Ok(ToDto(accident));
        }

        [HttpPost("register")]
        [CallerMatches("CustomerId")]
        public async Task<ActionResult> CreateAccident(PostAccidentDto model)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Id == model.CustomerId && !c.IsDeleted);
            if (customer == null) return NotFound("not-found");

            // Only the caller's own vehicles; typed plates of a car outside the account are kept as text
            var vehicles = _context.Vehicles
                .Where(v => v.CustomerId == model.CustomerId && !v.IsDeleted)
                .Include(v => v.Brand).Include(v => v.VehicleModel);
            var plates = model.Plates?.Trim().ToUpper() ?? string.Empty;
            var vehicle = model.VehicleId is int vehicleId
                ? await vehicles.FirstOrDefaultAsync(v => v.Id == vehicleId)
                : string.IsNullOrEmpty(plates) ? null : await vehicles.FirstOrDefaultAsync(v => v.Plates == plates);

            if (model.VehicleId != null && vehicle == null) return NotFound("not-found");
            if (vehicle != null) plates = vehicle.Plates.Trim().ToUpper();
            if (string.IsNullOrEmpty(plates)) return BadRequest("plates-required");

            // Double taps / retries on a bad connection must not email the contacts twice
            var recent = DateTime.Now.AddMinutes(-10);
            if (await _context.Accidents.AnyAsync(a => a.CustomerId == model.CustomerId && a.Plates == plates && a.CreatedAt > recent))
                return BadRequest("accident-already-reported");

            var vehicleName = vehicle == null
                ? $"con placas {plates}"
                : $"{(vehicle.BrandId == -1 ? vehicle.OtherBrand : vehicle.Brand.Name)} {(vehicle.VehicleModelId == -1 ? vehicle.OtherVehicleModel : vehicle.VehicleModel.Model)} {vehicle.Year} ({plates})".Trim();
            var kind = model.Kind != null && KindLabels.ContainsKey(model.Kind) ? model.Kind : "other";

            var accident = new Accident
            {
                CustomerId = model.CustomerId,
                Plates = plates,
                CreatedAt = DateTime.Now,
                VehicleId = vehicle?.Id,
                VehicleName = vehicleName,
                Kind = kind,
                HasInjured = model.HasInjured,
                Notes = string.IsNullOrWhiteSpace(model.Notes) ? null : model.Notes.Trim(),
                Latitude = model.Latitude,
                Longitude = model.Longitude,
            };
            await _context.Accidents.AddAsync(accident);
            await _context.SaveChangesAsync();

            var contacts = await _context.EmergencyContacts
                .Where(e => e.CustomerId == model.CustomerId && !e.IsDeleted && e.MustBeNotified)
                .ToListAsync();

            foreach (var contact in contacts)
            {
                try
                {
                    await _emailSender.SendEmailAsync(contact.Email, $"🚨 Alerta de Emergencia - {customer.FullName}",
                        AlertEmail(contact.FullName, customer.FullName, customer.PhoneNumber, accident));
                    accident.NotifiedContacts++;
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
                        Content = $"{customer.FullName} reportó un accidente con su vehículo {vehicleName}."
                            + (accident.HasInjured ? " Indicó que hay heridos." : ""),
                        Event = "AccidentReported",
                    });
                }
            }

            await _context.SaveChangesAsync();
            return Ok(ToDto(accident));
        }

        private static string AlertEmail(string contactName, string customerName, string? customerPhone, Accident a)
        {
            static string E(string? s) => WebUtility.HtmlEncode(s ?? string.Empty);
            var mapLink = a.Latitude != null && a.Longitude != null
                ? $"https://www.google.com/maps?q={E(a.Latitude)},{E(a.Longitude)}"
                : null;

            var rows = new List<string>
            {
                $"🚗 <strong>Vehículo:</strong> {E(a.VehicleName)}",
                $"⚠️ <strong>Qué pasó:</strong> {E(KindLabels[a.Kind])}",
                a.HasInjured
                    ? "🩹 <strong style=\"color:#d32f2f;\">Indicó que hay heridos.</strong>"
                    : "🩹 <strong>Heridos:</strong> indicó que no hay heridos",
                $"🕓 <strong>Fecha y hora:</strong> {a.CreatedAt:dd/MM/yyyy - HH:mm}",
            };
            if (mapLink != null) rows.Add($"📍 <strong>Ubicación:</strong> <a href=\"{mapLink}\" target=\"_blank\" style=\"color:#1a73e8;font-weight:bold;\">Ver en Google Maps</a>");
            if (a.Notes != null) rows.Add($"📝 <strong>Comentario:</strong> {E(a.Notes)}");
            if (!string.IsNullOrWhiteSpace(customerPhone)) rows.Add($"📞 <strong>Su teléfono:</strong> <a href=\"tel:{E(customerPhone)}\">{E(customerPhone)}</a>");

            return $@"<!DOCTYPE html>
<html lang=""es"">
<head><meta charset=""UTF-8"" /><title>Alerta de Emergencia</title></head>
<body style=""margin:0;font-family:Arial,sans-serif;background-color:#ffffff;"">
  <div style=""background-color:#f52222;padding:20px 0;text-align:center;"">
    <h1 style=""margin:0;color:white;""><span style=""font-weight:bold;"">MiTaller</span> <span style=""color:black;"">Financiero</span></h1>
  </div>
  <div style=""padding:30px 40px;color:#333;font-size:16px;"">
    <p><strong>Estimado(a) {E(contactName)},</strong></p>
    <p>Eres el contacto de emergencia registrado por <strong>{E(customerName)}</strong>, quien acaba de reportar un accidente desde la app Mi Taller.</p>
    <p style=""line-height:1.8;"">{string.Join("<br />", rows)}</p>
    <p>Te recomendamos contactar a <strong>{E(customerName)}</strong> para confirmar cómo está.</p>
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
