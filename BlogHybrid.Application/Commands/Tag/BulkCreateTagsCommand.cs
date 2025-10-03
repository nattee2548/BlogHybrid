// BlogHybrid.Application/Commands/Tag/BulkCreateTagsCommand.cs
using BlogHybrid.Application.Interfaces.Services;
using MediatR;

namespace BlogHybrid.Application.Commands.Tag
{
    public class BulkCreateTagsCommand : IRequest<BulkCreateTagsResult>
    {
        public List<string> TagNames { get; set; } = new();
        public string? CreatedBy { get; set; }  // ✅ User ID ที่สร้าง
    }

    public class BulkCreateTagsResult
    {
        public bool Success { get; set; }
        public List<CreatedTagInfo> CreatedTags { get; set; } = new();
        public List<ExistingTagInfo> ExistingTags { get; set; } = new();
        public List<SimilarTagWarning> SimilarTagWarnings { get; set; } = new();
        public List<string> Errors { get; set; } = new();
    }

    public class CreatedTagInfo
    {
        public int TagId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
    }

    public class ExistingTagInfo
    {
        public int TagId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
    }

    public class SimilarTagWarning
    {
        public string RequestedName { get; set; } = string.Empty;
        public List<SimilarTagResult> SimilarTags { get; set; } = new();
    }
}