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

            // Properties
            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.Slug)
                .IsRequired()
                .HasMaxLength(120);

            builder.Property(c => c.Description)
                .HasMaxLength(500);

            builder.Property(c => c.ImageUrl)
                .HasMaxLength(500);

            builder.Property(c => c.Color)
                .HasMaxLength(20);

            builder.Property(c => c.CreatedAt)
                .IsRequired();

            // Indexes
            builder.HasIndex(c => c.Slug).IsUnique();
            builder.HasIndex(c => c.Name);
            builder.HasIndex(c => c.IsActive);
            builder.HasIndex(c => c.SortOrder);
            builder.HasIndex(c => c.ParentCategoryId);

            // ========== Hierarchical Relationships ==========
            // Self-referencing relationship สำหรับ Parent-Child categories
            builder.HasOne(c => c.ParentCategory)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict); // ป้องกัน cascade delete

            // Relationships with other entities
            builder.HasMany(c => c.Posts)
                .WithOne(p => p.Category)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // ✅ Many-to-Many with Community via CommunityCategory
            builder.HasMany(c => c.CommunityCategories)
                .WithOne(cc => cc.Category)
                .HasForeignKey(cc => cc.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.ToTable("Categories");
        }
    }
}