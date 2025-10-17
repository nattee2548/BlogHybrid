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

    // GET: /Account/AdminLogin
    [HttpGet]
    public async Task<IActionResult> AdminLogin()
    {
        // ✅ ถ้า Login อยู่แล้ว → Redirect ไป Dashboard ตาม Role
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

                // ถ้าไม่ใช่ Admin → ไป Home
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
            // 1. ส่ง Login Command
            var command = new LoginUserCommand
            {
                Email = model.Email,
                Password = model.Password,
                RememberMe = model.RememberMe
            };

            var result = await _mediator.Send(command);

            // 2. ตรวจสอบผลลัพธ์
            if (!result.Success)
            {
                // Login ไม่สำเร็จ
                TempData["ErrorMessage"] = result.Errors?.FirstOrDefault() ?? "อีเมลหรือรหัสผ่านไม่ถูกต้อง";
                _logger.LogWarning($"Failed login attempt for: {model.Email}");
                return View(model);
            }

            // 3. Login สำเร็จ - ตรวจสอบว่าเป็น Admin หรือไม่
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                TempData["ErrorMessage"] = "ไม่พบข้อมูลผู้ใช้";
                await _signInManager.SignOutAsync();
                return View(model);
            }

            // 4. ตรวจสอบ Role Admin
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            if (!isAdmin)
            {
                TempData["ErrorMessage"] = "คุณไม่มีสิทธิ์เข้าถึงระบบ Admin";
                await _signInManager.SignOutAsync();
                _logger.LogWarning($"Non-admin user tried to access admin panel: {model.Email}");
                return View(model);
            }

            // 5. ตรวจสอบว่าบัญชี Active หรือไม่
            if (!user.IsActive)
            {
                TempData["WarningMessage"] = "บัญชีของคุณยังไม่ได้รับการอนุมัติ กรุณารอการอนุมัติจากผู้ดูแลระบบ";
                await _signInManager.SignOutAsync();
                _logger.LogWarning($"Inactive admin tried to login: {model.Email}");
                return View(model);
            }

            // 6. ✅ Login สำเร็จและเป็น Admin ที่ Active → Redirect ไป Admin Dashboard
            TempData["SuccessMessage"] = "เข้าสู่ระบบสำเร็จ! ยินดีต้อนรับ";
            _logger.LogInformation($"Admin logged in successfully: {model.Email}");

            return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "เกิดข้อผิดพลาด กรุณาลองใหม่อีกครั้ง";
            _logger.LogError(ex, $"Error during login for: {model.Email}");
            return View(model);
        }
    }

    // GET: /Account/AdminRegister
    [HttpGet]
    public async Task<IActionResult> AdminRegister()
    {
        // ✅ ถ้า Login อยู่แล้ว → Redirect ไป Dashboard ตาม Role
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

                // ถ้าไม่ใช่ Admin → ไป Home
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
           // ModelState.AddModelError(nameof(model.AcceptTerms), "กรุณายอมรับข้อกำหนดและเงื่อนไข");
            TempData["ErrorMessage"] = "กรุณายอมรับข้อกำหนดและเงื่อนไข";
            return View(model);
        }
        if (!ModelState.IsValid)
        {
            return View(model);
        }
       
        try
        {
            // 1. สมัครสมาชิกด้วย RegisterUserCommand
            var command = new RegisterUserCommand
            {
                Email = model.Email,
                Password = model.Password,
                ConfirmPassword = model.ConfirmPassword,
                DisplayName = model.DisplayName,
                FirstName = model.FirstName,
                LastName = model.LastName,
                PhoneNumber = model.PhoneNumber,
                Role = "Admin",  //
                IsActive = false  //
            };

            var result = await _mediator.Send(command);

            if (!result.Success)
            {
                ModelState.Remove(nameof(model.AcceptTerms));
                TempData["ErrorMessage"] = result.Errors?.FirstOrDefault() ?? "ไม่สามารถสมัครสมาชิกได้";
                _logger.LogWarning($"Failed registration for: {model.Email}");
                return View(model);
            }

            // 2. แสดงข้อความสำเร็จ
            TempData["SuccessMessage"] = "สมัครสมาชิกสำเร็จ! กรุณารอการอนุมัติจากผู้ดูแลระบบ";
            _logger.LogInformation($"New admin registered: {model.Email} (Pending approval)");

            return RedirectToAction("AdminLogin");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "เกิดข้อผิดพลาด กรุณาลองใหม่อีกครั้ง";
            _logger.LogError(ex, $"Error during registration for: {model.Email}");
            return View(model);
        }
    }

    // POST: /Account/Logout
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        TempData["InfoMessage"] = "ออกจากระบบเรียบร้อยแล้ว";
        _logger.LogInformation("User logged out");
        return RedirectToAction("AdminLogin");
    }
}