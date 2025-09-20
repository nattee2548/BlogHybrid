// BlogHybrid.Application/Commands/Auth/LoginUserCommand.cs
using MediatR;

namespace BlogHybrid.Application.Commands.Auth
{
    public class LoginUserCommand : IRequest<LoginUserResult>
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; } = false;
        public string? ReturnUrl { get; set; }
    }

    public class LoginUserResult
    {
        public bool Success { get; set; }
        public bool RequiresTwoFactor { get; set; } = false;
        public bool IsLockedOut { get; set; } = false;
        public bool IsNotAllowed { get; set; } = false;
        public List<string> Errors { get; set; } = new();
        public string? Message { get; set; }
        public string? RedirectUrl { get; set; }
    }
}