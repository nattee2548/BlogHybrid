// BlogHybrid.Web/Controllers/AccountController.cs
using BlogHybrid.Application.Commands.Auth;
using BlogHybrid.Domain.Entities;
using BlogHybrid.Web.Models.ViewModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BlogHybrid.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IMediator _mediator;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            IMediator mediator,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ILogger<AccountController> logger)
        {
            _mediator = mediator;
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        #region Register

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(new RegisterViewModel());
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            var isHtmxRequest = Request.Headers.ContainsKey("HX-Request");

            _logger.LogInformation("POST Register: HTMX={IsHtmx}, ModelValid={IsValid}",
                isHtmxRequest, ModelState.IsValid);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model validation failed");

                if (isHtmxRequest)
                {
                    Response.Headers.Append("HX-Retarget", "#register-form-container");
                    return PartialView("_RegisterValidation", model);
                }
                return View(model);
            }

            // ใช้ CQRS pattern
            var command = new RegisterUserCommand
            {
                DisplayName = model.DisplayName,
                Email = model.Email,
                Password = model.Password,
                AcceptTerms = model.AcceptTerms
            };

            var result = await _mediator.Send(command);

            if (result.Success)
            {
                _logger.LogInformation("User registration successful for email: {Email}", model.Email);

                if (isHtmxRequest)
                {
                    Response.Headers.Append("HX-Trigger", "registration-success");
                    Response.Headers.Append("HX-Retarget", "#register-form-container");
                    ViewBag.Email = model.Email;
                    return PartialView("_RegisterSuccess");
                }

                TempData["SuccessMessage"] = "สมัครสมาชิกเรียบร้อยแล้ว! กรุณาเข้าสู่ระบบ";
                return RedirectToAction("Login", "Account");
            }

            // มี errors
            foreach (var error in result.Errors)
            {
                if (error.Contains("อีเมล"))
                {
                    ModelState.AddModelError("Email", error);
                }
                else if (error.Contains("ชื่อที่แสดง"))
                {
                    ModelState.AddModelError("DisplayName", error);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, error);
                }
            }

            if (isHtmxRequest)
            {
                Response.Headers.Append("HX-Retarget", "#register-form-container");
                return PartialView("_RegisterValidation", model);
            }

            return View(model);
        }

        #endregion

        #region Login

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToUserDashboard();
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            var isHtmxRequest = Request.Headers.ContainsKey("HX-Request");

            _logger.LogInformation("POST Login: HTMX={IsHtmx}, ModelValid={IsValid}",
                isHtmxRequest, ModelState.IsValid);

            if (!ModelState.IsValid)
            {
                if (isHtmxRequest)
                {
                    Response.Headers.Append("HX-Retarget", "#login-form-container");
                    return PartialView("_LoginValidation", model);
                }
                return View(model);
            }

            // ใช้ CQRS pattern
            var command = new LoginUserCommand
            {
                Email = model.Email,
                Password = model.Password,
                RememberMe = model.RememberMe,
                ReturnUrl = model.ReturnUrl
            };

            var result = await _mediator.Send(command);

            if (result.Success)
            {
                _logger.LogInformation("User login successful for email: {Email}", model.Email);

                // *** ส่วนใหม่: Role-based Redirect ***
                var redirectUrl = await GetRoleBasedRedirectUrl(model.Email, model.ReturnUrl);

                if (isHtmxRequest)
                {
                    Response.Headers.Append("HX-Trigger", "login-success");
                    Response.Headers.Append("HX-Redirect", redirectUrl);
                    ViewBag.Email = model.Email;
                    ViewBag.RedirectUrl = redirectUrl;
                    return PartialView("_LoginSuccess");
                }

                TempData["SuccessMessage"] = "ยินดีต้อนรับกลับมา!";
                return LocalRedirect(redirectUrl);
            }

            // มี errors
            foreach (var error in result.Errors)
            {
                if (error.Contains("อีเมล") || error.Contains("รหัสผ่าน"))
                {
                    ModelState.AddModelError(string.Empty, error);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, error);
                }
            }

            if (isHtmxRequest)
            {
                Response.Headers.Append("HX-Retarget", "#login-form-container");
                return PartialView("_LoginValidation", model);
            }

            return View(model);
        }

        #endregion

        #region Logout

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var userEmail = User?.Identity?.Name;
            await _signInManager.SignOutAsync();

            _logger.LogInformation("User {Email} logged out", userEmail);

            TempData["SuccessMessage"] = "ออกจากระบบเรียบร้อยแล้ว";
            return RedirectToAction("Index", "Home");
        }

        #endregion

        #region Helper Methods

        // GET: /Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        /// <summary>
        /// กำหนด URL ปลายทางตาม Role ของผู้ใช้
        /// </summary>
        private async Task<string> GetRoleBasedRedirectUrl(string email, string? returnUrl = null)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    _logger.LogWarning("User not found for email: {Email}", email);
                    return "/";
                }

                var roles = await _userManager.GetRolesAsync(user);
                _logger.LogInformation("User {Email} has roles: {Roles}", email, string.Join(", ", roles));

                // ถ้ามี returnUrl และเป็น local URL ให้ใช้
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    _logger.LogInformation("Using returnUrl: {ReturnUrl}", returnUrl);
                    return returnUrl;
                }

                // Role-based redirect (เรียงตาม priority)
                if (roles.Contains("Admin"))
                {
                    _logger.LogInformation("Redirecting Admin to dashboard");
                    return "/Admin/Dashboard";
                }
                else if (roles.Contains("Moderator"))
                {
                    _logger.LogInformation("Redirecting Moderator to admin area");
                    return "/Admin/Moderation";
                }
                else
                {
                    _logger.LogInformation("Redirecting User to home page");
                    return "/";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error determining redirect URL for user: {Email}", email);
                return "/";
            }
        }

        /// <summary>
        /// Redirect ผู้ใช้ที่ login แล้วไปยังหน้าที่เหมาะสม
        /// </summary>
        private IActionResult RedirectToUserDashboard()
        {
            try
            {
                if (User.IsInRole("Admin"))
                {
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                }
                else if (User.IsInRole("Moderator"))
                {
                    return RedirectToAction("Index", "Moderation", new { area = "Admin" });
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error redirecting authenticated user");
                return RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// Helper method สำหรับ redirect ไปยัง local URLs เท่านั้น
        /// </summary>
        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        #endregion
    }
}