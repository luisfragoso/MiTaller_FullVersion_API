using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiTaller.Attributes;
using MiTaller.Data;
using MiTaller.DTO.Address;
using MiTaller.Models.Address;
using MiTaller.Models.Auth;

namespace MiTaller.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AddressController : ControllerBase
    {
        public readonly DataContext _dataContext;
        private readonly UserManager<BaseIdentityUser> _userManager;

        public AddressController(DataContext dataContext, UserManager<BaseIdentityUser> userManager)
        {
            _dataContext = dataContext;
            _userManager = userManager;
        }

        [HttpGet("Info-by-zipcode")]
        public async Task<ActionResult<AddressDto>> GetStates(string Zipcode)
        {
            try
            {
                var suburbs = await _dataContext.Suburbs
                .Where(sub => sub.Zipcode == Zipcode)
                .Include(sub => sub.Town)
                .ThenInclude(town => town.State)
                .ToListAsync();

                if (suburbs.Count == 0)
                {
                    return NotFound("not-found");
                }

                // Agrupar por estado y municipio
                var result = suburbs
                    .GroupBy(sub => new {
                        StateId = sub.Town.State.Id,
                        StateName = sub.Town.State.Name,
                        TownId = sub.Town.Id,
                        TownName = sub.Town.Name
                    })
                    .Select(group => new AddressDto
                    {
                        State = group.Key.StateName,  // Estado
                        Town = group.Key.TownName,    // Municipio
                        SuburbList = group.Select(sub => new Suburb
                        {
                            Id = sub.Id,
                            TownId = sub.TownId,
                            Zipcode = sub.Zipcode,
                            Name = sub.Name
                        }).ToList()
                    })
                    .FirstOrDefault();

                return Ok(result);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }

        }

        [HttpPost("register-address")]
        [Authorize]
        [AuthorizeEmployee("Administrador")]
        public async Task<ActionResult> RegisterAddress([FromBody] PostAccountAddressDto model)
        {
            try
            {
                var account = await _userManager.Users
                .Where(u => u.Id == model.Id
                        && u.UserType == model.UserType)
                .SingleOrDefaultAsync();

                if (account == null)
                {
                    return NotFound("not-found");
                }

                if (model.UserType == UserType.Customer)
                {
                    var customerAddress = await _dataContext.CustomerAddresses
                        .Where(c => c.CustomerId == model.Id)
                        .FirstOrDefaultAsync();

                    if (customerAddress != null)
                    {
                        return BadRequest("address-already-registered");
                    }

                    var newCustomerAddress = new CustomerAddress
                    {
                        CustomerId = model.Id,
                        SuburbId = model.SuburbId,
                        Street = model.Street
                    };

                    _dataContext.CustomerAddresses.Add(newCustomerAddress);
                    await _dataContext.SaveChangesAsync();

                    return Ok("address-registered");

                }
                else if (model.UserType == UserType.Workshop)
                {

                    var workshopAddress = await _dataContext.WorkshopAddresses
                        .Where(c => c.WorkshopId == model.Id)
                        .FirstOrDefaultAsync();

                    if (workshopAddress != null)
                    {
                        return BadRequest("address-already-registered");
                    }

                    var newWorkshopAddress = new WorkshopAddress { 
                        WorkshopId = model.Id,
                        SuburbId = model.SuburbId,
                        Street = model.Street
                    };

                    _dataContext.WorkshopAddresses.Add(newWorkshopAddress);
                    await _dataContext.SaveChangesAsync();

                    return Ok("address-registered");
                }
                return Ok();

            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }

        }

        [HttpPut("update-address")]
        [Authorize]
        [AuthorizeEmployee("Administrador")]
        public async Task<ActionResult> UpdateAddress([FromBody] PostAccountAddressDto model)
        {
            try
            {
                var account = await _userManager.Users
                .Where(u => u.Id == model.Id
                        && u.UserType == model.UserType)
                .SingleOrDefaultAsync();

                if (account == null)
                {
                    return NotFound("not-found");
                }

                if (model.UserType == UserType.Customer)
                {
                    var customerAddress = await _dataContext.CustomerAddresses
                        .Where(u => u.CustomerId == model.Id && !u.IsDeleted)
                        .FirstOrDefaultAsync();

                    if (customerAddress == null)
                    {
                        return NotFound("not-found");
                    }

                    customerAddress.SuburbId = model.SuburbId;
                    customerAddress.Street = model.Street;

                    await _dataContext.SaveChangesAsync();

                    return Ok("address-updated");

                }
                else if (model.UserType == UserType.Workshop)
                {
                    var workshopAddress = await _dataContext.WorkshopAddresses
                        .Where(u => u.WorkshopId == model.Id && !u.IsDeleted)
                        .FirstOrDefaultAsync();

                    if (workshopAddress == null)
                    {
                        return NotFound("not-found");
                    }

                    workshopAddress.SuburbId = model.SuburbId;
                    workshopAddress.Street = model.Street;

                    await _dataContext.SaveChangesAsync();

                    return Ok("address-updated");
                }
                return Ok();

            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }

        }

        [HttpGet("get-account-address")]
        public async Task<ActionResult<AccountAddressDto>> GetAccountAddress(UserType userType, Guid accountId)
        {
            try
            {
                var account = await _userManager.Users
                .Where(u => u.Id == accountId
                        && u.UserType == userType)
                .SingleOrDefaultAsync();

                if (account == null)
                {
                    return NotFound("not-found");
                }

                if (userType == UserType.Customer)
                {
                    var address = await _dataContext.CustomerAddresses
                        .Where(ca => account.Id == ca.CustomerId)
                        .Include(ca => ca.Suburb)
                        .ThenInclude(sub => sub.Town)
                        .ThenInclude(tow => tow.State)
                        .Select(ca => new AccountAddressDto
                        {
                            AccountId = ca.CustomerId,
                            StateName = ca.Suburb.Town.State.Name,
                            TownName = ca.Suburb.Town.Name,
                            SuburbName = ca.Suburb.Name,
                            SuburbId = ca.Suburb.Id,
                            Zipcode = ca.Suburb.Zipcode,
                            Street = ca.Street,
                        })
                        .FirstOrDefaultAsync();

                    if (address == null)
                    {
                        return NotFound("not-found");
                    }

                    return Ok(address);

                }
                else if (userType == UserType.Workshop)
                {
                    var address = await _dataContext.WorkshopAddresses
                        .Where(wa => account.Id == wa.WorkshopId)
                        .Include(wa => wa.Suburb)
                        .ThenInclude(sub => sub.Town)
                        .ThenInclude(tow => tow.State)
                        .Select(wa => new AccountAddressDto
                        {
                            AccountId = wa.WorkshopId,
                            StateName = wa.Suburb.Town.State.Name,
                            TownName = wa.Suburb.Town.Name,
                            SuburbName = wa.Suburb.Name,
                            SuburbId = wa.Suburb.Id,
                            Zipcode = wa.Suburb.Zipcode,
                            Street = wa.Street,
                        })
                        .FirstOrDefaultAsync();

                    if (address == null)
                    {
                        return NotFound("not-found");
                    }

                    return Ok(address);
                }

                return Ok();

            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }

        }


    }
}
