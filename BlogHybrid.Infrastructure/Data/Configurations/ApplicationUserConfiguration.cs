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
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.Property(u => u.DisplayName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.Bio)
                .HasMaxLength(500);

            builder.Property(u => u.ProfileImageUrl)
                .HasMaxLength(255);

            builder.HasIndex(u => u.DisplayName);
        }
    }
}
