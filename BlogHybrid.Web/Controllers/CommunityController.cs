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
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ErrorMessage"] = "กรุณาเข้าสู่ระบบ";
                    return RedirectToAction("Index", "Home");
                }

                command.CreatorId = userId;

                if (!ModelState.IsValid)
                {
                    var categoriesQuery = new GetCategoryTreeQuery { ActiveOnly = true };
                    var categories = await _mediator.Send(categoriesQuery);
                    ViewBag.Categories = categories;
                    ViewBag.SelectedCategoryIds = command.CategoryIds;
                    return View(command);
                }

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

                        var categoriesQuery = new GetCategoryTreeQuery { ActiveOnly = true };
                        var categories = await _mediator.Send(categoriesQuery);
                        ViewBag.Categories = categories;
                        ViewBag.SelectedCategoryIds = command.CategoryIds;
                        return View(command);
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
                        if (!string.IsNullOrEmpty(command.ImageUrl))
                        {
                            await _imageService.DeleteAsync(command.ImageUrl);
                        }
                        ModelState.AddModelError("CoverImageFile", "ไม่สามารถอัปโหลดรูปปกได้");

                        var categoriesQuery = new GetCategoryTreeQuery { ActiveOnly = true };
                        var categories = await _mediator.Send(categoriesQuery);
                        ViewBag.Categories = categories;
                        ViewBag.SelectedCategoryIds = command.CategoryIds;
                        return View(command);
                    }
                }

                var result = await _mediator.Send(command);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = "สร้างชุมชนสำเร็จ!";

                    // ✅ แก้ไข: เปลี่ยนเป็น /community/{slug}
                    return RedirectToAction("Details", new { communitySlug = result.Slug });
                }
                else
                {
                    if (!string.IsNullOrEmpty(command.ImageUrl))
                    {
                        await _imageService.DeleteAsync(command.ImageUrl);
                    }
                    if (!string.IsNullOrEmpty(command.CoverImageUrl))
                    {
                        await _imageService.DeleteAsync(command.CoverImageUrl);
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }

                    var categoriesQuery = new GetCategoryTreeQuery { ActiveOnly = true };
                    var categories = await _mediator.Send(categoriesQuery);
                    ViewBag.Categories = categories;
                    ViewBag.SelectedCategoryIds = command.CategoryIds;
                    return View(command);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating community");

                if (!string.IsNullOrEmpty(command.ImageUrl))
                {
                    await _imageService.DeleteAsync(command.ImageUrl);
                }
                if (!string.IsNullOrEmpty(command.CoverImageUrl))
                {
                    await _imageService.DeleteAsync(command.CoverImageUrl);
                }

                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการสร้างชุมชน: " + ex.Message;

                var categoriesQuery = new GetCategoryTreeQuery { ActiveOnly = true };
                var categories = await _mediator.Send(categoriesQuery);
                ViewBag.Categories = categories;
                ViewBag.SelectedCategoryIds = command.CategoryIds;
                return View(command);
            }
        }

        // ✅ แก้ไข: เปลี่ยนเป็น /community/{communitySlug}
        [AllowAnonymous]
        [HttpGet("community/{communitySlug}")]
        public async Task<IActionResult> Details(string communitySlug)
        {
            try
            {
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

                var community = await _unitOfWork.Communities.GetByIdWithDetailsAsync(id);
                if (community == null)
                {
                    TempData["ErrorMessage"] = "ไม่พบชุมชนที่ต้องการแก้ไข";
                    return RedirectToAction("MyCommunities");
                }

                if (community.CreatorId != userId)
                {
                    TempData["ErrorMessage"] = "คุณไม่มีสิทธิ์แก้ไขชุมชนนี้";
                    return RedirectToAction("MyCommunities");
                }

                var categoriesQuery = new GetCategoryTreeQuery { ActiveOnly = true };
                var categories = await _mediator.Send(categoriesQuery);
                ViewBag.Categories = categories;

                var selectedCategoryIds = community.CommunityCategories
                    .Select(cc => cc.CategoryId)
                    .ToList();

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
                    IsActive = community.IsActive,
                    IsNSFW = community.IsNSFW
                };

                ViewBag.SelectedCategoryIds = command.CategoryIds;
                return View(command);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading edit community page for ID: {CommunityId}", id);
                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการโหลดหน้า";
                return RedirectToAction("MyCommunities");
            }
        }

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

                var existingCommunity = await _unitOfWork.Communities.GetByIdAsync(id);
                if (existingCommunity == null)
                {
                    TempData["ErrorMessage"] = "ไม่พบชุมชนที่ต้องการแก้ไข";
                    return RedirectToAction("MyCommunities");
                }

                if (existingCommunity.CreatorId != userId)
                {
                    TempData["ErrorMessage"] = "คุณไม่มีสิทธิ์แก้ไขชุมชนนี้";
                    return RedirectToAction("MyCommunities");
                }

                var oldImageUrl = existingCommunity.ImageUrl;
                var oldCoverImageUrl = existingCommunity.CoverImageUrl;

                if (!ModelState.IsValid)
                {
                    var categoriesQuery = new GetCategoryTreeQuery { ActiveOnly = true };
                    var categories = await _mediator.Send(categoriesQuery);
                    ViewBag.Categories = categories;
                    ViewBag.SelectedCategoryIds = command.CategoryIds;
                    return View(command);
                }

                if (ImageFile != null && ImageFile.Length > 0)
                {
                    try
                    {
                        var imagePath = await _imageService.UploadAsync(ImageFile, "communities");
                        command.ImageUrl = _imageService.GetImageUrl(imagePath);

                        if (!string.IsNullOrEmpty(oldImageUrl))
                        {
                            await _imageService.DeleteAsync(oldImageUrl);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error uploading community profile image");
                        ModelState.AddModelError("ImageFile", "ไม่สามารถอัปโหลดรูปโปรไฟล์ได้");

                        var categoriesQuery = new GetCategoryTreeQuery { ActiveOnly = true };
                        var categories = await _mediator.Send(categoriesQuery);
                        ViewBag.Categories = categories;
                        ViewBag.SelectedCategoryIds = command.CategoryIds;
                        return View(command);
                    }
                }
                else
                {
                    command.ImageUrl = oldImageUrl;
                }

                if (CoverImageFile != null && CoverImageFile.Length > 0)
                {
                    try
                    {
                        var coverPath = await _imageService.UploadAsync(CoverImageFile, "communities/covers");
                        command.CoverImageUrl = _imageService.GetImageUrl(coverPath);

                        if (!string.IsNullOrEmpty(oldCoverImageUrl))
                        {
                            await _imageService.DeleteAsync(oldCoverImageUrl);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error uploading community cover image");

                        if (ImageFile != null && !string.IsNullOrEmpty(command.ImageUrl) && command.ImageUrl != oldImageUrl)
                        {
                            await _imageService.DeleteAsync(command.ImageUrl);
                        }

                        ModelState.AddModelError("CoverImageFile", "ไม่สามารถอัปโหลดรูปปกได้");

                        var categoriesQuery = new GetCategoryTreeQuery { ActiveOnly = true };
                        var categories = await _mediator.Send(categoriesQuery);
                        ViewBag.Categories = categories;
                        ViewBag.SelectedCategoryIds = command.CategoryIds;
                        return View(command);
                    }
                }
                else
                {
                    command.CoverImageUrl = oldCoverImageUrl;
                }

                var result = await _mediator.Send(command);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = "อัปเดตชุมชนเรียบร้อยแล้ว";
                    return RedirectToAction("MyCommunities");
                }
                else
                {
                    if (ImageFile != null && !string.IsNullOrEmpty(command.ImageUrl) && command.ImageUrl != oldImageUrl)
                    {
                        await _imageService.DeleteAsync(command.ImageUrl);
                    }
                    if (CoverImageFile != null && !string.IsNullOrEmpty(command.CoverImageUrl) && command.CoverImageUrl != oldCoverImageUrl)
                    {
                        await _imageService.DeleteAsync(command.CoverImageUrl);
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }

                    var categoriesQuery = new GetCategoryTreeQuery { ActiveOnly = true };
                    var categories = await _mediator.Send(categoriesQuery);
                    ViewBag.Categories = categories;
                    ViewBag.SelectedCategoryIds = command.CategoryIds;
                    return View(command);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating community");

                if (ImageFile != null && !string.IsNullOrEmpty(command.ImageUrl))
                {
                    await _imageService.DeleteAsync(command.ImageUrl);
                }
                if (CoverImageFile != null && !string.IsNullOrEmpty(command.CoverImageUrl))
                {
                    await _imageService.DeleteAsync(command.CoverImageUrl);
                }

                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการอัปเดตชุมชน: " + ex.Message;

                var categoriesQuery = new GetCategoryTreeQuery { ActiveOnly = true };
                var categories = await _mediator.Send(categoriesQuery);
                ViewBag.Categories = categories;
                ViewBag.SelectedCategoryIds = command.CategoryIds;
                return View(command);
            }
        }
    }
}