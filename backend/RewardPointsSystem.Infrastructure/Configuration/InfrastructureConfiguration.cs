using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RewardPointsSystem.Infrastructure.Data;

namespace RewardPointsSystem.Infrastructure.Configuration
{
    /// <summary>
    /// Configuration for Infrastructure layer services and Entity Framework Core
    /// </summary>
    public static class InfrastructureConfiguration
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Register DbContext with SQL Server
            services.AddDbContext<RewardPointsDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    sqlOptions =>
                    {
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 3,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null);
                    }));

           
            // services.AddScoped<IUserRepository, UserRepository>();
            // services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}
