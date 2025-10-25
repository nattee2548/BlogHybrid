using BlogHybrid.Domain.Enums;

namespace BlogHybrid.Domain.Entities
{
    /// <summary>
    /// Entity สำหรับการโหวต Comment (Upvote/Downvote)
    /// </summary>
    public class CommentVote
    {
        /// <summary>
        /// Comment ID ที่ถูกโหวต
        /// </summary>
        public int CommentId { get; set; }

        /// <summary>
        /// User ID ของผู้โหวต
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// ประเภทของการโหวต (Upvote = 1, Downvote = -1)
        /// </summary>
        public VoteType VoteType { get; set; }

        /// <summary>
        /// วันที่โหวต
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// วันที่แก้ไขโหวตล่าสุด (เปลี่ยนจาก Upvote เป็น Downvote หรือในทางกลับกัน)
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual Comment Comment { get; set; } = null!;
        public virtual ApplicationUser User { get; set; } = null!;
    }
}