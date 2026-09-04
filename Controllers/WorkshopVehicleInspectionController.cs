using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiTaller.Data;
using MiTaller.DTO.Customer;
using MiTaller.DTO.Vehicle;
using MiTaller.Models.Vehicle;
using MiTaller.Models.Customer;
using MiTaller.Models.Workshop;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Threading.Tasks;
using MiTaller.Models.Auth;
using MiTaller.Models.Notification;
using Microsoft.AspNetCore.Authorization;
using MiTaller.Attributes;
using MiTaller.DTO.Workshop;
using MiTaller.DTO.Inspections;
using MiTaller.Models.Inspections;
using MiTaller.DTO;
using Microsoft.AspNetCore.Identity.UI.Services;
using MiTaller.DTO.Quotation;
using MiTaller.Services;
using QuestPDF.Fluent;
using MiTaller.Services.Documents;
using MiTaller.DTO.Pager;
using System.Collections.Generic;

namespace MiTaller.Controllers
{
    [Route("api/workshop-vehicle-inspections")]
    [ApiController]
    [Authorize]
    public class WorkshopVehicleInspectionController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly UserManager<BaseIdentityUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly FirebaseNotificationService _firebaseNotificationService;

        public WorkshopVehicleInspectionController(DataContext context, UserManager<BaseIdentityUser> userManager, IEmailSender emailSender, FirebaseNotificationService firebaseNotificationService)
        {
            _context = context;
            _userManager = userManager;
            _emailSender = emailSender;
            _firebaseNotificationService = firebaseNotificationService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetWorkshopVehicleInspectionById(int id)
        {
            try 
            {
                var inspection = await _context.WorkshopVehicleInspections
                    .Include(i => i.Workshop)
                    .Include(i => i.Customer)
                    .Include(i => i.Vehicle)
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
        public async Task<IActionResult> GetWorkshopVehicleInspectionsByWorkshop(Guid workshopId)
        {
            try 
            {
                var inspections = await _context.WorkshopVehicleInspections
                    .Where(i => i.Workshop.Id == workshopId && i.IsActive)
                    .OrderByDescending(i => i.InspectionDate)
                    .ToListAsync();

                if (!inspections.Any()) return NotFound("not-found");

                return Ok(inspections);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpGet("active-vehicles-in-workshop/{workshopId}")]
        public async Task<ActionResult<ICollection<VehicleInWorkshopResponseDto>>> GetVehiclesInWorkshop(Guid workshopId)
        {
            try
            {
                var inspections = await _context.WorkshopVehicleInspections
                    .Where(i => i.WorkshopId == workshopId && i.IsActive)
                    .Include(i => i.Vehicle)
                    .ThenInclude(i => i.Brand)
                    .Include(i => i.Vehicle)
                    .ThenInclude(i => i.VehicleModel)
                    .Include(i => i.Vehicle)
                    .ThenInclude(i => i.VehicleVersion)
                    .Include(i => i.Vehicle)
                    .ThenInclude(i => i.VehicleType)
                    .OrderByDescending(i => i.InspectionDate)
                    .ToListAsync();

                if (!inspections.Any()) return NotFound("not-found");

                var vehiclesInWorkshopsDto = new List<VehicleInWorkshopResponseDto>();

                foreach (var inspection in inspections)
                {
                    var vehicleInWorkshopDto = new VehicleInWorkshopResponseDto
                    {
                        WorkshopInspectionId = inspection.Id,
                        CustomerId = inspection.CustomerId,
                        Status = inspection.Status,
                        IsActive = inspection.IsActive,
                        Vehicle = new VehicleResponseDto
                        {
                            Id = inspection.Vehicle.Id,
                            Brand = inspection.Vehicle.BrandId == -1 ? inspection.Vehicle.OtherBrand : inspection.Vehicle.Brand.Name,
                            Model = inspection.Vehicle.VehicleModelId == -1 ? inspection.Vehicle.OtherVehicleModel : inspection.Vehicle.VehicleModel.Model,
                            Version = inspection.Vehicle.VehicleVersionId == -1 ? inspection.Vehicle.OtherVehicleVersion : inspection.Vehicle.VehicleVersion.Version,
                            Type = inspection.Vehicle.VehicleTypeId == -1 ? inspection.Vehicle.OtherVehicleType : inspection.Vehicle.VehicleType.Type,
                            Year = inspection.Vehicle.Year,
                            SerialNumber = inspection.Vehicle.SerialNumber,
                            Color = inspection.Vehicle.Color,
                            Plates = inspection.Vehicle.Plates,
                            RimRubber = inspection.Vehicle.RimRubber,
                            Kms = inspection.Vehicle.Kms,
                            VehicleFormat = inspection.Vehicle.VehicleFormat,
                            Image = inspection.Vehicle.Image,
                        },
                    };
                    vehiclesInWorkshopsDto.Add(vehicleInWorkshopDto);
                }

                return Ok(vehiclesInWorkshopsDto);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPost("active-vehicles-in-workshop-pager/{workshopId}")]
        public async Task<ActionResult<PagerResponseDto<VehicleInWorkshopResponseDto>>> GetVehiclesInWorkshopPaged(Guid workshopId, [FromBody] PagerBodyDto pager)
        {
            try
            {
                var query = _context.WorkshopVehicleInspections
                    .Where(i => i.WorkshopId == workshopId && i.IsActive)
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
                var totalPages = (int)Math.Ceiling((double)totalCount / pager.PageSize);

                var inspections = await query
                    .Skip((pager.PageNumber - 1) * pager.PageSize)
                    .Take(pager.PageSize)
                    .ToListAsync();

                if (!inspections.Any()) return NotFound("not-found");

                var vehiclesInWorkshopsDto = inspections.Select(inspection => new VehicleInWorkshopResponseDto
                {
                    WorkshopInspectionId = inspection.Id,
                    CustomerId = inspection.CustomerId,
                    Status = inspection.Status,
                    IsActive = inspection.IsActive,
                    Vehicle = new VehicleResponseDto
                    {
                        Id = inspection.Vehicle.Id,
                        Brand = inspection.Vehicle.BrandId == -1 ? inspection.Vehicle.OtherBrand : inspection.Vehicle.Brand.Name,
                        Model = inspection.Vehicle.VehicleModelId == -1 ? inspection.Vehicle.OtherVehicleModel : inspection.Vehicle.VehicleModel.Model,
                        Version = inspection.Vehicle.VehicleVersionId == -1 ? inspection.Vehicle.OtherVehicleVersion : inspection.Vehicle.VehicleVersion.Version,
                        Type = inspection.Vehicle.VehicleTypeId == -1 ? inspection.Vehicle.OtherVehicleType : inspection.Vehicle.VehicleType.Type,
                        Year = inspection.Vehicle.Year,
                        SerialNumber = inspection.Vehicle.SerialNumber,
                        Color = inspection.Vehicle.Color,
                        Plates = inspection.Vehicle.Plates,
                        RimRubber = inspection.Vehicle.RimRubber,
                        Kms = inspection.Vehicle.Kms,
                        VehicleFormat = inspection.Vehicle.VehicleFormat,
                        Image = inspection.Vehicle.Image,
                    }
                }).ToList();

                var response = new PagerWithCountResponseDto<VehicleInWorkshopResponseDto>
                {
                    CurrentPage = pager.PageNumber,
                    TotalPages = totalPages,
                    TotalElements = totalCount,
                    Elements = vehiclesInWorkshopsDto
                };

                return Ok(response);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpGet("finished-vehicles-in-workshop/{workshopId}")]
        public async Task<ActionResult<ICollection<VehicleInWorkshopResponseDto>>> GetFinishedVehiclesInWorkshop(Guid workshopId)
        {
            try
            {
                var inspections = await _context.WorkshopVehicleInspections
                    .Where(i => i.WorkshopId == workshopId && !i.IsActive)
                    .Include(i => i.Vehicle)
                    .ThenInclude(i => i.Brand)
                    .Include(i => i.Vehicle)
                    .ThenInclude(i => i.VehicleModel)
                    .Include(i => i.Vehicle)
                    .ThenInclude(i => i.VehicleVersion)
                    .Include(i => i.Vehicle)
                    .ThenInclude(i => i.VehicleType)
                    .OrderByDescending(i => i.InspectionDate)
                    .ToListAsync();

                if (!inspections.Any()) return NotFound("not-found");

                var vehiclesInWorkshopsDto = new List<VehicleInWorkshopResponseDto>();

                foreach (var inspection in inspections)
                {
                    var vehicleInWorkshopDto = new VehicleInWorkshopResponseDto
                    {
                        WorkshopInspectionId = inspection.Id,
                        CustomerId = inspection.CustomerId,
                        Status = inspection.Status,
                        IsActive = inspection.IsActive,
                        Vehicle = new VehicleResponseDto
                        {
                            Id = inspection.Vehicle.Id,
                            Brand = inspection.Vehicle.BrandId == -1 ? inspection.Vehicle.OtherBrand : inspection.Vehicle.Brand.Name,
                            Model = inspection.Vehicle.VehicleModelId == -1 ? inspection.Vehicle.OtherVehicleModel : inspection.Vehicle.VehicleModel.Model,
                            Version = inspection.Vehicle.VehicleVersionId == -1 ? inspection.Vehicle.OtherVehicleVersion : inspection.Vehicle.VehicleVersion.Version,
                            Type = inspection.Vehicle.VehicleTypeId == -1 ? inspection.Vehicle.OtherVehicleType : inspection.Vehicle.VehicleType.Type,
                            Year = inspection.Vehicle.Year,
                            SerialNumber = inspection.Vehicle.SerialNumber,
                            Color = inspection.Vehicle.Color,
                            Plates = inspection.Vehicle.Plates,
                            RimRubber = inspection.Vehicle.RimRubber,
                            Kms = inspection.Vehicle.Kms,
                            VehicleFormat = inspection.Vehicle.VehicleFormat,
                            Image = inspection.Vehicle.Image,
                        },
                    };
                    vehiclesInWorkshopsDto.Add(vehicleInWorkshopDto);
                }

                return Ok(vehiclesInWorkshopsDto);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPost("finished-vehicles-in-workshop-pager/{workshopId}")]
        public async Task<ActionResult<PagerResponseDto<VehicleInWorkshopResponseDto>>> GetFinishedVehiclesInWorkshopPaged(Guid workshopId, [FromBody] PagerBodyDto pager)
        {
            try
            {
                var query = _context.WorkshopVehicleInspections
                    .Where(i => i.WorkshopId == workshopId && !i.IsActive)
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
                var totalPages = (int)Math.Ceiling((double)totalCount / pager.PageSize);

                var inspections = await query
                    .Skip((pager.PageNumber - 1) * pager.PageSize)
                    .Take(pager.PageSize)
                    .ToListAsync();

                if (!inspections.Any()) return NotFound("not-found");

                var vehiclesInWorkshopsDto = inspections.Select(inspection => new VehicleInWorkshopResponseDto
                {
                    WorkshopInspectionId = inspection.Id,
                    CustomerId = inspection.CustomerId,
                    Status = inspection.Status,
                    IsActive = inspection.IsActive,
                    Vehicle = new VehicleResponseDto
                    {
                        Id = inspection.Vehicle.Id,
                        Brand = inspection.Vehicle.BrandId == -1 ? inspection.Vehicle.OtherBrand : inspection.Vehicle.Brand.Name,
                        Model = inspection.Vehicle.VehicleModelId == -1 ? inspection.Vehicle.OtherVehicleModel : inspection.Vehicle.VehicleModel.Model,
                        Version = inspection.Vehicle.VehicleVersionId == -1 ? inspection.Vehicle.OtherVehicleVersion : inspection.Vehicle.VehicleVersion.Version,
                        Type = inspection.Vehicle.VehicleTypeId == -1 ? inspection.Vehicle.OtherVehicleType : inspection.Vehicle.VehicleType.Type,
                        Year = inspection.Vehicle.Year,
                        SerialNumber = inspection.Vehicle.SerialNumber,
                        Color = inspection.Vehicle.Color,
                        Plates = inspection.Vehicle.Plates,
                        RimRubber = inspection.Vehicle.RimRubber,
                        Kms = inspection.Vehicle.Kms,
                        VehicleFormat = inspection.Vehicle.VehicleFormat,
                        Image = inspection.Vehicle.Image,
                    }
                }).ToList();

                var response = new PagerResponseDto<VehicleInWorkshopResponseDto>
                {
                    CurrentPage = pager.PageNumber,
                    TotalPages = totalPages,
                    Elements = vehiclesInWorkshopsDto
                };

                return Ok(response);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpGet("vehicles-in-workshop-full-info/{workshopId}")]
        public async Task<ActionResult<ICollection<VehicleInWorkshopFullInfoResponseDto>>> GetVehiclesInWorkshopFullInfo(Guid workshopId)
        {
            try
            {
                var workshop = await _context.Workshops
                    .Where(w => w.Id == workshopId && !w.IsDeleted)
                    .FirstOrDefaultAsync();

                if (workshop == null) return NotFound("not-found");

                var inspections = await _context.WorkshopVehicleInspections
                    .Where(i => i.WorkshopId == workshopId && i.IsActive)
                    .Include(i => i.Customer)
                    .Include(i => i.Vehicle)
                        .ThenInclude(v => v.Brand)
                    .Include(i => i.Vehicle)
                        .ThenInclude(v => v.VehicleModel)
                    .Include(i => i.Vehicle)
                        .ThenInclude(v => v.VehicleVersion)
                    .Include(i => i.Vehicle)
                        .ThenInclude(v => v.VehicleType)
                    .OrderByDescending(i => i.InspectionDate)
                    .ToListAsync();

                if (!inspections.Any()) return NotFound("not-found");

                var result = inspections.Select(i => new VehicleInWorkshopFullInfoResponseDto
                {
                    WorkshopInspectionId = i.Id,
                    CustomerId = i.Customer.Id,
                    FullName = i.Customer.FullName,
                    Email = i.Customer.Email,
                    PhoneNumber = i.Customer.NormalizedPhoneNumber,
                    //ProfileImage = i.Customer.ProfileImage,
                    Status = i.Status,
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
                    InspectionHistory = [],
                }).ToList();

                return Ok(result);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPost("get-vehicles-in-workshop-history")]
        public async Task<ActionResult<VehicleInspectionHistoryResponseDto>> GetVehiclesHistoryInWorkshop(CustomerWorkshopVehicleBodyDto model)
        {
            try
            {
                var workshop = await _context.Workshops
                    .Where(w => w.Id == model.WorkshopId && !w.IsDeleted)
                    .FirstOrDefaultAsync();

                if (workshop == null) return NotFound("not-found");

                var customer = await _context.Customers
                    .Where(w => w.Id == model.CustomerId && !w.IsDeleted)
                    .FirstOrDefaultAsync();

                if (customer == null) return NotFound("not-found");

                var vehicle = await _context.Vehicles
                    .Where(w => w.Id == model.VehicleId && !w.IsDeleted)
                    .FirstOrDefaultAsync();

                if (vehicle == null) return NotFound("not-found");

                var inspectionHistoriesDto = await _context.VehicleInspectionHistory
                    .Where(h => h.VehicleInspection.WorkshopId == model.WorkshopId
                             && h.VehicleInspection.CustomerId == model.CustomerId
                             && h.VehicleInspection.VehicleId == model.VehicleId)
                    .OrderByDescending(w => w.VehicleInspection.InspectionDate)
                    .Select(h => new VehicleInspectionHistoryResponseDto
                    {
                        VehicleInspectionHistoryId = h.Id,
                        Title = h.Title,
                        Folio = h.Folio,
                        File = h.File,
                        CreatedAt = h.CreatedAt
                    })
                    .ToListAsync();

                //var quotationHistoriesDto = await _context.Quotations
                //    .Where(q => q.CustomerId == model.CustomerId
                //             && q.WorkshopId == model.WorkshopId
                //             && q.VehicleId == model.VehicleId
                //             && q.Status == "Confirmed")
                //    .Select(q => new VehicleQuotationHistoryResponseDto
                //    {
                //        QuotationId = q.Id,
                //        Description = q.Description,
                //        PriceOfLabor = q.PriceOfLabor,
                //        PriceOfSpareParts = q.PriceOfSpareParts,
                //        Status = q.Status,
                //        CreatedAt = q.CreatedAt
                //    })
                //    .ToListAsync();

                //var vehicleHistoryResponseDto = new VehicleHistoryResponseDto
                //{
                //    InspectionHistory = inspectionHistoriesDto,
                //    //QuotationHistory = quotationHistoriesDto,
                //};

                return Ok(inspectionHistoriesDto);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPost("get-vehicles-in-workshop-history-pager")]
        public async Task<ActionResult<PagerResponseDto<VehicleInspectionHistoryResponseDto>>> GetVehiclesHistoryInWorkshopPaged([FromBody] CustomerWorkshopVehicleBodyDto model, [FromQuery] PagerBodyDto pager)
        {
            try
            {
                var workshop = await _context.Workshops
                    .FirstOrDefaultAsync(w => w.Id == model.WorkshopId && !w.IsDeleted);
                if (workshop == null) return NotFound("not-found");

                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.Id == model.CustomerId && !c.IsDeleted);
                if (customer == null) return NotFound("not-found");

                var vehicle = await _context.Vehicles
                    .FirstOrDefaultAsync(v => v.Id == model.VehicleId && !v.IsDeleted);
                if (vehicle == null) return NotFound("not-found");

                var query = _context.VehicleInspectionHistory
                    .Where(h => h.VehicleInspection.WorkshopId == model.WorkshopId
                             && h.VehicleInspection.CustomerId == model.CustomerId
                             && h.VehicleInspection.VehicleId == model.VehicleId)
                    .OrderByDescending(w => w.VehicleInspection.InspectionDate)
                    .Select(h => new VehicleInspectionHistoryResponseDto
                    {
                        VehicleInspectionHistoryId = h.Id,
                        Title = h.Title,
                        Folio = h.Folio,
                        File = h.File,
                        CreatedAt = h.CreatedAt
                    });

                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalCount / pager.PageSize);

                var pagedResults = await query
                    .Skip((pager.PageNumber - 1) * pager.PageSize)
                    .Take(pager.PageSize)
                    .ToListAsync();

                var response = new PagerResponseDto<VehicleInspectionHistoryResponseDto>
                {
                    CurrentPage = pager.PageNumber,
                    TotalPages = totalPages,
                    Elements = pagedResults
                };

                return Ok(response);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpGet("vehicles-in-workshop-by/{customerId}")]
        public async Task<ActionResult<ICollection<VehicleInWorkshopResponseDto>>> GetVehiclesInWorkshopByCustomer(Guid customerId)
        {
            try
            {
                var inspections = await _context.WorkshopVehicleInspections
                    .Where(i => i.CustomerId == customerId && i.IsActive)
                    .Include(i => i.Vehicle)
                    .ThenInclude(i => i.Brand)
                    .Include(i => i.Vehicle)
                    .ThenInclude(i => i.VehicleModel)
                    .Include(i => i.Vehicle)
                    .ThenInclude(i => i.VehicleVersion)
                    .Include(i => i.Vehicle)
                    .ThenInclude(i => i.VehicleType)
                    .OrderByDescending(i => i.InspectionDate)
                    .ToListAsync();

                if (!inspections.Any()) return NotFound("not-found");

                var vehiclesInWorkshopsDto = new List<VehicleInWorkshopResponseDto>();

                foreach (var inspection in inspections)
                {
                    var vehicleInWorkshopDto = new VehicleInWorkshopResponseDto
                    {
                        WorkshopInspectionId = inspection.Id,
                        CustomerId = inspection.CustomerId,
                        Status = inspection.Status,
                        IsActive = inspection.IsActive,
                        Vehicle = new VehicleResponseDto
                        {
                            Id = inspection.Vehicle.Id,
                            Brand = inspection.Vehicle.BrandId == -1 ? inspection.Vehicle.OtherBrand : inspection.Vehicle.Brand.Name,
                            Model = inspection.Vehicle.VehicleModelId == -1 ? inspection.Vehicle.OtherVehicleModel : inspection.Vehicle.VehicleModel.Model,
                            Version = inspection.Vehicle.VehicleVersionId == -1 ? inspection.Vehicle.OtherVehicleVersion : inspection.Vehicle.VehicleVersion.Version,
                            Type = inspection.Vehicle.VehicleTypeId == -1 ? inspection.Vehicle.OtherVehicleType : inspection.Vehicle.VehicleType.Type,
                            Year = inspection.Vehicle.Year,
                            SerialNumber = inspection.Vehicle.SerialNumber,
                            Color = inspection.Vehicle.Color,
                            Plates = inspection.Vehicle.Plates,
                            RimRubber = inspection.Vehicle.RimRubber,
                            Kms = inspection.Vehicle.Kms,
                            VehicleFormat = inspection.Vehicle.VehicleFormat,
                            Image = inspection.Vehicle.Image,
                        },
                    };
                    vehiclesInWorkshopsDto.Add(vehicleInWorkshopDto);
                }

                return Ok(vehiclesInWorkshopsDto);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPost("vehicles-in-workshop-by-pager/{customerId}")]
        public async Task<ActionResult<PagerResponseDto<VehicleInWorkshopResponseDto>>> GetVehiclesInWorkshopByCustomerPaged(Guid customerId, [FromBody] PagerBodyDto pager)
        {
            try
            {
                var query = _context.WorkshopVehicleInspections
                    .Where(i => i.CustomerId == customerId && i.IsActive)
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
                var totalPages = (int)Math.Ceiling((double)totalCount / pager.PageSize);

                var inspections = await query
                    .Skip((pager.PageNumber - 1) * pager.PageSize)
                    .Take(pager.PageSize)
                    .ToListAsync();

                if (!inspections.Any()) return NotFound("not-found");

                var vehiclesInWorkshopsDto = inspections.Select(inspection => new VehicleInWorkshopResponseDto
                {
                    WorkshopInspectionId = inspection.Id,
                    CustomerId = inspection.CustomerId,
                    Status = inspection.Status,
                    IsActive = inspection.IsActive,
                    Vehicle = new VehicleResponseDto
                    {
                        Id = inspection.Vehicle.Id,
                        Brand = inspection.Vehicle.BrandId == -1 ? inspection.Vehicle.OtherBrand : inspection.Vehicle.Brand.Name,
                        Model = inspection.Vehicle.VehicleModelId == -1 ? inspection.Vehicle.OtherVehicleModel : inspection.Vehicle.VehicleModel.Model,
                        Version = inspection.Vehicle.VehicleVersionId == -1 ? inspection.Vehicle.OtherVehicleVersion : inspection.Vehicle.VehicleVersion.Version,
                        Type = inspection.Vehicle.VehicleTypeId == -1 ? inspection.Vehicle.OtherVehicleType : inspection.Vehicle.VehicleType.Type,
                        Year = inspection.Vehicle.Year,
                        SerialNumber = inspection.Vehicle.SerialNumber,
                        Color = inspection.Vehicle.Color,
                        Plates = inspection.Vehicle.Plates,
                        RimRubber = inspection.Vehicle.RimRubber,
                        Kms = inspection.Vehicle.Kms,
                        VehicleFormat = inspection.Vehicle.VehicleFormat,
                        Image = inspection.Vehicle.Image,
                    }
                }).ToList();

                var response = new PagerResponseDto<VehicleInWorkshopResponseDto>
                {
                    CurrentPage = pager.PageNumber,
                    TotalPages = totalPages,
                    Elements = vehiclesInWorkshopsDto
                };

                return Ok(response);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPost("customer-get-vehicles-in-workshop-history")]
        public async Task<ActionResult<VehicleInspectionHistoryResponseDto>> GetCustomerVehiclesHistory(CustomerVehicleBodyDto model)
        {
            try
            {
                var customer = await _context.Customers
                    .Where(w => w.Id == model.CustomerId && !w.IsDeleted)
                    .FirstOrDefaultAsync();

                if (customer == null) return NotFound("not-found");

                var vehicle = await _context.Vehicles
                    .Where(w => w.Id == model.VehicleId && !w.IsDeleted)
                    .FirstOrDefaultAsync();

                if (vehicle == null) return NotFound("not-found");

                var inspectionHistoriesDto = await _context.VehicleInspectionHistory
                    .Where(h => h.VehicleInspection.CustomerId == model.CustomerId
                             && h.VehicleInspection.VehicleId == model.VehicleId)
                    .OrderByDescending(w => w.VehicleInspection.InspectionDate)
                    .Select(h => new VehicleInspectionHistoryResponseDto
                    {
                        VehicleInspectionHistoryId = h.Id,
                        Title = h.Title,
                        Folio = h.Folio,
                        File = h.File,
                        CreatedAt = h.CreatedAt
                    })
                    .ToListAsync();

                //var quotationHistoriesDto = await _context.Quotations
                //    .Where(q => q.CustomerId == model.CustomerId
                //             && q.VehicleId == model.VehicleId
                //             && q.Status == "Confirmed")
                //    .Select(q => new VehicleQuotationHistoryResponseDto
                //    {
                //        QuotationId = q.Id,
                //        Description = q.Description,
                //        PriceOfLabor = q.PriceOfLabor,
                //        PriceOfSpareParts = q.PriceOfSpareParts,
                //        Status = q.Status,
                //        CreatedAt = q.CreatedAt
                //    })
                //    .ToListAsync();

                //var vehicleHistoryResponseDto = new VehicleHistoryResponseDto
                //{
                //    InspectionHistory = inspectionHistoriesDto,
                //    QuotationHistory = quotationHistoriesDto,
                //};

                return Ok(inspectionHistoriesDto);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPost("customer-get-vehicles-in-workshop-history-pager")]
        public async Task<ActionResult<PagerResponseDto<VehicleInspectionHistoryResponseDto>>> GetCustomerVehiclesHistoryPaged([FromBody] CustomerVehicleBodyDto model, [FromQuery] PagerBodyDto pager)
        {
            try
            {
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(w => w.Id == model.CustomerId && !w.IsDeleted);
                if (customer == null) return NotFound("not-found");

                var vehicle = await _context.Vehicles
                    .FirstOrDefaultAsync(w => w.Id == model.VehicleId && !w.IsDeleted);
                if (vehicle == null) return NotFound("not-found");

                var query = _context.VehicleInspectionHistory
                    .Where(h => h.VehicleInspection.CustomerId == model.CustomerId
                             && h.VehicleInspection.VehicleId == model.VehicleId)
                    .OrderByDescending(w => w.VehicleInspection.InspectionDate)
                    .Select(h => new VehicleInspectionHistoryResponseDto
                    {
                        VehicleInspectionHistoryId = h.Id,
                        Title = h.Title,
                        Folio = h.Folio,
                        File = h.File,
                        CreatedAt = h.CreatedAt
                    });

                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalCount / pager.PageSize);

                var pagedResults = await query
                    .Skip((pager.PageNumber - 1) * pager.PageSize)
                    .Take(pager.PageSize)
                    .ToListAsync();

                var response = new PagerResponseDto<VehicleInspectionHistoryResponseDto>
                {
                    CurrentPage = pager.PageNumber,
                    TotalPages = totalPages,
                    Elements = pagedResults
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
        public async Task<ActionResult> CreateWorkshopVehicleInspection([FromForm] PostWorkshopVehicleInspectionDto model)
        {
            try
            {
                var workshop = await _context.Workshops
                    .Where(w => w.Id == model.WorkshopId && !w.IsDeleted)
                    .FirstOrDefaultAsync(); 
                if (workshop == null) return NotFound("not-found");

                var activeInspection = await _context.WorkshopVehicleInspections
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

                    // Creamos el registro de notificationSettings
                    var notificationSettings = new NotificationSettings
                    {
                        UserId = newCustomer.Id,
                        UserType = UserType.Customer,
                    };

                    await _context.NotificationSettings.AddAsync(notificationSettings);
                    await _context.SaveChangesAsync();


                    // Notificación de bienvenida
                    var notification = new Notifications
                    {
                        UserId = newCustomer.Id,
                        UserType = UserType.Customer,
                        Title = "Cuenta creada exitosamente",
                        Content = "¡Tu cuenta ha sido creada con éxito! Ahora puedes acceder a todas las funcionalidades de tu cuentinspection.",
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

                var inspection = new WorkshopVehicleInspection
                {
                    Workshop = workshop,
                    Customer = customer,
                    Vehicle = vehicle,
                    IsNewCustomer = model.IsNewCustomer,
                    IsNewVehicle = model.IsNewVehicle,
                    IsActive = true,

                    FrontRightBrake = model.FrontRightBrake,
                    FrontRightTireTread = model.FrontRightTireTread,
                    FrontRightTireAlignment = model.FrontRightTireAlignment,

                    FrontLeftBrake = model.FrontLeftBrake,
                    FrontLeftTireTread = model.FrontLeftTireTread,
                    FrontLeftTireAlignment = model.FrontLeftTireAlignment,

                    RearRightBrake = model.RearRightBrake,
                    RearRightTireTread = model.RearRightTireTread,
                    RearRightTireAlignment = model.RearRightTireAlignment,

                    RearLeftBrake = model.RearLeftBrake,
                    RearLeftTireTread = model.RearLeftTireTread,
                    RearLeftTireAlignment = model.RearLeftTireAlignment,

                    TiresComments = model.TiresComments,

                    Brakes = model.Brakes,
                    TireTread = model.TireTread,
                    TireAlignment = model.TireAlignment,
                    Headlights = model.Headlights,
                    Taillights = model.Taillights,
                    TurnSignals = model.TurnSignals,
                    BrakeLights = model.BrakeLights,
                    HazardLights = model.HazardLights,
                    WindshieldWasherFluid = model.WindshieldWasherFluid,
                    WindshieldWiperBlades = model.WindshieldWiperBlades,
                    WindshieldCondition = model.WindshieldCondition,
                    Mirrors = model.Mirrors,
                    EmergencyBrake = model.EmergencyBrake,
                    Horn = model.Horn,
                    FuelTankCap = model.FuelTankCap,
                    AirConditioningFilter = model.AirConditioningFilter,
                    ReversingLights = model.ReversingLights,
                    LicensePlateLight = model.LicensePlateLight,
                    SeatBelts = model.SeatBelts,

                    InteriorAndExteriorComments = model.InteriorAndExteriorComments,

                    EngineOilLevel = model.EngineOilLevel,
                    CoolantLevel = model.CoolantLevel,
                    BrakeFluidLevel = model.BrakeFluidLevel,
                    AirFilter = model.AirFilter,
                    RadiatorHoses = model.RadiatorHoses,
                    HeatingHoses = model.HeatingHoses,
                    AirConditioningCondenser = model.AirConditioningCondenser,
                    TransmissionFluidLevel = model.TransmissionFluidLevel,
                    PowerSteeringFluidLevel = model.PowerSteeringFluidLevel,
                    AccessoryBelt = model.AccessoryBelt,
                    ExhaustSystem = model.ExhaustSystem,

                    EngineComments = model.EngineComments,

                    BatteryTerminals = model.BatteryTerminals,
                    BatteryCables = model.BatteryCables,
                    BatteryMounting = model.BatteryMounting,
                    GeneralBatteryCondition = model.GeneralBatteryCondition,

                    BatteryComments = model.BatteryComments,

                    FrontShockAbsorbers = model.FrontShockAbsorbers,
                    RearShockAbsorbers = model.RearShockAbsorbers,
                    BallJoints = model.BallJoints,
                    SteeringRackAndTierods = model.SteeringRackAndTierods,
                    SuspensionBushings = model.SuspensionBushings,

                    SuspensionComments = model.SuspensionComments,

                    ChasisComments = model.ChasisComments,

                    Observations = model.Observations,
                };

                _context.WorkshopVehicleInspections.Add(inspection);
                await _context.SaveChangesAsync();

                // Archivos Adjuntos
                if (model.Files != null && model.Files.Any())
                {
                    foreach (var file in model.Files)
                    {
                        using (var ms = new MemoryStream())
                        {
                            await file.CopyToAsync(ms);
                            var newFile = new WorkshopVehicleFile
                            {
                                WorkshopVehicleInspectionId = inspection.Id,
                                FileName = file.FileName,
                                FileType = file.ContentType,
                                FileData = ms.ToArray()
                            };

                            _context.WorkshopVehicleFiles.Add(newFile);
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                // Registro InspectionHistory
                var vehicleInspectionHistory = new VehicleInspectionHistory
                {
                    VehicleInspectionId = inspection.Id,
                    Title = "Solicitud de reparación",
                    Folio = GenerateFolio(),
                    File = null,
                    CreatedAt = DateTime.Now,
                };
                _context.VehicleInspectionHistory.Add(vehicleInspectionHistory);

                // Registro InspectionDetailHistory
                var vehicleInspectionDetailHistory = new VehicleInspectionDetailHistory
                {
                    VehicleInspectionId = inspection.Id,
                    Title = "Entró a taller",
                    IsCompleted = true,
                    CompletedAt = DateTime.Now,
                };
                _context.VehicleInspectionDetailHistory.Add(vehicleInspectionDetailHistory);

                //// Se crea el pdf
                //var photos = await _context.WorkshopVehicleFiles
                //    .Where(f => f.WorkshopVehicleInspectionId == inspection.Id)
                //    .ToListAsync();

                //var inspectionDocumentDto = new VehicleInspectionDocumentDto
                //{
                //    InspectionDate = inspection.InspectionDate,
                //    Folio = vehicleInspectionHistory.Folio,
                //    CustomerName = inspection.Customer.FullName,
                //    CustomerPhoneNumber = inspection.Customer.NormalizedPhoneNumber,
                //    CustomerGuid = inspection.Customer.Id.ToString().Substring(0, 13),

                //    WorkshopName = inspection.Workshop.WorkshopName,
                //    WorkshopPhoneNumber = inspection.Workshop.NormalizedPhoneNumber,
                //    WorkshopGuid = inspection.Workshop.Id.ToString().Substring(0, 13),

                //    Vehicle = new VehicleResponseDto
                //    {
                //        Plates = inspection.Vehicle.Plates,
                //        Year = inspection.Vehicle.Year,
                //        VehicleFormat = inspection.Vehicle.VehicleFormat,
                //        Brand = inspection.Vehicle.BrandId == -1 ? inspection.Vehicle.OtherBrand : inspection.Vehicle.Brand.Name,
                //        Model = inspection.Vehicle.VehicleModelId == -1 ? inspection.Vehicle.OtherVehicleModel : inspection.Vehicle.VehicleModel.Model,
                //        Version = inspection.Vehicle.VehicleVersionId == -1 ? inspection.Vehicle.OtherVehicleVersion : inspection.Vehicle.VehicleVersion.Version,
                //        Type = inspection.Vehicle.VehicleTypeId == -1 ? inspection.Vehicle.OtherVehicleType : inspection.Vehicle.VehicleType.Type,
                //    },

                //    FrontRightBrake = inspection.FrontRightBrake,
                //    FrontRightTireTread = inspection.FrontRightTireTread,
                //    FrontRightTireAlignment = inspection.FrontRightTireAlignment,

                //    FrontLeftBrake = inspection.FrontLeftBrake,
                //    FrontLeftTireTread = inspection.FrontLeftTireTread,
                //    FrontLeftTireAlignment = inspection.FrontLeftTireAlignment,

                //    RearRightBrake = inspection.RearRightBrake,
                //    RearRightTireTread = inspection.RearRightTireTread,
                //    RearRightTireAlignment = inspection.RearRightTireAlignment,

                //    RearLeftBrake = inspection.RearLeftBrake,
                //    RearLeftTireTread = inspection.RearLeftTireTread,
                //    RearLeftTireAlignment = inspection.RearLeftTireAlignment,

                //    TiresComments = inspection.TiresComments,

                //    Brakes = inspection.Brakes,
                //    TireTread = inspection.TireTread,
                //    TireAlignment = inspection.TireAlignment,
                //    Headlights = inspection.Headlights,
                //    Taillights = inspection.Taillights,
                //    TurnSignals = inspection.TurnSignals,
                //    BrakeLights = inspection.BrakeLights,
                //    HazardLights = inspection.HazardLights,
                //    WindshieldWasherFluid = inspection.WindshieldWasherFluid,
                //    WindshieldWiperBlades = inspection.WindshieldWiperBlades,
                //    WindshieldCondition = inspection.WindshieldCondition,
                //    Mirrors = inspection.Mirrors,
                //    EmergencyBrake = inspection.EmergencyBrake,
                //    Horn = inspection.Horn,
                //    FuelTankCap = inspection.FuelTankCap,
                //    AirConditioningFilter = inspection.AirConditioningFilter,
                //    ReversingLights = inspection.ReversingLights,


                //    InteriorAndExteriorComments = inspection.InteriorAndExteriorComments,

                //    EngineOilLevel = inspection.EngineOilLevel,
                //    CoolantLevel = inspection.CoolantLevel,
                //    BrakeFluidLevel = inspection.BrakeFluidLevel,
                //    AirFilter = inspection.AirFilter,
                //    RadiatorHoses = inspection.RadiatorHoses,
                //    HeatingHoses = inspection.HeatingHoses,
                //    AirConditioningCondenser = inspection.AirConditioningCondenser,

                //    EngineComments = inspection.EngineComments,

                //    BatteryTerminals = inspection.BatteryTerminals,
                //    BatteryCables = inspection.BatteryCables,
                //    BatteryMounting = inspection.BatteryMounting,
                //    GeneralBatteryCondition = inspection.GeneralBatteryCondition,

                //    BatteryComments = inspection.BatteryComments,

                //    ChasisComments = inspection.ChasisComments,

                //    Observations = inspection.Observations,

                //};

                //inspectionDocumentDto.Photos = photos
                //    .Select(p => new WorkshopVehicleFileDto
                //    {
                //        FileName = p.FileName,
                //        FileType = NormalizeFileType(p.FileName, p.FileType),
                //        FileData = p.FileData
                //    }).ToList();

                    //var pdfBytes = await GeneratePdf(inspectionDocumentDto);

                //vehicleInspectionHistory.File = pdfBytes;

                await _context.SaveChangesAsync();

                return Ok("inspection-created");
            }
            catch (Exception ex)
            {
                return BadRequest("unkwnown-error");
            }
        }


        [HttpGet("download-file/{fileId}")]
        public async Task<ActionResult> DownloadWorkshopVehicleFile(int fileId)
        {
            try 
            {
                var file = await _context.WorkshopVehicleFiles.FindAsync(fileId);
                if (file == null) return NotFound("not-found");

                return File(file.FileData, file.FileType, file.FileName);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }


        [HttpGet("detail-history/{vehicleInspectionId}")]
        public async Task<ActionResult<ICollection<VehicleInspectionDetailHistoryResponseDto>>> GetVehicleDetailInspection(int vehicleInspectionId)
        {
            try
            {
                var vehicleDetailsHistory = await _context.VehicleInspectionDetailHistory
                    .Where(v => v.VehicleInspectionId == vehicleInspectionId)
                    .ToListAsync();

                if (vehicleDetailsHistory == null) return NotFound("not-found");

                var vehicleDetailsHistoryDto = new List<VehicleInspectionDetailHistoryResponseDto>();

                foreach (var model in vehicleDetailsHistory)
                {
                    var vehicleDetailHistoryDto = new VehicleInspectionDetailHistoryResponseDto
                    {
                        VehicleInspectionDetailHistoryId = model.Id,
                        Title = model.Title,
                        IsCompleted = model.IsCompleted,
                        CreatedAt = model.CreatedAt,
                        CompletedAt = model.CompletedAt,
                    };
                    vehicleDetailsHistoryDto.Add(vehicleDetailHistoryDto);
                }

                return Ok(vehicleDetailsHistoryDto);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpGet("history/{vehicleInspectionId}")]
        public async Task<ActionResult<ICollection<VehicleInspectionHistoryResponseDto>>> GetVehicleInspectionHistory(int vehicleInspectionId)
        {
            try
            {
                var vehicleInspectionsHistory = await _context.VehicleInspectionHistory
                    .Where(v => v.VehicleInspectionId == vehicleInspectionId)
                    .ToListAsync();

                if (vehicleInspectionsHistory == null) return NotFound("not-found");

                var vehicleInspectionsHistoryDto = new List<VehicleInspectionHistoryResponseDto>();

                foreach (var model in vehicleInspectionsHistory)
                {
                    var vehicleInspectionHistoryDto = new VehicleInspectionHistoryResponseDto
                    {
                        VehicleInspectionHistoryId = model.Id,
                        Title = model.Title,
                        Folio = model.Folio,
                        File = model.File,
                        CreatedAt = model.CreatedAt,
                    };
                    vehicleInspectionsHistoryDto.Add(vehicleInspectionHistoryDto);
                }

                return Ok(vehicleInspectionsHistoryDto);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPost("get-history/customer-workshop")]
        public async Task<ActionResult<ICollection<VehicleInspectionHistoryResponseDto>>> GetVehicleInspectionHistoryCustomerWorkshop(CustomerWorkshopBodyDto model)
        {
            try
            {
                var vehicleInspectionsIds = await _context.WorkshopVehicleInspections
                    .Where(v => v.WorkshopId == model.WorkshopId && v.CustomerId ==  model.CustomerId)
                    .Select(v => v.Id)
                    .ToListAsync();

                var vehicleInspectionsHistory = await _context.VehicleInspectionHistory
                    .Where(v => vehicleInspectionsIds.Contains(v.Id))
                    .ToListAsync();

                if (vehicleInspectionsHistory == null) return NotFound("not-found");

                var vehicleInspectionsHistoryDto = new List<VehicleInspectionHistoryResponseDto>();

                foreach (var vehicleInspection in vehicleInspectionsHistory)
                {
                    var vehicleInspectionHistoryDto = new VehicleInspectionHistoryResponseDto
                    {
                        VehicleInspectionHistoryId = vehicleInspection.Id,
                        Title = vehicleInspection.Title,
                        Folio = vehicleInspection.Folio,
                        File = vehicleInspection.File,
                        CreatedAt = vehicleInspection.CreatedAt,
                    };
                    vehicleInspectionsHistoryDto.Add(vehicleInspectionHistoryDto);
                }

                return Ok(vehicleInspectionsHistoryDto);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPost("get-history/customer-workshop/vehicle")]
        public async Task<ActionResult<ICollection<VehicleInspectionHistoryResponseDto>>> GetVehicleInspectionHistoryCustomerWorkshopVehicle(CustomerWorkshopVehicleBodyDto model)
        {
            try
            {
                var vehicleInspectionsIds = await _context.WorkshopVehicleInspections
                    .Where(v => v.WorkshopId == model.WorkshopId && v.CustomerId == model.CustomerId && v.VehicleId == model.VehicleId)
                    .Select(v => v.Id)
                    .ToListAsync();

                var vehicleInspectionsHistory = await _context.VehicleInspectionHistory
                    .Where(v => vehicleInspectionsIds.Contains(v.Id))
                    .ToListAsync();

                if (vehicleInspectionsHistory == null) return NotFound("not-found");

                var vehicleInspectionsHistoryDto = new List<VehicleInspectionHistoryResponseDto>();

                foreach (var vehicleInspection in vehicleInspectionsHistory)
                {
                    var vehicleInspectionHistoryDto = new VehicleInspectionHistoryResponseDto
                    {
                        VehicleInspectionHistoryId = vehicleInspection.Id,
                        Title = vehicleInspection.Title,
                        Folio = vehicleInspection.Folio,
                        File = vehicleInspection.File,
                        CreatedAt = vehicleInspection.CreatedAt,
                    };
                    vehicleInspectionsHistoryDto.Add(vehicleInspectionHistoryDto);
                }

                return Ok(vehicleInspectionsHistoryDto);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPost("create-detail-history")]
        [AuthorizeEmployee("Administrador", "Registrar vehículos")]
        public async Task<ActionResult> CreateVehicleInspectionDetailHistory(PostVehicleInspectionDetailDto model)
        {
            try
            {
                var vehicleInspections = await _context.WorkshopVehicleInspections
                    .Where(v => v.Id == model.VehicleInspectionId)
                    .FirstOrDefaultAsync();

                if (vehicleInspections == null) return NotFound("not-found");

                var vehicleInspectionDetailHistory = new VehicleInspectionDetailHistory
                {
                    VehicleInspectionId = model.VehicleInspectionId,
                    Title = model.Title,
                    IsCompleted = model.IsCompleted,
                    CompletedAt = model.CompletedAt,
                };

                _context.VehicleInspectionDetailHistory.Add(vehicleInspectionDetailHistory);

                // Agregar notificación al cliente. VehicleInspectionUpdated.
                var notification = new Notifications
                {
                    UserId = vehicleInspections.CustomerId,
                    UserType = UserType.Customer,
                    Title = "Su inspección tiene cambios",
                    Content = $"{vehicleInspectionDetailHistory.Title}",
                    Event = "VehicleInspectionUpdated"
                };
                _context.Notifications.Add(notification);

                await _context.SaveChangesAsync();

                // TODO: Activar cuando se implementen las notificaciones push Firebase
                // Enviar notificación push al cliente
                //await _firebaseNotificationService.SendNotificationToCustomerAsync(
                //    vehicleInspections.CustomerId,
                //    notification.Title,
                //    notification.Content,
                //    notification.Event,
                //    new Dictionary<string, string> { { "vehicleInspectionId", model.VehicleInspectionId.ToString() } }
                //);

                return Ok("vehicle-inspection-detail-history-created");
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPut("update-detail-history/{vehicleInspectionDetailHistoryId}")]
        public async Task<ActionResult> UpdateVehicleInspectionDetailHistory(int vehicleInspectionDetailHistoryId, PutVehicleInspectionDetailHistoryDto model)
        {
            try
            {
                var vehicleInspectionDetailHistory = await _context.VehicleInspectionDetailHistory
                    .Where(v => v.Id == vehicleInspectionDetailHistoryId)
                    .Include(v => v.VehicleInspection)
                    .FirstOrDefaultAsync();

                if (vehicleInspectionDetailHistory == null) return NotFound("not-found");

                vehicleInspectionDetailHistory.Title = model.Title;
                vehicleInspectionDetailHistory.IsCompleted = model.IsCompleted;
                vehicleInspectionDetailHistory.CompletedAt = model.CompletedAt;

                //// Agregar notificación al cliente. VehicleInspectionUpdated.
                //var notification = new Notifications
                //{
                //    UserId = vehicleInspectionDetailHistory.VehicleInspection.CustomerId,
                //    UserType = UserType.Customer,
                //    Title = "Su inspección tiene cambios",
                //    Content = $"{vehicleInspectionDetailHistory.Title}",
                //    Event = "VehicleInspectionUpdated"
                //};
                //_context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                return Ok("vehicle-inspection-detail-history-updated");
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPost("customer-request-status/{vehicleInspectionId}")]
        public async Task<ActionResult> CustomerRequestStatus(int vehicleInspectionId)
        {
            try
            {
                var vehicleInspection = await _context.WorkshopVehicleInspections
                    .Where(v => v.Id == vehicleInspectionId && v.IsActive)
                    .Include(v => v.Customer)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.Brand)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.VehicleModel)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.VehicleVersion)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.VehicleType)
                    .FirstOrDefaultAsync();

                if (vehicleInspection == null) return NotFound("not-found");

                var vehicleBrand = vehicleInspection.Vehicle.BrandId == -1 ? vehicleInspection.Vehicle.OtherBrand : vehicleInspection.Vehicle.Brand.Name;
                var vehicleModel = vehicleInspection.Vehicle.VehicleModelId == -1 ? vehicleInspection.Vehicle.OtherVehicleModel : vehicleInspection.Vehicle.VehicleModel.Model;
                var vehicleVersion = vehicleInspection.Vehicle.VehicleVersionId == -1 ? vehicleInspection.Vehicle.OtherVehicleVersion : vehicleInspection.Vehicle.VehicleVersion.Version;
                var vehicleType = vehicleInspection.Vehicle.VehicleTypeId == -1 ? vehicleInspection.Vehicle.OtherVehicleType : vehicleInspection.Vehicle.VehicleType.Type;
                var fullVehicleName = $"{vehicleBrand} - {vehicleModel} - {vehicleVersion} - {vehicleType}";            

                var workshopInbox = new WorkshopInbox
                {
                    WorkshopId = vehicleInspection.WorkshopId,
                    CustomerId = vehicleInspection.CustomerId,
                    VehicleId = vehicleInspection.VehicleId,
                    ParentModelType = "VehicleInspection",
                    ParentModelId = vehicleInspection.Id,
                    Title = "Solicitud de estatus de inspección",
                    Details = $"El cliente {vehicleInspection.Customer.FullName} solicita estatus de inspección para su vehículo {fullVehicleName}.",

                };

                await _context.WorkshopInbox.AddAsync(workshopInbox);
                await _context.SaveChangesAsync();

                // TODO: Activar cuando se implementen las notificaciones push Firebase
                // Enviar notificación push al taller
                //await _firebaseNotificationService.SendNotificationToWorkshopAsync(
                //    vehicleInspection.WorkshopId,
                //    workshopInbox.Title,
                //    workshopInbox.Details,
                //    workshopInbox.ParentModelType,
                //    new Dictionary<string, string> { { "vehicleInspectionId", vehicleInspectionId.ToString() } }
                //);

                return Ok("message-sent");
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPut("end-inspection/{inspectionId}")]
        [AuthorizeEmployee("Administrador", "Registrar vehículos")]
        public async Task<ActionResult> EndVehicleInspection(int inspectionId)
        {
            try
            {
                var vehicleInspection = await _context.WorkshopVehicleInspections
                    .Where(v => v.Id == inspectionId && v.IsActive)
                    .FirstOrDefaultAsync();

                if (vehicleInspection == null)
                {
                    return NotFound("not-found");
                }

                vehicleInspection.IsActive = false;


                await _context.SaveChangesAsync();

                return Ok("inspection-ended");
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpGet("generate-pdf/{inspectionId}" )]
        public IActionResult GeneratePdf(int inspectionId)
        {
            var document = new SampleDocument();
            var pdfBytes = document.GeneratePdf();

            return File(pdfBytes, "application/pdf", "documento.pdf");
        }

        static string GenerateFolio()
        {
            string date = DateTime.Now.ToString("yyyyMMddHHmmss");

            Random random = new Random();
            string letters = "";
            for (int i = 0; i < 3; i++)
            {
                letters += (char)random.Next('A', 'Z' + 1);
            }

            return date + letters + "A";
        }


        //[HttpPost("generate-pdf")]
        //public IActionResult GeneratePdf([FromBody] PostWorkshopVehicleInspectionDto dto)
        //{
        //    var document = new VehicleInspectionDocument(dto);
        //    var pdfBytes = document.GeneratePdf();

        //    return File(pdfBytes, "application/pdf", $"inspeccion-{DateTime.Now:yyyyMMddHHmmss}.pdf");
        //}



        [HttpGet("generate-pdf2/{id}")]
        public async Task<IActionResult> GeneratePdfFromDb(int id)
        {

            var inspectionFile = await _context.VehicleInspectionHistory
                .Where(ifi => ifi.Id == id)
                .FirstOrDefaultAsync();

            if (inspectionFile == null)
            {
                return NotFound("not-found");
            }

            var inspection = await _context.WorkshopVehicleInspections
                .Include(i => i.Workshop)
                .Include(i => i.Customer)
                .Include(i => i.Vehicle)
                .ThenInclude(i => i.Brand)
                .Include(i => i.Vehicle)
                .ThenInclude(i => i.VehicleModel)
                .Include(i => i.Vehicle)
                .ThenInclude(i => i.VehicleVersion)
                .Include(i => i.Vehicle)
                .ThenInclude(i => i.VehicleType)
                .FirstOrDefaultAsync(i => i.Id == inspectionFile.VehicleInspectionId);
            

            var photos = await _context.WorkshopVehicleFiles
            .Where(f => f.WorkshopVehicleInspectionId == inspection.Id)
            .ToListAsync();

            

            var inspectionDto = new VehicleInspectionDocumentDto
            {
                InspectionDate = inspection.InspectionDate,
                Folio = inspectionFile.Folio,
                CustomerName = inspection.Customer.FullName,
                CustomerPhoneNumber = inspection.Customer.NormalizedPhoneNumber,
                CustomerGuid = inspection.Customer.Id.ToString().Substring(0, 13),

                WorkshopName = inspection.Workshop.WorkshopName,
                WorkshopPhoneNumber = inspection.Workshop.NormalizedPhoneNumber,
                WorkshopGuid = inspection.Workshop.Id.ToString().Substring(0, 13),

                Vehicle = new VehicleResponseDto
                {
                    Plates = inspection.Vehicle.Plates,
                    Year = inspection.Vehicle.Year,
                    VehicleFormat = inspection.Vehicle.VehicleFormat,
                    Brand = inspection.Vehicle.BrandId == -1 ? inspection.Vehicle.OtherBrand : inspection.Vehicle.Brand.Name,
                    Model = inspection.Vehicle.VehicleModelId == -1 ? inspection.Vehicle.OtherVehicleModel : inspection.Vehicle.VehicleModel.Model,
                    Version = inspection.Vehicle.VehicleVersionId == -1 ? inspection.Vehicle.OtherVehicleVersion : inspection.Vehicle.VehicleVersion.Version,
                    Type = inspection.Vehicle.VehicleTypeId == -1 ? inspection.Vehicle.OtherVehicleType : inspection.Vehicle.VehicleType.Type,
                },

                FrontRightBrake = inspection.FrontRightBrake,
                FrontRightTireTread = inspection.FrontRightTireTread,
                FrontRightTireAlignment = inspection.FrontRightTireAlignment,

                FrontLeftBrake = inspection.FrontLeftBrake,
                FrontLeftTireTread = inspection.FrontLeftTireTread,
                FrontLeftTireAlignment = inspection.FrontLeftTireAlignment,

                RearRightBrake = inspection.RearRightBrake,
                RearRightTireTread = inspection.RearRightTireTread,
                RearRightTireAlignment = inspection.RearRightTireAlignment,

                RearLeftBrake = inspection.RearLeftBrake,
                RearLeftTireTread = inspection.RearLeftTireTread,
                RearLeftTireAlignment = inspection.RearLeftTireAlignment,

                TiresComments = inspection.TiresComments,

                Brakes = inspection.Brakes,
                TireTread = inspection.TireTread,
                TireAlignment = inspection.TireAlignment,
                Headlights = inspection.Headlights,
                Taillights = inspection.Taillights,
                TurnSignals = inspection.TurnSignals,
                BrakeLights = inspection.BrakeLights,
                HazardLights = inspection.HazardLights,
                WindshieldWasherFluid = inspection.WindshieldWasherFluid,
                WindshieldWiperBlades = inspection.WindshieldWiperBlades,
                WindshieldCondition = inspection.WindshieldCondition,
                Mirrors = inspection.Mirrors,
                EmergencyBrake = inspection.EmergencyBrake,
                Horn = inspection.Horn,
                FuelTankCap = inspection.FuelTankCap,
                AirConditioningFilter = inspection.AirConditioningFilter,
                ReversingLights = inspection.ReversingLights,
                LicensePlateLight = inspection.LicensePlateLight,
                SeatBelts = inspection.SeatBelts,


                InteriorAndExteriorComments = inspection.InteriorAndExteriorComments,

                EngineOilLevel = inspection.EngineOilLevel,
                CoolantLevel = inspection.CoolantLevel,
                BrakeFluidLevel = inspection.BrakeFluidLevel,
                AirFilter = inspection.AirFilter,
                RadiatorHoses = inspection.RadiatorHoses,
                HeatingHoses = inspection.HeatingHoses,
                AirConditioningCondenser = inspection.AirConditioningCondenser,
                TransmissionFluidLevel = inspection.TransmissionFluidLevel,
                PowerSteeringFluidLevel = inspection.PowerSteeringFluidLevel,
                AccessoryBelt = inspection.AccessoryBelt,
                ExhaustSystem = inspection.ExhaustSystem,

                EngineComments = inspection.EngineComments,

                BatteryTerminals = inspection.BatteryTerminals,
                BatteryCables = inspection.BatteryCables,
                BatteryMounting = inspection.BatteryMounting,
                GeneralBatteryCondition = inspection.GeneralBatteryCondition,

                BatteryComments = inspection.BatteryComments,

                FrontShockAbsorbers = inspection.FrontShockAbsorbers,
                RearShockAbsorbers = inspection.RearShockAbsorbers,
                BallJoints = inspection.BallJoints,
                SteeringRackAndTierods = inspection.SteeringRackAndTierods,
                SuspensionBushings = inspection.SuspensionBushings,

                SuspensionComments = inspection.SuspensionComments,

                ChasisComments = inspection.ChasisComments,

                Observations = inspection.Observations,

            };

            inspectionDto.Photos = photos
                .Select(p => new WorkshopVehicleFileDto
                {
                    FileName = p.FileName,
                    FileType = NormalizeFileType(p.FileName, p.FileType),
                    FileData = p.FileData
                }).ToList();

            if (inspection == null)
                return NotFound("Inspección no encontrada.");

            var pdf = new VehicleInspectionDocument(inspectionDto).GeneratePdf();
            return File(pdf, "application/pdf", $"inspeccion-{id}.pdf");
        }


        //public async Task<byte[]> GeneratePdf(VehicleInspectionDocumentDto inspectionDto)
        //{
        //    var document = new WorkshopVehicleInspectionDocument(inspectionDto);
        //    var pdfBytes = document.GeneratePdf();
        //    return pdfBytes;
        //}


        private string NormalizeFileType(string fileName, string originalType)
        {
            if (!string.IsNullOrWhiteSpace(originalType) && originalType.StartsWith("image/"))
                return originalType;

            var ext = Path.GetExtension(fileName).ToLower();
            return ext switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                _ => "application/octet-stream"
            };
        }

    }
}
