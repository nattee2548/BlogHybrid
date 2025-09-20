using BlogHybrid.Application.Commands.Auth;
using BlogHybrid.Web.Models.ViewModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BlogHybrid.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IMediator mediator, ILogger<AccountController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

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

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
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
    }
}