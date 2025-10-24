namespace BlogHybrid.Web.Areas.User.Models
{
    public class PostDetailsViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? Excerpt { get; set; }
        public string? FeaturedImageUrl { get; set; }

        public bool IsPublished { get; set; }
        public bool IsFeatured { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? PublishedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Category & Community
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
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
            ? PublishedAt.Value.ToString("dd MMMM yyyy HH:mm")
            : CreatedAt.ToString("dd MMMM yyyy HH:mm");
    }
}