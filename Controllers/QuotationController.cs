using Microsoft.AspNetCore.Mvc;
using MiTaller.Data;
using MiTaller.Models;
using Microsoft.EntityFrameworkCore;
using MiTaller.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MiTaller.DTO.Quotation;
using Microsoft.AspNetCore.Authorization;
using MiTaller.Attributes;
using MiTaller.DTO.Vehicle;
using MiTaller.DTO.Workshop;
using MiTaller.Models.Workshop;
using MiTaller.DTO.Workshop.Services;
using MiTaller.Models.Vehicle;
using MiTaller.Models.Domain;
using MiTaller.Models.Auth;
using MiTaller.Models.Notification;
using MiTaller.DTO.Pager;
using MiTaller.Models.Customer;
using MiTaller.Services;
using System.Collections.Generic;

namespace MiTaller.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class QuotationController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly FirebaseNotificationService _firebaseNotificationService;

        public QuotationController(DataContext context, FirebaseNotificationService firebaseNotificationService)
        {
            _context = context;
            _firebaseNotificationService = firebaseNotificationService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<QuotationResponseDto>> GetQuotationById(int id)
        {
            try
            {
                var quotationToUpdate = await _context.Quotations
                    .Where(q => q.Id == id)
                    .Select(q => new { q.WorkshopId })
                    .FirstOrDefaultAsync();

                if (quotationToUpdate != null)
                {
                    await UpdateExpiredQuotations(quotationToUpdate.WorkshopId);
                }

                var quotation = await _context.Quotations
                    .Where(q => q.Id == id)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.Brand)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.VehicleModel)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.VehicleVersion)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.VehicleType)
                    .Include(q => q.Workshop)
                    .Include(q => q.Customer)
                    .FirstOrDefaultAsync();

                if (quotation == null)
                {
                    return NotFound("not-found");
                }

                var workshopServices = await _context.WorkshopServices
                    .Where(ws => ws.WorkshopId == quotation.WorkshopId && !ws.IsDeleted)
                    .Include(ws => ws.Service)
                    .ToListAsync();

                var quotationServices = await _context.QuotationServices
                    .Where(qs => qs.QuotationId == quotation.Id)
                    .Include(ws => ws.Service)
                    .ToListAsync();

                var vehicleBrand = quotation.Vehicle.BrandId == -1 ? quotation.Vehicle.OtherBrand : quotation.Vehicle.Brand.Name;
                var vehicleModel = quotation.Vehicle.VehicleModelId == -1 ? quotation.Vehicle.OtherVehicleModel : quotation.Vehicle.VehicleModel.Model;

                var quotationResponseDto = new QuotationResponseDto
                {
                    Id = quotation.Id,
                    WorkshopId = quotation.WorkshopId,
                    WorkshopName = quotation.Workshop.WorkshopName,
                    CustomerId = quotation.CustomerId,
                    CustomerName = quotation.Customer.FullName,
                    Vehicle = new VehicleResponseDto
                    {
                        Id = quotation.Vehicle.Id,
                        Brand = vehicleBrand,
                        Model = vehicleModel,
                        Version = quotation.Vehicle.VehicleVersionId == -1 ? quotation.Vehicle.OtherVehicleVersion : quotation.Vehicle.VehicleVersion.Version,
                        Type = quotation.Vehicle.VehicleTypeId == -1 ? quotation.Vehicle.OtherVehicleType : quotation.Vehicle.VehicleType.Type,
                        Year = quotation.Vehicle.Year,
                        SerialNumber = quotation.Vehicle.SerialNumber,
                        Color = quotation.Vehicle.Color,
                        Plates = quotation.Vehicle.Plates,
                        RimRubber = quotation.Vehicle.RimRubber,
                        Kms = quotation.Vehicle.Kms,
                        VehicleFormat = quotation.Vehicle.VehicleFormat,
                    },
                    Title = $"{vehicleBrand} - {vehicleModel} - {quotation.Vehicle.Year} - {quotation.Vehicle.Plates}",
                    Description = quotation.Description,
                    PriceOfLabor = quotation.PriceOfLabor,
                    PriceOfSpareParts = quotation.PriceOfSpareParts,
                    Status = quotation.Status,
                    CreatedAt = quotation.CreatedAt,
                    Services = quotationServices.Select(qs => new WorkshopServiceResponseDto
                    {
                        ServiceId = qs.WorkshopServiceId,
                        ServiceName = qs.Service.Service.Name,
                        Price = qs.Price ?? 0,
                    }).ToList()
                };

                return Ok(quotationResponseDto);
            }
            catch (Exception)
            {
                return BadRequest("unknown-error");
            }
        }


        [HttpGet("customer/{customerId}")]
        public async Task<ActionResult<ICollection<QuotationResponseDto>>> GetQuotationsByCustomer(Guid customerId)
        {
            try 
            {
                var quotationToUpdate = await _context.Quotations
                    .Where(q => q.CustomerId == customerId)
                    .Select(q => new { q.WorkshopId })
                    .FirstOrDefaultAsync();

                if (quotationToUpdate != null)
                {
                    await UpdateExpiredQuotations(quotationToUpdate.WorkshopId);
                }

                var quotations = await _context.Quotations
                    .Where(q => q.CustomerId == customerId)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.Brand)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.VehicleModel)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.VehicleVersion)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.VehicleType)
                    .Include(q => q.Workshop)
                    .Include(q => q.Customer)
                    .ToListAsync();

                if (!quotations.Any()) return NotFound("not-found");

                var quotationsResponseDto = new List<QuotationResponseDto>();
                foreach (var quotation in quotations)
                {
                    var vehicleBrand = quotation.Vehicle.BrandId == -1 ? quotation.Vehicle.OtherBrand : quotation.Vehicle.Brand.Name;
                    var vehicleModel = quotation.Vehicle.VehicleModelId == -1 ? quotation.Vehicle.OtherVehicleModel : quotation.Vehicle.VehicleModel.Model;

                    var workshopServices = await _context.WorkshopServices
                        .Where(ws => ws.WorkshopId == quotation.WorkshopId && !ws.IsDeleted)
                        .Include(ws => ws.Service)
                        .ToListAsync();

                    var quotationServices = await _context.QuotationServices
                    .Where(qs => qs.QuotationId == quotation.Id)
                    .Include(ws => ws.Service)
                    .ToListAsync();

                    var quotationResponseDto = new QuotationResponseDto
                    {
                        Id = quotation.Id,
                        WorkshopId = quotation.WorkshopId,
                        WorkshopName = quotation.Workshop.WorkshopName,
                        CustomerId = quotation.CustomerId,
                        CustomerName = quotation.Customer.FullName,
                        Vehicle = new VehicleResponseDto
                        {
                            Id = quotation.Vehicle.Id,
                            Brand = vehicleBrand,
                            Model = vehicleModel,
                            Version = quotation.Vehicle.VehicleVersionId == -1 ? quotation.Vehicle.OtherVehicleVersion : quotation.Vehicle.VehicleVersion.Version,
                            Type = quotation.Vehicle.VehicleTypeId == -1 ? quotation.Vehicle.OtherVehicleType : quotation.Vehicle.VehicleType.Type,
                            Year = quotation.Vehicle.Year,
                            SerialNumber = quotation.Vehicle.SerialNumber,
                            Color = quotation.Vehicle.Color,
                            Plates = quotation.Vehicle.Plates,
                            RimRubber = quotation.Vehicle.RimRubber,
                            Kms = quotation.Vehicle.Kms,
                            VehicleFormat = quotation.Vehicle.VehicleFormat,
                        },
                        Title = $"{vehicleBrand} - {vehicleModel} - {quotation.Vehicle.Year} - {quotation.Vehicle.Plates}",
                        Description = quotation.Description,
                        PriceOfLabor = quotation.PriceOfLabor,
                        PriceOfSpareParts = quotation.PriceOfSpareParts,
                        Status = quotation.Status,
                        CreatedAt = quotation.CreatedAt,
                        Services = quotationServices.Select(qs => new WorkshopServiceResponseDto
                        {
                            ServiceId = qs.WorkshopServiceId,
                            ServiceName = qs.Service.Service.Name,
                            Price = qs.Price ?? 0,
                        }).ToList()
                    };

                    quotationsResponseDto.Add(quotationResponseDto);
                }

                return Ok(quotationsResponseDto);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPost("customer/{customerId}/pager")]
        public async Task<ActionResult<PagerResponseDto<QuotationResponseDto>>> GetQuotationsByCustomerPaged(Guid customerId, [FromBody] PagerBodyDto pager)
        {
            try
            {
                var quotationToUpdate = await _context.Quotations
                    .Where(q => q.CustomerId == customerId)
                    .Select(q => new { q.WorkshopId })
                    .FirstOrDefaultAsync();

                if (quotationToUpdate != null)
                {
                    await UpdateExpiredQuotations(quotationToUpdate.WorkshopId);
                }

                var query = _context.Quotations
                    .Where(q => q.CustomerId == customerId)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.Brand)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.VehicleModel)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.VehicleVersion)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.VehicleType)
                    .Include(q => q.Workshop)
                    .Include(q => q.Customer)
                    .OrderByDescending(q => q.CreatedAt);

                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalCount / pager.PageSize);

                var quotations = await query
                    .Skip((pager.PageNumber - 1) * pager.PageSize)
                    .Take(pager.PageSize)
                    .ToListAsync();

                if (!quotations.Any()) return NotFound("not-found");

                var quotationsResponseDto = new List<QuotationResponseDto>();

                foreach (var quotation in quotations)
                {
                    var vehicleBrand = quotation.Vehicle.BrandId == -1 ? quotation.Vehicle.OtherBrand : quotation.Vehicle.Brand.Name;
                    var vehicleModel = quotation.Vehicle.VehicleModelId == -1 ? quotation.Vehicle.OtherVehicleModel : quotation.Vehicle.VehicleModel.Model;

                    var quotationServices = await _context.QuotationServices
                        .Where(qs => qs.QuotationId == quotation.Id)
                        .Include(ws => ws.Service)
                        .ToListAsync();

                    var quotationResponseDto = new QuotationResponseDto
                    {
                        Id = quotation.Id,
                        WorkshopId = quotation.WorkshopId,
                        WorkshopName = quotation.Workshop.WorkshopName,
                        CustomerId = quotation.CustomerId,
                        CustomerName = quotation.Customer.FullName,
                        Vehicle = new VehicleResponseDto
                        {
                            Id = quotation.Vehicle.Id,
                            Brand = vehicleBrand,
                            Model = vehicleModel,
                            Version = quotation.Vehicle.VehicleVersionId == -1 ? quotation.Vehicle.OtherVehicleVersion : quotation.Vehicle.VehicleVersion.Version,
                            Type = quotation.Vehicle.VehicleTypeId == -1 ? quotation.Vehicle.OtherVehicleType : quotation.Vehicle.VehicleType.Type,
                            Year = quotation.Vehicle.Year,
                            SerialNumber = quotation.Vehicle.SerialNumber,
                            Color = quotation.Vehicle.Color,
                            Plates = quotation.Vehicle.Plates,
                            RimRubber = quotation.Vehicle.RimRubber,
                            Kms = quotation.Vehicle.Kms,
                            VehicleFormat = quotation.Vehicle.VehicleFormat,
                        },
                        Title = $"{vehicleBrand} - {vehicleModel} - {quotation.Vehicle.Year} - {quotation.Vehicle.Plates}",
                        Description = quotation.Description,
                        PriceOfLabor = quotation.PriceOfLabor,
                        PriceOfSpareParts = quotation.PriceOfSpareParts,
                        Status = quotation.Status,
                        CreatedAt = quotation.CreatedAt,
                        Services = quotationServices.Select(qs => new WorkshopServiceResponseDto
                        {
                            ServiceId = qs.WorkshopServiceId,
                            ServiceName = qs.Service.Service.Name,
                            Price = qs.Price ?? 0,
                        }).ToList()
                    };

                    quotationsResponseDto.Add(quotationResponseDto);
                }

                var response = new PagerResponseDto<QuotationResponseDto>
                {
                    CurrentPage = pager.PageNumber,
                    TotalPages = totalPages,
                    Elements = quotationsResponseDto
                };

                return Ok(response);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }


        [HttpGet("workshop/{workshopId}")]
        public async Task<ActionResult<ICollection<QuotationResponseDto>>> GetQuotationsByWorkshop(Guid workshopId)
        {
            try 
            {
                var quotationToUpdate = await _context.Quotations
                    .Where(q => q.WorkshopId == workshopId)
                    .Select(q => new { q.WorkshopId })
                    .FirstOrDefaultAsync();

                if (quotationToUpdate != null)
                {
                    await UpdateExpiredQuotations(quotationToUpdate.WorkshopId);
                }

                var quotations = await _context.Quotations
                    .Where(q => q.WorkshopId == workshopId)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.Brand)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.VehicleModel)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.VehicleVersion)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.VehicleType)
                    .Include(q => q.Workshop)
                    .Include(q => q.Customer)
                    .ToListAsync();

                if (quotations == null)
                {
                    return NotFound("not-found");
                }

                var workshopServices = await _context.WorkshopServices
                    .Where(ws => ws.WorkshopId == workshopId && !ws.IsDeleted)
                    .Include(ws => ws.Service)
                .ToListAsync();

                var quotationsResponseDto = new List<QuotationResponseDto>();
                foreach (var quotation in quotations)
                {
                    var quotationServices = await _context.QuotationServices
                    .Where(qs => qs.QuotationId == quotation.Id)
                    .Include(ws => ws.Service)
                    .ToListAsync();

                    var vehicleBrand = quotation.Vehicle.BrandId == -1 ? quotation.Vehicle.OtherBrand : quotation.Vehicle.Brand.Name;
                    var vehicleModel = quotation.Vehicle.VehicleModelId == -1 ? quotation.Vehicle.OtherVehicleModel : quotation.Vehicle.VehicleModel.Model;

                    var quotationResponseDto = new QuotationResponseDto
                    {
                        Id = quotation.Id,
                        WorkshopId = quotation.WorkshopId,
                        WorkshopName = quotation.Workshop.WorkshopName,
                        CustomerId = quotation.CustomerId,
                        CustomerName = quotation.Customer.FullName,
                        Vehicle = new VehicleResponseDto
                        {
                            Id = quotation.Vehicle.Id,
                            Brand = vehicleBrand,
                            Model = vehicleModel,
                            Version = quotation.Vehicle.VehicleVersionId == -1 ? quotation.Vehicle.OtherVehicleVersion : quotation.Vehicle.VehicleVersion.Version,
                            Type = quotation.Vehicle.VehicleTypeId == -1 ? quotation.Vehicle.OtherVehicleType : quotation.Vehicle.VehicleType.Type,
                            Year = quotation.Vehicle.Year,
                            SerialNumber = quotation.Vehicle.SerialNumber,
                            Color = quotation.Vehicle.Color,
                            Plates = quotation.Vehicle.Plates,
                            RimRubber = quotation.Vehicle.RimRubber,
                            Kms = quotation.Vehicle.Kms,
                            VehicleFormat = quotation.Vehicle.VehicleFormat,
                        },
                        Title = $"{vehicleBrand} - {vehicleModel} - {quotation.Vehicle.Year} - {quotation.Vehicle.Plates}",
                        Description = quotation.Description,
                        PriceOfLabor = quotation.PriceOfLabor,
                        PriceOfSpareParts = quotation.PriceOfSpareParts,
                        Status = quotation.Status,
                        CreatedAt = quotation.CreatedAt,
                        Services = quotationServices.Select(qs => new WorkshopServiceResponseDto
                        {
                            ServiceId = qs.WorkshopServiceId,
                            ServiceName = qs.Service.Service.Name,
                            Price = qs.Price ?? 0,
                        }).ToList()
                    };

                    quotationsResponseDto.Add(quotationResponseDto);
                }

                return Ok(quotationsResponseDto);
                }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }


        [HttpPost("workshop/{workshopId}/pager")]
        public async Task<ActionResult<PagerResponseDto<QuotationResponseDto>>> GetQuotationsByWorkshopPaged(Guid workshopId, [FromBody] PagerBodyDto pager)
        {
            try
            {
                var quotationToUpdate = await _context.Quotations
                    .Where(q => q.WorkshopId == workshopId)
                    .Select(q => new { q.WorkshopId })
                    .FirstOrDefaultAsync();

                if (quotationToUpdate != null)
                {
                    await UpdateExpiredQuotations(quotationToUpdate.WorkshopId);
                }

                var query = _context.Quotations
                    .Where(q => q.WorkshopId == workshopId)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.Brand)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.VehicleModel)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.VehicleVersion)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.VehicleType)
                    .Include(q => q.Workshop)
                    .Include(q => q.Customer)
                    .OrderByDescending(q => q.CreatedAt);

                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalCount / pager.PageSize);

                var quotations = await query
                    .Skip((pager.PageNumber - 1) * pager.PageSize)
                    .Take(pager.PageSize)
                    .ToListAsync();

                if (!quotations.Any()) return NotFound("not-found");

                var quotationsResponseDto = new List<QuotationResponseDto>();

                foreach (var quotation in quotations)
                {
                    var quotationServices = await _context.QuotationServices
                        .Where(qs => qs.QuotationId == quotation.Id)
                        .Include(ws => ws.Service)
                        .ToListAsync();

                    var vehicleBrand = quotation.Vehicle.BrandId == -1 ? quotation.Vehicle.OtherBrand : quotation.Vehicle.Brand.Name;
                    var vehicleModel = quotation.Vehicle.VehicleModelId == -1 ? quotation.Vehicle.OtherVehicleModel : quotation.Vehicle.VehicleModel.Model;

                    var quotationResponseDto = new QuotationResponseDto
                    {
                        Id = quotation.Id,
                        WorkshopId = quotation.WorkshopId,
                        WorkshopName = quotation.Workshop.WorkshopName,
                        CustomerId = quotation.CustomerId,
                        CustomerName = quotation.Customer.FullName,
                        Vehicle = new VehicleResponseDto
                        {
                            Id = quotation.Vehicle.Id,
                            Brand = vehicleBrand,
                            Model = vehicleModel,
                            Version = quotation.Vehicle.VehicleVersionId == -1 ? quotation.Vehicle.OtherVehicleVersion : quotation.Vehicle.VehicleVersion.Version,
                            Type = quotation.Vehicle.VehicleTypeId == -1 ? quotation.Vehicle.OtherVehicleType : quotation.Vehicle.VehicleType.Type,
                            Year = quotation.Vehicle.Year,
                            SerialNumber = quotation.Vehicle.SerialNumber,
                            Color = quotation.Vehicle.Color,
                            Plates = quotation.Vehicle.Plates,
                            RimRubber = quotation.Vehicle.RimRubber,
                            Kms = quotation.Vehicle.Kms,
                            VehicleFormat = quotation.Vehicle.VehicleFormat,
                        },
                        Title = $"{vehicleBrand} - {vehicleModel} - {quotation.Vehicle.Year} - {quotation.Vehicle.Plates}",
                        Description = quotation.Description,
                        PriceOfLabor = quotation.PriceOfLabor,
                        PriceOfSpareParts = quotation.PriceOfSpareParts,
                        Status = quotation.Status,
                        CreatedAt = quotation.CreatedAt,
                        Services = quotationServices.Select(qs => new WorkshopServiceResponseDto
                        {
                            ServiceId = qs.WorkshopServiceId,
                            ServiceName = qs.Service.Service.Name,
                            Price = qs.Price ?? 0,
                        }).ToList()
                    };

                    quotationsResponseDto.Add(quotationResponseDto);
                }

                var response = new PagerResponseDto<QuotationResponseDto>
                {
                    CurrentPage = pager.PageNumber,
                    TotalPages = totalPages,
                    Elements = quotationsResponseDto
                };

                return Ok(response);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }


        [HttpPost]
        [AuthorizeEmployee("Administrador")]
        public async Task<ActionResult> CreateQuotation(PostQuotationDto model)
        {
            try 
            { 
                var customerExists = await _context.Customers.
                    Where(c => c.Id == model.CustomerId && !c.IsDeleted).FirstOrDefaultAsync();

                var workshopExists = await _context.Workshops.
                    AnyAsync(w => w.Id == model.WorkshopId && !w.IsDeleted);

                var vehicle = await _context.Vehicles
                    .Where(v => v.Id == model.VehicleId && v.CustomerId == model.CustomerId)
                    .Include(v => v.Brand)
                    .Include(v => v.VehicleModel)
                    .FirstOrDefaultAsync();

                if (customerExists == null || !workshopExists || vehicle == null)
                {
                    return NotFound("not-found");
                }

                var workshopServices = await _context.WorkshopServices
                    .Where(ws => model.Services.Select(s => s.ServiceId).Contains(ws.Id)
                                 && ws.WorkshopId == model.WorkshopId
                                 && !ws.IsDeleted)
                    .ToListAsync();

                if (workshopServices.Count != model.Services.Count)
                {
                    return BadRequest("not-found");
                }

                var quotation = new Quotation
                {
                    CustomerId = model.CustomerId,
                    WorkshopId = model.WorkshopId,
                    VehicleId = model.VehicleId,
                    Description = model.Description,
                    PriceOfLabor = model.PriceOfLabor,
                    PriceOfSpareParts = model.PriceOfSpareParts,
                    Status = model.Status,
                    
                };

                _context.Quotations.Add(quotation);
                await _context.SaveChangesAsync();

                quotation.Services = model.Services.Select(s =>
                {
                    var workshopService = workshopServices.FirstOrDefault(ws => ws.Id == s.ServiceId);
                    if (workshopService == null)
                        throw new Exception($"ServiceId {s.ServiceId} not found for this workshop.");

                    return new QuotationService
                    {
                        QuotationId = quotation.Id,
                        WorkshopServiceId = workshopService.Id,
                        Price = s.Price ?? 0
                    };
                }).ToList();

                await _context.SaveChangesAsync();

                var vehicleBrand = vehicle.BrandId == -1 ? vehicle.OtherBrand : vehicle.Brand.Name;
                var vehicleModel = vehicle.VehicleModelId == -1 ? vehicle.OtherBrand : vehicle.VehicleModel.Model;
                
                if(model.UserType == UserType.Customer)
                {
                    // WorkshopInbox
                    var workshopInbox = new WorkshopInbox
                    {
                        WorkshopId = model.WorkshopId,
                        CustomerId = model.CustomerId,
                        VehicleId = model.VehicleId,
                        ParentModelType = "Quotation",
                        ParentModelId = quotation.Id,
                        Title = $"Cotización del vehículo {vehicleBrand} - {vehicleModel} - {vehicle.Year} - {vehicle.Plates}",
                        Details = quotation.Description,
                    };
                    _context.WorkshopInbox.Add(workshopInbox);
                    await _context.SaveChangesAsync();

                    // TODO: Activar cuando se implementen las notificaciones push Firebase
                    // Enviar notificación push al taller
                    //await _firebaseNotificationService.SendNotificationToWorkshopAsync(
                    //    model.WorkshopId,
                    //    workshopInbox.Title,
                    //    workshopInbox.Details,
                    //    workshopInbox.ParentModelType,
                    //    new Dictionary<string, string> { { "quotationId", quotation.Id.ToString() } }
                    //);
                } else
                {
                    // Customer Notifications
                    var notification = new Notifications
                    {
                        UserId = model.CustomerId,
                        UserType = UserType.Customer,
                        Title = $"Cotización del vehículo {vehicleBrand} - {vehicleModel} - {vehicle.Year} - {vehicle.Plates}",
                        Content = quotation.Description,
                        Event = "QuotationCreated"
                    };
                    _context.Notifications.Add(notification);
                    await _context.SaveChangesAsync();

                    // TODO: Activar cuando se implementen las notificaciones push Firebase
                    // Enviar notificación push al cliente
                    //await _firebaseNotificationService.SendNotificationToCustomerAsync(
                    //    model.CustomerId,
                    //    notification.Title,
                    //    notification.Content,
                    //    notification.Event,
                    //    new Dictionary<string, string> { { "quotationId", quotation.Id.ToString() } }
                    //);
                }

                return Ok("quotation-created");
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPut("{id}")]
        [AuthorizeEmployee("Administrador")]
        public async Task<ActionResult> UpdateQuotation(int id, [FromBody] PutQuotationDto model)
        {
            try 
            { 
                var quotation = await _context.Quotations
                    .Include(q => q.Services)
                    .FirstOrDefaultAsync(q => q.Id == id);

                if (quotation == null) return NotFound("not-found");

                quotation.Description = model.Description;
                quotation.PriceOfLabor = model.PriceOfLabor;
                quotation.PriceOfSpareParts = model.PriceOfSpareParts;
                quotation.Status = model.Status;

                // Actualizar servicios
                _context.QuotationServices.RemoveRange(quotation.Services);
                quotation.Services = model.Services.Select(s => new QuotationService
                {
                    QuotationId = id,
                    WorkshopServiceId = s.ServiceId,
                    Price = s.Price
                }).ToList();

                if (model.UserType == UserType.Customer)
                {
                    if (quotation.Status == "Canceled")
                    {
                        // WorkshopInbox
                        var workshopInbox = new WorkshopInbox
                        {
                            WorkshopId = model.WorkshopId,
                            CustomerId = model.CustomerId,
                            VehicleId = model.VehicleId,
                            ParentModelType = "Quotation",
                            ParentModelId = quotation.Id,
                            Title = $"Cambio de estatus en cotización",
                            Details = "La cotización ha sido cancelada por el cliente.",
                        };
                        _context.WorkshopInbox.Add(workshopInbox);
                        await _context.SaveChangesAsync();

                        // TODO: Activar cuando se implementen las notificaciones push Firebase
                        // Enviar notificación push al taller
                        //await _firebaseNotificationService.SendNotificationToWorkshopAsync(
                        //    model.WorkshopId,
                        //    workshopInbox.Title,
                        //    workshopInbox.Details,
                        //    workshopInbox.ParentModelType,
                        //    new Dictionary<string, string> { { "quotationId", quotation.Id.ToString() } }
                        //);
                    } else if (quotation.Status == "Confirmed")
                    {
                        // WorkshopInbox
                        var workshopInbox = new WorkshopInbox
                        {
                            WorkshopId = model.WorkshopId,
                            CustomerId = model.CustomerId,
                            VehicleId = model.VehicleId,
                            ParentModelType = "Quotation",
                            ParentModelId = quotation.Id,
                            Title = $"Cambio de estatus en cotización",
                            Details = "La cotización ha sido aprobada por el cliente.",
                        };
                        _context.WorkshopInbox.Add(workshopInbox);
                        await _context.SaveChangesAsync();

                        // TODO: Activar cuando se implementen las notificaciones push Firebase
                        // Enviar notificación push al taller
                        //await _firebaseNotificationService.SendNotificationToWorkshopAsync(
                        //    model.WorkshopId,
                        //    workshopInbox.Title,
                        //    workshopInbox.Details,
                        //    workshopInbox.ParentModelType,
                        //    new Dictionary<string, string> { { "quotationId", quotation.Id.ToString() } }
                        //);
                    }
                } else
                {
                    // Customer Notifications
                    var notification = new Notifications
                    {
                        UserId = model.CustomerId,
                        UserType = UserType.Customer,
                        Title = $"Cambio de estatus en cotización",
                        Content = "La cotización ha sido modificada, favor de revisarla.",
                        Event = "QuotationUpdated"
                    };
                    _context.Notifications.Add(notification);
                    await _context.SaveChangesAsync();

                    // TODO: Activar cuando se implementen las notificaciones push Firebase
                    // Enviar notificación push al cliente
                    //await _firebaseNotificationService.SendNotificationToCustomerAsync(
                    //    model.CustomerId,
                    //    notification.Title,
                    //    notification.Content,
                    //    notification.Event,
                    //    new Dictionary<string, string> { { "quotationId", quotation.Id.ToString() } }
                    //);
                }

                await _context.SaveChangesAsync();
                return Ok("quotation-updated");
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteQuotation(int id)
        {
            try 
            { 
                var quotation = await _context.Quotations.FindAsync(id);
                if (quotation == null) return NotFound("not-found");

                _context.Quotations.Remove(quotation);
                await _context.SaveChangesAsync();

                return Ok("quotation-deleted");
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPost("get-workshop-customer-vehicle-quotation")]
        public async Task<ActionResult<ICollection<QuotationResponseDto>>> GetQuotationsByWorkshopCustomerVehicle(CustomerWorkshopVehicleBodyDto model)
        {
            try
            {
                var quotations = await _context.Quotations
                    .Where(q => q.WorkshopId == model.WorkshopId && q.CustomerId == model.CustomerId && q.VehicleId == model.VehicleId)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.Brand)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.VehicleModel)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.VehicleVersion)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.VehicleType)
                    .Include(q => q.Workshop)
                    .Include(q => q.Customer)
                    .ToListAsync();

                if (quotations == null)
                {
                    return NotFound("not-found");
                }

                var workshopServices = await _context.WorkshopServices
                    .Where(ws => ws.WorkshopId == model.WorkshopId && !ws.IsDeleted)
                    .Include(ws => ws.Service)
                    .ToListAsync();

                var quotationsResponseDto = new List<QuotationResponseDto>();
                foreach (var quotation in quotations)
                {
                    var quotationServices = await _context.QuotationServices
                    .Where(qs => qs.QuotationId == quotation.Id)
                    .Include(ws => ws.Service)
                    .ToListAsync();

                    var vehicleBrand = quotation.Vehicle.BrandId == -1 ? quotation.Vehicle.OtherBrand : quotation.Vehicle.Brand.Name;
                    var vehicleModel = quotation.Vehicle.VehicleModelId == -1 ? quotation.Vehicle.OtherVehicleModel : quotation.Vehicle.VehicleModel.Model;

                    var quotationResponseDto = new QuotationResponseDto
                    {
                        Id = quotation.Id,
                        WorkshopId = quotation.WorkshopId,
                        WorkshopName = quotation.Workshop.WorkshopName,
                        CustomerId = quotation.CustomerId,
                        CustomerName = quotation.Customer.FullName,
                        Vehicle = new VehicleResponseDto
                        {
                            Id = quotation.Vehicle.Id,
                            Brand = vehicleBrand,
                            Model = vehicleModel,
                            Version = quotation.Vehicle.VehicleVersionId == -1 ? quotation.Vehicle.OtherVehicleVersion : quotation.Vehicle.VehicleVersion.Version,
                            Type = quotation.Vehicle.VehicleTypeId == -1 ? quotation.Vehicle.OtherVehicleType : quotation.Vehicle.VehicleType.Type,
                            Year = quotation.Vehicle.Year,
                            SerialNumber = quotation.Vehicle.SerialNumber,
                            Color = quotation.Vehicle.Color,
                            Plates = quotation.Vehicle.Plates,
                            RimRubber = quotation.Vehicle.RimRubber,
                            Kms = quotation.Vehicle.Kms,
                            VehicleFormat = quotation.Vehicle.VehicleFormat,
                        },
                        Title = $"{vehicleBrand} - {vehicleModel} - {quotation.Vehicle.Year} - {quotation.Vehicle.Plates}",
                        Description = quotation.Description,
                        PriceOfLabor = quotation.PriceOfLabor,
                        PriceOfSpareParts = quotation.PriceOfSpareParts,
                        Status = quotation.Status,
                        CreatedAt = quotation.CreatedAt,
                        Services = quotationServices.Select(qs => new WorkshopServiceResponseDto
                        {
                            ServiceId = qs.WorkshopServiceId,
                            ServiceName = qs.Service.Service.Name,
                            Price = qs.Price ?? 0,
                        }).ToList()
                    };

                    quotationsResponseDto.Add(quotationResponseDto);
                }

                return Ok(quotationsResponseDto);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPost("get-workshop-customer-quotation")]
        public async Task<ActionResult<ICollection<QuotationResponseDto>>> GetQuotationsByWorkshopCustomer(CustomerWorkshopBodyDto model)
        {
            try
            {
                var quotations = await _context.Quotations
                    .Where(q => q.WorkshopId == model.WorkshopId && q.CustomerId == model.CustomerId)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.Brand)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.VehicleModel)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.VehicleVersion)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.VehicleType)
                    .Include(q => q.Workshop)
                    .Include(q => q.Customer)
                    .ToListAsync();

                if (quotations == null)
                {
                    return NotFound("not-found");
                }

                var workshopServices = await _context.WorkshopServices
                    .Where(ws => ws.WorkshopId == model.WorkshopId && !ws.IsDeleted)
                    .Include(ws => ws.Service)
                    .ToListAsync();

                var quotationsResponseDto = new List<QuotationResponseDto>();
                foreach (var quotation in quotations)
                {
                    var quotationServices = await _context.QuotationServices
                    .Where(qs => qs.QuotationId == quotation.Id)
                    .Include(ws => ws.Service)
                    .ToListAsync();

                    var vehicleBrand = quotation.Vehicle.BrandId == -1 ? quotation.Vehicle.OtherBrand : quotation.Vehicle.Brand.Name;
                    var vehicleModel = quotation.Vehicle.VehicleModelId == -1 ? quotation.Vehicle.OtherVehicleModel : quotation.Vehicle.VehicleModel.Model;

                    var quotationResponseDto = new QuotationResponseDto
                    {
                        Id = quotation.Id,
                        WorkshopId = quotation.WorkshopId,
                        WorkshopName = quotation.Workshop.WorkshopName,
                        CustomerId = quotation.CustomerId,
                        CustomerName = quotation.Customer.FullName,
                        Vehicle = new VehicleResponseDto
                        {
                            Id = quotation.Vehicle.Id,
                            Brand = vehicleBrand,
                            Model = vehicleModel,
                            Version = quotation.Vehicle.VehicleVersionId == -1 ? quotation.Vehicle.OtherVehicleVersion : quotation.Vehicle.VehicleVersion.Version,
                            Type = quotation.Vehicle.VehicleTypeId == -1 ? quotation.Vehicle.OtherVehicleType : quotation.Vehicle.VehicleType.Type,
                            Year = quotation.Vehicle.Year,
                            SerialNumber = quotation.Vehicle.SerialNumber,
                            Color = quotation.Vehicle.Color,
                            Plates = quotation.Vehicle.Plates,
                            RimRubber = quotation.Vehicle.RimRubber,
                            Kms = quotation.Vehicle.Kms,
                            VehicleFormat = quotation.Vehicle.VehicleFormat,
                        },
                        Title = $"{vehicleBrand} - {vehicleModel} - {quotation.Vehicle.Year} - {quotation.Vehicle.Plates}",
                        Description = quotation.Description,
                        PriceOfLabor = quotation.PriceOfLabor,
                        PriceOfSpareParts = quotation.PriceOfSpareParts,
                        Status = quotation.Status,
                        CreatedAt = quotation.CreatedAt,
                        Services = quotationServices.Select(qs => new WorkshopServiceResponseDto
                        {
                            ServiceId = qs.WorkshopServiceId,
                            ServiceName = qs.Service.Service.Name,
                            Price = qs.Price ?? 0,
                        }).ToList()
                    };

                    quotationsResponseDto.Add(quotationResponseDto);
                }

                return Ok(quotationsResponseDto);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpGet("workshop-canceled/{workshopId}")]
        public async Task<ActionResult<QuotationResponseListDto>> GetQuotationsCanceledByWorkshop(Guid workshopId)
        {
            try
            {
                var quotations = await _context.Quotations
                    .Where(q => q.WorkshopId == workshopId && q.Status == "Canceled" || q.Status == "Expired")
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.Brand)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.VehicleModel)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.VehicleVersion)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.VehicleType)
                    .Include(q => q.Workshop)
                    .Include(q => q.Customer)
                    .ToListAsync();

                if (quotations == null)
                {
                    return NotFound("not-found");
                }

                var workshopServices = await _context.WorkshopServices
                    .Where(ws => ws.WorkshopId == workshopId && !ws.IsDeleted)
                    .Include(ws => ws.Service)
                .ToListAsync();

                var quotationsResponseDto = new List<QuotationResponseDto>();
                foreach (var quotation in quotations)
                {
                    var quotationServices = await _context.QuotationServices
                    .Where(qs => qs.QuotationId == quotation.Id)
                    .Include(ws => ws.Service)
                    .ToListAsync();

                    var vehicleBrand = quotation.Vehicle.BrandId == -1 ? quotation.Vehicle.OtherBrand : quotation.Vehicle.Brand.Name;
                    var vehicleModel = quotation.Vehicle.VehicleModelId == -1 ? quotation.Vehicle.OtherVehicleModel : quotation.Vehicle.VehicleModel.Model;

                    var quotationResponseDto = new QuotationResponseDto
                    {
                        Id = quotation.Id,
                        WorkshopId = quotation.WorkshopId,
                        WorkshopName = quotation.Workshop.WorkshopName,
                        CustomerId = quotation.CustomerId,
                        CustomerName = quotation.Customer.FullName,
                        Vehicle = new VehicleResponseDto
                        {
                            Id = quotation.Vehicle.Id,
                            Brand = vehicleBrand,
                            Model = vehicleModel,
                            Version = quotation.Vehicle.VehicleVersionId == -1 ? quotation.Vehicle.OtherVehicleVersion : quotation.Vehicle.VehicleVersion.Version,
                            Type = quotation.Vehicle.VehicleTypeId == -1 ? quotation.Vehicle.OtherVehicleType : quotation.Vehicle.VehicleType.Type,
                            Year = quotation.Vehicle.Year,
                            SerialNumber = quotation.Vehicle.SerialNumber,
                            Color = quotation.Vehicle.Color,
                            Plates = quotation.Vehicle.Plates,
                            RimRubber = quotation.Vehicle.RimRubber,
                            Kms = quotation.Vehicle.Kms,
                            VehicleFormat = quotation.Vehicle.VehicleFormat,
                        },
                        Title = $"{vehicleBrand} - {vehicleModel} - {quotation.Vehicle.Year} - {quotation.Vehicle.Plates}",
                        Description = quotation.Description,
                        PriceOfLabor = quotation.PriceOfLabor,
                        PriceOfSpareParts = quotation.PriceOfSpareParts,
                        Status = quotation.Status,
                        CreatedAt = quotation.CreatedAt,
                        Services = quotationServices.Any()
                                    ? quotationServices
                                        .Where(qs => qs.Service != null && qs.Service.Service != null)
                                        .Select(qs => new WorkshopServiceResponseDto
                                        {
                                            ServiceId = qs.WorkshopServiceId,
                                            ServiceName = qs.Service.Service.Name,
                                            Price = qs.Price ?? 0
                                        }).ToList()
                                    : null
                    };

                    quotationsResponseDto.Add(quotationResponseDto);
                }

                var quotationListDto = new QuotationResponseListDto
                {
                    Count = quotations.Count(),
                    Quotations = quotationsResponseDto
                };

                return Ok(quotationListDto);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPost("workshop-canceled-pager/{workshopId}")]
        public async Task<ActionResult<PagerResponseDto<QuotationResponseDto>>> GetQuotationsCanceledByWorkshopPaged(Guid workshopId, [FromBody] PagerBodyDto pager)
        {
            try
            {
                var query = _context.Quotations
                    .Where(q => q.WorkshopId == workshopId && (q.Status == "Canceled" || q.Status == "Expired"))
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.Brand)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.VehicleModel)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.VehicleVersion)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.VehicleType)
                    .Include(q => q.Workshop)
                    .Include(q => q.Customer)
                    .OrderByDescending(q => q.CreatedAt);

                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalCount / pager.PageSize);

                var pagedQuotations = await query
                    .Skip((pager.PageNumber - 1) * pager.PageSize)
                    .Take(pager.PageSize)
                    .ToListAsync();

                if (!pagedQuotations.Any())
                {
                    return NotFound("not-found");
                }

                var quotationsResponseDto = new List<QuotationResponseDto>();

                foreach (var quotation in pagedQuotations)
                {
                    var quotationServices = await _context.QuotationServices
                        .Where(qs => qs.QuotationId == quotation.Id)
                        .Include(qs => qs.Service)
                            .ThenInclude(ws => ws.Service)
                        .ToListAsync();

                    var vehicleBrand = quotation.Vehicle.BrandId == -1 ? quotation.Vehicle.OtherBrand : quotation.Vehicle.Brand.Name;
                    var vehicleModel = quotation.Vehicle.VehicleModelId == -1 ? quotation.Vehicle.OtherVehicleModel : quotation.Vehicle.VehicleModel.Model;

                    var quotationResponseDto = new QuotationResponseDto
                    {
                        Id = quotation.Id,
                        WorkshopId = quotation.WorkshopId,
                        WorkshopName = quotation.Workshop.WorkshopName,
                        CustomerId = quotation.CustomerId,
                        CustomerName = quotation.Customer.FullName,
                        Vehicle = new VehicleResponseDto
                        {
                            Id = quotation.Vehicle.Id,
                            Brand = vehicleBrand,
                            Model = vehicleModel,
                            Version = quotation.Vehicle.VehicleVersionId == -1 ? quotation.Vehicle.OtherVehicleVersion : quotation.Vehicle.VehicleVersion.Version,
                            Type = quotation.Vehicle.VehicleTypeId == -1 ? quotation.Vehicle.OtherVehicleType : quotation.Vehicle.VehicleType.Type,
                            Year = quotation.Vehicle.Year,
                            SerialNumber = quotation.Vehicle.SerialNumber,
                            Color = quotation.Vehicle.Color,
                            Plates = quotation.Vehicle.Plates,
                            RimRubber = quotation.Vehicle.RimRubber,
                            Kms = quotation.Vehicle.Kms,
                            VehicleFormat = quotation.Vehicle.VehicleFormat,
                        },
                        Title = $"{vehicleBrand} - {vehicleModel} - {quotation.Vehicle.Year} - {quotation.Vehicle.Plates}",
                        Description = quotation.Description,
                        PriceOfLabor = quotation.PriceOfLabor,
                        PriceOfSpareParts = quotation.PriceOfSpareParts,
                        Status = quotation.Status,
                        CreatedAt = quotation.CreatedAt,
                        Services = quotationServices.Any()
                                    ? quotationServices
                                        .Where(qs => qs.Service?.Service != null)
                                        .Select(qs => new WorkshopServiceResponseDto
                                        {
                                            ServiceId = qs.WorkshopServiceId,
                                            ServiceName = qs.Service.Service.Name,
                                            Price = qs.Price ?? 0
                                        }).ToList()
                                    : null
                    };

                    quotationsResponseDto.Add(quotationResponseDto);
                }

                var response = new PagerResponseDto<QuotationResponseDto>
                {
                    CurrentPage = pager.PageNumber,
                    TotalPages = totalPages,
                    Elements = quotationsResponseDto
                };

                return Ok(response);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPost("customer-canceled-pager/{customerId}")]
        public async Task<ActionResult<PagerResponseDto<QuotationResponseDto>>> GetQuotationsCanceledByCustomerPaged(Guid customerId, [FromBody] PagerBodyDto pager)
        {
            try
            {
                var query = _context.Quotations
                    .Where(q => q.CustomerId == customerId && (q.Status == "Canceled" || q.Status == "Expired"))
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.Brand)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.VehicleModel)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.VehicleVersion)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.VehicleType)
                    .Include(q => q.Workshop)
                    .Include(q => q.Customer)
                    .OrderByDescending(q => q.CreatedAt);

                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalCount / pager.PageSize);

                var pagedQuotations = await query
                    .Skip((pager.PageNumber - 1) * pager.PageSize)
                    .Take(pager.PageSize)
                    .ToListAsync();

                if (!pagedQuotations.Any())
                {
                    return NotFound("not-found");
                }

                var quotationsResponseDto = new List<QuotationResponseDto>();

                foreach (var quotation in pagedQuotations)
                {
                    var quotationServices = await _context.QuotationServices
                        .Where(qs => qs.QuotationId == quotation.Id)
                        .Include(qs => qs.Service)
                            .ThenInclude(ws => ws.Service)
                        .ToListAsync();

                    var vehicleBrand = quotation.Vehicle.BrandId == -1 ? quotation.Vehicle.OtherBrand : quotation.Vehicle.Brand.Name;
                    var vehicleModel = quotation.Vehicle.VehicleModelId == -1 ? quotation.Vehicle.OtherVehicleModel : quotation.Vehicle.VehicleModel.Model;

                    var quotationResponseDto = new QuotationResponseDto
                    {
                        Id = quotation.Id,
                        WorkshopId = quotation.WorkshopId,
                        WorkshopName = quotation.Workshop.WorkshopName,
                        CustomerId = quotation.CustomerId,
                        CustomerName = quotation.Customer.FullName,
                        Vehicle = new VehicleResponseDto
                        {
                            Id = quotation.Vehicle.Id,
                            Brand = vehicleBrand,
                            Model = vehicleModel,
                            Version = quotation.Vehicle.VehicleVersionId == -1 ? quotation.Vehicle.OtherVehicleVersion : quotation.Vehicle.VehicleVersion.Version,
                            Type = quotation.Vehicle.VehicleTypeId == -1 ? quotation.Vehicle.OtherVehicleType : quotation.Vehicle.VehicleType.Type,
                            Year = quotation.Vehicle.Year,
                            SerialNumber = quotation.Vehicle.SerialNumber,
                            Color = quotation.Vehicle.Color,
                            Plates = quotation.Vehicle.Plates,
                            RimRubber = quotation.Vehicle.RimRubber,
                            Kms = quotation.Vehicle.Kms,
                            VehicleFormat = quotation.Vehicle.VehicleFormat,
                        },
                        Title = $"{vehicleBrand} - {vehicleModel} - {quotation.Vehicle.Year} - {quotation.Vehicle.Plates}",
                        Description = quotation.Description,
                        PriceOfLabor = quotation.PriceOfLabor,
                        PriceOfSpareParts = quotation.PriceOfSpareParts,
                        Status = quotation.Status,
                        CreatedAt = quotation.CreatedAt,
                        Services = quotationServices.Any()
                                    ? quotationServices
                                        .Where(qs => qs.Service?.Service != null)
                                        .Select(qs => new WorkshopServiceResponseDto
                                        {
                                            ServiceId = qs.WorkshopServiceId,
                                            ServiceName = qs.Service.Service.Name,
                                            Price = qs.Price ?? 0
                                        }).ToList()
                                    : null
                    };

                    quotationsResponseDto.Add(quotationResponseDto);
                }

                var response = new PagerResponseDto<QuotationResponseDto>
                {
                    CurrentPage = pager.PageNumber,
                    TotalPages = totalPages,
                    Elements = quotationsResponseDto
                };

                return Ok(response);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpGet("workshop-pending/{workshopId}")]
        public async Task<ActionResult<QuotationResponseListDto>> GetQuotationsPendingByWorkshop(Guid workshopId)
        {
            try
            {
                var quotations = await _context.Quotations
                    .Where(q => q.WorkshopId == workshopId && q.Status == "InProgress" || q.Status == "Quoted")
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.Brand)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.VehicleModel)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.VehicleVersion)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.VehicleType)
                    .Include(q => q.Workshop)
                    .Include(q => q.Customer)
                    .ToListAsync();

                if (quotations == null)
                {
                    return NotFound("not-found");
                }

                var workshopServices = await _context.WorkshopServices
                    .Where(ws => ws.WorkshopId == workshopId && !ws.IsDeleted)
                    .Include(ws => ws.Service)
                .ToListAsync();

                var quotationsResponseDto = new List<QuotationResponseDto>();
                foreach (var quotation in quotations)
                {
                    var quotationServices = await _context.QuotationServices
                    .Where(qs => qs.QuotationId == quotation.Id)
                    .Include(ws => ws.Service)
                    .ToListAsync();

                    var vehicleBrand = quotation.Vehicle.BrandId == -1 ? quotation.Vehicle.OtherBrand : quotation.Vehicle.Brand.Name;
                    var vehicleModel = quotation.Vehicle.VehicleModelId == -1 ? quotation.Vehicle.OtherVehicleModel : quotation.Vehicle.VehicleModel.Model;

                    var quotationResponseDto = new QuotationResponseDto
                    {
                        Id = quotation.Id,
                        WorkshopId = quotation.WorkshopId,
                        WorkshopName = quotation.Workshop.WorkshopName,
                        CustomerId = quotation.CustomerId,
                        CustomerName = quotation.Customer.FullName,
                        Vehicle = new VehicleResponseDto
                        {
                            Id = quotation.Vehicle.Id,
                            Brand = vehicleBrand,
                            Model = vehicleModel,
                            Version = quotation.Vehicle.VehicleVersionId == -1 ? quotation.Vehicle.OtherVehicleVersion : quotation.Vehicle.VehicleVersion.Version,
                            Type = quotation.Vehicle.VehicleTypeId == -1 ? quotation.Vehicle.OtherVehicleType : quotation.Vehicle.VehicleType.Type,
                            Year = quotation.Vehicle.Year,
                            SerialNumber = quotation.Vehicle.SerialNumber,
                            Color = quotation.Vehicle.Color,
                            Plates = quotation.Vehicle.Plates,
                            RimRubber = quotation.Vehicle.RimRubber,
                            Kms = quotation.Vehicle.Kms,
                            VehicleFormat = quotation.Vehicle.VehicleFormat,
                        },
                        Title = $"{vehicleBrand} - {vehicleModel} - {quotation.Vehicle.Year} - {quotation.Vehicle.Plates}",
                        Description = quotation.Description,
                        PriceOfLabor = quotation.PriceOfLabor,
                        PriceOfSpareParts = quotation.PriceOfSpareParts,
                        Status = quotation.Status,
                        CreatedAt = quotation.CreatedAt,
                        Services = quotationServices.Any()
                                    ? quotationServices
                                        .Where(qs => qs.Service != null && qs.Service.Service != null)
                                        .Select(qs => new WorkshopServiceResponseDto
                                        {
                                            ServiceId = qs.WorkshopServiceId,
                                            ServiceName = qs.Service.Service.Name,
                                            Price = qs.Price ?? 0
                                        }).ToList()
                                    : null
                    };

                    quotationsResponseDto.Add(quotationResponseDto);
                }

                var quotationListDto = new QuotationResponseListDto
                {
                    Count = quotations.Count(),
                    Quotations = quotationsResponseDto
                };

                return Ok(quotationListDto);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPost("workshop-pending-pager/{workshopId}")]
        public async Task<ActionResult<PagerResponseDto<QuotationResponseDto>>> GetQuotationsPendingByWorkshopPaged(Guid workshopId, [FromBody] PagerBodyDto pager)
        {
            try
            {
                var query = _context.Quotations
                    .Where(q => q.WorkshopId == workshopId && (q.Status == "InProgress" || q.Status == "Quoted"))
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.Brand)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.VehicleModel)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.VehicleVersion)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.VehicleType)
                    .Include(q => q.Workshop)
                    .Include(q => q.Customer)
                    .OrderByDescending(q => q.CreatedAt);

                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalCount / pager.PageSize);

                var pagedQuotations = await query
                    .Skip((pager.PageNumber - 1) * pager.PageSize)
                    .Take(pager.PageSize)
                    .ToListAsync();

                if (!pagedQuotations.Any())
                {
                    return NotFound("not-found");
                }

                var quotationsResponseDto = new List<QuotationResponseDto>();

                foreach (var quotation in pagedQuotations)
                {
                    var quotationServices = await _context.QuotationServices
                        .Where(qs => qs.QuotationId == quotation.Id)
                        .Include(qs => qs.Service)
                            .ThenInclude(ws => ws.Service)
                        .ToListAsync();

                    var vehicleBrand = quotation.Vehicle.BrandId == -1 ? quotation.Vehicle.OtherBrand : quotation.Vehicle.Brand.Name;
                    var vehicleModel = quotation.Vehicle.VehicleModelId == -1 ? quotation.Vehicle.OtherVehicleModel : quotation.Vehicle.VehicleModel.Model;

                    var quotationResponseDto = new QuotationResponseDto
                    {
                        Id = quotation.Id,
                        WorkshopId = quotation.WorkshopId,
                        WorkshopName = quotation.Workshop.WorkshopName,
                        CustomerId = quotation.CustomerId,
                        CustomerName = quotation.Customer.FullName,
                        Vehicle = new VehicleResponseDto
                        {
                            Id = quotation.Vehicle.Id,
                            Brand = vehicleBrand,
                            Model = vehicleModel,
                            Version = quotation.Vehicle.VehicleVersionId == -1 ? quotation.Vehicle.OtherVehicleVersion : quotation.Vehicle.VehicleVersion.Version,
                            Type = quotation.Vehicle.VehicleTypeId == -1 ? quotation.Vehicle.OtherVehicleType : quotation.Vehicle.VehicleType.Type,
                            Year = quotation.Vehicle.Year,
                            SerialNumber = quotation.Vehicle.SerialNumber,
                            Color = quotation.Vehicle.Color,
                            Plates = quotation.Vehicle.Plates,
                            RimRubber = quotation.Vehicle.RimRubber,
                            Kms = quotation.Vehicle.Kms,
                            VehicleFormat = quotation.Vehicle.VehicleFormat,
                        },
                        Title = $"{vehicleBrand} - {vehicleModel} - {quotation.Vehicle.Year} - {quotation.Vehicle.Plates}",
                        Description = quotation.Description,
                        PriceOfLabor = quotation.PriceOfLabor,
                        PriceOfSpareParts = quotation.PriceOfSpareParts,
                        Status = quotation.Status,
                        CreatedAt = quotation.CreatedAt,
                        Services = quotationServices.Any()
                                    ? quotationServices
                                        .Where(qs => qs.Service?.Service != null)
                                        .Select(qs => new WorkshopServiceResponseDto
                                        {
                                            ServiceId = qs.WorkshopServiceId,
                                            ServiceName = qs.Service.Service.Name,
                                            Price = qs.Price ?? 0
                                        }).ToList()
                                    : null
                    };

                    quotationsResponseDto.Add(quotationResponseDto);
                }

                var response = new PagerResponseDto<QuotationResponseDto>
                {
                    CurrentPage = pager.PageNumber,
                    TotalPages = totalPages,
                    Elements = quotationsResponseDto
                };

                return Ok(response);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPost("workshop-in-progress-pager/{workshopId}")]
        public async Task<ActionResult<PagerResponseDto<QuotationResponseDto>>> GetQuotationsInProgressByWorkshopPaged(Guid workshopId, [FromBody] PagerBodyDto pager)
        {
            try
            {
                var query = _context.Quotations
                    .Where(q => q.WorkshopId == workshopId && (q.Status == "InProgress"))
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.Brand)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.VehicleModel)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.VehicleVersion)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.VehicleType)
                    .Include(q => q.Workshop)
                    .Include(q => q.Customer)
                    .OrderByDescending(q => q.CreatedAt);

                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalCount / pager.PageSize);

                var pagedQuotations = await query
                    .Skip((pager.PageNumber - 1) * pager.PageSize)
                    .Take(pager.PageSize)
                    .ToListAsync();

                if (!pagedQuotations.Any())
                {
                    return NotFound("not-found");
                }

                var quotationsResponseDto = new List<QuotationResponseDto>();

                foreach (var quotation in pagedQuotations)
                {
                    var quotationServices = await _context.QuotationServices
                        .Where(qs => qs.QuotationId == quotation.Id)
                        .Include(qs => qs.Service)
                            .ThenInclude(ws => ws.Service)
                        .ToListAsync();

                    var vehicleBrand = quotation.Vehicle.BrandId == -1 ? quotation.Vehicle.OtherBrand : quotation.Vehicle.Brand.Name;
                    var vehicleModel = quotation.Vehicle.VehicleModelId == -1 ? quotation.Vehicle.OtherVehicleModel : quotation.Vehicle.VehicleModel.Model;

                    var quotationResponseDto = new QuotationResponseDto
                    {
                        Id = quotation.Id,
                        WorkshopId = quotation.WorkshopId,
                        WorkshopName = quotation.Workshop.WorkshopName,
                        CustomerId = quotation.CustomerId,
                        CustomerName = quotation.Customer.FullName,
                        Vehicle = new VehicleResponseDto
                        {
                            Id = quotation.Vehicle.Id,
                            Brand = vehicleBrand,
                            Model = vehicleModel,
                            Version = quotation.Vehicle.VehicleVersionId == -1 ? quotation.Vehicle.OtherVehicleVersion : quotation.Vehicle.VehicleVersion.Version,
                            Type = quotation.Vehicle.VehicleTypeId == -1 ? quotation.Vehicle.OtherVehicleType : quotation.Vehicle.VehicleType.Type,
                            Year = quotation.Vehicle.Year,
                            SerialNumber = quotation.Vehicle.SerialNumber,
                            Color = quotation.Vehicle.Color,
                            Plates = quotation.Vehicle.Plates,
                            RimRubber = quotation.Vehicle.RimRubber,
                            Kms = quotation.Vehicle.Kms,
                            VehicleFormat = quotation.Vehicle.VehicleFormat,
                        },
                        Title = $"{vehicleBrand} - {vehicleModel} - {quotation.Vehicle.Year} - {quotation.Vehicle.Plates}",
                        Description = quotation.Description,
                        PriceOfLabor = quotation.PriceOfLabor,
                        PriceOfSpareParts = quotation.PriceOfSpareParts,
                        Status = quotation.Status,
                        CreatedAt = quotation.CreatedAt,
                        Services = quotationServices.Any()
                                    ? quotationServices
                                        .Where(qs => qs.Service?.Service != null)
                                        .Select(qs => new WorkshopServiceResponseDto
                                        {
                                            ServiceId = qs.WorkshopServiceId,
                                            ServiceName = qs.Service.Service.Name,
                                            Price = qs.Price ?? 0
                                        }).ToList()
                                    : null
                    };

                    quotationsResponseDto.Add(quotationResponseDto);
                }

                var response = new PagerResponseDto<QuotationResponseDto>
                {
                    CurrentPage = pager.PageNumber,
                    TotalPages = totalPages,
                    Elements = quotationsResponseDto
                };

                return Ok(response);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPost("workshop-quoted-pager/{workshopId}")]
        public async Task<ActionResult<PagerResponseDto<QuotationResponseDto>>> GetQuotationsQuotedByWorkshopPaged(Guid workshopId, [FromBody] PagerBodyDto pager)
        {
            try
            {
                var query = _context.Quotations
                    .Where(q => q.WorkshopId == workshopId && (q.Status == "Quoted"))
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.Brand)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.VehicleModel)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.VehicleVersion)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.VehicleType)
                    .Include(q => q.Workshop)
                    .Include(q => q.Customer)
                    .OrderByDescending(q => q.CreatedAt);

                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalCount / pager.PageSize);

                var pagedQuotations = await query
                    .Skip((pager.PageNumber - 1) * pager.PageSize)
                    .Take(pager.PageSize)
                    .ToListAsync();

                if (!pagedQuotations.Any())
                {
                    return NotFound("not-found");
                }

                var quotationsResponseDto = new List<QuotationResponseDto>();

                foreach (var quotation in pagedQuotations)
                {
                    var quotationServices = await _context.QuotationServices
                        .Where(qs => qs.QuotationId == quotation.Id)
                        .Include(qs => qs.Service)
                            .ThenInclude(ws => ws.Service)
                        .ToListAsync();

                    var vehicleBrand = quotation.Vehicle.BrandId == -1 ? quotation.Vehicle.OtherBrand : quotation.Vehicle.Brand.Name;
                    var vehicleModel = quotation.Vehicle.VehicleModelId == -1 ? quotation.Vehicle.OtherVehicleModel : quotation.Vehicle.VehicleModel.Model;

                    var quotationResponseDto = new QuotationResponseDto
                    {
                        Id = quotation.Id,
                        WorkshopId = quotation.WorkshopId,
                        WorkshopName = quotation.Workshop.WorkshopName,
                        CustomerId = quotation.CustomerId,
                        CustomerName = quotation.Customer.FullName,
                        Vehicle = new VehicleResponseDto
                        {
                            Id = quotation.Vehicle.Id,
                            Brand = vehicleBrand,
                            Model = vehicleModel,
                            Version = quotation.Vehicle.VehicleVersionId == -1 ? quotation.Vehicle.OtherVehicleVersion : quotation.Vehicle.VehicleVersion.Version,
                            Type = quotation.Vehicle.VehicleTypeId == -1 ? quotation.Vehicle.OtherVehicleType : quotation.Vehicle.VehicleType.Type,
                            Year = quotation.Vehicle.Year,
                            SerialNumber = quotation.Vehicle.SerialNumber,
                            Color = quotation.Vehicle.Color,
                            Plates = quotation.Vehicle.Plates,
                            RimRubber = quotation.Vehicle.RimRubber,
                            Kms = quotation.Vehicle.Kms,
                            VehicleFormat = quotation.Vehicle.VehicleFormat,
                        },
                        Title = $"{vehicleBrand} - {vehicleModel} - {quotation.Vehicle.Year} - {quotation.Vehicle.Plates}",
                        Description = quotation.Description,
                        PriceOfLabor = quotation.PriceOfLabor,
                        PriceOfSpareParts = quotation.PriceOfSpareParts,
                        Status = quotation.Status,
                        CreatedAt = quotation.CreatedAt,
                        Services = quotationServices.Any()
                                    ? quotationServices
                                        .Where(qs => qs.Service?.Service != null)
                                        .Select(qs => new WorkshopServiceResponseDto
                                        {
                                            ServiceId = qs.WorkshopServiceId,
                                            ServiceName = qs.Service.Service.Name,
                                            Price = qs.Price ?? 0
                                        }).ToList()
                                    : null
                    };

                    quotationsResponseDto.Add(quotationResponseDto);
                }

                var response = new PagerResponseDto<QuotationResponseDto>
                {
                    CurrentPage = pager.PageNumber,
                    TotalPages = totalPages,
                    Elements = quotationsResponseDto
                };

                return Ok(response);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPost("customer-pending-pager/{customerId}")]
        public async Task<ActionResult<PagerResponseDto<QuotationResponseDto>>> GetQuotationsPendingByCustomerPaged(Guid customerId, [FromBody] PagerBodyDto pager)
        {
            try
            {
                var query = _context.Quotations
                    .Where(q => q.CustomerId == customerId && (q.Status == "InProgress" || q.Status == "Quoted"))
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.Brand)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.VehicleModel)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.VehicleVersion)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.VehicleType)
                    .Include(q => q.Workshop)
                    .Include(q => q.Customer)
                    .OrderByDescending(q => q.CreatedAt);

                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalCount / pager.PageSize);

                var pagedQuotations = await query
                    .Skip((pager.PageNumber - 1) * pager.PageSize)
                    .Take(pager.PageSize)
                    .ToListAsync();

                if (!pagedQuotations.Any())
                {
                    return NotFound("not-found");
                }

                var quotationsResponseDto = new List<QuotationResponseDto>();

                foreach (var quotation in pagedQuotations)
                {
                    var quotationServices = await _context.QuotationServices
                        .Where(qs => qs.QuotationId == quotation.Id)
                        .Include(qs => qs.Service)
                            .ThenInclude(ws => ws.Service)
                        .ToListAsync();

                    var vehicleBrand = quotation.Vehicle.BrandId == -1 ? quotation.Vehicle.OtherBrand : quotation.Vehicle.Brand.Name;
                    var vehicleModel = quotation.Vehicle.VehicleModelId == -1 ? quotation.Vehicle.OtherVehicleModel : quotation.Vehicle.VehicleModel.Model;

                    var quotationResponseDto = new QuotationResponseDto
                    {
                        Id = quotation.Id,
                        WorkshopId = quotation.WorkshopId,
                        WorkshopName = quotation.Workshop.WorkshopName,
                        CustomerId = quotation.CustomerId,
                        CustomerName = quotation.Customer.FullName,
                        Vehicle = new VehicleResponseDto
                        {
                            Id = quotation.Vehicle.Id,
                            Brand = vehicleBrand,
                            Model = vehicleModel,
                            Version = quotation.Vehicle.VehicleVersionId == -1 ? quotation.Vehicle.OtherVehicleVersion : quotation.Vehicle.VehicleVersion.Version,
                            Type = quotation.Vehicle.VehicleTypeId == -1 ? quotation.Vehicle.OtherVehicleType : quotation.Vehicle.VehicleType.Type,
                            Year = quotation.Vehicle.Year,
                            SerialNumber = quotation.Vehicle.SerialNumber,
                            Color = quotation.Vehicle.Color,
                            Plates = quotation.Vehicle.Plates,
                            RimRubber = quotation.Vehicle.RimRubber,
                            Kms = quotation.Vehicle.Kms,
                            VehicleFormat = quotation.Vehicle.VehicleFormat,
                        },
                        Title = $"{vehicleBrand} - {vehicleModel} - {quotation.Vehicle.Year} - {quotation.Vehicle.Plates}",
                        Description = quotation.Description,
                        PriceOfLabor = quotation.PriceOfLabor,
                        PriceOfSpareParts = quotation.PriceOfSpareParts,
                        Status = quotation.Status,
                        CreatedAt = quotation.CreatedAt,
                        Services = quotationServices.Any()
                                    ? quotationServices
                                        .Where(qs => qs.Service?.Service != null)
                                        .Select(qs => new WorkshopServiceResponseDto
                                        {
                                            ServiceId = qs.WorkshopServiceId,
                                            ServiceName = qs.Service.Service.Name,
                                            Price = qs.Price ?? 0
                                        }).ToList()
                                    : null
                    };

                    quotationsResponseDto.Add(quotationResponseDto);
                }

                var response = new PagerResponseDto<QuotationResponseDto>
                {
                    CurrentPage = pager.PageNumber,
                    TotalPages = totalPages,
                    Elements = quotationsResponseDto
                };

                return Ok(response);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPost("customer-quoted-pager/{customerId}")]
        public async Task<ActionResult<PagerResponseDto<QuotationResponseDto>>> GetQuotationsQuotedByCustomerPaged(Guid customerId, [FromBody] PagerBodyDto pager)
        {
            try
            {
                var query = _context.Quotations
                    .Where(q => q.CustomerId == customerId && (q.Status == "Quoted"))
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.Brand)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.VehicleModel)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.VehicleVersion)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.VehicleType)
                    .Include(q => q.Workshop)
                    .Include(q => q.Customer)
                    .OrderByDescending(q => q.CreatedAt);

                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalCount / pager.PageSize);

                var pagedQuotations = await query
                    .Skip((pager.PageNumber - 1) * pager.PageSize)
                    .Take(pager.PageSize)
                    .ToListAsync();

                if (!pagedQuotations.Any())
                {
                    return NotFound("not-found");
                }

                var quotationsResponseDto = new List<QuotationResponseDto>();

                foreach (var quotation in pagedQuotations)
                {
                    var quotationServices = await _context.QuotationServices
                        .Where(qs => qs.QuotationId == quotation.Id)
                        .Include(qs => qs.Service)
                            .ThenInclude(ws => ws.Service)
                        .ToListAsync();

                    var vehicleBrand = quotation.Vehicle.BrandId == -1 ? quotation.Vehicle.OtherBrand : quotation.Vehicle.Brand.Name;
                    var vehicleModel = quotation.Vehicle.VehicleModelId == -1 ? quotation.Vehicle.OtherVehicleModel : quotation.Vehicle.VehicleModel.Model;

                    var quotationResponseDto = new QuotationResponseDto
                    {
                        Id = quotation.Id,
                        WorkshopId = quotation.WorkshopId,
                        WorkshopName = quotation.Workshop.WorkshopName,
                        CustomerId = quotation.CustomerId,
                        CustomerName = quotation.Customer.FullName,
                        Vehicle = new VehicleResponseDto
                        {
                            Id = quotation.Vehicle.Id,
                            Brand = vehicleBrand,
                            Model = vehicleModel,
                            Version = quotation.Vehicle.VehicleVersionId == -1 ? quotation.Vehicle.OtherVehicleVersion : quotation.Vehicle.VehicleVersion.Version,
                            Type = quotation.Vehicle.VehicleTypeId == -1 ? quotation.Vehicle.OtherVehicleType : quotation.Vehicle.VehicleType.Type,
                            Year = quotation.Vehicle.Year,
                            SerialNumber = quotation.Vehicle.SerialNumber,
                            Color = quotation.Vehicle.Color,
                            Plates = quotation.Vehicle.Plates,
                            RimRubber = quotation.Vehicle.RimRubber,
                            Kms = quotation.Vehicle.Kms,
                            VehicleFormat = quotation.Vehicle.VehicleFormat,
                        },
                        Title = $"{vehicleBrand} - {vehicleModel} - {quotation.Vehicle.Year} - {quotation.Vehicle.Plates}",
                        Description = quotation.Description,
                        PriceOfLabor = quotation.PriceOfLabor,
                        PriceOfSpareParts = quotation.PriceOfSpareParts,
                        Status = quotation.Status,
                        CreatedAt = quotation.CreatedAt,
                        Services = quotationServices.Any()
                                    ? quotationServices
                                        .Where(qs => qs.Service?.Service != null)
                                        .Select(qs => new WorkshopServiceResponseDto
                                        {
                                            ServiceId = qs.WorkshopServiceId,
                                            ServiceName = qs.Service.Service.Name,
                                            Price = qs.Price ?? 0
                                        }).ToList()
                                    : null
                    };

                    quotationsResponseDto.Add(quotationResponseDto);
                }

                var response = new PagerResponseDto<QuotationResponseDto>
                {
                    CurrentPage = pager.PageNumber,
                    TotalPages = totalPages,
                    Elements = quotationsResponseDto
                };

                return Ok(response);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPost("customer-in-progress-pager/{customerId}")]
        public async Task<ActionResult<PagerResponseDto<QuotationResponseDto>>> GetQuotationsInProgressByCustomerPaged(Guid customerId, [FromBody] PagerBodyDto pager)
        {
            try
            {
                var query = _context.Quotations
                    .Where(q => q.CustomerId == customerId && (q.Status == "InProgress"))
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.Brand)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.VehicleModel)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.VehicleVersion)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.VehicleType)
                    .Include(q => q.Workshop)
                    .Include(q => q.Customer)
                    .OrderByDescending(q => q.CreatedAt);

                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalCount / pager.PageSize);

                var pagedQuotations = await query
                    .Skip((pager.PageNumber - 1) * pager.PageSize)
                    .Take(pager.PageSize)
                    .ToListAsync();

                if (!pagedQuotations.Any())
                {
                    return NotFound("not-found");
                }

                var quotationsResponseDto = new List<QuotationResponseDto>();

                foreach (var quotation in pagedQuotations)
                {
                    var quotationServices = await _context.QuotationServices
                        .Where(qs => qs.QuotationId == quotation.Id)
                        .Include(qs => qs.Service)
                            .ThenInclude(ws => ws.Service)
                        .ToListAsync();

                    var vehicleBrand = quotation.Vehicle.BrandId == -1 ? quotation.Vehicle.OtherBrand : quotation.Vehicle.Brand.Name;
                    var vehicleModel = quotation.Vehicle.VehicleModelId == -1 ? quotation.Vehicle.OtherVehicleModel : quotation.Vehicle.VehicleModel.Model;

                    var quotationResponseDto = new QuotationResponseDto
                    {
                        Id = quotation.Id,
                        WorkshopId = quotation.WorkshopId,
                        WorkshopName = quotation.Workshop.WorkshopName,
                        CustomerId = quotation.CustomerId,
                        CustomerName = quotation.Customer.FullName,
                        Vehicle = new VehicleResponseDto
                        {
                            Id = quotation.Vehicle.Id,
                            Brand = vehicleBrand,
                            Model = vehicleModel,
                            Version = quotation.Vehicle.VehicleVersionId == -1 ? quotation.Vehicle.OtherVehicleVersion : quotation.Vehicle.VehicleVersion.Version,
                            Type = quotation.Vehicle.VehicleTypeId == -1 ? quotation.Vehicle.OtherVehicleType : quotation.Vehicle.VehicleType.Type,
                            Year = quotation.Vehicle.Year,
                            SerialNumber = quotation.Vehicle.SerialNumber,
                            Color = quotation.Vehicle.Color,
                            Plates = quotation.Vehicle.Plates,
                            RimRubber = quotation.Vehicle.RimRubber,
                            Kms = quotation.Vehicle.Kms,
                            VehicleFormat = quotation.Vehicle.VehicleFormat,
                        },
                        Title = $"{vehicleBrand} - {vehicleModel} - {quotation.Vehicle.Year} - {quotation.Vehicle.Plates}",
                        Description = quotation.Description,
                        PriceOfLabor = quotation.PriceOfLabor,
                        PriceOfSpareParts = quotation.PriceOfSpareParts,
                        Status = quotation.Status,
                        CreatedAt = quotation.CreatedAt,
                        Services = quotationServices.Any()
                                    ? quotationServices
                                        .Where(qs => qs.Service?.Service != null)
                                        .Select(qs => new WorkshopServiceResponseDto
                                        {
                                            ServiceId = qs.WorkshopServiceId,
                                            ServiceName = qs.Service.Service.Name,
                                            Price = qs.Price ?? 0
                                        }).ToList()
                                    : null
                    };

                    quotationsResponseDto.Add(quotationResponseDto);
                }

                var response = new PagerResponseDto<QuotationResponseDto>
                {
                    CurrentPage = pager.PageNumber,
                    TotalPages = totalPages,
                    Elements = quotationsResponseDto
                };

                return Ok(response);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpGet("workshop-confirmed/{workshopId}")]
        public async Task<ActionResult<QuotationResponseListDto>> GetQuotationsConfirmedByWorkshop(Guid workshopId)
        {
            try
            {
                var quotations = await _context.Quotations
                    .Where(q => q.WorkshopId == workshopId && q.Status == "Confirmed")
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.Brand)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.VehicleModel)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.VehicleVersion)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.VehicleType)
                    .Include(q => q.Workshop)
                    .Include(q => q.Customer)
                    .ToListAsync();

                if (quotations == null)
                {
                    return NotFound("not-found");
                }

                var workshopServices = await _context.WorkshopServices
                    .Where(ws => ws.WorkshopId == workshopId && !ws.IsDeleted)
                    .Include(ws => ws.Service)
                .ToListAsync();

                var quotationsResponseDto = new List<QuotationResponseDto>();
                foreach (var quotation in quotations)
                {
                    var quotationServices = await _context.QuotationServices
                    .Where(qs => qs.QuotationId == quotation.Id)
                    .Include(ws => ws.Service)
                    .ToListAsync();

                    var vehicleBrand = quotation.Vehicle.BrandId == -1 ? quotation.Vehicle.OtherBrand : quotation.Vehicle.Brand.Name;
                    var vehicleModel = quotation.Vehicle.VehicleModelId == -1 ? quotation.Vehicle.OtherVehicleModel : quotation.Vehicle.VehicleModel.Model;

                    var quotationResponseDto = new QuotationResponseDto
                    {
                        Id = quotation.Id,
                        WorkshopId = quotation.WorkshopId,
                        WorkshopName = quotation.Workshop.WorkshopName,
                        CustomerId = quotation.CustomerId,
                        CustomerName = quotation.Customer.FullName,
                        Vehicle = new VehicleResponseDto
                        {
                            Id = quotation.Vehicle.Id,
                            Brand = vehicleBrand,
                            Model = vehicleModel,
                            Version = quotation.Vehicle.VehicleVersionId == -1 ? quotation.Vehicle.OtherVehicleVersion : quotation.Vehicle.VehicleVersion.Version,
                            Type = quotation.Vehicle.VehicleTypeId == -1 ? quotation.Vehicle.OtherVehicleType : quotation.Vehicle.VehicleType.Type,
                            Year = quotation.Vehicle.Year,
                            SerialNumber = quotation.Vehicle.SerialNumber,
                            Color = quotation.Vehicle.Color,
                            Plates = quotation.Vehicle.Plates,
                            RimRubber = quotation.Vehicle.RimRubber,
                            Kms = quotation.Vehicle.Kms,
                            VehicleFormat = quotation.Vehicle.VehicleFormat,
                        },
                        Title = $"{vehicleBrand} - {vehicleModel} - {quotation.Vehicle.Year} - {quotation.Vehicle.Plates}",
                        Description = quotation.Description,
                        PriceOfLabor = quotation.PriceOfLabor,
                        PriceOfSpareParts = quotation.PriceOfSpareParts,
                        Status = quotation.Status,
                        CreatedAt = quotation.CreatedAt,
                        Services = quotationServices.Any()
                                    ? quotationServices
                                        .Where(qs => qs.Service != null && qs.Service.Service != null)
                                        .Select(qs => new WorkshopServiceResponseDto
                                        {
                                            ServiceId = qs.WorkshopServiceId,
                                            ServiceName = qs.Service.Service.Name,
                                            Price = qs.Price ?? 0
                                        }).ToList()
                                    : null
                    };

                    quotationsResponseDto.Add(quotationResponseDto);
                }

                var quotationListDto = new QuotationResponseListDto
                {
                    Count = quotations.Count(),
                    Quotations = quotationsResponseDto
                };

                return Ok(quotationListDto);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPost("workshop-confirmed-pager/{workshopId}")]
        public async Task<ActionResult<PagerResponseDto<QuotationResponseDto>>> GetQuotationsConfirmedByWorkshopPaged(Guid workshopId, [FromBody] PagerBodyDto pager)
        {
            try
            {
                var query = _context.Quotations
                    .Where(q => q.WorkshopId == workshopId && q.Status == "Confirmed")
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.Brand)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.VehicleModel)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.VehicleVersion)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.VehicleType)
                    .Include(q => q.Workshop)
                    .Include(q => q.Customer)
                    .OrderByDescending(q => q.CreatedAt);

                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalCount / pager.PageSize);

                var pagedQuotations = await query
                    .Skip((pager.PageNumber - 1) * pager.PageSize)
                    .Take(pager.PageSize)
                    .ToListAsync();

                if (!pagedQuotations.Any())
                {
                    return NotFound("not-found");
                }

                var quotationsResponseDto = new List<QuotationResponseDto>();

                foreach (var quotation in pagedQuotations)
                {
                    var quotationServices = await _context.QuotationServices
                        .Where(qs => qs.QuotationId == quotation.Id)
                        .Include(qs => qs.Service)
                            .ThenInclude(ws => ws.Service)
                        .ToListAsync();

                    var vehicleBrand = quotation.Vehicle.BrandId == -1 ? quotation.Vehicle.OtherBrand : quotation.Vehicle.Brand.Name;
                    var vehicleModel = quotation.Vehicle.VehicleModelId == -1 ? quotation.Vehicle.OtherVehicleModel : quotation.Vehicle.VehicleModel.Model;

                    var quotationResponseDto = new QuotationResponseDto
                    {
                        Id = quotation.Id,
                        WorkshopId = quotation.WorkshopId,
                        WorkshopName = quotation.Workshop.WorkshopName,
                        CustomerId = quotation.CustomerId,
                        CustomerName = quotation.Customer.FullName,
                        Vehicle = new VehicleResponseDto
                        {
                            Id = quotation.Vehicle.Id,
                            Brand = vehicleBrand,
                            Model = vehicleModel,
                            Version = quotation.Vehicle.VehicleVersionId == -1 ? quotation.Vehicle.OtherVehicleVersion : quotation.Vehicle.VehicleVersion.Version,
                            Type = quotation.Vehicle.VehicleTypeId == -1 ? quotation.Vehicle.OtherVehicleType : quotation.Vehicle.VehicleType.Type,
                            Year = quotation.Vehicle.Year,
                            SerialNumber = quotation.Vehicle.SerialNumber,
                            Color = quotation.Vehicle.Color,
                            Plates = quotation.Vehicle.Plates,
                            RimRubber = quotation.Vehicle.RimRubber,
                            Kms = quotation.Vehicle.Kms,
                            VehicleFormat = quotation.Vehicle.VehicleFormat,
                        },
                        Title = $"{vehicleBrand} - {vehicleModel} - {quotation.Vehicle.Year} - {quotation.Vehicle.Plates}",
                        Description = quotation.Description,
                        PriceOfLabor = quotation.PriceOfLabor,
                        PriceOfSpareParts = quotation.PriceOfSpareParts,
                        Status = quotation.Status,
                        CreatedAt = quotation.CreatedAt,
                        Services = quotationServices.Any()
                                    ? quotationServices
                                        .Where(qs => qs.Service?.Service != null)
                                        .Select(qs => new WorkshopServiceResponseDto
                                        {
                                            ServiceId = qs.WorkshopServiceId,
                                            ServiceName = qs.Service.Service.Name,
                                            Price = qs.Price ?? 0
                                        }).ToList()
                                    : null
                    };

                    quotationsResponseDto.Add(quotationResponseDto);
                }

                var response = new PagerResponseDto<QuotationResponseDto>
                {
                    CurrentPage = pager.PageNumber,
                    TotalPages = totalPages,
                    Elements = quotationsResponseDto
                };

                return Ok(response);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPost("customer-confirmed-pager/{customerId}")]
        public async Task<ActionResult<PagerResponseDto<QuotationResponseDto>>> GetQuotationsConfirmedByCustomerPaged(Guid customerId, [FromBody] PagerBodyDto pager)
        {
            try
            {
                var query = _context.Quotations
                    .Where(q => q.CustomerId == customerId && q.Status == "Confirmed")
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.Brand)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.VehicleModel)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.VehicleVersion)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.VehicleType)
                    .Include(q => q.Workshop)
                    .Include(q => q.Customer)
                    .OrderByDescending(q => q.CreatedAt);

                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalCount / pager.PageSize);

                var pagedQuotations = await query
                    .Skip((pager.PageNumber - 1) * pager.PageSize)
                    .Take(pager.PageSize)
                    .ToListAsync();

                if (!pagedQuotations.Any())
                {
                    return NotFound("not-found");
                }

                var quotationsResponseDto = new List<QuotationResponseDto>();

                foreach (var quotation in pagedQuotations)
                {
                    var quotationServices = await _context.QuotationServices
                        .Where(qs => qs.QuotationId == quotation.Id)
                        .Include(qs => qs.Service)
                            .ThenInclude(ws => ws.Service)
                        .ToListAsync();

                    var vehicleBrand = quotation.Vehicle.BrandId == -1 ? quotation.Vehicle.OtherBrand : quotation.Vehicle.Brand.Name;
                    var vehicleModel = quotation.Vehicle.VehicleModelId == -1 ? quotation.Vehicle.OtherVehicleModel : quotation.Vehicle.VehicleModel.Model;

                    var quotationResponseDto = new QuotationResponseDto
                    {
                        Id = quotation.Id,
                        WorkshopId = quotation.WorkshopId,
                        WorkshopName = quotation.Workshop.WorkshopName,
                        CustomerId = quotation.CustomerId,
                        CustomerName = quotation.Customer.FullName,
                        Vehicle = new VehicleResponseDto
                        {
                            Id = quotation.Vehicle.Id,
                            Brand = vehicleBrand,
                            Model = vehicleModel,
                            Version = quotation.Vehicle.VehicleVersionId == -1 ? quotation.Vehicle.OtherVehicleVersion : quotation.Vehicle.VehicleVersion.Version,
                            Type = quotation.Vehicle.VehicleTypeId == -1 ? quotation.Vehicle.OtherVehicleType : quotation.Vehicle.VehicleType.Type,
                            Year = quotation.Vehicle.Year,
                            SerialNumber = quotation.Vehicle.SerialNumber,
                            Color = quotation.Vehicle.Color,
                            Plates = quotation.Vehicle.Plates,
                            RimRubber = quotation.Vehicle.RimRubber,
                            Kms = quotation.Vehicle.Kms,
                            VehicleFormat = quotation.Vehicle.VehicleFormat,
                        },
                        Title = $"{vehicleBrand} - {vehicleModel} - {quotation.Vehicle.Year} - {quotation.Vehicle.Plates}",
                        Description = quotation.Description,
                        PriceOfLabor = quotation.PriceOfLabor,
                        PriceOfSpareParts = quotation.PriceOfSpareParts,
                        Status = quotation.Status,
                        CreatedAt = quotation.CreatedAt,
                        Services = quotationServices.Any()
                                    ? quotationServices
                                        .Where(qs => qs.Service?.Service != null)
                                        .Select(qs => new WorkshopServiceResponseDto
                                        {
                                            ServiceId = qs.WorkshopServiceId,
                                            ServiceName = qs.Service.Service.Name,
                                            Price = qs.Price ?? 0
                                        }).ToList()
                                    : null
                    };

                    quotationsResponseDto.Add(quotationResponseDto);
                }

                var response = new PagerResponseDto<QuotationResponseDto>
                {
                    CurrentPage = pager.PageNumber,
                    TotalPages = totalPages,
                    Elements = quotationsResponseDto
                };

                return Ok(response);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task UpdateExpiredQuotations(Guid workshopId)
        {
            try
            {
                var oneMonthAgo = DateTime.Now.AddMonths(-1);

                var expiredQuotations = await _context.Quotations
                    .Where(q => q.WorkshopId == workshopId &&
                                (q.Status == "InProgress" || q.Status == "Quoted") &&
                                q.CreatedAt < oneMonthAgo)
                    .ToListAsync();

                if (expiredQuotations.Any())
                {
                    foreach (var quotation in expiredQuotations)
                    {
                        quotation.Status = "Expired";
                    }

                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UpdateExpiredQuotations] Error: {ex.Message}");
            }
        }


    }
}
