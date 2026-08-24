using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiTaller.Data;
using MiTaller.DTO;
using MiTaller.DTO.Address;
using MiTaller.DTO.Pager;
using MiTaller.DTO.Quotation;
using MiTaller.DTO.RoadsideAssistance;
using MiTaller.DTO.Vehicle;
using MiTaller.DTO.Workshop;
using MiTaller.DTO.Workshop.Inbox;
using MiTaller.DTO.Workshop.Note;
using MiTaller.Models;
using MiTaller.Models.Auth;
using MiTaller.Models.Notification;
using MiTaller.Models.Workshop;
using System.ComponentModel.DataAnnotations.Schema;
using MiTaller.Services;
using System.Collections.Generic;

namespace MiTaller.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RoadsideAssistanceController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly FirebaseNotificationService _firebaseNotificationService;

        public RoadsideAssistanceController(DataContext context, FirebaseNotificationService firebaseNotificationService)
        {
            _context = context;
            _firebaseNotificationService = firebaseNotificationService;
        }

        [HttpGet("{workshopId}")]
        public async Task<ActionResult<ICollection<RoadsideAssistanceResponseDto>>> GetWorkshopInbox(Guid workshopId)
        {
            try
            {
                var workshop = await _context.Workshops
                    .Where(w => w.Id == workshopId && !w.IsDeleted)
                    .FirstOrDefaultAsync();

                if (workshop == null)
                {
                    return NotFound("not-found");
                }

                var roadsideAssistances = await _context.RoadsideAssistances
                    .Where(w => w.WorkshopId == workshopId)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.Brand)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.VehicleModel)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.VehicleVersion)
                    .Include(q => q.Vehicle)
                        .ThenInclude(v => v.VehicleType)
                    .Include(w => w.Customer)
                    .Select(w => new RoadsideAssistanceResponseDto
                    {
                        Id = w.Id,
                        CustomerName = w.Customer.FullName,
                        Vehicle = new VehicleResponseDto
                        {
                            Id = w.Vehicle.Id,
                            Brand = w.Vehicle.BrandId == -1 ? w.Vehicle.OtherBrand : w.Vehicle.Brand.Name,
                            Model = w.Vehicle.VehicleModelId == -1 ? w.Vehicle.OtherVehicleModel : w.Vehicle.VehicleModel.Model,
                            Version = w.Vehicle.VehicleVersionId == -1 ? w.Vehicle.OtherVehicleVersion : w.Vehicle.VehicleVersion.Version,
                            Type = w.Vehicle.VehicleTypeId == -1 ? w.Vehicle.OtherVehicleType : w.Vehicle.VehicleType.Type,
                            Year = w.Vehicle.Year,
                            SerialNumber = w.Vehicle.SerialNumber,
                            Color = w.Vehicle.Color,
                            Plates = w.Vehicle.Plates,
                            RimRubber = w.Vehicle.RimRubber,
                            Kms = w.Vehicle.Kms,
                            Image = w.Vehicle.Image,
                            VehicleFormat = w.Vehicle.VehicleFormat,
                        },
                        Latitude = w.Latitude,
                        Longitude = w.Longitude,
                        Description = w.Description,
                        CreatedAt = w.CreatedAt,
                    })
                    .ToListAsync();

                if (roadsideAssistances == null)
                {
                    return NotFound("not-found");
                }

                return Ok(roadsideAssistances);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPost]
        public async Task<ActionResult> CreateQuotation([FromBody] PostRoadsideAssistance model)
        {
            try
            {
                var customerExists = await _context.Customers.AnyAsync(c => c.Id == model.CustomerId);
                var workshopExists = await _context.Workshops.AnyAsync(w => w.Id == model.WorkshopId);
                var vehicleExists = await _context.Vehicles.AnyAsync(v => v.Id == model.VehicleId && v.CustomerId == model.CustomerId);

                if (!customerExists || !workshopExists || !vehicleExists)
                {
                    return NotFound("not-found");
                }

                var roadsideAssistance = new RoadsideAssistance
                {
                    CustomerId = model.CustomerId,
                    WorkshopId = model.WorkshopId,
                    VehicleId = model.VehicleId,
                    Description = model.Description,
                    Latitude = model.Latitude,
                    Longitude = model.Longitude,
                };

                _context.RoadsideAssistances.Add(roadsideAssistance);
                await _context.SaveChangesAsync();

                // WorkshopInbox
                var workshopInbox = new WorkshopInbox
                {
                    WorkshopId = model.WorkshopId,
                    CustomerId = model.CustomerId,
                    VehicleId = model.VehicleId,
                    ParentModelType = "RoadsideAssistance",
                    ParentModelId = roadsideAssistance.Id,
                    Title = $"Consulta de asistencia {DateTime.Now}",
                    Details = roadsideAssistance.Description,
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
                //    new Dictionary<string, string> { { "roadsideAssistanceId", roadsideAssistance.Id.ToString() } }
                //);

                return Ok("roadside-assistance-created");
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpGet("search-workshop-by-name")]
        public async Task<ActionResult<IEnumerable<WorkshopInfoResponseDto>>> SearchWorkshopsByName([FromQuery] string workshopName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(workshopName))
                {
                    return BadRequest("invalid-workshop-name");
                }

                // Obtener los talleres que coincidan por nombre
                var workshops = await _context.Workshops
                    .Where(w => !w.IsDeleted && w.WorkshopName.Contains(workshopName))
                    .ToListAsync();

                if (!workshops.Any())
                {
                    return NotFound("not-found");
                }

                var workshopIds = workshops.Select(w => w.Id).ToList();

                // Obtener solo los talleres que tienen el servicio de asistencia
                var roadsideAssistanceWorkshopIds = await _context.WorkshopServices
                    .Include(ws => ws.Service)
                    .Where(ws => workshopIds.Contains(ws.WorkshopId) &&
                                 !ws.IsDeleted &&
                                 ws.Service.Name == "Asistencia en carretera y grúa")
                    .Select(ws => ws.WorkshopId)
                    .Distinct()
                    .ToListAsync();

                // Filtrar los talleres originales para quedarnos solo con los que tienen el servicio
                var filteredWorkshops = workshops
                    .Where(w => roadsideAssistanceWorkshopIds.Contains(w.Id))
                    .ToList();

                if (!filteredWorkshops.Any())
                {
                    return NotFound("no-matching-workshops-with-service");
                }

                var filteredWorkshopIds = filteredWorkshops.Select(w => w.Id).ToList();

                // Obtener reviews
                var reviews = await _context.Reviews
                    .Where(r => filteredWorkshopIds.Contains(r.WorkshopId))
                    .Include(r => r.Customer)
                    .ToListAsync();

                var addresses = await _context.WorkshopAddresses
                    .Where(wa => filteredWorkshopIds.Contains(wa.WorkshopId) && !wa.IsDeleted)
                    .Include(wa => wa.Suburb)
                        .ThenInclude(sub => sub.Town)
                            .ThenInclude(tow => tow.State)
                    .ToListAsync();

                var reviewsByWorkshop = reviews
                    .GroupBy(r => r.WorkshopId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                var addressesByWorkshop = addresses
                    .GroupBy(a => a.WorkshopId)
                    .ToDictionary(g => g.Key, g => g.FirstOrDefault());

                var workshopsList = new List<WorkshopInfoResponseDto>();

                foreach (var workshop in filteredWorkshops)
                {
                    float avgRate = 0;

                    if (reviewsByWorkshop.TryGetValue(workshop.Id, out var workshopReviews))
                    {
                        avgRate = workshopReviews.Any()
                            ? (float)Math.Round(workshopReviews.Average(r => r.Rate), 1)
                            : 0;
                    }

                    AccountAddressDto accountAddressDto = null;
                    string oneLineAddress = "";
                    if (addressesByWorkshop.TryGetValue(workshop.Id, out var address) && address != null)
                    {
                        accountAddressDto = new AccountAddressDto
                        {
                            AccountId = address.WorkshopId,
                            StateName = address.Suburb?.Town?.State?.Name,
                            TownName = address.Suburb?.Town?.Name,
                            SuburbName = address.Suburb?.Name,
                            SuburbId = address.Suburb?.Id ?? 0,
                            Zipcode = address.Suburb?.Zipcode,
                            Street = address.Street
                        };
                        oneLineAddress = $"Calle {accountAddressDto.Street}, {accountAddressDto.SuburbName}, {accountAddressDto.TownName}, {accountAddressDto.StateName}";
                    }

                    var workshopDto = new WorkshopInfoResponseDto
                    {
                        WorkshopId = workshop.Id,
                        AssociateName = workshop.AssociateFullName,
                        PhoneNumber = workshop.NormalizedPhoneNumber,
                        Landline = workshop.Landline,
                        Email = workshop.Email,
                        WorkshopName = workshop.WorkshopName,
                        Type = workshop.Type,
                        Latitude = workshop.Latitude?.ToString() ?? string.Empty,
                        Longitude = workshop.Longitude?.ToString() ?? string.Empty,
                        Address = accountAddressDto,
                        OneLineAddress = oneLineAddress,
                        ProfileImage = workshop.ProfileImage,
                        ReviewAverageRate = avgRate
                    };

                    workshopsList.Add(workshopDto);
                }

                // Ordenar por calificación descendente
                var orderedList = workshopsList
                    .OrderByDescending(w => w.ReviewAverageRate)
                    .ToList();

                return Ok(orderedList);
            }
            catch (Exception)
            {
                return BadRequest("unknown-error");
            }
        }

        [HttpPost("search-workshop-by-name-pager")]
        [AllowAnonymous]
        public async Task<ActionResult<PagerResponseDto<WorkshopInfoResponseDto>>> SearchWorkshopsByNamePaged([FromQuery] string workshopName, [FromBody] PagerBodyDto pager)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(workshopName))
                    return BadRequest("invalid-workshop-name");

                var workshops = await _context.Workshops
                    .Where(w => !w.IsDeleted && w.WorkshopName.Contains(workshopName))
                    .ToListAsync();

                if (!workshops.Any())
                    return NotFound("not-found");

                var workshopIds = workshops.Select(w => w.Id).ToList();

                var roadsideAssistanceWorkshopIds = await _context.WorkshopServices
                    .Include(ws => ws.Service)
                    .Where(ws => workshopIds.Contains(ws.WorkshopId) &&
                                 !ws.IsDeleted &&
                                 ws.Service.Name == "Asistencia en carretera y grúa")
                    .Select(ws => ws.WorkshopId)
                    .Distinct()
                    .ToListAsync();

                var filteredWorkshops = workshops
                    .Where(w => roadsideAssistanceWorkshopIds.Contains(w.Id))
                    .ToList();

                if (!filteredWorkshops.Any())
                    return NotFound("no-matching-workshops-with-service");

                var filteredWorkshopIds = filteredWorkshops.Select(w => w.Id).ToList();

                var reviews = await _context.Reviews
                    .Where(r => filteredWorkshopIds.Contains(r.WorkshopId))
                    .Include(r => r.Customer)
                    .ToListAsync();

                var addresses = await _context.WorkshopAddresses
                    .Where(wa => filteredWorkshopIds.Contains(wa.WorkshopId) && !wa.IsDeleted)
                    .Include(wa => wa.Suburb)
                        .ThenInclude(sub => sub.Town)
                            .ThenInclude(tow => tow.State)
                    .ToListAsync();

                var reviewsByWorkshop = reviews
                    .GroupBy(r => r.WorkshopId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                var addressesByWorkshop = addresses
                    .GroupBy(a => a.WorkshopId)
                    .ToDictionary(g => g.Key, g => g.FirstOrDefault());

                var workshopsList = new List<WorkshopInfoResponseDto>();

                foreach (var workshop in filteredWorkshops)
                {
                    float avgRate = 0;

                    if (reviewsByWorkshop.TryGetValue(workshop.Id, out var workshopReviews))
                    {
                        avgRate = workshopReviews.Any()
                            ? (float)Math.Round(workshopReviews.Average(r => r.Rate), 1)
                            : 0;
                    }

                    AccountAddressDto accountAddressDto = null;
                    string oneLineAddress = "";

                    if (addressesByWorkshop.TryGetValue(workshop.Id, out var address) && address != null)
                    {
                        accountAddressDto = new AccountAddressDto
                        {
                            AccountId = address.WorkshopId,
                            StateName = address.Suburb?.Town?.State?.Name,
                            TownName = address.Suburb?.Town?.Name,
                            SuburbName = address.Suburb?.Name,
                            SuburbId = address.Suburb?.Id ?? 0,
                            Zipcode = address.Suburb?.Zipcode,
                            Street = address.Street
                        };
                        oneLineAddress = $"Calle {accountAddressDto.Street}, {accountAddressDto.SuburbName}, {accountAddressDto.TownName}, {accountAddressDto.StateName}";
                    }

                    workshopsList.Add(new WorkshopInfoResponseDto
                    {
                        WorkshopId = workshop.Id,
                        AssociateName = workshop.AssociateFullName,
                        PhoneNumber = workshop.NormalizedPhoneNumber,
                        Landline = workshop.Landline,
                        Email = workshop.Email,
                        WorkshopName = workshop.WorkshopName,
                        Type = workshop.Type,
                        Latitude = workshop.Latitude?.ToString() ?? string.Empty,
                        Longitude = workshop.Longitude?.ToString() ?? string.Empty,
                        Address = accountAddressDto,
                        OneLineAddress = oneLineAddress,
                        ProfileImage = workshop.ProfileImage,
                        ReviewAverageRate = avgRate
                    });
                }

                // Ordenar y paginar
                var orderedList = workshopsList.OrderByDescending(w => w.ReviewAverageRate).ToList();
                var total = orderedList.Count;
                var totalPages = (int)Math.Ceiling((double)total / pager.PageSize);

                var pagedList = orderedList
                    .Skip((pager.PageNumber - 1) * pager.PageSize)
                    .Take(pager.PageSize)
                    .ToList();

                var response = new PagerResponseDto<WorkshopInfoResponseDto>
                {
                    CurrentPage = pager.PageNumber,
                    TotalPages = totalPages,
                    Elements = pagedList
                };

                return Ok(response);
            }
            catch (Exception)
            {
                return BadRequest("unknown-error");
            }
        }

        [HttpPost("search-workshop-by-name-location-service-pager")]
        [AllowAnonymous]
        public async Task<ActionResult<PagerResponseDto<WorkshopInfoResponseDto>>> SearchWorkshopsWithRoadsideServiceByNameAndLocationPaged([FromQuery] string workshopName, [FromQuery] LatitudeLongitude latlong, [FromBody] PagerBodyDto pager)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(workshopName))
                    return BadRequest("invalid-workshop-name");

                // Talleres con nombre coincidente y coordenadas válidas
                var workshops = await _context.Workshops
                    .Where(w => !w.IsDeleted &&
                                w.WorkshopName.Contains(workshopName) &&
                                w.Latitude.HasValue && w.Longitude.HasValue)
                    .ToListAsync();

                if (!workshops.Any())
                    return NotFound("not-found");

                var workshopIds = workshops.Select(w => w.Id).ToList();

                // Talleres con el servicio requerido
                var roadsideAssistanceWorkshopIds = await _context.WorkshopServices
                    .Include(ws => ws.Service)
                    .Where(ws => workshopIds.Contains(ws.WorkshopId) &&
                                 !ws.IsDeleted &&
                                 ws.Service.Name == "Asistencia en carretera y grúa")
                    .Select(ws => ws.WorkshopId)
                    .Distinct()
                    .ToListAsync();

                var filteredWorkshops = workshops
                    .Where(w => roadsideAssistanceWorkshopIds.Contains(w.Id))
                    .Select(w => new
                    {
                        Workshop = w,
                        Distance = GetDistanceInKm(latlong.Latitude, latlong.Longitude, w.Latitude!.Value, w.Longitude!.Value)
                    })
                    .OrderBy(w => w.Distance)
                    .ToList();

                if (!filteredWorkshops.Any())
                    return NotFound("no-matching-workshops-with-service");

                var filteredWorkshopIds = filteredWorkshops.Select(w => w.Workshop.Id).ToList();

                var reviews = await _context.Reviews
                    .Where(r => filteredWorkshopIds.Contains(r.WorkshopId))
                    .Include(r => r.Customer)
                    .ToListAsync();

                var addresses = await _context.WorkshopAddresses
                    .Where(wa => filteredWorkshopIds.Contains(wa.WorkshopId) && !wa.IsDeleted)
                    .Include(wa => wa.Suburb)
                        .ThenInclude(sub => sub.Town)
                            .ThenInclude(tow => tow.State)
                    .ToListAsync();

                var reviewsByWorkshop = reviews
                    .GroupBy(r => r.WorkshopId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                var addressesByWorkshop = addresses
                    .GroupBy(a => a.WorkshopId)
                    .ToDictionary(g => g.Key, g => g.FirstOrDefault());

                var workshopsList = new List<WorkshopInfoResponseDto>();

                foreach (var entry in filteredWorkshops)
                {
                    var workshop = entry.Workshop;
                    float avgRate = 0;

                    if (reviewsByWorkshop.TryGetValue(workshop.Id, out var workshopReviews))
                    {
                        avgRate = workshopReviews.Any()
                            ? (float)Math.Round(workshopReviews.Average(r => r.Rate), 1)
                            : 0;
                    }

                    AccountAddressDto accountAddressDto = null;
                    string oneLineAddress = "";
                    if (addressesByWorkshop.TryGetValue(workshop.Id, out var address) && address != null)
                    {
                        accountAddressDto = new AccountAddressDto
                        {
                            AccountId = address.WorkshopId,
                            StateName = address.Suburb?.Town?.State?.Name,
                            TownName = address.Suburb?.Town?.Name,
                            SuburbName = address.Suburb?.Name,
                            SuburbId = address.Suburb?.Id ?? 0,
                            Zipcode = address.Suburb?.Zipcode,
                            Street = address.Street
                        };
                        oneLineAddress = $"Calle {accountAddressDto.Street}, {accountAddressDto.SuburbName}, {accountAddressDto.TownName}, {accountAddressDto.StateName}";
                    }

                    workshopsList.Add(new WorkshopInfoResponseDto
                    {
                        WorkshopId = workshop.Id,
                        AssociateName = workshop.AssociateFullName,
                        PhoneNumber = workshop.NormalizedPhoneNumber,
                        Landline = workshop.Landline,
                        Email = workshop.Email,
                        WorkshopName = workshop.WorkshopName,
                        Type = workshop.Type,
                        Latitude = workshop.Latitude?.ToString() ?? string.Empty,
                        Longitude = workshop.Longitude?.ToString() ?? string.Empty,
                        Address = accountAddressDto,
                        OneLineAddress = oneLineAddress,
                        ProfileImage = workshop.ProfileImage,
                        ReviewAverageRate = avgRate
                    });
                }

                // Ordenar por cercanía y luego por calificación
                var orderedList = workshopsList
                    .OrderBy(w => filteredWorkshops.First(x => x.Workshop.Id == w.WorkshopId).Distance)
                    .ThenByDescending(w => w.ReviewAverageRate)
                    .ToList();

                // Paginación
                var total = orderedList.Count;
                var totalPages = (int)Math.Ceiling((double)total / pager.PageSize);
                var pagedList = orderedList
                    .Skip((pager.PageNumber - 1) * pager.PageSize)
                    .Take(pager.PageSize)
                    .ToList();

                var response = new PagerResponseDto<WorkshopInfoResponseDto>
                {
                    CurrentPage = pager.PageNumber,
                    TotalPages = totalPages,
                    Elements = pagedList
                };

                return Ok(response);
            }
            catch (Exception)
            {
                return BadRequest("unknown-error");
            }
        }

        [HttpPost("search-workshops-by-location")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<WorkshopInfoResponseDto>>> SearchWorkshopsByLocation([FromBody] LatitudeLongitude ubicacion)
        {
            try
            {
                var workshops = await _context.Workshops
                    .Where(w => !w.IsDeleted)
                    .ToListAsync();

                if (!workshops.Any())
                {
                    return NotFound("not-found");
                }

                var workshopIds = workshops.Select(w => w.Id).ToList();

                var roadsideAssistanceWorkshopIds = await _context.WorkshopServices
                    .Include(ws => ws.Service)
                    .Where(ws => workshopIds.Contains(ws.WorkshopId) &&
                                 !ws.IsDeleted &&
                                 ws.Service.Name == "Asistencia en carretera y grúa")
                    .Select(ws => ws.WorkshopId)
                    .Distinct()
                    .ToListAsync();

                var filteredWorkshops = workshops
                    .Where(w => roadsideAssistanceWorkshopIds.Contains(w.Id))
                    .ToList();

                if (!filteredWorkshops.Any())
                {
                    return NotFound("no-matching-workshops-with-service");
                }

                var filteredWorkshopIds = filteredWorkshops.Select(w => w.Id).ToList();

                var reviews = await _context.Reviews
                    .Where(r => filteredWorkshopIds.Contains(r.WorkshopId))
                    .Include(r => r.Customer)
                    .ToListAsync();

                var addresses = await _context.WorkshopAddresses
                    .Where(wa => filteredWorkshopIds.Contains(wa.WorkshopId) && !wa.IsDeleted)
                    .Include(wa => wa.Suburb)
                        .ThenInclude(sub => sub.Town)
                            .ThenInclude(tow => tow.State)
                    .ToListAsync();

                var reviewsByWorkshop = reviews
                    .GroupBy(r => r.WorkshopId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                var addressesByWorkshop = addresses
                    .GroupBy(a => a.WorkshopId)
                    .ToDictionary(g => g.Key, g => g.FirstOrDefault());

                var workshopsList = new List<(WorkshopInfoResponseDto Dto, double DistanciaKm)>();

                foreach (var workshop in filteredWorkshops)
                {
                    if (!workshop.Latitude.HasValue || !workshop.Longitude.HasValue)
                        continue;

                    float avgRate = 0;
                    if (reviewsByWorkshop.TryGetValue(workshop.Id, out var workshopReviews))
                    {
                        avgRate = workshopReviews.Any()
                            ? (float)Math.Round(workshopReviews.Average(r => r.Rate), 1)
                            : 0;
                    }

                    AccountAddressDto? accountAddressDto = null;
                    string oneLineAddress = "";
                    if (addressesByWorkshop.TryGetValue(workshop.Id, out var address) && address != null)
                    {
                        accountAddressDto = new AccountAddressDto
                        {
                            AccountId = address.WorkshopId,
                            StateName = address.Suburb?.Town?.State?.Name,
                            TownName = address.Suburb?.Town?.Name,
                            SuburbName = address.Suburb?.Name,
                            SuburbId = address.Suburb?.Id ?? 0,
                            Zipcode = address.Suburb?.Zipcode,
                            Street = address.Street
                        };
                        oneLineAddress = $"Calle {accountAddressDto.Street}, {accountAddressDto.SuburbName}, {accountAddressDto.TownName}, {accountAddressDto.StateName}";
                    }

                    double distancia = GetDistanceInKm(
                        ubicacion.Latitude,
                        ubicacion.Longitude,
                        workshop.Latitude.Value,
                        workshop.Longitude.Value
                    );

                    var dto = new WorkshopInfoResponseDto
                    {
                        WorkshopId = workshop.Id,
                        AssociateName = workshop.AssociateFullName,
                        PhoneNumber = workshop.NormalizedPhoneNumber,
                        Landline = workshop.Landline,
                        Email = workshop.Email,
                        WorkshopName = workshop.WorkshopName,
                        Type = workshop.Type,
                        Latitude = workshop.Latitude?.ToString() ?? string.Empty,
                        Longitude = workshop.Longitude?.ToString() ?? string.Empty,
                        Address = accountAddressDto,
                        OneLineAddress = oneLineAddress,
                        ProfileImage = workshop.ProfileImage,
                        ReviewAverageRate = avgRate
                    };

                    workshopsList.Add((dto, distancia));
                }

                var orderedList = workshopsList
                    .OrderBy(w => w.DistanciaKm)
                    .Select(w => w.Dto)
                    .Take(6)
                    .ToList();

                return Ok(orderedList);
            }
            catch (Exception)
            {
                return BadRequest("unknown-error");
            }
        }

        

        private double GetDistanceInKm(double lat1, double lon1, double lat2, double lon2)
        {
            var R = 6371; // Radio de la Tierra en km
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private double ToRadians(double deg) => deg * (Math.PI / 180);

    }
}
