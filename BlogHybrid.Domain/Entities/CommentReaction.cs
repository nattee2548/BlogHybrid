using BlogHybrid.Domain.Enums;

namespace BlogHybrid.Domain.Entities
{
    /// <summary>
    /// Entity สำหรับการแสดง Reaction บน Comment (เหมือน Facebook Reactions)
    /// </summary>
    public class CommentReaction
    {
        /// <summary>
        /// Comment ID ที่ถูก react
        /// </summary>
        public int CommentId { get; set; }

        /// <summary>
        /// User ID ของผู้ react
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// ประเภทของ Reaction (Like, Love, Haha, Wow, Sad, Angry)
        /// </summary>
        public ReactionType ReactionType { get; set; }

        /// <summary>
        /// วันที่ react
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// วันที่เปลี่ยน reaction ล่าสุด (เปลี่ยนจาก Like เป็น Love เช่น)
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual Comment Comment { get; set; } = null!;
        public virtual ApplicationUser User { get; set; } = null!;
    }
}