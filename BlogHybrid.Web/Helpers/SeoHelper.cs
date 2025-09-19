// Helpers/SeoHelper.cs
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace BlogHybrid.Web.Helpers
{
    public static class SeoHelper
    {
        public static void SetBasicSeo(this Controller controller,
            string title,
            string description,
            string keywords = null,
            string canonicalUrl = null)
        {
            controller.ViewData["Title"] = title;
            controller.ViewData["MetaDescription"] = description;

            if (!string.IsNullOrEmpty(keywords))
                controller.ViewData["MetaKeywords"] = keywords;

            if (!string.IsNullOrEmpty(canonicalUrl))
                controller.ViewData["CanonicalUrl"] = canonicalUrl;
        }

        public static void SetOpenGraph(this Controller controller,
            string title,
            string description,
            string imageUrl = null,
            string type = "website")
        {
            controller.ViewData["OgTitle"] = title;
            controller.ViewData["OgDescription"] = description;
            controller.ViewData["OgType"] = type;

            if (!string.IsNullOrEmpty(imageUrl))
                controller.ViewData["OgImage"] = imageUrl;
        }

        public static void SetTwitterCard(this Controller controller,
            string title,
            string description,
            string imageUrl = null)
        {
            controller.ViewData["TwitterTitle"] = title;
            controller.ViewData["TwitterDescription"] = description;

            if (!string.IsNullOrEmpty(imageUrl))
                controller.ViewData["TwitterImage"] = imageUrl;
        }

        public static void SetPostSeo(this Controller controller,
            string title,
            string description,
            string imageUrl,
            string canonicalUrl,
            string[] tags = null,
            DateTime? publishedDate = null,
            DateTime? modifiedDate = null,
            string authorName = null)
        {
            // Basic SEO
            controller.SetBasicSeo(title, description, string.Join(", ", tags ?? new string[0]), canonicalUrl);

            // Open Graph for articles
            controller.SetOpenGraph(title, description, imageUrl, "article");
            controller.ViewData["OgPublishedTime"] = publishedDate?.ToString("yyyy-MM-ddTHH:mm:ssZ");
            controller.ViewData["OgModifiedTime"] = modifiedDate?.ToString("yyyy-MM-ddTHH:mm:ssZ");
            controller.ViewData["OgAuthor"] = authorName;

            // Twitter Card
            controller.SetTwitterCard(title, description, imageUrl);

            // Article specific
            controller.ViewData["ArticleTags"] = tags;
            controller.ViewData["PublishedDate"] = publishedDate;
            controller.ViewData["ModifiedDate"] = modifiedDate;
            controller.ViewData["AuthorName"] = authorName;
        }

        public static void SetCategorySeo(this Controller controller,
            string categoryName,
            string description,
            int totalPosts,
            string canonicalUrl)
        {
            var title = $"{categoryName} - บทความและความรู้ใน 404talk.com";
            var metaDescription = $"อ่านบทความเกี่ยวกับ {categoryName} ทั้งหมด {totalPosts} บทความ ใน 404talk.com {description}";

            controller.SetBasicSeo(title, metaDescription, categoryName, canonicalUrl);
            controller.SetOpenGraph(title, metaDescription);
            controller.SetTwitterCard(title, metaDescription);
        }

        public static string GenerateSlug(string title)
        {
            if (string.IsNullOrEmpty(title))
                return string.Empty;

            // Convert to lowercase
            var slug = title.ToLowerInvariant();

            // Replace Thai characters with English equivalents (optional)
            var thaiToEng = new Dictionary<string, string>
            {
                {"ก", "g"}, {"ข", "k"}, {"ค", "k"}, {"ง", "ng"},
                {"จ", "j"}, {"ฉ", "ch"}, {"ช", "ch"}, {"ซ", "s"},
                {"ญ", "y"}, {"ด", "d"}, {"ต", "t"}, {"ถ", "th"},
                {"ท", "th"}, {"ธ", "th"}, {"น", "n"}, {"บ", "b"},
                {"ป", "p"}, {"ผ", "ph"}, {"ฝ", "f"}, {"พ", "ph"},
                {"ฟ", "f"}, {"ภ", "ph"}, {"ม", "m"}, {"ย", "y"},
                {"ร", "r"}, {"ล", "l"}, {"ว", "w"}, {"ศ", "s"},
                {"ษ", "s"}, {"ส", "s"}, {"ห", "h"}, {"ฬ", "l"},
                {"อ", "o"}, {"ฮ", "h"},
                {"ะ", "a"}, {"า", "a"}, {"ิ", "i"}, {"ี", "i"},
                {"ึ", "ue"}, {"ื", "ue"}, {"ุ", "u"}, {"ู", "u"},
                {"เ", "e"}, {"แ", "ae"}, {"โ", "o"}, {"ใ", "ai"},
                {"ไ", "ai"}, {"ํ", "am"}, {"ั", "a"}, {"ิ", "i"},
                {" ", "-"}, {"_", "-"}
            };

            foreach (var pair in thaiToEng)
            {
                slug = slug.Replace(pair.Key, pair.Value);
            }

            // Remove special characters except hyphens
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\-ก-๏]", "");

            // Replace multiple hyphens with single hyphen
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-");

            // Remove leading/trailing hyphens
            slug = slug.Trim('-');

            return slug;
        }

        public static string TruncateDescription(string text, int maxLength = 160)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
                return text;

            var truncated = text.Substring(0, maxLength);
            var lastSpace = truncated.LastIndexOf(' ');

            if (lastSpace > 0)
                truncated = truncated.Substring(0, lastSpace);

            return truncated + "...";
        }

        public static string StripHtmlTags(string html)
        {
            if (string.IsNullOrEmpty(html))
                return string.Empty;

            return System.Text.RegularExpressions.Regex.Replace(html, @"<[^>]*>", "").Trim();
        }

        public static string GenerateBreadcrumbJsonLd(List<(string name, string url)> breadcrumbs, HttpContext context)
        {
            var items = breadcrumbs.Select((breadcrumb, index) => new
            {
                type = "ListItem",
                position = index + 1,
                name = breadcrumb.name,
                item = $"{context.Request.Scheme}://{context.Request.Host}{breadcrumb.url}"
            });

            var breadcrumbList = new
            {
                context = "https://schema.org",
                type = "BreadcrumbList",
                itemListElement = items
            };

            return System.Text.Json.JsonSerializer.Serialize(breadcrumbList, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });
        }

        public static string GenerateArticleJsonLd(
            string title,
            string description,
            string authorName,
            DateTime publishedDate,
            DateTime modifiedDate,
            string imageUrl,
            string articleUrl,
            HttpContext context)
        {
            var article = new
            {
                context = "https://schema.org",
                type = "Article",
                headline = title,
                description = description,
                author = new
                {
                    type = "Person",
                    name = authorName
                },
                publisher = new
                {
                    type = "Organization",
                    name = "404talk.com",
                    logo = new
                    {
                        type = "ImageObject",
                        url = $"{context.Request.Scheme}://{context.Request.Host}/images/logo.png"
                    }
                },
                datePublished = publishedDate.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                dateModified = modifiedDate.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                image = imageUrl,
                url = articleUrl,
                mainEntityOfPage = new
                {
                    type = "WebPage",
                    id = articleUrl
                }
            };

            return System.Text.Json.JsonSerializer.Serialize(article, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });
        }
    }
}

// Extensions/ControllerExtensions.cs
namespace BlogHybrid.Web.Extensions
{
    public static class ControllerExtensions
    {
        public static string GetCurrentUrl(this Controller controller)
        {
            var request = controller.HttpContext.Request;
            return $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}";
        }

        public static string GetBaseUrl(this Controller controller)
        {
            var request = controller.HttpContext.Request;
            return $"{request.Scheme}://{request.Host}";
        }
    }
}