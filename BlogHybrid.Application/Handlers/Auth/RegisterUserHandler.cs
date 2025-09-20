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
                // ตรวจสอบว่ามี user ซ้ำหรือไม่
                var checkExists = await _mediator.Send(new CheckUserExistsQuery
                {
                    Email = request.Email,
                    DisplayName = request.DisplayName
                }, cancellationToken);

                var errors = new List<string>();

                if (checkExists.EmailExists)
                {
                    errors.Add("อีเมลนี้ถูกใช้งานแล้ว");
                }

                if (checkExists.DisplayNameExists)
                {
                    errors.Add("ชื่อที่แสดงนี้ถูกใช้งานแล้ว");
                }

                if (errors.Any())
                {
                    return new RegisterUserResult
                    {
                        Success = false,
                        Errors = errors
                    };
                }

                // สร้าง user ใหม่
                var user = new ApplicationUser
                {
                    UserName = request.Email,
                    Email = request.Email,
                    DisplayName = request.DisplayName,
                    CreatedAt = DateTime.UtcNow,
                    LastLoginAt = DateTime.UtcNow,
                    IsActive = true
                };

                _logger.LogInformation("Creating new user with email: {Email}", request.Email);
                var result = await _userManager.CreateAsync(user, request.Password);

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

                    return new RegisterUserResult
                    {
                        Success = true,
                        UserId = user.Id,
                        Message = "สมัครสมาชิกเรียบร้อยแล้ว"
                    };
                }

                // หากมีข้อผิดพลาดจาก Identity
                _logger.LogError("Identity errors during user creation:");
                var identityErrors = new List<string>();
                foreach (var error in result.Errors)
                {
                    _logger.LogError("Identity error: {Code} - {Description}", error.Code, error.Description);
                    var localizedError = GetLocalizedErrorMessage(error.Code, error.Description);
                    identityErrors.Add(localizedError);
                }

                return new RegisterUserResult
                {
                    Success = false,
                    Errors = identityErrors
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during user registration for email: {Email}", request.Email);
                return new RegisterUserResult
                {
                    Success = false,
                    Errors = new List<string> { "เกิดข้อผิดพลาดในระบบ กรุณาลองใหม่อีกครั้ง" }
                };
            }
        }

        private static string GetLocalizedErrorMessage(string errorCode, string defaultMessage)
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