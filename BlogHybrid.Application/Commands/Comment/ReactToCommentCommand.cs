// BlogHybrid.Application/Commands/Comment/ReactToCommentCommand.cs
using BlogHybrid.Domain.Enums;
using MediatR;

namespace BlogHybrid.Application.Commands.Comment
{
    /// <summary>
    /// Command สำหรับแสดงความรู้สึกบน Comment (Reaction)
    /// </summary>
    public class ReactToCommentCommand : IRequest<ReactToCommentResult>
    {
        /// <summary>
        /// Comment ID ที่ต้องการ react
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
    }

    /// <summary>
    /// Result สำหรับ ReactToCommentCommand
    /// </summary>
    public class ReactToCommentResult
    {
        /// <summary>
        /// React สำเร็จหรือไม่
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// ข้อความตอบกลับ
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// จำนวน Reactions แยกตามประเภท
        /// </summary>
        public CommentReactionCounts Reactions { get; set; } = new();

        /// <summary>
        /// จำนวน Reactions ทั้งหมด
        /// </summary>
        public int TotalReactionCount { get; set; }

        /// <summary>
        /// Reaction ปัจจุบันของ User (null = ยังไม่ได้ react)
        /// </summary>
        public ReactionType? CurrentUserReaction { get; set; }
    }

    /// <summary>
    /// Class สำหรับเก็บจำนวน Reactions แต่ละประเภท
    /// </summary>
    public class CommentReactionCounts
    {
        public int LikeCount { get; set; }
        public int LoveCount { get; set; }
        public int HahaCount { get; set; }
        public int WowCount { get; set; }
        public int SadCount { get; set; }
        public int AngryCount { get; set; }
    }
}