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
using MiTaller.Models.Domain;
using MiTaller.Models;
using System.Collections.Generic;

namespace MiTaller.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WorkshopMotocycleInspectionController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly UserManager<BaseIdentityUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly FirebaseNotificationService _firebaseNotificationService;

        public WorkshopMotocycleInspectionController(DataContext context, UserManager<BaseIdentityUser> userManager, IEmailSender emailSender, FirebaseNotificationService firebaseNotificationService)
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
                var inspection = await _context.WorkshopMotocycleInspections
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
        public async Task<ActionResult> GetWorkshopMotocycleInspectionsByWorkshop(Guid workshopId)
        {
            try
            {
                var inspections = await _context.WorkshopMotocycleInspections
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

        [HttpGet("active-motocycles-in-workshop/{workshopId}")]
        public async Task<ActionResult<ICollection<VehicleInWorkshopResponseDto>>> GetMotocycleInspectionsByWorkshop(Guid workshopId)
        {
            try
            {
                var inspections = await _context.WorkshopMotocycleInspections
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

        [HttpPost("active-motocycles-in-workshop-pager/{workshopId}")]
        public async Task<ActionResult<PagerResponseDto<VehicleInWorkshopResponseDto>>> GetMotocycleInspectionsByWorkshopPaged(Guid workshopId, [FromBody] PagerBodyDto pager)
        {
            try
            {
                var query = _context.WorkshopMotocycleInspections
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

        [HttpGet("finished-motocycles-in-workshop/{workshopId}")]
        public async Task<ActionResult<ICollection<VehicleInWorkshopResponseDto>>> GetFinishedMotocycleInspectionsByWorkshop(Guid workshopId)
        {
            try
            {
                var inspections = await _context.WorkshopMotocycleInspections
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

        [HttpPost("finished-motocycles-in-workshop-pager/{workshopId}")]
        public async Task<ActionResult<PagerResponseDto<VehicleInWorkshopResponseDto>>> GetFinishedMotocycleInspectionsByWorkshopPaged(Guid workshopId, [FromBody] PagerBodyDto pager)
        {
            try
            {
                var query = _context.WorkshopMotocycleInspections
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

        [HttpGet("motocycles-in-workshop-full-info/{workshopId}")]
        public async Task<ActionResult<ICollection<MotocycleInWorkshopFullInfoResponseDto>>> GetMotocyclesInWorkshopFullInfo(Guid workshopId)
        {
            try
            {
                var workshop = await _context.Workshops
                    .Where(w => w.Id == workshopId && !w.IsDeleted)
                    .FirstOrDefaultAsync();

                if (workshop == null) return NotFound("not-found");

                var inspections = await _context.WorkshopMotocycleInspections
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

                var result = inspections.Select(i => new MotocycleInWorkshopFullInfoResponseDto
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
                    InspectionHistory = []
                }).ToList();

                return Ok(result);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPost("get-motocycle-in-workshop-history")]
        public async Task<ActionResult<VehicleInspectionHistoryResponseDto>> GetMotocycleHistoryInWorkshop(CustomerWorkshopVehicleBodyDto model)
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

                var inspectionHistoriesDto = await _context.MotocycleInspectionHistory
                    .Where(h => h.MotocycleInspection.WorkshopId == model.WorkshopId
                             && h.MotocycleInspection.CustomerId == model.CustomerId
                             && h.MotocycleInspection.VehicleId == model.VehicleId)
                    .OrderByDescending(w => w.MotocycleInspection.InspectionDate)
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
                //    QuotationHistory = quotationHistoriesDto,
                //};

                return Ok(inspectionHistoriesDto);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPost("get-motocycle-in-workshop-history-pager")]
        public async Task<ActionResult<PagerResponseDto<VehicleInspectionHistoryResponseDto>>> GetMotocycleHistoryInWorkshopPaged([FromBody] CustomerWorkshopVehicleBodyDto model, [FromQuery] PagerBodyDto pager)
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

                var query = _context.MotocycleInspectionHistory
                    .Where(h => h.MotocycleInspection.WorkshopId == model.WorkshopId
                             && h.MotocycleInspection.CustomerId == model.CustomerId
                             && h.MotocycleInspection.VehicleId == model.VehicleId)
                    .OrderByDescending(w => w.MotocycleInspection.InspectionDate)
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

                var pagedInspections = await query
                    .Skip((pager.PageNumber - 1) * pager.PageSize)
                    .Take(pager.PageSize)
                    .ToListAsync();

                var response = new PagerResponseDto<VehicleInspectionHistoryResponseDto>
                {
                    CurrentPage = pager.PageNumber,
                    TotalPages = totalPages,
                    Elements = pagedInspections
                };

                return Ok(response);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpGet("motocycles-in-workshop-by/{customerId}")]
        public async Task<ActionResult<ICollection<VehicleInWorkshopResponseDto>>> GetMotocyclesInspectionsByCustomer(Guid customerId)
        {
            try
            {
                var inspections = await _context.WorkshopMotocycleInspections
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

        [HttpPost("motocycles-in-workshop-by-pager/{customerId}")]
        public async Task<ActionResult<PagerResponseDto<VehicleInWorkshopResponseDto>>> GetMotocyclesInspectionsByCustomerPaged(Guid customerId, [FromBody] PagerBodyDto pager)
        {
            try
            {
                var query = _context.WorkshopMotocycleInspections
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

        [HttpPost("customer-get-motocycle-in-workshop-history")]
        public async Task<ActionResult<VehicleInspectionHistoryResponseDto>> GetCustomerMotocycleHistory(CustomerVehicleBodyDto model)
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

                var inspectionHistoriesDto = await _context.MotocycleInspectionHistory
                    .Where(h => h.MotocycleInspection.CustomerId == model.CustomerId
                             && h.MotocycleInspection.VehicleId == model.VehicleId)
                    .OrderByDescending(w => w.MotocycleInspection.InspectionDate)
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

        [HttpPost("customer-get-motocycle-in-workshop-history-pager")]
        public async Task<ActionResult<PagerResponseDto<VehicleInspectionHistoryResponseDto>>> GetCustomerMotocycleHistoryPaged([FromBody] CustomerVehicleBodyDto model, [FromQuery] PagerBodyDto pager)
        {
            try
            {
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(w => w.Id == model.CustomerId && !w.IsDeleted);
                if (customer == null) return NotFound("not-found");

                var vehicle = await _context.Vehicles
                    .FirstOrDefaultAsync(w => w.Id == model.VehicleId && !w.IsDeleted);
                if (vehicle == null) return NotFound("not-found");

                var query = _context.MotocycleInspectionHistory
                    .Where(h => h.MotocycleInspection.CustomerId == model.CustomerId
                             && h.MotocycleInspection.VehicleId == model.VehicleId)
                    .OrderByDescending(w => w.MotocycleInspection.InspectionDate)
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
        public async Task<ActionResult> CreateWorkshopMotocycleInspection([FromForm] PostWorkshopMotocycleInspectionDto model)
        {
            try
            {
                var workshop = await _context.Workshops
                    .Where(w => w.Id == model.WorkshopId && !w.IsDeleted)
                    .FirstOrDefaultAsync();
                if (workshop == null) return NotFound("not-found");

                Customer customer;
                Vehicle vehicle;

                var activeInspection = await _context.WorkshopMotocycleInspections
                    .Where(a => a.WorkshopId == model.WorkshopId
                            && a.CustomerId == model.CustomerId
                            && a.VehicleId == model.VehicleId
                            && a.IsActive)
                    .FirstOrDefaultAsync();

                if (activeInspection != null)
                {
                    return BadRequest("cannot-register-duplicated-inspection-appointment");
                }

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
                                al que llevaste tu motocicleta para revisión o servicio.
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

                var inspection = new WorkshopMotocycleInspection
                {
                    Workshop = workshop,
                    Customer = customer,
                    Vehicle = vehicle,
                    IsNewCustomer = model.IsNewCustomer,
                    IsNewVehicle = model.IsNewVehicle,
                    IsActive = true,

                    FrontRadios = model.FrontRadios,
                    FrontTireThreadPattern = model.FrontTireThreadPattern,
                    FrontBearings = model.FrontBearings,
                    FrontStamps = model.FrontStamps,
                    FrontBrakeLining = model.FrontBrakeLining,
                    FrontWearPattern = model.FrontWearPattern,

                    RearRadios = model.RearRadios,
                    RearTireThreadPattern = model.RearTireThreadPattern,
                    RearBearings = model.RearBearings,
                    RearStamps = model.RearStamps,
                    RearBrakeLining = model.RearBrakeLining,
                    RearWearPattern = model.RearWearPattern,

                    TiresComments = model.TiresComments,

                    Headlight = model.Headlight,
                    Taillight = model.Taillight,
                    TurnSignals = model.TurnSignals,
                    HazardLights = model.HazardLights,
                    Stoplight = model.Stoplight,
                    LicensePlateLight = model.LicensePlateLight,
                    LeftMirror = model.LeftMirror,
                    RightMirror = model.RightMirror,
                    Switches = model.Switches,
                    Cabling = model.Cabling,
                    HandleBars = model.HandleBars,
                    LeversAndPedal = model.LeversAndPedal,
                    Hoses = model.Hoses,
                    ThrottleLever = model.ThrottleLever,
                    ClutchLever = model.ClutchLever,
                    FuelTankCap = model.FuelTankCap,
                    DashboardInstruments = model.DashboardInstruments,
                    Horn = model.Horn,

                    LightsAndControlsComments = model.LightsAndControlsComments,

                    FrameCondition = model.FrameCondition,
                    SteeringBearings = model.SteeringBearings,
                    SwingarmBushings = model.SwingarmBushings,
                    FrontForks = model.FrontForks,
                    RearShockAbsorbers = model.RearShockAbsorbers,
                    ChainOrStrap = model.ChainOrStrap,
                    Fasteners = model.Fasteners,
                    CentralSupport = model.CentralSupport,
                    LateralSupport = model.LateralSupport,

                    FrameAndSuspensionComments = model.FrameAndSuspensionComments,

                    EngineOil = model.EngineOil,
                    GearOil = model.GearOil,
                    AxleTransmissionOil = model.AxleTransmissionOil,
                    HydraulicFluid = model.HydraulicFluid,
                    Refrigerant = model.Refrigerant,
                    Fuel = model.Fuel,
                    Leaks = model.Leaks,

                    OilAndLevelsComments = model.OilAndLevelsComments,

                    BatteryTerminals = model.BatteryTerminals,
                    Cables = model.Cables,
                    Mounting = model.Mounting,
                    GeneralBatteryConditions = model.GeneralBatteryConditions,

                    BatteryComments = model.BatteryComments,

                    ChasisComments = model.ChasisComments,

                    Observations = model.Observations,
                };

                _context.WorkshopMotocycleInspections.Add(inspection);
                await _context.SaveChangesAsync();

                // Archivos Adjuntos
                if (model.Files != null && model.Files.Any())
                {
                    foreach (var file in model.Files)
                    {
                        using (var ms = new MemoryStream())
                        {
                            await file.CopyToAsync(ms);
                            var newFile = new MotocycleInspectionFile
                            {
                                WorkshopMotocycleInspectionId = inspection.Id,
                                FileName = file.FileName,
                                FileType = file.ContentType,
                                FileData = ms.ToArray()
                            };

                            _context.MotocycleInspectionFiles.Add(newFile);
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                // Registro InspectionHistory
                var motocycleInspectionHistory = new MotocycleInspectionHistory
                {
                    MotocycleInspectionId = inspection.Id,
                    Title = "Solicitud de reparación",
                    Folio = GenerateFolio(),
                    File = null,
                    CreatedAt = DateTime.Now,
                };
                _context.MotocycleInspectionHistory.Add(motocycleInspectionHistory);

                // Registro InspectionDetailHistory
                var motocycleInspectionDetailHistory = new MotocycleInspectionDetailHistory
                {
                    MotocycleInspectionId = inspection.Id,
                    Title = "Entró a taller",
                    IsCompleted = true,
                    CompletedAt = DateTime.Now,
                };
                _context.MotocycleInspectionDetailHistory.Add(motocycleInspectionDetailHistory);

                await _context.SaveChangesAsync();

                return Ok("inspection-created");
            }
            catch (Exception ex)
            {
                return BadRequest("unkwnown-error");
            }
        }


        [HttpGet("download-file/{fileId}")]
        public async Task<ActionResult> DownloadWorkshopMotocycleFile(int fileId)
        {
            try
            {
                var file = await _context.MotocycleInspectionFiles.FindAsync(fileId);
                if (file == null) return NotFound("not-found");

                return File(file.FileData, file.FileType, file.FileName);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }


        [HttpGet("detail-history/{motocycleInspectionId}")]
        public async Task<ActionResult<ICollection<MotocycleInspectionDetailHistoryResponseDto>>> GetMotocycleDetailInspection(int motocycleInspectionId)
        {
            try
            {
                var motocycleDetailsHistory = await _context.MotocycleInspectionDetailHistory
                    .Where(v => v.MotocycleInspectionId == motocycleInspectionId)
                    .ToListAsync();

                if (motocycleDetailsHistory == null) return NotFound("not-found");

                var motocycleDetailsHistoryDto = new List<MotocycleInspectionDetailHistoryResponseDto>();

                foreach (var model in motocycleDetailsHistory)
                {
                    var motocycleDetailHistoryDto = new MotocycleInspectionDetailHistoryResponseDto
                    {
                        MotocycleInspectionDetailHistoryId = model.Id,
                        Title = model.Title,
                        IsCompleted = model.IsCompleted,
                        CreatedAt = model.CreatedAt,
                        CompletedAt = model.CompletedAt,
                    };
                    motocycleDetailsHistoryDto.Add(motocycleDetailHistoryDto);
                }

                return Ok(motocycleDetailsHistoryDto);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpGet("history/{motocycleInspectionId}")]
        public async Task<ActionResult<ICollection<MotocycleInspectionHistoryResponseDto>>> GetMotocycleInspectionHistory(int motocycleInspectionId)
        {
            try
            {
                var motocycleInspectionsHistory = await _context.MotocycleInspectionHistory
                    .Where(v => v.MotocycleInspectionId == motocycleInspectionId)
                    .ToListAsync();

                if (motocycleInspectionsHistory == null) return NotFound("not-found");

                var motocycleInspectionsHistoryDto = new List<MotocycleInspectionHistoryResponseDto>();

                foreach (var model in motocycleInspectionsHistory)
                {
                    var motocycleInspectionHistoryDto = new MotocycleInspectionHistoryResponseDto
                    {
                        MotocycleInspectionHistoryId = model.Id,
                        Title = model.Title,
                        Folio = model.Folio,
                        File = model.File,
                        CreatedAt = model.CreatedAt,
                    };
                    motocycleInspectionsHistoryDto.Add(motocycleInspectionHistoryDto);
                }

                return Ok(motocycleInspectionsHistoryDto);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPost("get-history/customer-workshop")]
        public async Task<ActionResult<ICollection<MotocycleInspectionHistoryResponseDto>>> GetMotocycleInspectionHistoryCustomerWorkshop(CustomerWorkshopBodyDto model)
        {
            try
            {
                var motocycleInspectionsIds = await _context.WorkshopMotocycleInspections
                    .Where(v => v.WorkshopId == model.WorkshopId && v.CustomerId == model.CustomerId)
                    .Select(v => v.Id)
                    .ToListAsync();

                var motocycleInspectionsHistory = await _context.MotocycleInspectionHistory
                    .Where(v => motocycleInspectionsIds.Contains(v.Id))
                    .ToListAsync();

                if (motocycleInspectionsHistory == null) return NotFound("not-found");

                var motocycleInspectionsHistoryDto = new List<MotocycleInspectionHistoryResponseDto>();

                foreach (var motocycleInspection in motocycleInspectionsHistory)
                {
                    var motocycleInspectionHistoryDto = new MotocycleInspectionHistoryResponseDto
                    {
                        MotocycleInspectionHistoryId = motocycleInspection.Id,
                        Title = motocycleInspection.Title,
                        Folio = motocycleInspection.Folio,
                        File = motocycleInspection.File,
                        CreatedAt = motocycleInspection.CreatedAt,
                    };
                    motocycleInspectionsHistoryDto.Add(motocycleInspectionHistoryDto);
                }

                return Ok(motocycleInspectionsHistoryDto);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPost("get-history/customer-workshop-motocycle")]
        public async Task<ActionResult<ICollection<MotocycleInspectionHistoryResponseDto>>> GetMotocycleInspectionHistoryCustomerWorkshopMotocycleMotocycle(CustomerWorkshopVehicleBodyDto model)
        {
            try
            {
                var motocycleInspectionsIds = await _context.WorkshopMotocycleInspections
                    .Where(v => v.WorkshopId == model.WorkshopId && v.CustomerId == model.CustomerId && v.VehicleId == model.VehicleId)
                    .Select(v => v.Id)
                    .ToListAsync();

                var motocycleInspectionsHistory = await _context.MotocycleInspectionHistory
                    .Where(v => motocycleInspectionsIds.Contains(v.Id))
                    .ToListAsync();

                if (motocycleInspectionsHistory == null) return NotFound("not-found");

                var motocycleInspectionsHistoryDto = new List<MotocycleInspectionHistoryResponseDto>();

                foreach (var motocycleInspection in motocycleInspectionsHistory)
                {
                    var motocycleInspectionHistoryDto = new MotocycleInspectionHistoryResponseDto
                    {
                        MotocycleInspectionHistoryId = motocycleInspection.Id,
                        Title = motocycleInspection.Title,
                        Folio = motocycleInspection.Folio,
                        File = motocycleInspection.File,
                        CreatedAt = motocycleInspection.CreatedAt,
                    };
                    motocycleInspectionsHistoryDto.Add(motocycleInspectionHistoryDto);
                }

                return Ok(motocycleInspectionsHistoryDto);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPost("create-detail-history")]
        [AuthorizeEmployee("Administrador", "Registrar vehículos")]
        public async Task<ActionResult> CreateMotocycleInspectionDetailHistory(PostMotocycleInspectionDetailDto model)
        {
            try
            {
                var motocycleInspections = await _context.WorkshopMotocycleInspections
                    .Where(v => v.Id == model.MotocycleInspectionId)
                    .FirstOrDefaultAsync();

                if (motocycleInspections == null) return NotFound("not-found");

                var motocycleInspectionDetailHistory = new MotocycleInspectionDetailHistory
                {
                    MotocycleInspectionId = model.MotocycleInspectionId,
                    Title = model.Title,
                    IsCompleted = model.IsCompleted,
                    CompletedAt = model.CompletedAt,
                };

                _context.MotocycleInspectionDetailHistory.Add(motocycleInspectionDetailHistory);

                // Agregar la notificación para el cliente. MotocycleInspectionUpdated.
                var notification = new Notifications
                {
                    UserId = motocycleInspections.CustomerId,
                    UserType = UserType.Customer,
                    Title = "Su inspección tiene cambios",
                    Content = $"{motocycleInspectionDetailHistory.Title}",
                    Event = "MotocycleInspectionUpdated"
                };
                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                // TODO: Activar cuando se implementen las notificaciones push Firebase
                // Enviar notificación push al cliente
                //await _firebaseNotificationService.SendNotificationToCustomerAsync(
                //    motocycleInspections.CustomerId,
                //    notification.Title,
                //    notification.Content,
                //    notification.Event,
                //    new Dictionary<string, string> { { "motocycleInspectionId", model.MotocycleInspectionId.ToString() } }
                //);

                return Ok("motocycle-inspection-detail-history-created");
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPut("update-detail-history/{motocycleInspectionDetailHistoryId}")]
        public async Task<ActionResult> UpdateMotocycleInspectionDetailHistory(int motocycleInspectionDetailHistoryId, PutMotocycleInspectionDetailHistoryDto model)
        {
            try
            {
                var motocycleInspectionDetailHistory = await _context.MotocycleInspectionDetailHistory
                    .Where(v => v.Id == motocycleInspectionDetailHistoryId)
                    .Include(v => v.MotocycleInspection)
                    .FirstOrDefaultAsync();

                if (motocycleInspectionDetailHistory == null) return NotFound("not-found");

                motocycleInspectionDetailHistory.Title = model.Title;
                motocycleInspectionDetailHistory.IsCompleted = model.IsCompleted;
                motocycleInspectionDetailHistory.CompletedAt = model.CompletedAt;

                //// Agregar la notificación para el cliente. MotocycleInspectionUpdated.
                //var notification = new Notifications
                //{
                //    UserId = motocycleInspectionDetailHistory.MotocycleInspection.CustomerId,
                //    UserType = UserType.Customer,
                //    Title = "Su inspección tiene cambios",
                //    Content = $"{motocycleInspectionDetailHistory.Title}",
                //    Event = "MotocycleInspectionUpdated"
                //};
                //_context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                return Ok("motocycle-inspection-detail-history-updated");
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPost("customer-request-status/{motocycleInspectionId}")]
        public async Task<ActionResult> CustomerRequestStatus(int motocycleInspectionId)
        {
            try
            {
                var motocycleInspection = await _context.WorkshopMotocycleInspections
                    .Where(v => v.Id == motocycleInspectionId && v.IsActive)
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

                if (motocycleInspection == null) return NotFound("not-found");

                var vehicleBrand = motocycleInspection.Vehicle.BrandId == -1 ? motocycleInspection.Vehicle.OtherBrand : motocycleInspection.Vehicle.Brand.Name;
                var vehicleModel = motocycleInspection.Vehicle.VehicleModelId == -1 ? motocycleInspection.Vehicle.OtherVehicleModel : motocycleInspection.Vehicle.VehicleModel.Model;
                var vehicleVersion = motocycleInspection.Vehicle.VehicleVersionId == -1 ? motocycleInspection.Vehicle.OtherVehicleVersion : motocycleInspection.Vehicle.VehicleVersion.Version;
                var vehicleType = motocycleInspection.Vehicle.VehicleTypeId == -1 ? motocycleInspection.Vehicle.OtherVehicleType : motocycleInspection.Vehicle.VehicleType.Type;
                var fullVehicleName = $"{vehicleBrand} - {vehicleModel} - {vehicleVersion} - {vehicleType}";

                var workshopInbox = new WorkshopInbox
                {
                    WorkshopId = motocycleInspection.WorkshopId,
                    CustomerId = motocycleInspection.CustomerId,
                    VehicleId = motocycleInspection.VehicleId,
                    ParentModelType = "MotocycleInspection",
                    ParentModelId = motocycleInspection.Id,
                    Title = "Solicitud de estatus de inspección",
                    Details = $"El cliente {motocycleInspection.Customer.FullName} solicita estatus de inspección para su motocicleta {fullVehicleName}.",

                };

                await _context.WorkshopInbox.AddAsync(workshopInbox);
                await _context.SaveChangesAsync();

                // TODO: Activar cuando se implementen las notificaciones push Firebase
                // Enviar notificación push al taller
                //await _firebaseNotificationService.SendNotificationToWorkshopAsync(
                //    motocycleInspection.WorkshopId,
                //    workshopInbox.Title,
                //    workshopInbox.Details,
                //    workshopInbox.ParentModelType,
                //    new Dictionary<string, string> { { "motocycleInspectionId", motocycleInspectionId.ToString() } }
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
                var vehicleInspection = await _context.WorkshopMotocycleInspections
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

        [HttpGet("generate-pdf/{inspectionId}")]
        public IActionResult GeneratePdf(int inspectionId)
        {
            var document = new SampleDocument();
            var pdfBytes = document.GeneratePdf();

            return File(pdfBytes, "application/pdf", "documento.pdf");
        }


        [HttpGet("generate-pdf2/{id}")]
        public async Task<IActionResult> GeneratePdfFromDb(int id)
        {

            var inspectionFile = await _context.MotocycleInspectionHistory
                .Where(ifi => ifi.Id == id)
                .FirstOrDefaultAsync();

            if (inspectionFile == null)
            {
                return NotFound("not-found");
            }

            var inspection = await _context.WorkshopMotocycleInspections
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
                .FirstOrDefaultAsync(i => i.Id == inspectionFile.MotocycleInspectionId);


            var photos = await _context.MotocycleInspectionFiles
            .Where(f => f.WorkshopMotocycleInspectionId == inspection.Id)
            .ToListAsync();



            var inspectionDto = new MotocycleInspectionDocumentDto
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

                FrontRadios = inspection.FrontRadios,
                FrontTireThreadPattern = inspection.FrontTireThreadPattern,
                FrontBearings = inspection.FrontBearings,
                FrontStamps = inspection.FrontStamps,
                FrontBrakeLining = inspection.FrontBrakeLining,
                FrontWearPattern = inspection.FrontWearPattern,

                RearRadios = inspection.RearRadios,
                RearTireThreadPattern = inspection.RearTireThreadPattern,
                RearBearings = inspection.RearBearings,
                RearStamps = inspection.RearStamps,
                RearBrakeLining = inspection.RearBrakeLining,
                RearWearPattern = inspection.RearWearPattern,

                TiresComments = inspection.TiresComments,

                Headlight = inspection.Headlight,
                Taillight = inspection.Taillight,
                TurnSignals = inspection.TurnSignals,
                HazardLights = inspection.HazardLights,
                Stoplight = inspection.Stoplight,
                LicensePlateLight = inspection.LicensePlateLight,
                LeftMirror = inspection.LeftMirror,
                RightMirror = inspection.RightMirror,
                Switches = inspection.Switches,
                Cabling = inspection.Cabling,
                HandleBars = inspection.HandleBars,
                LeversAndPedal = inspection.LeversAndPedal,
                Hoses = inspection.Hoses,
                ThrottleLever = inspection.ThrottleLever,
                ClutchLever = inspection.ClutchLever,
                FuelTankCap = inspection.FuelTankCap,
                DashboardInstruments = inspection.DashboardInstruments,
                Horn = inspection.Horn,

                LightsAndControlsComments = inspection.LightsAndControlsComments,

                FrameCondition = inspection.FrameCondition,
                SteeringBearings = inspection.SteeringBearings,
                SwingarmBushings = inspection.SwingarmBushings,
                FrontForks = inspection.FrontForks,
                RearShockAbsorbers = inspection.RearShockAbsorbers,
                ChainOrStrap = inspection.ChainOrStrap,
                Fasteners = inspection.Fasteners,
                CentralSupport = inspection.CentralSupport,
                LateralSupport = inspection.LateralSupport,

                FrameAndSuspensionComments = inspection.FrameAndSuspensionComments,

                EngineOil = inspection.EngineOil,
                GearOil = inspection.GearOil,
                AxleTransmissionOil = inspection.AxleTransmissionOil,
                HydraulicFluid = inspection.HydraulicFluid,
                Refrigerant = inspection.Refrigerant,
                Fuel = inspection.Fuel,
                Leaks = inspection.Leaks,

                OilAndLevelsComments = inspection.OilAndLevelsComments,

                BatteryTerminals = inspection.BatteryTerminals,
                Cables = inspection.Cables,
                Mounting = inspection.Mounting,
                GeneralBatteryConditions = inspection.GeneralBatteryConditions,

                BatteryComments = inspection.BatteryComments,

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

            var pdf = new MotocycleInspectionDocument(inspectionDto).GeneratePdf();
            return File(pdf, "application/pdf", $"inspeccion-{id}.pdf");
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

            return date + letters + "M";
        }

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
