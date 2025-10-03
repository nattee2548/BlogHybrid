// BlogHybrid.Application/Handlers/Tag/SearchTagsHandler.cs
using AutoMapper;
using BlogHybrid.Application.DTOs.Tag;
using BlogHybrid.Application.Interfaces.Repositories;
using BlogHybrid.Application.Queries.Tag;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BlogHybrid.Application.Handlers.Tag
{
    public class SearchTagsHandler : IRequestHandler<SearchTagsQuery, List<TagDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<SearchTagsHandler> _logger;

        public SearchTagsHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<SearchTagsHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<TagDto>> Handle(SearchTagsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var tags = await _unitOfWork.Tags.SearchAsync(
                    request.SearchTerm,
                    request.Limit,
                    cancellationToken);

                var tagDtos = new List<TagDto>();

                foreach (var tag in tags)
                {
                    var dto = _mapper.Map<TagDto>(tag);
                    dto.PostCount = await _unitOfWork.Tags.GetPostCountAsync(tag.Id, cancellationToken);
                    tagDtos.Add(dto);
                }

                return tagDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching tags: {SearchTerm}", request.SearchTerm);
                return new List<TagDto>();
            }
        }
    }
}