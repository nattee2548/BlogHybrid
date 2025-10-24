using BlogHybrid.Application.Commands.Post;
using BlogHybrid.Application.Interfaces.Repositories;
using BlogHybrid.Application.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BlogHybrid.Application.Handlers.Post
{
    public class DeletePostHandler : IRequestHandler<DeletePostCommand, DeletePostResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IImageService _imageService;
        private readonly ILogger<DeletePostHandler> _logger;

        public DeletePostHandler(
            IUnitOfWork unitOfWork,
            IImageService imageService,
            ILogger<DeletePostHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _imageService = imageService;
            _logger = logger;
        }

        public async Task<DeletePostResult> Handle(
            DeletePostCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                // ดึงโพสต์
                var post = await _unitOfWork.Posts.GetByIdAsync(request.Id, cancellationToken);

                if (post == null)
                {
                    return new DeletePostResult
                    {
                        Success = false,
                        Errors = new List<string> { "ไม่พบโพสต์ที่ต้องการลบ" }
                    };
                }

                // ตรวจสอบสิทธิ์
                if (post.AuthorId != request.CurrentUserId)
                {
                    _logger.LogWarning(
                        "User {UserId} attempted to delete post {PostId} owned by {OwnerId}",
                        request.CurrentUserId, post.Id, post.AuthorId
                    );

                    return new DeletePostResult
                    {
                        Success = false,
                        Errors = new List<string> { "คุณไม่มีสิทธิ์ลบโพสต์นี้" }
                    };
                }

                // ลบรูปภาพถ้ามี (Soft Delete - ไม่ลบจริง)
                // if (!string.IsNullOrEmpty(post.FeaturedImageUrl))
                // {
                //     try
                //     {
                //         await _imageService.DeleteAsync(post.FeaturedImageUrl);
                //     }
                //     catch (Exception ex)
                //     {
                //         _logger.LogWarning(ex, "Failed to delete featured image for post {PostId}", post.Id);
                //     }
                // }

                // Soft Delete
                post.IsDeleted = true;
                post.UpdatedAt = DateTime.UtcNow;
                post.DeletedAt = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Post {PostId} soft deleted by user {UserId}", post.Id, request.CurrentUserId);

                return new DeletePostResult
                {
                    Success = true,
                    Message = "ลบโพสต์สำเร็จ"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting post {PostId}", request.Id);

                return new DeletePostResult
                {
                    Success = false,
                    Errors = new List<string> { "เกิดข้อผิดพลาดในการลบโพสต์" }
                };
            }
        }
    }
}