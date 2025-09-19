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
    public class PostLikeConfiguration : IEntityTypeConfiguration<PostLike>
    {
        public void Configure(EntityTypeBuilder<PostLike> builder)
        {
            // กำหนด composite primary key อย่างชัดเจน
            builder.HasKey(pl => new { pl.PostId, pl.UserId });

            // กำหนด relationships
            builder.HasOne(pl => pl.Post)
                .WithMany(p => p.PostLikes)
                .HasForeignKey(pl => pl.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(pl => pl.User)
                .WithMany(u => u.PostLikes)
                .HasForeignKey(pl => pl.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // กำหนด indexes
            builder.HasIndex(pl => pl.CreatedAt);
            builder.HasIndex(pl => pl.PostId);
            builder.HasIndex(pl => pl.UserId);

            // กำหนด table name ถ้าต้องการ
            builder.ToTable("PostLikes");
        }
    }
}
