using BlogHybrid.Domain.Entities;
using BlogHybrid.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogHybrid.Infrastructure.Data.Configurations
{
    public class CommentVoteConfiguration : IEntityTypeConfiguration<CommentVote>
    {
        public void Configure(EntityTypeBuilder<CommentVote> builder)
        {
            // กำหนด composite primary key (CommentId + UserId)
            // ผู้ใช้แต่ละคนโหวตได้ 1 ครั้งต่อ 1 comment
            builder.HasKey(cv => new { cv.CommentId, cv.UserId });

            // กำหนด properties
            builder.Property(cv => cv.VoteType)
                .HasConversion<int>() // Store enum as integer (Upvote=1, Downvote=-1)
                .IsRequired();

            builder.Property(cv => cv.CreatedAt)
                .IsRequired();

            builder.Property(cv => cv.UpdatedAt)
                .IsRequired(false);

            // กำหนด relationships
            builder.HasOne(cv => cv.Comment)
                .WithMany(c => c.CommentVotes)
                .HasForeignKey(cv => cv.CommentId)
                .OnDelete(DeleteBehavior.Cascade); // ลบ comment แล้ว votes หายด้วย

            builder.HasOne(cv => cv.User)
                .WithMany(u => u.CommentVotes)
                .HasForeignKey(cv => cv.UserId)
                .OnDelete(DeleteBehavior.Cascade); // ลบ user แล้ว votes หายด้วย

            // กำหนด indexes (เพื่อ query ได้เร็ว)
            builder.HasIndex(cv => cv.CommentId);
            builder.HasIndex(cv => cv.UserId);
            builder.HasIndex(cv => cv.VoteType);
            builder.HasIndex(cv => cv.CreatedAt);

            // Composite index สำหรับ query ที่รวมหลายเงื่อนไข
            builder.HasIndex(cv => new { cv.CommentId, cv.VoteType });

            // กำหนด table name
            builder.ToTable("CommentVotes");
        }
    }
}