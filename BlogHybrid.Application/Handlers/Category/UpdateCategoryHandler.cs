using BlogHybrid.Application.Commands.Category;
using BlogHybrid.Application.Common;
using BlogHybrid.Application.Interfaces.Repositories;
using BlogHybrid.Application.Queries.Category;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogHybrid.Application.Handlers.Category
{
    public class UpdateCategoryHandler : IRequestHandler<UpdateCategoryCommand, UpdateCategoryResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateCategoryHandler> _logger;

        public UpdateCategoryHandler(
            IUnitOfWork unitOfWork,
            ILogger<UpdateCategoryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<UpdateCategoryResult> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Find existing category
                var category = await _unitOfWork.Categories.GetByIdAsync(request.Id, cancellationToken);

                if (category == null)
                {
                    return new UpdateCategoryResult
                    {
                        Success = false,
                        Errors = new List<string> { "ไม่พบหมวดหมู่ที่ต้องการแก้ไข" }
                    };
                }

                // Validate input
                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    return new UpdateCategoryResult
                    {
                        Success = false,
                        Errors = new List<string> { "ชื่อหมวดหมู่เป็นข้อมูลที่จำเป็น" }
                    };
                }

                // Generate new slug if name changed
                var slug = category.Slug;
                if (category.Name != request.Name.Trim())
                {
                    slug = await SlugGenerator.GenerateUniqueSlug(
                        request.Name,
                        async (slugToCheck, excludeId) => await _unitOfWork.Categories.SlugExistsAsync(slugToCheck, excludeId, cancellationToken),
                        category.Id
                    );
                }

                // Update category
                category.Name = request.Name.Trim();
                category.Slug = slug;
                category.Description = request.Description?.Trim() ?? string.Empty;
                category.ImageUrl = request.ImageUrl?.Trim();
                category.Color = request.Color.Trim();
                category.IsActive = request.IsActive;
                category.SortOrder = request.SortOrder;

                await _unitOfWork.Categories.UpdateAsync(category, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Category updated: {CategoryId} - {CategoryName}", category.Id, category.Name);

                return new UpdateCategoryResult
                {
                    Success = true,
                    Slug = category.Slug,
                    Message = "แก้ไขหมวดหมู่เรียบร้อยแล้ว"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category: {CategoryId}", request.Id);
                return new UpdateCategoryResult
                {
                    Success = false,
                    Errors = new List<string> { "เกิดข้อผิดพลาดในการแก้ไขหมวดหมู่" }
                };
            }
        }
    }
}
