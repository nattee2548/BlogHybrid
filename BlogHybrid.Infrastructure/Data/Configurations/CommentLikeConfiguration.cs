using BlogHybrid.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogHybrid.Infrastructure.Data.Configurations
{
    public class CommentLikeConfiguration : IEntityTypeConfiguration<CommentLike>
    {
        public void Configure(EntityTypeBuilder<CommentLike> builder)
        {
            // กำหนด composite primary key อย่างชัดเจน
            builder.HasKey(cl => new { cl.CommentId, cl.UserId });

            // กำหนด relationships
            builder.HasOne(cl => cl.Comment)
                .WithMany(c => c.CommentLikes)
                .HasForeignKey(cl => cl.CommentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(cl => cl.User)
                .WithMany(u => u.CommentLikes)
                .HasForeignKey(cl => cl.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // กำหนด indexes
            builder.HasIndex(cl => cl.CreatedAt);
            builder.HasIndex(cl => cl.CommentId);
            builder.HasIndex(cl => cl.UserId);

            // กำหนด table name ถ้าต้องการ
            builder.ToTable("CommentLikes");
        }
    }
}
