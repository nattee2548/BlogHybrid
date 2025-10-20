// สร้างไฟล์ใหม่: BlogHybrid.Domain/Entities/CommunityCategory.cs

namespace BlogHybrid.Domain.Entities
{
    /// <summary>
    /// Many-to-Many relationship between Community and Category
    /// </summary>
    public class CommunityCategory
    {
        public int CommunityId { get; set; }
        public int CategoryId { get; set; }
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Community Community { get; set; } = null!;
        public virtual Category Category { get; set; } = null!;
    }
}