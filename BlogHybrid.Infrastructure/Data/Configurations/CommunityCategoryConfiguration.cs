// สร้างไฟล์ใหม่: BlogHybrid.Infrastructure/Data/Configurations/CommunityCategoryConfiguration.cs

using BlogHybrid.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogHybrid.Infrastructure.Data.Configurations
{
    public class CommunityCategoryConfiguration : IEntityTypeConfiguration<CommunityCategory>
    {
        public void Configure(EntityTypeBuilder<CommunityCategory> builder)
        {
            // Composite primary key (Many-to-Many)
            builder.HasKey(cc => new { cc.CommunityId, cc.CategoryId });

            // Properties
            builder.Property(cc => cc.AssignedAt)
                .IsRequired();

            // Indexes
            builder.HasIndex(cc => cc.CommunityId);
            builder.HasIndex(cc => cc.CategoryId);
            builder.HasIndex(cc => cc.AssignedAt);

            // Relationships
            builder.HasOne(cc => cc.Community)
                .WithMany(c => c.CommunityCategories)
                .HasForeignKey(cc => cc.CommunityId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(cc => cc.Category)
                .WithMany(c => c.CommunityCategories)
                .HasForeignKey(cc => cc.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.ToTable("CommunityCategories");
        }
    }
}