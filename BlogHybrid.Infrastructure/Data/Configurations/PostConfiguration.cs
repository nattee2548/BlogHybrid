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
    public class PostConfiguration : IEntityTypeConfiguration<Post>
    {
        public void Configure(EntityTypeBuilder<Post> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(p => p.Slug)
                .IsRequired()
                .HasMaxLength(250);

            builder.Property(p => p.Content)
                .IsRequired();

            builder.Property(p => p.Excerpt)
                .HasMaxLength(500);

            builder.Property(p => p.FeaturedImageUrl)
                .HasMaxLength(255);

            // Indexes
            builder.HasIndex(p => p.Slug).IsUnique();
            builder.HasIndex(p => p.IsPublished);
            builder.HasIndex(p => p.IsFeatured);
            builder.HasIndex(p => p.CreatedAt);
            builder.HasIndex(p => p.PublishedAt);
            builder.HasIndex(p => p.CommunityId);
            // Relationships
            builder.HasOne(p => p.Author)
                .WithMany(u => u.Posts)
                .HasForeignKey(p => p.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.Category)
                .WithMany(c => c.Posts)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.Community)
               .WithMany(c => c.Posts)
               .HasForeignKey(p => p.CommunityId)
               .OnDelete(DeleteBehavior.SetNull);

        }
    }

}
