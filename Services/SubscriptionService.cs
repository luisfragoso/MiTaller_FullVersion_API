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
