using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiTaller.Data;
using MiTaller.DTO;
using MiTaller.DTO.Accident;
using MiTaller.DTO.Address;
using MiTaller.DTO.Customer;
using MiTaller.DTO.Pager;
using MiTaller.Models.Accident;
using MiTaller.Models.Address;
using MiTaller.Models.Auth;
using MiTaller.Models.Customer;
using MiTaller.Models.Notification;
using MiTaller.Models.Workshop;
using MiTaller.Services;
using System.Collections.Generic;

namespace MiTaller.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AccidentController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IEmailSender _emailSender;
        private readonly FirebaseNotificationService _firebaseNotificationService;

        public AccidentController(DataContext dataContext, UserManager<BaseIdentityUser> userManager, IEmailSender emailSender, FirebaseNotificationService firebaseNotificationService)
        {
            _context = dataContext;
            _emailSender = emailSender;
            _firebaseNotificationService = firebaseNotificationService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AccidentResponseDto>>> GetAccidents()
        {
            try
            {
                var accidents = await _context.Accidents
                    .OrderByDescending(e => e.CreatedAt)
                    .ToListAsync();

                if (accidents == null)
                {
                    return NotFound("not-found");
                }

                var accidentsDto = new List<AccidentResponseDto>();

                foreach (var accident in accidents)
                {
                    var accidentDto = new AccidentResponseDto
                    {
                        Id = accident.Id,
                        CustomerId = accident.CustomerId,
                        Plates = accident.Plates,
                        CreatedAt = accident.CreatedAt
                    };

                    accidentsDto.Add(accidentDto);
                }

                return Ok(accidentsDto);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPost("pager")]
        public async Task<ActionResult<PagerResponseDto<AccidentResponseDto>>> GetAccidentsPaged([FromBody] PagerBodyDto pager)
        {
            try
            {
                var query = _context.Accidents
                    .OrderByDescending(e => e.CreatedAt);

                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalCount / pager.PageSize);

                var pagedAccidents = await query
                    .Skip((pager.PageNumber - 1) * pager.PageSize)
                    .Take(pager.PageSize)
                    .ToListAsync();

                if (!pagedAccidents.Any())
                {
                    return NotFound("not-found");
                }

                var accidentsDto = pagedAccidents.Select(accident => new AccidentResponseDto
                {
                    Id = accident.Id,
                    CustomerId = accident.CustomerId,
                    Plates = accident.Plates,
                    CreatedAt = accident.CreatedAt
                }).ToList();

                var response = new PagerResponseDto<AccidentResponseDto>
                {
                    CurrentPage = pager.PageNumber,
                    TotalPages = totalPages,
                    Elements = accidentsDto
                };

                return Ok(response);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpGet("{accidentId}")]
        public async Task<ActionResult<AccidentResponseDto>> GetAccidentsById(int accidentId)
        {
            try
            {
                var accident = await _context.Accidents
                    .Where(a => a.Id == accidentId)
                    .FirstOrDefaultAsync();

                if (accident == null)
                {
                    return NotFound("not-found");
                }

                var accidentDto = new AccidentResponseDto
                {
                    Id = accident.Id,
                    CustomerId = accident.CustomerId,
                    Plates = accident.Plates,
                    CreatedAt = accident.CreatedAt
                };

                return Ok(accidentDto);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPost("register")]
        public async Task<ActionResult> CreateAccident(PostAccidentDto model)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(model.Plates))
                {
                    model.Plates = model.Plates.Trim().ToUpper();
                }

                var customer = await _context.Customers
                    .Where(c => c.Id == model.CustomerId && !c.IsDeleted)
                    .FirstOrDefaultAsync();

                if (customer == null)
                {
                    return NotFound("not-found");
                }

                var existingAccident = await _context.Accidents
                    .AnyAsync(a => a.Plates == model.Plates);

                if (existingAccident)
                {
                    return BadRequest("accident-already-registered");
                }

                var existingVehicle = await _context.Vehicles
                    .Where(v => v.Plates == model.Plates && !v.IsDeleted)
                    .Include(a => a.Brand)
                    .Include(v => v.VehicleModel)
                    .Include(v => v.VehicleVersion)
                    .Include(v => v.VehicleType)
                    .FirstOrDefaultAsync();

                if (existingVehicle == null)
                {
                    return NotFound("not-found");
                }

                var vehicleBrand = existingVehicle.BrandId == -1 ? existingVehicle.OtherBrand : existingVehicle.Brand.Name;
                var vehicleModel = existingVehicle.VehicleModelId == -1 ? existingVehicle.OtherVehicleModel : existingVehicle.VehicleModel.Model;
                var vehicleVersion = existingVehicle.VehicleVersionId == -1 ? existingVehicle.OtherVehicleVersion : existingVehicle.VehicleVersion.Version;
                var vehicleType = existingVehicle.VehicleTypeId == -1 ? existingVehicle.OtherVehicleType : existingVehicle.VehicleType.Type;
                var fullVehicleName = $"{vehicleBrand} - {vehicleModel} - {vehicleVersion} - {vehicleType}";

                var accident = new Accident
                {
                    CustomerId = model.CustomerId,
                    Plates = model.Plates,
                    CreatedAt = DateTime.Now,
                };

                var emergencyContacts = await _context.EmergencyContacts
                    .Where(e => e.CustomerId == model.CustomerId && !e.IsDeleted && e.MustBeNotified)
                    .ToListAsync();

                foreach (var contact in emergencyContacts)
                {
                    var subject = $"🚨 Alerta de Emergencia - {customer.FullName}";

                    var reportLink = model.Latitude != null && model.Longitude != null
                        ? $"https://www.google.com/maps?q={model.Latitude},{model.Longitude}"
                        : "";

                    var htmlBody = $@"
                                <!DOCTYPE html>
                                <html lang=""es"">
                                <head>
                                  <meta charset=""UTF-8"" />
                                  <title>Alerta de Emergencia</title>
                                </head>
                                <body style=""margin: 0; font-family: Arial, sans-serif; background-color: #ffffff;"">
                                  <div style=""background-color: #f52222; padding: 20px 0; text-align: center;"">
                                    <h1 style=""margin: 0; color: white;"">
                                      <span style=""font-weight: bold;"">MiTaller</span> <span style=""color: black;"">Financiero</span>
                                    </h1>
                                  </div>

                                  <div style=""padding: 30px 40px; color: #333;"">
                                    <p style=""font-size: 16px;"">
                                      <strong>Estimado(a) {contact.FullName},</strong>
                                    </p>

                                    <p style=""font-size: 16px;"">
                                      Eres el contacto de emergencia registrado por <strong>{customer.FullName}</strong>. Se ha reportado un posible accidente con su vehículo <strong>{fullVehicleName}</strong>.
                                    </p>

                                    {(reportLink != "" ? $@"
                                    <p style=""font-size: 16px;"">
                                      📍 <strong>Ubicación del reporte:</strong> <a href=""{reportLink}"" target=""_blank"" style=""color: #1a73e8; font-weight: bold;"">Ver en Google Maps</a><br />
                                      🕓 <strong>Fecha y hora del reporte:</strong> {DateTime.Now:dd/MM/yyyy - HH:mm}
                                    </p>" : "")}

                                    <p style=""font-size: 16px;"">
                                      Este mensaje no necesariamente indica una situación grave, pero te recomendamos contactar a <strong>{customer.FullName}</strong> para confirmar su estado.
                                    </p>

                                    <p style=""font-size: 16px;"">MiTaller Financiero – Conectando movilidad y seguridad</p>
                                  </div>

                                  <div style=""padding: 20px 30px; font-size: 12px; color: #555;"">
                                    <p><strong>AVISO DE CONFIDENCIALIDAD.</strong> Este correo y la información contenida o adjunta al mismo es privada y confidencial y va dirigida exclusivamente a su destinatario. Mi Taller Financiero informa a quien pueda haber recibido este correo por error que contiene información confidencial cuyo uso, copia, reproducción o distribución está expresamente prohibida. Si no eres el destinatario del mismo y recibes este correo por error, te pedimos pongas en conocimiento al emisor y procedas a la eliminación sin copiarlo, imprimirlo o utilizarlo de ningún modo.</p>
                                    <p><strong>CONFIDENTIALITY WARNING.</strong> This message and the information contained in or attached to it are private and confidential and intended exclusively for the addressee. If you are not an intended recipient of this E-mail, please notify the sender, delete it and do not read, act upon, print, disclose, copy, retain or redistribute any portion of this E-mail.</p>
                                  </div>
                                </body>
                                </html>";

                    await _emailSender.SendEmailAsync(contact.Email, subject, htmlBody);


                    // Creamos la notificación si existe el usuario en la plataforma
                    var registeredUser = await _context.Customers
                        .Where(u => u.Email == contact.Email && u.UserType == UserType.Customer && !u.IsDeleted)
                        .FirstOrDefaultAsync();

                    if (registeredUser != null)
                    {
                        var notification = new Notifications
                        {
                            UserId = registeredUser.Id,
                            UserType = UserType.Customer,
                            Title = "Accidente reportado",
                            Content = $"El vehículo {fullVehicleName} de {customer.FullName} ha sido reportado en un accidente.",
                            Event = "AccidentReported",
                        };

                        await _context.Notifications.AddAsync(notification);
                        await _context.SaveChangesAsync();

                        // TODO: Activar cuando se implementen las notificaciones push Firebase
                        // Enviar notificación push al cliente
                        //await _firebaseNotificationService.SendNotificationToCustomerAsync(
                        //    registeredUser.Id,
                        //    notification.Title,
                        //    notification.Content,
                        //    notification.Event,
                        //    new Dictionary<string, string> { { "accidentId", accident.Id.ToString() } }
                        //);
                    }
                }

                await _context.Accidents.AddAsync(accident);
                await _context.SaveChangesAsync();

                return Ok("accident-created");
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

    }
}
