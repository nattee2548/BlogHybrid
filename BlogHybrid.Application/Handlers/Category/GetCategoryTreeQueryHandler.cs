using AutoMapper;
using BlogHybrid.Application.DTOs.Category;
using BlogHybrid.Application.Interfaces.Repositories;
using BlogHybrid.Application.Queries.Category;
using MediatR;

namespace BlogHybrid.Application.Handlers.Queries.Category
{
    public class GetCategoryTreeQueryHandler : IRequestHandler<GetCategoryTreeQuery, List<CategoryDto>>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public GetCategoryTreeQueryHandler(
            ICategoryRepository categoryRepository,
            IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public async Task<List<CategoryDto>> Handle(GetCategoryTreeQuery request, CancellationToken cancellationToken)
        {
            // ดึงหมวดหมู่แบบ tree (parent + children)
            var categories = await _categoryRepository.GetCategoryTreeAsync(cancellationToken);

            // Map to DTO พร้อม SubCategories
            var categoryDtos = categories.Select(cat => new CategoryDto
            {
                Id = cat.Id,
                Name = cat.Name,
                Slug = cat.Slug,
                Description = cat.Description,
                ImageUrl = cat.ImageUrl,
                Color = cat.Color,
                IsActive = cat.IsActive,
                SortOrder = cat.SortOrder,
                CreatedAt = cat.CreatedAt,
                ParentCategoryId = cat.ParentCategoryId,
                ParentCategoryName = cat.ParentCategory?.Name,
                IsParentCategory = cat.IsParentCategory,
                SubCategoryCount = cat.SubCategoryCount,
                SubCategories = cat.SubCategories?.Select(sub => new CategoryDto
                {
                    Id = sub.Id,
                    Name = sub.Name,
                    Slug = sub.Slug,
                    Description = sub.Description,
                    ImageUrl = sub.ImageUrl,
                    Color = sub.Color,
                    IsActive = sub.IsActive,
                    SortOrder = sub.SortOrder,
                    CreatedAt = sub.CreatedAt,
                    ParentCategoryId = sub.ParentCategoryId,
                    ParentCategoryName = cat.Name,
                    IsParentCategory = false,
                    SubCategoryCount = 0
                }).OrderBy(s => s.SortOrder).ThenBy(s => s.Name).ToList()
            }).ToList();

            return categoryDtos;
        }
    }
}