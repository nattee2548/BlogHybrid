using BlogHybrid.Application.Commands.Post;
using BlogHybrid.Application.Interfaces.Services;
using BlogHybrid.Application.Queries.Category;
using BlogHybrid.Application.Queries.Community;
using BlogHybrid.Web.Models.Post;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BlogHybrid.Web.Controllers
{
    [Authorize]
    public class PostController : Controller
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PostController> _logger;
        private readonly IImageService _imageService;

        public PostController(
            IMediator mediator,
            ILogger<PostController> logger,
            IImageService imageService)
        {
            _mediator = mediator;
            _logger = logger;
            _imageService = imageService;
        }

        // ===================================
        // API: Upload Image
        // ===================================
        [HttpPost("api/upload-image")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadImage(IFormFile file, string folder = "posts")
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "กรุณาเข้าสู่ระบบ" });
                }

                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { message = "ไม่พบไฟล์รูปภาพ" });
                }

                // Upload to Cloudflare R2
                var imagePath = await _imageService.UploadAsync(file, folder);
                var imageUrl = _imageService.GetImageUrl(imagePath);

                return Ok(new { url = imageUrl });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid image file");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image");
                return StatusCode(500, new { message = "เกิดข้อผิดพลาดในการอัปโหลดรูปภาพ" });
            }
        }

        // GET: /post/create
        // GET: /post/create?communityId=5 (สร้างจากหน้าชุมชน)
        [HttpGet("post/create")]
        public async Task<IActionResult> Create(int? communityId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ErrorMessage"] = "กรุณาเข้าสู่ระบบก่อนสร้างโพสต์";
                    return RedirectToAction("Login", "Account");
                }

                var viewModel = new CreatePostViewModel();

                // ===================================
                // ⭐ ถ้ามี communityId แสดงว่าสร้างจากหน้าชุมชน
                // ===================================
                if (communityId.HasValue)
                {
                    var communityQuery = new GetCommunityDetailsQuery
                    {
                        Slug = "", // จะใช้ GetByIdAsync แทน
                        CurrentUserId = userId
                    };

                    // ดึงข้อมูลชุมชน
                    var community = await _mediator.Send(new GetUserCommunitiesQuery { UserId = userId });
                    var selectedCommunity = community.FirstOrDefault(c => c.Id == communityId.Value);

                    if (selectedCommunity != null)
                    {
                        // ตรวจสอบว่าเป็นสมาชิกหรือไม่ และไม่ถูกแบน
                        var isMemberAndNotBanned = selectedCommunity.IsCurrentUserMember == true && 
                                                   selectedCommunity.MemberStatus != Domain.Enums.CommunityMemberStatus.Banned;
                        
                        if (isMemberAndNotBanned)
                        {
                            viewModel.CommunityId = communityId;
                            viewModel.CommunityName = selectedCommunity.Name;
                            viewModel.IsFromCommunity = true; // ล็อคไม่ให้แก้ไข
                            
                            _logger.LogInformation("User {UserId} creating post in community {CommunityId}", 
                                userId, communityId);
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "คุณไม่มีสิทธิ์สร้างโพสต์ในชุมชนนี้";
                            return RedirectToAction("Index", "Home");
                        }
                    }
                }

                // ===================================
                // Load Categories (ทั้งหมดที่ active)
                // ===================================
                var categoriesQuery = new GetCategoryTreeQuery { ActiveOnly = true };
                var categories = await _mediator.Send(categoriesQuery);

                viewModel.Categories = categories
                    .SelectMany(parent => new[]
                    {
                        new CategorySelectItem
                        {
                            Id = parent.Id,
                            Name = parent.Name,
                            Color = parent.Color,
                            ParentCategoryId = null,
                            ParentCategoryName = null
                        }
                    }
                    .Concat(
                        parent.SubCategories?.Select(sub => new CategorySelectItem
                        {
                            Id = sub.Id,
                            Name = sub.Name,
                            Color = sub.Color,
                            ParentCategoryId = parent.Id,
                            ParentCategoryName = parent.Name
                        }) ?? Enumerable.Empty<CategorySelectItem>()
                    ))
                    .ToList();

                // ===================================
                // Load Communities (เฉพาะที่ user เป็นสมาชิก)
                // ===================================
                var userCommunitiesQuery = new GetUserCommunitiesQuery { UserId = userId };
                var userCommunities = await _mediator.Send(userCommunitiesQuery);

                viewModel.Communities = userCommunities
                    .Where(c => c.IsCurrentUserMember == true && 
                               c.MemberStatus != Domain.Enums.CommunityMemberStatus.Banned)
                    .Select(c => new CommunitySelectItem
                    {
                        Id = c.Id,
                        Name = c.Name,
                        ImageUrl = c.ImageUrl,
                        MemberCount = c.MemberCount,
                        IsPrivate = c.IsPrivate
                    })
                    .OrderBy(c => c.Name)
                    .ToList();

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create post page");
                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการโหลดหน้า";
                return RedirectToAction("Index", "Home");
            }
        }

        // POST: /post/create
        [HttpPost("post/create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePostViewModel model)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ErrorMessage"] = "กรุณาเข้าสู่ระบบก่อนสร้างโพสต์";
                    return RedirectToAction("Login", "Account");
                }

                // ===================================
                // Custom Validation: ต้องเลือกอย่างน้อย 1 อย่าง
                // ===================================
                if (!model.CategoryId.HasValue && !model.CommunityId.HasValue)
                {
                    ModelState.AddModelError("", "กรุณาเลือกหมวดหมู่ หรือ ชุมชน");
                }

                if (!ModelState.IsValid)
                {
                    // Reload dropdowns
                    await LoadDropdownData(model, userId);
                    return View(model);
                }

                // ===================================
                // สร้าง Command
                // ===================================
                var command = new CreatePostCommand
                {
                    Title = model.Title.Trim(),
                    Content = model.Content.Trim(),
                    Excerpt = model.Excerpt?.Trim(),
                    FeaturedImageUrl = model.FeaturedImageUrl,
                    CategoryId = model.CategoryId,
                    CommunityId = model.CommunityId,
                    Tags = model.Tags,
                    IsPublished = model.IsPublished,
                    IsFeatured = model.IsFeatured,
                    AuthorId = userId
                };

                // ===================================
                // ส่ง Command
                // ===================================
                var result = await _mediator.Send(command);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = "สร้างโพสต์สำเร็จ";
                    
                    _logger.LogInformation(
                        "Post created successfully. PostId: {PostId}, UserId: {UserId}", 
                        result.PostId, userId
                    );

                    // TODO: Redirect to post details page
                    // return RedirectToAction("Details", new { slug = result.Slug });
                    
                    // ชั่วคราว redirect กลับไปหน้าแรก
                    return RedirectToAction("Index", "Home");
                }

                // ===================================
                // แสดง Errors
                // ===================================
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error);
                }

                // Reload dropdowns
                await LoadDropdownData(model, userId);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating post");
                ModelState.AddModelError("", "เกิดข้อผิดพลาดในการสร้างโพสต์");

                // Reload dropdowns
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(userId))
                {
                    await LoadDropdownData(model, userId);
                }
                
                return View(model);
            }
        }

        // ===================================
        // Helper Method: โหลดข้อมูล Dropdowns
        // ===================================
        private async Task LoadDropdownData(CreatePostViewModel model, string userId)
        {
            try
            {
                // Load Categories
                var categoriesQuery = new GetCategoryTreeQuery { ActiveOnly = true };
                var categories = await _mediator.Send(categoriesQuery);

                model.Categories = categories
                    .SelectMany(parent => new[]
                    {
                        new CategorySelectItem
                        {
                            Id = parent.Id,
                            Name = parent.Name,
                            Color = parent.Color,
                            ParentCategoryId = null,
                            ParentCategoryName = null
                        }
                    }
                    .Concat(
                        parent.SubCategories?.Select(sub => new CategorySelectItem
                        {
                            Id = sub.Id,
                            Name = sub.Name,
                            Color = sub.Color,
                            ParentCategoryId = parent.Id,
                            ParentCategoryName = parent.Name
                        }) ?? Enumerable.Empty<CategorySelectItem>()
                    ))
                    .ToList();

                // Load Communities
                var userCommunitiesQuery = new GetUserCommunitiesQuery { UserId = userId };
                var userCommunities = await _mediator.Send(userCommunitiesQuery);

                model.Communities = userCommunities
                    .Where(c => c.IsCurrentUserMember == true && 
                               c.MemberStatus != Domain.Enums.CommunityMemberStatus.Banned)
                    .Select(c => new CommunitySelectItem
                    {
                        Id = c.Id,
                        Name = c.Name,
                        ImageUrl = c.ImageUrl,
                        MemberCount = c.MemberCount,
                        IsPrivate = c.IsPrivate
                    })
                    .OrderBy(c => c.Name)
                    .ToList();

                // ถ้าสร้างจากหน้าชุมชน ให้ set community name
                if (model.IsFromCommunity && model.CommunityId.HasValue)
                {
                    var community = model.Communities.FirstOrDefault(c => c.Id == model.CommunityId.Value);
                    if (community != null)
                    {
                        model.CommunityName = community.Name;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dropdown data");
            }
        }
    }
}
