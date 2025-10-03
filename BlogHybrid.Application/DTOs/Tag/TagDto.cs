// BlogHybrid.Application/DTOs/Tag/TagDto.cs
namespace BlogHybrid.Application.DTOs.Tag
{
    public class TagDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int PostCount { get; set; }

        public string? CreatedBy { get; set; }
        public string? CreatorName { get; set; }
    }
}