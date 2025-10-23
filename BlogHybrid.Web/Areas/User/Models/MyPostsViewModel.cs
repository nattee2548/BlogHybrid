namespace BlogHybrid.Web.Areas.User.Models
{
    public class MyPostsViewModel
    {
        public List<MyPostItemViewModel> Posts { get; set; } = new();

        // Pagination
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }

        // Filters
        public string? SearchTerm { get; set; }
        public string StatusFilter { get; set; } = "all";

        // Statistics
        public PostStatisticsViewModel Statistics { get; set; } = new();
    }

    public class MyPostItemViewModel
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

        // Computed Properties
        public string StatusBadge => IsPublished ? "เผยแพร่แล้ว" : "แบบร่าง";
        public string StatusColor => IsPublished ? "success" : "secondary";
        public string DisplayDate => IsPublished && PublishedAt.HasValue
            ? PublishedAt.Value.ToString("dd MMM yyyy")
            : CreatedAt.ToString("dd MMM yyyy");
        public string TruncatedExcerpt => Excerpt?.Length > 100
            ? Excerpt.Substring(0, 100) + "..."
            : Excerpt ?? "";
    }

    public class PostStatisticsViewModel
    {
        public int PublishedCount { get; set; }
        public int DraftCount { get; set; }
        public int FeaturedCount { get; set; }
        public int TotalViews { get; set; }
        public int TotalLikes { get; set; }
        public int TotalComments { get; set; }
        public int TotalPosts => PublishedCount + DraftCount;
    }
}