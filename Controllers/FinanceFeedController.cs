using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiTaller.Data;
using MiTaller.Services;

namespace MiTaller.Controllers
{
    /// <summary>
    /// Server-to-server feed for the finance portal: every subscription payment (any status) so it can book the
    /// approved ones as income and the refunds as reversals. Protected by the X-Finance-Key header.
    /// </summary>
    [Route("api/finance-feed")]
    [ApiController]
    [AllowAnonymous]
    public class FinanceFeedController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly string _key;

        public FinanceFeedController(DataContext context, IConfiguration config)
        {
            _context = context;
            _key = config["Finance:FeedKey"] ?? string.Empty;
        }

        private bool Authorized()
        {
            if (_key.Length < 24) return false; // unset key = feed off, never open
            var sent = Request.Headers["X-Finance-Key"].ToString();
            return CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(sent), Encoding.UTF8.GetBytes(_key));
        }

        /// <summary>Payments created in [from, to); the finance portal syncs a rolling window.</summary>
        [HttpGet("payments")]
        public async Task<IActionResult> Payments([FromQuery(Name = "from")] DateTime start, [FromQuery(Name = "to")] DateTime end)
        {
            if (!Authorized()) return Unauthorized();
            if (end <= start || (end - start).TotalDays > 800) return BadRequest("invalid-range");

            var rows = await (
                from p in _context.SubscriptionPayments.AsNoTracking()
                join w in _context.Workshops on p.WorkshopId equals w.Id into ws
                from w in ws.DefaultIfEmpty()
                where p.CreatedAt >= start && p.CreatedAt < end
                orderby p.CreatedAt
                select new { Payment = p, WorkshopName = w != null ? w.WorkshopName : null })
                .ToListAsync();
            return Ok(rows.Select(r => SubscriptionService.ToPaymentDto(r.Payment, r.WorkshopName)));
        }
    }
}
