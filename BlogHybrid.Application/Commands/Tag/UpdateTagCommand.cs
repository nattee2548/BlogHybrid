// BlogHybrid.Application/Commands/Tag/UpdateTagCommand.cs
using MediatR;

namespace BlogHybrid.Application.Commands.Tag
{
    public class UpdateTagCommand : IRequest<UpdateTagResult>
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Slug { get; set; }
    }

    public class UpdateTagResult
    {
        public bool Success { get; set; }
        public string? Slug { get; set; }
        public List<string> Errors { get; set; } = new();
        public string? Message { get; set; }
    }
}