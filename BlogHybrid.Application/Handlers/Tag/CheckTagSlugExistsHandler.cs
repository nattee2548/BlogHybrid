// BlogHybrid.Application/Handlers/Tag/CheckTagSlugExistsHandler.cs
using BlogHybrid.Application.Interfaces.Repositories;
using BlogHybrid.Application.Queries.Tag;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BlogHybrid.Application.Handlers.Tag
{
    public class CheckTagSlugExistsHandler : IRequestHandler<CheckTagSlugExistsQuery, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CheckTagSlugExistsHandler> _logger;

        public CheckTagSlugExistsHandler(
            IUnitOfWork unitOfWork,
            ILogger<CheckTagSlugExistsHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<bool> Handle(CheckTagSlugExistsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                return await _unitOfWork.Tags.SlugExistsAsync(request.Slug, request.ExcludeId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking slug exists: {Slug}", request.Slug);
                return true;
            }
        }
    }
}