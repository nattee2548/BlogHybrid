using BlogHybrid.Application.Commands.Community;
using BlogHybrid.Application.Queries.Community;
using BlogHybrid.Application.Queries.Category;
using BlogHybrid.Application.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BlogHybrid.Web.Controllers
{
    public class CommunityController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IImageService _imageService;
        private readonly ILogger<CommunityController> _logger;

        public CommunityController(
            IMediator mediator,
            IImageService imageService,
            ILogger<CommunityController> logger)
        {
            _mediator = mediator;
            _imageService = imageService;
            _logger = logger;
        }

        // GET: /create-community
        [Authorize]
        [HttpGet("create-community")]
        public async Task<IActionResult> Create()
        {
            try
            {
                // Get active categories for dropdown
                var categoriesQuery = new GetActiveCategoriesQuery();
                var categories = await _mediator.Send(categoriesQuery);

                ViewBag.Categories = categories;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create community page");
                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการโหลดหน้า";
                return RedirectToAction("Index", "Home");
            }
        }

        // POST: /create-community
        [Authorize]
        [HttpPost("create-community")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CreateCommunityCommand command,
            IFormFile? ImageFile,
            IFormFile? CoverImageFile)
        {
            try
            {
                // Get current user ID
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ErrorMessage"] = "กรุณาเข้าสู่ระบบก่อนสร้างชุมชน";
                    return RedirectToAction("Login", "Account");
                }

                command.CreatorId = userId;

                // Upload community image to Cloudflare R2
                if (ImageFile != null)
                {
                    try
                    {
                        var imagePath = await _imageService.UploadAsync(ImageFile, "communities");
                        command.ImageUrl = _imageService.GetImageUrl(imagePath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error uploading community image");
                        ModelState.AddModelError("ImageFile", "ไม่สามารถอัพโหลดรูปภาพได้");
                    }
                }

                // Upload cover image to Cloudflare R2
                if (CoverImageFile != null)
                {
                    try
                    {
                        var coverPath = await _imageService.UploadAsync(CoverImageFile, "communities/covers");
                        command.CoverImageUrl = _imageService.GetImageUrl(coverPath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error uploading cover image");
                        ModelState.AddModelError("CoverImageFile", "ไม่สามารถอัพโหลดรูปภาพปกได้");
                    }
                }

                // Check if there are any model errors
                if (!ModelState.IsValid)
                {
                    // Get categories again for dropdown
                    var categoriesQuery = new GetActiveCategoriesQuery();
                    var categories = await _mediator.Send(categoriesQuery);
                    ViewBag.Categories = categories;
                    return View(command);
                }

                // Send command to create community
                var result = await _mediator.Send(command);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;
                    // Redirect to community detail page: /{category-slug}/{community-slug}
                    return Redirect($"/{result.FullSlug}");
                }
                else
                {
                    // Delete uploaded images if community creation failed
                    if (!string.IsNullOrEmpty(command.ImageUrl))
                    {
                        await _imageService.DeleteAsync(command.ImageUrl);
                    }
                    if (!string.IsNullOrEmpty(command.CoverImageUrl))
                    {
                        await _imageService.DeleteAsync(command.CoverImageUrl);
                    }

                    // Get categories again for dropdown
                    var categoriesQuery = new GetActiveCategoriesQuery();
                    var categories = await _mediator.Send(categoriesQuery);
                    ViewBag.Categories = categories;

                    // Show errors
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }

                    return View(command);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating community");
                ModelState.AddModelError("", "เกิดข้อผิดพลาดในการสร้างชุมชน กรุณาลองใหม่อีกครั้ง");

                // Get categories again for dropdown
                var categoriesQuery = new GetActiveCategoriesQuery();
                var categories = await _mediator.Send(categoriesQuery);
                ViewBag.Categories = categories;

                return View(command);
            }
        }

        // GET: /{category-slug}/{community-slug}
        [AllowAnonymous]
        [HttpGet("{categorySlug}/{communitySlug}")]
        public async Task<IActionResult> Details(string categorySlug, string communitySlug)
        {
            try
            {
                // ใช้ community slug เพื่อดึงข้อมูล (slug ของ community เป็น unique)
                var query = new GetCommunityBySlugQuery
                {
                    Slug = communitySlug,
                    CurrentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                };

                var community = await _mediator.Send(query);

                if (community == null)
                {
                    return NotFound();
                }

                // ตรวจสอบว่า category slug ตรงกันไหม (เพื่อ SEO และ URL correctness)
                if (community.CategorySlug != categorySlug)
                {
                    // Redirect ไปยัง URL ที่ถูกต้อง
                    return RedirectPermanent($"/{community.CategorySlug}/{community.Slug}");
                }

                return View(community);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading community details for slug: {CommunitySlug}", communitySlug);
                return NotFound();
            }
        }
    }
}