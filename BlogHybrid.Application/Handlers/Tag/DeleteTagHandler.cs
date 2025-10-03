// BlogHybrid.Application/Handlers/Tag/DeleteTagHandler.cs
using BlogHybrid.Application.Commands.Tag;
using BlogHybrid.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BlogHybrid.Application.Handlers.Tag
{
    public class DeleteTagHandler : IRequestHandler<DeleteTagCommand, DeleteTagResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeleteTagHandler> _logger;

        public DeleteTagHandler(
            IUnitOfWork unitOfWork,
            ILogger<DeleteTagHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<DeleteTagResult> Handle(DeleteTagCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var tag = await _unitOfWork.Tags.GetByIdAsync(request.Id, cancellationToken);

                if (tag == null)
                {
                    return new DeleteTagResult
                    {
                        Success = false,
                        Errors = new List<string> { "ไม่พบแท็กที่ต้องการลบ" }
                    };
                }

                var postCount = await _unitOfWork.Tags.GetPostCountAsync(request.Id, cancellationToken);

                if (postCount > 0 && !request.ForceDelete)
                {
                    return new DeleteTagResult
                    {
                        Success = false,
                        HasPosts = true,
                        PostCount = postCount,
                        Errors = new List<string> { $"ไม่สามารถลบแท็กได้ เนื่องจากมีบทความ {postCount} รายการ" }
                    };
                }

                if (request.ForceDelete && postCount > 0)
                {
                    return new DeleteTagResult
                    {
                        Success = false,
                        HasPosts = true,
                        PostCount = postCount,
                        Errors = new List<string> { "กรุณาลบแท็กออกจากบทความก่อน" }
                    };
                }

                await _unitOfWork.Tags.DeleteAsync(tag, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Tag deleted: {TagId} - {TagName}", tag.Id, tag.Name);

                return new DeleteTagResult
                {
                    Success = true,
                    Message = "ลบแท็กเรียบร้อยแล้ว"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting tag: {TagId}", request.Id);
                return new DeleteTagResult
                {
                    Success = false,
                    Errors = new List<string> { "เกิดข้อผิดพลาดในการลบแท็ก" }
                };
            }
        }
    }
}