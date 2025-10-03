// BlogHybrid.Application/Commands/Tag/DeleteTagCommand.cs
using MediatR;

namespace BlogHybrid.Application.Commands.Tag
{
    public class DeleteTagCommand : IRequest<DeleteTagResult>
    {
        public int Id { get; set; }
        public bool ForceDelete { get; set; } = false;
    }

    public class DeleteTagResult
    {
        public bool Success { get; set; }
        public List<string> Errors { get; set; } = new();
        public string? Message { get; set; }
        public bool HasPosts { get; set; }
        public int PostCount { get; set; }
    }
}