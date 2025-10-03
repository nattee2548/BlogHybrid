// BlogHybrid.Application/Interfaces/Services/ITagSimilarityService.cs
namespace BlogHybrid.Application.Interfaces.Services
{
    public interface ITagSimilarityService
    {
        /// <summary>
        /// Find similar tags using AI-like algorithm
        /// </summary>
        Task<List<SimilarTagResult>> FindSimilarTagsAsync(
            string tagName,
            int limit = 5,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Calculate similarity score between two tag names (0-100)
        /// </summary>
        double CalculateSimilarity(string tag1, string tag2);

        /// <summary>
        /// Check if tag is too similar to existing tags
        /// </summary>
        Task<bool> IsTooSimilarAsync(
            string tagName,
            double threshold = 85,
            CancellationToken cancellationToken = default);
    }

    public class SimilarTagResult
    {
        public int TagId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public double SimilarityScore { get; set; }  // 0-100
        public int PostCount { get; set; }
    }
}