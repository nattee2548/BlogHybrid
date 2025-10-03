using BlogHybrid.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogHybrid.Infrastructure.Data.Configurations
{
    public class CommunityConfiguration : IEntityTypeConfiguration<Community>
    {
        public void Configure(EntityTypeBuilder<Community> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.Slug)
                .IsRequired()
                .HasMaxLength(120);

            builder.Property(c => c.Description)
                .HasMaxLength(1000);

            builder.Property(c => c.ImageUrl)
                .HasMaxLength(255);

            builder.Property(c => c.CoverImageUrl)
                .HasMaxLength(255);

            builder.Property(c => c.Rules)
                .HasMaxLength(5000);

            builder.Property(c => c.CreatorId)
                .IsRequired();

            // Indexes
            builder.HasIndex(c => c.Slug).IsUnique();
            builder.HasIndex(c => c.CategoryId);
            builder.HasIndex(c => c.CreatorId);
            builder.HasIndex(c => c.IsActive);
            builder.HasIndex(c => c.IsDeleted);
            builder.HasIndex(c => c.IsPrivate);
            builder.HasIndex(c => c.CreatedAt);

            // Composite index สำหรับ query ที่ใช้บ่อย
            builder.HasIndex(c => new { c.CategoryId, c.IsActive, c.IsDeleted });

            // Relationships
            builder.HasOne(c => c.Category)
                .WithMany(cat => cat.Communities)
                .HasForeignKey(c => c.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.Creator)
                .WithMany(u => u.CreatedCommunities)
                .HasForeignKey(c => c.CreatorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Query filter สำหรับ soft delete
            builder.HasQueryFilter(c => !c.IsDeleted);
        }
    }
}