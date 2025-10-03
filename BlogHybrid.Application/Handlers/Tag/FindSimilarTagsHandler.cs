// BlogHybrid.Application/Handlers/Tag/FindSimilarTagsHandler.cs
using BlogHybrid.Application.Interfaces.Services;
using BlogHybrid.Application.Queries.Tag;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BlogHybrid.Application.Handlers.Tag
{
    public class FindSimilarTagsHandler : IRequestHandler<FindSimilarTagsQuery, List<SimilarTagResult>>
    {
        private readonly ITagSimilarityService _similarityService;
        private readonly ILogger<FindSimilarTagsHandler> _logger;

        public FindSimilarTagsHandler(
            ITagSimilarityService similarityService,
            ILogger<FindSimilarTagsHandler> logger)
        {
            _similarityService = similarityService;
            _logger = logger;
        }

        public async Task<List<SimilarTagResult>> Handle(FindSimilarTagsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                return await _similarityService.FindSimilarTagsAsync(
                    request.TagName,
                    request.Limit,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding similar tags for: {TagName}", request.TagName);
                return new List<SimilarTagResult>();
            }
        }
    }
}