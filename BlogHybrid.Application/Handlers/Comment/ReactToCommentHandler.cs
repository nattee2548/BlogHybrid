// BlogHybrid.Application/Handlers/Comment/ReactToCommentHandler.cs
using BlogHybrid.Application.Commands.Comment;
using BlogHybrid.Application.Interfaces.Repositories;
using BlogHybrid.Domain.Entities;
using BlogHybrid.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BlogHybrid.Application.Handlers.Comment
{
    public class ReactToCommentHandler : IRequestHandler<ReactToCommentCommand, ReactToCommentResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ReactToCommentHandler> _logger;

        public ReactToCommentHandler(
            IUnitOfWork unitOfWork,
            ILogger<ReactToCommentHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ReactToCommentResult> Handle(
            ReactToCommentCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                // 1. ตรวจสอบว่า comment มีอยู่จริง
                var comment = await _unitOfWork.DbContext.Set<Domain.Entities.Comment>()
                    .Include(c => c.CommentReactions)
                    .FirstOrDefaultAsync(c => c.Id == request.CommentId, cancellationToken);

                if (comment == null)
                {
                    return new ReactToCommentResult
                    {
                        Success = false,
                        Message = "ไม่พบความคิดเห็น"
                    };
                }

                // 2. ตรวจสอบว่า user เคย react แล้วหรือยัง
                var existingReaction = await _unitOfWork.DbContext.Set<CommentReaction>()
                    .FirstOrDefaultAsync(cr => cr.CommentId == request.CommentId &&
                                               cr.UserId == request.UserId,
                                         cancellationToken);

                ReactionType? currentUserReaction = null;

                if (existingReaction != null)
                {
                    // 3a. เคย react แล้ว
                    if (existingReaction.ReactionType == request.ReactionType)
                    {
                        // กดซ้ำ = ยกเลิก reaction
                        _unitOfWork.DbContext.Set<CommentReaction>().Remove(existingReaction);

                        // ลด count ของ reaction ที่ยกเลิก
                        DecrementReactionCount(comment, request.ReactionType);
                        comment.TotalReactionCount = Math.Max(0, comment.TotalReactionCount - 1);

                        await _unitOfWork.SaveChangesAsync(cancellationToken);

                        _logger.LogInformation("User {UserId} cancelled {ReactionType} on comment {CommentId}",
                            request.UserId, request.ReactionType, request.CommentId);

                        return new ReactToCommentResult
                        {
                            Success = true,
                            Message = "ยกเลิก reaction แล้ว",
                            Reactions = GetReactionCounts(comment),
                            TotalReactionCount = comment.TotalReactionCount,
                            CurrentUserReaction = null
                        };
                    }
                    else
                    {
                        // เปลี่ยนใจ (Like → Love ฯลฯ)
                        var oldReactionType = existingReaction.ReactionType;

                        // ลด count ของ reaction เก่า
                        DecrementReactionCount(comment, oldReactionType);

                        // เพิ่ม count ของ reaction ใหม่
                        existingReaction.ReactionType = request.ReactionType;
                        existingReaction.UpdatedAt = DateTime.UtcNow;
                        IncrementReactionCount(comment, request.ReactionType);

                        // TotalReactionCount ไม่เปลี่ยน (แค่เปลี่ยนประเภท)
                        currentUserReaction = request.ReactionType;

                        await _unitOfWork.SaveChangesAsync(cancellationToken);

                        _logger.LogInformation("User {UserId} changed reaction on comment {CommentId} from {OldReaction} to {NewReaction}",
                            request.UserId, request.CommentId, oldReactionType, request.ReactionType);

                        return new ReactToCommentResult
                        {
                            Success = true,
                            Message = "เปลี่ยน reaction แล้ว",
                            Reactions = GetReactionCounts(comment),
                            TotalReactionCount = comment.TotalReactionCount,
                            CurrentUserReaction = currentUserReaction
                        };
                    }
                }
                else
                {
                    // 3b. ยังไม่เคย react = เพิ่มใหม่
                    var newReaction = new CommentReaction
                    {
                        CommentId = request.CommentId,
                        UserId = request.UserId,
                        ReactionType = request.ReactionType,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _unitOfWork.DbContext.Set<CommentReaction>().AddAsync(newReaction, cancellationToken);

                    // เพิ่ม count
                    IncrementReactionCount(comment, request.ReactionType);
                    comment.TotalReactionCount++;
                    currentUserReaction = request.ReactionType;

                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation("User {UserId} reacted {ReactionType} on comment {CommentId}",
                        request.UserId, request.ReactionType, request.CommentId);

                    return new ReactToCommentResult
                    {
                        Success = true,
                        Message = "แสดงความรู้สึกสำเร็จ",
                        Reactions = GetReactionCounts(comment),
                        TotalReactionCount = comment.TotalReactionCount,
                        CurrentUserReaction = currentUserReaction
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reacting to comment: CommentId={CommentId}, UserId={UserId}",
                    request.CommentId, request.UserId);

                return new ReactToCommentResult
                {
                    Success = false,
                    Message = "เกิดข้อผิดพลาดในการแสดงความรู้สึก"
                };
            }
        }

        /// <summary>
        /// เพิ่ม count ของ reaction ตามประเภท
        /// </summary>
        private void IncrementReactionCount(Domain.Entities.Comment comment, ReactionType reactionType)
        {
            switch (reactionType)
            {
                case ReactionType.Like:
                    comment.ReactionLikeCount++;
                    break;
                case ReactionType.Love:
                    comment.ReactionLoveCount++;
                    break;
                case ReactionType.Haha:
                    comment.ReactionHahaCount++;
                    break;
                case ReactionType.Wow:
                    comment.ReactionWowCount++;
                    break;
                case ReactionType.Sad:
                    comment.ReactionSadCount++;
                    break;
                case ReactionType.Angry:
                    comment.ReactionAngryCount++;
                    break;
            }
        }

        /// <summary>
        /// ลด count ของ reaction ตามประเภท
        /// </summary>
        private void DecrementReactionCount(Domain.Entities.Comment comment, ReactionType reactionType)
        {
            switch (reactionType)
            {
                case ReactionType.Like:
                    comment.ReactionLikeCount = Math.Max(0, comment.ReactionLikeCount - 1);
                    break;
                case ReactionType.Love:
                    comment.ReactionLoveCount = Math.Max(0, comment.ReactionLoveCount - 1);
                    break;
                case ReactionType.Haha:
                    comment.ReactionHahaCount = Math.Max(0, comment.ReactionHahaCount - 1);
                    break;
                case ReactionType.Wow:
                    comment.ReactionWowCount = Math.Max(0, comment.ReactionWowCount - 1);
                    break;
                case ReactionType.Sad:
                    comment.ReactionSadCount = Math.Max(0, comment.ReactionSadCount - 1);
                    break;
                case ReactionType.Angry:
                    comment.ReactionAngryCount = Math.Max(0, comment.ReactionAngryCount - 1);
                    break;
            }
        }

        /// <summary>
        /// ดึง reaction counts ทั้งหมด
        /// </summary>
        private CommentReactionCounts GetReactionCounts(Domain.Entities.Comment comment)
        {
            return new CommentReactionCounts
            {
                LikeCount = comment.ReactionLikeCount,
                LoveCount = comment.ReactionLoveCount,
                HahaCount = comment.ReactionHahaCount,
                WowCount = comment.ReactionWowCount,
                SadCount = comment.ReactionSadCount,
                AngryCount = comment.ReactionAngryCount
            };
        }
    }
}