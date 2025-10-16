using System.Text;
using System.Text.Json;

namespace BlogHybrid.Web.Helpers
{
    /// <summary>
    /// Helper Class สำหรับจัดการ SEO Meta Tags และ Structured Data
    /// </summary>
    public static class SeoHelper
    {
        /// <summary>
        /// สร้าง Basic SEO Metadata
        /// </summary>
        public static SeoMetadata CreateBasicSeo(
            string title,
            string description,
            string? keywords = null,
            string? ogImage = null)
        {
            return new SeoMetadata
            {
                Title = title,
                Description = TruncateDescription(description),
                Keywords = keywords ?? "blog, article, community, technology, 404alk",
                OgImage = ogImage ?? "/images/default-og-image.jpg",
                OgType = "website"
            };
        }

        /// <summary>
        /// สร้าง Article SEO Metadata (สำหรับบทความ)
        /// </summary>
        public static SeoMetadata CreateArticleSeo(
            string title,
            string description,
            string authorName,
            DateTime publishedDate,
            DateTime? modifiedDate = null,
            string? featuredImage = null,
            List<string>? tags = null,
            string? category = null)
        {
            return new SeoMetadata
            {
                Title = $"{title} | 404alk",
                Description = TruncateDescription(description),
                Author = authorName,
                OgType = "article",
                OgImage = featuredImage ?? "/images/default-article-og.jpg",
                PublishedTime = publishedDate,
                ModifiedTime = modifiedDate ?? publishedDate,
                ArticleTags = tags,
                ArticleSection = category,
                TwitterCard = "summary_large_image"
            };
        }

        /// <summary>
        /// สร้าง Profile SEO Metadata (สำหรับโปรไฟล์ผู้ใช้)
        /// </summary>
        public static SeoMetadata CreateProfileSeo(
            string userName,
            string bio,
            string? profileImage = null)
        {
            return new SeoMetadata
            {
                Title = $"{userName} - โปรไฟล์ | 404alk",
                Description = TruncateDescription(bio),
                OgType = "profile",
                OgImage = profileImage ?? "/images/default-profile-og.jpg",
                Author = userName
            };
        }

        /// <summary>
        /// Truncate description ให้เหมาะสมกับ Google Search (150-160 ตัวอักษร)
        /// </summary>
        private static string TruncateDescription(string description, int maxLength = 155)
        {
            if (string.IsNullOrWhiteSpace(description))
                return string.Empty;

            if (description.Length <= maxLength)
                return description;

            // ตัดที่คำสุดท้าย ไม่ตัดกลางคำ
            var truncated = description.Substring(0, maxLength);
            var lastSpace = truncated.LastIndexOf(' ');

            if (lastSpace > 0)
                truncated = truncated.Substring(0, lastSpace);

            return truncated + "...";
        }

        /// <summary>
        /// สร้าง Structured Data JSON-LD สำหรับ Article
        /// </summary>
        public static string GenerateArticleStructuredData(
            string title,
            string description,
            string authorName,
            DateTime publishedDate,
            DateTime modifiedDate,
            string articleUrl,
            string? imageUrl = null,
            List<string>? keywords = null)
        {
            var structuredData = new
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
                    name = "404alk",
                    logo = new
                    {
                        type = "ImageObject",
                        url = "https://yourdomain.com/logo.png"
                    }
                },
                datePublished = publishedDate.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                dateModified = modifiedDate.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                image = imageUrl ?? "https://yourdomain.com/images/default-article.jpg",
                url = articleUrl,
                keywords = keywords != null ? string.Join(", ", keywords) : string.Empty
            };

            return JsonSerializer.Serialize(structuredData, new JsonSerializerOptions
            {
                WriteIndented = false
            });
        }

        /// <summary>
        /// สร้าง Structured Data JSON-LD สำหรับ Website
        /// </summary>
        public static string GenerateWebsiteStructuredData(
            string siteName,
            string siteUrl,
            string description)
        {
            var structuredData = new
            {
                context = "https://schema.org",
                type = "WebSite",
                name = siteName,
                url = siteUrl,
                description = description,
                potentialAction = new
                {
                    type = "SearchAction",
                    target = new
                    {
                        type = "EntryPoint",
                        urlTemplate = $"{siteUrl}/search?q={{search_term_string}}"
                    },
                    queryInput = "required name=search_term_string"
                }
            };

            return JsonSerializer.Serialize(structuredData, new JsonSerializerOptions
            {
                WriteIndented = false
            });
        }

        /// <summary>
        /// สร้าง Structured Data JSON-LD สำหรับ BreadcrumbList
        /// </summary>
        public static string GenerateBreadcrumbStructuredData(
            List<(string Name, string Url)> breadcrumbs)
        {
            var items = breadcrumbs.Select((item, index) => new
            {
                type = "ListItem",
                position = index + 1,
                name = item.Name,
                item = item.Url
            });

            var structuredData = new
            {
                context = "https://schema.org",
                type = "BreadcrumbList",
                itemListElement = items
            };

            return JsonSerializer.Serialize(structuredData, new JsonSerializerOptions
            {
                WriteIndented = false
            });
        }

        /// <summary>
        /// สร้าง Structured Data JSON-LD สำหรับ Organization
        /// </summary>
        public static string GenerateOrganizationStructuredData(
            string name,
            string url,
            string logo,
            string description,
            string? email = null,
            string? phone = null,
            Dictionary<string, string>? socialLinks = null)
        {
            var organization = new Dictionary<string, object>
            {
                ["@context"] = "https://schema.org",
                ["@type"] = "Organization",
                ["name"] = name,
                ["url"] = url,
                ["logo"] = logo,
                ["description"] = description
            };

            if (!string.IsNullOrEmpty(email))
                organization["email"] = email;

            if (!string.IsNullOrEmpty(phone))
                organization["telephone"] = phone;

            if (socialLinks != null && socialLinks.Any())
                organization["sameAs"] = socialLinks.Values.ToList();

            return JsonSerializer.Serialize(organization, new JsonSerializerOptions
            {
                WriteIndented = false
            });
        }

        /// <summary>
        /// สร้าง Full URL จาก Relative Path
        /// </summary>
        public static string GetFullUrl(string baseUrl, string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                return baseUrl;

            if (relativePath.StartsWith("http://") || relativePath.StartsWith("https://"))
                return relativePath;

            return $"{baseUrl.TrimEnd('/')}/{relativePath.TrimStart('/')}";
        }

        /// <summary>
        /// Validate URL Format
        /// </summary>
        public static bool IsValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }
    }
}


