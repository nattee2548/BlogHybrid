using System;
using System.Collections.Generic;

namespace BlogHybrid.Domain.Entities
{
    /// <summary>
    /// Comment entity พร้อม Vote และ Reaction System
    /// </summary>
    public class Comment
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public bool IsApproved { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // ===================================
        // Legacy Like System (เก็บไว้เพื่อ backward compatibility)
        // ===================================
        public int LikeCount { get; set; } = 0;

        // ===================================
        // NEW: Vote System (Upvote/Downvote)
        // ===================================
        /// <summary>
        /// จำนวน Upvotes
        /// </summary>
        public int UpvoteCount { get; set; } = 0;

        /// <summary>
        /// จำนวน Downvotes
        /// </summary>
        public int DownvoteCount { get; set; } = 0;

        /// <summary>
        /// คะแนนโหวตรวม (UpvoteCount - DownvoteCount)
        /// ใช้สำหรับจัดเรียงความน่าเชื่อถือของ comment
        /// </summary>
        public int VoteScore { get; set; } = 0;

        // ===================================
        // NEW: Reaction System (Like, Love, Haha, etc.)
        // ===================================
        /// <summary>
        /// จำนวน Like reactions (😊)
        /// </summary>
        public int ReactionLikeCount { get; set; } = 0;

        /// <summary>
        /// จำนวน Love reactions (❤️)
        /// </summary>
        public int ReactionLoveCount { get; set; } = 0;

        /// <summary>
        /// จำนวน Haha reactions (😂)
        /// </summary>
        public int ReactionHahaCount { get; set; } = 0;

        /// <summary>
        /// จำนวน Wow reactions (😮)
        /// </summary>
        public int ReactionWowCount { get; set; } = 0;

        /// <summary>
        /// จำนวน Sad reactions (😢)
        /// </summary>
        public int ReactionSadCount { get; set; } = 0;

        /// <summary>
        /// จำนวน Angry reactions (😡)
        /// </summary>
        public int ReactionAngryCount { get; set; } = 0;

        /// <summary>
        /// จำนวน Reactions ทั้งหมด
        /// </summary>
        public int TotalReactionCount { get; set; } = 0;

        // Foreign Keys
        public int PostId { get; set; }
        public string AuthorId { get; set; } = string.Empty;
        public int? ParentCommentId { get; set; }

        // ===================================
        // Navigation properties
        // ===================================
        public virtual Post Post { get; set; } = null!;
        public virtual ApplicationUser Author { get; set; } = null!;
        public virtual Comment? ParentComment { get; set; }
        public virtual ICollection<Comment> Replies { get; set; } = new List<Comment>();

        // Legacy
        public virtual ICollection<CommentLike> CommentLikes { get; set; } = new List<CommentLike>();

        // NEW: Vote & Reaction collections
        public virtual ICollection<CommentVote> CommentVotes { get; set; } = new List<CommentVote>();
        public virtual ICollection<CommentReaction> CommentReactions { get; set; } = new List<CommentReaction>();
    }
}