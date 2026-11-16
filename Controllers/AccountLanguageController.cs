using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiTaller.Data;
using MiTaller.Services.Localization;

namespace MiTaller.Controllers
{
    public class PutLanguageDto
    {
        public string Language { get; set; } = Lang.Es;
    }

    /// Lets the apps save the caller's language when it changes in Settings (login also saves it).
    [Route("api/account/language")]
    [ApiController]
    [Authorize]
    public class AccountLanguageController : ControllerBase
    {
        private readonly DataContext _context;

        public AccountLanguageController(DataContext context) => _context = context;

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] PutLanguageDto model)
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)) return Unauthorized();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
            if (user == null) return NotFound("not-found");
            user.PreferredLanguage = Lang.Normalize(model.Language);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
