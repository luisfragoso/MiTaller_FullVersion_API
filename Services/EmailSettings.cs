namespace MiTaller.Services
{
    public class EmailSettings
    {
        public string SmtpServer { get; set; } = string.Empty;
        public int SmtpPort { get; set; }
        public string SmtpUser { get; set; } = string.Empty;
        public string SmtpPass { get; set; } = string.Empty;
        public bool EnableSsl { get; set; }
        // Visible sender; must be an alias of SmtpUser. Empty = SmtpUser.
        public string FromAddress { get; set; } = string.Empty;
        public string FromName { get; set; } = "MiTaller";
        // Staging guard: comma-separated addresses or @domains; non-empty = only these get mail, the rest is logged and skipped.
        public string AllowedRecipients { get; set; } = string.Empty;
    }
}
