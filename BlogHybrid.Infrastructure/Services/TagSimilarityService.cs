// BlogHybrid.Infrastructure/Services/TagSimilarityService.cs
using BlogHybrid.Application.Interfaces.Repositories;
using BlogHybrid.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace BlogHybrid.Infrastructure.Services
{
    public class TagSimilarityService : ITagSimilarityService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TagSimilarityService> _logger;

        public TagSimilarityService(
            IUnitOfWork unitOfWork,
            ILogger<TagSimilarityService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<List<SimilarTagResult>> FindSimilarTagsAsync(
            string tagName,
            int limit = 5,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var allTags = await _unitOfWork.Tags.GetAllAsync(cancellationToken);
                var normalizedInput = NormalizeTagName(tagName);

                var similarTags = allTags
                    .Select(tag => new SimilarTagResult
                    {
                        TagId = tag.Id,
                        Name = tag.Name,
                        Slug = tag.Slug,
                        SimilarityScore = CalculateSimilarity(normalizedInput, NormalizeTagName(tag.Name)),
                        PostCount = 0
                    })
                    .Where(x => x.SimilarityScore > 50)
                    .OrderByDescending(x => x.SimilarityScore)
                    .Take(limit)
                    .ToList();

                foreach (var tag in similarTags)
                {
                    tag.PostCount = await _unitOfWork.Tags.GetPostCountAsync(tag.TagId, cancellationToken);
                }

                return similarTags;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding similar tags for: {TagName}", tagName);
                return new List<SimilarTagResult>();
            }
        }

        public double CalculateSimilarity(string tag1, string tag2)
        {
            var normalized1 = NormalizeTagName(tag1);
            var normalized2 = NormalizeTagName(tag2);

            if (normalized1 == normalized2)
                return 100.0;

            var distance = LevenshteinDistance(normalized1, normalized2);
            var maxLength = Math.Max(normalized1.Length, normalized2.Length);

            if (maxLength == 0)
                return 100.0;

            var similarity = (1.0 - ((double)distance / maxLength)) * 100;

            if (normalized1.Contains(normalized2) || normalized2.Contains(normalized1))
            {
                similarity = Math.Min(100, similarity + 20);
            }

            if (normalized1.Length > 0 && normalized2.Length > 0)
            {
                var commonPrefix = GetCommonPrefixLength(normalized1, normalized2);
                var prefixBonus = (commonPrefix / (double)Math.Min(normalized1.Length, normalized2.Length)) * 10;
                similarity = Math.Min(100, similarity + prefixBonus);
            }

            return Math.Round(similarity, 2);
        }

        public async Task<bool> IsTooSimilarAsync(
            string tagName,
            double threshold = 85,
            CancellationToken cancellationToken = default)
        {
            var similarTags = await FindSimilarTagsAsync(tagName, 1, cancellationToken);
            return similarTags.Any(t => t.SimilarityScore >= threshold);
        }

        private string NormalizeTagName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return string.Empty;

            return new string(name
                .ToLowerInvariant()
                .Where(c => char.IsLetterOrDigit(c))
                .ToArray());
        }

        private int LevenshteinDistance(string s1, string s2)
        {
            if (string.IsNullOrEmpty(s1))
                return s2?.Length ?? 0;

            if (string.IsNullOrEmpty(s2))
                return s1.Length;

            var d = new int[s1.Length + 1, s2.Length + 1];

            for (int i = 0; i <= s1.Length; i++)
                d[i, 0] = i;

            for (int j = 0; j <= s2.Length; j++)
                d[0, j] = j;

            for (int i = 1; i <= s1.Length; i++)
            {
                for (int j = 1; j <= s2.Length; j++)
                {
                    int cost = (s2[j - 1] == s1[i - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }

            return d[s1.Length, s2.Length];
        }

        private int GetCommonPrefixLength(string s1, string s2)
        {
            int length = 0;
            int maxLength = Math.Min(s1.Length, s2.Length);

            for (int i = 0; i < maxLength; i++)
            {
                if (s1[i] == s2[i])
                    length++;
                else
                    break;
            }

            return length;
        }
    }
}