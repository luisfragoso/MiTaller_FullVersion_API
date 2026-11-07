using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiTaller.Data;
using MiTaller.Models.Auth;
using MiTaller.Models.Interest;

namespace MiTaller.Controllers
{
    /// "Avisarme cuando esté disponible": measures demand for features we can't offer yet.
    [Route("api/feature-interest")]
    [ApiController]
    [Authorize]
    public class FeatureInterestController : ControllerBase
    {
        public static readonly HashSet<string> Features = new() { "credit" };

        private readonly DataContext _context;

        public FeatureInterestController(DataContext context) => _context = context;

        private Guid? CallerCustomerId() =>
            Enum.TryParse<UserType>(User.FindFirst("UserType")?.Value, out var type) && type == UserType.Customer
            && Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

        [HttpGet("{feature}")]
        public async Task<ActionResult<object>> Get(string feature)
        {
            if (!Features.Contains(feature)) return NotFound("unknown-feature");
            if (CallerCustomerId() is not Guid customerId) return StatusCode(StatusCodes.Status403Forbidden, "customer-only");

            var interested = await _context.FeatureInterests.AnyAsync(f => f.CustomerId == customerId && f.Feature == feature);
            return Ok(new { interested });
        }

        // Idempotent: asking twice keeps a single row
        [HttpPost("{feature}")]
        public async Task<ActionResult<object>> Register(string feature)
        {
            if (!Features.Contains(feature)) return NotFound("unknown-feature");
            if (CallerCustomerId() is not Guid customerId) return StatusCode(StatusCodes.Status403Forbidden, "customer-only");

            if (!await _context.FeatureInterests.AnyAsync(f => f.CustomerId == customerId && f.Feature == feature))
            {
                _context.FeatureInterests.Add(new FeatureInterest { CustomerId = customerId, Feature = feature });
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    // Double tap raced the unique index: already registered
                }
            }
            return Ok(new { interested = true });
        }
    }
}
