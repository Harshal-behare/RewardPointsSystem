using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RewardPointsSystem.Infrastructure.Data;
using RewardPointsSystem.Application.Interfaces;
using RewardPointsSystem.Infrastructure.Repositories;
using RewardPointsSystem.Tests.TestHelpers;

namespace RewardPointsSystem.Tests.FunctionalTests
{
    /// <summary>
    /// Custom WebApplicationFactory for functional testing
    /// 
    /// This factory configures the application for testing by:
    /// - Replacing the production database with InMemory database
    /// - Configuring authentication for testing
    /// 
    /// WHY: Allows testing API endpoints against a real ASP.NET Core pipeline
    /// without external dependencies (database, external services, etc.)
    /// </summary>
    public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> 
        where TProgram : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Find and remove ALL DbContext-related services
                // Need to remove by ServiceType to avoid conflicts
                var descriptorsToRemove = services
                    .Where(d => 
                        d.ServiceType == typeof(DbContextOptions<RewardPointsDbContext>) ||
                        d.ServiceType == typeof(DbContextOptions) ||
                        d.ServiceType.FullName?.Contains("EntityFrameworkCore") == true ||
                        d.ImplementationType?.FullName?.Contains("SqlServer") == true ||
                        (d.ServiceType.IsGenericType && 
                         d.ServiceType.GetGenericTypeDefinition().FullName?.Contains("DbContextOptions") == true))
                    .ToList();

                foreach (var descriptor in descriptorsToRemove)
                {
                    services.Remove(descriptor);
                }

                // Now add InMemory database 
                var databaseName = $"TestDatabase_{Guid.NewGuid()}";
                services.AddDbContext<RewardPointsDbContext>((sp, options) =>
                {
                    options.UseInMemoryDatabase(databaseName);
                    options.EnableSensitiveDataLogging();
                }, ServiceLifetime.Scoped);

                // Replace UnitOfWork with InMemory version
                services.RemoveAll<IUnitOfWork>();
                services.AddScoped<IUnitOfWork, EfUnitOfWork>();
            });

            builder.UseEnvironment("Testing");
        }
    }
}
