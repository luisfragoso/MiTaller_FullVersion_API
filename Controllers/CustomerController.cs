using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiTaller.Data;
using MiTaller.DTO.Customer;
using MiTaller.DTO.Vehicle;
using MiTaller.DTO.Workshop;
using MiTaller.Models.Auth;
using MiTaller.Models.Customer;
using MiTaller.Models.Notification;
using MiTaller.Models.Workshop;
using System.Security.Claims;

namespace MiTaller.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CustomerController : ControllerBase
    {
        private readonly UserManager<BaseIdentityUser> _userManager;
        private readonly DataContext _context;

        public CustomerController(UserManager<BaseIdentityUser> userManager, DataContext context)
        {
            _userManager = userManager;
            _context = context;
        }


        [HttpGet("{customerId}")]
        public async Task<ActionResult<CustomerInfoResponseDto>> GetCustomerById(Guid customerId)
        {
            try 
            { 
                var customer = await _userManager.Users
                    .OfType<Customer>()
                    .Where(c => c.Id == customerId && !c.IsDeleted)
                    .FirstOrDefaultAsync();

                var customerInfo = await _context.Customers
                    .Where(c => c.Id == customerId && !c.IsDeleted)
                    .FirstOrDefaultAsync();

                if (customerInfo == null)
                {
                    return NotFound("not-found");
                }

                if (customer == null)
                {
                    return NotFound("not-found");
                }

                var response = new CustomerInfoResponseDto
                {
                    CustomerId = customerId,
                    FullName = customerInfo.FullName,
                    PhoneNumber = customer.NormalizedPhoneNumber,
                    ProfilePicture = customer.ProfileImage,
                    Landline = customer.Landline,
                    Email = customer.Email
                };

                return Ok(response);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPost("upload-profile-image/{customerId}")]
        public async Task<ActionResult> UploadImage(Guid customerId, IFormFile file)
        {
            try 
            { 
                var customer = await _context.Customers
                    .Where(c => c.Id == customerId && !c.IsDeleted)
                    .FirstOrDefaultAsync();

                if (customer == null)
                {
                    return NotFound("not-found");
                }

                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    customer.ProfileImage = memoryStream.ToArray();
                }

                await _context.SaveChangesAsync();

                return Ok("image-saved");
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }


        [HttpPut("{customerId}")]
        public async Task<ActionResult> UpdateCustomer(Guid customerId, [FromBody] PutCustomerDto model)
        {
            try 
            { 
                var customer = await _userManager.Users
                    .OfType<Customer>()
                    .Where(c => c.Id == customerId && !c.IsDeleted)
                    .FirstOrDefaultAsync();

                if (customer == null)
                {
                    return NotFound("not-found");
                }

                var existingUser = await _userManager.Users
                    .Where(u => (u.Email == model.Email || u.NormalizedPhoneNumber == model.PhoneNumber) && u.Id != customerId && u.UserType == customer.UserType)
                    .FirstOrDefaultAsync();

                if (existingUser != null)
                {
                    return BadRequest("unknown-error");
                }

                customer.Landline = model.Landline;
                customer.NormalizedPhoneNumber = model.PhoneNumber;
                customer.PhoneNumber = model.PhoneNumber + "_customer";
                customer.Email = model.Email;
                customer.UpdatedAt = DateTime.Now;

                var result = await _userManager.UpdateAsync(customer);

                if (result.Succeeded)
                {
                    return Ok("customer-updated");
                }

                return BadRequest(result.Errors);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }


        [HttpDelete("{customerId}")]
        public async Task<ActionResult> SoftDeleteCustomer(Guid customerId)
        {
            try 
            { 
                var customer = await _userManager.Users
                    .OfType<Customer>()
                    .Where(c => c.Id == customerId && !c.IsDeleted)
                    .FirstOrDefaultAsync();

                if (customer == null)
                {
                    return NotFound("not-found");
                }

                customer.IsDeleted = true;
                customer.DeletedAt = DateTime.Now;

                var result = await _userManager.UpdateAsync(customer);

                if (result.Succeeded)
                {
                    return Ok("customer-deleted");
                }

                return BadRequest(result.Errors);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpGet("public-info")]
        public async Task<ActionResult<CustomerPublicInfoDto>> GetCustomersPublicInfo(Guid customerId)
        {
            try
            {
                var customersPublicInfoDto = await (from user in _userManager.Users.OfType<Customer>()
                                                    join customer in _context.Customers
                                                    on user.Id equals customer.Id
                                                    where !customer.IsDeleted && customer.Id == customerId
                                                    select new CustomerPublicInfoDto
                                                    {
                                                        Id = user.Id,
                                                        Name = customer.FullName,
                                                        PhoneNumber = user.NormalizedPhoneNumber,
                                                        Email = user.Email,
                                                        ProfilePicture = customer.ProfileImage
                                                    }).FirstOrDefaultAsync();

                return Ok(customersPublicInfoDto);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpGet("all-customers-with-vehicles")]
        public async Task<ActionResult<CustomerWithVehicleDto>> GetCustomersWithVehicle([FromQuery] bool includePhoto)
        {
            try
            {
                var customersWithVehicles = await _context.Customers
                    .Where(c => _context.Vehicles.Any(v => v.CustomerId == c.Id && !v.IsDeleted))
                    .Select(c => new CustomerWithVehicleDto
                    {
                        CustomerId = c.Id,
                        FullName = c.FullName,
                        Email = c.Email,
                        PhoneNumber = c.NormalizedPhoneNumber,
                        ProfileImage = includePhoto ? c.ProfileImage : null
                    })
                    .ToListAsync();

                if (!customersWithVehicles.Any())
                {
                    return NotFound("not-found");
                }

                return Ok(customersWithVehicles);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpGet("customers-with-vehicles-by-name")]
        public async Task<ActionResult<CustomerWithVehicleDto>> GetCustomersWithVehicleByName([FromQuery] bool includePhoto, string name)
        {
            try
            {
                var customersWithVehicles = await _context.Customers
                    .Where(c => _context.Vehicles.Any(v => v.CustomerId == c.Id && !v.IsDeleted && c.FullName.Contains(name)))
                    .Select(c => new CustomerWithVehicleDto
                    {
                        CustomerId = c.Id,
                        FullName = c.FullName,
                        Email = c.Email,
                        PhoneNumber = c.NormalizedPhoneNumber,
                        ProfileImage = includePhoto ? c.ProfileImage : null
                    })
                    .ToListAsync();

                if (!customersWithVehicles.Any())
                {
                    return NotFound("not-found");
                }

                return Ok(customersWithVehicles);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPost("create-from-workshop")]
        public async Task<ActionResult> CreateCustomerFromWorkshop([FromBody] PostCustomerFromWorkshopDto model)
        {
            try
            {
                var normalizedPhone = model.PhoneNumber?.Trim().Replace(" ", "").Replace("-", "");

                var existingUser = await _userManager.Users
                    .AnyAsync(u => (u.NormalizedPhoneNumber == normalizedPhone || u.Email == model.Email) && u.UserType == UserType.Customer);

                if (existingUser)
                {
                    return BadRequest("user-already-registered");
                }

                var tempPassword = "MiTaller@" + Guid.NewGuid().ToString("N").Substring(0, 8);

                var customer = new Customer
                {
                    Id = Guid.NewGuid(),
                    UserName = model.Email + "_customer",
                    Email = model.Email,
                    FullName = model.FullName,
                    PhoneNumber = normalizedPhone + "_customer",
                    NormalizedPhoneNumber = normalizedPhone,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = null,
                    DeletedAt = null,
                    IsDeleted = false,
                    UserType = UserType.Customer
                };

                var result = await _userManager.CreateAsync(customer, tempPassword);

                if (!result.Succeeded)
                {
                    return BadRequest(result.Errors);
                }

                var notificationSettings = new NotificationSettings
                {
                    UserId = customer.Id,
                    UserType = UserType.Customer,
                };

                await _context.NotificationSettings.AddAsync(notificationSettings);

                var workshopIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (Guid.TryParse(workshopIdClaim, out var workshopId))
                {
                    await _context.WorkshopCustomers.AddAsync(new WorkshopCustomers
                    {
                        WorkshopId = workshopId,
                        CustomerId = customer.Id,
                    });
                }

                await _context.SaveChangesAsync();

                return Ok(new { customerId = customer.Id, message = "customer-created" });
            }
            catch (Exception)
            {
                return BadRequest("unknown-error");
            }
        }

        [HttpGet("customers-by-name")]
        public async Task<ActionResult<CustomerWithVehicleDto>> GetCustomersByName([FromQuery] bool includePhoto, string name)
        {
            try
            {
                var customersWithVehicles = await _context.Customers
                    .Where(c => c.FullName.Contains(name) && !c.IsDeleted)
                    .Select(c => new CustomerWithVehicleDto
                    {
                        CustomerId = c.Id,
                        FullName = c.FullName,
                        Email = c.Email,
                        PhoneNumber = c.NormalizedPhoneNumber,
                        ProfileImage = includePhoto ? c.ProfileImage : null
                    })
                    .ToListAsync();

                if (!customersWithVehicles.Any())
                {
                    return NotFound("not-found");
                }

                return Ok(customersWithVehicles);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }


    }
}
