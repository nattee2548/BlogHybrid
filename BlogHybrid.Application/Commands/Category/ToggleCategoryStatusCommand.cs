// BlogHybrid.Application/Commands/Category/ToggleCategoryStatusCommand.cs
using MediatR;

namespace BlogHybrid.Application.Commands.Category
{
    public class ToggleCategoryStatusCommand : IRequest<ToggleCategoryStatusResult>
    {
        public int CategoryId { get; set; }
    }

    public class ToggleCategoryStatusResult
    {
        public bool Success { get; set; }
        public bool NewStatus { get; set; } // สถานะใหม่หลังจาก toggle
        public string Message { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new();
    }
}