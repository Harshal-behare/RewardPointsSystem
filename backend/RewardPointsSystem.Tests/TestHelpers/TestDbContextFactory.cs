using Microsoft.EntityFrameworkCore;
using RewardPointsSystem.Application.Interfaces;
using RewardPointsSystem.Infrastructure.Data;
using RewardPointsSystem.Infrastructure.Repositories;
using System;

namespace RewardPointsSystem.Tests.TestHelpers
{
    /// <summary>
    /// Factory for creating in-memory database contexts for testing.
    /// Uses EF Core InMemory provider instead of custom in-memory repositories.
    /// </summary>
    public static class TestDbContextFactory
    {
        /// <summary>
        /// Creates a new RewardPointsDbContext with an in-memory database.
        /// Each call creates a new unique database instance.
        /// </summary>
        public static RewardPointsDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<RewardPointsDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new RewardPointsDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        /// <summary>
        /// Creates a new IUnitOfWork backed by an in-memory database.
        /// </summary>
        public static IUnitOfWork CreateInMemoryUnitOfWork()
        {
            var context = CreateInMemoryContext();
            return new EfUnitOfWork(context);
        }

        /// <summary>
        /// Creates a new IUnitOfWork backed by a shared in-memory database context.
        /// </summary>
        public static IUnitOfWork CreateInMemoryUnitOfWork(RewardPointsDbContext context)
        {
            return new EfUnitOfWork(context);
        }
    }
}
