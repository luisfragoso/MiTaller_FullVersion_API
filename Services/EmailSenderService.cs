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
                    From = new MailAddress(_emailSettings.SmtpUser),
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
    }
}
