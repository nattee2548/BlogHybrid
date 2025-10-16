namespace BlogHybrid.Web.Helpers
{
    /// <summary>
    /// Model สำหรับเก็บข้อมูล SEO Meta Tags
    /// </summary>
    public class SeoMetadata
    {
        /// <summary>
        /// Title หน้าเว็บ (แสดงใน Browser Tab และ Google Search)
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Description สั้นๆ เกี่ยวกับหน้านี้ (แสดงใน Google Search Results)
        /// ควรยาวประมาณ 150-160 ตัวอักษร
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Keywords สำหรับ SEO (คั่นด้วย comma)
        /// เช่น "blog, programming, asp.net core, thai developer"
        /// </summary>
        public string? Keywords { get; set; }

        /// <summary>
        /// Canonical URL - URL ที่ถูกต้องของหน้านี้
        /// ใช้เพื่อป้องกัน Duplicate Content
        /// </summary>
        public string? CanonicalUrl { get; set; }

        /// <summary>
        /// รูปภาพสำหรับแชร์บน Social Media (Open Graph)
        /// ขนาดแนะนำ: 1200x630 pixels
        /// </summary>
        public string? OgImage { get; set; }

        /// <summary>
        /// ประเภทของ Content สำหรับ Open Graph
        /// เช่น "website", "article", "profile"
        /// </summary>
        public string OgType { get; set; } = "website";

        /// <summary>
        /// Site Name สำหรับ Open Graph
        /// </summary>
        public string SiteName { get; set; } = "404alk";

        /// <summary>
        /// Twitter Card Type
        /// เช่น "summary", "summary_large_image"
        /// </summary>
        public string TwitterCard { get; set; } = "summary_large_image";

        /// <summary>
        /// Twitter Username ของเว็บไซต์ (ถ้ามี)
        /// เช่น "@404alk"
        /// </summary>
        public string? TwitterSite { get; set; }

        /// <summary>
        /// Author ของหน้านี้
        /// </summary>
        public string? Author { get; set; }

        /// <summary>
        /// วันที่เผยแพร่ (สำหรับบทความ)
        /// </summary>
        public DateTime? PublishedTime { get; set; }

        /// <summary>
        /// วันที่แก้ไขล่าสุด (สำหรับบทความ)
        /// </summary>
        public DateTime? ModifiedTime { get; set; }

        /// <summary>
        /// Tags ของบทความ (สำหรับ article:tag)
        /// </summary>
        public List<string>? ArticleTags { get; set; }

        /// <summary>
        /// หมวดหมู่ของบทความ
        /// </summary>
        public string? ArticleSection { get; set; }

        /// <summary>
        /// ข้อมูล Structured Data (JSON-LD)
        /// </summary>
        public string? StructuredData { get; set; }

        /// <summary>
        /// Robots Meta Tag
        /// เช่น "index, follow" หรือ "noindex, nofollow"
        /// </summary>
        public string Robots { get; set; } = "index, follow";

        /// <summary>
        /// Language/Locale
        /// เช่น "th_TH", "en_US"
        /// </summary>
        public string Locale { get; set; } = "th_TH";

        /// <summary>
        /// Alternative Languages
        /// </summary>
        public Dictionary<string, string>? AlternateLanguages { get; set; }
    }
}


