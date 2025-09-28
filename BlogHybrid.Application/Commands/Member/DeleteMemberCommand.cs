using MediatR;

namespace BlogHybrid.Application.Commands.Member
{
    public class DeleteMemberCommand : IRequest<DeleteMemberResult>
    {
        public string Id { get; set; } = string.Empty;
    }

    public class DeleteMemberResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}