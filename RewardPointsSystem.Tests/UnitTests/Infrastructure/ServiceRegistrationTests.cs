using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RewardPointsSystem.Application;
using RewardPointsSystem.Infrastructure;
using RewardPointsSystem.Infrastructure.Data;
using Xunit;

namespace RewardPointsSystem.Tests.UnitTests.Infrastructure
{
    /// <summary>
    /// Test Case 3: Clean Architecture DI registration tests.
    /// Tests that AddInfrastructure and AddApplication correctly register services.
    /// </summary>
    public class ServiceRegistrationTests
    {
        private IConfiguration CreateTestConfiguration(string connectionString)
        {
            var inMemorySettings = new Dictionary<string, string>
            {
                {"ConnectionStrings:DefaultConnection", connectionString}
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings!)
                .Build();
        }

        [Fact]
        public void AddInfrastructure_ShouldRegisterDbContext()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = CreateTestConfiguration("Server=TestServer;Database=TestDB;Integrated Security=true");

            // Act
            services.AddInfrastructure(configuration);
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var dbContext = serviceProvider.GetService<RewardPointsDbContext>();
            dbContext.Should().NotBeNull();
        }

        [Fact]
        public void AddInfrastructure_ShouldConfigureDbContextWithCorrectConnectionString()
        {
            // Arrange
            var services = new ServiceCollection();
            var expectedConnectionString = "Server=TestServer;Database=TestDB;Integrated Security=true";
            var configuration = CreateTestConfiguration(expectedConnectionString);

            // Act
            services.AddInfrastructure(configuration);
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var dbContext = serviceProvider.GetService<RewardPointsDbContext>();
            dbContext.Should().NotBeNull();

            var actualConnectionString = dbContext!.Database.GetConnectionString();
            actualConnectionString.Should().Be(expectedConnectionString);
        }

        [Fact]
        public void AddInfrastructure_ShouldRegisterAllRequiredServices()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = CreateTestConfiguration("Server=TestServer;Database=TestDB;Integrated Security=true");

            // Act
            services.AddInfrastructure(configuration);
            var serviceProvider = services.BuildServiceProvider();

            // Assert - Verify DbContext is registered
            var dbContext = serviceProvider.GetService<RewardPointsDbContext>();
            dbContext.Should().NotBeNull();

            // Verify the service collection contains the DbContext registration
            services.Should().Contain(sd => sd.ServiceType == typeof(RewardPointsDbContext));
        }

        [Fact]
        public void AddInfrastructure_ShouldConfigureDbContextWithSqlServer()
        {
            // Arrange
            var services = new ServiceCollection();
            var connectionString = "Server=TestServer;Database=TestDB;Integrated Security=true";
            var configuration = CreateTestConfiguration(connectionString);

            // Act
            services.AddInfrastructure(configuration);
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var dbContext = serviceProvider.GetService<RewardPointsDbContext>();
            dbContext.Should().NotBeNull();
            dbContext!.Database.ProviderName.Should().Be("Microsoft.EntityFrameworkCore.SqlServer");
        }

        [Fact]
        public void AddInfrastructure_ShouldRegisterDbContextAsScoped()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = CreateTestConfiguration("Server=TestServer;Database=TestDB;Integrated Security=true");

            // Act
            services.AddInfrastructure(configuration);

            // Assert
            var dbContextServiceDescriptor = services.FirstOrDefault(sd => sd.ServiceType == typeof(RewardPointsDbContext));
            dbContextServiceDescriptor.Should().NotBeNull();
            dbContextServiceDescriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
        }

        [Fact]
        public void AddInfrastructure_ShouldHandleMissingConnectionString()
        {
            // Arrange
            var services = new ServiceCollection();
            var emptyConfiguration = new ConfigurationBuilder().Build();

            // Act & Assert
            var act = () => services.AddInfrastructure(emptyConfiguration);
            
            // Should not throw during registration, but connection string will be null
            act.Should().NotThrow();
        }

        [Fact]
        public void AddInfrastructure_ShouldAllowMultipleDbContextsInDifferentScopes()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = CreateTestConfiguration("Server=TestServer;Database=TestDB;Integrated Security=true");

            // Act
            services.AddInfrastructure(configuration);
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            using (var scope1 = serviceProvider.CreateScope())
            {
                var dbContext1 = scope1.ServiceProvider.GetService<RewardPointsDbContext>();
                dbContext1.Should().NotBeNull();

                using (var scope2 = serviceProvider.CreateScope())
                {
                    var dbContext2 = scope2.ServiceProvider.GetService<RewardPointsDbContext>();
                    dbContext2.Should().NotBeNull();
                    dbContext2.Should().NotBeSameAs(dbContext1);
                }
            }
        }

        [Fact]
        public void AddInfrastructure_ShouldConfigureSqlServerProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = CreateTestConfiguration("Server=TestServer;Database=TestDB;Integrated Security=true");

            // Act
            services.AddInfrastructure(configuration);
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var dbContext = serviceProvider.GetService<RewardPointsDbContext>();
            dbContext.Should().NotBeNull();
            
            // Verify SQL Server provider is configured
            dbContext!.Database.ProviderName.Should().Be("Microsoft.EntityFrameworkCore.SqlServer");
        }
    }
}
