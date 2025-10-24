// BlogHybrid.Application/Handlers/Comment/AddCommentHandler.cs
using BlogHybrid.Application.Commands.Comment;
using BlogHybrid.Application.Interfaces.Repositories;
using BlogHybrid.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BlogHybrid.Application.Handlers.Comment
{
    public class AddCommentHandler : IRequestHandler<AddCommentCommand, AddCommentResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AddCommentHandler> _logger;

        public AddCommentHandler(
            IUnitOfWork unitOfWork,
            ILogger<AddCommentHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<AddCommentResult> Handle(
            AddCommentCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                // ตรวจสอบว่าโพสต์มีอยู่และเปิดให้ comment ได้
                var post = await _unitOfWork.Posts.GetByIdAsync(request.PostId, cancellationToken);
                if (post == null || !post.IsPublished || post.IsDeleted)
                {
                    return new AddCommentResult
                    {
                        Success = false,
                        Errors = new List<string> { "ไม่พบโพสต์หรือโพสต์ถูกลบแล้ว" }
                    };
                }

                // ถ้าเป็น reply ต้องเช็คว่า parent comment มีอยู่
                if (request.ParentCommentId.HasValue)
                {
                    var parentComment = await _unitOfWork.DbContext.Set<Domain.Entities.Comment>()
                        .FirstOrDefaultAsync(c => c.Id == request.ParentCommentId.Value && c.PostId == request.PostId,
                                           cancellationToken);

                    if (parentComment == null)
                    {
                        return new AddCommentResult
                        {
                            Success = false,
                            Errors = new List<string> { "ไม่พบความคิดเห็นที่ต้องการตอบกลับ" }
                        };
                    }
                }

                // สร้าง comment ใหม่
                var comment = new Domain.Entities.Comment
                {
                    PostId = request.PostId,
                    Content = request.Content.Trim(),
                    AuthorId = request.AuthorId,
                    ParentCommentId = request.ParentCommentId,
                    IsApproved = true, // Auto-approve (อาจจะเพิ่มระบบ moderation ภายหลัง)
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _unitOfWork.DbContext.Set<Domain.Entities.Comment>().AddAsync(comment, cancellationToken);

                // อัปเดต comment count ของโพสต์
                post.CommentCount++;
                await _unitOfWork.Posts.UpdateAsync(post, cancellationToken);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Comment added: CommentId={CommentId}, PostId={PostId}, AuthorId={AuthorId}, IsReply={IsReply}",
                    comment.Id, request.PostId, request.AuthorId, request.ParentCommentId.HasValue
                );

                return new AddCommentResult
                {
                    Success = true,
                    CommentId = comment.Id,
                    Message = "เพิ่มความคิดเห็นสำเร็จ"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding comment to post: {PostId}", request.PostId);
                return new AddCommentResult
                {
                    Success = false,
                    Errors = new List<string> { "เกิดข้อผิดพลาดในการเพิ่มความคิดเห็น" }
                };
            }
        }
    }
}