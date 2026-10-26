using System;

namespace MiTaller.Helpers
{
    public static class FolioGenerator
    {
        // Same shape as the legacy Auto/Moto folios: yyyyMMddHHmmss + 3 random letters + format initial
        public static string New(string? vehicleFormat)
        {
            var letters = new char[3];
            for (int i = 0; i < letters.Length; i++)
            {
                letters[i] = (char)Random.Shared.Next('A', 'Z' + 1);
            }

            var initial = string.IsNullOrWhiteSpace(vehicleFormat)
                ? 'X'
                : char.ToUpperInvariant(vehicleFormat.Trim()[0]);

            return DateTime.Now.ToString("yyyyMMddHHmmss") + new string(letters) + initial;
        }
    }
}
