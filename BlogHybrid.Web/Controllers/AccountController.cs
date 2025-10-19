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

    #region User Login & Register (Modal)

    // POST: /Account/Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        // ถ้า ModelState ไม่ valid → กลับไปพร้อมเปิด Modal
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "กรุณากรอกข้อมูลให้ครบถ้วน";
            TempData["OpenLoginModal"] = true; // ✨ Flag สำหรับเปิด Modal
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
                TempData["ErrorMessage"] = result.Errors?.FirstOrDefault() ?? "อีเมลหรือรหัสผ่านไม่ถูกต้อง";
                TempData["OpenLoginModal"] = true; // ✨ เปิด Modal อีกครั้ง
                _logger.LogWarning($"Failed login attempt for: {model.Email}");
                return RedirectToAction("Index", "Home");
            }

            _logger.LogInformation($"User logged in: {model.Email}");
            TempData["SuccessMessage"] = "เข้าสู่ระบบสำเร็จ!";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "เกิดข้อผิดพลาด กรุณาลองใหม่อีกครั้ง";
            TempData["OpenLoginModal"] = true;
            _logger.LogError(ex, $"Error during login for: {model.Email}");
            return RedirectToAction("Index", "Home");
        }
    }

    // POST: /Account/Register
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        // ตรวจสอบ AcceptTerms
        if (!model.AcceptTerms)
        {
            TempData["ErrorMessage"] = "กรุณายอมรับข้อกำหนดและเงื่อนไข";
            TempData["OpenRegisterModal"] = true; // ✨ Flag สำหรับเปิด Modal
            return RedirectToAction("Index", "Home");
        }

        // ตรวจสอบ ModelState
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .FirstOrDefault();

            TempData["ErrorMessage"] = errors ?? "กรุณากรอกข้อมูลให้ถูกต้อง";
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
                TempData["ErrorMessage"] = result.Errors?.FirstOrDefault() ?? "ไม่สามารถสมัครสมาชิกได้";
                TempData["OpenRegisterModal"] = true;
                _logger.LogWarning($"Failed registration for: {model.Email}");
                return RedirectToAction("Index", "Home");
            }

            TempData["SuccessMessage"] = "สมัครสมาชิกสำเร็จ! กรุณาเข้าสู่ระบบ";
            TempData["OpenLoginModal"] = true; // ✨ เปิด Login Modal
            _logger.LogInformation($"New user registered: {model.Email}");

            return RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "เกิดข้อผิดพลาด กรุณาลองใหม่อีกครั้ง";
            TempData["OpenRegisterModal"] = true;
            _logger.LogError(ex, $"Error during registration for: {model.Email}");
            return RedirectToAction("Index", "Home");
        }
    }

    #endregion

    #region Admin Login & Register

    // ... เก็บโค้ด Admin เดิมไว้ตามเดิม ...

    #endregion

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
}