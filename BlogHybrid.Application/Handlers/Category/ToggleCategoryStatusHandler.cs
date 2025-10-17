// BlogHybrid.Application/Handlers/Category/ToggleCategoryStatusHandler.cs
using BlogHybrid.Application.Commands.Category;
using BlogHybrid.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BlogHybrid.Application.Handlers.Category
{
    public class ToggleCategoryStatusHandler : IRequestHandler<ToggleCategoryStatusCommand, ToggleCategoryStatusResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ToggleCategoryStatusHandler> _logger;

        public ToggleCategoryStatusHandler(
            IUnitOfWork unitOfWork,
            ILogger<ToggleCategoryStatusHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ToggleCategoryStatusResult> Handle(ToggleCategoryStatusCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var category = await _unitOfWork.Categories.GetByIdAsync(request.CategoryId, cancellationToken);

                if (category == null)
                {
                    return new ToggleCategoryStatusResult
                    {
                        Success = false,
                        Errors = new List<string> { "ไม่พบหมวดหมู่ที่ต้องการ" }
                    };
                }

                // Toggle status
                category.IsActive = !category.IsActive;

                await _unitOfWork.Categories.UpdateAsync(category, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var statusText = category.IsActive ? "เปิดใช้งาน" : "ปิดใช้งาน";
                _logger.LogInformation("Category {CategoryId} status toggled to: {Status}", request.CategoryId, statusText);

                return new ToggleCategoryStatusResult
                {
                    Success = true,
                    NewStatus = category.IsActive,
                    Message = $"{statusText}หมวดหมู่ '{category.Name}' เรียบร้อยแล้ว"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling category status for ID: {CategoryId}", request.CategoryId);
                return new ToggleCategoryStatusResult
                {
                    Success = false,
                    Errors = new List<string> { "เกิดข้อผิดพลาดในการเปลี่ยนสถานะ" }
                };
            }
        }
    }
}