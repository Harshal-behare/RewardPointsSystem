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
            // Get UnitOfWork to demonstrate repository pattern
            var unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();

            Console.WriteLine("1. Demonstrating Repository Pattern...");
            
            // Create sample user using repository
            var user = new RewardPointsSystem.Models.Core.User
            {
                FirstName = "John",
                LastName = "Doe", 
                Email = "john.doe@agdata.com",
                EmployeeId = "EMP001"
            };
            
            await unitOfWork.Users.AddAsync(user);
            await unitOfWork.SaveChangesAsync();
            Console.WriteLine($"   ✅ Created user: {user.FirstName} {user.LastName}");
            
            // Create sample role
            var adminRole = new RewardPointsSystem.Models.Core.Role
            {
                Name = "Admin",
                Description = "System Administrator"
            };
            
            await unitOfWork.Roles.AddAsync(adminRole);
            await unitOfWork.SaveChangesAsync();
            Console.WriteLine($"   ✅ Created role: {adminRole.Name}");
            
            Console.WriteLine("\n2. Testing Repository Queries...");
            var allUsers = await unitOfWork.Users.GetAllAsync();
            var allRoles = await unitOfWork.Roles.GetAllAsync();
            Console.WriteLine($"   📊 Total Users: {allUsers.Count()}");
            Console.WriteLine($"   📊 Total Roles: {allRoles.Count()}");
            
            Console.WriteLine("\n✅ Repository Pattern Demo Completed Successfully!");
            Console.WriteLine("\n🎯 Current Implementation Status:");
            Console.WriteLine("   - 11 Domain Models ✅");
            Console.WriteLine("   - 14 Service Interfaces ✅");
            Console.WriteLine("   - Repository Pattern ✅");
            Console.WriteLine("   - In-Memory Database ✅");
            Console.WriteLine("   - Dependency Injection ✅");
            Console.WriteLine("   - Service Implementations (In Progress...) ⏳");
        }
    }
}