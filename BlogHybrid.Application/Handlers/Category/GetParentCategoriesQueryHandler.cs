using AutoMapper;
using BlogHybrid.Application.DTOs.Category;
using BlogHybrid.Application.Interfaces.Repositories;
using BlogHybrid.Application.Queries.Category;
using MediatR;

namespace BlogHybrid.Application.Handlers.Queries.Category
{
    public class GetParentCategoriesQueryHandler : IRequestHandler<GetParentCategoriesQuery, List<CategoryDto>>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public GetParentCategoriesQueryHandler(
            ICategoryRepository categoryRepository,
            IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public async Task<List<CategoryDto>> Handle(GetParentCategoriesQuery request, CancellationToken cancellationToken)
        {
            var categories = await _categoryRepository.GetParentCategoriesAsync(cancellationToken);

            return categories.Select(cat => new CategoryDto
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
                ParentCategoryId = null,
                ParentCategoryName = null,
                IsParentCategory = true,
                SubCategoryCount = cat.SubCategoryCount
            }).ToList();
        }
    }
}