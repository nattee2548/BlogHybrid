// BlogHybrid.Application/Handlers/Comment/VoteCommentHandler.cs
using BlogHybrid.Application.Commands.Comment;
using BlogHybrid.Application.Interfaces.Repositories;
using BlogHybrid.Domain.Entities;
using BlogHybrid.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BlogHybrid.Application.Handlers.Comment
{
    public class VoteCommentHandler : IRequestHandler<VoteCommentCommand, VoteCommentResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<VoteCommentHandler> _logger;

        public VoteCommentHandler(
            IUnitOfWork unitOfWork,
            ILogger<VoteCommentHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<VoteCommentResult> Handle(
            VoteCommentCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                // 1. ตรวจสอบว่า comment มีอยู่จริง
                var comment = await _unitOfWork.DbContext.Set<Domain.Entities.Comment>()
                    .Include(c => c.CommentVotes)
                    .FirstOrDefaultAsync(c => c.Id == request.CommentId, cancellationToken);

                if (comment == null)
                {
                    return new VoteCommentResult
                    {
                        Success = false,
                        Message = "ไม่พบความคิดเห็น"
                    };
                }

                // 2. ตรวจสอบว่า user เคยโหวตแล้วหรือยัง
                var existingVote = await _unitOfWork.DbContext.Set<CommentVote>()
                    .FirstOrDefaultAsync(cv => cv.CommentId == request.CommentId &&
                                              cv.UserId == request.UserId,
                                        cancellationToken);

                VoteType? currentUserVote = null;

                if (existingVote != null)
                {
                    // 3a. เคยโหวตแล้ว
                    if (existingVote.VoteType == request.VoteType)
                    {
                        // กดซ้ำ = ยกเลิก vote
                        _unitOfWork.DbContext.Set<CommentVote>().Remove(existingVote);

                        // อัปเดต count
                        if (request.VoteType == VoteType.Upvote)
                            comment.UpvoteCount = Math.Max(0, comment.UpvoteCount - 1);
                        else
                            comment.DownvoteCount = Math.Max(0, comment.DownvoteCount - 1);

                        comment.VoteScore = comment.UpvoteCount - comment.DownvoteCount;

                        await _unitOfWork.SaveChangesAsync(cancellationToken);

                        _logger.LogInformation("User {UserId} cancelled vote on comment {CommentId}",
                            request.UserId, request.CommentId);

                        return new VoteCommentResult
                        {
                            Success = true,
                            Message = "ยกเลิกการโหวตแล้ว",
                            UpvoteCount = comment.UpvoteCount,
                            DownvoteCount = comment.DownvoteCount,
                            VoteScore = comment.VoteScore,
                            CurrentUserVote = null
                        };
                    }
                    else
                    {
                        // เปลี่ยนใจ (Upvote → Downvote หรือตรงกันข้าม)
                        var oldVoteType = existingVote.VoteType;
                        existingVote.VoteType = request.VoteType;
                        existingVote.UpdatedAt = DateTime.UtcNow;

                        // อัปเดต count (ลบเก่า เพิ่มใหม่)
                        if (oldVoteType == VoteType.Upvote)
                        {
                            comment.UpvoteCount = Math.Max(0, comment.UpvoteCount - 1);
                            comment.DownvoteCount++;
                        }
                        else
                        {
                            comment.DownvoteCount = Math.Max(0, comment.DownvoteCount - 1);
                            comment.UpvoteCount++;
                        }

                        comment.VoteScore = comment.UpvoteCount - comment.DownvoteCount;
                        currentUserVote = request.VoteType;

                        await _unitOfWork.SaveChangesAsync(cancellationToken);

                        _logger.LogInformation("User {UserId} changed vote on comment {CommentId} from {OldVote} to {NewVote}",
                            request.UserId, request.CommentId, oldVoteType, request.VoteType);

                        return new VoteCommentResult
                        {
                            Success = true,
                            Message = "เปลี่ยนการโหวตแล้ว",
                            UpvoteCount = comment.UpvoteCount,
                            DownvoteCount = comment.DownvoteCount,
                            VoteScore = comment.VoteScore,
                            CurrentUserVote = currentUserVote
                        };
                    }
                }
                else
                {
                    // 3b. ยังไม่เคยโหวต = เพิ่มใหม่
                    var newVote = new CommentVote
                    {
                        CommentId = request.CommentId,
                        UserId = request.UserId,
                        VoteType = request.VoteType,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _unitOfWork.DbContext.Set<CommentVote>().AddAsync(newVote, cancellationToken);

                    // อัปเดต count
                    if (request.VoteType == VoteType.Upvote)
                        comment.UpvoteCount++;
                    else
                        comment.DownvoteCount++;

                    comment.VoteScore = comment.UpvoteCount - comment.DownvoteCount;
                    currentUserVote = request.VoteType;

                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation("User {UserId} voted {VoteType} on comment {CommentId}",
                        request.UserId, request.VoteType, request.CommentId);

                    return new VoteCommentResult
                    {
                        Success = true,
                        Message = "โหวตสำเร็จ",
                        UpvoteCount = comment.UpvoteCount,
                        DownvoteCount = comment.DownvoteCount,
                        VoteScore = comment.VoteScore,
                        CurrentUserVote = currentUserVote
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error voting comment: CommentId={CommentId}, UserId={UserId}",
                    request.CommentId, request.UserId);

                return new VoteCommentResult
                {
                    Success = false,
                    Message = "เกิดข้อผิดพลาดในการโหวต"
                };
            }
        }
    }
}