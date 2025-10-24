// BlogHybrid.Application/Commands/Comment/AddCommentCommand.cs
using MediatR;

namespace BlogHybrid.Application.Commands.Comment
{
    public class AddCommentCommand : IRequest<AddCommentResult>
    {
        public int PostId { get; set; }
        public string Content { get; set; } = string.Empty;
        public string AuthorId { get; set; } = string.Empty;
        public int? ParentCommentId { get; set; } // null = root comment, มีค่า = reply
    }

    public class AddCommentResult
    {
        public bool Success { get; set; }
        public int CommentId { get; set; }
        public string? Message { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}