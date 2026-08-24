using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiTaller.Data;
using MiTaller.DTO.Customer;
using MiTaller.Models.Customer;
using MiTaller.Models.Domain;

namespace MiTaller.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EmergencyContactController : ControllerBase
    {
        private readonly DataContext _context;

        public EmergencyContactController(DataContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public async Task<ActionResult> CreateEmergencyContact(PostEmergencyContactDto model)
        {
            try
            {
                var customer = await _context.Customers
                    .Where(c => c.Id == model.CustomerId && !c.IsDeleted)
                    .FirstOrDefaultAsync();

                if (customer == null)
                {
                    return NotFound("not-found");
                }

                var existingEmergencyContact = await _context.EmergencyContacts
                    .AnyAsync(u => (u.PhoneNumber == model.PhoneNumber || u.Email == model.Email) && u.CustomerId == model.CustomerId);

                if (existingEmergencyContact)
                {
                    return BadRequest("emergency-contact-already-registered");
                }

                var newEmergencyContact = new EmergencyContact
                {
                    CustomerId = model.CustomerId,
                    FullName = model.FullName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    MustBeNotified = model.MustBeNotified
                };

                await _context.EmergencyContacts.AddAsync(newEmergencyContact);
                await _context.SaveChangesAsync();

                return Ok("emergency-contact-created");
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpGet("{customerId}")]
        public async Task<ActionResult<IEnumerable<EmergencyContactResponseDto>>> GetCustomerEmergencyContacts(Guid customerId)
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

                var emergencyContacts = await _context.EmergencyContacts
                    .Where(e => e.CustomerId == customerId && e.IsDeleted == false)
                    .ToListAsync();

                if (emergencyContacts == null)
                {
                    return NotFound("not-found");
                }

                var emergencyContactsDto = new List<EmergencyContactResponseDto>();
                foreach (var model in emergencyContacts)
                {
                    var emergencyContactDto = new EmergencyContactResponseDto
                    {
                        Id = model.Id,
                        CustomerId = model.CustomerId,
                        FullName = model.FullName,
                        Email = model.Email,
                        PhoneNumber = model.PhoneNumber,
                        MustBeNotified = model.MustBeNotified,
                    };

                    emergencyContactsDto.Add(emergencyContactDto);
                }

                return Ok(emergencyContactsDto);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpDelete("{emergencyContactId}")]
        public async Task<ActionResult> DeleteCustomer(int emergencyContactId)
        {
            try
            {
                var emergencyContact = await _context.EmergencyContacts
                    .Where(e => e.Id == emergencyContactId && !e.IsDeleted)
                    .FirstOrDefaultAsync();

                if (emergencyContact == null)
                {
                    return NotFound("not-found");
                }

                emergencyContact.IsDeleted = true;

                await _context.SaveChangesAsync();

                return Ok("emergency-contact-deleted");
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }
    }
}
