// BlogHybrid.Application/Handlers/Tag/BulkCreateTagsHandler.cs
using BlogHybrid.Application.Commands.Tag;
using BlogHybrid.Application.Common;
using BlogHybrid.Application.Interfaces.Repositories;
using BlogHybrid.Application.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BlogHybrid.Application.Handlers.Tag
{
    public class BulkCreateTagsHandler : IRequestHandler<BulkCreateTagsCommand, BulkCreateTagsResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITagSimilarityService _similarityService;
        private readonly ILogger<BulkCreateTagsHandler> _logger;

        public BulkCreateTagsHandler(
            IUnitOfWork unitOfWork,
            ITagSimilarityService similarityService,
            ILogger<BulkCreateTagsHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _similarityService = similarityService;
            _logger = logger;
        }

        public async Task<BulkCreateTagsResult> Handle(BulkCreateTagsCommand request, CancellationToken cancellationToken)
        {
            var result = new BulkCreateTagsResult { Success = true };

            try
            {
                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                foreach (var tagName in request.TagNames.Distinct())
                {
                    if (string.IsNullOrWhiteSpace(tagName))
                        continue;

                    var trimmedName = tagName.Trim();

                    var slug = SlugGenerator.GenerateSlug(trimmedName);
                    var existingTag = await _unitOfWork.Tags.GetBySlugAsync(slug, cancellationToken);

                    if (existingTag != null)
                    {
                        result.ExistingTags.Add(new ExistingTagInfo
                        {
                            TagId = existingTag.Id,
                            Name = existingTag.Name,
                            Slug = existingTag.Slug
                        });
                        continue;
                    }

                    var similarTags = await _similarityService.FindSimilarTagsAsync(trimmedName, 3, cancellationToken);

                    if (similarTags.Any(t => t.SimilarityScore >= 85))
                    {
                        var bestMatch = similarTags.First();
                        result.ExistingTags.Add(new ExistingTagInfo
                        {
                            TagId = bestMatch.TagId,
                            Name = bestMatch.Name,
                            Slug = bestMatch.Slug
                        });

                        result.SimilarTagWarnings.Add(new SimilarTagWarning
                        {
                            RequestedName = trimmedName,
                            SimilarTags = similarTags
                        });
                        continue;
                    }

                    var uniqueSlug = await SlugGenerator.GenerateUniqueSlug(
                        trimmedName,
                        async (slugToCheck, excludeId) =>
                            await _unitOfWork.Tags.SlugExistsAsync(slugToCheck, excludeId, cancellationToken)
                    );

                    var newTag = new BlogHybrid.Domain.Entities.Tag
                    {
                        Name = trimmedName,
                        Slug = uniqueSlug,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = request.CreatedBy
                    };

                    await _unitOfWork.Tags.AddAsync(newTag, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    result.CreatedTags.Add(new CreatedTagInfo
                    {
                        TagId = newTag.Id,
                        Name = newTag.Name,
                        Slug = newTag.Slug
                    });

                    if (similarTags.Any(t => t.SimilarityScore >= 60))
                    {
                        result.SimilarTagWarnings.Add(new SimilarTagWarning
                        {
                            RequestedName = trimmedName,
                            SimilarTags = similarTags
                        });
                    }
                }

                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                _logger.LogInformation(
                    "Bulk created {CreatedCount} tags, found {ExistingCount} existing, {WarningCount} warnings",
                    result.CreatedTags.Count,
                    result.ExistingTags.Count,
                    result.SimilarTagWarnings.Count);

                return result;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                _logger.LogError(ex, "Error in bulk create tags");

                result.Success = false;
                result.Errors.Add("เกิดข้อผิดพลาดในการสร้างแท็กแบบ bulk");
                return result;
            }
        }
    }
}