using MediatR;

namespace BlogHybrid.Application.Commands.Post
{
    public class UpdatePostCommand : IRequest<UpdatePostResult>
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? Excerpt { get; set; }
        public string? FeaturedImageUrl { get; set; }

        // Optional Fields
        public int? CategoryId { get; set; }
        public int? CommunityId { get; set; }

        // Tags (comma-separated string: "tag1,tag2,tag3")
        public string? Tags { get; set; }

        // จะถูก set จาก Controller (Current User)
        public string? AuthorId { get; set; }

        // Publish settings
        public bool IsPublished { get; set; } = true;
        public bool IsFeatured { get; set; } = false;
    }

    public class UpdatePostResult
    {
        public bool Success { get; set; }
        public string? Slug { get; set; }
        public string? Message { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}