using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogHybrid.Application.Commands.Category
{
    public class ToggleCategoryStatusCommand : IRequest<ToggleCategoryStatusResult>
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
    }

    public class ToggleCategoryStatusResult
    {
        public bool Success { get; set; }
        public bool NewStatus { get; set; }
        public List<string> Errors { get; set; } = new();
        public string? Message { get; set; }
    }
}
