using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using MiTaller.Data;
using MiTaller.DTO.Settings;
using MiTaller.Models.Settings;
using MiTaller.Services;

namespace MiTaller.Controllers
{
    [Route("api/admin/contact")]
    [ApiController]
    [Authorize(Policy = "PlatformAdmin")]
    [EnableCors("AdminPortal")]
    public class AdminContactController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly ContactSettingsService _contact;

        public AdminContactController(DataContext context, ContactSettingsService contact)
        {
            _context = context;
            _contact = contact;
        }

        [HttpGet]
        public async Task<ActionResult<ContactSettingsDto>> Get() => Ok(ToDto(await _contact.GetAsync()));

        [HttpPut]
        public async Task<ActionResult<ContactSettingsDto>> Update([FromBody] ContactSettingsDto model)
        {
            var info = model.InfoEmail?.Trim() ?? "";
            var support = model.SupportEmail?.Trim() ?? "";
            if (!IsEmail(info)) return BadRequest("invalid-info-email");
            if (!IsEmail(support)) return BadRequest("invalid-support-email");

            var settings = await _contact.GetAsync();
            settings.InfoEmail = info;
            settings.SupportEmail = support;
            settings.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return Ok(ToDto(settings));
        }

        private static bool IsEmail(string value) =>
            value.Length is > 0 and <= 254 && new EmailAddressAttribute().IsValid(value) && value.Contains('.');

        private static ContactSettingsDto ToDto(ContactSettings s) =>
            new() { InfoEmail = s.InfoEmail, SupportEmail = s.SupportEmail, UpdatedAt = s.UpdatedAt };
    }
}
