// BlogHybrid.Application/Handlers/Category/GetCategoryByIdHandler.cs
using AutoMapper;
using BlogHybrid.Application.DTOs.Category;
using BlogHybrid.Application.Interfaces.Repositories;
using BlogHybrid.Application.Queries.Category;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BlogHybrid.Application.Handlers.Category
{
    public class GetCategoryByIdHandler : IRequestHandler<GetCategoryByIdQuery, CategoryDto?>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetCategoryByIdHandler> _logger;

        public GetCategoryByIdHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetCategoryByIdHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<CategoryDto?> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var category = await _unitOfWork.Categories.GetByIdAsync(request.Id, cancellationToken);

                if (category == null)
                {
                    return null;
                }

                var dto = _mapper.Map<CategoryDto>(category);

                // Get post count
                dto.PostCount = await _unitOfWork.Categories.GetPostCountAsync(category.Id, cancellationToken);

                // ✅ เพิ่ม: Get community count
                dto.CommunityCount = await _unitOfWork.Categories.GetCommunityCountAsync(category.Id, cancellationToken);

                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category by ID: {CategoryId}", request.Id);
                return null;
            }
        }
    }
}