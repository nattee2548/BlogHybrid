using BlogHybrid.Application.DTOs.Category;

namespace BlogHybrid.Web.Models.ViewModels.Admin
{
    public class CategoryDetailsViewModel
    {
        public CategoryDto Category { get; set; } = new();
        public List<RecentPost> RecentPosts { get; set; } = new();
        public CategoryStats Stats { get; set; } = new();
    }

    public class RecentPost
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsPublished { get; set; }
        public int ViewCount { get; set; }
        public int CommentCount { get; set; }
    }

    public class CategoryStats
    {
        public int TotalPosts { get; set; }
        public int PublishedPosts { get; set; }
        public int DraftPosts { get; set; }
        public int TotalViews { get; set; }
        public int TotalComments { get; set; }
        public DateTime? LastPostDate { get; set; }
        public string MostActiveAuthor { get; set; } = string.Empty;
    }
}
