using AutoMapper;
using BlogHybrid.Application.DTOs.Category;
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
    public class GetCategoriesHandler : IRequestHandler<GetCategoriesQuery, CategoryListDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetCategoriesHandler> _logger;

        public GetCategoriesHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetCategoriesHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<CategoryListDto> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var (categories, totalCount) = await _unitOfWork.Categories.GetPagedAsync(
                    request.PageNumber,
                    request.PageSize,
                    request.SearchTerm,
                    request.IsActive,
                    request.SortBy,
                    request.SortDirection,
                    cancellationToken);

                var categoryDtos = new List<CategoryDto>();

                foreach (var category in categories)
                {
                    var dto = _mapper.Map<CategoryDto>(category);

                    // Get post count for each category
                    dto.PostCount = await _unitOfWork.Categories.GetPostCountAsync(category.Id, cancellationToken);

                    categoryDtos.Add(dto);
                }

                return new CategoryListDto
                {
                    Categories = categoryDtos,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting categories");
                return new CategoryListDto
                {
                    Categories = new List<CategoryDto>(),
                    TotalCount = 0,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };
            }
        }
    }
}
