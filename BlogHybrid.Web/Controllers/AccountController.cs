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
            _logger.LogInformation("GET Register action called");

            // หากล็อกอินแล้วให้ redirect ไปหน้าแรก
            if (User.Identity?.IsAuthenticated == true)
            {
                _logger.LogInformation("User already authenticated, redirecting to home");
                return RedirectToAction("Index", "Home");
            }

            var model = new RegisterViewModel();
            _logger.LogInformation("Returning Register view with empty model");
            return View(model);
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            var isHtmxRequest = Request.Headers.ContainsKey("HX-Request");

            //_logger.LogInformation("POST Register: HTMX={IsHtmx}, ModelValid={IsValid}",
            //    isHtmxRequest, ModelState.IsValid);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model validation failed");
                foreach (var error in ModelState)
                {
                    _logger.LogWarning("Validation error in {Key}: {Errors}",
                        error.Key, string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage)));
                }

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
                    _logger.LogWarning("Duplicate email registration attempt: {Email}", model.Email);
                    ModelState.AddModelError("Email", "อีเมลนี้ถูกใช้งานแล้ว");

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

                _logger.LogInformation("Creating new user with email: {Email}", model.Email);
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User {Email} created successfully", user.Email);

                    // กำหนด role เป็น User
                    var roleResult = await _userManager.AddToRoleAsync(user, "User");
                    if (!roleResult.Succeeded)
                    {
                        _logger.LogWarning("Failed to assign User role to {Email}: {Errors}",
                            user.Email, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                    }

                    // ไม่ auto sign in - ให้ไป login page
                    _logger.LogInformation("User {Email} registration completed, redirecting to login", user.Email);

                    if (isHtmxRequest)
                    {
                        _logger.LogInformation("Returning HTMX success response");
                        Response.Headers.Append("HX-Trigger", "registration-success");
                        Response.Headers.Append("HX-Retarget", "#register-form-container");
                        ViewBag.Email = model.Email;
                        ViewBag.RedirectToLogin = true;
                        return PartialView("_RegisterSuccess");
                    }

                    TempData["SuccessMessage"] = "สมัครสมาชิกเรียบร้อยแล้ว! กรุณาเข้าสู่ระบบ";
                    return RedirectToAction("Login", "Account");
                }

                // หากมีข้อผิดพลาดจาก Identity
                _logger.LogError("Identity errors during user creation:");
                foreach (var error in result.Errors)
                {
                    _logger.LogError("Identity error: {Code} - {Description}", error.Code, error.Description);
                    var localizedError = GetLocalizedErrorMessage(error.Code, error.Description);
                    ModelState.AddModelError(string.Empty, localizedError);
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

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            _logger.LogInformation("GET Login action called");

            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;

            // สร้าง simple view สำหรับ Login
            return Content(@"
<!DOCTYPE html>
<html>
<head>
    <title>เข้าสู่ระบบ - 404talk.com</title>
    <link href='https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css' rel='stylesheet'>
</head>
<body>
    <div class='container mt-5'>
        <div class='row justify-content-center'>
            <div class='col-md-6'>
                <div class='card'>
                    <div class='card-header'>
                        <h4>เข้าสู่ระบบ</h4>
                    </div>
                    <div class='card-body'>
                        <p>หน้าเข้าสู่ระบบยังไม่พร้อม</p>
                        <a href='" + Url.Action("Register", "Account") + @"' class='btn btn-primary'>สมัครสมาชิก</a>
                        <a href='" + Url.Action("Index", "Home") + @"' class='btn btn-secondary'>กลับหน้าแรก</a>
                    </div>
                </div>
            </div>
        </div>
    </div>
</body>
</html>", "text/html");
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