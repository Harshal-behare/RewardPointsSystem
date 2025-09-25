using RewardPointsSystem.Interfaces;
using RewardPointsSystem.Services;
using RewardPointsSystem.Repositories;

namespace RewardPointsSystem.Configuration
{
    /// <summary>
    /// Service configuration demonstrating how services would be wired in a DI container.
    /// For production use, add Microsoft.Extensions.DependencyInjection package.
    /// </summary>
    public static class ServiceConfiguration
    {
        /// <summary>
        /// Creates a configured set of services for the application.
        /// In a real application, this would use an IoC container.
        /// </summary>
        public static ApplicationServices CreateServices()
        {
            // In production, these would be registered with an IoC container
            var unitOfWork = new InMemoryUnitOfWork();
            var roleService = new RoleService();
            var inventoryService = new InventoryService();
            var userService = new UserService(unitOfWork, roleService);
            var productService = new ProductService(unitOfWork, inventoryService);
            var redemptionService = new RedemptionService(unitOfWork, inventoryService, userService, roleService);

            return new ApplicationServices
            {
                UnitOfWork = unitOfWork,
                UserService = userService,
                ProductService = productService,
                RedemptionService = redemptionService,
                InventoryService = inventoryService,
                RoleService = roleService
            };
        }
    }

    /// <summary>
    /// Container for all application services.
    /// </summary>
    public class ApplicationServices
    {
        public IUnitOfWork UnitOfWork { get; set; }
        public IUserService UserService { get; set; }
        public IProductService ProductService { get; set; }
        public IRedemptionService RedemptionService { get; set; }
        public IInventoryService InventoryService { get; set; }
        public IRoleService RoleService { get; set; }
    }
}
