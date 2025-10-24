// BlogHybrid.Application/DTOs/Post/PostDetailDto.cs
namespace BlogHybrid.Application.DTOs.Post
{
    /// <summary>
    /// DTO สำหรับแสดงรายละเอียดโพสต์ทั้งหมดพร้อม comments
    /// </summary>
    public class PostDetailDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Excerpt { get; set; } = string.Empty;
        public string? FeaturedImageUrl { get; set; }

        // Stats
        public int ViewCount { get; set; }
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }
        public bool IsLikedByCurrentUser { get; set; }

        // Timestamps
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? PublishedAt { get; set; }

        // Author info
        public string AuthorId { get; set; } = string.Empty;
        public string AuthorDisplayName { get; set; } = string.Empty;
        public string AuthorUserName { get; set; } = string.Empty;
        public string? AuthorProfileImageUrl { get; set; }
        public string? AuthorBio { get; set; }

        // Category info
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? CategorySlug { get; set; }
        public string? CategoryColor { get; set; }

        // Community info
        public int? CommunityId { get; set; }
        public string? CommunityName { get; set; }
        public string? CommunitySlug { get; set; }
        public string? CommunityImageUrl { get; set; }

        // Tags
        public List<string> Tags { get; set; } = new();

        // Comments
        public List<CommentDto> Comments { get; set; } = new();

        // Permissions
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
    }

    /// <summary>
    /// DTO สำหรับ Comment พร้อม Replies (Hierarchical)
    /// </summary>
    public class CommentDto
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int LikeCount { get; set; }
        public bool IsApproved { get; set; }
        public bool IsLikedByCurrentUser { get; set; }

        // Author info
        public string AuthorId { get; set; } = string.Empty;
        public string AuthorDisplayName { get; set; } = string.Empty;
        public string AuthorUserName { get; set; } = string.Empty;
        public string? AuthorProfileImageUrl { get; set; }

        // Parent comment (for replies)
        public int? ParentCommentId { get; set; }

        // Replies (nested comments)
        public List<CommentDto> Replies { get; set; } = new();

        // Permissions
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
    }
}