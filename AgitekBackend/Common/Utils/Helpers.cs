using System.Globalization;
using System.Text.RegularExpressions;

namespace AgitekBackend.Common.Utils
{
    public static class Helpers
    {
        public static string GenerateSlug(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;

            text = new string(text.ToLowerInvariant().Replace('đ', 'd')
                .Normalize(System.Text.NormalizationForm.FormD)
                .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                .ToArray());

            return Regex.Replace(text, @"[^a-z0-9]+", "-").Trim('-');
        }
    }
}
