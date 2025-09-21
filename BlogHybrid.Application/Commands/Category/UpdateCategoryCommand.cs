using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogHybrid.Application.Commands.Category
{
    public class UpdateCategoryCommand : IRequest<UpdateCategoryResult>
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string Color { get; set; } = "#0066cc";
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; }
    }

    public class UpdateCategoryResult
    {
        public bool Success { get; set; }
        public string? Slug { get; set; }
        public List<string> Errors { get; set; } = new();
        public string? Message { get; set; }
    }
}
