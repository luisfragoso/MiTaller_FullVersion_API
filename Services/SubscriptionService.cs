using Microsoft.EntityFrameworkCore;
using MiTaller.Data;
using MiTaller.DTO.Subscription;
using MiTaller.Models.Subscription;

namespace MiTaller.Services
{
    /// <summary>
    /// Decides a workshop's effective plan (Pro while in trial, paid period or grace; otherwise Gratis)
    /// and the Gratis limits. Computed on every call, never cached in the JWT, so a payment applies at once.
    /// </summary>
    public class SubscriptionService
    {
        public const string PlanPro = "Pro";
        public const string PlanFree = "Gratis";
        public const string RequiresProError = "plan-requires-pro";
        public const string EmployeeLimitError = "plan-limit-employees";
        public const string VehicleLimitError = "plan-limit-vehicles";

        private readonly DataContext _context;

        public SubscriptionService(DataContext context)
        {
            _context = context;
        }

        public Task<SubscriptionSettings> GetSettingsAsync() => _context.SubscriptionSettings.SingleAsync(s => s.Id == 1);

        /// <summary>Creates the row on first use: the trial runs from signup, or from the launch date for older workshops.</summary>
        public async Task<WorkshopSubscription> GetOrCreateAsync(Guid workshopId)
        {
            var subscription = await _context.WorkshopSubscriptions.FirstOrDefaultAsync(s => s.WorkshopId == workshopId);
            if (subscription != null) return subscription;

            var settings = await GetSettingsAsync();
            var signup = await _context.Workshops.Where(w => w.Id == workshopId).Select(w => w.CreatedAt).SingleAsync();
            var trialStart = signup > settings.LaunchDate ? signup : settings.LaunchDate;
            subscription = new WorkshopSubscription
            {
                WorkshopId = workshopId,
                TrialEndsAt = trialStart.AddDays(settings.TrialDays),
            };
            _context.WorkshopSubscriptions.Add(subscription);
            await _context.SaveChangesAsync();
            return subscription;
        }

        public static bool IsPro(WorkshopSubscription subscription, DateTime now) =>
            now < subscription.TrialEndsAt
            || subscription.CurrentPeriodEnd > now
            || subscription.GraceEndsAt > now;

        public async Task<bool> IsProAsync(Guid workshopId) => IsPro(await GetOrCreateAsync(workshopId), DateTime.Now);

        /// <summary>Null when the workshop may add one more employee, otherwise the error code.</summary>
        public async Task<string?> CheckEmployeeLimitAsync(Guid workshopId)
        {
            if (await IsProAsync(workshopId)) return null;
            var settings = await GetSettingsAsync();
            return await CountEmployeesAsync(workshopId) >= settings.FreeMaxEmployees ? EmployeeLimitError : null;
        }

        /// <summary>Null when the workshop may register one more vehicle this month, otherwise the error code.</summary>
        public async Task<string?> CheckVehicleLimitAsync(Guid workshopId)
        {
            if (await IsProAsync(workshopId)) return null;
            var settings = await GetSettingsAsync();
            return await CountVehiclesThisMonthAsync(workshopId) >= settings.FreeMaxVehiclesPerMonth ? VehicleLimitError : null;
        }

        public async Task<SubscriptionStatusDto> GetStatusAsync(Guid workshopId)
        {
            var subscription = await GetOrCreateAsync(workshopId);
            var settings = await GetSettingsAsync();
            var now = DateTime.Now;
            var annualPrice = settings.MonthlyPrice * settings.AnnualPaidMonths;

            return new SubscriptionStatusDto
            {
                Plan = IsPro(subscription, now) ? PlanPro : PlanFree,
                Status = DisplayStatus(subscription, now),
                InTrial = now < subscription.TrialEndsAt,
                TrialEndsAt = subscription.TrialEndsAt,
                TrialDaysLeft = Math.Max(0, (int)Math.Ceiling((subscription.TrialEndsAt - now).TotalDays)),
                BillingPeriod = subscription.BillingPeriod?.ToString(),
                CurrentPeriodEnd = subscription.CurrentPeriodEnd,
                GraceEndsAt = subscription.GraceEndsAt,
                MonthlyPrice = settings.MonthlyPrice,
                MonthlyTotal = WithIva(settings.MonthlyPrice, settings.IvaRate),
                AnnualPrice = annualPrice,
                AnnualTotal = WithIva(annualPrice, settings.IvaRate),
                IvaRate = settings.IvaRate,
                FreeMaxEmployees = settings.FreeMaxEmployees,
                FreeMaxVehiclesPerMonth = settings.FreeMaxVehiclesPerMonth,
                Employees = await CountEmployeesAsync(workshopId),
                VehiclesThisMonth = await CountVehiclesThisMonthAsync(workshopId),
            };
        }

        // The stored Status only changes on payment events; what the workshop sees depends on today's date.
        public static string DisplayStatus(WorkshopSubscription subscription, DateTime now) =>
            now < subscription.TrialEndsAt ? nameof(SubscriptionStatus.Trial)
            : subscription.CurrentPeriodEnd > now ? subscription.Status.ToString()
            : subscription.GraceEndsAt > now ? nameof(SubscriptionStatus.PastDue)
            : "Free";

        public static SubscriptionPaymentDto ToPaymentDto(SubscriptionPayment p, string? workshopName = null) => new()
        {
            Id = p.Id,
            WorkshopId = p.WorkshopId,
            WorkshopName = workshopName,
            MpPaymentId = p.MpPaymentId,
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
        };

        /// <summary>
        /// Prices a payment of the given period for the workshop, validating the code if one is sent.
        /// Returns the error code instead when the code or channel can't be used.
        /// </summary>
        public async Task<(SubscriptionQuoteDto? Quote, string? Error)> QuoteAsync(Guid workshopId, SubscriptionQuoteRequestDto request)
        {
            if (!Enum.IsDefined(request.BillingPeriod)) return (null, "invalid-period");
            var settings = await GetSettingsAsync();
            var subscription = await GetOrCreateAsync(workshopId);
            var now = DateTime.Now;

            var subtotal = request.BillingPeriod == BillingPeriod.Annual
                ? settings.MonthlyPrice * settings.AnnualPaidMonths
                : settings.MonthlyPrice;

            DiscountCode? code = null;
            if (!string.IsNullOrWhiteSpace(request.Code))
            {
                var text = request.Code.Trim().ToUpperInvariant();
                code = await _context.DiscountCodes.FirstOrDefaultAsync(c => c.Code == text);
                var error = code switch
                {
                    null => "code-not-found",
                    { IsActive: false } => "code-not-found",
                    _ when code.ValidFrom > now => "code-not-started",
                    _ when code.ValidUntil < now => "code-expired",
                    _ when code.BillingPeriod != request.BillingPeriod => "code-wrong-period",
                    _ when code.MaxUses != null && code.UsedCount >= code.MaxUses => "code-max-uses",
                    _ => null,
                };
                if (error != null) return (null, error);
                if (await _context.DiscountRedemptions.AnyAsync(r => r.DiscountCodeId == code!.Id && r.WorkshopId == workshopId))
                    return (null, "code-already-used");
                if (request.DiscountSourceId == null
                    || !await _context.DiscountSources.AnyAsync(s => s.Id == request.DiscountSourceId && s.IsActive))
                    return (null, "invalid-source");
            }

            var discount = code == null ? 0m
                : code.Type == DiscountType.Percent ? Math.Round(subtotal * code.Value / 100, 2)
                : Math.Min(code.Value, subtotal);
            var taxable = subtotal - discount;
            var iva = Math.Round(taxable * settings.IvaRate, 2);

            // A payment starts when the trial or the already-paid period ends, so no paid day is lost.
            var start = new[] { now, subscription.TrialEndsAt, subscription.CurrentPeriodEnd ?? now }.Max();
            var end = request.BillingPeriod == BillingPeriod.Annual ? start.AddYears(1) : start.AddMonths(1);

            return (new SubscriptionQuoteDto
            {
                BillingPeriod = request.BillingPeriod.ToString(),
                Subtotal = subtotal,
                Discount = discount,
                Iva = iva,
                Total = taxable + iva,
                CoverageStart = start,
                CoverageEnd = end,
                Code = code?.Code,
                CodeDescription = code == null ? null : DescribeCode(code),
            }, null);
        }

        private static string DescribeCode(DiscountCode code)
        {
            var value = code.Type == DiscountType.Percent ? $"{code.Value:0.##}%" : $"${code.Value:N2}";
            var charges = code.Charges switch
            {
                null => "en todos tus cobros",
                1 => "en tu primer cobro",
                var n => $"en tus primeros {n} cobros",
            };
            return $"{value} de descuento {charges}";
        }

        public static decimal WithIva(decimal amount, decimal ivaRate) => Math.Round(amount * (1 + ivaRate), 2);

        private Task<int> CountEmployeesAsync(Guid workshopId) =>
            _context.WorkshopEmployees.CountAsync(e => e.WorkshopId == workshopId && !e.IsDeleted);

        // Inspection.InspectionDate is set by the server (local time), so it can't be backdated to dodge the limit.
        private Task<int> CountVehiclesThisMonthAsync(Guid workshopId)
        {
            var now = DateTime.Now;
            var monthStart = new DateTime(now.Year, now.Month, 1);
            return _context.Inspections.CountAsync(i => i.WorkshopId == workshopId && i.LegacySource == null && i.InspectionDate >= monthStart);
        }
    }
}
