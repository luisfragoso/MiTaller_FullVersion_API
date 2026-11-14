using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiTaller.DTO.Settings;
using MiTaller.Services;

namespace MiTaller.Controllers
{
    /// <summary>Public contact addresses for the apps and the site.</summary>
    [Route("api/contact")]
    [ApiController]
    [AllowAnonymous]
    public class ContactController : ControllerBase
    {
        private readonly ContactSettingsService _contact;

        public ContactController(ContactSettingsService contact) => _contact = contact;

        [HttpGet]
        public async Task<ActionResult<ContactSettingsDto>> Get()
        {
            var s = await _contact.GetAsync();
            return Ok(new ContactSettingsDto { InfoEmail = s.InfoEmail, SupportEmail = s.SupportEmail });
        }
    }
}
