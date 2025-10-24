using BlogHybrid.Application.Commands.Post;
using BlogHybrid.Application.Interfaces.Repositories;
using BlogHybrid.Application.Interfaces.Services;
using BlogHybrid.Application.Queries.Category;
using BlogHybrid.Application.Queries.Community;
using BlogHybrid.Application.Queries.Post;
using BlogHybrid.Domain.Entities;
using BlogHybrid.Web.Areas.User.Models;
using BlogHybrid.Web.Models.Post;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BlogHybrid.Web.Areas.User.Controllers
{
    [Area("User")]
    [Authorize]
    public class PostController : Controller
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PostController> _logger;
        private readonly IImageService _imageService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        public PostController(
            IMediator mediator,
            ILogger<PostController> logger,
            IImageService imageService,
            SignInManager<ApplicationUser> signInManager,
            IUnitOfWork unitOfWork)
        {
            _mediator = mediator;
            _logger = logger;
            _imageService = imageService;
            _userManager = signInManager.UserManager;
            _unitOfWork = unitOfWork;
        }
        // GET: /User/Posts
        public async Task<IActionResult> Index(
            int page = 1,
            string? search = null,
            string? status = "all")
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account", new { area = "" });
                }

                var query = new GetUserPostsQuery
                {
                    UserId = userId,
                    PageNumber = page,
                    PageSize = 12,
                    SearchTerm = search,
                    StatusFilter = status,
                    SortBy = "CreatedAt",
                    SortDirection = "desc"
                };

                var result = await _mediator.Send(query);

                var viewModel = new MyPostsViewModel
                {
                    Posts = result.Posts.Select(p => new MyPostItemViewModel
                    {
                        Id = p.Id,
                        Title = p.Title,
                        Slug = p.Slug,
                        Excerpt = p.Excerpt,
                        FeaturedImageUrl = p.FeaturedImageUrl,
                        IsPublished = p.IsPublished,
                        IsFeatured = p.IsFeatured,
                        CreatedAt = p.CreatedAt,
                        PublishedAt = p.PublishedAt,
                        UpdatedAt = p.UpdatedAt,
                        CategoryId = p.CategoryId,
                        CategoryName = p.CategoryName,
                        CommunityId = p.CommunityId,
                        CommunityName = p.CommunityName,
                        ViewCount = p.ViewCount,
                        LikeCount = p.LikeCount,
                        CommentCount = p.CommentCount,
                        Tags = p.Tags
                    }).ToList(),
                    TotalCount = result.TotalCount,
                    PageNumber = result.PageNumber,
                    PageSize = result.PageSize,
                    TotalPages = result.TotalPages,
                    HasPreviousPage = result.HasPreviousPage,
                    HasNextPage = result.HasNextPage,
                    SearchTerm = search,
                    StatusFilter = status ?? "all",
                    Statistics = new PostStatisticsViewModel
                    {
                        PublishedCount = result.PublishedCount,
                        DraftCount = result.DraftCount,
                        FeaturedCount = result.FeaturedCount,
                        TotalViews = result.TotalViews,
                        TotalLikes = result.TotalLikes,
                        TotalComments = result.TotalComments
                    }
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user posts");
                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการโหลดโพสต์";
                return RedirectToAction("Index", "Profile");
            }
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




        // ============================================================
        // GET: /User/Posts/Edit/{id}
        // ============================================================
        [HttpGet("posts/edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ErrorMessage"] = "กรุณาเข้าสู่ระบบ";
                    return RedirectToAction("Login", "Account", new { area = "" });
                }

                // ดึงโพสต์พร้อมข้อมูล Category และ Community
                var post = await _unitOfWork.Posts.GetByIdWithDetailsAsync(id);

                if (post == null)
                {
                    TempData["ErrorMessage"] = "ไม่พบโพสต์ที่ต้องการแก้ไข";
                    return RedirectToAction(nameof(Index));
                }

                // ตรวจสอบสิทธิ์การแก้ไข
                if (post.AuthorId != userId)
                {
                    TempData["ErrorMessage"] = "คุณไม่มีสิทธิ์แก้ไขโพสต์นี้";
                    return RedirectToAction(nameof(Index));
                }

                // สร้าง ViewModel
                var viewModel = new EditPostViewModel
                {
                    Id = post.Id,
                    Title = post.Title,
                    Content = post.Content,
                    Excerpt = post.Excerpt,
                    FeaturedImageUrl = post.FeaturedImageUrl,
                    CurrentFeaturedImageUrl = post.FeaturedImageUrl,
                    CategoryId = post.CategoryId,
                    CommunityId = post.CommunityId,
                    Tags = string.Join(",", post.PostTags.Select(pt => pt.Tag.Name)),
                    IsPublished = post.IsPublished,
                    IsFeatured = post.IsFeatured,
                    CreatedAt = post.CreatedAt,
                    UpdatedAt = post.UpdatedAt
                };

                // ✅ FIX 1: ใช้ GetCategoryTreeQuery แทน GetActiveCategoriesQuery
                var categoriesQuery = new GetCategoryTreeQuery { ActiveOnly = true };
                var categories = await _mediator.Send(categoriesQuery);
                ViewBag.Categories = categories;

                // โหลด Communities ของ User
                var communitiesQuery = new GetUserCommunitiesQuery { UserId = userId };
                var communities = await _mediator.Send(communitiesQuery);
                ViewBag.Communities = communities;

                // ✅ FIX 2: ตั้งค่า Selected Names ให้ถูกต้อง
                // Set Category Name
                if (post.CategoryId.HasValue)
                {
                    if (post.Category != null)
                    {
                        ViewBag.SelectedCategoryName = post.Category.Name;
                    }
                    else
                    {
                        // Fallback: ถ้า Navigation Property ไม่ load ให้หาจาก categories list
                        var selectedCategory = categories.FirstOrDefault(c => c.Id == post.CategoryId.Value);
                        ViewBag.SelectedCategoryName = selectedCategory?.Name ?? "เลือกหมวดหมู่";
                    }
                }
                else
                {
                    ViewBag.SelectedCategoryName = "เลือกหมวดหมู่";
                }

                // Set Community Name
                if (post.CommunityId.HasValue)
                {
                    if (post.Community != null)
                    {
                        ViewBag.SelectedCommunityName = post.Community.Name;
                    }
                    else
                    {
                        // Fallback: ถ้า Navigation Property ไม่ load ให้หาจาก communities list
                        var selectedCommunity = communities.FirstOrDefault(c => c.Id == post.CommunityId.Value);
                        ViewBag.SelectedCommunityName = selectedCommunity?.Name ?? "เลือกชุมชน";
                    }
                }
                else
                {
                    ViewBag.SelectedCommunityName = "เลือกชุมชน";
                }

                _logger.LogInformation($"Loading edit form for post ID: {id}, Category: {ViewBag.SelectedCategoryName}, Community: {ViewBag.SelectedCommunityName}");

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading edit form for post ID: {id}");
                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการโหลดหน้าแก้ไข";
                return RedirectToAction(nameof(Index));
            }
        }


        // ============================================================
        // POST: /User/Posts/Edit/{id}
        // ============================================================
        [HttpPost("posts/edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditPostViewModel model, IFormFile? FeaturedImageFile)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ErrorMessage"] = "กรุณาเข้าสู่ระบบ";
                    return RedirectToAction("Login", "Account", new { area = "" });
                }

                // ✅ Custom Validation
                if (!model.CategoryId.HasValue && !model.CommunityId.HasValue)
                {
                    ModelState.AddModelError("", "กรุณาเลือกหมวดหมู่ หรือ ชุมชน");
                }

                if (!ModelState.IsValid)
                {
                    // ✅ FIX 3: ใช้ GetCategoryTreeQuery แทน GetActiveCategoriesQuery
                    var categoriesQuery = new GetCategoryTreeQuery { ActiveOnly = true };
                    var categories = await _mediator.Send(categoriesQuery);
                    ViewBag.Categories = categories;

                    var communitiesQuery = new GetUserCommunitiesQuery { UserId = userId };
                    var communities = await _mediator.Send(communitiesQuery);
                    ViewBag.Communities = communities;

                    return View(model);
                }

                // ✅ FIX 4: จัดการ Featured Image อย่างถูกต้อง
                string? featuredImageUrl = model.CurrentFeaturedImageUrl;

                if (FeaturedImageFile != null && FeaturedImageFile.Length > 0)
                {
                    try
                    {
                        // ลบรูปเดิมถ้ามีและไม่ใช่ URL ภายนอก
                        if (!string.IsNullOrEmpty(model.CurrentFeaturedImageUrl) &&
                            !model.CurrentFeaturedImageUrl.StartsWith("http"))
                        {
                            await _imageService.DeleteAsync(model.CurrentFeaturedImageUrl);
                        }

                        // Upload รูปใหม่
                        var uploadedPath = await _imageService.UploadAsync(FeaturedImageFile, "posts");
                        featuredImageUrl = _imageService.GetImageUrl(uploadedPath);

                        _logger.LogInformation($"Uploaded new featured image: {featuredImageUrl}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error uploading featured image");
                        TempData["WarningMessage"] = "อัปโหลดรูปภาพไม่สำเร็จ แต่ยังคงอัปเดตข้อมูลอื่น";
                        featuredImageUrl = model.CurrentFeaturedImageUrl;
                    }
                }
                else if (string.IsNullOrEmpty(model.FeaturedImageUrl))
                {
                    // ถ้าไม่มีการอัปโหลดใหม่ และ FeaturedImageUrl เป็นค่าว่าง = ผู้ใช้ต้องการลบรูป
                    featuredImageUrl = null;
                }
                else
                {
                    // ใช้ URL ที่มีอยู่แล้ว (อาจเป็น URL ภายนอกหรือ URL เดิม)
                    featuredImageUrl = model.FeaturedImageUrl;
                }

                // สร้าง Command
                var command = new UpdatePostCommand
                {
                    Id = model.Id,
                    Title = model.Title.Trim(),
                    Content = model.Content.Trim(),
                    Excerpt = model.Excerpt?.Trim(),
                    FeaturedImageUrl = featuredImageUrl,
                    CategoryId = model.CategoryId,
                    CommunityId = model.CommunityId,
                    Tags = model.Tags?.Trim(),
                    IsPublished = model.IsPublished,
                    IsFeatured = model.IsFeatured,
                    AuthorId = userId
                };

                // ส่ง Command
                var result = await _mediator.Send(command);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = "อัปเดตโพสต์สำเร็จ";
                    _logger.LogInformation($"Post updated successfully: ID={model.Id}, Slug={result.Slug}");

                    // ✅ FIX 5: Redirect กลับไปหน้ารายการโพสต์
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }

                    // Reload dropdowns
                    var categoriesQuery = new GetCategoryTreeQuery { ActiveOnly = true };
                    var categories = await _mediator.Send(categoriesQuery);
                    ViewBag.Categories = categories;

                    var communitiesQuery = new GetUserCommunitiesQuery { UserId = userId };
                    var communities = await _mediator.Send(communitiesQuery);
                    ViewBag.Communities = communities;
                    TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการอัปเดตโพสต์";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating post ID: {id}");
                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการอัปเดตโพสต์";
                return RedirectToAction(nameof(Index));
            }
        }



    }
}
