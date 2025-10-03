// Path: BlogHybrid.Application/Commands/Auth/RegisterUserCommand.cs
using MediatR;

namespace BlogHybrid.Application.Commands.Auth
{
    public class RegisterUserCommand : IRequest<RegisterUserResult>
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public bool AcceptTerms { get; set; } = false;
    }

    public class RegisterUserResult
    {
        public bool Success { get; set; }
        public string? UserId { get; set; }
        public string? Message { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}