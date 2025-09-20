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

        // POST: /Account/Register (HTMX)
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            // ตรวจสอบว่าเป็น HTMX request หรือไม่
            var isHtmxRequest = Request.Headers.ContainsKey("HX-Request");

            if (!ModelState.IsValid)
            {
                if (isHtmxRequest)
                {
                    // ส่งกลับ validation errors สำหรับ HTMX
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

                    if (isHtmxRequest)
                    {
                        return PartialView("_RegisterValidation", model);
                    }
                    return View(model);
                }

                // สร้าง ApplicationUser ใหม่ (ใช้ Entity ที่มีอยู่แล้ว)
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
                    // กำหนด role เป็น User (ใช้ role ที่ seed แล้ว)
                    await _userManager.AddToRoleAsync(user, "User");

                    _logger.LogInformation("ผู้ใช้ {Email} สร้างบัญชีใหม่สำเร็จ", user.Email);

                    if (isHtmxRequest)
                    {
                        // ส่งสัญญาณให้ HTMX รู้ว่าสำเร็จ
                        Response.Headers.Append("HX-Trigger", "registration-success");

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
                    ModelState.AddModelError(string.Empty, GetLocalizedErrorMessage(error.Code, error.Description));
                }

                if (isHtmxRequest)
                {
                    return PartialView("_RegisterValidation", model);
                }
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "เกิดข้อผิดพลาดในการสร้างบัญชีผู้ใช้");

                if (isHtmxRequest)
                {
                    Response.Headers.Append("HX-Trigger", "registration-error");
                    return PartialView("_RegisterError");
                }

                TempData["ErrorMessage"] = "เกิดข้อผิดพลาด กรุณาลองใหม่อีกครั้ง";
                return View(model);
            }
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
                _ => defaultMessage
            };
        }
    }
}