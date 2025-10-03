// BlogHybrid.Application/Commands/Tag/CreateTagCommand.cs
using BlogHybrid.Application.Interfaces.Services;
using MediatR;

namespace BlogHybrid.Application.Commands.Tag
{
    public class CreateTagCommand : IRequest<CreateTagResult>
    {
        public string Name { get; set; } = string.Empty;
        public string? Slug { get; set; }
        public string? CreatedBy { get; set; } 
    }

    public class CreateTagResult
    {
        public bool Success { get; set; }
        public int TagId { get; set; }
        public string? Slug { get; set; }
        public List<string> Errors { get; set; } = new();
        public string? Message { get; set; }
        public List<SimilarTagResult>? SimilarTags { get; set; }  
    }
}