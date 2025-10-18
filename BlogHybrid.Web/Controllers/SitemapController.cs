using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Xml.Linq;

namespace BlogHybrid.Web.Controllers
{
    public class SitemapController : Controller
    {
        private readonly ILogger<SitemapController> _logger;

        // TODO: Inject your services here
        // private readonly IPostService _postService;
        // private readonly ICategoryService _categoryService;

        public SitemapController(ILogger<SitemapController> logger)
        {
            _logger = logger;
        }

        [HttpGet("sitemap.xml")]
        [ResponseCache(Duration = 3600)] // Cache for 1 hour
        public async Task<IActionResult> Index()
        {
            try
            {
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var sitemap = new XElement("urlset",
                    new XAttribute("xmlns", "http://www.sitemaps.org/schemas/sitemap/0.9"),
                    new XAttribute(XNamespace.Xmlns + "image", "http://www.google.com/schemas/sitemap-image/1.1")
                );

                // Homepage
                sitemap.Add(CreateUrlElement(baseUrl, "", "1.0", "daily", DateTime.UtcNow));

                // Static pages
                sitemap.Add(CreateUrlElement(baseUrl, "/about", "0.8", "monthly", DateTime.UtcNow));
                sitemap.Add(CreateUrlElement(baseUrl, "/privacy", "0.5", "yearly", DateTime.UtcNow));

                // Categories
                // var categories = await _categoryService.GetAllActiveCategories();
                // foreach (var category in categories)
                // {
                //     sitemap.Add(CreateUrlElement(
                //         baseUrl, 
                //         $"/category/{category.Slug}", 
                //         "0.9", 
                //         "weekly", 
                //         category.UpdatedAt ?? category.CreatedAt
                //     ));
                // }

                // Posts
                // var posts = await _postService.GetAllPublishedPosts();
                // foreach (var post in posts)
                // {
                //     var postUrl = CreateUrlElement(
                //         baseUrl,
                //         $"/post/{post.Slug}",
                //         "0.7",
                //         "monthly",
                //         post.UpdatedAt ?? post.PublishedAt ?? post.CreatedAt
                //     );
                //     
                //     // Add image if exists
                //     if (!string.IsNullOrEmpty(post.ImageUrl))
                //     {
                //         postUrl.Add(new XElement(XNamespace.Get("http://www.google.com/schemas/sitemap-image/1.1") + "image",
                //             new XElement(XNamespace.Get("http://www.google.com/schemas/sitemap-image/1.1") + "loc", post.ImageUrl)
                //         ));
                //     }
                //     
                //     sitemap.Add(postUrl);
                // }

                var xml = new XDeclaration("1.0", "UTF-8", null);
                var document = new XDocument(xml, sitemap);

                return Content(document.ToString(), "application/xml", Encoding.UTF8);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating sitemap");
                return StatusCode(500);
            }
        }

        private XElement CreateUrlElement(string baseUrl, string path, string priority, string changefreq, DateTime lastmod)
        {
            return new XElement("url",
                new XElement("loc", $"{baseUrl}{path}"),
                new XElement("lastmod", lastmod.ToString("yyyy-MM-dd")),
                new XElement("changefreq", changefreq),
                new XElement("priority", priority)
            );
        }

        [HttpGet("robots.txt")]
        [ResponseCache(Duration = 86400)] // Cache for 24 hours
        public IActionResult Robots()
        {
            var sb = new StringBuilder();
            sb.AppendLine("User-agent: *");
            sb.AppendLine("Allow: /");
            sb.AppendLine("Disallow: /admin/");
            sb.AppendLine("Disallow: /account/");
            sb.AppendLine("Disallow: /api/");
            sb.AppendLine("");
            sb.AppendLine($"Sitemap: {Request.Scheme}://{Request.Host}/sitemap.xml");

            return Content(sb.ToString(), "text/plain", Encoding.UTF8);
        }
    }
}