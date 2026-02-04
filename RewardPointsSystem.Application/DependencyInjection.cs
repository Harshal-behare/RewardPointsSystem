using Microsoft.Extensions.DependencyInjection;
using RewardPointsSystem.Application.Interfaces;
using RewardPointsSystem.Application.Services.Core;
using RewardPointsSystem.Application.Services.Events;
using RewardPointsSystem.Application.Services.Accounts;
using RewardPointsSystem.Application.Services.Products;
using RewardPointsSystem.Application.Services.Orchestrators;
using RewardPointsSystem.Application.Services.Admin;
using RewardPointsSystem.Application.Services.Employee;
using RewardPointsSystem.Application.Services.Redemptions;
using RewardPointsSystem.Application.Services.Users;
using RewardPointsSystem.Application.Services.Points;
using RewardPointsSystem.Application.Services.Auth;
using RewardPointsSystem.Application.Services.Roles;
using FluentValidation;
using System.Reflection;

namespace RewardPointsSystem.Application
{
    /// <summary>
    /// Application layer dependency injection configuration.
    /// This class registers all application services (business logic).
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Adds application services to the dependency injection container.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // AutoMapper Configuration
            services.AddAutoMapper(Assembly.GetExecutingAssembly());

            // FluentValidation Configuration
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            // Core/User Services
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IUserQueryService, UserQueryService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IUserRoleService, UserRoleService>();
            services.AddScoped<IUserManagementService, UserManagementService>();

            // Authentication Services (Clean Architecture - API layer uses these)
            services.AddScoped<IAuthService, AuthService>();

            // Role Services (Clean Architecture - API layer uses these)
            services.AddScoped<IRoleQueryService, RoleQueryService>();
            services.AddScoped<IRoleManagementService, RoleManagementService>();

            // Event Services
            services.AddScoped<IEventService, EventService>();
            services.AddScoped<IEventParticipationService, EventParticipationService>();
            services.AddScoped<IPointsAwardingService, PointsAwardingService>();
            services.AddScoped<IEventQueryService, EventQueryService>();
            services.AddScoped<IEventStatusService, EventStatusService>();
            services.AddScoped<IEventParticipantQueryService, EventParticipantQueryService>();

            // Account Services
            services.AddScoped<IUserPointsAccountService, UserPointsAccountService>();
            services.AddScoped<IUserPointsTransactionService, UserPointsTransactionService>();

            // Points Services
            services.AddScoped<IPointsQueryService, PointsQueryService>();
            services.AddScoped<IPointsManagementService, PointsManagementService>();

            // Product Services
            services.AddScoped<IProductCatalogService, ProductCatalogService>();
            services.AddScoped<IProductQueryService, ProductQueryService>();
            services.AddScoped<IPricingService, PricingService>();
            services.AddScoped<IInventoryService, InventoryService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IProductManagementService, ProductManagementService>();

            // Redemption Services
            services.AddScoped<IRedemptionQueryService, RedemptionQueryService>();
            services.AddScoped<IRedemptionManagementService, RedemptionManagementService>();

            // Orchestrators
            services.AddScoped<IEventRewardOrchestrator, EventRewardOrchestrator>();
            services.AddScoped<IRedemptionOrchestrator, RedemptionOrchestrator>();

            // Dashboard Services
            services.AddScoped<IAdminDashboardService, AdminDashboardService>();
            services.AddScoped<IEmployeeDashboardService, EmployeeDashboardService>();

            // Admin Services
            services.AddScoped<IAdminReportService, AdminReportService>();
            services.AddScoped<IAdminAlertService, AdminAlertService>();

            // Budget Services
            services.AddScoped<IAdminBudgetService, AdminBudgetService>();

            return services;
        }
    }
}
