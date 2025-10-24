using MediatR;

namespace BlogHybrid.Application.Queries.Post
{
    public class GetPostForEditQuery : IRequest<PostForEditResult?>
    {
        public int PostId { get; set; }
        public string CurrentUserId { get; set; } = string.Empty;
    }

    public class PostForEditResult
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? Excerpt { get; set; }
        public string? FeaturedImageUrl { get; set; }
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public int? CommunityId { get; set; }
        public string? CommunityName { get; set; }
        public string Tags { get; set; } = string.Empty;
        public bool IsPublished { get; set; }
        public bool IsFeatured { get; set; }
        public string AuthorId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}