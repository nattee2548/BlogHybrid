using BlogHybrid.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogHybrid.Infrastructure.Data.Configurations
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.Slug)
                .IsRequired()
                .HasMaxLength(120);

            builder.Property(c => c.Description)
                .HasMaxLength(500);

            builder.Property(c => c.ImageUrl)
                .HasMaxLength(255);

            builder.Property(c => c.Color)
                .HasMaxLength(7);

            // ========== Hierarchical Relationship ==========
            builder.HasOne(c => c.ParentCategory)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict); // ป้องกันการลบหมวดหมู่หลักที่มีหมวดหมู่ย่อย

            // Indexes
            builder.HasIndex(c => c.Slug).IsUnique();
            builder.HasIndex(c => c.IsActive);
            builder.HasIndex(c => c.SortOrder);
            builder.HasIndex(c => c.ParentCategoryId); // เพิ่ม index สำหรับ query หมวดหมู่ย่อย
        }
    }
}