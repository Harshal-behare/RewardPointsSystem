using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RewardPointsSystem.Domain.Entities.Core;
using RewardPointsSystem.Infrastructure.Data;
using Xunit;

namespace RewardPointsSystem.Tests.UnitTests.Infrastructure
{
    /// <summary>
    /// Unit tests for RewardPointsDbContext configuration
    /// </summary>
    public class RewardPointsDbContextTests
    {
        /// <summary>
        /// Test Case 1: RewardPointsDbContext is correctly configured with the SQL Server connection string.
        /// </summary>
        [Fact]
        public void DbContext_ShouldBeConfiguredWithSqlServer()
        {
            // Arrange
            var connectionString = "Server=TestServer;Database=TestDB;Integrated Security=true;TrustServerCertificate=True";
            var options = new DbContextOptionsBuilder<RewardPointsDbContext>()
                .UseSqlServer(connectionString)
                .Options;

            // Act
            using var context = new RewardPointsDbContext(options);

            // Assert
            context.Should().NotBeNull();
            context.Database.Should().NotBeNull();
            context.Database.ProviderName.Should().Be("Microsoft.EntityFrameworkCore.SqlServer");
            
            // Verify connection string is configured
            var connection = context.Database.GetConnectionString();
            connection.Should().Be(connectionString);
        }

        /// <summary>
        /// Test that DbContext can be instantiated with valid options
        /// </summary>
        [Fact]
        public void DbContext_ShouldBeInstantiatedWithValidOptions()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<RewardPointsDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            // Act
            using var context = new RewardPointsDbContext(options);

            // Assert
            context.Should().NotBeNull();
            context.Database.Should().NotBeNull();
        }

        /// <summary>
        /// Test that all DbSet properties are properly initialized
        /// </summary>
        [Fact]
        public void DbContext_ShouldHaveAllDbSetsInitialized()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<RewardPointsDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDbSets")
                .Options;

            // Act
            using var context = new RewardPointsDbContext(options);

            // Assert
            context.Users.Should().NotBeNull();
            context.Roles.Should().NotBeNull();
            context.UserRoles.Should().NotBeNull();
            context.PointsAccounts.Should().NotBeNull();
            context.PointsTransactions.Should().NotBeNull();
            context.Events.Should().NotBeNull();
            context.EventParticipants.Should().NotBeNull();
            context.Products.Should().NotBeNull();
            context.ProductPricings.Should().NotBeNull();
            context.InventoryItems.Should().NotBeNull();
            context.Redemptions.Should().NotBeNull();
        }

        /// <summary>
        /// Test that the model is created correctly with entity configurations
        /// </summary>
        [Fact]
        public void DbContext_ShouldCreateModelWithEntityConfigurations()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<RewardPointsDbContext>()
                .UseInMemoryDatabase(databaseName: "TestModelCreation")
                .Options;

            // Act
            using var context = new RewardPointsDbContext(options);
            var model = context.Model;

            // Assert
            model.Should().NotBeNull();
            
            // Verify that key entities are configured
            var userEntityType = model.FindEntityType(typeof(User));
            userEntityType.Should().NotBeNull();
            
            var roleEntityType = model.FindEntityType(typeof(Role));
            roleEntityType.Should().NotBeNull();
        }

        /// <summary>
        /// Test that SQL Server specific configurations are applied
        /// </summary>
        [Fact]
        public void DbContext_ShouldApplySqlServerConfigurations()
        {
            // Arrange
            var connectionString = "Server=TestServer;Database=TestDB;Integrated Security=true";
            var options = new DbContextOptionsBuilder<RewardPointsDbContext>()
                .UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                })
                .Options;

            // Act
            using var context = new RewardPointsDbContext(options);

            // Assert
            context.Database.ProviderName.Should().Be("Microsoft.EntityFrameworkCore.SqlServer");
        }
    }
}
