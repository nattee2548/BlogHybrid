using MediatR;

namespace BlogHybrid.Application.Commands.User
{
    #region Create User

    public class CreateUserCommand : IRequest<CreateUserResult>
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsActive { get; set; } = true;
        public bool EmailConfirmed { get; set; } = false;
        public List<string>? SelectedRoles { get; set; } = new();
    }

    public class CreateUserResult
    {
        public bool Success { get; set; }
        public string? UserId { get; set; }
        public string? Message { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    #endregion

    #region Update User

    public class UpdateUserCommand : IRequest<UpdateUserResult>
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsActive { get; set; }
        public bool EmailConfirmed { get; set; }
        public List<string>? SelectedRoles { get; set; } = new();
    }

    public class UpdateUserResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    #endregion

    #region Delete User

    public class DeleteUserCommand : IRequest<DeleteUserResult>
    {
        public string Id { get; set; } = string.Empty;
    }

    public class DeleteUserResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    #endregion

    #region Toggle User Status

    public class ToggleUserStatusCommand : IRequest<ToggleUserStatusResult>
    {
        public string Id { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class ToggleUserStatusResult
    {
        public bool Success { get; set; }
        public bool IsActive { get; set; }
        public string? Message { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    #endregion

    #region Reset Password

    public class ResetUserPasswordCommand : IRequest<ResetUserPasswordResult>
    {
        public string UserId { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    public class ResetUserPasswordResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    #endregion
}