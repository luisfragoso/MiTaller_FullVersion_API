using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiTaller.Attributes;
using MiTaller.Data;
using MiTaller.DTO.Services;
using MiTaller.DTO.Workshop.Services;
using MiTaller.Models.Auth;
using MiTaller.Models.Workshop;

namespace MiTaller.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WorkshopServiceController : ControllerBase
    {
        private readonly UserManager<BaseIdentityUser> _userManager;
        private readonly DataContext _context;

        public WorkshopServiceController(UserManager<BaseIdentityUser> userManager, DataContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [HttpGet("service-categories/{type}")]
        public async Task<ActionResult<ICollection<ServiceCategoryResponseDto>>> GetServiceCategories(string type)
        {
            try
            {
                var serviceCategories = await _context.ServiceCategories
                    .Where(c  => c.Type == type)
                    .ToListAsync();

                if (serviceCategories == null)
                {
                    return NotFound("not-found");
                }

                var serviceCategoriesDto = new List<ServiceCategoryResponseDto>();
                foreach (var serviceCategory in serviceCategories)
                {
                    var serviceCategoryDto = new ServiceCategoryResponseDto
                    {
                        Id = serviceCategory.Id,
                        Name = serviceCategory.Name,
                    };
                    serviceCategoriesDto.Add(serviceCategoryDto);
                }

                return Ok(serviceCategoriesDto);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpGet("services-by/{categoryId}")]
        public async Task<ActionResult<ICollection<ServiceCategoryResponseDto>>> GetServicesByCategoryId(int categoryId)
        {
            try
            {
                var serviceCategory = await _context.ServiceCategories
                    .Where(c => c.Id == categoryId)
                    .FirstOrDefaultAsync();

                if (serviceCategory == null)
                {
                    return NotFound("not-found");
                }

                var services = await _context.Services
                    .Where(c => c.ServiceCategoryId == categoryId)
                    .ToListAsync();

                if (services == null)
                {
                    return NotFound("not-found");
                }

                var servicesDto = new List<ServiceResponseDto>();
                foreach (var service in services)
                {
                    var serviceCategoryDto = new ServiceResponseDto
                    {
                        Id = service.Id,
                        Name = service.Name,
                    };
                    servicesDto.Add(serviceCategoryDto);
                }

                return Ok(servicesDto);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPost("register-service")]
        [AuthorizeEmployee("Administrador")]
        public async Task<ActionResult> RegisterWorkshopService(PostWorkshopServiceDto model)
        {
            try
            {
                var workshop = await _context.Workshops
                    .Where(w => w.Id == model.WorkshopId && !w.IsDeleted)
                    .FirstOrDefaultAsync();

                if (workshop == null)
                {
                    return NotFound("not-found");
                }

                var service = await _context.Services
                    .Where(s => s.Id == model.ServiceId)
                    .FirstOrDefaultAsync();

                if (service == null)
                {
                    return NotFound("not-found");
                }

                var newService = new WorkshopServices
                {
                    WorkshopId = model.WorkshopId,
                    ServiceId = model.ServiceId,
                    Price = model.Price,
                    IsDeleted = false
                };

                await _context.WorkshopServices.AddAsync(newService);

                await _context.SaveChangesAsync();

                return Ok("service-created");
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpGet("by/{workshopId}")]
        [AllowAnonymous]
        public async Task<ActionResult<ICollection<WorkshopServiceResponseDto>>> GetServicesByWorkshop(Guid workshopId)
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

                var workshopServices = await _context.WorkshopServices
                    .Where(ws => ws.WorkshopId == workshopId && !ws.IsDeleted)
                    .Include(w => w.Service)
                    .ToListAsync();

                if (workshopServices == null)
                {
                    return NotFound("not-found");
                }

                var workshopServicesDto = new List<WorkshopServiceResponseDto>();

                foreach (var model in workshopServices)
                {
                    var newService = new WorkshopServiceResponseDto
                    {
                        WorkshopId = model.WorkshopId,
                        ServiceId = model.Id,
                        ServiceName = model.Service.Name,
                        Price = model.Price,

                    };
                    workshopServicesDto.Add(newService);
                }


                return Ok(workshopServicesDto);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpGet("service-by/{workshopServiceId}")]
        public async Task<ActionResult<ICollection<WorkshopServiceResponseDto>>> GetServiceById(int workshopServiceId)
        {
            try
            {
                var workshopService = await _context.WorkshopServices
                    .Where(ws => ws.Id == workshopServiceId && !ws.IsDeleted)
                    .Include(w => w.Service)
                    .FirstOrDefaultAsync();

                if (workshopService == null)
                {
                    return NotFound("not-found");
                }

                var serviceDto = new WorkshopServiceResponseDto
                {
                    WorkshopId = workshopService.WorkshopId,
                    ServiceName = workshopService.Service.Name,
                    Price = workshopService.Price,
                };

                return Ok(workshopService);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPut("service")]
        [AuthorizeEmployee("Administrador")]
        public async Task<ActionResult<ICollection<PutWorkshopServiceDto>>> UpdateServiceById(PutWorkshopServiceDto model)
        {
            try
            {
                var workshopService = await _context.WorkshopServices
                    .Where(ws => ws.Id == model.WorkshopServiceId && !ws.IsDeleted)
                    .FirstOrDefaultAsync();

                if (workshopService == null)
                {
                    return NotFound("not-found");
                }

                workshopService.Price = model.Price;

                await _context.SaveChangesAsync();

                return Ok("service-updated");
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpDelete("service/{serviceId}")]
        [AuthorizeEmployee("Administrador")]
        public async Task<ActionResult<ICollection<PostWorkshopServiceDto>>> DeleteServiceById(int serviceId)
        {
            try
            {
                var workshopService = await _context.WorkshopServices
                    .Where(ws => ws.Id == serviceId && !ws.IsDeleted)
                    .FirstOrDefaultAsync();

                if (workshopService == null)
                {
                    return NotFound("not-found");
                }

                workshopService.IsDeleted = true;

                await _context.SaveChangesAsync();

                return Ok("service-deleted");
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }
    }
}
