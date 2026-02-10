using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RewardPointsSystem.Application.Interfaces;
using RewardPointsSystem.Infrastructure.Data;
using RewardPointsSystem.Infrastructure.Repositories;
using RewardPointsSystem.Infrastructure.Services;

namespace RewardPointsSystem.Infrastructure
{
    /// <summary>
    /// Infrastructure layer dependency injection configuration.
    /// This class registers all infrastructure implementations (DbContext, repositories, external services).
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Adds infrastructure services to the dependency injection container.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configuration">The configuration.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Database Configuration - Entity Framework Core with SQL Server
            services.AddDbContext<RewardPointsDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // Repository Layer - Unit of Work Pattern
            services.AddScoped<IUnitOfWork, EfUnitOfWork>();

            // Infrastructure Services - External Concerns
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();

            return services;
        }
    }
}
