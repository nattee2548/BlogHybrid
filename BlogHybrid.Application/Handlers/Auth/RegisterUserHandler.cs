// Path: BlogHybrid.Application/Handlers/Auth/RegisterUserHandler.cs
using BlogHybrid.Application.Commands.Auth;
using BlogHybrid.Application.Queries.Auth;
using BlogHybrid.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace BlogHybrid.Application.Handlers.Auth
{
    public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, RegisterUserResult>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMediator _mediator;
        private readonly ILogger<RegisterUserHandler> _logger;

        public RegisterUserHandler(
            UserManager<ApplicationUser> userManager,
            IMediator mediator,
            ILogger<RegisterUserHandler> logger)
        {
            _userManager = userManager;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<RegisterUserResult> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // 1. ตรวจสอบว่า Email และ DisplayName ซ้ำหรือไม่
                var checkExists = await _mediator.Send(new CheckUserExistsQuery
                {
                    Email = request.Email,
                    DisplayName = request.DisplayName
                }, cancellationToken);

                if (checkExists.EmailExists)
                {
                    return new RegisterUserResult
                    {
                        Success = false,
                        Errors = new List<string> { "อีเมลนี้ถูกใช้งานแล้ว" }
                    };
                }

                if (checkExists.DisplayNameExists)
                {
                    return new RegisterUserResult
                    {
                        Success = false,
                        Errors = new List<string> { "ชื่อที่แสดงนี้ถูกใช้งานแล้ว" }
                    };
                }

                //// 2. ตรวจสอบว่ายอมรับข้อกำหนดหรือไม่
                //if (!request.AcceptTerms)
                //{
                //    return new RegisterUserResult
                //    {
                //        Success = false,
                //        Errors = new List<string> { "กรุณายอมรับข้อกำหนดและเงื่อนไข" }
                //    };
                //}

                // 3. สร้าง User
                var user = new ApplicationUser
                {
                    UserName = request.Email,
                    Email = request.Email,
                    DisplayName = request.DisplayName,
                    FirstName = request.FirstName ?? string.Empty,
                    LastName = request.LastName ?? string.Empty,
                    PhoneNumber = request.PhoneNumber,
                    EmailConfirmed = false, // ต้อง verify email
                    IsActive = request.IsActive, // ✨ ใช้ค่าจาก Command (Admin = false, User = true)
                    CreatedAt = DateTime.UtcNow,
                    LastLoginAt = DateTime.UtcNow
                };

                // 4. สร้าง User พร้อมรหัสผ่าน
                var result = await _userManager.CreateAsync(user, request.Password);

                if (!result.Succeeded)
                {
                    return new RegisterUserResult
                    {
                        Success = false,
                        Errors = result.Errors.Select(e => e.Description).ToList()
                    };
                }

                // 5. ✨ เพิ่ม Role ตามที่ระบุใน Command (แทนที่จะ hardcode "User")
                var roleResult = await _userManager.AddToRoleAsync(user, request.Role);

                if (!roleResult.Succeeded)
                {
                    _logger.LogWarning("Failed to add {Role} role to {Email}: {Errors}",
                        request.Role, user.Email, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                }

                // 6. TODO: ส่ง Email Verification (optional)
                // var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                // await _emailService.SendVerificationEmailAsync(user.Email, token);

                _logger.LogInformation("User registered successfully: {Email} with role {Role} (IsActive: {IsActive})", 
                    user.Email, request.Role, user.IsActive);

                return new RegisterUserResult
                {
                    Success = true,
                    UserId = user.Id,
                    Message = user.IsActive 
                        ? "สมัครสมาชิกเรียบร้อยแล้ว กรุณาเข้าสู่ระบบ" 
                        : "สมัครสมาชิกเรียบร้อยแล้ว กรุณารอการอนุมัติจากผู้ดูแลระบบ"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration: {Email}", request.Email);
                return new RegisterUserResult
                {
                    Success = false,
                    Errors = new List<string> { "เกิดข้อผิดพลาดในระบบ กรุณาลองใหม่อีกครั้ง" }
                };
            }
        }
    }
}
