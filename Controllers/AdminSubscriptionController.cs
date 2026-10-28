using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiTaller.Data;
using MiTaller.DTO.Subscription;
using MiTaller.Models.Subscription;
using MiTaller.Services;

namespace MiTaller.Controllers
{
    /// <summary>Portal Admin: subscription settings, discount channels/codes and each workshop's subscription.</summary>
    [Route("api/admin/subscription")]
    [ApiController]
    [Authorize(Policy = "PlatformAdmin")]
    [EnableCors("AdminPortal")]
    public class AdminSubscriptionController : ControllerBase
    {
        private static readonly Regex CodePattern = new("^[A-Z0-9_-]{3,40}$");

        private readonly DataContext _context;
        private readonly SubscriptionService _subscriptions;

        public AdminSubscriptionController(DataContext context, SubscriptionService subscriptions)
        {
            _context = context;
            _subscriptions = subscriptions;
        }

        [HttpGet("settings")]
        public async Task<ActionResult<SubscriptionSettingsDto>> GetSettings() => Ok(ToDto(await _subscriptions.GetSettingsAsync()));

        [HttpPut("settings")]
        public async Task<ActionResult<SubscriptionSettingsDto>> UpdateSettings([FromBody] SubscriptionSettingsDto model)
        {
            if (model.MonthlyPrice <= 0 || model.IvaRate < 0 || model.IvaRate >= 1
                || model.AnnualPaidMonths is < 1 or > 12 || model.TrialDays is < 0 or > 365 || model.GraceDays is < 0 or > 60
                || model.FreeMaxEmployees is < 0 or > 100 || model.FreeMaxVehiclesPerMonth is < 0 or > 10000)
            {
                return BadRequest("invalid-settings");
            }

            var settings = await _subscriptions.GetSettingsAsync();
            settings.MonthlyPrice = model.MonthlyPrice;
            settings.IvaRate = model.IvaRate;
            settings.AnnualPaidMonths = model.AnnualPaidMonths;
            settings.TrialDays = model.TrialDays;
            settings.GraceDays = model.GraceDays;
            settings.FreeMaxEmployees = model.FreeMaxEmployees;
            settings.FreeMaxVehiclesPerMonth = model.FreeMaxVehiclesPerMonth;
            settings.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok(ToDto(settings));
        }

        /// <summary>Workshops per displayed status; workshops without a row yet count as their computed trial.</summary>
        [HttpGet("summary")]
        public async Task<ActionResult<SubscriptionSummaryDto>> GetSummary()
        {
            var settings = await _subscriptions.GetSettingsAsync();
            var workshops = await _context.Workshops.Where(w => !w.IsDeleted).Select(w => new { w.Id, w.CreatedAt }).ToListAsync();
            var rows = await _context.WorkshopSubscriptions.ToDictionaryAsync(s => s.WorkshopId);
            var now = DateTime.Now;

            var summary = new SubscriptionSummaryDto { Total = workshops.Count };
            foreach (var workshop in workshops)
            {
                var subscription = rows.GetValueOrDefault(workshop.Id) ?? new WorkshopSubscription
                {
                    TrialEndsAt = (workshop.CreatedAt > settings.LaunchDate ? workshop.CreatedAt : settings.LaunchDate).AddDays(settings.TrialDays),
                };
                switch (SubscriptionService.DisplayStatus(subscription, now))
                {
                    case nameof(SubscriptionStatus.Trial): summary.Trial++; break;
                    case nameof(SubscriptionStatus.PastDue): summary.PastDue++; break;
                    case "Free": summary.Free++; break;
                    default: summary.Active++; break;
                }
            }
            return Ok(summary);
        }

        [HttpGet("sources")]
        public async Task<ActionResult<List<DiscountSourceDto>>> GetSources() =>
            Ok(await _context.DiscountSources.OrderBy(s => s.Name)
                .Select(s => new DiscountSourceDto { Id = s.Id, Name = s.Name, IsActive = s.IsActive })
                .ToListAsync());

        [HttpPost("sources")]
        public async Task<ActionResult<DiscountSourceDto>> CreateSource([FromBody] PostDiscountSourceDto model)
        {
            var name = model.Name.Trim();
            if (name.Length is 0 or > 100) return BadRequest("invalid-name");
            if (await _context.DiscountSources.AnyAsync(s => s.Name == name)) return BadRequest("source-already-exists");

            var source = new DiscountSource { Name = name, IsActive = model.IsActive };
            _context.DiscountSources.Add(source);
            await _context.SaveChangesAsync();
            return Ok(new DiscountSourceDto { Id = source.Id, Name = source.Name, IsActive = source.IsActive });
        }

        // Channels are deactivated, never deleted, so past redemptions keep their channel.
        [HttpPut("sources/{id}")]
        public async Task<ActionResult<DiscountSourceDto>> UpdateSource(int id, [FromBody] PostDiscountSourceDto model)
        {
            var source = await _context.DiscountSources.FindAsync(id);
            if (source == null) return NotFound("not-found");
            var name = model.Name.Trim();
            if (name.Length is 0 or > 100) return BadRequest("invalid-name");
            if (await _context.DiscountSources.AnyAsync(s => s.Name == name && s.Id != id)) return BadRequest("source-already-exists");

            source.Name = name;
            source.IsActive = model.IsActive;
            await _context.SaveChangesAsync();
            return Ok(new DiscountSourceDto { Id = source.Id, Name = source.Name, IsActive = source.IsActive });
        }

        [HttpGet("codes")]
        public async Task<ActionResult<List<DiscountCodeDto>>> GetCodes()
        {
            var codes = await _context.DiscountCodes.OrderByDescending(c => c.CreatedAt).ToListAsync();
            var uses = await _context.DiscountRedemptions
                .GroupBy(r => new { r.DiscountCodeId, Source = r.DiscountSource!.Name })
                .Select(g => new { g.Key.DiscountCodeId, g.Key.Source, Count = g.Count() })
                .ToListAsync();

            return Ok(codes.Select(c =>
            {
                var dto = ToDto(c);
                dto.UsesBySource = uses.Where(u => u.DiscountCodeId == c.Id).ToDictionary(u => u.Source, u => u.Count);
                return dto;
            }).ToList());
        }

        [HttpPost("codes")]
        public async Task<ActionResult<DiscountCodeDto>> CreateCode([FromBody] PostDiscountCodeDto model)
        {
            var code = model.Code.Trim().ToUpperInvariant();
            if (ValidateCode(code, model) is string error) return BadRequest(error);
            if (await _context.DiscountCodes.AnyAsync(c => c.Code == code)) return BadRequest("code-already-exists");

            var discount = new DiscountCode { Code = code };
            Apply(discount, model);
            _context.DiscountCodes.Add(discount);
            await _context.SaveChangesAsync();
            return Ok(ToDto(discount));
        }

        // The code text itself can't change once created: workshops may already have it.
        [HttpPut("codes/{id}")]
        public async Task<ActionResult<DiscountCodeDto>> UpdateCode(int id, [FromBody] PostDiscountCodeDto model)
        {
            var discount = await _context.DiscountCodes.FindAsync(id);
            if (discount == null) return NotFound("not-found");
            if (ValidateCode(discount.Code, model) is string error) return BadRequest(error);

            Apply(discount, model);
            await _context.SaveChangesAsync();
            return Ok(ToDto(discount));
        }

        [HttpGet("workshops/{workshopId}")]
        public async Task<ActionResult<AdminWorkshopSubscriptionDto>> GetWorkshopSubscription(Guid workshopId)
        {
            if (!await _context.Workshops.AnyAsync(w => w.Id == workshopId)) return NotFound("not-found");

            var subscription = await _subscriptions.GetOrCreateAsync(workshopId);
            var redemption = await _context.DiscountRedemptions
                .Where(r => r.WorkshopId == workshopId)
                .OrderByDescending(r => r.RedeemedAt)
                .Select(r => new { Code = r.DiscountCode!.Code, Source = r.DiscountSource!.Name })
                .FirstOrDefaultAsync();
            var payments = await _context.SubscriptionPayments
                .Where(p => p.WorkshopId == workshopId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return Ok(new AdminWorkshopSubscriptionDto
            {
                Status = await _subscriptions.GetStatusAsync(workshopId),
                PaymentMethod = subscription.PaymentMethod.ToString(),
                DiscountCode = redemption?.Code,
                DiscountSource = redemption?.Source,
                Payments = payments.Select(p => new SubscriptionPaymentDto
                {
                    Id = p.Id,
                    BillingPeriod = p.BillingPeriod.ToString(),
                    Method = p.Method.ToString(),
                    Status = p.Status.ToString(),
                    Subtotal = p.Subtotal,
                    Discount = p.Discount,
                    Iva = p.Iva,
                    Total = p.Total,
                    PeriodStart = p.PeriodStart,
                    PeriodEnd = p.PeriodEnd,
                    CreatedAt = p.CreatedAt,
                }).ToList(),
            });
        }

        /// <summary>Adds days to the trial; an expired trial restarts from today.</summary>
        [HttpPost("workshops/{workshopId}/extend-trial")]
        public async Task<ActionResult<AdminWorkshopSubscriptionDto>> ExtendTrial(Guid workshopId, [FromBody] ExtendTrialDto model)
        {
            if (model.Days is < 1 or > 365) return BadRequest("invalid-days");
            if (!await _context.Workshops.AnyAsync(w => w.Id == workshopId)) return NotFound("not-found");

            var subscription = await _subscriptions.GetOrCreateAsync(workshopId);
            var from = subscription.TrialEndsAt > DateTime.Now ? subscription.TrialEndsAt : DateTime.Now;
            subscription.TrialEndsAt = from.AddDays(model.Days);
            subscription.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return await GetWorkshopSubscription(workshopId);
        }

        private static string? ValidateCode(string code, PostDiscountCodeDto model)
        {
            if (!CodePattern.IsMatch(code)) return "invalid-code";
            if (!Enum.IsDefined(model.BillingPeriod) || !Enum.IsDefined(model.Type)) return "invalid-code";
            if (model.Value <= 0 || (model.Type == DiscountType.Percent && model.Value > 100)) return "invalid-value";
            if (model.Charges is < 1 || model.MaxUses is < 1) return "invalid-limits";
            if (model.ValidFrom != null && model.ValidUntil != null && model.ValidUntil < model.ValidFrom) return "invalid-dates";
            return null;
        }

        private static void Apply(DiscountCode discount, PostDiscountCodeDto model)
        {
            discount.BillingPeriod = model.BillingPeriod;
            discount.Type = model.Type;
            discount.Value = model.Value;
            discount.Charges = model.Charges;
            discount.ValidFrom = model.ValidFrom;
            discount.ValidUntil = model.ValidUntil;
            discount.MaxUses = model.MaxUses;
            discount.IsActive = model.IsActive;
        }

        private static DiscountCodeDto ToDto(DiscountCode c) => new()
        {
            Id = c.Id,
            Code = c.Code,
            BillingPeriod = c.BillingPeriod,
            Type = c.Type,
            Value = c.Value,
            Charges = c.Charges,
            ValidFrom = c.ValidFrom,
            ValidUntil = c.ValidUntil,
            MaxUses = c.MaxUses,
            UsedCount = c.UsedCount,
            IsActive = c.IsActive,
            CreatedAt = c.CreatedAt,
        };

        private static SubscriptionSettingsDto ToDto(SubscriptionSettings s) => new()
        {
            MonthlyPrice = s.MonthlyPrice,
            IvaRate = s.IvaRate,
            AnnualPaidMonths = s.AnnualPaidMonths,
            TrialDays = s.TrialDays,
            GraceDays = s.GraceDays,
            FreeMaxEmployees = s.FreeMaxEmployees,
            FreeMaxVehiclesPerMonth = s.FreeMaxVehiclesPerMonth,
            LaunchDate = s.LaunchDate,
            UpdatedAt = s.UpdatedAt,
        };
    }
}
