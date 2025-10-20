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
                // Get category tree (parent + subcategories)
                var categoriesQuery = new GetCategoryTreeQuery { ActiveOnly = true };
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
                    TempData["ErrorMessage"] = "กรุณาเข้าสู่ระบบ";
                    return RedirectToAction("Index", "Home");
                }

                command.CreatorId = userId;

                // Upload profile image if provided
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    try
                    {
                        var imagePath = await _imageService.UploadAsync(ImageFile, "communities");
                        command.ImageUrl = _imageService.GetImageUrl(imagePath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error uploading community profile image");
                        ModelState.AddModelError("ImageFile", "ไม่สามารถอัปโหลดรูปโปรไฟล์ได้");
                        return await ReloadCreateView(command);
                    }
                }

                // Upload cover image if provided
                if (CoverImageFile != null && CoverImageFile.Length > 0)
                {
                    try
                    {
                        var coverPath = await _imageService.UploadAsync(CoverImageFile, "communities/covers");
                        command.CoverImageUrl = _imageService.GetImageUrl(coverPath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error uploading community cover image");

                        // Delete profile image if already uploaded
                        if (!string.IsNullOrEmpty(command.ImageUrl))
                        {
                            await _imageService.DeleteAsync(command.ImageUrl);
                        }

                        ModelState.AddModelError("CoverImageFile", "ไม่สามารถอัปโหลดรูปปกได้");
                        return await ReloadCreateView(command);
                    }
                }

                // Validate model state
                if (!ModelState.IsValid)
                {
                    return await ReloadCreateView(command);
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

                    // Show errors
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }

                    return await ReloadCreateView(command);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating community");

                // Clean up uploaded images on error
                if (!string.IsNullOrEmpty(command.ImageUrl))
                {
                    await _imageService.DeleteAsync(command.ImageUrl);
                }
                if (!string.IsNullOrEmpty(command.CoverImageUrl))
                {
                    await _imageService.DeleteAsync(command.CoverImageUrl);
                }

                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการสร้างชุมชน";
                return await ReloadCreateView(command);
            }
        }

        // Helper method to reload view with categories
        private async Task<IActionResult> ReloadCreateView(CreateCommunityCommand command)
        {
            var categoriesQuery = new GetCategoryTreeQuery { ActiveOnly = true };
            var categories = await _mediator.Send(categoriesQuery);
            ViewBag.Categories = categories;
            return View(command);
        }
    }
}