using BlogHybrid.Application.DTOs.Category;

namespace BlogHybrid.Web.Models.ViewModels.Admin
{
    public class CategoryIndexViewModel
    {
        public CategoryListDto Categories { get; set; } = new();
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public bool? IsActiveFilter { get; set; }
        public string SortBy { get; set; } = "SortOrder";
        public string SortDirection { get; set; } = "asc";

        // Helper properties
        public string NextSortDirection => SortDirection == "asc" ? "desc" : "asc";
        public bool HasPrevPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < Categories.TotalPages;
    }
}
