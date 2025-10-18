// Path: BlogHybrid.Application/Handlers/User/UpdateUserProfileHandler.cs
using BlogHybrid.Application.Commands.User;
using BlogHybrid.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BlogHybrid.Application.Handlers.User
{
    public class UpdateUserProfileHandler : IRequestHandler<UpdateUserProfileCommand, UpdateUserProfileResult>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<UpdateUserProfileHandler> _logger;

        public UpdateUserProfileHandler(
            UserManager<ApplicationUser> userManager,
            ILogger<UpdateUserProfileHandler> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<UpdateUserProfileResult> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(request.UserId);
                if (user == null)
                {
                    return new UpdateUserProfileResult
                    {
                        Success = false,
                        Errors = new List<string> { "ไม่พบผู้ใช้งาน" }
                    };
                }

                // ตรวจสอบว่า DisplayName ซ้ำกับคนอื่นหรือไม่
                if (user.DisplayName != request.DisplayName)
                {
                    var existingUser = await _userManager.Users
                        .FirstOrDefaultAsync(u => u.DisplayName == request.DisplayName && u.Id != request.UserId, cancellationToken);

                    if (existingUser != null)
                    {
                        return new UpdateUserProfileResult
                        {
                            Success = false,
                            Errors = new List<string> { "ชื่อที่แสดงนี้ถูกใช้งานแล้ว" }
                        };
                    }
                }

                // อัปเดตข้อมูล
                user.DisplayName = request.DisplayName.Trim();
                user.FirstName = request.FirstName?.Trim() ?? string.Empty;
                user.LastName = request.LastName?.Trim() ?? string.Empty;
                user.PhoneNumber = request.PhoneNumber?.Trim();
                user.Bio = request.Bio?.Trim();
                user.ProfileImageUrl = request.ProfileImageUrl;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User profile updated successfully: {UserId}", request.UserId);

                    return new UpdateUserProfileResult
                    {
                        Success = true,
                        Message = "อัปเดตโปรไฟล์สำเร็จ"
                    };
                }
                else
                {
                    return new UpdateUserProfileResult
                    {
                        Success = false,
                        Errors = result.Errors.Select(e => e.Description).ToList()
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user profile: {UserId}", request.UserId);

                return new UpdateUserProfileResult
                {
                    Success = false,
                    Errors = new List<string> { "เกิดข้อผิดพลาดในการอัปเดตโปรไฟล์" }
                };
            }
        }
    }
}