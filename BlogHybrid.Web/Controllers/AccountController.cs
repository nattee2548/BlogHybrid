using BlogHybrid.Application.Commands.Auth;
using BlogHybrid.Domain.Entities;
using BlogHybrid.Web.Models.Account;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BlogHybrid.Web.Controllers;

public class AccountController : Controller
{
    private readonly IMediator _mediator;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        IMediator mediator,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ILogger<AccountController> logger)
    {
        _mediator = mediator;
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    #region User Login & Register (AJAX Modal)

    // POST: /Account/Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        // ตรวจสอบว่าเป็น AJAX request
        bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

        if (!ModelState.IsValid)
        {
            if (isAjax)
            {
                return Json(new { success = false, message = "กรุณากรอกข้อมูลให้ครบถ้วน" });
            }

            TempData["ErrorMessage"] = "กรุณากรอกข้อมูลให้ครบถ้วน";
            TempData["OpenLoginModal"] = true;
            return RedirectToAction("Index", "Home");
        }

        try
        {
            var command = new LoginUserCommand
            {
                Email = model.Email,
                Password = model.Password,
                RememberMe = model.RememberMe
            };

            var result = await _mediator.Send(command);

            if (!result.Success)
            {
                var errorMessage = result.Errors?.FirstOrDefault() ?? "อีเมลหรือรหัสผ่านไม่ถูกต้อง";
                _logger.LogWarning($"Failed login attempt for: {model.Email}");

                if (isAjax)
                {
                    return Json(new { success = false, message = errorMessage });
                }

                TempData["ErrorMessage"] = errorMessage;
                TempData["OpenLoginModal"] = true;
                return RedirectToAction("Index", "Home");
            }

            _logger.LogInformation($"User logged in: {model.Email}");

            var redirectUrl = "/";
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                redirectUrl = returnUrl;
            }

            if (isAjax)
            {
                return Json(new
                {
                    success = true,
                    message = "เข้าสู่ระบบสำเร็จ!",
                    redirectUrl = redirectUrl
                });
            }

            TempData["SuccessMessage"] = "เข้าสู่ระบบสำเร็จ!";
            return Redirect(redirectUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error during login for: {model.Email}");

            if (isAjax)
            {
                return Json(new { success = false, message = "เกิดข้อผิดพลาด กรุณาลองใหม่อีกครั้ง" });
            }

            TempData["ErrorMessage"] = "เกิดข้อผิดพลาด กรุณาลองใหม่อีกครั้ง";
            TempData["OpenLoginModal"] = true;
            return RedirectToAction("Index", "Home");
        }
    }

    // POST: /Account/Register
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

        if (!model.AcceptTerms)
        {
            if (isAjax)
            {
                return Json(new { success = false, message = "กรุณายอมรับข้อกำหนดและเงื่อนไข" });
            }

            TempData["ErrorMessage"] = "กรุณายอมรับข้อกำหนดและเงื่อนไข";
            TempData["OpenRegisterModal"] = true;
            return RedirectToAction("Index", "Home");
        }

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .FirstOrDefault();

            var errorMessage = errors ?? "กรุณากรอกข้อมูลให้ถูกต้อง";

            if (isAjax)
            {
                return Json(new { success = false, message = errorMessage });
            }

            TempData["ErrorMessage"] = errorMessage;
            TempData["OpenRegisterModal"] = true;
            return RedirectToAction("Index", "Home");
        }

        try
        {
            var command = new RegisterUserCommand
            {
                Email = model.Email,
                Password = model.Password,
                ConfirmPassword = model.ConfirmPassword,
                DisplayName = model.DisplayName,
                FirstName = null,
                LastName = null,
                PhoneNumber = null,
                Role = "User",
                IsActive = true
            };

            var result = await _mediator.Send(command);

            if (!result.Success)
            {
                var errorMessage = result.Errors?.FirstOrDefault() ?? "ไม่สามารถสมัครสมาชิกได้";
                _logger.LogWarning($"Failed registration for: {model.Email}");

                if (isAjax)
                {
                    return Json(new { success = false, message = errorMessage });
                }

                TempData["ErrorMessage"] = errorMessage;
                TempData["OpenRegisterModal"] = true;
                return RedirectToAction("Index", "Home");
            }

            _logger.LogInformation($"New user registered: {model.Email}");

            if (isAjax)
            {
                return Json(new
                {
                    success = true,
                    message = "สมัครสมาชิกสำเร็จ! กรุณาเข้าสู่ระบบ",
                    openLogin = true
                });
            }

            TempData["SuccessMessage"] = "สมัครสมาชิกสำเร็จ! กรุณาเข้าสู่ระบบ";
            TempData["OpenLoginModal"] = true;
            return RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error during registration for: {model.Email}");

            if (isAjax)
            {
                return Json(new { success = false, message = "เกิดข้อผิดพลาด กรุณาลองใหม่อีกครั้ง" });
            }

            TempData["ErrorMessage"] = "เกิดข้อผิดพลาด กรุณาลองใหม่อีกครั้ง";
            TempData["OpenRegisterModal"] = true;
            return RedirectToAction("Index", "Home");
        }
    }

    #endregion

    #region Admin Login & Register

    // GET: /Account/AdminLogin
    [HttpGet]
    public async Task<IActionResult> AdminLogin()
    {
        // ถ้า Login อยู่แล้ว → Redirect ตาม Role
        if (_signInManager.IsSignedIn(User))
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
                if (isAdmin)
                {
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                }

                return RedirectToAction("Index", "Home");
            }
        }

        return View();
    }

    // POST: /Account/AdminLogin
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AdminLogin(AdminLoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var command = new LoginUserCommand
            {
                Email = model.Email,
                Password = model.Password,
                RememberMe = model.RememberMe
            };

            var result = await _mediator.Send(command);

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.Errors?.FirstOrDefault() ?? "อีเมลหรือรหัสผ่านไม่ถูกต้อง";
                _logger.LogWarning($"Failed admin login attempt for: {model.Email}");
                return View(model);
            }

            // ตรวจสอบว่าเป็น Admin หรือไม่
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                TempData["ErrorMessage"] = "ไม่พบข้อมูลผู้ใช้";
                await _signInManager.SignOutAsync();
                return View(model);
            }

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            if (!isAdmin)
            {
                TempData["ErrorMessage"] = "คุณไม่มีสิทธิ์เข้าถึงระบบ Admin";
                await _signInManager.SignOutAsync();
                _logger.LogWarning($"Non-admin user tried to access admin panel: {model.Email}");
                return View(model);
            }

            // ตรวจสอบสถานะ Active
            if (!user.IsActive)
            {
                TempData["WarningMessage"] = "บัญชีของคุณยังไม่ได้รับการอนุมัติ กรุณารอการอนุมัติจากผู้ดูแลระบบ";
                await _signInManager.SignOutAsync();
                _logger.LogWarning($"Inactive admin tried to login: {model.Email}");
                return View(model);
            }

            TempData["SuccessMessage"] = "เข้าสู่ระบบสำเร็จ! ยินดีต้อนรับ";
            _logger.LogInformation($"Admin logged in successfully: {model.Email}");

            return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "เกิดข้อผิดพลาด กรุณาลองใหม่อีกครั้ง";
            _logger.LogError(ex, $"Error during admin login for: {model.Email}");
            return View(model);
        }
    }

    // GET: /Account/AdminRegister
    [HttpGet]
    public async Task<IActionResult> AdminRegister()
    {
        // ถ้า Login อยู่แล้ว → Redirect ตาม Role
        if (_signInManager.IsSignedIn(User))
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
                if (isAdmin)
                {
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                }

                return RedirectToAction("Index", "Home");
            }
        }

        return View();
    }

    // POST: /Account/AdminRegister
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AdminRegister(AdminRegisterViewModel model)
    {
        if (!model.AcceptTerms)
        {
            TempData["ErrorMessage"] = "กรุณายอมรับข้อกำหนดและเงื่อนไข";
            return View(model);
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var command = new RegisterUserCommand
            {
                Email = model.Email,
                Password = model.Password,
                ConfirmPassword = model.ConfirmPassword,
                DisplayName = model.DisplayName,
                FirstName = model.FirstName,
                LastName = model.LastName,
                PhoneNumber = model.PhoneNumber,
                Role = "Admin",
                IsActive = false  // Admin ต้องรออนุมัติ
            };

            var result = await _mediator.Send(command);

            if (!result.Success)
            {
                ModelState.Remove(nameof(model.AcceptTerms));
                TempData["ErrorMessage"] = result.Errors?.FirstOrDefault() ?? "ไม่สามารถสมัครสมาชิกได้";
                _logger.LogWarning($"Failed admin registration for: {model.Email}");
                return View(model);
            }

            TempData["SuccessMessage"] = "สมัครสมาชิกสำเร็จ! กรุณารอการอนุมัติจากผู้ดูแลระบบ";
            _logger.LogInformation($"New admin registered: {model.Email} (Pending approval)");

            return RedirectToAction("AdminLogin");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "เกิดข้อผิดพลาด กรุณาลองใหม่อีกครั้ง";
            _logger.LogError(ex, $"Error during admin registration for: {model.Email}");
            return View(model);
        }
    }

    #endregion

    #region Logout

    // POST: /Account/Logout
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        TempData["SuccessMessage"] = "ออกจากระบบเรียบร้อยแล้ว";
        _logger.LogInformation("User logged out");
        return RedirectToAction("Index", "Home");
    }

    #endregion
}