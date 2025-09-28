using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using RewardPointsSystem.Configuration;
using RewardPointsSystem.Interfaces;
using RewardPointsSystem.Models.Core;
using RewardPointsSystem.Models.Events;
using RewardPointsSystem.Models.Products;
using RewardPointsSystem.Models.Operations;

namespace RewardPointsSystem
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("========================================");
            Console.WriteLine("    AGDATA Boot Camp Milestone 1");
            Console.WriteLine("   Complete C# Masterclass Demo");
            Console.WriteLine("========================================\n");

            // Build the service container
            var services = new ServiceCollection();
            services.RegisterRewardPointsServices();
            
            var serviceProvider = services.BuildServiceProvider();

            try
            {
                await RunMilestone1DemoAsync(serviceProvider);
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Error: {ex.Message}");
            }
            finally
            {
                serviceProvider?.Dispose();
            }

            Console.WriteLine("\n Press any key to exit...");
            Console.ReadKey();
        }

        private static async Task RunMilestone1DemoAsync(IServiceProvider serviceProvider)
        {
            // Get all required services - demonstrating Dependency Injection
            var userService = serviceProvider.GetRequiredService<IUserService>();
            var roleService = serviceProvider.GetRequiredService<IRoleService>();
            var userRoleService = serviceProvider.GetRequiredService<IUserRoleService>();
            var eventService = serviceProvider.GetRequiredService<IEventService>();
            var participationService = serviceProvider.GetRequiredService<IEventParticipationService>();
            var pointsAwardingService = serviceProvider.GetRequiredService<IPointsAwardingService>();
            var accountService = serviceProvider.GetRequiredService<IRewardAccountService>();
            var transactionService = serviceProvider.GetRequiredService<ITransactionService>();
            var productService = serviceProvider.GetRequiredService<IProductCatalogService>();
            var pricingService = serviceProvider.GetRequiredService<IPricingService>();
            var inventoryService = serviceProvider.GetRequiredService<IInventoryService>();
            var redemptionOrchestrator = serviceProvider.GetRequiredService<IRedemptionOrchestrator>();

            Console.WriteLine(" Design Business Logic - Key Entities Implementation\n");
            
            // ========================================
            // USER SERVICE DEMONSTRATION
            // ========================================
            Console.WriteLine(" USER SERVICE: Add, update, and retrieve users");
            Console.WriteLine("   Prevent duplicate users (by email or employee ID)");
            
            // Create Admin and Employee roles
            var adminRole = await roleService.CreateRoleAsync("Admin", "System Administrator with full access");
            var employeeRole = await roleService.CreateRoleAsync("Employee", "Regular employee with limited access");
            Console.WriteLine($"   Created roles: {adminRole.Name}, {employeeRole.Name}");

            // Create users and demonstrate validation
            var admin = await userService.CreateUserAsync("admin@agdata.com", "EMP001", "Admin", "AdminUser");
            var employee1 = await userService.CreateUserAsync("harshal@agdata.com", "EMP002", "Harshal", "Behare");
            var employee2 = await userService.CreateUserAsync("rohit@agdata.com", "EMP003", "Rohit", "Sharma");
            Console.WriteLine($"  Created users: {admin.FirstName}, {employee1.FirstName}, {employee2.FirstName}");
            
            // Try to create duplicate user (should handle gracefully)
            try
            {
                await userService.CreateUserAsync("alice@agdata.com", "EMP004", "Duplicate", "User");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Duplicate prevention works: {ex.Message.Split('.')[0]}");
            }

            // Assign roles
            await userRoleService.AssignRoleAsync(admin.Id, adminRole.Id, admin.Id);
            await userRoleService.AssignRoleAsync(employee1.Id, employeeRole.Id, admin.Id);
            await userRoleService.AssignRoleAsync(employee2.Id, employeeRole.Id, admin.Id);
            Console.WriteLine("   User roles assigned successfully\n");

            // ========================================
            // EVENT SERVICE DEMONSTRATION
            // ========================================
            Console.WriteLine(" EVENT SERVICE: Create and manage events");
            Console.WriteLine("   Link events to points transactions");
            
            var salesEvent = await eventService.CreateEventAsync(
                "Q4 Sales Competition", 
                "Quarterly sales performance competition with rewards", 
                DateTime.UtcNow.AddDays(1), 
                1000);
            var trainingEvent = await eventService.CreateEventAsync(
                "Team Training Workshop", 
                "Professional development and team building event", 
                DateTime.UtcNow.AddDays(7), 
                500);
                
            Console.WriteLine($"  Created events: {salesEvent.Name} ({salesEvent.TotalPointsPool}pts), {trainingEvent.Name} ({trainingEvent.TotalPointsPool}pts)");

            // Register participants for events
            await participationService.RegisterParticipantAsync(salesEvent.Id, employee1.Id);
            await participationService.RegisterParticipantAsync(salesEvent.Id, employee2.Id);
            await participationService.RegisterParticipantAsync(trainingEvent.Id, employee1.Id);
            Console.WriteLine("  Event participants registered\n");

            // ========================================
            // PRODUCT CATALOG SERVICE DEMONSTRATION
            // ========================================
            Console.WriteLine(" PRODUCT CATALOG SERVICE: Add, update, delete, and list products");
            Console.WriteLine("    Validate required points and stock");
            
            // Create products
            var laptop = await productService.CreateProductAsync("Gaming Laptop", "High-performance laptop for work and gaming", "Electronics");
            var headphones = await productService.CreateProductAsync("Wireless Headphones", "Premium noise-canceling headphones", "Electronics");
            var giftCard = await productService.CreateProductAsync("Amazon Gift Card", "$50 Amazon gift card for online shopping", "Gift Cards");
            Console.WriteLine($"  Created products: {laptop.Name}, {headphones.Name}, {giftCard.Name}");

            // Set product pricing
            await pricingService.SetProductPriceAsync(laptop.Id, 2000, DateTime.UtcNow);
            await pricingService.SetProductPriceAsync(headphones.Id, 500, DateTime.UtcNow);
            await pricingService.SetProductPriceAsync(giftCard.Id, 250, DateTime.UtcNow);
            Console.WriteLine("  Product pricing configured");

            // Set up inventory
            await inventoryService.CreateInventoryAsync(laptop.Id, 5, 2);
            await inventoryService.CreateInventoryAsync(headphones.Id, 20, 5);
            await inventoryService.CreateInventoryAsync(giftCard.Id, 100, 10);
            Console.WriteLine("  Product inventory initialized\n");

            // ========================================
            // REDEMPTION SERVICE DEMONSTRATION
            // ========================================
            Console.WriteLine(" REDEMPTION SERVICE: Simulate redemption logic");
            Console.WriteLine("   Validate user balance and product availability");
            
            // Create reward accounts first
            var account1 = await accountService.CreateAccountAsync(employee1.Id);
            var account2 = await accountService.CreateAccountAsync(employee2.Id);
            Console.WriteLine("    Reward accounts created");

            // Award points from events
            await pointsAwardingService.AwardPointsAsync(salesEvent.Id, employee1.Id, 600, 1);
            await pointsAwardingService.AwardPointsAsync(salesEvent.Id, employee2.Id, 400, 2);
            await pointsAwardingService.AwardPointsAsync(trainingEvent.Id, employee1.Id, 300, 1);
            Console.WriteLine($"    Points awarded: {employee1.FirstName} (900pts total), {employee2.FirstName} (400pts)");

            // Record transactions and update balances
            await transactionService.RecordEarnedPointsAsync(employee1.Id, 600, salesEvent.Id, $"1st place in {salesEvent.Name}");
            await transactionService.RecordEarnedPointsAsync(employee1.Id, 300, trainingEvent.Id, $"Completed {trainingEvent.Name}");
            await transactionService.RecordEarnedPointsAsync(employee2.Id, 400, salesEvent.Id, $"2nd place in {salesEvent.Name}");
            await accountService.AddPointsAsync(employee1.Id, 900);
            await accountService.AddPointsAsync(employee2.Id, 400);
            Console.WriteLine("    Points transactions recorded and balances updated");

            // Attempt redemptions
            var redemptionResult1 = await redemptionOrchestrator.ProcessRedemptionAsync(employee1.Id, headphones.Id);
            if (redemptionResult1.Success)
            {
                Console.WriteLine($"    {employee1.FirstName} successfully redeemed {headphones.Name}");
                await redemptionOrchestrator.ApproveRedemptionAsync(redemptionResult1.Redemption.Id);
                await redemptionOrchestrator.DeliverRedemptionAsync(redemptionResult1.Redemption.Id, "Delivered to employee desk");
                Console.WriteLine("    Redemption processed: Approved and Delivered");
            }

            // Try redemption with insufficient balance
            var redemptionResult2 = await redemptionOrchestrator.ProcessRedemptionAsync(employee2.Id, laptop.Id);
            if (!redemptionResult2.Success)
            {
                Console.WriteLine($"    Redemption validation works: {redemptionResult2.Message.Split('.')[0]}");
            }

            Console.WriteLine();

            // ========================================
            // IN-MEMORY DATA STORAGE DEMONSTRATION
            // ========================================
            Console.WriteLine(" IN-MEMORY DATA STORAGE: CRUD operations without database");
            Console.WriteLine("    Store users, products, events, and transactions");
            Console.WriteLine("    Simulate CRUD operations");
            
            // Demonstrate data retrieval
            var allUsers = await userService.GetActiveUsersAsync();
            var allEvents = await eventService.GetUpcomingEventsAsync();
            var userTransactions = await transactionService.GetUserTransactionsAsync(employee1.Id);
            var activeProducts = await productService.GetActiveProductsAsync();
            
            Console.WriteLine($"    Retrieved data: {allUsers.Count()} users, {allEvents.Count()} upcoming events, {userTransactions.Count()} transactions, {activeProducts.Count()} products");
            
            // Show final balances
            var finalBalance1 = await accountService.GetBalanceAsync(employee1.Id);
            var finalBalance2 = await accountService.GetBalanceAsync(employee2.Id);
            var remainingEventPoints = await pointsAwardingService.GetRemainingPointsPoolAsync(salesEvent.Id);
            
            Console.WriteLine($"    Final balances: {employee1.FirstName} ({finalBalance1}pts), {employee2.FirstName} ({finalBalance2}pts)");
            Console.WriteLine($"    Event pool remaining: {remainingEventPoints} points\n");

            // ========================================
            // MILESTONE 1 COMPLETION SUMMARY
            // ========================================
            Console.WriteLine(" MILESTONE 1 - COMPLETION SUMMARY");
            Console.WriteLine(" Design Business Logic:");
            Console.WriteLine("    Key Entities: User, Event, Product, Redemption, PointsTransaction , RewardAccount");
            Console.WriteLine("    Data Models: C# classes with properties and constructors");
            Console.WriteLine("    Relationships: User has many Redemptions, Events link to transactions");
            Console.WriteLine("    OOP Principles: Encapsulation, inheritance, interfaces for services");
            Console.WriteLine();
            Console.WriteLine(" Implement Core Services (In-Memory):");
            Console.WriteLine("    User Service: Add, update, retrieve users + duplicate prevention");
            Console.WriteLine("    Event Service: Create and manage events + link to transactions");
            Console.WriteLine("    Product Catalog Service: Add, update, delete, list products + validation");
            Console.WriteLine("    Redemption Service: Simulate redemption logic + balance validation");
            Console.WriteLine();
            Console.WriteLine(" In-Memory Data Storage:");
            Console.WriteLine("    Collections (List<T>, Dictionary<K,V>) for data storage");
            Console.WriteLine("    CRUD operations without database");
            Console.WriteLine("    Data persistence during application runtime");
            Console.WriteLine();
            Console.WriteLine(" TECHNICAL IMPLEMENTATION:");
            Console.WriteLine($"    Domain Models: 11 classes implemented");
            Console.WriteLine($"    Service Interfaces: 14 interfaces with SRP compliance");
            Console.WriteLine($"    Repository Pattern: Generic repository + Unit of Work");
            Console.WriteLine($"    Dependency Injection: Full service container setup");
            Console.WriteLine($"    Business Logic: Complete event-reward-redemption workflow");
            Console.WriteLine($"    Data Validation: Email uniqueness, balance checks, stock validation");
            Console.WriteLine($"    In-Memory Storage: No external database required");
        }
    }
}