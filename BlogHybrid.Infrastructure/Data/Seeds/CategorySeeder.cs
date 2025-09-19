using BlogHybrid.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogHybrid.Infrastructure.Data.Seeds
{
    public class CategorySeeder
    {
        public static async Task SeedCategoriesAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("DatabaseSeeding");

            // Check if categories already exist
            if (await context.Categories.AnyAsync())
            {
                logger.LogInformation("Categories already exist. Skipping seed.");
                return;
            }

            var categories = new List<Category>
            {
                new Category
                {
                    Name = "Career Talk",
                    Slug = "career",
                    Description = "เรื่องราวเกี่ยวกับการทำงาน เส้นทางอาชีพ และการพัฒนาตนเอง",
                    Color = "#2563eb",
                    SortOrder = 1,
                    IsActive = true
                },
                new Category
                {
                    Name = "Life Advice",
                    Slug = "life",
                    Description = "คำแนะนำและแนวทางในการใช้ชีวิต ความสัมพันธ์ และการแก้ปัญหา",
                    Color = "#059669",
                    SortOrder = 2,
                    IsActive = true
                },
                new Category
                {
                    Name = "Learning Hub",
                    Slug = "learning",
                    Description = "การเรียนรู้ ทักษะใหม่ๆ และการพัฒนาความรู้",
                    Color = "#dc2626",
                    SortOrder = 3,
                    IsActive = true
                },
                new Category
                {
                    Name = "Mental Health",
                    Slug = "mental-health",
                    Description = "สุขภาพจิต การดูแลตนเอง และความเป็นอยู่ที่ดี",
                    Color = "#7c3aed",
                    SortOrder = 4,
                    IsActive = true
                },
                new Category
                {
                    Name = "Lifestyle",
                    Slug = "lifestyle",
                    Description = "รูปแบบการใช้ชีวิต งานอิสระ และการพัฒนาคุณภาพชีวิต",
                    Color = "#ea580c",
                    SortOrder = 5,
                    IsActive = true
                },
                new Category
                {
                    Name = "Technology",
                    Slug = "technology",
                    Description = "เทคโนโลยี การเขียนโปรแกรม และนวัตกรรมใหม่ๆ",
                    Color = "#0891b2",
                    SortOrder = 6,
                    IsActive = true
                }
            };

            context.Categories.AddRange(categories);
            await context.SaveChangesAsync();

            logger.LogInformation($"Seeded {categories.Count} categories successfully.");
        }
    }
}
