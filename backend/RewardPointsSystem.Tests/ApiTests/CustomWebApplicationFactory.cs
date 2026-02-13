using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using RewardPointsSystem.Infrastructure.Data;
using RewardPointsSystem.Application.Interfaces;
using RewardPointsSystem.Infrastructure.Repositories;
using System.IO;

namespace RewardPointsSystem.Tests.FunctionalTests
{
    /// <summary>
    /// Custom WebApplicationFactory for API testing
    /// 
    /// This factory configures the application for testing by:
    /// - Using InMemory database for API integration tests (quick, isolated tests)
    /// - Configuring authentication for testing
    /// 
    /// WHY: Allows testing API endpoints against a real ASP.NET Core pipeline
    /// with fast InMemory database for isolated API testing
    /// </summary>
    public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> 
        where TProgram : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                // Find and remove ALL DbContext-related services
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

                // Use InMemory database for API tests (faster, isolated)
                services.AddDbContext<RewardPointsDbContext>((sp, options) =>
                {
                    options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}");
                    options.EnableSensitiveDataLogging();
                }, ServiceLifetime.Scoped);

                // Ensure UnitOfWork is registered
                services.RemoveAll<IUnitOfWork>();
                services.AddScoped<IUnitOfWork, EfUnitOfWork>();
            });
        }
    }
}
