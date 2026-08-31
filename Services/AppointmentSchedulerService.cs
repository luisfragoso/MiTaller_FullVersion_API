using Microsoft.EntityFrameworkCore;
using MiTaller.Data;

namespace MiTaller.Services
{
    /// <summary>
    /// Revisa periódicamente las citas "Pendiente" para mandar el recordatorio ~1 día
    /// antes, el aviso de "por vencer" ~4h antes, y cancelarlas automáticamente ~2h
    /// antes si nadie las confirmó. No usa Hangfire/Quartz - el volumen no lo justifica,
    /// un PeriodicTimer basta y no agrega dependencias ni tablas nuevas.
    /// </summary>
    public class AppointmentSchedulerService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<AppointmentSchedulerService> _logger;

        // Ajustables para pruebas locales (acortar para ver un ciclo completo en minutos
        // en vez de horas reales).
        private static readonly TimeSpan PollInterval = TimeSpan.FromMinutes(15);
        private static readonly TimeSpan ReminderWindow = TimeSpan.FromHours(25);
        private static readonly TimeSpan ExpiringWindow = TimeSpan.FromHours(4);
        private static readonly TimeSpan CancelWindow = TimeSpan.FromHours(2);

        // Citas ya vencidas por más de esto al momento del primer arranque se cancelan
        // en silencio (sin correo/notificación) para no bombardear con avisos de citas
        // viejas que quedaron "Pendiente" desde antes de que existiera este job.
        private static readonly TimeSpan StaleBacklogThreshold = TimeSpan.FromHours(24);

        public AppointmentSchedulerService(
            IServiceScopeFactory scopeFactory,
            ILogger<AppointmentSchedulerService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(PollInterval);
            do
            {
                try
                {
                    await RunOnceAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error procesando recordatorios/cancelaciones de citas");
                }
            } while (await timer.WaitForNextTickAsync(stoppingToken));
        }

        private async Task RunOnceAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DataContext>();
            var notificationService = scope.ServiceProvider.GetRequiredService<AppointmentNotificationService>();

            var now = DateTime.Now;

            await SendRemindersAsync(context, notificationService, now, ct);
            await SendExpiringNoticesAsync(context, notificationService, now, ct);
            await CancelOverdueAsync(context, notificationService, now, ct);
        }

        private async Task SendRemindersAsync(
            DataContext context, AppointmentNotificationService notificationService, DateTime now, CancellationToken ct)
        {
            var dueForReminder = await context.Appointments
                .Where(a => a.Status == "Pendiente"
                    && a.ReminderSentAt == null
                    && a.Date > now
                    && a.Date <= now.Add(ReminderWindow))
                .Include(a => a.Customer)
                .Include(a => a.Workshop)
                .ToListAsync(ct);

            foreach (var appointment in dueForReminder)
            {
                await notificationService.NotifyReminderAsync(appointment, appointment.Customer, appointment.Workshop);
                appointment.ReminderSentAt = now;
            }

            if (dueForReminder.Count > 0)
            {
                await context.SaveChangesAsync(ct);
                _logger.LogInformation("Recordatorio de cita enviado a {Count} citas", dueForReminder.Count);
            }
        }

        private async Task SendExpiringNoticesAsync(
            DataContext context, AppointmentNotificationService notificationService, DateTime now, CancellationToken ct)
        {
            var dueForExpiringNotice = await context.Appointments
                .Where(a => a.Status == "Pendiente"
                    && a.ExpiringNoticeSentAt == null
                    && a.Date > now
                    && a.Date <= now.Add(ExpiringWindow))
                .Include(a => a.Customer)
                .Include(a => a.Workshop)
                .ToListAsync(ct);

            foreach (var appointment in dueForExpiringNotice)
            {
                await notificationService.NotifyExpiringAsync(appointment, appointment.Customer, appointment.Workshop);
                appointment.ExpiringNoticeSentAt = now;
            }

            if (dueForExpiringNotice.Count > 0)
            {
                await context.SaveChangesAsync(ct);
                _logger.LogInformation("Aviso de cita por vencer enviado a {Count} citas", dueForExpiringNotice.Count);
            }
        }

        private async Task CancelOverdueAsync(
            DataContext context, AppointmentNotificationService notificationService, DateTime now, CancellationToken ct)
        {
            var dueForCancellation = await context.Appointments
                .Where(a => a.Status == "Pendiente"
                    && a.CancelNoticeSentAt == null
                    && a.Date <= now.Add(CancelWindow))
                .Include(a => a.Customer)
                .Include(a => a.Workshop)
                .ToListAsync(ct);

            foreach (var appointment in dueForCancellation)
            {
                var isStaleBacklog = now - appointment.Date > StaleBacklogThreshold;

                appointment.Status = "Cancelada";
                appointment.CancelNoticeSentAt = now;

                if (!isStaleBacklog)
                {
                    await notificationService.NotifyCancellationAsync(appointment, appointment.Customer, appointment.Workshop);
                }
            }

            if (dueForCancellation.Count > 0)
            {
                await context.SaveChangesAsync(ct);
                _logger.LogInformation("{Count} citas canceladas automáticamente por falta de confirmación", dueForCancellation.Count);
            }
        }
    }
}
