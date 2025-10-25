// BlogHybrid.Application/DTOs/Post/PostDetailDto.cs
using BlogHybrid.Domain.Enums;

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
    /// รองรับทั้ง Legacy Like และ Vote/Reaction System แบบใหม่
    /// </summary>
    public class CommentDto
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsApproved { get; set; }

        // ===================================
        // Legacy Like System (เก็บไว้เพื่อ backward compatibility)
        // ===================================
        public int LikeCount { get; set; }
        public bool IsLikedByCurrentUser { get; set; }

        // ===================================
        // NEW: Vote System (Upvote/Downvote)
        // ===================================
        /// <summary>
        /// จำนวน Upvotes
        /// </summary>
        public int UpvoteCount { get; set; }

        /// <summary>
        /// จำนวน Downvotes
        /// </summary>
        public int DownvoteCount { get; set; }

        /// <summary>
        /// คะแนนโหวตรวม (UpvoteCount - DownvoteCount)
        /// </summary>
        public int VoteScore { get; set; }

        /// <summary>
        /// Vote ของ current user (null = ยังไม่ได้โหวต)
        /// </summary>
        public VoteType? CurrentUserVote { get; set; }

        // ===================================
        // NEW: Reaction System (Like, Love, Haha, etc.)
        // ===================================
        /// <summary>
        /// จำนวน reactions แต่ละประเภท
        /// </summary>
        public CommentReactionCounts Reactions { get; set; } = new();

        /// <summary>
        /// จำนวน reactions รวมทั้งหมด
        /// </summary>
        public int TotalReactionCount { get; set; }

        /// <summary>
        /// Reaction ของ current user (null = ยังไม่ได้ react)
        /// </summary>
        public ReactionType? CurrentUserReaction { get; set; }

        // ===================================
        // Author info
        // ===================================
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

    /// <summary>
    /// Class สำหรับเก็บจำนวน reactions แต่ละประเภท
    /// </summary>
    public class CommentReactionCounts
    {
        public int LikeCount { get; set; } = 0;      // 😊
        public int LoveCount { get; set; } = 0;      // ❤️
        public int HahaCount { get; set; } = 0;      // 😂
        public int WowCount { get; set; } = 0;       // 😮
        public int SadCount { get; set; } = 0;       // 😢
        public int AngryCount { get; set; } = 0;     // 😡

        /// <summary>
        /// ตรวจสอบว่ามี reaction หรือไม่
        /// </summary>
        public bool HasAnyReaction => LikeCount + LoveCount + HahaCount + WowCount + SadCount + AngryCount > 0;
    }
}