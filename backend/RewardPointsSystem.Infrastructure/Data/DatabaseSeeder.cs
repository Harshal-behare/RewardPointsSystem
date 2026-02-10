using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RewardPointsSystem.Application.Interfaces;
using RewardPointsSystem.Domain.Entities.Accounts;
using RewardPointsSystem.Domain.Entities.Core;

namespace RewardPointsSystem.Infrastructure.Data
{
    /// <summary>
    /// Database seeder for essential system data
    /// Seeds: Roles, Users, UserRoles, and UserPointsAccounts
    /// </summary>
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<RewardPointsDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<RewardPointsDbContext>>();
            var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

            try
            {
                // Ensure database is created and migrated
                await context.Database.MigrateAsync();

                // Seed essential data
                var roles = await SeedRolesAsync(context, logger);
                var users = await SeedUsersAsync(context, logger, passwordHasher);
                await SeedUserRolesAsync(context, logger, roles, users);
                await SeedUserPointsAccountsAsync(context, logger, users);

                await context.SaveChangesAsync();
                
                logger.LogInformation("Database seeding completed successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database");
                throw;
            }
        }

        private static async Task<Dictionary<string, Role>> SeedRolesAsync(RewardPointsDbContext context, ILogger logger)
        {
            var roleDict = new Dictionary<string, Role>();

            if (await context.Roles.AnyAsync())
            {
                logger.LogInformation("Roles already exist, loading existing roles");
                var existingRoles = await context.Roles.ToListAsync();
                foreach (var role in existingRoles)
                {
                    roleDict[role.Name] = role;
                }
                return roleDict;
            }

            var roles = new[]
            {
                Role.Create("Admin", "System Administrator with full access to manage users, products, events, and redemptions"),
                Role.Create("Employee", "Regular employee who can earn points, participate in events, and redeem products"),
            };

            await context.Roles.AddRangeAsync(roles);
            await context.SaveChangesAsync();
            
            foreach (var role in roles)
            {
                roleDict[role.Name] = role;
            }

            logger.LogInformation("Seeded {Count} roles: Admin, Employee", roles.Length);
            return roleDict;
        }

        private static async Task<Dictionary<string, User>> SeedUsersAsync(
            RewardPointsDbContext context, 
            ILogger logger,
            IPasswordHasher passwordHasher)
        {
            var userDict = new Dictionary<string, User>();

            if (await context.Users.AnyAsync())
            {
                logger.LogInformation("Users already exist, loading existing users");
                var existingUsers = await context.Users.ToListAsync();
                foreach (var user in existingUsers)
                {
                    userDict[user.Email] = user;
                }
                return userDict;
            }

            // Admin User: system@agdata.com / System@123
            var adminUser = User.Create("system@agdata.com", "System", "Administrator");
            adminUser.SetPasswordHash(passwordHasher.HashPassword("System@123"));

            // Employee User: Harshal.Behare@agdata.com / Harshal@123
            var employeeUser = User.Create("harshal.behare@agdata.com", "Harshal", "Behare");
            employeeUser.SetPasswordHash(passwordHasher.HashPassword("Harshal@123"));

            var users = new[] { adminUser, employeeUser };

            await context.Users.AddRangeAsync(users);
            await context.SaveChangesAsync();

            userDict[adminUser.Email] = adminUser;
            userDict[employeeUser.Email] = employeeUser;

            logger.LogInformation("Seeded {Count} users: system@agdata.com (Admin), harshal.behare@agdata.com (Employee)", users.Length);
            return userDict;
        }

        private static async Task SeedUserRolesAsync(
            RewardPointsDbContext context, 
            ILogger logger,
            Dictionary<string, Role> roles,
            Dictionary<string, User> users)
        {
            if (await context.UserRoles.AnyAsync())
            {
                logger.LogInformation("UserRoles already exist, skipping seed");
                return;
            }

            var adminRole = roles["Admin"];
            var employeeRole = roles["Employee"];
            var adminUser = users["system@agdata.com"];
            var employeeUser = users["harshal.behare@agdata.com"];

            var userRoles = new[]
            {
                // Admin user gets Admin role (assigned by system - using own ID)
                UserRole.Assign(adminUser.Id, adminRole.Id, adminUser.Id),
                // Employee user gets Employee role (assigned by admin)
                UserRole.Assign(employeeUser.Id, employeeRole.Id, adminUser.Id),
            };

            await context.UserRoles.AddRangeAsync(userRoles);
            logger.LogInformation("Seeded {Count} user role assignments", userRoles.Length);
        }

        private static async Task SeedUserPointsAccountsAsync(
            RewardPointsDbContext context, 
            ILogger logger,
            Dictionary<string, User> users)
        {
            if (await context.UserPointsAccounts.AnyAsync())
            {
                logger.LogInformation("UserPointsAccounts already exist, skipping seed");
                return;
            }

            var pointsAccounts = users.Values.Select(user => 
                UserPointsAccount.CreateForUser(user.Id)
            ).ToArray();

            await context.UserPointsAccounts.AddRangeAsync(pointsAccounts);
            logger.LogInformation("Seeded {Count} user points accounts", pointsAccounts.Length);
        }
    }
}
