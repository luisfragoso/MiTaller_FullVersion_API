using MiTaller.Models.Auth;

namespace MiTaller.Services.Localization
{
    /// Supported languages: "es" (default) and "en". Anything else falls back to Spanish.
    public static class Lang
    {
        public const string Es = "es";
        public const string En = "en";

        public static string Normalize(string? code) =>
            code != null && code.Trim().StartsWith(En, StringComparison.OrdinalIgnoreCase) ? En : Es;

        /// Language of the caller, sent by the apps in Accept-Language (first tag wins).
        public static string FromRequest(HttpRequest? request) =>
            Normalize(request?.Headers.AcceptLanguage.ToString().Split(',').FirstOrDefault());

        /// Language for messages sent to someone other than the caller (emails, push, inbox).
        public static string Of(BaseIdentityUser? user) => Normalize(user?.PreferredLanguage);
    }
}

namespace MiTaller.Services.Localization
{
    public static class LangExtensions
    {
        /// Preferred language of any account (customer, workshop, employee); Spanish when unknown.
        public static async Task<string> LanguageOfAsync(this MiTaller.Data.DataContext context, Guid userId) =>
            Lang.Normalize(await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(
                context.Users.Where(u => u.Id == userId).Select(u => u.PreferredLanguage)));

        /// Catalog name in the caller's language, falling back to the Spanish one.
        public static string Localized(this string name, string? nameEn, string lang) =>
            lang == Lang.En && !string.IsNullOrWhiteSpace(nameEn) ? nameEn : name;

        public static string ShortDate(this DateTime date, string lang) =>
            date.ToString(lang == Lang.En ? "MM/dd/yy" : "dd/MM/yy");

        public static string LongDate(this DateTime date, string lang) =>
            date.ToString(lang == Lang.En ? "MM/dd/yyyy" : "dd/MM/yyyy");

        public static string ShortTime(this DateTime date, string lang) =>
            date.ToString(lang == Lang.En ? "h:mm tt" : "HH:mm", System.Globalization.CultureInfo.InvariantCulture);
    }
}
