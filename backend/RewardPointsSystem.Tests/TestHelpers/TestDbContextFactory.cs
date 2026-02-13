using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RewardPointsSystem.Application.Interfaces;
using RewardPointsSystem.Infrastructure.Data;
using RewardPointsSystem.Infrastructure.Repositories;
using System;
using System.IO;

namespace RewardPointsSystem.Tests.TestHelpers
{
    /// <summary>
    /// Factory for creating database contexts for testing.
    /// Uses SQL Server for integration tests against real database.
    /// </summary>
    public static class TestDbContextFactory
    {
        private static readonly string TestConnectionString;

        static TestDbContextFactory()
        {
            // Load connection string from appsettings.Testing.json
            var configuration = new ConfigurationBuilder()
                .SetBasePath(GetApiProjectPath())
                .AddJsonFile("appsettings.Testing.json", optional: false)
                .Build();

            TestConnectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Test connection string not found in appsettings.Testing.json");
        }

        private static string GetApiProjectPath()
        {
            // Navigate from test project to API project
            var currentDir = Directory.GetCurrentDirectory();
            var solutionDir = Path.GetFullPath(Path.Combine(currentDir, "..", "..", "..", ".."));
            return Path.Combine(solutionDir, "RewardPointsSystem.Api");
        }

        /// <summary>
        /// Creates a new RewardPointsDbContext connected to SQL Server test database.
        /// </summary>
        public static RewardPointsDbContext CreateSqlServerContext()
        {
            var options = new DbContextOptionsBuilder<RewardPointsDbContext>()
                .UseSqlServer(TestConnectionString)
                .Options;

            var context = new RewardPointsDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        /// <summary>
        /// Creates a new IUnitOfWork backed by SQL Server test database.
        /// </summary>
        public static IUnitOfWork CreateSqlServerUnitOfWork()
        {
            var context = CreateSqlServerContext();
            return new EfUnitOfWork(context);
        }

        /// <summary>
        /// Creates a new IUnitOfWork backed by a shared database context.
        /// </summary>
        public static IUnitOfWork CreateSqlServerUnitOfWork(RewardPointsDbContext context)
        {
            return new EfUnitOfWork(context);
        }

        /// <summary>
        /// Gets the test connection string.
        /// </summary>
        public static string GetConnectionString() => TestConnectionString;

        /// <summary>
        /// Cleans up all data in the test database.
        /// Use this at the start of tests that need a clean state.
        /// </summary>
        public static void CleanupDatabase()
        {
            using var context = CreateSqlServerContext();
            
            // Ensure database and tables exist
            context.Database.EnsureCreated();
            
            // Use safer approach: disable all FK constraints, delete, then re-enable
            context.Database.ExecuteSqlRaw(@"
                -- Disable all constraints
                EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL'
            ");
            
            // Delete from all user tables
            context.Database.ExecuteSqlRaw(@"
                EXEC sp_MSforeachtable 'DELETE FROM ?'
            ");
            
            // Re-enable all constraints  
            context.Database.ExecuteSqlRaw(@"
                EXEC sp_MSforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL'
            ");
        }

        /// <summary>
        /// Creates a clean SQL Server context with empty tables.
        /// </summary>
        public static RewardPointsDbContext CreateCleanSqlServerContext()
        {
            CleanupDatabase();
            return CreateSqlServerContext();
        }

        /// <summary>
        /// Creates a clean SQL Server unit of work with empty tables.
        /// </summary>
        public static IUnitOfWork CreateCleanSqlServerUnitOfWork()
        {
            CleanupDatabase();
            return CreateSqlServerUnitOfWork();
        }

        // Legacy methods for backward compatibility - now use SQL Server
        [Obsolete("Use CreateSqlServerContext instead")]
        public static RewardPointsDbContext CreateInMemoryContext() => CreateSqlServerContext();

        [Obsolete("Use CreateSqlServerUnitOfWork instead")]
        public static IUnitOfWork CreateInMemoryUnitOfWork() => CreateCleanSqlServerUnitOfWork();

        [Obsolete("Use CreateSqlServerUnitOfWork instead")]
        public static IUnitOfWork CreateInMemoryUnitOfWork(RewardPointsDbContext context) => CreateSqlServerUnitOfWork(context);
    }
}
