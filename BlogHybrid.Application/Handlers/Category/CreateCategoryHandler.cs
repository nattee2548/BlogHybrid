using BlogHybrid.Application.Commands.Category;
using BlogHybrid.Application.Common;
using BlogHybrid.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BlogHybrid.Application.Handlers.Category
{
    public class CreateCategoryHandler : IRequestHandler<CreateCategoryCommand, CreateCategoryResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMediator _mediator;
        private readonly ILogger<CreateCategoryHandler> _logger;

        public CreateCategoryHandler(
            IUnitOfWork unitOfWork,
            IMediator mediator,
            ILogger<CreateCategoryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<CreateCategoryResult> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    return new CreateCategoryResult
                    {
                        Success = false,
                        Errors = new List<string> { "ชื่อหมวดหมู่เป็นข้อมูลที่จำเป็น" }
                    };
                }

                // เริ่ม Transaction
                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                // Validate ParentCategoryId ถ้ามีการระบุ
                if (request.ParentCategoryId.HasValue)
                {
                    var parentCategory = await _unitOfWork.Categories.GetByIdAsync(request.ParentCategoryId.Value, cancellationToken);
                    if (parentCategory == null)
                    {
                        await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                        return new CreateCategoryResult
                        {
                            Success = false,
                            Errors = new List<string> { "ไม่พบหมวดหมู่หลักที่ระบุ" }
                        };
                    }

                    // ป้องกันการสร้าง subcategory ของ subcategory (ถ้าต้องการให้มีแค่ 2 level)
                    if (parentCategory.ParentCategoryId.HasValue)
                    {
                        await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                        return new CreateCategoryResult
                        {
                            Success = false,
                            Errors = new List<string> { "ไม่สามารถสร้างหมวดหมู่ย่อยของหมวดหมู่ย่อยได้" }
                        };
                    }
                }

                // เช็คว่ามี slug มาจาก view หรือไม่ ถ้าไม่มีให้ generate
                string slug;
                if (!string.IsNullOrWhiteSpace(request.Slug))
                {
                    slug = SlugGenerator.GenerateSlug(request.Slug);

                    var slugExists = await _unitOfWork.Categories.SlugExistsAsync(slug, null, cancellationToken);
                    if (slugExists)
                    {
                        slug = await SlugGenerator.GenerateUniqueSlug(
                            request.Slug,
                            async (slugToCheck, excludeId) => await _unitOfWork.Categories.SlugExistsAsync(slugToCheck, excludeId, cancellationToken)
                        );
                    }
                }
                else
                {
                    slug = await SlugGenerator.GenerateUniqueSlug(
                        request.Name,
                        async (slugToCheck, excludeId) => await _unitOfWork.Categories.SlugExistsAsync(slugToCheck, excludeId, cancellationToken)
                    );
                }

                // Auto-assign sort order if not provided
                if (request.SortOrder <= 0)
                {
                    var maxSortOrder = await _unitOfWork.Categories.GetMaxSortOrderAsync(cancellationToken);
                    request.SortOrder = maxSortOrder + 1;
                }

                // Create category
                var category = new BlogHybrid.Domain.Entities.Category
                {
                    Name = request.Name.Trim(),
                    Slug = slug,
                    Description = request.Description?.Trim() ?? string.Empty,
                    Color = request.Color?.Trim() ?? "#0066cc",
                    ImageUrl = request.ImageUrl?.Trim(),
                    SortOrder = request.SortOrder,
                    IsActive = request.IsActive,
                    ParentCategoryId = request.ParentCategoryId,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Categories.AddAsync(category, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Commit Transaction
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                _logger.LogInformation("Created category: {CategoryName} with slug: {Slug}, ParentCategoryId: {ParentCategoryId}",
                    category.Name, category.Slug, category.ParentCategoryId);

                return new CreateCategoryResult
                {
                    Success = true,
                    CategoryId = category.Id,
                    Slug = category.Slug,
                    Message = "สร้างหมวดหมู่สำเร็จ"
                };
            }
            catch (Exception ex)
            {
                // Rollback Transaction
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);

                _logger.LogError(ex, "Error creating category: {CategoryName}", request.Name);

                return new CreateCategoryResult
                {
                    Success = false,
                    Errors = new List<string> { "เกิดข้อผิดพลาดในการสร้างหมวดหมู่" }
                };
            }
        }
    }
}