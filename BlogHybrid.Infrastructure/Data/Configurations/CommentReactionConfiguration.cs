using BlogHybrid.Domain.Entities;
using BlogHybrid.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogHybrid.Infrastructure.Data.Configurations
{
    public class CommentReactionConfiguration : IEntityTypeConfiguration<CommentReaction>
    {
        public void Configure(EntityTypeBuilder<CommentReaction> builder)
        {
            // กำหนด composite primary key (CommentId + UserId)
            // ผู้ใช้แต่ละคนแสดง reaction ได้ 1 ครั้งต่อ 1 comment
            // แต่สามารถเปลี่ยน reaction ได้ (จาก Like เป็น Love เช่น)
            builder.HasKey(cr => new { cr.CommentId, cr.UserId });

            // กำหนด properties
            builder.Property(cr => cr.ReactionType)
                .HasConversion<int>() // Store enum as integer (Like=1, Love=2, etc.)
                .IsRequired();

            builder.Property(cr => cr.CreatedAt)
                .IsRequired();

            builder.Property(cr => cr.UpdatedAt)
                .IsRequired(false);

            // กำหนด relationships
            builder.HasOne(cr => cr.Comment)
                .WithMany(c => c.CommentReactions)
                .HasForeignKey(cr => cr.CommentId)
                .OnDelete(DeleteBehavior.Cascade); // ลบ comment แล้ว reactions หายด้วย

            builder.HasOne(cr => cr.User)
                .WithMany(u => u.CommentReactions)
                .HasForeignKey(cr => cr.UserId)
                .OnDelete(DeleteBehavior.Cascade); // ลบ user แล้ว reactions หายด้วย

            // กำหนด indexes (เพื่อ query ได้เร็ว)
            builder.HasIndex(cr => cr.CommentId);
            builder.HasIndex(cr => cr.UserId);
            builder.HasIndex(cr => cr.ReactionType);
            builder.HasIndex(cr => cr.CreatedAt);

            // Composite indexes สำหรับ query ที่รวมหลายเงื่อนไข
            builder.HasIndex(cr => new { cr.CommentId, cr.ReactionType });
            builder.HasIndex(cr => new { cr.UserId, cr.ReactionType });

            // กำหนด table name
            builder.ToTable("CommentReactions");
        }
    }
}