using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogHybrid.Infrastructure.Data.Seeds
{
    public class DatabaseSeeder
    {
        public static async Task SeedAllAsync(IServiceProvider serviceProvider)
        {
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("DatabaseSeeding");

            try
            {
                logger.LogInformation("Starting database seeding...");

                // Seed roles first
                await RoleSeeder.SeedRolesAsync(serviceProvider);

                //// Then seed categories
                //await CategorySeeder.SeedCategoriesAsync(serviceProvider);

                logger.LogInformation("Database seeding completed successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database.");
                throw;
            }
        }
    }
}
