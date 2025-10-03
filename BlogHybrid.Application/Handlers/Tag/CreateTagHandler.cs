// BlogHybrid.Application/Handlers/Tag/CreateTagHandler.cs
using BlogHybrid.Application.Commands.Tag;
using BlogHybrid.Application.Common;
using BlogHybrid.Application.Interfaces.Repositories;
using BlogHybrid.Application.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BlogHybrid.Application.Handlers.Tag
{
    public class CreateTagHandler : IRequestHandler<CreateTagCommand, CreateTagResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITagSimilarityService _similarityService;
        private readonly ILogger<CreateTagHandler> _logger;

        public CreateTagHandler(
            IUnitOfWork unitOfWork,
            ITagSimilarityService similarityService,
            ILogger<CreateTagHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _similarityService = similarityService;
            _logger = logger;
        }

        public async Task<CreateTagResult> Handle(CreateTagCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    return new CreateTagResult
                    {
                        Success = false,
                        Errors = new List<string> { "ชื่อแท็กเป็นข้อมูลที่จำเป็น" }
                    };
                }

                // ✅ AI Duplicate Detection
                var similarTags = await _similarityService.FindSimilarTagsAsync(
                    request.Name,
                    limit: 3,
                    cancellationToken);

                if (similarTags.Any(t => t.SimilarityScore >= 90))
                {
                    var mostSimilar = similarTags.First();
                    return new CreateTagResult
                    {
                        Success = false,
                        Errors = new List<string>
                        {
                            $"มีแท็กที่คล้ายกันมากอยู่แล้ว: '{mostSimilar.Name}' (ความคล้าย {mostSimilar.SimilarityScore:F1}%)"
                        },
                        SimilarTags = similarTags
                    };
                }

                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                string slug;
                if (!string.IsNullOrWhiteSpace(request.Slug))
                {
                    slug = SlugGenerator.GenerateSlug(request.Slug);
                    var slugExists = await _unitOfWork.Tags.SlugExistsAsync(slug, null, cancellationToken);
                    if (slugExists)
                    {
                        slug = await SlugGenerator.GenerateUniqueSlug(
                            request.Slug,
                            async (slugToCheck, excludeId) =>
                                await _unitOfWork.Tags.SlugExistsAsync(slugToCheck, excludeId, cancellationToken)
                        );
                    }
                }
                else
                {
                    slug = await SlugGenerator.GenerateUniqueSlug(
                        request.Name,
                        async (slugToCheck, excludeId) =>
                            await _unitOfWork.Tags.SlugExistsAsync(slugToCheck, excludeId, cancellationToken)
                    );
                }

                var tag = new BlogHybrid.Domain.Entities.Tag
                {
                    Name = request.Name.Trim(),
                    Slug = slug,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = request.CreatedBy
                };

                await _unitOfWork.Tags.AddAsync(tag, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                _logger.LogInformation("Created tag: {TagId} - {TagName} by {UserId}",
                    tag.Id, tag.Name, request.CreatedBy ?? "Anonymous");

                var warningMessage = "สร้างแท็กสำเร็จ";
                if (similarTags.Any(t => t.SimilarityScore >= 60 && t.SimilarityScore < 90))
                {
                    warningMessage += $" (มีแท็กที่คล้ายกัน: {string.Join(", ", similarTags.Take(2).Select(t => t.Name))})";
                }

                return new CreateTagResult
                {
                    Success = true,
                    TagId = tag.Id,
                    Slug = tag.Slug,
                    Message = warningMessage,
                    SimilarTags = similarTags.Any() ? similarTags : null
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                _logger.LogError(ex, "Error creating tag: {TagName}", request.Name);

                return new CreateTagResult
                {
                    Success = false,
                    Errors = new List<string> { "เกิดข้อผิดพลาดในการสร้างแท็ก" }
                };
            }
        }
    }
}