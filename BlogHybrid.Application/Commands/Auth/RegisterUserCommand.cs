using MediatR;

namespace BlogHybrid.Application.Commands.Auth
{
    public class RegisterUserCommand : IRequest<RegisterUserResult>
    {
        public string DisplayName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool AcceptTerms { get; set; }
    }

    public class RegisterUserResult
    {
        public bool Success { get; set; }
        public string? UserId { get; set; }
        public List<string> Errors { get; set; } = new();
        public string? Message { get; set; }
    }
}