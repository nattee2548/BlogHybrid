using BlogHybrid.Application.Queries.Post;

namespace BlogHybrid.Web.Models
{
    public class HomeViewModel
    {
        // Featured Posts (แสดงด้านบน)
        public List<PublicPostItem> FeaturedPosts { get; set; } = new();

        // Regular Posts
        public List<PublicPostItem> Posts { get; set; } = new();

        // Pagination
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }

        // Filters
        public string? SearchTerm { get; set; }
        public int? CategoryId { get; set; }
        public int? CommunityId { get; set; }
        public string? Tag { get; set; }
        public string Sort { get; set; } = "latest";

        // Helper Properties
        public bool HasFilters => !string.IsNullOrEmpty(SearchTerm) ||
                                  CategoryId.HasValue ||
                                  CommunityId.HasValue ||
                                  !string.IsNullOrEmpty(Tag);
    }
}