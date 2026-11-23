using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Options;

namespace MiTaller.Services
{
    public class EmailSenderService : IEmailSender
    {
        private readonly EmailSettings _emailSettings;

        public EmailSenderService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            if (!IsAllowedRecipient(email))
            {
                Console.WriteLine($"Email to {email} skipped (EmailSettings:AllowedRecipients), subject {subject}");
                return;
            }
            try
            {
                using var smtpClient = new SmtpClient(_emailSettings.SmtpServer)
                {
                    Port = _emailSettings.SmtpPort,
                    Credentials = new NetworkCredential(_emailSettings.SmtpUser, _emailSettings.SmtpPass),
                    EnableSsl = _emailSettings.EnableSsl
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(
                        string.IsNullOrWhiteSpace(_emailSettings.FromAddress) ? _emailSettings.SmtpUser : _emailSettings.FromAddress,
                        _emailSettings.FromName),
                    Subject = subject,
                    Body = message,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(email);

                // Log before sending
                Console.WriteLine($"Sending email to {email} with subject {subject}");
                await smtpClient.SendMailAsync(mailMessage);
                Console.WriteLine("Email sent successfully");
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error sending email: {ex.Message}");
            }
        }

        private bool IsAllowedRecipient(string email)
        {
            var allowed = _emailSettings.AllowedRecipients
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (allowed.Length == 0) return true;
            return allowed.Any(a => a.StartsWith('@')
                ? email.EndsWith(a, StringComparison.OrdinalIgnoreCase)
                : string.Equals(email.Trim(), a, StringComparison.OrdinalIgnoreCase));
        }
    }
}
