using Microsoft.AspNetCore.Identity.UI.Services;
using MiTaller.Data;
using MiTaller.Models;
using MiTaller.Models.Auth;
using MiTaller.Models.Customer;
using MiTaller.Models.Notification;
using MiTaller.Models.Workshop;
using System.Net;

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

        public async Task NotifyAcceptRequestAsync(Appointment appointment, Customer customer, Workshop workshop)
        {
            var (formattedDate, formattedTime) = FormatDateTime(appointment.Date);
            var title = $"{workshop.WorkshopName} te ha agendado una cita para el {formattedDate} a las {formattedTime}";
            const string content = "Confirma tu asistencia desde la app para que el taller prepare tu cita.";

            _context.Notifications.Add(new Notifications
            {
                UserId = customer.Id,
                UserType = UserType.Customer,
                Title = title,
                Content = content,
                Event = "AppointmentCreated",
            });

            await SendEmailIfPossible(customer.Email, "Nueva cita agendada - confirma tu asistencia",
                BuildEmailBody("Tienes una nueva cita", title,
                    "Ábrela en la app y toca \"Confirmar cita\" para que el taller la prepare."));

            // TODO: Activar cuando se implementen las notificaciones push Firebase
            // await _firebaseNotificationService.SendNotificationToCustomerAsync(
            //     customer.Id, title, content, "AppointmentCreated",
            //     new Dictionary<string, string> { { "appointmentId", appointment.Id.ToString() } });
        }

        public async Task NotifyReminderAsync(Appointment appointment, Customer customer, Workshop workshop)
        {
            var (formattedDate, formattedTime) = FormatDateTime(appointment.Date);
            var title = $"Recordatorio: tu cita con {workshop.WorkshopName} es el {formattedDate} a las {formattedTime}";
            const string content = "Aún no la has confirmado. Si no la confirmas se cancelará automáticamente unas horas antes.";

            _context.Notifications.Add(new Notifications
            {
                UserId = customer.Id,
                UserType = UserType.Customer,
                Title = title,
                Content = content,
                Event = "AppointmentReminder",
            });

            await SendEmailIfPossible(customer.Email, "Recordatorio de tu cita - confírmala",
                BuildEmailBody("Tu cita es pronto", title,
                    "Confírmala desde la app; si no lo haces, se cancelará automáticamente unas horas antes."));

            // TODO: Activar cuando se implementen las notificaciones push Firebase
            // await _firebaseNotificationService.SendNotificationToCustomerAsync(
            //     customer.Id, title, content, "AppointmentReminder",
            //     new Dictionary<string, string> { { "appointmentId", appointment.Id.ToString() } });
        }

        public async Task NotifyExpiringAsync(Appointment appointment, Customer customer, Workshop workshop)
        {
            var (formattedDate, formattedTime) = FormatDateTime(appointment.Date);
            var title = $"Tu cita con {workshop.WorkshopName} del {formattedDate} a las {formattedTime} está por vencer";
            const string content = "Te quedan unas horas para confirmarla. Si no la confirmas se cancelará automáticamente.";

            _context.Notifications.Add(new Notifications
            {
                UserId = customer.Id,
                UserType = UserType.Customer,
                Title = title,
                Content = content,
                Event = "AppointmentExpiring",
            });

            await SendEmailIfPossible(customer.Email, "Tu cita está por vencer - confírmala",
                BuildEmailBody("Tu cita está por vencer", title,
                    "Confírmala desde la app antes de que se cancele automáticamente."));

            // TODO: Activar cuando se implementen las notificaciones push Firebase
            // await _firebaseNotificationService.SendNotificationToCustomerAsync(
            //     customer.Id, title, content, "AppointmentExpiring",
            //     new Dictionary<string, string> { { "appointmentId", appointment.Id.ToString() } });
        }

        public async Task NotifyCancellationAsync(Appointment appointment, Customer customer, Workshop workshop)
        {
            var (formattedDate, formattedTime) = FormatDateTime(appointment.Date);
            var title = $"Tu cita con {workshop.WorkshopName} del {formattedDate} a las {formattedTime} fue cancelada";
            var content = appointment.BookedBy == Models.Domain.RecordOrigin.Self
                ? "Se canceló automáticamente porque el taller no la confirmó a tiempo. Puedes pedir otra o elegir otro taller."
                : "Se canceló automáticamente porque no fue confirmada a tiempo. Agenda una nueva cuando quieras.";

            _context.Notifications.Add(new Notifications
            {
                UserId = customer.Id,
                UserType = UserType.Customer,
                Title = title,
                Content = content,
                Event = "AppointmentCanceled",
            });

            await SendEmailIfPossible(customer.Email, "Tu cita fue cancelada",
                BuildEmailBody("Cita cancelada", title,
                    "No fue confirmada a tiempo. Puedes agendar una nueva cuando gustes."));

            // TODO: Activar cuando se implementen las notificaciones push Firebase
            // await _firebaseNotificationService.SendNotificationToCustomerAsync(
            //     customer.Id, title, content, "AppointmentCanceled",
            //     new Dictionary<string, string> { { "appointmentId", appointment.Id.ToString() } });
        }

        // A customer's request waits on the workshop: its reminders go to the workshop's inbox instead
        public void NotifyWorkshopPending(Appointment appointment, Customer customer, string when)
        {
            var (formattedDate, formattedTime) = FormatDateTime(appointment.Date);
            AddWorkshopInbox(appointment,
                $"{customer.FullName} espera que confirmes su cita del {formattedDate} a las {formattedTime}",
                $"Confírmala {when}; si no, se cancelará automáticamente 2 horas antes.");
        }

        public void NotifyWorkshopCanceled(Appointment appointment, Customer customer)
        {
            var (formattedDate, formattedTime) = FormatDateTime(appointment.Date);
            var who = appointment.BookedBy == Models.Domain.RecordOrigin.Self ? "no la confirmaste" : "el cliente no la confirmó";
            AddWorkshopInbox(appointment,
                $"La cita de {customer.FullName} del {formattedDate} a las {formattedTime} se canceló",
                $"Se canceló automáticamente porque {who} a tiempo.");
        }

        public async Task NotifyStatusChangeAsync(Appointment appointment, bool byCustomer)
        {
            var customer = await _context.Customers.FindAsync(appointment.CustomerId);
            var workshop = await _context.Workshops.FindAsync(appointment.WorkshopId);
            if (customer == null || workshop == null) return;

            var (formattedDate, formattedTime) = FormatDateTime(appointment.Date);
            var verb = appointment.Status == "Confirmada" ? "confirmó" : "canceló";
            if (byCustomer)
            {
                AddWorkshopInbox(appointment, $"{customer.FullName} {verb} su cita del {formattedDate} a las {formattedTime}",
                    appointment.Title);
                return;
            }

            _context.Notifications.Add(new Notifications
            {
                UserId = customer.Id,
                UserType = UserType.Customer,
                Title = $"{workshop.WorkshopName} {verb} tu cita del {formattedDate} a las {formattedTime}",
                Content = appointment.Title,
                Event = appointment.Status == "Confirmada" ? "AppointmentConfirmed" : "AppointmentCanceled",
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

        private static (string date, string time) FormatDateTime(DateTime date) =>
            (date.ToString("dd/MM/yy"), date.ToString("HH:mm"));

        // Mismo esqueleto de plantilla inline que ya usa AuthController.SendVerificationEmail
        // - no hay un helper de templating compartido en el proyecto todavía.
        private static string BuildEmailBody(string heading, string message, string subMessage) => $@"
                <html>
                <body style=""font-family: Arial, sans-serif; margin: 0; padding: 0; background-color: #f4f4f4;"">
                    <div style=""max-width: 600px; margin: 0 auto; background-color: white;"">
                    <div style=""background-color: #f52222; padding: 20px 0; text-align: center;"">
                        <h1 style=""margin: 0; color: white;"">
                        <span style=""font-weight: bold;"">MiTaller</span>
                        </h1>
                    </div>
                    <div style=""padding: 30px; text-align: center; color: #333;"">
                        <h2 style=""margin-top: 0;"">{WebUtility.HtmlEncode(heading)}</h2>
                        <p style=""font-size: 16px;"">{WebUtility.HtmlEncode(message)}</p>
                        <p style=""margin-top: 20px; color: #555;"">{WebUtility.HtmlEncode(subMessage)}</p>
                    </div>
                    </div>
                </body>
                </html>
                ";
    }
}
