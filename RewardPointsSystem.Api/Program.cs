using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using RewardPointsSystem.Api.Configuration;
using RewardPointsSystem.Application.Interfaces;
using RewardPointsSystem.Domain.Entities.Core;
using RewardPointsSystem.Domain.Entities.Events;
using RewardPointsSystem.Domain.Entities.Products;
using RewardPointsSystem.Domain.Entities.Operations;
using RewardPointsSystem.Domain.Exceptions;
using RewardPointsSystem.Infrastructure.Data;

namespace RewardPointsSystem.Api
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("========================================");
            Console.WriteLine("  Reward Points System - SQL Server Demo");
            Console.WriteLine("========================================\n");

            IServiceProvider serviceProvider = null;

            try
            {
                // Build configuration
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

                // Validate connection string
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                if (string.IsNullOrEmpty(connectionString))
                {
                    Console.WriteLine("✗ ERROR: Connection string 'DefaultConnection' not found in appsettings.json");
                    Console.WriteLine("\nPress any key to exit...");
                    Console.ReadKey();
                    return;
                }

                // Build the service container
                var services = new ServiceCollection();
                services.AddSingleton<IConfiguration>(configuration);
                services.RegisterRewardPointsServices(configuration);
                
                serviceProvider = services.BuildServiceProvider();

                // Initialize database
                await InitializeDatabaseAsync(serviceProvider);

                // Run interactive demo
                await RunInteractiveDemoAsync(serviceProvider);
            }
            catch (FileNotFoundException fileEx)
            {
                Console.WriteLine($"\n✗ Configuration Error: {fileEx.Message}");
                Console.WriteLine("Please ensure appsettings.json exists in the application directory.");
            }
            catch (Microsoft.Data.SqlClient.SqlException sqlEx)
            {
                Console.WriteLine($"\n✗ Database Connection Error: {sqlEx.Message}");
                Console.WriteLine("\nPossible causes:");
                Console.WriteLine("  1. SQL Server is not running");
                Console.WriteLine("  2. Incorrect server name in connection string");
                Console.WriteLine("  3. Authentication/permission issues");
                Console.WriteLine("\nPlease check your connection string in appsettings.json");
            }
            catch (DomainException domainEx)
            {
                Console.WriteLine($"\n✗ Business Rule Violation: {domainEx.Message}");
                Console.WriteLine($"Exception Type: {domainEx.GetType().Name}");
            }
            catch (InvalidOperationException invEx) when (invEx.Message.Contains("database") || invEx.Message.Contains("migration"))
            {
                Console.WriteLine($"\n✗ Database Initialization Error: {invEx.Message}");
                Console.WriteLine("\nTry running: dotnet ef database update");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n✗ Unexpected Error: {ex.Message}");
                Console.WriteLine($"\nException Type: {ex.GetType().Name}");
                if (args.Contains("--verbose"))
                {
                    Console.WriteLine($"\nStack Trace: {ex.StackTrace}");
                }
            }
            finally
            {
                if (serviceProvider is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }

        private static async Task InitializeDatabaseAsync(IServiceProvider serviceProvider)
        {
            Console.WriteLine("✓ Initializing SQL Server Database...");
            
            try
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<RewardPointsDbContext>();
                    
                    // Test database connection
                    var canConnect = await context.Database.CanConnectAsync();
                    if (!canConnect)
                    {
                        throw new InvalidOperationException("Cannot connect to the database. Please check your connection string.");
                    }
                    
                    // Ensure database is created and migrations are applied
                    await context.Database.MigrateAsync();
                    Console.WriteLine("✓ Database initialized successfully\n");
                }
            }
            catch (Microsoft.Data.SqlClient.SqlException sqlEx)
            {
                Console.WriteLine($"\n✗ Database Connection Failed: {sqlEx.Message}");
                Console.WriteLine("\nPlease verify:");
                Console.WriteLine("  - SQL Server service is running");
                Console.WriteLine("  - Server name is correct in appsettings.json");
                Console.WriteLine("  - You have permission to create databases");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n✗ Database Initialization Failed: {ex.Message}");
                throw;
            }
        }

        private static async Task RunInteractiveDemoAsync(IServiceProvider serviceProvider)
        {
            // Get services
            var userService = serviceProvider.GetRequiredService<IUserService>();
            var roleService = serviceProvider.GetRequiredService<IRoleService>();
            var userRoleService = serviceProvider.GetRequiredService<IUserRoleService>();
            var eventService = serviceProvider.GetRequiredService<IEventService>();
            var pointsAwardingService = serviceProvider.GetRequiredService<IPointsAwardingService>();
            var accountService = serviceProvider.GetRequiredService<IPointsAccountService>();
            var transactionService = serviceProvider.GetRequiredService<ITransactionService>();
            var productService = serviceProvider.GetRequiredService<IProductCatalogService>();
            var pricingService = serviceProvider.GetRequiredService<IPricingService>();
            var inventoryService = serviceProvider.GetRequiredService<IInventoryService>();
            var redemptionOrchestrator = serviceProvider.GetRequiredService<IRedemptionOrchestrator>();

            bool running = true;
            while (running)
            {
                Console.WriteLine("\n========================================");
                Console.WriteLine("       MAIN MENU");
                Console.WriteLine("========================================");
                Console.WriteLine("1. Add New User");
                Console.WriteLine("2. View All Users");
                Console.WriteLine("3. Add New Event");
                Console.WriteLine("4. View All Events");
                Console.WriteLine("5. Delete Event");
                Console.WriteLine("6. Add New Product");
                Console.WriteLine("7. View All Products");
                Console.WriteLine("8. Award Points to User");
                Console.WriteLine("9. View User Balance");
                Console.WriteLine("10. Process Redemption");
                Console.WriteLine("11. View All Transactions");
                Console.WriteLine("0. Exit");
                Console.WriteLine("========================================");
                Console.Write("Select an option: ");

                var choice = Console.ReadLine();
                Console.WriteLine();

                try
                {
                    switch (choice)
                    {
                        case "1":
                            await AddNewUserAsync(userService, roleService, userRoleService);
                            break;
                        case "2":
                            await ViewAllUsersAsync(userService);
                            break;
                        case "3":
                            await AddNewEventAsync(eventService);
                            break;
                        case "4":
                            await ViewAllEventsAsync(eventService);
                            break;
                        case "5":
                            await DeleteEventAsync(eventService);
                            break;
                        case "6":
                            await AddNewProductAsync(productService, pricingService, inventoryService);
                            break;
                        case "7":
                            await ViewAllProductsAsync(productService, pricingService, inventoryService);
                            break;
                        case "8":
                            await AwardPointsToUserAsync(userService, eventService, pointsAwardingService, transactionService, accountService);
                            break;
                        case "9":
                            await ViewUserBalanceAsync(userService, accountService, transactionService);
                            break;
                        case "10":
                            await ProcessRedemptionAsync(userService, productService, redemptionOrchestrator);
                            break;
                        case "11":
                            await ViewAllTransactionsAsync(transactionService, userService);
                            break;
                        case "0":
                            running = false;
                            Console.WriteLine("✓ Thank you for using Reward Points System!");
                            break;
                        default:
                            Console.WriteLine("✗ Invalid option. Please try again.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\n✗ Error: {ex.Message}");
                }
            }
        }

        // ========================================
        // INTERACTIVE MENU METHODS
        // ========================================

        private static async Task AddNewUserAsync(IUserService userService, IRoleService roleService, IUserRoleService userRoleService)
        {
            try
            {
                Console.Write("Enter Email: ");
                var email = Console.ReadLine();
                
                if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
                {
                    Console.WriteLine("✗ Invalid email format. Please enter a valid email address.");
                    return;
                }
                
                Console.Write("Enter First Name: ");
                var firstName = Console.ReadLine();
                
                if (string.IsNullOrWhiteSpace(firstName))
                {
                    Console.WriteLine("✗ First name cannot be empty.");
                    return;
                }
                
                Console.Write("Enter Last Name: ");
                var lastName = Console.ReadLine();
                
                if (string.IsNullOrWhiteSpace(lastName))
                {
                    Console.WriteLine("✗ Last name cannot be empty.");
                    return;
                }

                var user = await userService.CreateUserAsync(email, firstName, lastName);

                Console.WriteLine($"\n✓ User created successfully! ID: {user.Id}");
                Console.WriteLine($"  Name: {user.FirstName} {user.LastName}");
                Console.WriteLine($"  Email: {user.Email}");
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
            {
                Console.WriteLine($"\n✗ Error: A user with this email already exists.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n✗ Error creating user: {ex.Message}");
            }
        }

        private static async Task ViewAllUsersAsync(IUserService userService)
        {
            try
            {
                var users = await userService.GetActiveUsersAsync();
                
                Console.WriteLine("════════════════════════════════════════");
                Console.WriteLine("ALL USERS");
                Console.WriteLine("════════════════════════════════════════");
                
                if (!users.Any())
                {
                    Console.WriteLine("No users found. Try adding users first (Option 1).");
                    return;
                }

                foreach (var user in users)
                {
                    Console.WriteLine($"ID: {user.Id}");
                    Console.WriteLine($"Name: {user.FirstName} {user.LastName}");
                    Console.WriteLine($"Email: {user.Email}");
                    Console.WriteLine($"Created: {user.CreatedAt:yyyy-MM-dd}");
                    Console.WriteLine("────────────────────────────────────────");
                }
                
                Console.WriteLine($"\nTotal Users: {users.Count()}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n✗ Error retrieving users: {ex.Message}");
            }
        }

        private static async Task AddNewEventAsync(IEventService eventService)
        {
            try
            {
                Console.Write("Enter Event Name: ");
                var name = Console.ReadLine();
                
                if (string.IsNullOrWhiteSpace(name))
                {
                    Console.WriteLine("✗ Event name cannot be empty.");
                    return;
                }
                
                Console.Write("Enter Description: ");
                var description = Console.ReadLine();
                
                Console.Write("Enter Days Until Event: ");
                if (!int.TryParse(Console.ReadLine(), out var daysUntil) || daysUntil < 0)
                {
                    Console.WriteLine("✗ Invalid number of days. Please enter a positive number.");
                    return;
                }
                
                Console.Write("Enter Total Points Pool: ");
                if (!int.TryParse(Console.ReadLine(), out var pointsPool) || pointsPool <= 0)
                {
                    Console.WriteLine("✗ Invalid points pool. Please enter a positive number.");
                    return;
                }

                var ev = await eventService.CreateEventAsync(name, description, DateTime.UtcNow.AddDays(daysUntil), pointsPool);
                
                Console.WriteLine($"\n✓ Event created successfully! ID: {ev.Id}");
                Console.WriteLine($"  Name: {ev.Name}");
                Console.WriteLine($"  Date: {ev.EventDate:yyyy-MM-dd}");
                Console.WriteLine($"  Points Pool: {ev.TotalPointsPool}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n✗ Error creating event: {ex.Message}");
            }
        }

        private static async Task ViewAllEventsAsync(IEventService eventService)
        {
            var events = await eventService.GetUpcomingEventsAsync();
            
            Console.WriteLine("════════════════════════════════════════");
            Console.WriteLine("UPCOMING EVENTS");
            Console.WriteLine("════════════════════════════════════════");
            
            if (!events.Any())
            {
                Console.WriteLine("No upcoming events found.");
                return;
            }

            foreach (var ev in events)
            {
                Console.WriteLine($"ID: {ev.Id}");
                Console.WriteLine($"Name: {ev.Name}");
                Console.WriteLine($"Description: {ev.Description}");
                Console.WriteLine($"Date: {ev.EventDate:yyyy-MM-dd HH:mm}");
                Console.WriteLine($"Status: {ev.Status}");
                Console.WriteLine($"Points Pool: {ev.TotalPointsPool}");
                Console.WriteLine("────────────────────────────────────────");
            }
            
            Console.WriteLine($"\nTotal Events: {events.Count()}");
        }

        private static async Task DeleteEventAsync(IEventService eventService)
        {
            try
            {
                Console.Write("Enter Event ID to delete: ");
                var eventIdStr = Console.ReadLine();
                
                if (!Guid.TryParse(eventIdStr, out var eventId))
                {
                    Console.WriteLine("✗ Invalid Event ID format. Please enter a valid GUID.");
                    return;
                }

                await eventService.CancelEventAsync(eventId);
                Console.WriteLine($"\n✓ Event {eventId} has been cancelled successfully!");
            }
            catch (KeyNotFoundException)
            {
                Console.WriteLine($"\n✗ Event not found. Please check the Event ID.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n✗ Error deleting event: {ex.Message}");
            }
        }

        private static async Task AddNewProductAsync(IProductCatalogService productService, IPricingService pricingService, IInventoryService inventoryService)
        {
            try
            {
                Console.Write("Enter Product Name: ");
                var name = Console.ReadLine();
                
                if (string.IsNullOrWhiteSpace(name))
                {
                    Console.WriteLine("✗ Product name cannot be empty.");
                    return;
                }
                
                Console.Write("Enter Description: ");
                var description = Console.ReadLine();
                
                if (string.IsNullOrWhiteSpace(description))
                {
                    description = "No description provided";
                }
                
                Console.Write("Enter Category: ");
                var category = Console.ReadLine();
                
                if (string.IsNullOrWhiteSpace(category))
                {
                    category = "General";
                }
                
                Console.Write("Enter Points Cost: ");
                if (!int.TryParse(Console.ReadLine(), out var pointsCost) || pointsCost <= 0)
                {
                    Console.WriteLine("✗ Invalid points cost. Please enter a positive number.");
                    return;
                }
                
                Console.Write("Enter Stock Quantity: ");
                if (!int.TryParse(Console.ReadLine(), out var quantity) || quantity < 0)
                {
                    Console.WriteLine("✗ Invalid quantity. Please enter a non-negative number.");
                    return;
                }
                
                Console.Write("Enter Reorder Level: ");
                if (!int.TryParse(Console.ReadLine(), out var reorderLevel) || reorderLevel < 0)
                {
                    Console.WriteLine("✗ Invalid reorder level. Please enter a non-negative number.");
                    return;
                }

                var product = await productService.CreateProductAsync(name, description, category);
                await pricingService.SetProductPointsCostAsync(product.Id, pointsCost, DateTime.UtcNow);
                await inventoryService.CreateInventoryAsync(product.Id, quantity, reorderLevel);
                
                Console.WriteLine($"\n✓ Product created successfully! ID: {product.Id}");
                Console.WriteLine($"  Name: {product.Name}");
                Console.WriteLine($"  Category: {product.Category}");
                Console.WriteLine($"  Points Cost: {pointsCost}");
                Console.WriteLine($"  Stock: {quantity}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n✗ Error creating product: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"   Details: {ex.InnerException.Message}");
                }
            }
        }

        private static async Task ViewAllProductsAsync(IProductCatalogService productService, IPricingService pricingService, IInventoryService inventoryService)
        {
            var products = await productService.GetActiveProductsAsync();
            
            Console.WriteLine("════════════════════════════════════════");
            Console.WriteLine("ALL PRODUCTS");
            Console.WriteLine("════════════════════════════════════════");
            
            if (!products.Any())
            {
                Console.WriteLine("No products found.");
                return;
            }

            foreach (var product in products)
            {
                var pointsCost = await pricingService.GetCurrentPointsCostAsync(product.Id);
                var isInStock = await inventoryService.IsInStockAsync(product.Id);
                
                Console.WriteLine($"ID: {product.Id}");
                Console.WriteLine($"Name: {product.Name}");
                Console.WriteLine($"Category: {product.Category}");
                Console.WriteLine($"Description: {product.Description}");
                Console.WriteLine($"Points Cost: {pointsCost}");
                Console.WriteLine($"In Stock: {(isInStock ? "Yes" : "No")}");
                Console.WriteLine("────────────────────────────────────────");
            }
            
            Console.WriteLine($"\nTotal Products: {products.Count()}");
        }

        private static async Task AwardPointsToUserAsync(IUserService userService, IEventService eventService, 
            IPointsAwardingService pointsAwardingService, ITransactionService transactionService, IPointsAccountService accountService)
        {
            try
            {
                Console.Write("Enter User Email: ");
                var email = Console.ReadLine();
                
                if (string.IsNullOrWhiteSpace(email))
                {
                    Console.WriteLine("✗ Email cannot be empty.");
                    return;
                }
                
                var user = await userService.GetUserByEmailAsync(email);
                
                if (user == null)
                {
                    Console.WriteLine("✗ User not found. Please check the email address.");
                    return;
                }

                // Ensure user has an account
                try
                {
                    await accountService.GetBalanceAsync(user.Id);
                }
                catch
                {
                    await accountService.CreateAccountAsync(user.Id);
                    Console.WriteLine("✓ Points account created for user.");
                }

                Console.Write("Enter Event ID (or press Enter to skip event): ");
                var eventIdStr = Console.ReadLine();
                Guid? eventId = null;
                
                if (!string.IsNullOrWhiteSpace(eventIdStr))
                {
                    if (!Guid.TryParse(eventIdStr, out var parsedEventId))
                    {
                        Console.WriteLine("✗ Invalid Event ID format. Continuing without event...");
                    }
                    else
                    {
                        eventId = parsedEventId;
                    }
                }

                Console.Write("Enter Points Amount: ");
                if (!int.TryParse(Console.ReadLine(), out var points) || points <= 0)
                {
                    Console.WriteLine("✗ Invalid points amount. Please enter a positive number.");
                    return;
                }
                
                Console.Write("Enter Description: ");
                var description = Console.ReadLine();
                
                if (string.IsNullOrWhiteSpace(description))
                {
                    description = "Points awarded";
                }

                if (eventId.HasValue)
                {
                    await pointsAwardingService.AwardPointsAsync(eventId.Value, user.Id, points, 1);
                    await transactionService.RecordEarnedPointsAsync(user.Id, points, eventId.Value, description);
                }
                else
                {
                    await transactionService.RecordEarnedPointsAsync(user.Id, points, Guid.NewGuid(), description);
                }
                
                await accountService.AddPointsAsync(user.Id, points);
                
                Console.WriteLine($"\n✓ {points} points awarded to {user.FirstName} {user.LastName}!");
                var balance = await accountService.GetBalanceAsync(user.Id);
                Console.WriteLine($"  New Balance: {balance} points");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n✗ Error awarding points: {ex.Message}");
            }
        }

        private static async Task ViewUserBalanceAsync(IUserService userService, IPointsAccountService accountService, ITransactionService transactionService)
        {
            Console.Write("Enter User Email: ");
            var email = Console.ReadLine();
            var user = await userService.GetUserByEmailAsync(email);
            
            if (user == null)
            {
                Console.WriteLine("✗ User not found.");
                return;
            }

            try
            {
                var account = await accountService.GetAccountAsync(user.Id);
                var transactions = await transactionService.GetUserTransactionsAsync(user.Id);
                
                Console.WriteLine($"\n════════════════════════════════════════");
                Console.WriteLine($"ACCOUNT FOR: {user.FirstName} {user.LastName}");
                Console.WriteLine("════════════════════════════════════════");
                Console.WriteLine($"Current Balance: {account.CurrentBalance} points");
                Console.WriteLine($"Total Earned: {account.TotalEarned} points");
                Console.WriteLine($"Total Redeemed: {account.TotalRedeemed} points");
                Console.WriteLine($"\nRecent Transactions ({transactions.Count()}):");
                
                foreach (var tx in transactions.Take(10))
                {
                    Console.WriteLine($"  {tx.Timestamp:yyyy-MM-dd} | {tx.Type} | {tx.Points} pts | {tx.Description}");
                }
            }
            catch
            {
                Console.WriteLine($"\n✗ No account found for {user.FirstName} {user.LastName}.");
            }
        }

        private static async Task ProcessRedemptionAsync(IUserService userService, IProductCatalogService productService, IRedemptionOrchestrator redemptionOrchestrator)
        {
            try
            {
                Console.Write("Enter User Email: ");
                var email = Console.ReadLine();
                
                if (string.IsNullOrWhiteSpace(email))
                {
                    Console.WriteLine("✗ Email cannot be empty.");
                    return;
                }
                
                var user = await userService.GetUserByEmailAsync(email);
                
                if (user == null)
                {
                    Console.WriteLine("✗ User not found. Please check the email address.");
                    return;
                }

                Console.Write("Enter Product ID: ");
                var productIdStr = Console.ReadLine();
                
                if (!Guid.TryParse(productIdStr, out var productId))
                {
                    Console.WriteLine("✗ Invalid Product ID format. Please enter a valid GUID.");
                    return;
                }

                var result = await redemptionOrchestrator.ProcessRedemptionAsync(user.Id, productId);
                
                if (result.Success)
                {
                    Console.WriteLine($"\n✓ Redemption processed successfully!");
                    Console.WriteLine($"  Redemption ID: {result.Redemption.Id}");
                    Console.WriteLine($"  Status: {result.Redemption.Status}");
                    Console.WriteLine($"  Points Spent: {result.Redemption.PointsSpent}");
                }
                else
                {
                    Console.WriteLine($"\n✗ Redemption failed: {result.Message}");
                }
            }
            catch (InvalidOperationException invEx) when (invEx.Message.Contains("balance") || invEx.Message.Contains("points"))
            {
                Console.WriteLine($"\n✗ Insufficient balance: {invEx.Message}");
            }
            catch (InvalidOperationException invEx) when (invEx.Message.Contains("stock") || invEx.Message.Contains("inventory"))
            {
                Console.WriteLine($"\n✗ Product unavailable: {invEx.Message}");
            }
            catch (KeyNotFoundException)
            {
                Console.WriteLine($"\n✗ Product not found. Please check the Product ID.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n✗ Error processing redemption: {ex.Message}");
            }
        }

        private static async Task ViewAllTransactionsAsync(ITransactionService transactionService, IUserService userService)
        {
            var allUsers = await userService.GetActiveUsersAsync();
            
            Console.WriteLine("════════════════════════════════════════");
            Console.WriteLine("ALL TRANSACTIONS");
            Console.WriteLine("════════════════════════════════════════");
            
            foreach (var user in allUsers)
            {
                var transactions = await transactionService.GetUserTransactionsAsync(user.Id);
                if (transactions.Any())
                {
                    Console.WriteLine($"\n{user.FirstName} {user.LastName} ({user.Email}):");
                    foreach (var tx in transactions)
                    {
                        Console.WriteLine($"  {tx.Timestamp:yyyy-MM-dd HH:mm} | {tx.Type} | {tx.Points} pts | {tx.Description}");
                    }
                }
            }
        }
    }
}
