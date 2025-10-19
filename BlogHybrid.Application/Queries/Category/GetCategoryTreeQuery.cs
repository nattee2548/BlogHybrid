using BlogHybrid.Application.DTOs.Category;
using MediatR;

namespace BlogHybrid.Application.Queries.Category
{
    /// <summary>
    /// Query สำหรับดึงหมวดหมู่แบบ tree (parent + children)
    /// </summary>
    public class GetCategoryTreeQuery : IRequest<List<CategoryDto>>
    {
        /// <summary>
        /// รวมเฉพาะ active categories
        /// </summary>
        public bool ActiveOnly { get; set; } = true;
    }
}