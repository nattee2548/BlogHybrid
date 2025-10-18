using BlogHybrid.Application.Commands.User;
using BlogHybrid.Application.Interfaces.Services;
using BlogHybrid.Application.Queries.User;
using BlogHybrid.Domain.Entities;
using BlogHybrid.Web.Areas.User.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BlogHybrid.Web.Areas.User.Controllers
{
    [Area("User")]
    [Authorize] // ต้อง login เท่านั้น
    public class ProfileController : Controller
    {
        private readonly IMediator _mediator;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IImageService _imageService;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(
            IMediator mediator,
            UserManager<ApplicationUser> userManager,
            IImageService imageService,
            ILogger<ProfileController> logger)
        {
            _mediator = mediator;
            _userManager = userManager;
            _imageService = imageService;
            _logger = logger;
        }

        // GET: /User/Profile
        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account", new { area = "" });
                }

                var query = new GetUserDetailsQuery { Id = userId };
                var user = await _mediator.Send(query);

                if (user == null)
                {
                    return NotFound();
                }

                var viewModel = new ProfileViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    DisplayName = user.DisplayName,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = user.PhoneNumber,
                    Bio = user.Bio,
                    ProfileImageUrl = user.ProfileImageUrl,
                    EmailConfirmed = user.EmailConfirmed,
                    CreatedAt = user.CreatedAt,
                    LastLoginAt = user.LastLoginAt,
                    //PostCount = user.PostCount,
                    //CommentCount = user.CommentCount,
                    //CommunityCount = user.CommunityCount
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user profile");
                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการโหลดโปรไฟล์";
                return RedirectToAction("Index", "Home", new { area = "" });
            }
        }

        // GET: /User/Profile/Edit
        public async Task<IActionResult> Edit()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account", new { area = "" });
                }

                var query = new GetUserDetailsQuery { Id = userId };
                var user = await _mediator.Send(query);

                if (user == null)
                {
                    return NotFound();
                }

                var viewModel = new EditProfileViewModel
                {
                    Id = user.Id,
                    DisplayName = user.DisplayName,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = user.PhoneNumber,
                    Bio = user.Bio,
                    CurrentProfileImageUrl = user.ProfileImageUrl
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading edit profile page");
                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการโหลดหน้าแก้ไขโปรไฟล์";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /User/Profile/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId) || userId != model.Id)
                {
                    return Forbid();
                }

                string? profileImageUrl = model.CurrentProfileImageUrl;

                // Upload รูปโปรไฟล์ใหม่ถ้ามี
                if (model.ProfileImage != null)
                {
                    try
                    {
                        // ลบรูปเดิมถ้ามี
                        if (!string.IsNullOrEmpty(model.CurrentProfileImageUrl))
                        {
                            await _imageService.DeleteAsync(model.CurrentProfileImageUrl);
                        }

                        // Upload รูปใหม่
                        var uploadedPath = await _imageService.UploadAsync(model.ProfileImage, "profiles");
                        profileImageUrl = _imageService.GetImageUrl(uploadedPath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error uploading profile image");
                        TempData["WarningMessage"] = "อัปโหลดรูปภาพไม่สำเร็จ แต่ยังคงอัปเดตข้อมูลอื่น";
                        profileImageUrl = model.CurrentProfileImageUrl;
                    }
                }

                var command = new UpdateUserProfileCommand
                {
                    UserId = userId,
                    DisplayName = model.DisplayName,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNumber = model.PhoneNumber,
                    Bio = model.Bio,
                    ProfileImageUrl = profileImageUrl
                };

                var result = await _mediator.Send(command);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = "อัปเดตโปรไฟล์สำเร็จ";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ErrorMessage"] = result.Errors?.FirstOrDefault() ?? "ไม่สามารถอัปเดตโปรไฟล์ได้";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile");
                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการอัปเดตโปรไฟล์";
                return View(model);
            }
        }

        // GET: /User/Profile/ChangePassword
        public IActionResult ChangePassword()
        {
            return View();
        }

        // POST: /User/Profile/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login", "Account", new { area = "" });
                }

                var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "เปลี่ยนรหัสผ่านสำเร็จ";
                    _logger.LogInformation($"User {user.Email} changed password");
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    TempData["ErrorMessage"] = "ไม่สามารถเปลี่ยนรหัสผ่านได้";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการเปลี่ยนรหัสผ่าน";
                return View(model);
            }
        }
    }
}