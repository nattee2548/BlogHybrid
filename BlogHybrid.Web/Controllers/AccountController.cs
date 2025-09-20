// BlogHybrid.Web/Controllers/AccountController.cs
using BlogHybrid.Domain.Entities;
using BlogHybrid.Web.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BlogHybrid.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            // หากล็อกอินแล้วให้ redirect ไปหน้าแรก
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(new RegisterViewModel());
        }

        // POST: /Account/Register (HTMX + Regular Form Support)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            // ตรวจสอบว่าเป็น HTMX request หรือไม่
            var isHtmxRequest = Request.Headers.ContainsKey("HX-Request");

            _logger.LogInformation("Register POST: HTMX={IsHtmx}, ModelValid={ModelValid}",
                isHtmxRequest, ModelState.IsValid);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model validation failed: {Errors}",
                    string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));

                if (isHtmxRequest)
                {
                    Response.Headers.Append("HX-Retarget", "#register-form-container");
                    return PartialView("_RegisterValidation", model);
                }
                return View(model);
            }

            try
            {
                // ตรวจสอบว่า email ซ้ำหรือไม่
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "อีเมลนี้ถูกใช้งานแล้ว");
                    _logger.LogWarning("Duplicate email registration attempt: {Email}", model.Email);

                    if (isHtmxRequest)
                    {
                        Response.Headers.Append("HX-Retarget", "#register-form-container");
                        return PartialView("_RegisterValidation", model);
                    }
                    return View(model);
                }

                // สร้าง ApplicationUser ใหม่
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    DisplayName = model.DisplayName,
                    CreatedAt = DateTime.UtcNow,
                    LastLoginAt = DateTime.UtcNow,
                    IsActive = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // กำหนด role เป็น User
                    var roleResult = await _userManager.AddToRoleAsync(user, "User");
                    if (!roleResult.Succeeded)
                    {
                        _logger.LogWarning("Failed to assign User role to {Email}: {Errors}",
                            user.Email, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                    }

                    // Auto sign in the user
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    _logger.LogInformation("User {Email} registered successfully", user.Email);

                    if (isHtmxRequest)
                    {
                        // ส่งสัญญาณให้ HTMX รู้ว่าสำเร็จ
                        Response.Headers.Append("HX-Trigger", "registration-success");
                        Response.Headers.Append("HX-Retarget", "#register-form-container");

                        // ส่ง email ผ่าน ViewBag
                        ViewBag.Email = model.Email;
                        return PartialView("_RegisterSuccess");
                    }

                    TempData["SuccessMessage"] = "ยินดีต้อนรับสู่ 404talk.com! บัญชีของคุณถูกสร้างเรียบร้อยแล้ว";
                    return RedirectToAction("Index", "Home");
                }

                // หากมีข้อผิดพลาดจาก Identity
                foreach (var error in result.Errors)
                {
                    var localizedError = GetLocalizedErrorMessage(error.Code, error.Description);
                    ModelState.AddModelError(string.Empty, localizedError);
                    _logger.LogWarning("Identity error during registration: {Code} - {Description}",
                        error.Code, error.Description);
                }

                if (isHtmxRequest)
                {
                    Response.Headers.Append("HX-Retarget", "#register-form-container");
                    return PartialView("_RegisterValidation", model);
                }
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during user registration for email: {Email}", model.Email);

                if (isHtmxRequest)
                {
                    Response.Headers.Append("HX-Trigger", "registration-error");
                    Response.Headers.Append("HX-Retarget", "#register-form-container");
                    return PartialView("_RegisterError");
                }

                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในระบบ กรุณาลองใหม่อีกครั้ง";
                return View(model);
            }
        }

        // GET: /Account/Login (Placeholder)
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out");

            TempData["SuccessMessage"] = "ออกจากระบบเรียบร้อยแล้ว";
            return RedirectToAction("Index", "Home");
        }

        // Helper method สำหรับแปลข้อความ error
        private string GetLocalizedErrorMessage(string errorCode, string defaultMessage)
        {
            return errorCode switch
            {
                "DuplicateUserName" => "ชื่อผู้ใช้นี้ถูกใช้งานแล้ว",
                "DuplicateEmail" => "อีเมลนี้ถูกใช้งานแล้ว",
                "InvalidEmail" => "รูปแบบอีเมลไม่ถูกต้อง",
                "PasswordTooShort" => "รหัสผ่านต้องมีอย่างน้อย 6 ตัวอักษร",
                "PasswordRequiresDigit" => "รหัสผ่านต้องมีตัวเลข",
                "PasswordRequiresLower" => "รหัสผ่านต้องมีตัวอักษรพิมพ์เล็ก",
                "PasswordRequiresUpper" => "รหัสผ่านต้องมีตัวอักษรพิมพ์ใหญ่",
                "PasswordRequiresNonAlphanumeric" => "รหัสผ่านต้องมีอักขระพิเศษ",
                "PasswordMismatch" => "รหัสผ่านไม่ตรงกัน",
                "InvalidToken" => "โทเค็นไม่ถูกต้องหรือหมดอายุ",
                "UserAlreadyExists" => "ผู้ใช้นี้มีอยู่แล้ว",
                "UserNotFound" => "ไม่พบผู้ใช้",
                "InvalidUserName" => "ชื่อผู้ใช้ไม่ถูกต้อง",
                "LoginAlreadyAssociated" => "บัญชีนี้ถูกเชื่อมโยงแล้ว",
                "UserAlreadyInRole" => "ผู้ใช้มี role นี้อยู่แล้ว",
                "UserNotInRole" => "ผู้ใช้ไม่มี role นี้",
                "RoleNotFound" => "ไม่พบ role ที่ระบุ",
                "UserLockoutNotEnabled" => "การล็อกบัญชีไม่ได้เปิดใช้งาน",
                "TooManyFailedAttempts" => "ความพยายามล้มเหลวมากเกินไป บัญชีถูกล็อก",
                _ => defaultMessage
            };
        }
    }
}