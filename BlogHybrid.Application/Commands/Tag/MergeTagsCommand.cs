// BlogHybrid.Application/Commands/Tag/MergeTagsCommand.cs
using MediatR;

namespace BlogHybrid.Application.Commands.Tag
{
    public class MergeTagsCommand : IRequest<MergeTagsResult>
    {
        public int SourceTagId { get; set; }  // Tag ที่จะถูกรวม
        public int TargetTagId { get; set; }  // Tag หลักที่จะรวมเข้า
    }

    public class MergeTagsResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public List<string> Errors { get; set; } = new();
        public int PostsMerged { get; set; }
    }
}