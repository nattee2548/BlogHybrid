using BlogHybrid.Domain.Entities;
using BlogHybrid.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogHybrid.Infrastructure.Data.Configurations
{
    public class CommunityMemberConfiguration : IEntityTypeConfiguration<CommunityMember>
    {
        public void Configure(EntityTypeBuilder<CommunityMember> builder)
        {
            // Composite primary key
            builder.HasKey(cm => new { cm.CommunityId, cm.UserId });

            builder.Property(cm => cm.Role)
                .HasConversion<int>() // Store enum as int
                .IsRequired();

            builder.Property(cm => cm.JoinedAt)
                .IsRequired();

            // Indexes
            builder.HasIndex(cm => cm.CommunityId);
            builder.HasIndex(cm => cm.UserId);
            builder.HasIndex(cm => cm.Role);
            builder.HasIndex(cm => cm.IsApproved);
            builder.HasIndex(cm => cm.IsBanned);
            builder.HasIndex(cm => cm.JoinedAt);

            // Composite indexes
            builder.HasIndex(cm => new { cm.CommunityId, cm.Role });
            builder.HasIndex(cm => new { cm.CommunityId, cm.IsApproved });

            // Relationships
            builder.HasOne(cm => cm.Community)
                .WithMany(c => c.CommunityMembers)
                .HasForeignKey(cm => cm.CommunityId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(cm => cm.User)
                .WithMany(u => u.CommunityMemberships)
                .HasForeignKey(cm => cm.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.ToTable("CommunityMembers");
        }
    }
}