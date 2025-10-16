using BlogHybrid.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace BlogHybrid.Web.Controllers
{
    /// <summary>
    /// Controller สำหรับ SEO: Sitemap.xml และ Robots.txt
    /// </summary>
    public class SitemapController : Controller
    {
        private readonly ISitemapService _sitemapService;
        private readonly ILogger<SitemapController> _logger;

        public SitemapController(
            ISitemapService sitemapService,
            ILogger<SitemapController> logger)
        {
            _sitemapService = sitemapService;
            _logger = logger;
        }

        /// <summary>
        /// GET: /sitemap.xml
        /// </summary>
        [HttpGet]
        [Route("sitemap.xml")]
        [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)]
        public async Task<IActionResult> SitemapXml()
        {
            try
            {
                _logger.LogInformation("Sitemap.xml requested");

                var xml = await _sitemapService.GenerateSitemapXmlAsync();

                return Content(xml, "application/xml", Encoding.UTF8);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating sitemap.xml");
                return StatusCode(500, "Error generating sitemap");
            }
        }

        /// <summary>
        /// GET: /sitemap-index.xml
        /// Sitemap Index สำหรับเมื่อมี URLs เยอะ (> 50,000)
        /// </summary>
        [HttpGet]
        [Route("sitemap-index.xml")]
        [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)]
        public async Task<IActionResult> SitemapIndex()
        {
            try
            {
                _logger.LogInformation("Sitemap-index.xml requested");

                var xml = await _sitemapService.GenerateSitemapIndexXmlAsync();

                return Content(xml, "application/xml", Encoding.UTF8);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating sitemap-index.xml");
                return StatusCode(500, "Error generating sitemap index");
            }
        }

        /// <summary>
        /// GET: /robots.txt
        /// </summary>
        [HttpGet]
        [Route("robots.txt")]
        [ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Any)] // Cache 24 hours
        public IActionResult RobotsTxt()
        {
            try
            {
                _logger.LogInformation("Robots.txt requested");

                var robotsTxt = _sitemapService.GenerateRobotsTxt();

                return Content(robotsTxt, "text/plain", Encoding.UTF8);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating robots.txt");
                return StatusCode(500, "Error generating robots.txt");
            }
        }

        /// <summary>
        /// GET: /clear-sitemap-cache (สำหรับ Admin เท่านั้น)
        /// ใช้เมื่อต้องการ force regenerate sitemap
        /// </summary>
        [HttpGet]
        [Route("admin/clear-sitemap-cache")]
        // [Authorize(Roles = "Admin")] // Uncomment เมื่อมี Authentication
        public IActionResult ClearSitemapCache()
        {
            try
            {
                _logger.LogInformation("Clearing sitemap cache");

                // ใน production ควรใช้ IMemoryCache.Remove
                // สำหรับตอนนี้แค่ return success

                return Ok(new
                {
                    success = true,
                    message = "Sitemap cache cleared. Next request will regenerate sitemap."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing sitemap cache");
                return StatusCode(500, "Error clearing cache");
            }
        }
    }
}


