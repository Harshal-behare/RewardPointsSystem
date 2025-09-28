using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RewardPointsSystem.Configuration;
using RewardPointsSystem.Interfaces;

namespace RewardPointsSystem
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== AGDATA Reward Points System - SRP Architecture Demo ===\n");

            // Build the service container
            var services = new ServiceCollection();
            services.RegisterRewardPointsServices();
            
            var serviceProvider = services.BuildServiceProvider();

            try
            {
                await RunDemoAsync(serviceProvider);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                serviceProvider?.Dispose();
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }

        private static async Task RunDemoAsync(IServiceProvider serviceProvider)
        {
            // Get services
            var userService = serviceProvider.GetRequiredService<IUserService>();
            var roleService = serviceProvider.GetRequiredService<IRoleService>();
            var userRoleService = serviceProvider.GetRequiredService<IUserRoleService>();
            var eventService = serviceProvider.GetRequiredService<IEventService>();
            var participationService = serviceProvider.GetRequiredService<IEventParticipationService>();
            var pointsAwardingService = serviceProvider.GetRequiredService<IPointsAwardingService>();
            var rewardAccountService = serviceProvider.GetRequiredService<IRewardAccountService>();
            var transactionService = serviceProvider.GetRequiredService<ITransactionService>();
            var productService = serviceProvider.GetRequiredService<IProductCatalogService>();

            Console.WriteLine("1. Creating Roles...");
            var adminRole = await roleService.CreateRoleAsync("Admin", "System Administrator");
            var employeeRole = await roleService.CreateRoleAsync("Employee", "Regular Employee");
            Console.WriteLine($"   ✅ Created roles: {adminRole.Name}, {employeeRole.Name}");

            Console.WriteLine("\n2. Creating Users...");
            var admin = await userService.CreateUserAsync(new CreateUserDto 
            { 
                FirstName = "John", 
                LastName = "Admin", 
                Email = "admin@agdata.com", 
                EmployeeId = "EMP001" 
            });
            
            var employee1 = await userService.CreateUserAsync(new CreateUserDto 
            { 
                FirstName = "Alice", 
                LastName = "Smith", 
                Email = "alice@agdata.com", 
                EmployeeId = "EMP002" 
            });

            var employee2 = await userService.CreateUserAsync(new CreateUserDto 
            { 
                FirstName = "Bob", 
                LastName = "Johnson", 
                Email = "bob@agdata.com", 
                EmployeeId = "EMP003" 
            });
            
            Console.WriteLine($"   ✅ Created users: {admin.FirstName} {admin.LastName}, {employee1.FirstName} {employee1.LastName}, {employee2.FirstName} {employee2.LastName}");

            Console.WriteLine("\n3. Assigning Roles...");
            await userRoleService.AssignRoleAsync(admin.Id, adminRole.Id);
            await userRoleService.AssignRoleAsync(employee1.Id, employeeRole.Id);
            await userRoleService.AssignRoleAsync(employee2.Id, employeeRole.Id);
            Console.WriteLine("   ✅ Roles assigned successfully");

            Console.WriteLine("\n4. Creating Reward Accounts...");
            await rewardAccountService.CreateAccountAsync(employee1.Id);
            await rewardAccountService.CreateAccountAsync(employee2.Id);
            Console.WriteLine("   ✅ Reward accounts created");

            Console.WriteLine("\n5. Creating Event...");
            var salesEvent = await eventService.CreateEventAsync(new CreateEventDto
            {
                Name = "Q3 Sales Competition",
                Description = "Quarterly sales performance competition",
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(30),
                PointsReward = 1000,
                MaxParticipants = 10
            });
            Console.WriteLine($"   ✅ Created event: {salesEvent.Name} with {salesEvent.PointsReward} points pool");

            Console.WriteLine("\n6. Registering Participants...");
            await participationService.RegisterParticipantAsync(salesEvent.Id, employee1.Id);
            await participationService.RegisterParticipantAsync(salesEvent.Id, employee2.Id);
            Console.WriteLine("   ✅ Participants registered");

            Console.WriteLine("\n7. Awarding Points...");
            var winner1 = await pointsAwardingService.AwardPointsAsync(salesEvent.Id, employee1.Id, 600, 1);
            var winner2 = await pointsAwardingService.AwardPointsAsync(salesEvent.Id, employee2.Id, 400, 2);
            Console.WriteLine($"   ✅ Points awarded: {employee1.FirstName} - 600pts (1st), {employee2.FirstName} - 400pts (2nd)");

            Console.WriteLine("\n8. Recording Transactions...");
            await transactionService.RecordEarnedPointsAsync(employee1.Id, 600, salesEvent.Id, $"1st place in {salesEvent.Name}");
            await transactionService.RecordEarnedPointsAsync(employee2.Id, 400, salesEvent.Id, $"2nd place in {salesEvent.Name}");
            Console.WriteLine("   ✅ Transactions recorded");

            Console.WriteLine("\n9. Updating Account Balances...");
            await rewardAccountService.AddPointsAsync(employee1.Id, 600);
            await rewardAccountService.AddPointsAsync(employee2.Id, 400);
            Console.WriteLine("   ✅ Account balances updated");

            Console.WriteLine("\n10. Creating Products...");
            var laptop = await productService.CreateProductAsync("MacBook Pro", "High-performance laptop", "Electronics");
            var giftCard = await productService.CreateProductAsync("Amazon Gift Card", "$100 Amazon gift card", "Gift Cards");
            Console.WriteLine($"   ✅ Created products: {laptop.Name}, {giftCard.Name}");

            Console.WriteLine("\n11. Checking System State...");
            var alice_balance = await rewardAccountService.GetBalanceAsync(employee1.Id);
            var bob_balance = await rewardAccountService.GetBalanceAsync(employee2.Id);
            var activeProducts = await productService.GetActiveProductsAsync();
            var activeEvents = await eventService.GetActiveEventsAsync();

            Console.WriteLine($"   📊 Alice's Balance: {alice_balance} points");
            Console.WriteLine($"   📊 Bob's Balance: {bob_balance} points");
            Console.WriteLine($"   📊 Active Products: {activeProducts.Count()}");
            Console.WriteLine($"   📊 Active Events: {activeEvents.Count()}");

            Console.WriteLine("\n✅ System Demo Completed Successfully!");
            Console.WriteLine("\n🎯 Architecture Summary:");
            Console.WriteLine("   - 11 Domain Models ✅");
            Console.WriteLine("   - 14 Service Interfaces ✅");
            Console.WriteLine("   - Repository Pattern ✅");
            Console.WriteLine("   - Dependency Injection ✅");
            Console.WriteLine("   - Single Responsibility Principle ✅");
            Console.WriteLine("   - Clean Architecture ✅");
        }
    }
}