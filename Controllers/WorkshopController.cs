using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MiTaller.Data;
using MiTaller.Models.Workshop;
using MiTaller.DTO.Workshop;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using MiTaller.Attributes;
using MiTaller.Models.Auth;
using MiTaller.DTO.Review;
using MiTaller.DTO.Address;
using MiTaller.DTO.Workshop.Services;
using MiTaller.DTO.Tag;
using MiTaller.DTO;
using MiTaller.DTO.Pager;

namespace MiTaller.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class WorkshopController : ControllerBase
    {
        private readonly UserManager<BaseIdentityUser> _userManager;
        private readonly DataContext _context;

        public WorkshopController(UserManager<BaseIdentityUser> userManager, DataContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [HttpGet("{workshopId}")]
        public async Task<ActionResult<WorkshopInfoResponseDto>> GetWorkshopById(Guid workshopId)
        {
            try 
            {
                var workshop = await _userManager.Users
                    .OfType<Workshop>()
                    .Where(w => w.Id == workshopId && !w.IsDeleted)
                    .FirstOrDefaultAsync();

                if (workshop == null)
                {
                    return NotFound("not-found");
                }

                var address = await _context.WorkshopAddresses
                    .Where(wa => wa.WorkshopId == workshopId && !wa.IsDeleted)
                    .Include(wa => wa.Suburb)
                        .ThenInclude(sub => sub.Town)
                            .ThenInclude(tow => tow.State)
                    .FirstOrDefaultAsync();

                float averageRate = 0;
                var reviews = await _context.Reviews
                    .Where(r => r.WorkshopId == workshopId)
                    .Include(r => r.Customer)
                    .Select(r => r.Rate)
                    .ToListAsync();

                if (reviews.Any())
                {
                    averageRate = reviews.Average();
                }

                AccountAddressDto accountAddressDto = null;
                var oneLineAddress = "";
                if (address != null)
                {
                    accountAddressDto = new AccountAddressDto
                    {
                        AccountId = address.WorkshopId,
                        StateName = address.Suburb?.Town?.State?.Name,
                        TownName = address.Suburb?.Town?.Name,
                        SuburbName = address.Suburb?.Name,
                        SuburbId = address.Suburb != null ? address.Suburb.Id : 0,
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
                    ProfileImage = workshop.ProfileImage,
                    Latitude = workshop.Latitude?.ToString() ?? string.Empty,
                    Longitude = workshop.Longitude?.ToString() ?? string.Empty,
                    Address = accountAddressDto,
                    OneLineAddress = oneLineAddress,
                    ReviewAverageRate = averageRate,
                };

                return Ok(workshopDto);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPost("upload-profile-image/{workshopId}")]
        [Authorize]
        [AuthorizeEmployee("Administrador")]
        public async Task<ActionResult> UploadImage(Guid workshopId, IFormFile file)
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

                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    workshop.ProfileImage = memoryStream.ToArray();
                }

                await _context.SaveChangesAsync();

                return Ok("image-saved");
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPut("{workshopId}")]
        [Authorize]
        [AuthorizeEmployee("Administrador")]
        public async Task<ActionResult> UpdateWorkshop(Guid workshopId, [FromBody] PostWorkshopDto model)
        {
            try 
            {
                var workshop = await _userManager.Users
                    .OfType<Workshop>()
                    .Where(w => w.Id == workshopId && !w.IsDeleted)
                    .FirstOrDefaultAsync();

                if (workshop == null)
                {
                    return NotFound("not-found");
                }

                workshop.WorkshopName = model.WorkshopName;
                workshop.Email = model.Email;
                workshop.PhoneNumber = model.PhoneNumber + "_workshop";
                workshop.NormalizedPhoneNumber = model.PhoneNumber;
                workshop.Landline = model.Landline;
                workshop.Type = model.Type;
                workshop.AssociateFullName = model.AssociateFullName;
                workshop.UpdatedAt = DateTime.Now;

                var result = await _userManager.UpdateAsync(workshop);

                if (result.Succeeded)
                {
                    return Ok("workshop-updated");
                }

                return BadRequest(result.Errors);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPut("coordenates/{workshopId}")]
        [Authorize]
        [AuthorizeEmployee("Administrador")]
        public async Task<ActionResult> UpdateWorkshopLatLong(Guid workshopId, LatitudeLongitude model)
        {
            try
            {
                var workshop = await _userManager.Users
                    .OfType<Workshop>()
                    .Where(w => w.Id == workshopId && !w.IsDeleted)
                    .FirstOrDefaultAsync();

                if (workshop == null)
                {
                    return NotFound("not-found");
                }

                if (model.Latitude == null || model.Longitude == null)
                {
                    return BadRequest("invalid-empty");
                }

                workshop.Latitude = model.Latitude;
                workshop.Longitude = model.Longitude;

                var result = await _userManager.UpdateAsync(workshop);

                if (result.Succeeded)
                {
                    return Ok("workshop-updated");
                }

                return BadRequest(result.Errors);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpDelete("{workshopId}")]
        [Authorize]
        public async Task<ActionResult> SoftDeleteWorkshop(Guid workshopId)
        {
            try 
            {
                var workshop = await _userManager.Users
                    .OfType<Workshop>()
                    .Where(w => w.Id == workshopId && !w.IsDeleted)
                    .FirstOrDefaultAsync();

                if (workshop == null)
                {
                    return NotFound("not-found");
                }

                workshop.IsDeleted = true;
                workshop.DeletedAt = DateTime.Now;

                var result = await _userManager.UpdateAsync(workshop);

                if (result.Succeeded)
                {
                    return Ok("workshop-deleted");
                }

                return BadRequest(result.Errors);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpGet("recommended-workshops")]
        public async Task<ActionResult<List<WorkshopInfoResponseDto>>> GetRecommendedWorkshops()
        {
            try
            {
                var topRatedWorkshopIds = await _context.Reviews
                    .GroupBy(r => r.WorkshopId)
                    .Select(g => new
                    {
                        WorkshopId = g.Key,
                        AverageRate = g.Average(r => r.Rate),
                        TotalReviews = g.Count()
                    })
                    .OrderByDescending(g => g.AverageRate)
                    .ThenByDescending(g => g.TotalReviews)
                    .Take(5)
                    .Select(g => g.WorkshopId)
                    .ToListAsync();

                var workshops = await _context.Workshops
                    .Where(w => topRatedWorkshopIds.Contains(w.Id) && !w.IsDeleted)
                    .ToListAsync();

                if (workshops == null || !workshops.Any())
                {
                    return NotFound("not-found");
                }

                var workshopIds = workshops.Select(w => w.Id).ToList();

                var reviews = await _context.Reviews
                    .Where(r => workshopIds.Contains(r.WorkshopId))
                    .Include(r => r.Customer)
                    .ToListAsync();

                var addresses = await _context.WorkshopAddresses
                    .Where(wa => workshopIds.Contains(wa.WorkshopId) && !wa.IsDeleted)
                    .Include(wa => wa.Suburb)
                        .ThenInclude(sub => sub.Town)
                            .ThenInclude(tow => tow.State)
                    .ToListAsync();

                // Agrupar las reviews por WorkshopId
                var reviewsByWorkshop = reviews
                    .GroupBy(r => r.WorkshopId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                var addressesByWorkshop = addresses
                    .GroupBy(a => a.WorkshopId)
                    .ToDictionary(g => g.Key, g => g.FirstOrDefault());

                var workshopsList = new List<WorkshopInfoResponseDto>();

                foreach (var workshop in workshops)
                {
                    float avgRate = 0;

                    if (reviewsByWorkshop.TryGetValue(workshop.Id, out var workshopReviews))
                    {
                        avgRate = workshopReviews.Any()
                            ? (float)Math.Round(workshopReviews.Average(r => r.Rate), 1)
                            : 0;
                    }

                    // Address
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
                            SuburbId = address.Suburb != null ? address.Suburb.Id : 0,
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

                return Ok(workshopsList);
            }
            catch (Exception)
            {
                return BadRequest("unknown-error");
            }
        }

        [HttpGet("recommended-workshops-with-coordenates")]
        public async Task<ActionResult<List<WorkshopInfoResponseDto>>> GetRecommendedWorkshops([FromQuery] LatitudeLongitude latlong)
        {
            try
            {
                var validWorkshops = await _context.Workshops
                    .Where(w => w.Latitude.HasValue && w.Longitude.HasValue)
                    .ToListAsync();

                // Calcular distancia desde el punto recibido
                var workshopsWithDistance = validWorkshops
                    .Where(w => w.Latitude.HasValue && w.Longitude.HasValue)
                    .Select(w => new
                    {
                        Workshop = w,
                        Distance = GetDistanceInKm(latlong.Latitude, latlong.Longitude, w.Latitude.Value, w.Longitude.Value)
                    })
                    .OrderBy(w => w.Distance)
                    .Take(5)
                    .ToList();

                var nearestWorkshopIds = workshopsWithDistance.Select(w => w.Workshop.Id).ToList();

                var reviews = await _context.Reviews
                    .Where(r => nearestWorkshopIds.Contains(r.WorkshopId))
                    .Include(r => r.Customer)
                    .ToListAsync();

                var addresses = await _context.WorkshopAddresses
                    .Where(wa => nearestWorkshopIds.Contains(wa.WorkshopId) && !wa.IsDeleted)
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

                foreach (var entry in workshopsWithDistance)
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

                return Ok(workshopsList);
            }
            catch (Exception)
            {
                return BadRequest("unknown-error");
            }
        }

        [HttpGet("search-by-name")]
        public async Task<ActionResult<IEnumerable<WorkshopInfoResponseDto>>> SearchWorkshopsByName([FromQuery] string workshopName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(workshopName))
                {
                    return BadRequest("invalid-workshop-name");
                }

                var workshops = await _context.Workshops
                    .Where(w => !w.IsDeleted && w.WorkshopName.Contains(workshopName))
                    .ToListAsync();

                if (workshops == null || !workshops.Any())
                {
                    return NotFound("not-found");
                }

                var workshopIds = workshops.Select(w => w.Id).ToList();

                var reviews = await _context.Reviews
                    .Where(r => workshopIds.Contains(r.WorkshopId))
                    .Include(r => r.Customer)
                    .ToListAsync();

                var addresses = await _context.WorkshopAddresses
                    .Where(wa => workshopIds.Contains(wa.WorkshopId) && !wa.IsDeleted)
                    .Include(wa => wa.Suburb)
                        .ThenInclude(sub => sub.Town)
                            .ThenInclude(tow => tow.State)
                    .ToListAsync();

                // Agrupar las reviews por WorkshopId
                var reviewsByWorkshop = reviews
                    .GroupBy(r => r.WorkshopId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                var addressesByWorkshop = addresses
                    .GroupBy(a => a.WorkshopId)
                    .ToDictionary(g => g.Key, g => g.FirstOrDefault());

                var workshopsList = new List<WorkshopInfoResponseDto>();

                foreach (var workshop in workshops)
                {
                    float avgRate = 0;

                    if (reviewsByWorkshop.TryGetValue(workshop.Id, out var workshopReviews))
                    {
                        avgRate = workshopReviews.Any()
                            ? (float)Math.Round(workshopReviews.Average(r => r.Rate), 1)
                            : 0;
                    }

                    // Address
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
                            SuburbId = address.Suburb != null ? address.Suburb.Id : 0,
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

                return Ok(workshopsList);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPost("search-by-name-pager")]
        public async Task<ActionResult<PagerResponseDto<WorkshopInfoResponseDto>>> SearchWorkshopsByNamePaged([FromQuery] string workshopName, [FromBody] PagerBodyDto pager)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(workshopName))
                {
                    return BadRequest("invalid-workshop-name");
                }

                var workshops = await _context.Workshops
                    .Where(w => !w.IsDeleted && w.WorkshopName.Contains(workshopName))
                    .ToListAsync();

                if (workshops == null || !workshops.Any())
                {
                    return NotFound("not-found");
                }

                var workshopIds = workshops.Select(w => w.Id).ToList();

                var reviews = await _context.Reviews
                    .Where(r => workshopIds.Contains(r.WorkshopId))
                    .Include(r => r.Customer)
                    .ToListAsync();

                var addresses = await _context.WorkshopAddresses
                    .Where(wa => workshopIds.Contains(wa.WorkshopId) && !wa.IsDeleted)
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

                foreach (var workshop in workshops)
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

                // Ordenar por calificación y paginar
                var orderedList = workshopsList
                    .OrderByDescending(w => w.ReviewAverageRate)
                    .ToList();

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
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPost("search-by-name-location-pager")]
        public async Task<ActionResult<PagerResponseDto<WorkshopInfoResponseDto>>> SearchWorkshopsByNameAndLocationPaged([FromQuery] string workshopName, [FromQuery] LatitudeLongitude latlong, [FromBody] PagerBodyDto pager)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(workshopName))
                {
                    return BadRequest("invalid-workshop-name");
                }

                // Filtrar talleres por nombre (CON y SIN coordenadas)
                var allWorkshops = await _context.Workshops
                    .Where(w => !w.IsDeleted && w.WorkshopName.Contains(workshopName))
                    .ToListAsync();

                if (!allWorkshops.Any())
                {
                    return NotFound("not-found");
                }

                // Separar talleres con y sin coordenadas
                var workshopsWithCoords = allWorkshops
                    .Where(w => w.Latitude.HasValue && w.Longitude.HasValue)
                    .ToList();

                var workshopsWithoutCoords = allWorkshops
                    .Where(w => !w.Latitude.HasValue || !w.Longitude.HasValue)
                    .ToList();

                // Calcular distancias para los que tienen coordenadas
                var workshopsWithDistance = workshopsWithCoords
                    .Select(w => new
                    {
                        Workshop = w,
                        Distance = GetDistanceInKm(latlong.Latitude, latlong.Longitude, w.Latitude.Value, w.Longitude.Value),
                        HasCoordinates = true
                    })
                    .OrderBy(w => w.Distance)
                    .ToList();

                // Agregar los talleres sin coordenadas al final
                var workshopsWithoutDistance = workshopsWithoutCoords
                    .Select(w => new
                    {
                        Workshop = w,
                        Distance = double.MaxValue, // Distancia infinita para ordenar al final
                        HasCoordinates = false
                    })
                    .ToList();

                // Combinar ambas listas: primero los que tienen coordenadas (ordenados por distancia), luego los que no tienen
                var allWorkshopsOrdered = workshopsWithDistance
                    .Concat(workshopsWithoutDistance)
                    .ToList();

                var workshopIds = allWorkshopsOrdered.Select(w => w.Workshop.Id).ToList();

                // Cargar reseñas y direcciones
                var reviews = await _context.Reviews
                    .Where(r => workshopIds.Contains(r.WorkshopId))
                    .Include(r => r.Customer)
                    .ToListAsync();

                var addresses = await _context.WorkshopAddresses
                    .Where(wa => workshopIds.Contains(wa.WorkshopId) && !wa.IsDeleted)
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

                foreach (var entry in allWorkshopsOrdered)
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

                // Ordenar manteniendo la prioridad:
                // 1. Talleres con coordenadas ordenados por distancia y luego por calificación
                // 2. Talleres sin coordenadas ordenados por calificación
                var orderedList = workshopsList
                    .OrderBy(w => !allWorkshopsOrdered.First(x => x.Workshop.Id == w.WorkshopId).HasCoordinates) // false primero (con coords)
                    .ThenBy(w =>
                    {
                        var entry = allWorkshopsOrdered.First(x => x.Workshop.Id == w.WorkshopId);
                        return entry.HasCoordinates ? entry.Distance : 0;
                    })
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

        [HttpGet("search-workshop-complete/{workshopName}")]
        public async Task<ActionResult<List<WorkshopInfoCompleteResponseDto>>> SearchWorkshopsInfoCompleteByName(string workshopName)
        {
            try
            {
                var workshops = await _context.Workshops
                    .Where(w => !w.IsDeleted && w.WorkshopName.Contains(workshopName))
                    .ToListAsync();

                if (workshops == null || !workshops.Any())
                {
                    return NotFound("not-found");
                }

                var workshopIds = workshops.Select(w => w.Id).ToList();

                var reviews = await _context.Reviews
                    .Where(r => workshopIds.Contains(r.WorkshopId))
                    .Include(r => r.Customer)
                    .ToListAsync();

                var services = await _context.WorkshopServices
                    .Where(s => workshopIds.Contains(s.WorkshopId) && !s.IsDeleted)
                    .Include(w => w.Service)
                    .Take(6)
                    .ToListAsync();

                var addresses = await _context.WorkshopAddresses
                    .Where(wa => workshopIds.Contains(wa.WorkshopId) && !wa.IsDeleted)
                    .Include(wa => wa.Suburb)
                        .ThenInclude(sub => sub.Town)
                            .ThenInclude(tow => tow.State)
                    .ToListAsync();

                // Agrupar las reviews y los servicios por WorkshopId
                var reviewsByWorkshop = reviews
                    .GroupBy(r => r.WorkshopId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                var servicesByWorkshop = services
                    .GroupBy(s => s.WorkshopId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                var addressesByWorkshop = addresses
                    .GroupBy(a => a.WorkshopId)
                    .ToDictionary(g => g.Key, g => g.FirstOrDefault());

                var workshopsList = new List<WorkshopInfoCompleteResponseDto>();

                foreach (var workshop in workshops)
                {
                    // Procesar las reviews para el taller actual
                    ReviewResponseDto reviewResponse = new ReviewResponseDto();
                    if (reviewsByWorkshop.TryGetValue(workshop.Id, out var workshopReviews))
                    {
                        // Mapear cada review a WorkshopReviewDto con el formato requerido
                        var workshopReviewDtos = workshopReviews.Select(r => new WorkshopReviewDto
                        {
                            Id = r.Id,
                            CustomerId = r.CustomerId,
                            CustomerName = r.Customer.FullName,
                            Comment = r.Comment,
                            Rate = r.Rate,
                            Date = r.Date.ToString("yyyy-MM-dd")
                        }).ToList();

                        // Calcular el promedio redondeado a 1 decimal y el total de reviews
                        float avgRate = workshopReviewDtos.Any() ? (float)Math.Round(workshopReviewDtos.Average(r => r.Rate), 1) : 0;
                        int totalReviews = workshopReviewDtos.Count;
                        // Contar cuántas veces se ha asignado cada estrella (de 1 a 5)
                        var starCounts = Enumerable.Range(1, 5)
                            .ToDictionary(i => i, i => workshopReviewDtos.Count(r => r.Rate == i));

                        reviewResponse.AverageRate = avgRate;
                        reviewResponse.TotalReviews = totalReviews;
                        reviewResponse.StarCounts = starCounts;
                        reviewResponse.WorkshopReviews = workshopReviewDtos;
                    }
                    else
                    {
                        reviewResponse.AverageRate = 0;
                        reviewResponse.TotalReviews = 0;
                        reviewResponse.StarCounts = Enumerable.Range(1, 5)
                            .ToDictionary(i => i, i => 0);
                        reviewResponse.WorkshopReviews = new List<WorkshopReviewDto>();
                    }

                    // Obtener los servicios para el taller actual (si existen)
                    var workshopServices = servicesByWorkshop.ContainsKey(workshop.Id)
                        ? servicesByWorkshop[workshop.Id]
                        : new List<WorkshopServices>();

                    var workshopServicesDto = workshopServices.Select(s => new WorkshopServiceResponseDto
                    {
                        ServiceId = s.Id,
                        ServiceName = s.Service.Name,
                        Price = s.Price
                    }).ToList();

                    // Address
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
                            SuburbId = address.Suburb != null ? address.Suburb.Id : 0,
                            Zipcode = address.Suburb?.Zipcode,
                            Street = address.Street
                        };
                        oneLineAddress = $"Calle {accountAddressDto.Street}, {accountAddressDto.SuburbName}, {accountAddressDto.TownName}, {accountAddressDto.StateName}";
                    }

                    // Mapear el taller al DTO
                    var workshopDto = new WorkshopInfoCompleteResponseDto
                    {
                        WorkshopId = workshop.Id,
                        AssociateName = workshop.AssociateFullName,
                        PhoneNumber = workshop.NormalizedPhoneNumber,
                        Email = workshop.Email,
                        WorkshopName = workshop.WorkshopName,
                        Type = workshop.Type,
                        Latitude = workshop.Latitude?.ToString() ?? string.Empty,
                        Longitude = workshop.Longitude?.ToString() ?? string.Empty,
                        Reviews = reviewResponse,
                        Services = workshopServicesDto,
                        Address = accountAddressDto,
                        OneLineAddress = oneLineAddress,
                        ProfileImage = workshop.ProfileImage
                    };

                    workshopsList.Add(workshopDto);
                }

                return Ok(workshopsList);
            }
            catch (Exception)
            {
                return BadRequest("unknown-error");
            }
        }

        [HttpGet("search-workshop-simple/{workshopName}")]
        public async Task<ActionResult<List<WorkshopServiceSimpleResponseDto>>> SearchWorkshopsInfoSimpleByName(string workshopName)
        {
            try
            {
                var workshops = await _context.Workshops
                    .Where(w => !w.IsDeleted && w.WorkshopName.Contains(workshopName))
                    .ToListAsync();

                if (workshops == null || !workshops.Any())
                {
                    return NotFound("not-found");
                }

                var workshopIds = workshops.Select(w => w.Id).ToList();

                var services = await _context.WorkshopServices
                    .Where(s => workshopIds.Contains(s.WorkshopId) && !s.IsDeleted)
                    .Include(w => w.Service)
                    .ToListAsync();

                var servicesByWorkshop = services
                    .GroupBy(s => s.WorkshopId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                var workshopsList = new List<WorkshopServiceSimpleResponseDto>();

                foreach (var workshop in workshops)
                {

                    // Obtener los servicios para el taller actual (si existen)
                    var workshopServices = servicesByWorkshop.ContainsKey(workshop.Id)
                        ? servicesByWorkshop[workshop.Id]
                        : new List<WorkshopServices>();

                    var workshopServicesDto = workshopServices.Select(s => new ServiceSimpleResponseDto
                    {
                        ServiceId = s.Id,
                        ServiceName = s.Service.Name,
                        Price = s.Price
                    }).ToList();

                    // Mapear el taller al DTO
                    var workshopDto = new WorkshopServiceSimpleResponseDto
                    {
                        WorkshopId = workshop.Id,
                        WorkshopName = workshop.WorkshopName,
                        Services = workshopServicesDto,
                        Image = workshop.ProfileImage
                    };

                    workshopsList.Add(workshopDto);
                }

                return Ok(workshopsList);
            }
            catch (Exception)
            {
                return BadRequest("unknown-error");
            }
        }


        [HttpPost("search-workshop-simple-pager/{workshopName}")]
        public async Task<ActionResult<PagerResponseDto<WorkshopServiceSimpleResponseDto>>> SearchWorkshopsInfoSimpleByNamePaged(string workshopName, [FromBody] PagerBodyDto pager)
        {
            try
            {
                var baseQuery = _context.Workshops
                    .Where(w => !w.IsDeleted && w.WorkshopName.Contains(workshopName))
                    .OrderBy(w => w.WorkshopName); // Orden alfabético

                var totalCount = await baseQuery.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalCount / pager.PageSize);

                var pagedWorkshops = await baseQuery
                    .Skip((pager.PageNumber - 1) * pager.PageSize)
                    .Take(pager.PageSize)
                    .ToListAsync();

                if (!pagedWorkshops.Any())
                {
                    return NotFound("not-found");
                }

                var workshopIds = pagedWorkshops.Select(w => w.Id).ToList();

                var services = await _context.WorkshopServices
                    .Where(s => workshopIds.Contains(s.WorkshopId) && !s.IsDeleted)
                    .Include(w => w.Service)
                    .ToListAsync();

                var servicesByWorkshop = services
                    .GroupBy(s => s.WorkshopId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                var workshopsList = new List<WorkshopServiceSimpleResponseDto>();

                foreach (var workshop in pagedWorkshops)
                {
                    var workshopServices = servicesByWorkshop.TryGetValue(workshop.Id, out var ws)
                        ? ws
                        : new List<WorkshopServices>();

                    var workshopServicesDto = workshopServices.Select(s => new ServiceSimpleResponseDto
                    {
                        ServiceId = s.Id,
                        ServiceName = s.Service.Name,
                        Price = s.Price
                    }).ToList();

                    var workshopDto = new WorkshopServiceSimpleResponseDto
                    {
                        WorkshopId = workshop.Id,
                        WorkshopName = workshop.WorkshopName,
                        Services = workshopServicesDto,
                        Image = workshop.ProfileImage
                    };

                    workshopsList.Add(workshopDto);
                }

                var response = new PagerResponseDto<WorkshopServiceSimpleResponseDto>
                {
                    CurrentPage = pager.PageNumber,
                    TotalPages = totalPages,
                    Elements = workshopsList
                };

                return Ok(response);
            }
            catch (Exception)
            {
                return BadRequest("unknown-error");
            }
        }

        [HttpGet("workshop-clients/{workshopId}")]
        public async Task<ActionResult<IEnumerable<WorkshopCustomersDto>>> SearchWorkshopClients(Guid workshopId)
        {
            try
            {
                var customerAppointmentIds = await _context.Appointments
                    .Where(a => a.WorkshopId == workshopId)
                    .Select(a => a.CustomerId)
                    .ToListAsync();

                var customerQuotationIds = await _context.Quotations
                    .Where(q => q.WorkshopId == workshopId)
                    .Select(q => q.CustomerId)
                    .ToListAsync();

                var customerVehicleInspectionIds = await _context.WorkshopVehicleInspections
                    .Where(v => v.WorkshopId == workshopId)
                    .Select(v => v.CustomerId)
                    .ToListAsync();

                var customerMotocycleInspectionIds = await _context.WorkshopMotocycleInspections
                    .Where(m => m.WorkshopId == workshopId)
                    .Select(m => m.CustomerId)
                    .ToListAsync();

                var allCustomerIds = customerAppointmentIds
                    .Concat(customerQuotationIds)
                    .Concat(customerVehicleInspectionIds)
                    .Concat(customerMotocycleInspectionIds)
                    .Distinct()
                    .ToList();

                var customers = await _context.Customers
                    .Where(c => allCustomerIds.Contains(c.Id))
                    .ToListAsync();

                var tagsByCustomer = await _context.CustomerAssociatedTags
                    .Where(c => allCustomerIds.Contains(c.CustomerId) && c.WorkshopId == workshopId)
                    .Include(c => c.Tag)
                    .GroupBy(c => c.CustomerId)
                    .ToDictionaryAsync(g => g.Key, g => g
                        .Select(t => new CustomerTagsDto
                        {
                            CustomerTagId = t.Id,
                            Value = t.Tag.Value,
                            HexColor = t.Tag.HexColor
                        }).ToList());

                var workshopCustomers = customers.Select(c => new WorkshopCustomersDto
                {
                    CustomerId = c.Id,
                    Name = c.FullName,
                    Email = c.Email,
                    PhoneNumber = c.NormalizedPhoneNumber,
                    ProfileImage = c.ProfileImage,
                    Tags = tagsByCustomer.ContainsKey(c.Id) ? tagsByCustomer[c.Id] : new List<CustomerTagsDto>()
                }).ToList();

                return Ok(workshopCustomers);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPost("workshop-clients-pager/{workshopId}")]
        public async Task<ActionResult<PagerResponseDto<WorkshopCustomersDto>>> SearchWorkshopClientsPaged(Guid workshopId, [FromBody] PagerBodyDto pager)
        {
            try
            {
                var customerAppointmentIds = await _context.Appointments
                    .Where(a => a.WorkshopId == workshopId)
                    .Select(a => a.CustomerId)
                    .ToListAsync();

                var customerQuotationIds = await _context.Quotations
                    .Where(q => q.WorkshopId == workshopId)
                    .Select(q => q.CustomerId)
                    .ToListAsync();

                var customerVehicleInspectionIds = await _context.WorkshopVehicleInspections
                    .Where(v => v.WorkshopId == workshopId)
                    .Select(v => v.CustomerId)
                    .ToListAsync();

                var customerMotocycleInspectionIds = await _context.WorkshopMotocycleInspections
                    .Where(m => m.WorkshopId == workshopId)
                    .Select(m => m.CustomerId)
                    .ToListAsync();

                var allCustomerIds = customerAppointmentIds
                    .Concat(customerQuotationIds)
                    .Concat(customerVehicleInspectionIds)
                    .Concat(customerMotocycleInspectionIds)
                    .Distinct()
                    .ToList();

                var totalCount = allCustomerIds.Count;
                var totalPages = (int)Math.Ceiling((double)totalCount / pager.PageSize);

                var pagedCustomerIds = allCustomerIds
                    .Skip((pager.PageNumber - 1) * pager.PageSize)
                    .Take(pager.PageSize)
                    .ToList();

                var customers = await _context.Customers
                    .Where(c => pagedCustomerIds.Contains(c.Id) && c.IsDeleted != true)
                    .ToListAsync();

                var tagsByCustomer = await _context.CustomerAssociatedTags
                    .Where(c => pagedCustomerIds.Contains(c.CustomerId) && c.WorkshopId == workshopId)
                    .Include(c => c.Tag)
                    .GroupBy(c => c.CustomerId)
                    .ToDictionaryAsync(g => g.Key, g => g
                        .Select(t => new CustomerTagsDto
                        {
                            CustomerTagId = t.Id,
                            Value = t.Tag.Value,
                            HexColor = t.Tag.HexColor
                        }).ToList());

                var workshopCustomers = customers.Select(c => new WorkshopCustomersDto
                {
                    CustomerId = c.Id,
                    Name = c.FullName,
                    Email = c.Email,
                    PhoneNumber = c.NormalizedPhoneNumber,
                    ProfileImage = c.ProfileImage,
                    Tags = tagsByCustomer.ContainsKey(c.Id) ? tagsByCustomer[c.Id] : new List<CustomerTagsDto>()
                }).ToList();

                var response = new PagerResponseDto<WorkshopCustomersDto>
                {
                    CurrentPage = pager.PageNumber,
                    TotalPages = totalPages,
                    Elements = workshopCustomers
                };

                return Ok(response);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
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
