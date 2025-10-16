using BlogHybrid.Application.Interfaces.Repositories;
using BlogHybrid.Web.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Text;
using System.Xml;

namespace BlogHybrid.Web.Services
{
    /// <summary>
    /// Service สำหรับสร้าง Sitemap.xml และ Robots.txt
    /// </summary>
    public class SitemapService : ISitemapService
    {
       // private readonly IPostRepository _postRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ICommunityRepository _communityRepository;
        private readonly ITagRepository _tagRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMemoryCache _cache;
        private readonly ILogger<SitemapService> _logger;
        private readonly SitemapConfiguration _config;

        public SitemapService(
           // IPostRepository postRepository,
            ICategoryRepository categoryRepository,
            ICommunityRepository communityRepository,
            ITagRepository tagRepository,
            IUserRepository userRepository,
            IMemoryCache cache,
            ILogger<SitemapService> logger,
            IConfiguration configuration)
        {
           // _postRepository = postRepository;
            _categoryRepository = categoryRepository;
            _communityRepository = communityRepository;
            _tagRepository = tagRepository;
            _userRepository = userRepository;
            _cache = cache;
            _logger = logger;

            // โหลด configuration
            _config = new SitemapConfiguration
            {
                BaseUrl = configuration["SiteSettings:BaseUrl"] ?? "https://yourdomain.com",
                MaxUrlsPerSitemap = int.Parse(configuration["Sitemap:MaxUrlsPerSitemap"] ?? "50000"),
                CacheDurationMinutes = int.Parse(configuration["Sitemap:CacheDurationMinutes"] ?? "60"),
                IncludeUserProfiles = bool.Parse(configuration["Sitemap:IncludeUserProfiles"] ?? "false"),
                IncludeTags = bool.Parse(configuration["Sitemap:IncludeTags"] ?? "true"),
                OnlyPublishedPosts = bool.Parse(configuration["Sitemap:OnlyPublishedPosts"] ?? "true")
            };
        }

        /// <summary>
        /// สร้าง Sitemap XML
        /// </summary>
        public async Task<string> GenerateSitemapXmlAsync()
        {
            const string cacheKey = "sitemap_xml";

            // ลอง get จาก cache ก่อน
            if (_cache.TryGetValue(cacheKey, out string? cachedXml) && !string.IsNullOrEmpty(cachedXml))
            {
                _logger.LogInformation("Serving sitemap from cache");
                return cachedXml;
            }

            _logger.LogInformation("Generating new sitemap");

            var urls = await GetAllUrlsAsync();

            // Check if need sitemap index
            if (urls.Count > _config.MaxUrlsPerSitemap)
            {
                _logger.LogWarning("URL count ({Count}) exceeds max per sitemap. Consider using sitemap index.", urls.Count);
            }

            var xml = GenerateXmlFromUrls(urls);

            // Cache result
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(_config.CacheDurationMinutes));
            _cache.Set(cacheKey, xml, cacheOptions);

            return xml;
        }

        /// <summary>
        /// สร้าง Sitemap Index XML
        /// </summary>
        public async Task<string> GenerateSitemapIndexXmlAsync()
        {
            var urls = await GetAllUrlsAsync();

            // แบ่ง URLs เป็น chunks
            var chunks = urls.Chunk(_config.MaxUrlsPerSitemap).ToList();

            if (chunks.Count <= 1)
            {
                // ถ้ามี URLs น้อย ไม่ต้องใช้ index
                return await GenerateSitemapXmlAsync();
            }

            var sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sb.AppendLine("<sitemapindex xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");

            for (int i = 0; i < chunks.Count; i++)
            {
                sb.AppendLine("  <sitemap>");
                sb.AppendLine($"    <loc>{_config.BaseUrl}/sitemap-{i + 1}.xml</loc>");
                sb.AppendLine($"    <lastmod>{DateTime.UtcNow:yyyy-MM-dd}</lastmod>");
                sb.AppendLine("  </sitemap>");
            }

            sb.AppendLine("</sitemapindex>");
            return sb.ToString();
        }

        /// <summary>
        /// ดึง URLs ทั้งหมด
        /// </summary>
        public async Task<List<SitemapUrl>> GetAllUrlsAsync()
        {
            var urls = new List<SitemapUrl>();

            try
            {
                // 1. Static pages
                urls.AddRange(GetStaticUrls());

                // 2. Posts
                var postUrls = await GetPostUrlsAsync();
                urls.AddRange(postUrls);

                // 3. Categories
                var categoryUrls = await GetCategoryUrlsAsync();
                urls.AddRange(categoryUrls);

                // 4. Communities
                var communityUrls = await GetCommunityUrlsAsync();
                urls.AddRange(communityUrls);

                // 5. Tags (optional)
                if (_config.IncludeTags)
                {
                    var tagUrls = await GetTagUrlsAsync();
                    urls.AddRange(tagUrls);
                }

                // 6. User profiles (optional)
                if (_config.IncludeUserProfiles)
                {
                    var userUrls = await GetUserProfileUrlsAsync();
                    urls.AddRange(userUrls);
                }

                _logger.LogInformation("Generated {Count} URLs for sitemap", urls.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating sitemap URLs");
            }

            return urls;
        }

        /// <summary>
        /// Static Pages URLs
        /// </summary>
        public List<SitemapUrl> GetStaticUrls()
        {
            return new List<SitemapUrl>
            {
                new SitemapUrl
                {
                    Loc = _config.BaseUrl,
                    LastMod = DateTime.UtcNow,
                    ChangeFreq = ChangeFrequency.Daily,
                    Priority = 1.0
                },
                new SitemapUrl
                {
                    Loc = $"{_config.BaseUrl}/about",
                    LastMod = DateTime.UtcNow,
                    ChangeFreq = ChangeFrequency.Monthly,
                    Priority = 0.8
                },
                new SitemapUrl
                {
                    Loc = $"{_config.BaseUrl}/privacy",
                    LastMod = DateTime.UtcNow,
                    ChangeFreq = ChangeFrequency.Yearly,
                    Priority = 0.3
                },
                new SitemapUrl
                {
                    Loc = $"{_config.BaseUrl}/contact",
                    LastMod = DateTime.UtcNow,
                    ChangeFreq = ChangeFrequency.Monthly,
                    Priority = 0.5
                }
            };
        }

        /// <summary>
        /// Posts URLs
        /// </summary>
        public async Task<List<SitemapUrl>> GetPostUrlsAsync()
        {
            var urls = new List<SitemapUrl>();

            //try
            //{
            //    // ดึง posts ทั้งหมด (เฉพาะที่ published)
            //    var posts = await _postRepository.GetAllAsync(includeDeleted: false);

            //    if (_config.OnlyPublishedPosts)
            //    {
            //        posts = posts.Where(p => p.IsPublished).ToList();
            //    }

            //    foreach (var post in posts)
            //    {
            //        urls.Add(new SitemapUrl
            //        {
            //            Loc = $"{_config.BaseUrl}/post/{post.Slug}",
            //            LastMod = post.UpdatedAt,
            //            ChangeFreq = ChangeFrequency.Weekly,
            //            Priority = post.IsFeatured ? 0.9 : 0.7
            //        });
            //    }

            //    _logger.LogInformation("Added {Count} post URLs", urls.Count);
            //}
            //catch (Exception ex)
            //{
            //    _logger.LogError(ex, "Error getting post URLs");
            //}

            return urls;
        }

        /// <summary>
        /// Categories URLs
        /// </summary>
        public async Task<List<SitemapUrl>> GetCategoryUrlsAsync()
        {
            var urls = new List<SitemapUrl>();

            try
            {
                var categories = await _categoryRepository.GetActiveAsync();

                // Category Index
                urls.Add(new SitemapUrl
                {
                    Loc = $"{_config.BaseUrl}/category",
                    LastMod = DateTime.UtcNow,
                    ChangeFreq = ChangeFrequency.Weekly,
                    Priority = 0.8
                });

                // Individual categories
                foreach (var category in categories)
                {
                    urls.Add(new SitemapUrl
                    {
                        Loc = $"{_config.BaseUrl}/category/{category.Slug}",
                        //LastMod = category.UpdatedAt,
                        ChangeFreq = ChangeFrequency.Weekly,
                        Priority = 0.8
                    });
                }

                _logger.LogInformation("Added {Count} category URLs", urls.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category URLs");
            }

            return urls;
        }

        /// <summary>
        /// Communities URLs
        /// </summary>
        public async Task<List<SitemapUrl>> GetCommunityUrlsAsync()
        {
            var urls = new List<SitemapUrl>();

            try
            {
                var communities = await _communityRepository.GetAllAsync(includeDeleted: false);

                // ดึงเฉพาะที่ active และไม่ใช่ private
                communities = communities
                    .Where(c => c.IsActive && !c.IsPrivate)
                    .ToList();

                // Community Index
                urls.Add(new SitemapUrl
                {
                    Loc = $"{_config.BaseUrl}/community",
                    LastMod = DateTime.UtcNow,
                    ChangeFreq = ChangeFrequency.Daily,
                    Priority = 0.8
                });

                // Individual communities
                foreach (var community in communities)
                {
                    urls.Add(new SitemapUrl
                    {
                        Loc = $"{_config.BaseUrl}/community/{community.Slug}",
                        LastMod = community.UpdatedAt,
                        ChangeFreq = ChangeFrequency.Daily,
                        Priority = 0.7
                    });
                }

                _logger.LogInformation("Added {Count} community URLs", urls.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting community URLs");
            }

            return urls;
        }

        /// <summary>
        /// Tags URLs
        /// </summary>
        public async Task<List<SitemapUrl>> GetTagUrlsAsync()
        {
            var urls = new List<SitemapUrl>();

            try
            {
                var tags = await _tagRepository.GetAllAsync();

                foreach (var tag in tags)
                {
                    urls.Add(new SitemapUrl
                    {
                        Loc = $"{_config.BaseUrl}/tag/{tag.Slug}",
                        //LastMod = tag.UpdatedAt,
                        ChangeFreq = ChangeFrequency.Weekly,
                        Priority = 0.5
                    });
                }

                _logger.LogInformation("Added {Count} tag URLs", urls.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tag URLs");
            }

            return urls;
        }

        /// <summary>
        /// User Profiles URLs
        /// </summary>
        public async Task<List<SitemapUrl>> GetUserProfileUrlsAsync()
        {
            var urls = new List<SitemapUrl>();

            try
            {
                // ดึงเฉพาะ users ที่ active
                var users = await _userRepository.GetAllAsync();
                users = users.Where(u => u.IsActive).ToList();

                foreach (var user in users)
                {
                    urls.Add(new SitemapUrl
                    {
                        Loc = $"{_config.BaseUrl}/profile/{user.UserName}",
                        //LastMod = user.UpdatedAt,
                        ChangeFreq = ChangeFrequency.Monthly,
                        Priority = 0.4
                    });
                }

                _logger.LogInformation("Added {Count} user profile URLs", urls.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user profile URLs");
            }

            return urls;
        }

        /// <summary>
        /// สร้าง Robots.txt
        /// </summary>
        public string GenerateRobotsTxt()
        {
            var sb = new StringBuilder();
            sb.AppendLine("# Robots.txt for 404alk");
            sb.AppendLine();
            sb.AppendLine("User-agent: *");
            sb.AppendLine("Allow: /");
            sb.AppendLine();
            sb.AppendLine("# Disallow admin pages");
            sb.AppendLine("Disallow: /admin/");
            sb.AppendLine("Disallow: /account/");
            sb.AppendLine();
            sb.AppendLine("# Disallow API endpoints");
            sb.AppendLine("Disallow: /api/");
            sb.AppendLine();
            sb.AppendLine("# Disallow search results (to prevent duplicate content)");
            sb.AppendLine("Disallow: /search?");
            sb.AppendLine();
            sb.AppendLine("# Sitemap location");
            sb.AppendLine($"Sitemap: {_config.BaseUrl}/sitemap.xml");
            sb.AppendLine();
            sb.AppendLine("# Crawl-delay (optional - adjust as needed)");
            sb.AppendLine("# Crawl-delay: 10");

            return sb.ToString();
        }

        /// <summary>
        /// สร้าง XML จาก URLs
        /// </summary>
        private string GenerateXmlFromUrls(List<SitemapUrl> urls)
        {
            var settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                Encoding = Encoding.UTF8
            };

            using var stringWriter = new StringWriter();
            using var xmlWriter = XmlWriter.Create(stringWriter, settings);

            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");

            foreach (var url in urls)
            {
                xmlWriter.WriteStartElement("url");

                // loc (required)
                xmlWriter.WriteElementString("loc", url.Loc);

                // lastmod (optional)
                if (url.LastMod.HasValue)
                {
                    xmlWriter.WriteElementString("lastmod", url.LastMod.Value.ToString("yyyy-MM-dd"));
                }

                // changefreq (optional)
                if (url.ChangeFreq.HasValue)
                {
                    xmlWriter.WriteElementString("changefreq", url.ChangeFreq.Value.ToString().ToLower());
                }

                // priority (optional)
                if (url.Priority.HasValue)
                {
                    xmlWriter.WriteElementString("priority", url.Priority.Value.ToString("0.0"));
                }

                xmlWriter.WriteEndElement(); // url
            }

            xmlWriter.WriteEndElement(); // urlset
            xmlWriter.WriteEndDocument();
            xmlWriter.Flush();

            return stringWriter.ToString();
        }
    }
}


