using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiTaller.Data;
using MiTaller.Models.Auth;
using MiTaller.Models.Subscription;

namespace MiTaller.Controllers
{
    /// <summary>
    /// Server-to-server feed for the marketing portal (its own API, not a browser): aggregates for CAC/churn/LTV/ROAS and
    /// the funnel, plus the trial-ending list for its lifecycle campaigns. Protected by the X-Marketing-Key header.
    /// </summary>
    [Route("api/marketing-feed")]
    [ApiController]
    [AllowAnonymous]
    public class MarketingFeedController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly string _key;

        public MarketingFeedController(DataContext context, IConfiguration config)
        {
            _context = context;
            _key = config["Marketing:FeedKey"] ?? string.Empty;
        }

        private bool Authorized()
        {
            // An unset key disables the feed instead of leaving it open
            if (_key.Length < 24) return false;
            var sent = Request.Headers["X-Marketing-Key"].ToString();
            return CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(sent), Encoding.UTF8.GetBytes(_key));
        }

        /// <summary>Lead matching without sending emails: sha256 of the trimmed, lowercased address.</summary>
        public static string EmailHash(string? email) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes((email ?? string.Empty).Trim().ToLowerInvariant()))).ToLowerInvariant();

        private static decimal Net(SubscriptionPayment p) => p.Subtotal - p.Discount;

        /// <summary>Totals for [from, to). Money is before IVA, in MXN.</summary>
        [HttpGet("summary")]
        public async Task<IActionResult> Summary([FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            if (!Authorized()) return Unauthorized();
            if (to <= from || (to - from).TotalDays > 400) return BadRequest("invalid-range");

            var workshopUsers = _context.Users.Where(u => u.UserType == UserType.Workshop && !u.IsDeleted);
            var signups = await workshopUsers
                .Where(u => u.CreatedAt >= from && u.CreatedAt < to)
                .Select(u => new { u.Id, u.Email, u.CreatedAt })
                .ToListAsync();

            var approved = _context.SubscriptionPayments.Where(p => p.Status == PaymentStatus.Approved);
            var firstPaid = await approved
                .GroupBy(p => p.WorkshopId)
                .Select(g => new { WorkshopId = g.Key, First = g.Min(p => p.CreatedAt) })
                .ToListAsync();
            var firstPaidById = firstPaid.ToDictionary(x => x.WorkshopId, x => x.First);

            var paymentsInRange = await approved.Where(p => p.CreatedAt >= from && p.CreatedAt < to).ToListAsync();
            var subs = await _context.WorkshopSubscriptions.AsNoTracking().ToListAsync();

            // Paying at the start = had a paid period covering `from`; the churn denominator
            var payingAtStart = await approved
                .Where(p => p.PeriodStart <= from && p.PeriodEnd > from)
                .Select(p => p.WorkshopId).Distinct().CountAsync();

            var paying = subs.Where(s => s.Status is SubscriptionStatus.Active or SubscriptionStatus.PastDue).ToList();
            var lastPayments = await approved
                .GroupBy(p => p.WorkshopId)
                .Select(g => g.OrderByDescending(p => p.CreatedAt).First())
                .ToListAsync();
            var lastById = lastPayments.ToDictionary(p => p.WorkshopId);
            var mrr = paying.Sum(s => lastById.TryGetValue(s.WorkshopId, out var p)
                ? Net(p) / (p.BillingPeriod == BillingPeriod.Annual ? 12m : 1m) : 0m);

            var sources = await _context.DiscountRedemptions
                .Include(r => r.DiscountSource)
                .Select(r => new { r.WorkshopId, r.RedeemedAt, Source = r.DiscountSource!.Name })
                .ToListAsync();
            var bySource = sources
                .GroupBy(r => r.Source)
                .Select(g => new
                {
                    source = g.Key,
                    signups = g.Count(r => r.RedeemedAt >= from && r.RedeemedAt < to),
                    payingNow = g.Count(r => paying.Any(s => s.WorkshopId == r.WorkshopId)),
                    revenue = paymentsInRange.Where(p => g.Any(r => r.WorkshopId == p.WorkshopId)).Sum(Net),
                })
                .OrderByDescending(x => x.signups)
                .ToList();

            return Ok(new
            {
                from,
                to,
                signups = signups.Count,
                firstPayments = firstPaid.Count(x => x.First >= from && x.First < to),
                revenue = paymentsInRange.Sum(Net),
                revenueNew = paymentsInRange
                    .Where(p => firstPaidById.TryGetValue(p.WorkshopId, out var f) && f >= from && f < to).Sum(Net),
                canceled = subs.Count(s => s.Status == SubscriptionStatus.Canceled && s.UpdatedAt >= from && s.UpdatedAt < to),
                payingAtStart,
                payingNow = paying.Count,
                trialsNow = subs.Count(s => s.Status == SubscriptionStatus.Trial && s.TrialEndsAt > DateTime.UtcNow),
                mrr,
                bySource,
                signupsDetail = signups.Select(u => new
                {
                    emailHash = EmailHash(u.Email),
                    u.CreatedAt,
                    firstPaidAt = firstPaidById.TryGetValue(u.Id, out var f) ? f : (DateTime?)null,
                }),
            });
        }

        /// <summary>Workshops still on trial whose trial ends between today+fromDays and today+toDays (negative = already ended).</summary>
        [HttpGet("trial-ending")]
        public async Task<IActionResult> TrialEnding([FromQuery] int fromDays = -7, [FromQuery] int toDays = 15)
        {
            if (!Authorized()) return Unauthorized();
            if (fromDays < -60 || toDays > 60 || toDays < fromDays) return BadRequest("invalid-range");

            var today = DateTime.UtcNow.Date;
            var start = today.AddDays(fromDays);
            var end = today.AddDays(toDays + 1);
            var rows = await (
                from s in _context.WorkshopSubscriptions
                join w in _context.Workshops on s.WorkshopId equals w.Id
                join u in _context.Users on s.WorkshopId equals u.Id
                where s.Status == SubscriptionStatus.Trial && s.TrialEndsAt >= start && s.TrialEndsAt < end
                      && !u.IsDeleted && u.UserType == UserType.Workshop
                select new
                {
                    workshopId = s.WorkshopId,
                    email = u.Email,
                    name = w.AssociateFullName,
                    workshopName = w.WorkshopName,
                    trialEndsAt = s.TrialEndsAt,
                    language = u.PreferredLanguage,
                }).ToListAsync();
            return Ok(rows);
        }
    }
}
