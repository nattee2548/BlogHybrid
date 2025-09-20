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
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            IMediator mediator,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AccountController> logger)
        {
            _mediator = mediator;
            _signInManager = signInManager;
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
                return RedirectToAction("Index", "Home");
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

                if (isHtmxRequest)
                {
                    Response.Headers.Append("HX-Trigger", "login-success");
                    Response.Headers.Append("HX-Redirect", result.RedirectUrl ?? "/");
                    ViewBag.Email = model.Email;
                    ViewBag.RedirectUrl = result.RedirectUrl;
                    return PartialView("_LoginSuccess");
                }

                TempData["SuccessMessage"] = "ยินดีต้อนรับกลับมา!";
                return LocalRedirect(result.RedirectUrl ?? "/");
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

        #region Helpers

        // GET: /Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // Helper method to redirect to local URLs only
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