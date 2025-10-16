using BlogHybrid.Web.Models;

namespace BlogHybrid.Web.Services
{
    /// <summary>
    /// Service สำหรับสร้าง Sitemap.xml และ Robots.txt
    /// </summary>
    public interface ISitemapService
    {
        /// <summary>
        /// สร้าง Sitemap XML สำหรับ Google และ Search Engines
        /// </summary>
        Task<string> GenerateSitemapXmlAsync();

        /// <summary>
        /// สร้าง Sitemap Index (เมื่อมี URLs เยอะ)
        /// </summary>
        Task<string> GenerateSitemapIndexXmlAsync();

        /// <summary>
        /// ดึง Sitemap URLs ทั้งหมด
        /// </summary>
        Task<List<SitemapUrl>> GetAllUrlsAsync();

        /// <summary>
        /// ดึง Static Pages URLs
        /// </summary>
        List<SitemapUrl> GetStaticUrls();

        /// <summary>
        /// ดึง Posts URLs
        /// </summary>
        Task<List<SitemapUrl>> GetPostUrlsAsync();

        /// <summary>
        /// ดึง Categories URLs
        /// </summary>
        Task<List<SitemapUrl>> GetCategoryUrlsAsync();

        /// <summary>
        /// ดึง Communities URLs
        /// </summary>
        Task<List<SitemapUrl>> GetCommunityUrlsAsync();

        /// <summary>
        /// ดึง Tags URLs (optional)
        /// </summary>
        Task<List<SitemapUrl>> GetTagUrlsAsync();

        /// <summary>
        /// ดึง User Profiles URLs (optional)
        /// </summary>
        Task<List<SitemapUrl>> GetUserProfileUrlsAsync();

        /// <summary>
        /// สร้าง Robots.txt content
        /// </summary>
        string GenerateRobotsTxt();
    }
}


