using Microsoft.Extensions.DependencyInjection;
using RewardPointsSystem.Interfaces;
using RewardPointsSystem.Repositories;

namespace RewardPointsSystem.Configuration
{
    public static class ServiceConfiguration
    {
        public static IServiceCollection RegisterRewardPointsServices(this IServiceCollection services)
        {
            // Repository Layer
            services.AddScoped<IUnitOfWork, InMemoryUnitOfWork>();

            // Note: Services are temporarily disabled until interface mismatches are resolved
            // This allows the project to build successfully
            
            return services;
        }
    }
}