using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RewardPointsSystem.Domain.Entities.Accounts;
using RewardPointsSystem.Domain.Entities.Core;
using RewardPointsSystem.Infrastructure.Data;
using Xunit;

namespace RewardPointsSystem.Tests.UnitTests.Infrastructure
{
    /// <summary>
    /// Test Case 5: Basic CRUD operations for an entity (e.g., User) function correctly via RewardPointsDbContext.
    /// </summary>
    public class CrudOperationsTests
    {
        private RewardPointsDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<RewardPointsDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new RewardPointsDbContext(options);
        }

        #region Create Tests

        [Fact]
        public void Create_ShouldAddUserToDatabase()
        {
            // Arrange
            using var context = CreateContext();
            var user = new User
            {
                Email = "newuser@example.com",
                FirstName = "John",
                LastName = "Doe"
            };

            // Act
            context.Users.Add(user);
            var result = context.SaveChanges();

            // Assert
            result.Should().BeGreaterThan(0);
            context.Users.Should().Contain(u => u.Email == "newuser@example.com");
        }

        [Fact]
        public void Create_ShouldAddMultipleUsers()
        {
            // Arrange
            using var context = CreateContext();
            var users = new[]
            {
                new User { Email = "user1@example.com", FirstName = "User", LastName = "One" },
                new User { Email = "user2@example.com", FirstName = "User", LastName = "Two" },
                new User { Email = "user3@example.com", FirstName = "User", LastName = "Three" }
            };

            // Act
            context.Users.AddRange(users);
            var result = context.SaveChanges();

            // Assert
            result.Should().Be(3);
            context.Users.Count().Should().Be(3);
        }

        [Fact]
        public async Task CreateAsync_ShouldAddUserAsynchronously()
        {
            // Arrange
            using var context = CreateContext();
            var user = new User
            {
                Email = "asyncuser@example.com",
                FirstName = "Async",
                LastName = "User"
            };

            // Act
            await context.Users.AddAsync(user);
            var result = await context.SaveChangesAsync();

            // Assert
            result.Should().BeGreaterThan(0);
            context.Users.Should().Contain(u => u.Email == "asyncuser@example.com");
        }

        #endregion

        #region Read Tests

        [Fact]
        public void Read_ShouldRetrieveUserById()
        {
            // Arrange
            using var context = CreateContext();
            var user = new User
            {
                Email = "read@example.com",
                FirstName = "Read",
                LastName = "User"
            };
            context.Users.Add(user);
            context.SaveChanges();

            // Act
            var retrievedUser = context.Users.Find(user.Id);

            // Assert
            retrievedUser.Should().NotBeNull();
            retrievedUser!.Email.Should().Be("read@example.com");
            retrievedUser.FirstName.Should().Be("Read");
            retrievedUser.LastName.Should().Be("User");
        }

        [Fact]
        public void Read_ShouldRetrieveUserByEmail()
        {
            // Arrange
            using var context = CreateContext();
            var user = new User
            {
                Email = "findbyme@example.com",
                FirstName = "Find",
                LastName = "Me"
            };
            context.Users.Add(user);
            context.SaveChanges();

            // Act
            var retrievedUser = context.Users.FirstOrDefault(u => u.Email == "findbyme@example.com");

            // Assert
            retrievedUser.Should().NotBeNull();
            retrievedUser!.FirstName.Should().Be("Find");
        }

        [Fact]
        public void Read_ShouldRetrieveAllUsers()
        {
            // Arrange
            using var context = CreateContext();
            var users = new[]
            {
                new User { Email = "user1@example.com", FirstName = "User", LastName = "One" },
                new User { Email = "user2@example.com", FirstName = "User", LastName = "Two" }
            };
            context.Users.AddRange(users);
            context.SaveChanges();

            // Act
            var allUsers = context.Users.ToList();

            // Assert
            allUsers.Should().HaveCount(2);
            allUsers.Should().Contain(u => u.Email == "user1@example.com");
            allUsers.Should().Contain(u => u.Email == "user2@example.com");
        }

        [Fact]
        public async Task ReadAsync_ShouldRetrieveUserAsynchronously()
        {
            // Arrange
            using var context = CreateContext();
            var user = new User
            {
                Email = "asyncread@example.com",
                FirstName = "Async",
                LastName = "Read"
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Act
            var retrievedUser = await context.Users.FindAsync(user.Id);

            // Assert
            retrievedUser.Should().NotBeNull();
            retrievedUser!.Email.Should().Be("asyncread@example.com");
        }

        [Fact]
        public void Read_ShouldReturnNullForNonExistentUser()
        {
            // Arrange
            using var context = CreateContext();
            var nonExistentId = Guid.NewGuid();

            // Act
            var user = context.Users.Find(nonExistentId);

            // Assert
            user.Should().BeNull();
        }

        [Fact]
        public void Read_ShouldQueryWithLinq()
        {
            // Arrange
            using var context = CreateContext();
            var users = new[]
            {
                new User { Email = "active1@example.com", FirstName = "Active", LastName = "One", IsActive = true },
                new User { Email = "active2@example.com", FirstName = "Active", LastName = "Two", IsActive = true },
                new User { Email = "inactive@example.com", FirstName = "Inactive", LastName = "User", IsActive = false }
            };
            context.Users.AddRange(users);
            context.SaveChanges();

            // Act
            var activeUsers = context.Users.Where(u => u.IsActive).ToList();

            // Assert
            activeUsers.Should().HaveCount(2);
            activeUsers.Should().AllSatisfy(u => u.IsActive.Should().BeTrue());
        }

        #endregion

        #region Update Tests

        [Fact]
        public void Update_ShouldModifyUserProperties()
        {
            // Arrange
            using var context = CreateContext();
            var user = new User
            {
                Email = "update@example.com",
                FirstName = "Original",
                LastName = "Name"
            };
            context.Users.Add(user);
            context.SaveChanges();

            // Act
            user.FirstName = "Updated";
            user.LastName = "NewName";
            user.UpdatedAt = DateTime.UtcNow;
            context.Users.Update(user);
            var result = context.SaveChanges();

            // Assert
            result.Should().BeGreaterThan(0);
            var updatedUser = context.Users.Find(user.Id);
            updatedUser!.FirstName.Should().Be("Updated");
            updatedUser.LastName.Should().Be("NewName");
            updatedUser.UpdatedAt.Should().NotBeNull();
        }

        [Fact]
        public void Update_ShouldModifyUserWithoutExplicitUpdate()
        {
            // Arrange
            using var context = CreateContext();
            var user = new User
            {
                Email = "trackingupdate@example.com",
                FirstName = "Original",
                LastName = "Name"
            };
            context.Users.Add(user);
            context.SaveChanges();

            // Act - EF tracks changes automatically
            user.FirstName = "Modified";
            var result = context.SaveChanges();

            // Assert
            result.Should().BeGreaterThan(0);
            var updatedUser = context.Users.Find(user.Id);
            updatedUser!.FirstName.Should().Be("Modified");
        }

        [Fact]
        public void Update_ShouldToggleUserActiveStatus()
        {
            // Arrange
            using var context = CreateContext();
            var user = new User
            {
                Email = "active@example.com",
                FirstName = "Active",
                LastName = "User",
                IsActive = true
            };
            context.Users.Add(user);
            context.SaveChanges();

            // Act
            user.IsActive = false;
            context.SaveChanges();

            // Assert
            var updatedUser = context.Users.Find(user.Id);
            updatedUser!.IsActive.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateAsync_ShouldModifyUserAsynchronously()
        {
            // Arrange
            using var context = CreateContext();
            var user = new User
            {
                Email = "asyncupdate@example.com",
                FirstName = "Original",
                LastName = "Name"
            };
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            // Act
            user.FirstName = "AsyncUpdated";
            var result = await context.SaveChangesAsync();

            // Assert
            result.Should().BeGreaterThan(0);
            var updatedUser = await context.Users.FindAsync(user.Id);
            updatedUser!.FirstName.Should().Be("AsyncUpdated");
        }

        #endregion

        #region Delete Tests

        [Fact]
        public void Delete_ShouldRemoveUserFromDatabase()
        {
            // Arrange
            using var context = CreateContext();
            var user = new User
            {
                Email = "delete@example.com",
                FirstName = "Delete",
                LastName = "Me"
            };
            context.Users.Add(user);
            context.SaveChanges();
            var userId = user.Id;

            // Act
            context.Users.Remove(user);
            var result = context.SaveChanges();

            // Assert
            result.Should().BeGreaterThan(0);
            var deletedUser = context.Users.Find(userId);
            deletedUser.Should().BeNull();
        }

        [Fact]
        public void Delete_ShouldRemoveMultipleUsers()
        {
            // Arrange
            using var context = CreateContext();
            var users = new[]
            {
                new User { Email = "delete1@example.com", FirstName = "Delete", LastName = "One" },
                new User { Email = "delete2@example.com", FirstName = "Delete", LastName = "Two" }
            };
            context.Users.AddRange(users);
            context.SaveChanges();

            // Act
            context.Users.RemoveRange(users);
            var result = context.SaveChanges();

            // Assert
            result.Should().Be(2);
            context.Users.Should().BeEmpty();
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveUserAsynchronously()
        {
            // Arrange
            using var context = CreateContext();
            var user = new User
            {
                Email = "asyncdelete@example.com",
                FirstName = "Async",
                LastName = "Delete"
            };
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();
            var userId = user.Id;

            // Act
            context.Users.Remove(user);
            var result = await context.SaveChangesAsync();

            // Assert
            result.Should().BeGreaterThan(0);
            var deletedUser = await context.Users.FindAsync(userId);
            deletedUser.Should().BeNull();
        }

        #endregion

        #region Complex CRUD Operations

        [Fact]
        public void ComplexOperation_ShouldCreateUserWithPointsAccount()
        {
            // Arrange
            using var context = CreateContext();
            var user = new User
            {
                Email = "withaccount@example.com",
                FirstName = "With",
                LastName = "Account"
            };

            var pointsAccount = new PointsAccount
            {
                UserId = user.Id,
                CurrentBalance = 100,
                TotalEarned = 150,
                TotalRedeemed = 50,
                User = user
            };

            user.PointsAccount = pointsAccount;

            // Act
            context.Users.Add(user);
            var result = context.SaveChanges();

            // Assert
            result.Should().BeGreaterThan(0);
            var savedUser = context.Users
                .Include(u => u.PointsAccount)
                .FirstOrDefault(u => u.Id == user.Id);

            savedUser.Should().NotBeNull();
            savedUser!.PointsAccount.Should().NotBeNull();
            savedUser.PointsAccount.CurrentBalance.Should().Be(100);
        }

        [Fact]
        public void ComplexOperation_ShouldDeleteUserAndCascadePointsAccount()
        {
            // Arrange
            using var context = CreateContext();
            var user = new User
            {
                Email = "cascade@example.com",
                FirstName = "Cascade",
                LastName = "Delete"
            };

            var pointsAccount = new PointsAccount
            {
                UserId = user.Id,
                User = user
            };

            user.PointsAccount = pointsAccount;

            context.Users.Add(user);
            context.SaveChanges();

            var userId = user.Id;
            var accountId = pointsAccount.Id;

            // Act
            context.Users.Remove(user);
            context.SaveChanges();

            // Assert
            context.Users.Find(userId).Should().BeNull();
            context.PointsAccounts.Find(accountId).Should().BeNull();
        }

        [Fact]
        public void ComplexOperation_ShouldSaveMultipleEntitiesInOneTransaction()
        {
            // Arrange
            using var context = CreateContext();

            // Act
            var user1 = new User
            {
                Email = "batch1@example.com",
                FirstName = "Batch",
                LastName = "One"
            };

            var user2 = new User
            {
                Email = "batch2@example.com",
                FirstName = "Batch",
                LastName = "Two"
            };

            context.Users.Add(user1);
            context.Users.Add(user2);
            var result = context.SaveChanges();

            // Assert
            result.Should().Be(2);
            context.Users.Should().HaveCount(2);
        }

        #endregion
    }
}
