// BlogHybrid.Application/DTOs/Tag/TagListDto.cs
namespace BlogHybrid.Application.DTOs.Tag
{
    public class TagListDto
    {
        public List<TagDto> Tags { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }
}