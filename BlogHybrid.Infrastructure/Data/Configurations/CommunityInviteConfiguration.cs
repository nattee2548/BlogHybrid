using BlogHybrid.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogHybrid.Infrastructure.Data.Configurations
{
    public class CommunityInviteConfiguration : IEntityTypeConfiguration<CommunityInvite>
    {
        public void Configure(EntityTypeBuilder<CommunityInvite> builder)
        {
            builder.HasKey(ci => ci.Id);

            builder.Property(ci => ci.InviteeEmail)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(ci => ci.Token)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(ci => ci.ExpiresAt)
                .IsRequired();

            // Indexes
            builder.HasIndex(ci => ci.Token).IsUnique();
            builder.HasIndex(ci => ci.CommunityId);
            builder.HasIndex(ci => ci.InviterId);
            builder.HasIndex(ci => ci.InviteeId);
            builder.HasIndex(ci => ci.InviteeEmail);
            builder.HasIndex(ci => ci.IsUsed);
            builder.HasIndex(ci => ci.ExpiresAt);
            builder.HasIndex(ci => ci.CreatedAt);

            // Composite indexes
            builder.HasIndex(ci => new { ci.CommunityId, ci.InviteeEmail });
            builder.HasIndex(ci => new { ci.Token, ci.IsUsed });

            // Relationships
            builder.HasOne(ci => ci.Community)
                .WithMany(c => c.CommunityInvites)
                .HasForeignKey(ci => ci.CommunityId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ci => ci.Inviter)
                .WithMany(u => u.SentInvites)
                .HasForeignKey(ci => ci.InviterId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(ci => ci.Invitee)
                .WithMany(u => u.ReceivedInvites)
                .HasForeignKey(ci => ci.InviteeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.ToTable("CommunityInvites");
        }
    }
}