using BlogHybrid.Application.Commands.User;
using BlogHybrid.Application.Interfaces.Repositories;
using BlogHybrid.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BlogHybrid.Application.Handlers.User
{
    #region Create User Handler

    public class CreateUserHandler : IRequestHandler<CreateUserCommand, CreateUserResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CreateUserHandler> _logger;

        public CreateUserHandler(
            IUnitOfWork unitOfWork,
            ILogger<CreateUserHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<CreateUserResult> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Validate email doesn't exist
                if (await _unitOfWork.Users.EmailExistsAsync(request.Email, cancellationToken: cancellationToken))
                {
                    return new CreateUserResult
                    {
                        Success = false,
                        Errors = new List<string> { "อีเมลนี้มีผู้ใช้งานแล้ว" }
                    };
                }

                // Create user entity
                var user = new ApplicationUser
                {
                    UserName = request.Email,
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    PhoneNumber = request.PhoneNumber,
                    IsActive = request.IsActive,
                    EmailConfirmed = request.EmailConfirmed,
                    CreatedAt = DateTime.UtcNow
                };

                // Create user
                var result = await _unitOfWork.Users.CreateAsync(user, request.Password, cancellationToken);

                if (!result.Succeeded)
                {
                    return new CreateUserResult
                    {
                        Success = false,
                        Errors = result.Errors.Select(e => e.Description).ToList()
                    };
                }

                // Add roles if specified
                if (request.SelectedRoles?.Any() == true)
                {
                    var roleResult = await _unitOfWork.Users.AddToRolesAsync(user, request.SelectedRoles, cancellationToken);
                    if (!roleResult.Succeeded)
                    {
                        _logger.LogWarning("Failed to add roles to user {Email}: {Errors}",
                            user.Email, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                    }
                }

                _logger.LogInformation("User created successfully: {Email}", user.Email);

                return new CreateUserResult
                {
                    Success = true,
                    UserId = user.Id,
                    Message = $"สร้างผู้ใช้ {user.Email} เรียบร้อยแล้ว"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user: {Email}", request.Email);
                return new CreateUserResult
                {
                    Success = false,
                    Errors = new List<string> { "เกิดข้อผิดพลาดในการสร้างผู้ใช้" }
                };
            }
        }
    }

    #endregion

    #region Update User Handler

    public class UpdateUserHandler : IRequestHandler<UpdateUserCommand, UpdateUserResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateUserHandler> _logger;

        public UpdateUserHandler(
            IUnitOfWork unitOfWork,
            ILogger<UpdateUserHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<UpdateUserResult> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(request.Id, cancellationToken);
                if (user == null)
                {
                    return new UpdateUserResult
                    {
                        Success = false,
                        Errors = new List<string> { "ไม่พบผู้ใช้ที่ต้องการแก้ไข" }
                    };
                }

                // Check email uniqueness (exclude current user)
                if (user.Email != request.Email &&
                    await _unitOfWork.Users.EmailExistsAsync(request.Email, request.Id, cancellationToken))
                {
                    return new UpdateUserResult
                    {
                        Success = false,
                        Errors = new List<string> { "อีเมลนี้มีผู้ใช้งานแล้ว" }
                    };
                }

                // Check username uniqueness (exclude current user)
                if (user.UserName != request.UserName &&
                    await _unitOfWork.Users.UserNameExistsAsync(request.UserName, request.Id, cancellationToken))
                {
                    return new UpdateUserResult
                    {
                        Success = false,
                        Errors = new List<string> { "ชื่อผู้ใช้นี้มีผู้ใช้งานแล้ว" }
                    };
                }

                // Update user properties
                user.Email = request.Email;
                user.UserName = request.UserName;
                user.FirstName = request.FirstName;
                user.LastName = request.LastName;
                user.PhoneNumber = request.PhoneNumber;
                user.IsActive = request.IsActive;
                user.EmailConfirmed = request.EmailConfirmed;

                var updateResult = await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
                if (!updateResult.Succeeded)
                {
                    return new UpdateUserResult
                    {
                        Success = false,
                        Errors = updateResult.Errors.Select(e => e.Description).ToList()
                    };
                }

                // Update roles
                var currentRoles = await _unitOfWork.Users.GetRolesAsync(user, cancellationToken);
                var selectedRoles = request.SelectedRoles ?? new List<string>();

                // Remove roles that are no longer selected
                var rolesToRemove = currentRoles.Except(selectedRoles);
                if (rolesToRemove.Any())
                {
                    await _unitOfWork.Users.RemoveFromRolesAsync(user, rolesToRemove, cancellationToken);
                }

                // Add new roles
                var rolesToAdd = selectedRoles.Except(currentRoles);
                if (rolesToAdd.Any())
                {
                    await _unitOfWork.Users.AddToRolesAsync(user, rolesToAdd, cancellationToken);
                }

                _logger.LogInformation("User updated successfully: {Email}", user.Email);

                return new UpdateUserResult
                {
                    Success = true,
                    Message = $"แก้ไขข้อมูลผู้ใช้ {user.Email} เรียบร้อยแล้ว"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user: {UserId}", request.Id);
                return new UpdateUserResult
                {
                    Success = false,
                    Errors = new List<string> { "เกิดข้อผิดพลาดในการแก้ไขข้อมูลผู้ใช้" }
                };
            }
        }
    }

    #endregion

    #region Delete User Handler

    public class DeleteUserHandler : IRequestHandler<DeleteUserCommand, DeleteUserResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeleteUserHandler> _logger;

        public DeleteUserHandler(
            IUnitOfWork unitOfWork,
            ILogger<DeleteUserHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<DeleteUserResult> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(request.Id, cancellationToken);
                if (user == null)
                {
                    return new DeleteUserResult
                    {
                        Success = false,
                        Errors = new List<string> { "ไม่พบผู้ใช้ที่ต้องการลบ" }
                    };
                }

                var result = await _unitOfWork.Users.DeleteAsync(user, cancellationToken);
                if (!result.Succeeded)
                {
                    return new DeleteUserResult
                    {
                        Success = false,
                        Errors = result.Errors.Select(e => e.Description).ToList()
                    };
                }

                _logger.LogInformation("User deleted successfully: {Email}", user.Email);

                return new DeleteUserResult
                {
                    Success = true,
                    Message = $"ลบผู้ใช้ {user.Email} เรียบร้อยแล้ว"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user: {UserId}", request.Id);
                return new DeleteUserResult
                {
                    Success = false,
                    Errors = new List<string> { "เกิดข้อผิดพลาดในการลบผู้ใช้" }
                };
            }
        }
    }

    #endregion

    #region Toggle User Status Handler

    public class ToggleUserStatusHandler : IRequestHandler<ToggleUserStatusCommand, ToggleUserStatusResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ToggleUserStatusHandler> _logger;

        public ToggleUserStatusHandler(
            IUnitOfWork unitOfWork,
            ILogger<ToggleUserStatusHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ToggleUserStatusResult> Handle(ToggleUserStatusCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(request.Id, cancellationToken);
                if (user == null)
                {
                    return new ToggleUserStatusResult
                    {
                        Success = false,
                        Errors = new List<string> { "ไม่พบผู้ใช้ที่ต้องการ" }
                    };
                }

                user.IsActive = !user.IsActive;
                var result = await _unitOfWork.Users.UpdateAsync(user, cancellationToken);

                if (!result.Succeeded)
                {
                    return new ToggleUserStatusResult
                    {
                        Success = false,
                        Errors = result.Errors.Select(e => e.Description).ToList()
                    };
                }

                var status = user.IsActive ? "เปิดใช้งาน" : "ปิดใช้งาน";
                _logger.LogInformation("User status toggled: {Email} -> {Status}", user.Email, status);

                return new ToggleUserStatusResult
                {
                    Success = true,
                    IsActive = user.IsActive,
                    Message = $"{status}ผู้ใช้ {user.Email} เรียบร้อยแล้ว"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling user status: {UserId}", request.Id);
                return new ToggleUserStatusResult
                {
                    Success = false,
                    Errors = new List<string> { "เกิดข้อผิดพลาดในการเปลี่ยนสถานะผู้ใช้" }
                };
            }
        }
    }

    #endregion

    #region Reset User Password Handler

    public class ResetUserPasswordHandler : IRequestHandler<ResetUserPasswordCommand, ResetUserPasswordResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ResetUserPasswordHandler> _logger;

        public ResetUserPasswordHandler(
            IUnitOfWork unitOfWork,
            ILogger<ResetUserPasswordHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ResetUserPasswordResult> Handle(ResetUserPasswordCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken);
                if (user == null)
                {
                    return new ResetUserPasswordResult
                    {
                        Success = false,
                        Errors = new List<string> { "ไม่พบผู้ใช้ที่ต้องการ" }
                    };
                }

                var result = await _unitOfWork.Users.ResetPasswordAsync(user, request.NewPassword, cancellationToken);
                if (!result.Succeeded)
                {
                    return new ResetUserPasswordResult
                    {
                        Success = false,
                        Errors = result.Errors.Select(e => e.Description).ToList()
                    };
                }

                _logger.LogInformation("Password reset successfully for user: {Email}", user.Email);

                return new ResetUserPasswordResult
                {
                    Success = true,
                    Message = $"รีเซ็ตรหัสผ่านสำหรับ {user.Email} เรียบร้อยแล้ว"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for user: {UserId}", request.UserId);
                return new ResetUserPasswordResult
                {
                    Success = false,
                    Errors = new List<string> { "เกิดข้อผิดพลาดในการรีเซ็ตรหัสผ่าน" }
                };
            }
        }
    }

    #endregion
}