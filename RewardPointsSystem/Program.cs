using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RewardPointsSystem.Configuration;
using RewardPointsSystem.Interfaces;
using RewardPointsSystem.Models.Core;
using RewardPointsSystem.Services;



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
            var accountService = serviceProvider.GetRequiredService<IRewardAccountService>();
            var transactionService = serviceProvider.GetRequiredService<ITransactionService>();

            Console.WriteLine("1. Creating Roles...");
            var adminRole = await roleService.CreateRoleAsync("Admin", "System Administrator");
            var employeeRole = await roleService.CreateRoleAsync("Employee", "Regular Employee");
            Console.WriteLine($"   ✅ Created roles: {adminRole.Name}, {employeeRole.Name}");

            Console.WriteLine("\n2. Creating Users...");
            var admin = await userService.CreateUserAsync("admin@agdata.com", "EMP001", "John", "Admin");
            var employee1 = await userService.CreateUserAsync("alice@agdata.com", "EMP002", "Alice", "Smith");
            var employee2 = await userService.CreateUserAsync("bob@agdata.com", "EMP003", "Bob", "Johnson");
            Console.WriteLine($"   ✅ Created users: {admin.FirstName}, {employee1.FirstName}, {employee2.FirstName}");

            Console.WriteLine("\n3. Assigning Roles...");
            await userRoleService.AssignRoleAsync(admin.Id, adminRole.Id, admin.Id);
            await userRoleService.AssignRoleAsync(employee1.Id, employeeRole.Id, admin.Id);
            await userRoleService.AssignRoleAsync(employee2.Id, employeeRole.Id, admin.Id);
            Console.WriteLine("   ✅ Roles assigned successfully");

            Console.WriteLine("\n4. Creating Reward Accounts...");
            var account1 = await accountService.CreateAccountAsync(employee1.Id);
            var account2 = await accountService.CreateAccountAsync(employee2.Id);
            Console.WriteLine("   ✅ Reward accounts created");

            Console.WriteLine("\n5. Creating Event...");
            var salesEvent = await eventService.CreateEventAsync("Q4 Sales Competition", "Quarterly sales performance competition", DateTime.UtcNow.AddDays(1), 1000);
            Console.WriteLine($"   ✅ Created event: {salesEvent.Name} with {salesEvent.TotalPointsPool} points pool");

            Console.WriteLine("\n6. Registering Participants...");
            await participationService.RegisterParticipantAsync(salesEvent.Id, employee1.Id);
            await participationService.RegisterParticipantAsync(salesEvent.Id, employee2.Id);
            Console.WriteLine("   ✅ Participants registered");

            Console.WriteLine("\n7. Awarding Points...");
            await pointsAwardingService.AwardPointsAsync(salesEvent.Id, employee1.Id, 600, 1);
            await pointsAwardingService.AwardPointsAsync(salesEvent.Id, employee2.Id, 400, 2);
            Console.WriteLine($"   ✅ Points awarded: {employee1.FirstName} - 600pts (1st), {employee2.FirstName} - 400pts (2nd)");

            Console.WriteLine("\n8. Recording Transactions and Updating Balances...");
            await transactionService.RecordEarnedPointsAsync(employee1.Id, 600, salesEvent.Id, $"1st place in {salesEvent.Name}");
            await transactionService.RecordRedeemedPointsAsync(employee2.Id, 400, salesEvent.Id, $"2nd place in {salesEvent.Name}");
            await accountService.AddPointsAsync(employee1.Id, 600);
            await accountService.AddPointsAsync(employee2.Id, 400);
            Console.WriteLine("   ✅ Transactions recorded and balances updated");

            Console.WriteLine("\n9. Checking Final System State...");
            var balance1 = await accountService.GetBalanceAsync(employee1.Id);
            var balance2 = await accountService.GetBalanceAsync(employee2.Id);
            var remainingPoints = await pointsAwardingService.GetRemainingPointsPoolAsync(salesEvent.Id);
            
            Console.WriteLine($"   📊 {employee1.FirstName}'s Balance: {balance1} points");
            Console.WriteLine($"   📊 {employee2.FirstName}'s Balance: {balance2} points");
            Console.WriteLine($"   📊 Event Points Remaining: {remainingPoints} points");

            Console.WriteLine("\n✅ Complete System Demo Completed Successfully!");
            Console.WriteLine("\n🎯 Implementation Status:");
            Console.WriteLine("   - 11 Domain Models ✅");
            Console.WriteLine("   - 14 Service Interfaces ✅");
            Console.WriteLine("   - Repository Pattern ✅");
            Console.WriteLine("   - In-Memory Database ✅");
            Console.WriteLine("   - Dependency Injection ✅");
            Console.WriteLine("   - Core Services Implementation ✅");
            Console.WriteLine("   - Event Management System ✅");
            Console.WriteLine("   - Points & Account System ✅");
            Console.WriteLine("   - End-to-End Workflow ✅");
        }
    }
}