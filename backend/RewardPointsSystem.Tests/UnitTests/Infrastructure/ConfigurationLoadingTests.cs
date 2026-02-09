using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace RewardPointsSystem.Tests.UnitTests.Infrastructure
{
    /// <summary>
    /// Test Case 4: The application correctly loads the database connection string from appsettings.json.
    /// </summary>
    public class ConfigurationLoadingTests
    {
        [Fact]
        public void Configuration_ShouldLoadConnectionStringFromInMemorySettings()
        {
            // Arrange
            var expectedConnectionString = "Server=TestServer;Database=TestDB;Integrated Security=true";
            var inMemorySettings = new Dictionary<string, string>
            {
                {"ConnectionStrings:DefaultConnection", expectedConnectionString}
            };

            // Act
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings!)
                .Build();

            var actualConnectionString = configuration.GetConnectionString("DefaultConnection");

            // Assert
            actualConnectionString.Should().NotBeNull();
            actualConnectionString.Should().Be(expectedConnectionString);
        }

        [Fact]
        public void Configuration_ShouldReturnNullForMissingConnectionString()
        {
            // Arrange
            var configuration = new ConfigurationBuilder().Build();

            // Act
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            // Assert
            connectionString.Should().BeNull();
        }

        [Fact]
        public void Configuration_ShouldLoadMultipleConnectionStrings()
        {
            // Arrange
            var inMemorySettings = new Dictionary<string, string>
            {
                {"ConnectionStrings:DefaultConnection", "Server=DefaultServer;Database=DefaultDB"},
                {"ConnectionStrings:SecondaryConnection", "Server=SecondaryServer;Database=SecondaryDB"}
            };

            // Act
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings!)
                .Build();

            var defaultConnection = configuration.GetConnectionString("DefaultConnection");
            var secondaryConnection = configuration.GetConnectionString("SecondaryConnection");

            // Assert
            defaultConnection.Should().Be("Server=DefaultServer;Database=DefaultDB");
            secondaryConnection.Should().Be("Server=SecondaryServer;Database=SecondaryDB");
        }

        [Fact]
        public void Configuration_ShouldHandleComplexConnectionString()
        {
            // Arrange
            var complexConnectionString = "Server=LAPTOP-TJP69TAG\\\\SQLEXPRESS;Database=RewardPointsDB;Integrated Security=true;TrustServerCertificate=True;MultipleActiveResultSets=true";
            var inMemorySettings = new Dictionary<string, string>
            {
                {"ConnectionStrings:DefaultConnection", complexConnectionString}
            };

            // Act
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings!)
                .Build();

            var actualConnectionString = configuration.GetConnectionString("DefaultConnection");

            // Assert
            actualConnectionString.Should().NotBeNull();
            actualConnectionString.Should().Be(complexConnectionString);
        }

        [Fact]
        public void Configuration_ShouldAccessConnectionStringViaIndexer()
        {
            // Arrange
            var expectedConnectionString = "Server=TestServer;Database=TestDB";
            var inMemorySettings = new Dictionary<string, string>
            {
                {"ConnectionStrings:DefaultConnection", expectedConnectionString}
            };

            // Act
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings!)
                .Build();

            var actualConnectionString = configuration["ConnectionStrings:DefaultConnection"];

            // Assert
            actualConnectionString.Should().Be(expectedConnectionString);
        }

        [Fact]
        public void Configuration_ShouldLoadFromJsonFile()
        {
            // Arrange - Create a temporary appsettings.json file
            var testJsonContent = @"{
                ""ConnectionStrings"": {
                    ""DefaultConnection"": ""Server=JsonTestServer;Database=JsonTestDB;Integrated Security=true""
                }
            }";

            var tempFilePath = Path.Combine(Path.GetTempPath(), $"appsettings.test.{Guid.NewGuid()}.json");
            File.WriteAllText(tempFilePath, testJsonContent);

            try
            {
                // Act
                var configuration = new ConfigurationBuilder()
                    .AddJsonFile(tempFilePath, optional: false, reloadOnChange: false)
                    .Build();

                var connectionString = configuration.GetConnectionString("DefaultConnection");

                // Assert
                connectionString.Should().NotBeNull();
                connectionString.Should().Be("Server=JsonTestServer;Database=JsonTestDB;Integrated Security=true");
            }
            finally
            {
                // Cleanup
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
            }
        }

        [Fact]
        public void Configuration_ShouldHandleOptionalJsonFile()
        {
            // Arrange - Use a non-existent file path
            var nonExistentPath = Path.Combine(Path.GetTempPath(), $"nonexistent.{Guid.NewGuid()}.json");

            // Act
            var act = () => new ConfigurationBuilder()
                .AddJsonFile(nonExistentPath, optional: true, reloadOnChange: false)
                .Build();

            // Assert
            act.Should().NotThrow();
        }

        [Fact]
        public void Configuration_ShouldOverrideWithEnvironmentVariables()
        {
            // Arrange
            var inMemorySettings = new Dictionary<string, string>
            {
                {"ConnectionStrings:DefaultConnection", "Server=OriginalServer;Database=OriginalDB"}
            };

            var environmentOverrides = new Dictionary<string, string>
            {
                {"ConnectionStrings:DefaultConnection", "Server=OverriddenServer;Database=OverriddenDB"}
            };

            // Act
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings!)
                .AddInMemoryCollection(environmentOverrides!) // Simulates environment variable override
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            // Assert
            connectionString.Should().Be("Server=OverriddenServer;Database=OverriddenDB");
        }

        [Fact]
        public void Configuration_ShouldAccessNestedConfigurationValues()
        {
            // Arrange
            var inMemorySettings = new Dictionary<string, string>
            {
                {"Logging:LogLevel:Default", "Information"},
                {"Logging:LogLevel:Microsoft.EntityFrameworkCore", "Warning"}
            };

            // Act
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings!)
                .Build();

            var defaultLogLevel = configuration["Logging:LogLevel:Default"];
            var efLogLevel = configuration["Logging:LogLevel:Microsoft.EntityFrameworkCore"];

            // Assert
            defaultLogLevel.Should().Be("Information");
            efLogLevel.Should().Be("Warning");
        }

        [Fact]
        public void Configuration_ShouldGetSection()
        {
            // Arrange
            var inMemorySettings = new Dictionary<string, string>
            {
                {"ConnectionStrings:DefaultConnection", "Server=TestServer;Database=TestDB"},
                {"ConnectionStrings:SecondaryConnection", "Server=SecondaryServer;Database=SecondaryDB"}
            };

            // Act
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings!)
                .Build();

            var connectionStringsSection = configuration.GetSection("ConnectionStrings");
            var defaultConnection = connectionStringsSection["DefaultConnection"];

            // Assert
            connectionStringsSection.Should().NotBeNull();
            defaultConnection.Should().Be("Server=TestServer;Database=TestDB");
        }
    }
}
