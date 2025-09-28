using MediatR;

namespace BlogHybrid.Application.Commands.Member
{
    public class ToggleMemberStatusCommand : IRequest<ToggleMemberStatusResult>
    {
        public string Id { get; set; } = string.Empty;
    }

    public class ToggleMemberStatusResult
    {
        public bool Success { get; set; }
        public bool IsActive { get; set; }
        public string? Message { get; set; }
    }
}