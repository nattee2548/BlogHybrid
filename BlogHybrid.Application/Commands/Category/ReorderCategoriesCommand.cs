using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogHybrid.Application.Commands.Category
{
    public class ReorderCategoriesCommand : IRequest<ReorderCategoriesResult>
    {
        public List<CategoryOrderItem> Categories { get; set; } = new();
    }

    public class CategoryOrderItem
    {
        public int Id { get; set; }
        public int SortOrder { get; set; }
    }

    public class ReorderCategoriesResult
    {
        public bool Success { get; set; }
        public List<string> Errors { get; set; } = new();
        public string? Message { get; set; }
    }
}
