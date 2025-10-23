using BlogHybrid.Domain.Entities;
using MediatR;

namespace BlogHybrid.Application.Queries.Post
{
    /// <summary>
    /// Query to get posts by user (author)
    /// </summary>
    public class GetUserPostsQuery : IRequest<GetUserPostsResult>
    {
        public string UserId { get; set; } = string.Empty;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public string? StatusFilter { get; set; } // all, published, draft, featured
        public string SortBy { get; set; } = "CreatedAt";
        public string SortDirection { get; set; } = "desc";
    }

    public class GetUserPostsResult
    {
        public List<UserPostItem> Posts { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;

        // Statistics
        public int PublishedCount { get; set; }
        public int DraftCount { get; set; }
        public int FeaturedCount { get; set; }
        public int TotalViews { get; set; }
        public int TotalLikes { get; set; }
        public int TotalComments { get; set; }
    }

    public class UserPostItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Excerpt { get; set; }
        public string? FeaturedImageUrl { get; set; }
        public bool IsPublished { get; set; }
        public bool IsFeatured { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PublishedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Category & Community
        public int? CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int? CommunityId { get; set; }
        public string? CommunityName { get; set; }

        // Stats
        public int ViewCount { get; set; }
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }

        // Tags
        public List<string> Tags { get; set; } = new();
    }
}