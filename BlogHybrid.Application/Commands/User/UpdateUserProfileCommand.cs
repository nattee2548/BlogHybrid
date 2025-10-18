// Path: BlogHybrid.Application/Commands/User/UpdateUserProfileCommand.cs
using MediatR;

namespace BlogHybrid.Application.Commands.User
{
    public class UpdateUserProfileCommand : IRequest<UpdateUserProfileResult>
    {
        public string UserId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Bio { get; set; }
        public string? ProfileImageUrl { get; set; }
    }

    public class UpdateUserProfileResult
    {
        public bool Success { get; set; }
        public List<string> Errors { get; set; } = new();
        public string? Message { get; set; }
    }
}