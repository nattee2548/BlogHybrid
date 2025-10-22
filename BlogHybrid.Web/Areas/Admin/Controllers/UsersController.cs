using BlogHybrid.Application.Commands.User;
using BlogHybrid.Application.Queries.User;
using BlogHybrid.Domain.Entities;
using BlogHybrid.Web.Areas.Admin.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BlogHybrid.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly IMediator _mediator;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<UsersController> _logger;
        private readonly SignInManager<ApplicationUser> _signInManager;
        public UsersController(
            IMediator mediator,
            UserManager<ApplicationUser> userManager,
            ILogger<UsersController> logger,
            SignInManager<ApplicationUser> signInManager)
        {
            _mediator = mediator;
            _userManager = userManager;
            _logger = logger;
            _signInManager = signInManager;
        }

        // GET: /Admin/Users
        public async Task<IActionResult> Index(int page = 1, string? search = null, string? status = "all")
        {
            try
            {
                var query = new GetPagedUsersQuery
                {
                    PageNumber = page,
                    PageSize = 20,
                    SearchTerm = search,
                    RoleFilter = "User", // กรองเฉพาะ User ทั่วไป (ไม่ใช่ Admin)
                    SortBy = "CreatedAt",
                    SortDirection = "desc"
                };

                var result = await _mediator.Send(query);

                // Filter by status
                var filteredUsers = result.Users.AsEnumerable();
                if (status == "active")
                {
                    filteredUsers = filteredUsers.Where(u => u.IsActive && u.EmailConfirmed);
                }
                else if (status == "inactive")
                {
                    filteredUsers = filteredUsers.Where(u => !u.IsActive);
                }
                else if (status == "pending")
                {
                    filteredUsers = filteredUsers.Where(u => u.IsActive && !u.EmailConfirmed);
                }

                var viewModel = new UsersListViewModel
                {
                    Users = filteredUsers.Select(u => new UserItemViewModel
                    {
                        Id = u.Id,
                        Email = u.Email,
                        UserName = u.UserName,
                        DisplayName = u.DisplayName,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        FullName = $"{u.FirstName} {u.LastName}".Trim(),
                        IsActive = u.IsActive,
                        EmailConfirmed = u.EmailConfirmed,
                        CreatedAt = u.CreatedAt,
                        LastLoginAt = u.LastLoginAt,
                        PostCount = 0, // TODO: จะเพิ่มการนับจริงในอนาคต
                        CommentCount = 0,
                        CommunityCount = 0
                    }).ToList(),
                    TotalCount = filteredUsers.Count(),
                    PageNumber = page,
                    PageSize = 20,
                    TotalPages = (int)Math.Ceiling(filteredUsers.Count() / 20.0),
                    SearchTerm = search,
                    StatusFilter = status
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users list");
                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการโหลดข้อมูล";
                return View(new UsersListViewModel());
            }
        }

        // GET: /Admin/Users/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                var query = new GetUserDetailsQuery { Id = id };
                var result = await _mediator.Send(query);

                if (result == null)
                {
                    return NotFound();
                }

                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                // ตรวจสอบว่าเป็น User ทั่วไป (ไม่ใช่ Admin)
                var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
                if (isAdmin)
                {
                    TempData["ErrorMessage"] = "ไม่สามารถเข้าถึงข้อมูล Admin ได้จากหน้านี้";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new UserDetailsViewModel
                {
                    Id = result.Id,
                    Email = result.Email,
                    UserName = result.UserName,
                    DisplayName = result.DisplayName,
                    FirstName = result.FirstName,
                    LastName = result.LastName,
                    FullName = $"{result.FirstName} {result.LastName}".Trim(),
                    PhoneNumber = result.PhoneNumber,
                    Bio = result.Bio,
                    ProfileImageUrl = result.ProfileImageUrl,
                    IsActive = result.IsActive,
                    EmailConfirmed = result.EmailConfirmed,
                    PhoneNumberConfirmed = result.PhoneNumberConfirmed,
                    TwoFactorEnabled = result.TwoFactorEnabled,
                    CreatedAt = result.CreatedAt,
                    LastLoginAt = result.LastLoginAt,
                    Roles = result.Roles,
                    PostCount = 0, // TODO: เพิ่มการนับจริง
                    CommentCount = 0,
                    CommunityCount = 0,
                    FollowerCount = 0,
                    FollowingCount = 0
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting user details for ID: {id}");
                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการโหลดข้อมูล";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: /Admin/Users/Create
        public IActionResult Create()
        {
            return View(new CreateUserViewModel());
        }

        // POST: /Admin/Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var user = new ApplicationUser
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    DisplayName = model.UserName,
                    PhoneNumber = model.PhoneNumber,
                    IsActive = model.IsActive,
                    EmailConfirmed = model.EmailConfirmed,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // เพิ่ม Role "User"
                    await _userManager.AddToRoleAsync(user, "User");

                    _logger.LogInformation($"Admin created user: {user.UserName}");
                    TempData["SuccessMessage"] = "สร้างบัญชีผู้ใช้สำเร็จ";
                    return RedirectToAction(nameof(Details), new { id = user.Id });
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                ModelState.AddModelError(string.Empty, "เกิดข้อผิดพลาดในการสร้างบัญชี");
            }

            return View(model);
        }

        // GET: /Admin/Users/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                var query = new GetUserDetailsQuery { Id = id };
                var result = await _mediator.Send(query);

                if (result == null)
                {
                    return NotFound();
                }

                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                // ตรวจสอบว่าไม่ใช่ Admin
                var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
                if (isAdmin)
                {
                    TempData["ErrorMessage"] = "ไม่สามารถแก้ไขข้อมูล Admin ได้จากหน้านี้";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new EditUserViewModel
                {
                    Id = result.Id,
                    Email = result.Email,
                    UserName = result.UserName,
                    DisplayName = result.DisplayName,
                    FirstName = result.FirstName,
                    LastName = result.LastName,
                    PhoneNumber = result.PhoneNumber,
                    Bio = result.Bio,
                    IsActive = result.IsActive,
                    EmailConfirmed = result.EmailConfirmed
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading edit form for user ID: {id}");
                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการโหลดข้อมูล";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Admin/Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, EditUserViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                // อัปเดตข้อมูล
                user.Email = model.Email;
                user.UserName = model.UserName;
                user.DisplayName = model.DisplayName;
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.PhoneNumber = model.PhoneNumber;
                user.Bio = model.Bio;
                user.IsActive = model.IsActive;
                user.EmailConfirmed = model.EmailConfirmed;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    _logger.LogInformation($"Admin updated user: {user.UserName}");
                    TempData["SuccessMessage"] = "อัปเดตข้อมูลสำเร็จ";
                    return RedirectToAction(nameof(Details), new { id = model.Id });
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating user ID: {id}");
                ModelState.AddModelError(string.Empty, "เกิดข้อผิดพลาดในการอัปเดตข้อมูล");
            }

            return View(model);
        }

        // GET: /Admin/Users/ChangePassword/5
        public async Task<IActionResult> ChangePassword(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var viewModel = new ChangeUserPasswordViewModel
            {
                UserId = user.Id,
                UserName = user.UserName
            };

            return View(viewModel);
        }

        // POST: /Admin/Users/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangeUserPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var user = await _userManager.FindByIdAsync(model.UserId);
                if (user == null)
                {
                    return NotFound();
                }

                // ✅ เก็บ Current User (Admin) ไว้ก่อน
                var currentAdminUser = await _userManager.GetUserAsync(User);
                var currentAdminId = currentAdminUser?.Id;

                // ลบรหัสผ่านเดิมและตั้งรหัสผ่านใหม่
                var removePasswordResult = await _userManager.RemovePasswordAsync(user);
                if (!removePasswordResult.Succeeded)
                {
                    foreach (var error in removePasswordResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(model);
                }

                var addPasswordResult = await _userManager.AddPasswordAsync(user, model.NewPassword);
                if (addPasswordResult.Succeeded)
                {
                    _logger.LogInformation($"Admin changed password for user: {user.UserName}");

                    // ✅ ถ้าเป็นการเปลี่ยนรหัสผ่านให้ตัวเอง → Update Security Stamp ของ Admin
                    if (user.Id == currentAdminId)
                    {
                        await _userManager.UpdateSecurityStampAsync(currentAdminUser!);

                        // ✅ Refresh Sign In เพื่อไม่ให้โดน logout
                        await _signInManager.RefreshSignInAsync(currentAdminUser!);

                        TempData["SuccessMessage"] = "เปลี่ยนรหัสผ่านของคุณสำเร็จ";
                    }
                    else
                    {
                        // ✅ เปลี่ยนรหัสผ่านให้ User อื่น → Update Security Stamp ของ User นั้น
                        await _userManager.UpdateSecurityStampAsync(user);

                        TempData["SuccessMessage"] = $"เปลี่ยนรหัสผ่านของ {user.UserName} สำเร็จ";
                    }

                    return RedirectToAction(nameof(Details), new { id = model.UserId });
                }

                foreach (var error in addPasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error changing password for user ID: {model.UserId}");
                ModelState.AddModelError(string.Empty, "เกิดข้อผิดพลาดในการเปลี่ยนรหัสผ่าน");
            }

            return View(model);
        }

        // POST: /Admin/Users/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                // ตรวจสอบว่าไม่ใช่ Admin
                var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
                if (isAdmin)
                {
                    TempData["ErrorMessage"] = "ไม่สามารถลบบัญชี Admin ได้จากหน้านี้";
                    return RedirectToAction(nameof(Index));
                }

                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    _logger.LogInformation($"Admin deleted user: {user.UserName}");
                    TempData["SuccessMessage"] = "ลบบัญชีผู้ใช้สำเร็จ";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    TempData["ErrorMessage"] = error.Description;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting user ID: {id}");
                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการลบบัญชี";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/Users/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                user.IsActive = !user.IsActive;
                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    var status = user.IsActive ? "เปิดใช้งาน" : "ปิดใช้งาน";
                    _logger.LogInformation($"Admin toggled user status: {user.UserName} - {status}");
                    TempData["SuccessMessage"] = $"{status}บัญชีสำเร็จ";
                }
                else
                {
                    TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการเปลี่ยนสถานะ";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error toggling user status for ID: {id}");
                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการเปลี่ยนสถานะ";
            }

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}