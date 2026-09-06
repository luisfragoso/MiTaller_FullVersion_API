using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiTaller.Data;
using MiTaller.DTO.Customer;
using MiTaller.DTO.Vehicle;
using MiTaller.Models.Vehicle;
using MiTaller.Models.Customer;
using MiTaller.Models.Inspections;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Security.Claims;
using MiTaller.Models.Auth;
using MiTaller.Models.Notification;
using Microsoft.AspNetCore.Authorization;
using MiTaller.Attributes;
using MiTaller.DTO.Inspections;
using MiTaller.DTO.Pager;
using Microsoft.AspNetCore.Identity.UI.Services;
using MiTaller.Services;
using QuestPDF.Fluent;
using MiTaller.Services.Documents;
using System.Collections.Generic;

namespace MiTaller.Controllers
{
    [Route("api/inspections")]
    [ApiController]
    [Authorize]
    public class InspectionController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly UserManager<BaseIdentityUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly FirebaseNotificationService _firebaseNotificationService;

        public InspectionController(DataContext context, UserManager<BaseIdentityUser> userManager, IEmailSender emailSender, FirebaseNotificationService firebaseNotificationService)
        {
            _context = context;
            _userManager = userManager;
            _emailSender = emailSender;
            _firebaseNotificationService = firebaseNotificationService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetInspectionById(int id)
        {
            try
            {
                // AsNoTracking avoids EF's relationship fixup wiring each Result's/
                // SectionComment's back-reference to this same Inspection instance,
                // which would otherwise create a cycle the JSON serializer can't handle.
                var inspection = await _context.Inspections
                    .AsNoTracking()
                    .Include(i => i.Workshop)
                    .Include(i => i.Customer)
                    .Include(i => i.Vehicle)
                    .Include(i => i.Template)
                    .Include(i => i.Results)
                    .Include(i => i.SectionComments)
                    .Where(i => i.Id == id && i.IsActive)
                    .FirstOrDefaultAsync();

                if (inspection == null) return NotFound("not-found");

                return Ok(inspection);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpGet("workshop/{workshopId}")]
        public async Task<ActionResult<PagerResponseDto<InspectionInWorkshopFullInfoResponseDto>>> GetActiveInspectionsByWorkshop(Guid workshopId, [FromQuery] string vehicleFormat, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            return await GetInspectionsByWorkshopAndStatus(workshopId, vehicleFormat, page, pageSize, isActive: true);
        }

        [HttpGet("workshop/{workshopId}/closed")]
        public async Task<ActionResult<PagerResponseDto<InspectionInWorkshopFullInfoResponseDto>>> GetClosedInspectionsByWorkshop(Guid workshopId, [FromQuery] string vehicleFormat, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            return await GetInspectionsByWorkshopAndStatus(workshopId, vehicleFormat, page, pageSize, isActive: false);
        }

        private async Task<ActionResult<PagerResponseDto<InspectionInWorkshopFullInfoResponseDto>>> GetInspectionsByWorkshopAndStatus(Guid workshopId, string vehicleFormat, int page, int pageSize, bool isActive)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(vehicleFormat))
                {
                    return BadRequest("invalid-empty");
                }

                var workshop = await _context.Workshops
                    .Where(w => w.Id == workshopId && !w.IsDeleted)
                    .FirstOrDefaultAsync();

                if (workshop == null) return NotFound("not-found");

                // vehicleFormat here can be a real vehicle format (Yate, Avión, ...)
                // or a "taller especializado" specialty key (Radiadores, ...) - the
                // enabled-formats list surfaced to the app already mixes both, so
                // this just resolves whichever one matches.
                var query = _context.Inspections
                    .Where(i => i.WorkshopId == workshopId
                             && i.IsActive == isActive
                             && (_context.InspectionTemplateFormats
                                    .Any(f => f.TemplateId == i.TemplateId && f.VehicleFormat.ToLower() == vehicleFormat.ToLower())
                                 || _context.InspectionTemplateSpecialties
                                    .Any(s => s.TemplateId == i.TemplateId && s.Specialty.ToLower() == vehicleFormat.ToLower())))
                    .Include(i => i.Customer)
                    .Include(i => i.Template)
                    .Include(i => i.Vehicle)
                        .ThenInclude(v => v.Brand)
                    .Include(i => i.Vehicle)
                        .ThenInclude(v => v.VehicleModel)
                    .Include(i => i.Vehicle)
                        .ThenInclude(v => v.VehicleVersion)
                    .Include(i => i.Vehicle)
                        .ThenInclude(v => v.VehicleType)
                    .OrderByDescending(i => i.InspectionDate);

                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                var inspections = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var elements = inspections.Select(i => new InspectionInWorkshopFullInfoResponseDto
                {
                    InspectionId = i.Id,
                    CustomerId = i.Customer.Id,
                    FullName = i.Customer.FullName,
                    Email = i.Customer.Email,
                    PhoneNumber = i.Customer.NormalizedPhoneNumber,
                    Status = i.Status,
                    VehicleFormat = i.Template.VehicleFormat,
                    Vehicle = new VehicleResponseDto
                    {
                        Id = i.Vehicle.Id,
                        Brand = i.Vehicle.BrandId == -1 ? i.Vehicle.OtherBrand : i.Vehicle.Brand.Name,
                        Model = i.Vehicle.VehicleModelId == -1 ? i.Vehicle.OtherVehicleModel : i.Vehicle.VehicleModel.Model,
                        Version = i.Vehicle.VehicleVersionId == -1 ? i.Vehicle.OtherVehicleVersion : i.Vehicle.VehicleVersion.Version,
                        Type = i.Vehicle.VehicleTypeId == -1 ? i.Vehicle.OtherVehicleType : i.Vehicle.VehicleType.Type,
                        Year = i.Vehicle.Year,
                        SerialNumber = i.Vehicle.SerialNumber,
                        Color = i.Vehicle.Color,
                        Plates = i.Vehicle.Plates,
                        RimRubber = i.Vehicle.RimRubber,
                        Kms = i.Vehicle.Kms,
                        VehicleFormat = i.Vehicle.VehicleFormat,
                    },
                    InspectionHistory = new List<VehicleInspectionHistoryResponseDto>(),
                }).ToList();

                var response = new PagerResponseDto<InspectionInWorkshopFullInfoResponseDto>
                {
                    CurrentPage = page,
                    TotalPages = totalPages,
                    Elements = elements
                };

                return Ok(response);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPost]
        [AuthorizeEmployee("Administrador", "Registrar vehículos")]
        public async Task<ActionResult> CreateInspection([FromForm] PostInspectionDto model)
        {
            try
            {
                var workshop = await _context.Workshops
                    .Where(w => w.Id == model.WorkshopId && !w.IsDeleted)
                    .FirstOrDefaultAsync();
                if (workshop == null) return NotFound("not-found");

                var template = await _context.InspectionTemplates
                    .Where(t => t.Id == model.TemplateId && t.IsActive)
                    .Include(t => t.Sections)
                        .ThenInclude(s => s.Items)
                    .FirstOrDefaultAsync();
                if (template == null) return NotFound("not-found");

                var validItemIds = template.Sections.SelectMany(s => s.Items).Select(i => i.Id).ToHashSet();
                var validSectionIds = template.Sections.Select(s => s.Id).ToHashSet();

                if (model.Results != null && model.Results.Any(r => !validItemIds.Contains(r.TemplateItemId)))
                {
                    return BadRequest("invalid-template-item");
                }

                if (model.SectionComments != null && model.SectionComments.Any(c => !validSectionIds.Contains(c.SectionId)))
                {
                    return BadRequest("invalid-template-section");
                }

                var activeInspection = await _context.Inspections
                    .Where(a => a.WorkshopId == model.WorkshopId
                            && a.CustomerId == model.CustomerId
                            && a.VehicleId == model.VehicleId
                            && a.IsActive)
                    .FirstOrDefaultAsync();

                if (activeInspection != null)
                {
                    return BadRequest("cannot-register-duplicated-inspection-appointment");
                }

                Customer customer;
                Vehicle vehicle;

                if (model.IsNewCustomer)
                {
                    if (model.Customer == null) return BadRequest("invalid-empty");
                    var newCustomer = new Customer
                    {
                        Id = Guid.NewGuid(),
                        UserName = model.Customer.Email + "_customer",
                        FullName = model.Customer.FullName,
                        Email = model.Customer.Email,
                        PhoneNumber = model.Customer.PhoneNumber + "_customer",
                        NormalizedPhoneNumber = model.Customer.PhoneNumber,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = null,
                        DeletedAt = null,
                        IsDeleted = false,
                        UserType = UserType.Customer
                    };

                    var result = await _userManager.CreateAsync(newCustomer, "Password123!");
                    if (!result.Succeeded) return BadRequest("unknown-error");

                    var subject = "¡Bienvenid@ a Mi taller!";
                    var htmlBody = $@"
                        <!DOCTYPE html>
                        <html lang=""es"">
                          <head>
                            <meta charset=""UTF-8"" />
                            <title>Bienvenida MiTaller</title>
                          </head>
                          <body style=""margin: 0; font-family: Arial, sans-serif; background-color: #ffffff;"">
                            <div style=""background-color: #f52222; padding: 20px 0; text-align: center;"">
                              <h1 style=""margin: 0; color: white;"">
                                <span style=""font-weight: bold;"">MiTaller</span> <span style=""color: black;"">Financiero</span>
                              </h1>
                            </div>

                            <div style=""padding: 30px; text-align: center; color: #333;"">
                              <h2 style=""margin-top: 0;"">Hola</h2>
                              <p style=""font-size: 18px;"">
                                Hola <strong>{newCustomer.FullName}</strong>. Te damos la bienvenida a MiTaller. Tu cuenta ha sido creada por el taller <strong>{workshop.WorkshopName}</strong>,
                                al que llevaste tu vehículo para revisión o servicio.
                              </p>
                              <p style=""font-size: 18px; margin-top: 20px;"">
                                Para comenzar a usar tu cuenta, descarga la aplicación de clientes y<br />
                                restablece tu contraseña
                              </p>
                              <p style=""margin-top: 30px;"">¡Saludos, gracias por tu descarga!</p>
                            </div>

                            <div style=""padding: 20px 30px; font-size: 12px; color: #555;"">
                                <p><strong>AVISO DE CONFIDENCIALIDAD.</strong> Este correo y la información contenida o adjunta al mismo es privada y confidencial y va dirigida exclusivamente a su destinatario. Mi Taller Financiero informa a quien pueda haber recibido este correo por error que contiene información confidencial cuyo uso, copia, reproducción o distribución está expresamente prohibida. Si no eres el destinatario del mismo y recibes este correo por error, te pedimos pongas en conocimiento al emisor y procedas a la eliminación sin copiarlo, imprimirlo o utilizarlo de ningún modo.</p>
                                <p><strong>CONFIDENTIALITY WARNING.</strong> This message and the information contained in or attached to it are private and confidential and intended exclusively for the addressee. Comisiónn Federal de Electricidad informs whomit may receive it in error that it contains privileged information and its use, copy, reproduction or distributions is prohibited.  If you are not an intended recipient of this E-mail, please notify the sender, delete it and do not read, act upon, print, disclose, copy, retain or redistribute any portion of this E-mail.</p>
                            </div>
                          </body>
                        </html>
                        ";

                    await _emailSender.SendEmailAsync(newCustomer.Email, subject, htmlBody);

                    var notificationSettings = new NotificationSettings
                    {
                        UserId = newCustomer.Id,
                        UserType = UserType.Customer,
                    };

                    await _context.NotificationSettings.AddAsync(notificationSettings);
                    await _context.SaveChangesAsync();

                    var notification = new Notifications
                    {
                        UserId = newCustomer.Id,
                        UserType = UserType.Customer,
                        Title = "Cuenta creada exitosamente",
                        Content = "¡Tu cuenta ha sido creada con éxito! Ahora puedes acceder a todas las funcionalidades de tu cuenta.",
                        Event = "AccountCreated"
                    };

                    await _context.Notifications.AddAsync(notification);
                    await _context.SaveChangesAsync();

                    customer = newCustomer;

                    if (model.Vehicle == null) return BadRequest("invalid-empty");
                    vehicle = new Vehicle
                    {
                        CustomerId = customer.Id,
                        Year = model.Vehicle.Year,
                        BrandId = model.Vehicle.BrandId,
                        OtherBrand = model.Vehicle.OtherBrand,
                        VehicleModelId = model.Vehicle.VehicleModelId,
                        OtherVehicleModel = model.Vehicle.OtherVehicleModel,
                        VehicleVersionId = model.Vehicle.VehicleVersionId,
                        OtherVehicleVersion = model.Vehicle.OtherVehicleVersion,
                        VehicleTypeId = model.Vehicle.VehicleTypeId,
                        OtherVehicleType = model.Vehicle.OtherVehicleType,
                        SerialNumber = model.Vehicle.SerialNumber,
                        Color = model.Vehicle.Color,
                        Plates = model.Vehicle.Plates,
                        RimRubber = model.Vehicle.RimRubber,
                        Kms = model.Vehicle.Kms,
                        VehicleFormat = model.Vehicle.VehicleFormat
                    };

                    if (model.Vehicle.Image != null)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await model.Vehicle.Image.CopyToAsync(memoryStream);
                            vehicle.Image = memoryStream.ToArray();
                        }
                    }

                    _context.Vehicles.Add(vehicle);
                    await _context.SaveChangesAsync();
                }
                else if (!model.IsNewCustomer && model.IsNewVehicle)
                {
                    customer = await _context.Customers
                        .Where(c => c.Id == model.CustomerId && !c.IsDeleted)
                        .FirstOrDefaultAsync();
                    if (customer == null) return NotFound("not-found");

                    if (model.Vehicle == null) return BadRequest("invalid-empty");
                    vehicle = new Vehicle
                    {
                        CustomerId = customer.Id,
                        Year = model.Vehicle.Year,
                        BrandId = model.Vehicle.BrandId,
                        OtherBrand = model.Vehicle.OtherBrand,
                        VehicleModelId = model.Vehicle.VehicleModelId,
                        OtherVehicleModel = model.Vehicle.OtherVehicleModel,
                        VehicleVersionId = model.Vehicle.VehicleVersionId,
                        OtherVehicleVersion = model.Vehicle.OtherVehicleVersion,
                        VehicleTypeId = model.Vehicle.VehicleTypeId,
                        OtherVehicleType = model.Vehicle.OtherVehicleType,
                        SerialNumber = model.Vehicle.SerialNumber,
                        Color = model.Vehicle.Color,
                        Plates = model.Vehicle.Plates,
                        RimRubber = model.Vehicle.RimRubber,
                        Kms = model.Vehicle.Kms,
                        VehicleFormat = model.Vehicle.VehicleFormat
                    };

                    if (model.Vehicle.Image != null)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await model.Vehicle.Image.CopyToAsync(memoryStream);
                            vehicle.Image = memoryStream.ToArray();
                        }
                    }

                    _context.Vehicles.Add(vehicle);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    customer = await _context.Customers
                        .Where(c => c.Id == model.CustomerId && !c.IsDeleted)
                        .FirstOrDefaultAsync();
                    if (customer == null) return NotFound("not-found");

                    vehicle = await _context.Vehicles
                        .Where(v => v.Id == model.VehicleId && !v.IsDeleted)
                        .FirstOrDefaultAsync();
                    if (vehicle == null) return NotFound("not-found");
                }

                var inspection = new Inspection
                {
                    TemplateId = template.Id,
                    WorkshopId = workshop.Id,
                    CustomerId = customer.Id,
                    VehicleId = vehicle.Id,
                    IsActive = true,
                    Status = "Pendiente",
                    GeneralObservations = model.GeneralObservations,
                };

                _context.Inspections.Add(inspection);
                await _context.SaveChangesAsync();

                if (model.Results != null && model.Results.Any())
                {
                    foreach (var resultDto in model.Results)
                    {
                        _context.InspectionResultItems.Add(new InspectionResultItem
                        {
                            InspectionId = inspection.Id,
                            TemplateItemId = resultDto.TemplateItemId,
                            Value = resultDto.Value
                        });
                    }
                    await _context.SaveChangesAsync();
                }

                if (model.SectionComments != null && model.SectionComments.Any())
                {
                    foreach (var commentDto in model.SectionComments)
                    {
                        _context.InspectionSectionComments.Add(new InspectionSectionComment
                        {
                            InspectionId = inspection.Id,
                            SectionId = commentDto.SectionId,
                            Comment = commentDto.Comment
                        });
                    }
                    await _context.SaveChangesAsync();
                }

                if (model.Files != null && model.Files.Any())
                {
                    foreach (var file in model.Files)
                    {
                        using (var ms = new MemoryStream())
                        {
                            await file.CopyToAsync(ms);
                            var newFile = new InspectionFile
                            {
                                InspectionId = inspection.Id,
                                FileName = file.FileName,
                                FileType = file.ContentType,
                                FileData = ms.ToArray()
                            };

                            _context.InspectionFiles.Add(newFile);
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                var changedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var snapshot = new
                {
                    results = model.Results?.Select(r => new { r.TemplateItemId, Value = r.Value.ToString() }),
                    sectionComments = model.SectionComments?.Select(c => new { c.SectionId, c.Comment }),
                    generalObservations = inspection.GeneralObservations,
                };

                var inspectionHistory = new InspectionHistory
                {
                    InspectionId = inspection.Id,
                    ChangedAt = DateTime.Now,
                    ChangedBy = changedBy,
                    Status = inspection.Status,
                    ResultsSnapshotJson = JsonSerializer.Serialize(snapshot),
                };
                _context.InspectionHistories.Add(inspectionHistory);
                await _context.SaveChangesAsync();

                return Ok(new { id = inspection.Id });
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpGet("download-file/{fileId}")]
        public async Task<ActionResult> DownloadInspectionFile(int fileId)
        {
            try
            {
                var file = await _context.InspectionFiles.FindAsync(fileId);
                if (file == null) return NotFound("not-found");

                return File(file.FileData, file.FileType, file.FileName);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpGet("history/{inspectionId}")]
        public async Task<ActionResult> GetInspectionHistory(int inspectionId)
        {
            try
            {
                var history = await _context.InspectionHistories
                    .Where(h => h.InspectionId == inspectionId)
                    .OrderByDescending(h => h.ChangedAt)
                    .ToListAsync();

                return Ok(history);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpGet("{id}/pdf")]
        public async Task<ActionResult> GenerateInspectionPdf(int id)
        {
            try
            {
                var inspection = await _context.Inspections
                    .AsNoTracking()
                    .Include(i => i.Workshop)
                    .Include(i => i.Customer)
                    .Include(i => i.Vehicle)
                        .ThenInclude(v => v.Brand)
                    .Include(i => i.Vehicle)
                        .ThenInclude(v => v.VehicleModel)
                    .Include(i => i.Vehicle)
                        .ThenInclude(v => v.VehicleVersion)
                    .Include(i => i.Vehicle)
                        .ThenInclude(v => v.VehicleType)
                    .Include(i => i.Results)
                    .Include(i => i.SectionComments)
                    .Where(i => i.Id == id)
                    .FirstOrDefaultAsync();

                if (inspection == null) return NotFound("not-found");

                var template = await _context.InspectionTemplates
                    .AsNoTracking()
                    .Where(t => t.Id == inspection.TemplateId)
                    .Include(t => t.Sections)
                        .ThenInclude(s => s.Items)
                    .FirstOrDefaultAsync();

                if (template == null) return NotFound("not-found");

                var document = new GenericInspectionDocument(inspection, template);
                var pdfBytes = document.GeneratePdf();

                return File(pdfBytes, "application/pdf", "reporte-inspeccion.pdf");
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

    }
}
