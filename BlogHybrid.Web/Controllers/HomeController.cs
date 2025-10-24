// ============================================================
// อัปเดต HomeController.cs
// Location: BlogHybrid.Web/Controllers/HomeController.cs
// ============================================================

using BlogHybrid.Application.Queries.Post;
using BlogHybrid.Web.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BlogHybrid.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IMediator _mediator;

        public HomeController(
            ILogger<HomeController> logger,
            IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        // ============================================================
        // GET: /
        // ============================================================
        public async Task<IActionResult> Index(
            int page = 1,
            string? search = null,
            int? categoryId = null,
            int? communityId = null,
            string? tag = null,
            string? sort = "latest")
        {
            try
            {
                // Map sort parameter
                var (sortBy, sortDirection) = sort switch
                {
                    "popular" => ("ViewCount", "desc"),
                    "liked" => ("LikeCount", "desc"),
                    "oldest" => ("PublishedAt", "asc"),
                    _ => ("PublishedAt", "desc") // latest
                };

                // สร้าง Query
                var query = new GetPublicPostsQuery
                {
                    PageNumber = page,
                    PageSize = 12,
                    SearchTerm = search,
                    CategoryId = categoryId,
                    CommunityId = communityId,
                    Tag = tag,
                    SortBy = sortBy,
                    SortDirection = sortDirection,
                    FeaturedOnly = false
                };

                // ดึง Posts
                var result = await _mediator.Send(query);

                // สร้าง ViewModel
                var viewModel = new HomeViewModel
                {
                    Posts = result.Posts,
                    TotalCount = result.TotalCount,
                    PageNumber = result.PageNumber,
                    PageSize = result.PageSize,
                    TotalPages = result.TotalPages,
                    HasPreviousPage = result.HasPreviousPage,
                    HasNextPage = result.HasNextPage,
                    SearchTerm = search,
                    CategoryId = categoryId,
                    CommunityId = communityId,
                    Tag = tag,
                    Sort = sort ?? "latest"
                };

                // ดึง Featured Posts สำหรับแสดงด้านบน (ถ้าเป็นหน้าแรก)
                if (page == 1 && string.IsNullOrEmpty(search) && !categoryId.HasValue && !communityId.HasValue)
                {
                    var featuredQuery = new GetPublicPostsQuery
                    {
                        PageNumber = 1,
                        PageSize = 3,
                        FeaturedOnly = true,
                        SortBy = "PublishedAt",
                        SortDirection = "desc"
                    };
                    var featuredResult = await _mediator.Send(featuredQuery);
                    viewModel.FeaturedPosts = featuredResult.Posts;
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading home page");
                return View(new HomeViewModel());
            }
        }

        // ============================================================
        // GET: /Privacy
        // ============================================================
        public IActionResult Privacy()
        {
            return View();
        }

        // ============================================================
        // Error Handler
        // ============================================================
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}