// BlogHybrid.Application/Handlers/Auth/LoginUserHandler.cs
using BlogHybrid.Application.Commands.Auth;
using BlogHybrid.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace BlogHybrid.Application.Handlers.Auth
{
    public class LoginUserHandler : IRequestHandler<LoginUserCommand, LoginUserResult>
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<LoginUserHandler> _logger;

        public LoginUserHandler(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ILogger<LoginUserHandler> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<LoginUserResult> Handle(LoginUserCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Login attempt for email: {Email}", request.Email);

                // ค้นหา user จาก email
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    _logger.LogWarning("Login failed: User not found for email {Email}", request.Email);
                    return new LoginUserResult
                    {
                        Success = false,
                        Errors = new List<string> { "อีเมลหรือรหัสผ่านไม่ถูกต้อง" }
                    };
                }

                // ตรวจสอบว่า user active หรือไม่
                if (!user.IsActive)
                {
                    _logger.LogWarning("Login failed: User account is inactive for {Email}", request.Email);
                    return new LoginUserResult
                    {
                        Success = false,
                        Errors = new List<string> { "บัญชีผู้ใช้ถูกระงับการใช้งาน" }
                    };
                }

                // ลองเข้าสู่ระบบ
                var result = await _signInManager.PasswordSignInAsync(
                    user.UserName!,
                    request.Password,
                    request.RememberMe,
                    lockoutOnFailure: true
                );

                if (result.Succeeded)
                {
                    // อัพเดท LastLoginAt
                    user.LastLoginAt = DateTime.UtcNow;
                    await _userManager.UpdateAsync(user);

                    _logger.LogInformation("User {Email} logged in successfully", request.Email);

                    return new LoginUserResult
                    {
                        Success = true,
                        Message = "เข้าสู่ระบบสำเร็จ",
                        RedirectUrl = !string.IsNullOrEmpty(request.ReturnUrl) ? request.ReturnUrl : "/"
                    };
                }

                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out for {Email}", request.Email);
                    return new LoginUserResult
                    {
                        Success = false,
                        IsLockedOut = true,
                        Errors = new List<string> { "บัญชีถูกล็อค กรุณาลองใหม่ภายหลัง" }
                    };
                }

                if (result.IsNotAllowed)
                {
                    _logger.LogWarning("User not allowed to sign in for {Email}", request.Email);
                    return new LoginUserResult
                    {
                        Success = false,
                        IsNotAllowed = true,
                        Errors = new List<string> { "บัญชียังไม่ได้รับการยืนยัน" }
                    };
                }

                if (result.RequiresTwoFactor)
                {
                    _logger.LogInformation("Two-factor authentication required for {Email}", request.Email);
                    return new LoginUserResult
                    {
                        Success = false,
                        RequiresTwoFactor = true,
                        Errors = new List<string> { "ต้องการการยืนยันสองขั้นตอน" }
                    };
                }

                // Login failed
                _logger.LogWarning("Login failed for {Email}: Invalid password", request.Email);
                return new LoginUserResult
                {
                    Success = false,
                    Errors = new List<string> { "อีเมลหรือรหัสผ่านไม่ถูกต้อง" }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during login for email: {Email}", request.Email);
                return new LoginUserResult
                {
                    Success = false,
                    Errors = new List<string> { "เกิดข้อผิดพลาดในระบบ กรุณาลองใหม่อีกครั้ง" }
                };
            }
        }
    }
}