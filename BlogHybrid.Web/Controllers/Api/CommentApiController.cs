// BlogHybrid.Web/Controllers/Api/CommentApiController.cs
using BlogHybrid.Application.Commands.Comment;
using BlogHybrid.Domain.Entities;
using BlogHybrid.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BlogHybrid.Web.Controllers.Api
{
    /// <summary>
    /// API Controller สำหรับ Comment Vote และ Reaction System
    /// </summary>
    [Route("api/comment")]
    [ApiController]
    [Authorize] // ต้อง login ก่อนถึงจะ vote/react ได้
    public class CommentApiController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<CommentApiController> _logger;

        public CommentApiController(
            IMediator mediator,
            UserManager<ApplicationUser> userManager,
            ILogger<CommentApiController> logger)
        {
            _mediator = mediator;
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// Vote บน Comment (Upvote/Downvote)
        /// POST /api/comment/{commentId}/vote
        /// </summary>
        /// <param name="commentId">Comment ID</param>
        /// <param name="request">{ "voteType": "Upvote" } หรือ "Downvote"</param>
        [HttpPost("{commentId}/vote")]
        public async Task<IActionResult> VoteComment(
            int commentId,
            [FromBody] VoteRequest request)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { success = false, message = "กรุณาเข้าสู่ระบบ" });
                }

                // Parse VoteType
                if (!Enum.TryParse<VoteType>(request.VoteType, true, out var voteType))
                {
                    return BadRequest(new { success = false, message = "ประเภท vote ไม่ถูกต้อง" });
                }

                var command = new VoteCommentCommand
                {
                    CommentId = commentId,
                    UserId = userId,
                    VoteType = voteType
                };

                var result = await _mediator.Send(command);

                if (result.Success)
                {
                    return Ok(new
                    {
                        success = true,
                        message = result.Message,
                        data = new
                        {
                            upvoteCount = result.UpvoteCount,
                            downvoteCount = result.DownvoteCount,
                            voteScore = result.VoteScore,
                            currentUserVote = result.CurrentUserVote?.ToString()
                        }
                    });
                }

                return BadRequest(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error voting comment: {CommentId}", commentId);
                return StatusCode(500, new { success = false, message = "เกิดข้อผิดพลาดในการโหวต" });
            }
        }

        /// <summary>
        /// React บน Comment (Like/Love/Haha/Wow/Sad/Angry)
        /// POST /api/comment/{commentId}/react
        /// </summary>
        /// <param name="commentId">Comment ID</param>
        /// <param name="request">{ "reactionType": "Like" }</param>
        [HttpPost("{commentId}/react")]
        public async Task<IActionResult> ReactToComment(
            int commentId,
            [FromBody] ReactionRequest request)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { success = false, message = "กรุณาเข้าสู่ระบบ" });
                }

                // Parse ReactionType
                if (!Enum.TryParse<ReactionType>(request.ReactionType, true, out var reactionType))
                {
                    return BadRequest(new { success = false, message = "ประเภท reaction ไม่ถูกต้อง" });
                }

                var command = new ReactToCommentCommand
                {
                    CommentId = commentId,
                    UserId = userId,
                    ReactionType = reactionType
                };

                var result = await _mediator.Send(command);

                if (result.Success)
                {
                    return Ok(new
                    {
                        success = true,
                        message = result.Message,
                        data = new
                        {
                            reactions = result.Reactions,
                            totalReactionCount = result.TotalReactionCount,
                            currentUserReaction = result.CurrentUserReaction?.ToString()
                        }
                    });
                }

                return BadRequest(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reacting to comment: {CommentId}", commentId);
                return StatusCode(500, new { success = false, message = "เกิดข้อผิดพลาดในการ react" });
            }
        }

        // ========================================
        // Request Models
        // ========================================

        public class VoteRequest
        {
            public string VoteType { get; set; } = string.Empty; // "Upvote" or "Downvote"
        }

        public class ReactionRequest
        {
            public string ReactionType { get; set; } = string.Empty; // "Like", "Love", "Haha", "Wow", "Sad", "Angry"
        }
    }
}