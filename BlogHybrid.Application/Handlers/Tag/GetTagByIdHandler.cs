// BlogHybrid.Application/Handlers/Tag/GetTagByIdHandler.cs
using AutoMapper;
using BlogHybrid.Application.DTOs.Tag;
using BlogHybrid.Application.Interfaces.Repositories;
using BlogHybrid.Application.Queries.Tag;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BlogHybrid.Application.Handlers.Tag
{
    public class GetTagByIdHandler : IRequestHandler<GetTagByIdQuery, TagDto?>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetTagByIdHandler> _logger;

        public GetTagByIdHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetTagByIdHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<TagDto?> Handle(GetTagByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var tag = await _unitOfWork.Tags.GetByIdAsync(request.Id, cancellationToken);

                if (tag == null)
                    return null;

                var dto = _mapper.Map<TagDto>(tag);
                dto.PostCount = await _unitOfWork.Tags.GetPostCountAsync(tag.Id, cancellationToken);

                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tag by id: {TagId}", request.Id);
                return null;
            }
        }
    }
}