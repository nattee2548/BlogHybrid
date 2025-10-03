// BlogHybrid.Application/Handlers/Tag/GetTagsHandler.cs
using AutoMapper;
using BlogHybrid.Application.DTOs.Tag;
using BlogHybrid.Application.Interfaces.Repositories;
using BlogHybrid.Application.Queries.Tag;
using BlogHybrid.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace BlogHybrid.Application.Handlers.Tag
{
    public class GetTagsHandler : IRequestHandler<GetTagsQuery, TagListDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<GetTagsHandler> _logger;

        public GetTagsHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            UserManager<ApplicationUser> userManager,
            ILogger<GetTagsHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<TagListDto> Handle(GetTagsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var (tags, totalCount) = await _unitOfWork.Tags.GetPagedAsync(
                    request.PageNumber,
                    request.PageSize,
                    request.SearchTerm,
                    request.SortBy,
                    request.SortDirection,
                    cancellationToken);

                var tagDtos = new List<TagDto>();

                foreach (var tag in tags)
                {
                    var dto = _mapper.Map<TagDto>(tag);
                    dto.PostCount = await _unitOfWork.Tags.GetPostCountAsync(tag.Id, cancellationToken);

                    if (!string.IsNullOrEmpty(tag.CreatedBy))
                    {
                        var creator = await _userManager.FindByIdAsync(tag.CreatedBy);
                        dto.CreatorName = creator?.DisplayName ?? creator?.Email ?? "Unknown";
                    }

                    tagDtos.Add(dto);
                }

                return new TagListDto
                {
                    Tags = tagDtos,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tags");
                return new TagListDto
                {
                    Tags = new List<TagDto>(),
                    TotalCount = 0,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };
            }
        }
    }
}