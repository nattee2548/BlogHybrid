// BlogHybrid.Application/Handlers/Tag/GetTagBySlugHandler.cs
using AutoMapper;
using BlogHybrid.Application.DTOs.Tag;
using BlogHybrid.Application.Interfaces.Repositories;
using BlogHybrid.Application.Queries.Tag;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BlogHybrid.Application.Handlers.Tag
{
    public class GetTagBySlugHandler : IRequestHandler<GetTagBySlugQuery, TagDto?>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetTagBySlugHandler> _logger;

        public GetTagBySlugHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetTagBySlugHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<TagDto?> Handle(GetTagBySlugQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var tag = await _unitOfWork.Tags.GetBySlugAsync(request.Slug, cancellationToken);

                if (tag == null)
                    return null;

                var dto = _mapper.Map<TagDto>(tag);
                dto.PostCount = await _unitOfWork.Tags.GetPostCountAsync(tag.Id, cancellationToken);

                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tag by slug: {Slug}", request.Slug);
                return null;
            }
        }
    }
}