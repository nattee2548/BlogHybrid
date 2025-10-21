using BlogHybrid.Application.Commands.Community;
using BlogHybrid.Application.Interfaces.Repositories;
using BlogHybrid.Application.Interfaces.Services;
using BlogHybrid.Application.Queries.Category;
using BlogHybrid.Application.Queries.Community;
using BlogHybrid.Domain.Enums;
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

        // GET: /communities
        [HttpGet("communities")]
        public async Task<IActionResult> Index(
            int pageNumber = 1,
            int pageSize = 12,
            int? categoryId = null,
            string? searchTerm = null,
            bool? isPrivate = null,
            string sortBy = "CreatedAt",
            string sortDirection = "desc")
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var query = new GetCommunitiesQuery
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    CategoryId = categoryId,
                    SearchTerm = searchTerm,
                    IsPrivate = isPrivate,
                    IsActive = true, // แสดงเฉพาะชุมชนที่ active
                    SortBy = sortBy,
                    SortDirection = sortDirection,
                    CurrentUserId = userId
                };

                var result = await _mediator.Send(query);

                // Get categories for filter dropdown
                var categoriesQuery = new GetCategoryTreeQuery { ActiveOnly = true };
                var categories = await _mediator.Send(categoriesQuery);
                ViewBag.Categories = categories;

                // Pass filter values back to view
                ViewBag.SearchTerm = searchTerm;
                ViewBag.SelectedCategoryId = categoryId;
                ViewBag.IsPrivate = isPrivate;
                ViewBag.SortBy = sortBy;

                return View(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading communities page");
                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการโหลดหน้า";
                return RedirectToAction("Index", "Home");
            }
        }

        // POST: /community/join/{id}
        [Authorize]
        [HttpPost("community/join/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Join(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ErrorMessage"] = "กรุณาเข้าสู่ระบบ";
                    return RedirectToAction("Login", "Account");
                }

                var command = new JoinCommunityCommand
                {
                    CommunityId = id,
                    UserId = userId
                };

                var result = await _mediator.Send(command);

                if (result.Success)
                {
                    if (result.RequiresApproval)
                    {
                        TempData["SuccessMessage"] = "ส่งคำขอเข้าร่วมชุมชนเรียบร้อย! กรุณารอการอนุมัติจากผู้ดูแล";
                    }
                    else
                    {
                        TempData["SuccessMessage"] = "เข้าร่วมชุมชนสำเร็จ!";
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = string.Join(", ", result.Errors);
                }

                // Redirect back to the community or communities list
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error joining community {CommunityId}", id);
                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการเข้าร่วมชุมชน";
                return RedirectToAction("Index");
            }
        }

        // POST: /community/leave/{id}
        [Authorize]
        [HttpPost("community/leave/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Leave(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ErrorMessage"] = "กรุณาเข้าสู่ระบบ";
                    return RedirectToAction("Login", "Account");
                }

                var command = new LeaveCommunityCommand
                {
                    CommunityId = id,
                    UserId = userId
                };

                var result = await _mediator.Send(command);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = "ออกจากชุมชนสำเร็จ!";
                }
                else
                {
                    TempData["ErrorMessage"] = string.Join(", ", result.Errors);
                }

                // Redirect back to communities list
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error leaving community {CommunityId}", id);
                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการออกจากชุมชน";
                return RedirectToAction("Index");
            }
        }



        #region Member Management

        // GET: /community/manage-members/{id}
        [Authorize]
        [HttpGet("community/manage-members/{id}")]
        public async Task<IActionResult> ManageMembers(
            int id,
            int pageNumber = 1,
            int pageSize = 20,
            string? searchTerm = null,
            CommunityRole? roleFilter = null,
            bool showPending = false)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ErrorMessage"] = "กรุณาเข้าสู่ระบบ";
                    return RedirectToAction("Login", "Account");
                }

                // Get community
                var community = await _unitOfWork.Communities.GetByIdWithDetailsAsync(id);
                if (community == null)
                {
                    TempData["ErrorMessage"] = "ไม่พบชุมชนที่ต้องการ";
                    return RedirectToAction("Index");
                }

                // Check permission - only Admin/Moderator can manage members
                var isAuthorized = await _unitOfWork.Communities.IsModeratorOrAdminAsync(id, userId);
                if (!isAuthorized)
                {
                    TempData["ErrorMessage"] = "คุณไม่มีสิทธิ์เข้าถึงหน้านี้";
                    return RedirectToAction("Details", new { communitySlug = community.Slug });
                }

                // Get members
                var (members, totalCount) = await _unitOfWork.Communities.GetMembersPagedAsync(
                    id,
                    pageNumber,
                    pageSize,
                    roleFilter);

                // Filter by search term if provided
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    members = members.Where(m =>
                        m.User.DisplayName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        m.User.Email.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                    ).ToList();
                    totalCount = members.Count;
                }

                // Filter pending members if requested
                if (showPending)
                {
                    members = members.Where(m => !m.IsApproved && !m.IsBanned).ToList();
                    totalCount = members.Count;
                }

                // Pass data to view
                ViewBag.Community = community;
                ViewBag.IsCreator = community.CreatorId == userId;
                ViewBag.CurrentUserId = userId;
                ViewBag.SearchTerm = searchTerm;
                ViewBag.RoleFilter = roleFilter;
                ViewBag.ShowPending = showPending;
                ViewBag.PageNumber = pageNumber;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalCount = totalCount;
                ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                return View(members);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading manage members page for community {CommunityId}", id);
                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการโหลดหน้า";
                return RedirectToAction("Index");
            }
        }

        // POST: /community/approve-member
        [Authorize]
        [HttpPost("community/approve-member")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveMember(int communityId, string memberUserId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ErrorMessage"] = "กรุณาเข้าสู่ระบบ";
                    return RedirectToAction("Login", "Account");
                }

                var command = new ApproveMemberCommand
                {
                    CommunityId = communityId,
                    MemberUserId = memberUserId,
                    CurrentUserId = userId
                };

                var result = await _mediator.Send(command);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = "อนุมัติสมาชิกเรียบร้อยแล้ว";
                }
                else
                {
                    TempData["ErrorMessage"] = string.Join(", ", result.Errors);
                }

                return RedirectToAction("ManageMembers", new { id = communityId, showPending = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving member");
                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการอนุมัติสมาชิก";
                return RedirectToAction("ManageMembers", new { id = communityId });
            }
        }

        // POST: /community/reject-member
        [Authorize]
        [HttpPost("community/reject-member")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectMember(int communityId, string memberUserId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ErrorMessage"] = "กรุณาเข้าสู่ระบบ";
                    return RedirectToAction("Login", "Account");
                }

                var command = new RejectMemberCommand
                {
                    CommunityId = communityId,
                    MemberUserId = memberUserId,
                    CurrentUserId = userId
                };

                var result = await _mediator.Send(command);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = "ปฏิเสธคำขอเข้าร่วมเรียบร้อยแล้ว";
                }
                else
                {
                    TempData["ErrorMessage"] = string.Join(", ", result.Errors);
                }

                return RedirectToAction("ManageMembers", new { id = communityId, showPending = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting member");
                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการปฏิเสธคำขอ";
                return RedirectToAction("ManageMembers", new { id = communityId });
            }
        }

        // POST: /community/change-member-role
        [Authorize]
        [HttpPost("community/change-member-role")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeMemberRole(int communityId, string memberUserId, CommunityRole newRole)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ErrorMessage"] = "กรุณาเข้าสู่ระบบ";
                    return RedirectToAction("Login", "Account");
                }

                var command = new ChangeMemberRoleCommand
                {
                    CommunityId = communityId,
                    MemberUserId = memberUserId,
                    NewRole = newRole,
                    CurrentUserId = userId
                };

                var result = await _mediator.Send(command);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = $"เปลี่ยนบทบาทเป็น {newRole} เรียบร้อยแล้ว";
                }
                else
                {
                    TempData["ErrorMessage"] = string.Join(", ", result.Errors);
                }

                return RedirectToAction("ManageMembers", new { id = communityId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing member role");
                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการเปลี่ยนบทบาท";
                return RedirectToAction("ManageMembers", new { id = communityId });
            }
        }

        // POST: /community/ban-member
        [Authorize]
        [HttpPost("community/ban-member")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BanMember(int communityId, string memberUserId, bool isBanned = true)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ErrorMessage"] = "กรุณาเข้าสู่ระบบ";
                    return RedirectToAction("Login", "Account");
                }

                var command = new BanMemberCommand
                {
                    CommunityId = communityId,
                    MemberUserId = memberUserId,
                    IsBanned = isBanned,
                    CurrentUserId = userId
                };

                var result = await _mediator.Send(command);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = isBanned ? "แบนสมาชิกเรียบร้อยแล้ว" : "ปลดแบนสมาชิกเรียบร้อยแล้ว";
                }
                else
                {
                    TempData["ErrorMessage"] = string.Join(", ", result.Errors);
                }

                return RedirectToAction("ManageMembers", new { id = communityId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error banning member");
                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการแบนสมาชิก";
                return RedirectToAction("ManageMembers", new { id = communityId });
            }
        }

        // POST: /community/remove-member
        [Authorize]
        [HttpPost("community/remove-member")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveMember(int communityId, string memberUserId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ErrorMessage"] = "กรุณาเข้าสู่ระบบ";
                    return RedirectToAction("Login", "Account");
                }

                var command = new RemoveMemberCommand
                {
                    CommunityId = communityId,
                    MemberUserId = memberUserId,
                    CurrentUserId = userId
                };

                var result = await _mediator.Send(command);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = "ลบสมาชิกออกจากชุมชนเรียบร้อยแล้ว";
                }
                else
                {
                    TempData["ErrorMessage"] = string.Join(", ", result.Errors);
                }

                return RedirectToAction("ManageMembers", new { id = communityId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing member");
                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการลบสมาชิก";
                return RedirectToAction("ManageMembers", new { id = communityId });
            }
        }

        #endregion

    }
}