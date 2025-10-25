// BlogHybrid.Application/Commands/Comment/VoteCommentCommand.cs
using BlogHybrid.Domain.Enums;
using MediatR;

namespace BlogHybrid.Application.Commands.Comment
{
    /// <summary>
    /// Command สำหรับโหวต Comment (Upvote/Downvote)
    /// </summary>
    public class VoteCommentCommand : IRequest<VoteCommentResult>
    {
        /// <summary>
        /// Comment ID ที่ต้องการโหวต
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
    }

    /// <summary>
    /// Result สำหรับ VoteCommentCommand
    /// </summary>
    public class VoteCommentResult
    {
        /// <summary>
        /// โหวตสำเร็จหรือไม่
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// ข้อความตอบกลับ
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// จำนวน Upvote ทั้งหมด
        /// </summary>
        public int UpvoteCount { get; set; }

        /// <summary>
        /// จำนวน Downvote ทั้งหมด
        /// </summary>
        public int DownvoteCount { get; set; }

        /// <summary>
        /// Vote Score (Upvotes - Downvotes)
        /// </summary>
        public int VoteScore { get; set; }

        /// <summary>
        /// Vote ปัจจุบันของ User (null = ยังไม่ได้โหวต)
        /// </summary>
        public VoteType? CurrentUserVote { get; set; }
    }
}