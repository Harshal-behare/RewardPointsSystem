using System;
using System.Linq;
using RewardPointsSystem.Models;
using RewardPointsSystem.Services;
using RewardPointsSystem.Repositories;
using RewardPointsSystem.Interfaces;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== AGDATA Reward Points System - Milestone 1 ===");
        Console.WriteLine("\nInitializing services...\n");

        // Initialize Unit of Work and Services (simulating DI container)
        var unitOfWork = new InMemoryUnitOfWork();
        var roleService = new RoleService();
        var inventoryService = new InventoryService();
        var userService = new UserService(unitOfWork, roleService);
        var productService = new ProductService(unitOfWork, inventoryService);
        var redemptionService = new RedemptionService(unitOfWork, inventoryService, userService, roleService);

        try
        {
            // 1. Create and add a user
            Console.WriteLine("1. Creating user...");
            var user = new User("Harshal Behare", "harshal.behare@agdata.com", "EMP001");
            userService.AddUser(user);
            Console.WriteLine($"   ✓ User created: {user.Name} (ID: {user.Id})");
            Console.WriteLine($"   ✓ Default role assigned: Employee");

            // 2. Create and add products
            Console.WriteLine("\n2. Creating products...");
            var coffeeMug = new Product("Coffee Mug", 50, "Merchandise", "AGDATA branded coffee mug");
            productService.AddProduct(coffeeMug);
            inventoryService.AddInventoryItem(coffeeMug.Id, 10);
            Console.WriteLine($"   ✓ Product created: {coffeeMug.Name} (50 points, 10 in stock)");

            var tShirt = new Product("AGDATA T-Shirt", 100, "Merchandise", "Company branded t-shirt");
            productService.AddProduct(tShirt);
            inventoryService.AddInventoryItem(tShirt.Id, 5);
            Console.WriteLine($"   ✓ Product created: {tShirt.Name} (100 points, 5 in stock)");

            // 3. Award points to user
            Console.WriteLine("\n3. Awarding points to user...");
            user.AddPoints(150);
            var earnTransaction = new PointsTransaction(user, 150, "Earn", "Monthly performance bonus");
            unitOfWork.PointsTransactions.Add(earnTransaction);
            unitOfWork.Complete();
            Console.WriteLine($"   ✓ Awarded 150 points to {user.Name}");
            Console.WriteLine($"   ✓ Current balance: {user.PointsBalance} points");

            // 4. Display available products
            Console.WriteLine("\n4. Available products:");
            foreach (var product in productService.GetAllProducts())
            {
                var stock = inventoryService.GetAvailableQuantity(product.Id);
                Console.WriteLine($"   - {product.Name}: {product.RequiredPoints} points (Stock: {stock})");
            }

            // 5. Redeem a product
            Console.WriteLine("\n5. Redeeming product...");
            var redemption = redemptionService.RedeemProduct(user, coffeeMug);
            Console.WriteLine($"   ✓ {user.Name} redeemed {redemption.Product.Name}");
            Console.WriteLine($"   ✓ New balance: {user.PointsBalance} points");
            Console.WriteLine($"   ✓ Coffee Mug stock: {inventoryService.GetAvailableQuantity(coffeeMug.Id)}");

            // 6. Display transaction history
            Console.WriteLine("\n6. Transaction History:");
            var transactions = unitOfWork.PointsTransactions.Find(t => t.User.Id == user.Id);
            foreach (var tx in transactions)
            {
                var sign = tx.Type == "Earn" ? "+" : "-";
                Console.WriteLine($"   {tx.Timestamp:yyyy-MM-dd HH:mm:ss} | {sign}{Math.Abs(tx.Points)} points | {tx.Description}");
            }

            // 7. Demonstrate role management
            Console.WriteLine("\n7. Role Management Demo:");
            var adminRole = roleService.GetRoleByName("Admin");
            userService.AssignRoleToUser(user.Id, adminRole.Id);
            Console.WriteLine($"   ✓ Assigned Admin role to {user.Name}");
            
            var permissions = roleService.GetUserPermissions(user);
            Console.WriteLine($"   ✓ User now has {permissions.Count()} permissions");

            // 8. Try to redeem another product
            Console.WriteLine("\n8. Attempting to redeem T-Shirt...");
            try
            {
                var redemption2 = redemptionService.RedeemProduct(user, tShirt);
                Console.WriteLine($"   ✓ {user.Name} redeemed {redemption2.Product.Name}");
                Console.WriteLine($"   ✓ Final balance: {user.PointsBalance} points");
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"   ✗ Redemption failed: {ex.Message}");
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nError: {ex.Message}");
        }

        Console.WriteLine("\n=== End of Demo ===");
        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
}
