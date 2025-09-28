using MediatR;

namespace BlogHybrid.Application.Commands.Member
{
    public class CreateMemberCommand : IRequest<CreateMemberResult>
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Bio { get; set; }
        public List<string>? Roles { get; set; }
    }

    public class CreateMemberResult
    {
        public bool Success { get; set; }
        public string? MemberId { get; set; }
        public string? Message { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}