using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogHybrid.Application.Commands.Category
{
    public class DeleteCategoryCommand : IRequest<DeleteCategoryResult>
    {
        public int Id { get; set; }
        public bool ForceDelete { get; set; } = false; // true = hard delete, false = soft delete
    }

    public class DeleteCategoryResult
    {
        public bool Success { get; set; }
        public List<string> Errors { get; set; } = new();
        public string? Message { get; set; }
        public bool HasPosts { get; set; } // แจ้งว่ามีโพสต์อยู่หรือไม่
        public int PostCount { get; set; }
    }
}
