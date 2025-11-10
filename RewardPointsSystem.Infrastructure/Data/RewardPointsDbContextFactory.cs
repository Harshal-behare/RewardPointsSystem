using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace RewardPointsSystem.Infrastructure.Data
{
    /// <summary>
    /// Design-time factory for creating DbContext during EF Core migrations
    /// </summary>
    public class RewardPointsDbContextFactory : IDesignTimeDbContextFactory<RewardPointsDbContext>
    {
        public RewardPointsDbContext CreateDbContext(string[] args)
        {
            // Build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../RewardPointsSystem.Api"))
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            // Get connection string
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            // Build DbContextOptions
            var optionsBuilder = new DbContextOptionsBuilder<RewardPointsDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new RewardPointsDbContext(optionsBuilder.Options);
        }
    }
}
