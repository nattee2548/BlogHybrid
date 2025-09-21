using BlogHybrid.Application.Commands.Category;
using BlogHybrid.Application.Common;
using BlogHybrid.Application.Interfaces.Repositories;
using BlogHybrid.Application.Queries.Category;
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

                // Generate unique slug
                var slug = await SlugGenerator.GenerateUniqueSlug(
                    request.Name,
                    async (slugToCheck, excludeId) => await _unitOfWork.Categories.SlugExistsAsync(slugToCheck, excludeId, cancellationToken)
                );

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
                    ImageUrl = request.ImageUrl?.Trim(),
                    Color = request.Color.Trim(),
                    IsActive = request.IsActive,
                    SortOrder = request.SortOrder,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Categories.AddAsync(category, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Category created: {CategoryId} - {CategoryName}", category.Id, category.Name);

                return new CreateCategoryResult
                {
                    Success = true,
                    CategoryId = category.Id,
                    Slug = category.Slug,
                    Message = "สร้างหมวดหมู่เรียบร้อยแล้ว"
                };
            }
            catch (Exception ex)
            {
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
