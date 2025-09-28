using MediatR;

namespace BlogHybrid.Application.Commands.Member
{
    public class ChangeMemberPasswordCommand : IRequest<ChangeMemberPasswordResult>
    {
        public string MemberId { get; set; } = string.Empty;
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    public class ChangeMemberPasswordResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}