using Microsoft.AspNetCore.Identity.UI.Services;
using MiTaller.Data;
using MiTaller.Models;
using MiTaller.Models.Auth;
using MiTaller.Models.Customer;
using MiTaller.Models.Notification;
using MiTaller.Models.Workshop;
using MiTaller.Services.Localization;

namespace MiTaller.Services
{
    /// <summary>
    /// Centraliza las 4 notificaciones del ciclo de vida de una cita: creada (debe
    /// aceptarla), recordatorio ~1 día antes, aviso de "por vencer" ~4h antes, y
    /// cancelación ~2h antes si sigue sin confirmar. Envía por in-app (tabla
    /// Notifications) + correo; el push por Firebase queda listo pero comentado hasta
    /// que existan las credenciales de servicio (ver FirebaseNotificationService).
    /// </summary>
    public class AppointmentNotificationService
    {
        private readonly DataContext _context;
        private readonly IEmailSender _emailSender;
        private readonly FirebaseNotificationService _firebaseNotificationService;

        public AppointmentNotificationService(
            DataContext context,
            IEmailSender emailSender,
            FirebaseNotificationService firebaseNotificationService)
        {
            _context = context;
            _emailSender = emailSender;
            _firebaseNotificationService = firebaseNotificationService;
        }

        public Task NotifyAcceptRequestAsync(Appointment appointment, Customer customer, Workshop workshop) =>
            NotifyCustomerAsync(appointment, customer, workshop, "appt.created", "AppointmentCreated");

        public Task NotifyReminderAsync(Appointment appointment, Customer customer, Workshop workshop) =>
            NotifyCustomerAsync(appointment, customer, workshop, "appt.reminder", "AppointmentReminder");

        public Task NotifyExpiringAsync(Appointment appointment, Customer customer, Workshop workshop) =>
            NotifyCustomerAsync(appointment, customer, workshop, "appt.expiring", "AppointmentExpiring");

        public Task NotifyCancellationAsync(Appointment appointment, Customer customer, Workshop workshop) =>
            NotifyCustomerAsync(appointment, customer, workshop, "appt.canceled", "AppointmentCanceled",
                appointment.BookedBy == Models.Domain.RecordOrigin.Self ? "appt.canceled.byShop" : "appt.canceled.byCustomer");

        // In-app notice + email for the customer, in the customer's language. Push stays off until Firebase credentials exist.
        private async Task NotifyCustomerAsync(Appointment appointment, Customer customer, Workshop workshop,
            string prefix, string eventName, string? contentKey = null)
        {
            var lang = Lang.Of(customer);
            var title = T.Get(lang, $"{prefix}.title", workshop.WorkshopName,
                appointment.Date.ShortDate(lang), appointment.Date.ShortTime(lang));

            _context.Notifications.Add(new Notifications
            {
                UserId = customer.Id,
                UserType = UserType.Customer,
                Title = title,
                Content = T.Get(lang, contentKey ?? $"{prefix}.content"),
                Event = eventName,
            });

            await SendEmailIfPossible(customer.Email, T.Get(lang, $"{prefix}.subject"),
                EmailLayout.Notice(lang, T.Get(lang, $"{prefix}.heading"), title, T.Get(lang, $"{prefix}.sub")));
        }

        // A customer's request waits on the workshop: its reminders go to the workshop's inbox instead
        public void NotifyWorkshopPending(Appointment appointment, Customer customer, bool today)
        {
            var lang = Lang.Of(appointment.Workshop);
            AddWorkshopInbox(appointment,
                T.Get(lang, "appt.pending.title", customer.FullName, appointment.Date.ShortDate(lang), appointment.Date.ShortTime(lang)),
                T.Get(lang, today ? "appt.pending.today" : "appt.pending.now"));
        }

        public void NotifyWorkshopCanceled(Appointment appointment, Customer customer)
        {
            var lang = Lang.Of(appointment.Workshop);
            AddWorkshopInbox(appointment,
                T.Get(lang, "appt.autoCanceled.title", customer.FullName, appointment.Date.ShortDate(lang), appointment.Date.ShortTime(lang)),
                T.Get(lang, appointment.BookedBy == Models.Domain.RecordOrigin.Self ? "appt.autoCanceled.bySelf" : "appt.autoCanceled.byCustomer"));
        }

        public async Task NotifyStatusChangeAsync(Appointment appointment, bool byCustomer)
        {
            var customer = await _context.Customers.FindAsync(appointment.CustomerId);
            var workshop = await _context.Workshops.FindAsync(appointment.WorkshopId);
            if (customer == null || workshop == null) return;

            var confirmed = appointment.Status == "Confirmada";
            if (byCustomer)
            {
                var shopLang = Lang.Of(workshop);
                AddWorkshopInbox(appointment, T.Get(shopLang, confirmed ? "appt.customerConfirmed" : "appt.customerCanceled",
                        customer.FullName, appointment.Date.ShortDate(shopLang), appointment.Date.ShortTime(shopLang)),
                    appointment.Title);
                return;
            }

            var lang = Lang.Of(customer);
            _context.Notifications.Add(new Notifications
            {
                UserId = customer.Id,
                UserType = UserType.Customer,
                Title = T.Get(lang, confirmed ? "appt.shopConfirmed" : "appt.shopCanceled",
                    workshop.WorkshopName, appointment.Date.ShortDate(lang), appointment.Date.ShortTime(lang)),
                Content = appointment.Title,
                Event = confirmed ? "AppointmentConfirmed" : "AppointmentCanceled",
            });
        }

        private void AddWorkshopInbox(Appointment appointment, string title, string details)
        {
            _context.WorkshopInbox.Add(new WorkshopInbox
            {
                WorkshopId = appointment.WorkshopId,
                CustomerId = appointment.CustomerId,
                VehicleId = appointment.VehicleId,
                ParentModelType = "Appointment",
                ParentModelId = appointment.Id,
                Title = title,
                Details = details,
            });
        }

        private async Task SendEmailIfPossible(string? email, string subject, string htmlBody)
        {
            if (string.IsNullOrWhiteSpace(email)) return;
            await _emailSender.SendEmailAsync(email, subject, htmlBody);
        }
    }
}
