// BlogHybrid.Infrastructure/Data/Configurations/TagConfiguration.cs
using BlogHybrid.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogHybrid.Infrastructure.Data.Configurations
{
    public class TagConfiguration : IEntityTypeConfiguration<Tag>
    {
        public void Configure(EntityTypeBuilder<Tag> builder)
        {
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(t => t.Slug)
                .IsRequired()
                .HasMaxLength(60);

           
            builder.Property(t => t.CreatedBy)
                .HasMaxLength(450);

            // Indexes
            builder.HasIndex(t => t.Slug).IsUnique();
            builder.HasIndex(t => t.Name).IsUnique();
            builder.HasIndex(t => t.CreatedBy);

            
            builder.HasOne(t => t.Creator)
                .WithMany()
                .HasForeignKey(t => t.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}