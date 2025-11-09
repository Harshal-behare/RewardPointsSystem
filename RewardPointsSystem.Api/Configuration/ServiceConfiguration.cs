using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RewardPointsSystem.Application.Interfaces;
using RewardPointsSystem.Infrastructure.Data;
using RewardPointsSystem.Infrastructure.Repositories;
using RewardPointsSystem.Application.Services.Users;
using RewardPointsSystem.Application.Services.Events;
using RewardPointsSystem.Application.Services.Accounts;
using RewardPointsSystem.Application.Services.Products;
using RewardPointsSystem.Application.Services.Orchestrators;
using RewardPointsSystem.Application.Services.Admin;

namespace RewardPointsSystem.Api.Configuration
{
    public static class ServiceConfiguration
    {
        public static IServiceCollection RegisterRewardPointsServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Add DbContext with SQL Server
            services.AddDbContext<RewardPointsDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
            
            // Repository Layer - Using EF Core with SQL Server
            services.AddScoped<IUnitOfWork, EfUnitOfWork>();

            // Core/User Services
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IUserRoleService, UserRoleService>();

            // Event Services
            services.AddScoped<IEventService, EventService>();
            services.AddScoped<IEventParticipationService, EventParticipationService>();
            services.AddScoped<IPointsAwardingService, PointsAwardingService>();

            // Account Services
            services.AddScoped<IUserPointsAccountService, UserPointsAccountService>();
            services.AddScoped<IUserPointsTransactionService, UserPointsTransactionService>();

            // Product Services
            services.AddScoped<IProductCatalogService, ProductCatalogService>();
            services.AddScoped<IPricingService, PricingService>();
            services.AddScoped<IInventoryService, InventoryService>();

            // Orchestrators
            services.AddScoped<IEventRewardOrchestrator, EventRewardOrchestrator>();
            services.AddScoped<IRedemptionOrchestrator, RedemptionOrchestrator>();
            services.AddScoped<IAdminDashboardService, AdminDashboardService>();
            
            return services;
        }
    }
}