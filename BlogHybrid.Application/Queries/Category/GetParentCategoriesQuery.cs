using BlogHybrid.Application.DTOs.Category;
using MediatR;

namespace BlogHybrid.Application.Queries.Category
{
    /// <summary>
    /// Query สำหรับดึงเฉพาะหมวดหมู่หลัก (ParentCategoryId == null)
    /// ใช้สำหรับ dropdown ในหน้าสร้าง/แก้ไขหมวดหมู่ย่อย
    /// </summary>
    public class GetParentCategoriesQuery : IRequest<List<CategoryDto>>
    {
        public bool ActiveOnly { get; set; } = true;
    }
}