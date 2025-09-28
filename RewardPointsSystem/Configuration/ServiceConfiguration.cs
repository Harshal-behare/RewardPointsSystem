using Microsoft.Extensions.DependencyInjection;
using RewardPointsSystem.Interfaces;
using RewardPointsSystem.Repositories;
using RewardPointsSystem.Services.Users;
using RewardPointsSystem.Services.Events;
using RewardPointsSystem.Services.Accounts;

namespace RewardPointsSystem.Configuration
{
    public static class ServiceConfiguration
    {
        public static IServiceCollection RegisterRewardPointsServices(this IServiceCollection services)
        {
            // Repository Layer
            services.AddScoped<IUnitOfWork, InMemoryUnitOfWork>();

            // Core/User Services
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IUserRoleService, UserRoleService>();

            // Event Services
            services.AddScoped<IEventService, EventService>();
            services.AddScoped<IEventParticipationService, EventParticipationService>();
            services.AddScoped<IPointsAwardingService, PointsAwardingService>();

            // Account Services
            services.AddScoped<IRewardAccountService, RewardAccountService>();
            services.AddScoped<ITransactionService, TransactionService>();
            
            return services;
        }
    }
}