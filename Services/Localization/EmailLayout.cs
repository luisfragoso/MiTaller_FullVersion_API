using System.Net;

namespace MiTaller.Services.Localization
{
    /// One look for every email: red MiTaller header, body, and the confidentiality notice in the recipient's language.
    public static class EmailLayout
    {
        public static string E(string? s) => WebUtility.HtmlEncode(s ?? string.Empty);

        /// bodyHtml is trusted markup: encode every user-provided value with E() before passing it in.
        public static string Shell(string lang, string bodyHtml, bool tagline = false) => $@"<!DOCTYPE html>
<html lang=""{lang}"">
<head><meta charset=""UTF-8"" /><title>MiTaller</title></head>
<body style=""margin:0;font-family:Arial,sans-serif;background-color:#ffffff;"">
  <div style=""background-color:#f52222;padding:20px 0;text-align:center;"">
    <h1 style=""margin:0;color:white;""><span style=""font-weight:bold;"">MiTaller</span></h1>
  </div>
  <div style=""padding:30px 40px;color:#333;font-size:16px;"">{bodyHtml}{(tagline ? $"\n    <p>{T.Get(lang, "email.tagline")}</p>" : "")}
  </div>
  <div style=""padding:20px 30px;font-size:12px;color:#555;"">
    {T.Get(lang, "email.confidential")}
  </div>
</body>
</html>";

        /// Centered heading + message + secondary line (appointment notices).
        public static string Notice(string lang, string heading, string message, string subMessage) => Shell(lang, $@"
    <div style=""text-align:center;"">
      <h2 style=""margin-top:0;"">{E(heading)}</h2>
      <p style=""font-size:16px;"">{E(message)}</p>
      <p style=""margin-top:20px;color:#555;"">{E(subMessage)}</p>
    </div>");

        /// Verification / password reset code.
        public static string Code(string lang, string intro, string code) => Shell(lang, $@"
    <div style=""text-align:center;"">
      <h2 style=""margin-top:0;"">{T.Get(lang, "email.hi")}</h2>
      <p style=""font-size:18px;"">{E(intro)}</p>
      <p style=""font-size:32px;font-weight:bold;color:#f52222;"">{E(code)}</p>
      <p style=""margin-top:20px;"">{T.Get(lang, "email.codeExpires")}</p>
      <p style=""margin-top:30px;"">{T.Get(lang, "email.thanks")}</p>
    </div>");

        /// Welcome for accounts a workshop creates for someone else (customer or employee).
        public static string Welcome(string lang, string paragraphHtml, string nextStep) => Shell(lang, $@"
    <div style=""text-align:center;"">
      <h2 style=""margin-top:0;"">{T.Get(lang, "email.hi")}</h2>
      <p style=""font-size:18px;"">{paragraphHtml}</p>
      <p style=""font-size:18px;margin-top:20px;"">{E(nextStep)}</p>
      <p style=""margin-top:30px;"">{T.Get(lang, "email.thanks")}</p>
    </div>");
    }
}
