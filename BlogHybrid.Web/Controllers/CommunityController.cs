using BlogHybrid.Application.Commands.Community;
using BlogHybrid.Application.Interfaces.Repositories;
using BlogHybrid.Application.Interfaces.Services;
using BlogHybrid.Application.Queries.Category;
using BlogHybrid.Application.Queries.Community;
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
        private readonly IUnitOfWork _unitOfWork;
        public CommunityController(
            IMediator mediator,
            IImageService imageService,
            ILogger<CommunityController> logger,
            IUnitOfWork unitOfWork)
        {
            _mediator = mediator;
            _imageService = imageService;
            _logger = logger;
            _unitOfWork = unitOfWork;
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
                    // แสดง Toast สวยงามก่อน redirect
                    TempData["ToastType"] = "success";
                    TempData["ToastMessage"] = "สร้างชุมชนสำเร็จ! กำลังนำคุณไปยังชุมชนของคุณ...";
                    TempData["ToastIcon"] = "bi-check-circle-fill";
                    TempData["RedirectUrl"] = $"/{result.FullSlug}";
                    TempData["RedirectDelay"] = 2000; // 2 วินาที

                    return View("CreateSuccess", result);
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
        private async Task<IActionResult> ReloadCreateView(CreateCommunityCommand command)
        {
            var categoriesQuery = new GetCategoryTreeQuery { ActiveOnly = true };
            var categories = await _mediator.Send(categoriesQuery);
            ViewBag.Categories = categories;
            return View(command);
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


        [Authorize]
        [HttpGet("my-communities")]
        public async Task<IActionResult> MyCommunities()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ErrorMessage"] = "กรุณาเข้าสู่ระบบ";
                    return RedirectToAction("Login", "Account");
                }

                // Get user's communities using the existing query
                var query = new GetUserCommunitiesQuery { UserId = userId };
                var communities = await _mediator.Send(query);

                return View(communities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading my communities page");
                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการโหลดหน้า";
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: /community/edit/{id}
        [Authorize]
        [HttpGet("community/edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ErrorMessage"] = "กรุณาเข้าสู่ระบบ";
                    return RedirectToAction("Login", "Account");
                }

                // Get community by ID
                var community = await _unitOfWork.Communities.GetByIdWithDetailsAsync(id);
                if (community == null)
                {
                    TempData["ErrorMessage"] = "ไม่พบชุมชนที่ต้องการแก้ไข";
                    return RedirectToAction("MyCommunities");
                }

                // Check permission (only creator can edit)
                if (community.CreatorId != userId)
                {
                    TempData["ErrorMessage"] = "คุณไม่มีสิทธิ์แก้ไขชุมชนนี้";
                    return RedirectToAction("MyCommunities");
                }

                // Get categories for dropdown
                var categoriesQuery = new GetCategoryTreeQuery { ActiveOnly = true };
                var categories = await _mediator.Send(categoriesQuery);
                ViewBag.Categories = categories;

                // Get selected category IDs
                var selectedCategoryIds = community.CommunityCategories
                    .Select(cc => cc.CategoryId)
                    .ToList();

                // Create command for view
                var command = new UpdateCommunityCommand
                {
                    Id = community.Id,
                    Name = community.Name,
                    Description = community.Description,
                    ImageUrl = community.ImageUrl,
                    CoverImageUrl = community.CoverImageUrl,
                    Rules = community.Rules,
                    CategoryIds = string.Join(",", selectedCategoryIds),
                    IsPrivate = community.IsPrivate,
                    RequireApproval = community.RequireApproval,
                    IsActive = community.IsActive
                };

                return View(command);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading edit community page for ID: {CommunityId}", id);
                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการโหลดหน้า";
                return RedirectToAction("MyCommunities");
            }
        }

        // POST: /community/edit/{id}
        [Authorize]
        [HttpPost("community/edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            UpdateCommunityCommand command,
            IFormFile? ImageFile,
            IFormFile? CoverImageFile)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ErrorMessage"] = "กรุณาเข้าสู่ระบบ";
                    return RedirectToAction("Login", "Account");
                }

                command.Id = id;
                command.CurrentUserId = userId;

                // Get existing community to check old images
                var existingCommunity = await _unitOfWork.Communities.GetByIdAsync(id);
                if (existingCommunity == null)
                {
                    TempData["ErrorMessage"] = "ไม่พบชุมชนที่ต้องการแก้ไข";
                    return RedirectToAction("MyCommunities");
                }

                var oldImageUrl = existingCommunity.ImageUrl;
                var oldCoverImageUrl = existingCommunity.CoverImageUrl;

                // Upload new profile image if provided
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    try
                    {
                        var imagePath = await _imageService.UploadAsync(ImageFile, "communities");
                        command.ImageUrl = _imageService.GetImageUrl(imagePath);

                        // Delete old image
                        if (!string.IsNullOrEmpty(oldImageUrl))
                        {
                            await _imageService.DeleteAsync(oldImageUrl);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error uploading community profile image");
                        ModelState.AddModelError("ImageFile", "ไม่สามารถอัปโหลดรูปโปรไฟล์ได้");
                        return await ReloadEditView(command);
                    }
                }
                else
                {
                    // Keep existing image
                    command.ImageUrl = oldImageUrl;
                }

                // Upload new cover image if provided
                if (CoverImageFile != null && CoverImageFile.Length > 0)
                {
                    try
                    {
                        var coverPath = await _imageService.UploadAsync(CoverImageFile, "communities/covers");
                        command.CoverImageUrl = _imageService.GetImageUrl(coverPath);

                        // Delete old cover image
                        if (!string.IsNullOrEmpty(oldCoverImageUrl))
                        {
                            await _imageService.DeleteAsync(oldCoverImageUrl);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error uploading community cover image");

                        // Delete new profile image if already uploaded
                        if (ImageFile != null && !string.IsNullOrEmpty(command.ImageUrl) && command.ImageUrl != oldImageUrl)
                        {
                            await _imageService.DeleteAsync(command.ImageUrl);
                        }

                        ModelState.AddModelError("CoverImageFile", "ไม่สามารถอัปโหลดรูปปกได้");
                        return await ReloadEditView(command);
                    }
                }
                else
                {
                    // Keep existing cover image
                    command.CoverImageUrl = oldCoverImageUrl;
                }

                // Validate model state
                if (!ModelState.IsValid)
                {
                    return await ReloadEditView(command);
                }

                // Send command to update community
                var result = await _mediator.Send(command);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = "อัปเดตชุมชนเรียบร้อยแล้ว";
                    return RedirectToAction("MyCommunities");
                }
                else
                {
                    // Show errors
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }

                    return await ReloadEditView(command);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating community");
                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการอัปเดตชุมชน";
                return await ReloadEditView(command);
            }
        }
        private async Task<IActionResult> ReloadEditView(UpdateCommunityCommand command)
        {
            var categoriesQuery = new GetCategoryTreeQuery { ActiveOnly = true };
            var categories = await _mediator.Send(categoriesQuery);
            ViewBag.Categories = categories;
            return View("Edit", command);
        }

    }
}