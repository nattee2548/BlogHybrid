using BlogHybrid.Application.Commands.Category;
using BlogHybrid.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BlogHybrid.Application.Handlers.Category
{
    public class DeleteCategoryHandler : IRequestHandler<DeleteCategoryCommand, DeleteCategoryResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeleteCategoryHandler> _logger;

        public DeleteCategoryHandler(
            IUnitOfWork unitOfWork,
            ILogger<DeleteCategoryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<DeleteCategoryResult> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var category = await _unitOfWork.Categories.GetByIdAsync(request.Id, cancellationToken);

                if (category == null)
                {
                    return new DeleteCategoryResult
                    {
                        Success = false,
                        Errors = new List<string> { "ไม่พบหมวดหมู่ที่ต้องการลบ" }
                    };
                }

                var postCount = await _unitOfWork.Categories.GetPostCountAsync(request.Id, cancellationToken);

                // Check if category has posts
                if (postCount > 0 && !request.ForceDelete)
                {
                    return new DeleteCategoryResult
                    {
                        Success = false,
                        HasPosts = true,
                        PostCount = postCount,
                        Errors = new List<string> { $"ไม่สามารถลบหมวดหมู่ได้ เนื่องจากมีบทความ {postCount} รายการ" }
                    };
                }

                if (request.ForceDelete && postCount > 0)
                {
                    return new DeleteCategoryResult
                    {
                        Success = false,
                        HasPosts = true,
                        PostCount = postCount,
                        Errors = new List<string> { "กรุณาย้ายหรือลบบทความในหมวดหมู่นี้ก่อน" }
                    };
                }

                await _unitOfWork.Categories.DeleteAsync(category, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Category deleted: {CategoryId} - {CategoryName}", category.Id, category.Name);

                return new DeleteCategoryResult
                {
                    Success = true,
                    Message = "ลบหมวดหมู่เรียบร้อยแล้ว"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category: {CategoryId}", request.Id);
                return new DeleteCategoryResult
                {
                    Success = false,
                    Errors = new List<string> { "เกิดข้อผิดพลาดในการลบหมวดหมู่" }
                };
            }
        }
    }
}
