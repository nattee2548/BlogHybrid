using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MediatR;
using BlogHybrid.Web.Models.ViewModels.Admin;
using BlogHybrid.Application.Commands.User;
using BlogHybrid.Application.Queries.User;

namespace BlogHybrid.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly IMediator _mediator;
        private readonly ILogger<UserController> _logger;

        public UserController(
            IMediator mediator,
            ILogger<UserController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        #region Index

        // GET: /Admin/User
        [HttpGet]
        public async Task<IActionResult> Index(
            string? searchTerm = null,
            string? roleFilter = null,
            bool? isActiveFilter = null,
            int page = 1,
            int pageSize = 20)
        {
            try
            {
                var query = new GetPagedUsersQuery
                {
                    PageNumber = page,
                    PageSize = pageSize,
                    SearchTerm = searchTerm,
                    RoleFilter = roleFilter,
                    IsActiveFilter = isActiveFilter
                };

                var result = await _mediator.Send(query);

                // Get available roles for filter dropdown
                var rolesQuery = new GetAllRolesQuery();
                var availableRoles = await _mediator.Send(rolesQuery);

                var userViewModels = result.Users.Select(u => new UserListItemViewModel
                {
                    Id = u.Id,
                    Email = u.Email,
                    UserName = u.UserName,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    IsActive = u.IsActive,
                    EmailConfirmed = u.EmailConfirmed,
                    CreatedAt = u.CreatedAt,
                    LastLoginAt = u.LastLoginAt,
                    Roles = u.Roles
                }).ToList();

                var model = new UserIndexViewModel
                {
                    Users = userViewModels,
                    SearchTerm = searchTerm,
                    RoleFilter = roleFilter,
                    IsActiveFilter = isActiveFilter,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalUsers = result.TotalCount,
                    TotalPages = result.TotalPages,
                    AvailableRoles = availableRoles
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading users list");
                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการโหลดรายการผู้ใช้";
                return View(new UserIndexViewModel());
            }
        }

        #endregion

        #region Details

        // GET: /Admin/User/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            try
            {
                var query = new GetUserDetailsQuery { Id = id };
                var userDetails = await _mediator.Send(query);

                if (userDetails == null)
                {
                    TempData["ErrorMessage"] = "ไม่พบผู้ใช้ที่ต้องการ";
                    return RedirectToAction(nameof(Index));
                }

                var model = new UserDetailsViewModel
                {
                    Id = userDetails.Id,
                    Email = userDetails.Email,
                    UserName = userDetails.UserName,
                    FirstName = userDetails.FirstName,
                    LastName = userDetails.LastName,
                    PhoneNumber = userDetails.PhoneNumber,
                    IsActive = userDetails.IsActive,
                    EmailConfirmed = userDetails.EmailConfirmed,
                    PhoneNumberConfirmed = userDetails.PhoneNumberConfirmed,
                    TwoFactorEnabled = userDetails.TwoFactorEnabled,
                    LockoutEnabled = userDetails.LockoutEnabled,
                    LockoutEnd = userDetails.LockoutEnd,
                    AccessFailedCount = userDetails.AccessFailedCount,
                    CreatedAt = userDetails.CreatedAt,
                    LastLoginAt = userDetails.LastLoginAt,
                    ProfileImageUrl = userDetails.ProfileImageUrl,
                    Bio = userDetails.Bio,
                    Roles = userDetails.Roles,
                    Claims = userDetails.Claims,
                    ExternalLogins = userDetails.ExternalLogins
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user details for ID: {UserId}", id);
                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการโหลดข้อมูลผู้ใช้";
                return RedirectToAction(nameof(Index));
            }
        }

        #endregion

        #region Create

        // GET: /Admin/User/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            try
            {
                var rolesQuery = new GetAllRolesQuery();
                var roles = await _mediator.Send(rolesQuery);

                var roleSelectList = roles.Select(r => new SelectListItem
                {
                    Value = r,
                    Text = r
                }).ToList();

                var model = new CreateUserViewModel
                {
                    AvailableRoles = roleSelectList,
                    IsActive = true
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create user page");
                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการโหลดหน้าสร้างผู้ใช้";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Admin/User/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var command = new CreateUserCommand
                    {
                        Email = model.Email,
                        Password = model.Password,
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        PhoneNumber = model.PhoneNumber,
                        IsActive = model.IsActive,
                        EmailConfirmed = model.EmailConfirmed,
                        SelectedRoles = model.SelectedRoles
                    };

                    var result = await _mediator.Send(command);

                    if (result.Success)
                    {
                        _logger.LogInformation("Admin created new user: {Email}", model.Email);
                        TempData["SuccessMessage"] = result.Message;
                        return RedirectToAction(nameof(Index));
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                ModelState.AddModelError(string.Empty, "เกิดข้อผิดพลาดในการสร้างผู้ใช้");
            }

            // Reload roles for dropdown on error
            var rolesQuery = new GetAllRolesQuery();
            var roles = await _mediator.Send(rolesQuery);
            model.AvailableRoles = roles.Select(r => new SelectListItem
            {
                Value = r,
                Text = r
            }).ToList();

            return View(model);
        }

        #endregion

        #region Edit

        // GET: /Admin/User/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            try
            {
                var userQuery = new GetUserDetailsQuery { Id = id };
                var userDetails = await _mediator.Send(userQuery);

                if (userDetails == null)
                {
                    TempData["ErrorMessage"] = "ไม่พบผู้ใช้ที่ต้องการแก้ไข";
                    return RedirectToAction(nameof(Index));
                }

                var rolesQuery = new GetAllRolesQuery();
                var allRoles = await _mediator.Send(rolesQuery);

                var roleSelectList = allRoles.Select(r => new SelectListItem
                {
                    Value = r,
                    Text = r,
                    Selected = userDetails.Roles.Contains(r)
                }).ToList();

                var model = new EditUserViewModel
                {
                    Id = userDetails.Id,
                    Email = userDetails.Email,
                    UserName = userDetails.UserName,
                    FirstName = userDetails.FirstName,
                    LastName = userDetails.LastName,
                    PhoneNumber = userDetails.PhoneNumber,
                    IsActive = userDetails.IsActive,
                    EmailConfirmed = userDetails.EmailConfirmed,
                    SelectedRoles = userDetails.Roles,
                    AvailableRoles = roleSelectList
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user for edit: {UserId}", id);
                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการโหลดข้อมูลผู้ใช้";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Admin/User/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, EditUserViewModel model)
        {
            try
            {
                if (id != model.Id)
                {
                    return BadRequest();
                }

                if (ModelState.IsValid)
                {
                    var command = new UpdateUserCommand
                    {
                        Id = model.Id,
                        Email = model.Email,
                        UserName = model.UserName,
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        PhoneNumber = model.PhoneNumber,
                        IsActive = model.IsActive,
                        EmailConfirmed = model.EmailConfirmed,
                        SelectedRoles = model.SelectedRoles
                    };

                    var result = await _mediator.Send(command);

                    if (result.Success)
                    {
                        _logger.LogInformation("Admin updated user: {Email}", model.Email);
                        TempData["SuccessMessage"] = result.Message;
                        return RedirectToAction(nameof(Details), new { id = model.Id });
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user: {UserId}", id);
                ModelState.AddModelError(string.Empty, "เกิดข้อผิดพลาดในการแก้ไขข้อมูลผู้ใช้");
            }

            // Reload roles for dropdown on error
            var rolesQuery = new GetAllRolesQuery();
            var allRoles = await _mediator.Send(rolesQuery);
            model.AvailableRoles = allRoles.Select(r => new SelectListItem
            {
                Value = r,
                Text = r,
                Selected = model.SelectedRoles?.Contains(r) ?? false
            }).ToList();

            return View(model);
        }

        #endregion

        #region Delete

        // POST: /Admin/User/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                // ป้องกันไม่ให้ลบ Admin ตัวเอง
                var currentUserId = User.FindFirst("sub")?.Value ?? User.FindFirst("id")?.Value;
                if (id == currentUserId)
                {
                    return Json(new { success = false, message = "ไม่สามารถลบบัญชีของตัวเองได้" });
                }

                var command = new DeleteUserCommand { Id = id };
                var result = await _mediator.Send(command);

                if (result.Success)
                {
                    _logger.LogInformation("Admin deleted user with ID: {UserId}", id);
                    return Json(new { success = true, message = result.Message });
                }

                return Json(new { success = false, message = string.Join(", ", result.Errors) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user: {UserId}", id);
                return Json(new { success = false, message = "เกิดข้อผิดพลาดในการลบผู้ใช้" });
            }
        }

        #endregion

        #region Toggle Status

        // POST: /Admin/User/ToggleStatus/5
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            try
            {
                // ป้องกันไม่ให้ปิดใช้งาน Admin ตัวเอง
                var currentUserId = User.FindFirst("sub")?.Value ?? User.FindFirst("id")?.Value;
                if (id == currentUserId)
                {
                    return Json(new { success = false, message = "ไม่สามารถปิดใช้งานบัญชีของตัวเองได้" });
                }

                var command = new ToggleUserStatusCommand { Id = id };
                var result = await _mediator.Send(command);

                if (result.Success)
                {
                    return Json(new
                    {
                        success = true,
                        message = result.Message,
                        isActive = result.IsActive
                    });
                }

                return Json(new { success = false, message = string.Join(", ", result.Errors) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling user status: {UserId}", id);
                return Json(new { success = false, message = "เกิดข้อผิดพลาดในการเปลี่ยนสถานะผู้ใช้" });
            }
        }

        #endregion

        #region Reset Password

        // POST: /Admin/User/ResetPassword/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string userId, string newPassword)
        {
            try
            {
                var command = new ResetUserPasswordCommand
                {
                    UserId = userId,
                    NewPassword = newPassword
                };

                var result = await _mediator.Send(command);

                if (result.Success)
                {
                    _logger.LogInformation("Admin reset password for user: {UserId}", userId);
                    return Json(new { success = true, message = result.Message });
                }

                return Json(new { success = false, message = string.Join(", ", result.Errors) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for user: {UserId}", userId);
                return Json(new { success = false, message = "เกิดข้อผิดพลาดในการรีเซ็ตรหัสผ่าน" });
            }
        }

        #endregion
    }
}