namespace BlogHybrid.Web.Models
{
    /// <summary>
    /// Sitemap URL Entry
    /// </summary>
    public class SitemapUrl
    {
        /// <summary>
        /// URL ของหน้า (required)
        /// </summary>
        public string Loc { get; set; } = string.Empty;

        /// <summary>
        /// วันที่แก้ไขล่าสุด (optional)
        /// </summary>
        public DateTime? LastMod { get; set; }

        /// <summary>
        /// ความถี่ในการเปลี่ยนแปลง (optional)
        /// </summary>
        public ChangeFrequency? ChangeFreq { get; set; }

        /// <summary>
        /// ความสำคัญของหน้า 0.0 - 1.0 (optional)
        /// </summary>
        public double? Priority { get; set; }

        /// <summary>
        /// Alternative language URLs (optional)
        /// </summary>
        public List<SitemapAlternate>? Alternates { get; set; }
    }

    /// <summary>
    /// Alternative language URL
    /// </summary>
    public class SitemapAlternate
    {
        public string HrefLang { get; set; } = string.Empty;
        public string Href { get; set; } = string.Empty;
    }

    /// <summary>
    /// ความถี่ในการเปลี่ยนแปลงของหน้า
    /// </summary>
    public enum ChangeFrequency
    {
        Always,
        Hourly,
        Daily,
        Weekly,
        Monthly,
        Yearly,
        Never
    }

    /// <summary>
    /// Sitemap Index Entry (สำหรับเมื่อมี sitemap เยอะ)
    /// </summary>
    public class SitemapIndexEntry
    {
        public string Loc { get; set; } = string.Empty;
        public DateTime? LastMod { get; set; }
    }

    /// <summary>
    /// Sitemap Configuration
    /// </summary>
    public class SitemapConfiguration
    {
        /// <summary>
        /// Base URL ของเว็บไซต์
        /// </summary>
        public string BaseUrl { get; set; } = string.Empty;

        /// <summary>
        /// จำนวน URLs สูงสุดต่อ sitemap file (Google แนะนำไม่เกิน 50,000)
        /// </summary>
        public int MaxUrlsPerSitemap { get; set; } = 50000;

        /// <summary>
        /// Cache duration (minutes)
        /// </summary>
        public int CacheDurationMinutes { get; set; } = 60;

        /// <summary>
        /// รวม User Profiles ใน sitemap หรือไม่
        /// </summary>
        public bool IncludeUserProfiles { get; set; } = false;

        /// <summary>
        /// รวม Tags ใน sitemap หรือไม่
        /// </summary>
        public bool IncludeTags { get; set; } = true;

        /// <summary>
        /// รวมเฉพาะ Posts ที่ Published
        /// </summary>
        public bool OnlyPublishedPosts { get; set; } = true;
    }
}


