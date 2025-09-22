using AutoMapper;
using BlogHybrid.Application.DTOs.Category;
using BlogHybrid.Application.Interfaces.Repositories;
using BlogHybrid.Application.Queries.Category;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BlogHybrid.Application.Handlers.Category
{
    public class GetActiveCategoriesHandler : IRequestHandler<GetActiveCategoriesQuery, List<CategoryDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetActiveCategoriesHandler> _logger;

        public GetActiveCategoriesHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetActiveCategoriesHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<CategoryDto>> Handle(GetActiveCategoriesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var categories = await _unitOfWork.Categories.GetActiveAsync(cancellationToken);

                // Apply ordering
                if (request.OrderByName)
                {
                    categories = categories.OrderBy(c => c.Name).ToList();
                }
                // else already ordered by SortOrder in repository

                var categoryDtos = new List<CategoryDto>();

                foreach (var category in categories)
                {
                    var dto = _mapper.Map<CategoryDto>(category);
                    dto.PostCount = await _unitOfWork.Categories.GetPostCountAsync(category.Id, cancellationToken);
                    categoryDtos.Add(dto);
                }

                return categoryDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active categories");
                return new List<CategoryDto>();
            }
        }
    }
}