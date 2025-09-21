using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BlogHybrid.Application.Common
{
    public static class SlugGenerator
    {
        private static readonly Dictionary<string, string> ThaiToEnglish = new()
        {
            // Thai consonants
            {"ก", "g"}, {"ข", "k"}, {"ค", "k"}, {"ง", "ng"},
            {"จ", "j"}, {"ฉ", "ch"}, {"ช", "ch"}, {"ซ", "s"},
            {"ญ", "y"}, {"ด", "d"}, {"ต", "t"}, {"ถ", "th"},
            {"ท", "th"}, {"ธ", "th"}, {"น", "n"}, {"บ", "b"},
            {"ป", "p"}, {"ผ", "ph"}, {"ฝ", "f"}, {"พ", "ph"},
            {"ฟ", "f"}, {"ภ", "ph"}, {"ม", "m"}, {"ย", "y"},
            {"ร", "r"}, {"ล", "l"}, {"ว", "w"}, {"ศ", "s"},
            {"ษ", "s"}, {"ส", "s"}, {"ห", "h"}, {"ฬ", "l"},
            {"อ", "o"}, {"ฮ", "h"},
            
            // Thai vowels
            {"ะ", "a"}, {"า", "a"}, {"ิ", "i"}, {"ี", "i"},
            {"ึ", "ue"}, {"ื", "ue"}, {"ุ", "u"}, {"ู", "u"},
            {"เ", "e"}, {"แ", "ae"}, {"โ", "o"}, {"ใ", "ai"},
            {"ไ", "ai"}, {"ํ", "am"}, {"ั", "a"}, {"ำ", "am"},
            
            // Common Thai words
            {"การ", "gan"}, {"และ", "lae"}, {"หรือ", "rue"},
            {"ใน", "nai"}, {"ของ", "kong"}, {"ที่", "thi"},
            {"เป็น", "pen"}, {"มี", "mi"}, {"ไม่", "mai"},
            {"จะ", "ja"}, {"ได้", "dai"}, {"แล้ว", "laew"}
        };

        public static string GenerateSlug(string input, int maxLength = 50)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Convert to lowercase
            var slug = input.Trim().ToLowerInvariant();

            // Replace Thai characters
            foreach (var pair in ThaiToEnglish)
            {
                slug = slug.Replace(pair.Key, pair.Value);
            }

            // Remove diacritics (accented characters)
            slug = RemoveDiacritics(slug);

            // Replace spaces and special characters with hyphens
            slug = Regex.Replace(slug, @"[^a-z0-9\u0E00-\u0E7F]+", "-");

            // Remove multiple consecutive hyphens
            slug = Regex.Replace(slug, @"-+", "-");

            // Remove leading and trailing hyphens
            slug = slug.Trim('-');

            // Limit length
            if (slug.Length > maxLength)
            {
                slug = slug.Substring(0, maxLength).TrimEnd('-');
            }

            return slug;
        }

        public static async Task<string> GenerateUniqueSlug(
            string input,
            Func<string, int?, Task<bool>> checkSlugExists,
            int? excludeId = null,
            int maxLength = 50)
        {
            var baseSlug = GenerateSlug(input, maxLength - 10); // Reserve space for numbers
            var slug = baseSlug;
            var counter = 1;

            while (await checkSlugExists(slug, excludeId))
            {
                slug = $"{baseSlug}-{counter}";
                counter++;

                // Prevent infinite loop
                if (counter > 999)
                {
                    slug = $"{baseSlug}-{Guid.NewGuid().ToString("N")[..8]}";
                    break;
                }
            }

            return slug;
        }

        private static string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        public static string GenerateFromTitle(string title)
        {
            // For blog post titles, we might want different logic
            return GenerateSlug(title, 80);
        }

        public static string GenerateFromName(string name)
        {
            // For category names, shorter slugs
            return GenerateSlug(name, 50);
        }
    }
}
