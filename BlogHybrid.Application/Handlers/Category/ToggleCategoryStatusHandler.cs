using BlogHybrid.Application.Commands.Category;
using BlogHybrid.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                var category = await _unitOfWork.Categories.GetByIdAsync(request.Id, cancellationToken);

                if (category == null)
                {
                    return new ToggleCategoryStatusResult
                    {
                        Success = false,
                        Errors = new List<string> { "ไม่พบหมวดหมู่ที่ต้องการเปลี่ยนสถานะ" }
                    };
                }

                // Toggle the status
                category.IsActive = request.IsActive;

                await _unitOfWork.Categories.UpdateAsync(category, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Category status toggled: {CategoryId} - {Status}",
                    category.Id, request.IsActive ? "Active" : "Inactive");

                return new ToggleCategoryStatusResult
                {
                    Success = true,
                    NewStatus = category.IsActive,
                    Message = $"เปลี่ยนสถานะเป็น{(category.IsActive ? "เปิดใช้งาน" : "ปิดใช้งาน")}แล้ว"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling category status: {CategoryId}", request.Id);
                return new ToggleCategoryStatusResult
                {
                    Success = false,
                    Errors = new List<string> { "เกิดข้อผิดพลาดในการเปลี่ยนสถานะ" }
                };
            }
        }
    }
}
