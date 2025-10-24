using MediatR;

namespace BlogHybrid.Application.Commands.Post
{
    public class DeletePostCommand : IRequest<DeletePostResult>
    {
        public int Id { get; set; }
        public string CurrentUserId { get; set; } = string.Empty;
    }

    public class DeletePostResult
    {
        public bool Success { get; set; }
        public List<string> Errors { get; set; } = new();
        public string? Message { get; set; }
    }
}