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
    public class AdminUsersController : Controller
    {
        private readonly IMediator _mediator;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AdminUsersController> _logger;

        public AdminUsersController(
            IMediator mediator,
            UserManager<ApplicationUser> userManager,
            ILogger<AdminUsersController> logger)
        {
            _mediator = mediator;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: /Admin/AdminUsers
        public async Task<IActionResult> Index(int page = 1, string? search = null)
        {
            try
            {
                var query = new GetPagedUsersQuery
                {
                    PageNumber = page,
                    PageSize = 20,
                    SearchTerm = search,
                    RoleFilter = "Admin",
                    SortBy = "CreatedAt",
                    SortDirection = "desc"
                };

                var result = await _mediator.Send(query);

                var viewModel = new AdminUsersListViewModel
                {
                    Users = result.Users.Select(u => new AdminUserItemViewModel
                    {
                        Id = u.Id,
                        Email = u.Email,
                        UserName = u.UserName,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        FullName = $"{u.FirstName} {u.LastName}".Trim(),
                        IsActive = u.IsActive,
                        EmailConfirmed = u.EmailConfirmed,
                        CreatedAt = u.CreatedAt,
                        LastLoginAt = u.LastLoginAt
                    }).ToList(),
                    TotalCount = result.TotalCount,
                    PageNumber = result.PageNumber,
                    PageSize = result.PageSize,
                    TotalPages = result.TotalPages,
                    SearchTerm = search
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting admin users list");
                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการโหลดข้อมูล";
                return View(new AdminUsersListViewModel());
            }
        }

        // GET: /Admin/AdminUsers/Details/5
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
                if (user == null || !await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    return NotFound();
                }

                var viewModel = new AdminUserDetailsViewModel
                {
                    Id = result.Id,
                    Email = result.Email,
                    UserName = result.UserName,
                    DisplayName = result.DisplayName,
                    FirstName = result.FirstName,
                    LastName = result.LastName,
                    FullName = $"{result.FirstName} {result.LastName}".Trim(),
                    PhoneNumber = result.PhoneNumber,
                    IsActive = result.IsActive,
                    EmailConfirmed = result.EmailConfirmed,
                    PhoneNumberConfirmed = result.PhoneNumberConfirmed,
                    TwoFactorEnabled = result.TwoFactorEnabled,
                    CreatedAt = result.CreatedAt,
                    LastLoginAt = result.LastLoginAt,
                    Roles = result.Roles
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting admin user details for ID: {id}");
                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการโหลดข้อมูล";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: /Admin/AdminUsers/Edit/5
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
                if (user == null || !await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    return NotFound();
                }

                var viewModel = new EditAdminUserViewModel
                {
                    Id = result.Id,
                    Email = result.Email,
                    UserName = result.UserName,
                    FirstName = result.FirstName,
                    LastName = result.LastName,
                    PhoneNumber = result.PhoneNumber,
                    IsActive = result.IsActive,
                    EmailConfirmed = result.EmailConfirmed
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading edit form for admin user ID: {id}");
                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการโหลดข้อมูล";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Admin/AdminUsers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, EditAdminUserViewModel model)
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
                var command = new UpdateUserCommand
                {
                    Id = model.Id,  // ✅ ใช้ Id ไม่ใช่ UserId
                    Email = model.Email,
                    UserName = model.UserName,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNumber = model.PhoneNumber,
                    IsActive = model.IsActive,
                    EmailConfirmed = model.EmailConfirmed
                };

                var result = await _mediator.Send(command);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = "อัปเดตข้อมูลสำเร็จ";
                    return RedirectToAction(nameof(Details), new { id = model.Id });
                }
                else
                {
                    TempData["ErrorMessage"] = result.Errors?.FirstOrDefault() ?? "ไม่สามารถอัปเดตข้อมูลได้";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating admin user ID: {id}");
                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการอัปเดตข้อมูล";
                return View(model);
            }
        }

        // POST: /Admin/AdminUsers/ToggleActive/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null || !await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    return NotFound();
                }

                // ป้องกันการปิด Admin ตัวเอง
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser?.Id == id)
                {
                    TempData["ErrorMessage"] = "ไม่สามารถปิดการใช้งานบัญชีของตัวเองได้";
                    return RedirectToAction(nameof(Details), new { id });
                }

                var command = new UpdateUserCommand
                {
                    Id = id,  // ✅ ใช้ Id ไม่ใช่ UserId
                    Email = user.Email!,
                    UserName = user.UserName!,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = user.PhoneNumber,
                    IsActive = !user.IsActive,
                    EmailConfirmed = user.EmailConfirmed
                };

                var result = await _mediator.Send(command);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = user.IsActive ? "ปิดการใช้งานสำเร็จ" : "เปิดการใช้งานสำเร็จ";
                }
                else
                {
                    TempData["ErrorMessage"] = result.Errors?.FirstOrDefault() ?? "ไม่สามารถเปลี่ยนสถานะได้";
                }

                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error toggling active status for admin user ID: {id}");
                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการเปลี่ยนสถานะ";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}