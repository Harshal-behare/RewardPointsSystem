using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RewardPointsSystem.Domain.Entities.Core;
using RewardPointsSystem.Domain.Entities.Products;

namespace RewardPointsSystem.Infrastructure.Data
{
    /// <summary>
    /// Seeds the database with initial data for roles, product categories, etc.
    /// </summary>
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<RewardPointsDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<RewardPointsDbContext>>();

            try
            {
                // Ensure database is created
                await context.Database.MigrateAsync();

                // Seed Roles
                await SeedRolesAsync(context, logger);

                // Seed Product Categories
                await SeedProductCategoriesAsync(context, logger);

                await context.SaveChangesAsync();
                
                logger.LogInformation("Database seeding completed successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database");
                throw;
            }
        }

        private static async Task SeedRolesAsync(RewardPointsDbContext context, ILogger logger)
        {
            if (await context.Roles.AnyAsync())
            {
                logger.LogInformation("Roles already exist, skipping seed");
                return;
            }

            var roles = new[]
            {
                Role.Create("Admin", "System Administrator with full access"),
                Role.Create("Employee", "Regular employee with limited access"),
            };

            await context.Roles.AddRangeAsync(roles);
            logger.LogInformation("Seeded {Count} roles", roles.Length);
        }

        private static async Task SeedProductCategoriesAsync(RewardPointsDbContext context, ILogger logger)
        {
            if (await context.ProductCategories.AnyAsync())
            {
                logger.LogInformation("Product categories already exist, skipping seed");
                return;
            }

            var categories = new[]
            {
                ProductCategory.Create("Electronics", 1, "Electronic devices and gadgets"),
                ProductCategory.Create("Office Supplies", 2, "Office equipment and supplies"),
                ProductCategory.Create("Gift Cards", 3, "Various gift cards"),
                ProductCategory.Create("Apparel", 4, "Clothing and accessories"),
                ProductCategory.Create("Home & Living", 5, "Home decor and living essentials"),
                ProductCategory.Create("Health & Wellness", 6, "Health and wellness products"),
                ProductCategory.Create("Books & Media", 7, "Books, movies, and media")
            };

            await context.ProductCategories.AddRangeAsync(categories);
            logger.LogInformation("Seeded {Count} product categories", categories.Length);
        }
    }
}
