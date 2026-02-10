using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RewardPointsSystem.Domain.Entities.Accounts;
using RewardPointsSystem.Domain.Entities.Core;
using RewardPointsSystem.Domain.Entities.Products;
using RewardPointsSystem.Domain.Entities.Events;
using RewardPointsSystem.Infrastructure.Data;
using Xunit;

namespace RewardPointsSystem.Tests.UnitTests.Infrastructure
{
    /// <summary>
    /// Tests for Entity Framework CRUD operations
    /// 
    /// These tests verify that basic database operations work correctly
    /// using the RewardPointsDbContext with an in-memory database.
    /// 
    /// NOTE: Domain entities use factory methods (e.g., User.Create()) instead of
    /// parameterless constructors. This is by design to enforce proper initialization.
    /// 
    /// Key scenarios tested:
    /// - Creating entities using factory methods
    /// - Reading entities by ID and with queries
    /// - Updating entities through domain methods
    /// - Deleting entities
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

        /// <summary>
        /// SCENARIO: Add a new user to the database
        /// EXPECTED: User is persisted with all properties
        /// WHY: Basic create operation validation
        /// </summary>
        [Fact]
        public void Create_ShouldAddUserToDatabase()
        {
            // Arrange - Create user using factory method
            using var context = CreateContext();
            var user = User.Create("newuser@example.com", "John", "Doe");

            // Act - Add to database
            context.Users.Add(user);
            var result = context.SaveChanges();

            // Assert - Verify persistence
            result.Should().BeGreaterThan(0, "changes should be saved");
            context.Users.Should().Contain(u => u.Email == "newuser@example.com");
        }

        /// <summary>
        /// SCENARIO: Add multiple users in a batch
        /// EXPECTED: All users are persisted
        /// WHY: Batch operations should work efficiently
        /// </summary>
        [Fact]
        public void Create_ShouldAddMultipleUsers()
        {
            // Arrange - Create multiple users using factory method
            using var context = CreateContext();
            var users = new[]
            {
                User.Create("user1@example.com", "User", "One"),
                User.Create("user2@example.com", "User", "Two"),
                User.Create("user3@example.com", "User", "Three")
            };

            // Act - Add all to database
            context.Users.AddRange(users);
            var result = context.SaveChanges();

            // Assert - Verify all saved
            result.Should().Be(3, "three users should be saved");
            context.Users.Count().Should().Be(3);
        }

        /// <summary>
        /// SCENARIO: Add a user asynchronously
        /// EXPECTED: User is persisted
        /// WHY: Async operations should work correctly
        /// </summary>
        [Fact]
        public async Task CreateAsync_ShouldAddUserAsynchronously()
        {
            // Arrange - Create user using factory method
            using var context = CreateContext();
            var user = User.Create("asyncuser@example.com", "Async", "User");

            // Act - Add asynchronously
            await context.Users.AddAsync(user);
            var result = await context.SaveChangesAsync();

            // Assert - Verify persistence
            result.Should().BeGreaterThan(0, "changes should be saved");
            context.Users.Should().Contain(u => u.Email == "asyncuser@example.com");
        }

        /// <summary>
        /// SCENARIO: Add a product to the database
        /// EXPECTED: Product is persisted with all properties
        /// WHY: Verify create works for different entity types
        /// </summary>
        [Fact]
        public async Task Create_ShouldAddProductToDatabase()
        {
            // Arrange - Create a user first (for CreatedBy)
            using var context = CreateContext();
            var user = User.Create("admin@example.com", "Admin", "User");
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            // Create product using factory method
            var product = Product.Create("Laptop", user.Id, "High-end laptop");

            // Act - Add product to database
            await context.Products.AddAsync(product);
            var result = await context.SaveChangesAsync();

            // Assert - Verify persistence
            result.Should().BeGreaterThan(0, "changes should be saved");
            context.Products.Should().Contain(p => p.Name == "Laptop");
        }

        #endregion

        #region Read Tests

        /// <summary>
        /// SCENARIO: Retrieve a user by ID
        /// EXPECTED: User is found with correct details
        /// WHY: Basic read operation validation
        /// </summary>
        [Fact]
        public void Read_ShouldRetrieveUserById()
        {
            // Arrange - Create and save a user
            using var context = CreateContext();
            var user = User.Create("read@example.com", "Read", "User");
            context.Users.Add(user);
            context.SaveChanges();

            // Act - Retrieve by ID
            var retrievedUser = context.Users.Find(user.Id);

            // Assert - Verify details
            retrievedUser.Should().NotBeNull("user should be found");
            retrievedUser!.Email.Should().Be("read@example.com");
            retrievedUser.FirstName.Should().Be("Read");
            retrievedUser.LastName.Should().Be("User");
        }

        /// <summary>
        /// SCENARIO: Find a user by email using LINQ
        /// EXPECTED: User is found
        /// WHY: Query by non-key field should work
        /// </summary>
        [Fact]
        public void Read_ShouldRetrieveUserByEmail()
        {
            // Arrange - Create and save a user
            using var context = CreateContext();
            var user = User.Create("findbyme@example.com", "Find", "Me");
            context.Users.Add(user);
            context.SaveChanges();

            // Act - Query by email
            var retrievedUser = context.Users.FirstOrDefault(u => u.Email == "findbyme@example.com");

            // Assert - Verify found
            retrievedUser.Should().NotBeNull("user should be found by email");
            retrievedUser!.FirstName.Should().Be("Find");
        }

        /// <summary>
        /// SCENARIO: Retrieve all users
        /// EXPECTED: All users are returned
        /// WHY: GetAll operation should work
        /// </summary>
        [Fact]
        public void Read_ShouldRetrieveAllUsers()
        {
            // Arrange - Create and save multiple users
            using var context = CreateContext();
            var users = new[]
            {
                User.Create("user1@example.com", "User", "One"),
                User.Create("user2@example.com", "User", "Two")
            };
            context.Users.AddRange(users);
            context.SaveChanges();

            // Act - Retrieve all
            var allUsers = context.Users.ToList();

            // Assert - Verify count
            allUsers.Should().HaveCount(2);
            allUsers.Should().Contain(u => u.Email == "user1@example.com");
            allUsers.Should().Contain(u => u.Email == "user2@example.com");
        }

        /// <summary>
        /// SCENARIO: Retrieve a user asynchronously
        /// EXPECTED: User is found
        /// WHY: Async operations should work correctly
        /// </summary>
        [Fact]
        public async Task ReadAsync_ShouldRetrieveUserAsynchronously()
        {
            // Arrange - Create and save a user
            using var context = CreateContext();
            var user = User.Create("asyncread@example.com", "Async", "Read");
            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Act - Retrieve asynchronously
            var retrievedUser = await context.Users.FindAsync(user.Id);

            // Assert - Verify found
            retrievedUser.Should().NotBeNull("user should be found");
            retrievedUser!.Email.Should().Be("asyncread@example.com");
        }

        /// <summary>
        /// SCENARIO: Try to retrieve a non-existent user
        /// EXPECTED: Returns null
        /// WHY: Should handle missing records gracefully
        /// </summary>
        [Fact]
        public void Read_ShouldReturnNullForNonExistentUser()
        {
            // Arrange
            using var context = CreateContext();
            var nonExistentId = Guid.NewGuid();

            // Act - Try to find
            var user = context.Users.Find(nonExistentId);

            // Assert - Should be null
            user.Should().BeNull("non-existent user should return null");
        }

        /// <summary>
        /// SCENARIO: Query users using LINQ expressions
        /// EXPECTED: Correct users are returned
        /// WHY: Complex queries should work
        /// </summary>
        [Fact]
        public void Read_ShouldQueryWithLinq()
        {
            // Arrange - Create users with different first names
            using var context = CreateContext();
            var users = new[]
            {
                User.Create("john1@example.com", "John", "Doe"),
                User.Create("john2@example.com", "John", "Smith"),
                User.Create("jane@example.com", "Jane", "Doe")
            };
            context.Users.AddRange(users);
            context.SaveChanges();

            // Act - Query for Johns
            var johns = context.Users.Where(u => u.FirstName == "John").ToList();

            // Assert - Should find 2 Johns
            johns.Should().HaveCount(2);
            johns.Should().OnlyContain(u => u.FirstName == "John");
        }

        /// <summary>
        /// SCENARIO: Filter users by active status
        /// EXPECTED: Only matching users are returned
        /// WHY: Status filtering is common operation
        /// </summary>
        [Fact]
        public void Read_ShouldFilterByActiveStatus()
        {
            // Arrange - Create users (all start as active by default)
            using var context = CreateContext();
            var user1 = User.Create("active1@example.com", "Active", "One");
            var user2 = User.Create("active2@example.com", "Active", "Two");
            var user3 = User.Create("inactive@example.com", "Inactive", "User");
            
            // Deactivate user3
            user3.Deactivate(user1.Id);
            
            context.Users.AddRange(new[] { user1, user2, user3 });
            context.SaveChanges();

            // Act - Query active users
            var activeUsers = context.Users.Where(u => u.IsActive).ToList();

            // Assert - Should find 2 active users
            activeUsers.Should().HaveCount(2);
            activeUsers.Should().AllSatisfy(u => u.IsActive.Should().BeTrue());
        }

        #endregion

        #region Update Tests

        /// <summary>
        /// SCENARIO: Update a user's information
        /// EXPECTED: User is updated with new values
        /// WHY: Basic update operation validation
        /// </summary>
        [Fact]
        public void Update_ShouldModifyUserProperties()
        {
            // Arrange - Create and save a user
            using var context = CreateContext();
            var user = User.Create("update@example.com", "Original", "Name");
            context.Users.Add(user);
            context.SaveChanges();

            // Act - Update through domain method
            user.UpdateInfo("updated@example.com", "Updated", "NewName", user.Id);
            context.Users.Update(user);
            var result = context.SaveChanges();

            // Assert - Verify updates
            result.Should().BeGreaterThan(0, "changes should be saved");
            var updatedUser = context.Users.Find(user.Id);
            updatedUser!.FirstName.Should().Be("Updated");
            updatedUser.LastName.Should().Be("NewName");
            updatedUser.UpdatedAt.Should().NotBeNull("update timestamp should be set");
        }

        /// <summary>
        /// SCENARIO: Deactivate a user
        /// EXPECTED: User's IsActive is set to false
        /// WHY: Soft delete is common pattern
        /// </summary>
        [Fact]
        public void Update_ShouldToggleUserActiveStatus()
        {
            // Arrange - Create an active user
            using var context = CreateContext();
            var user = User.Create("active@example.com", "Active", "User");
            context.Users.Add(user);
            context.SaveChanges();
            
            user.IsActive.Should().BeTrue("user starts active");

            // Act - Deactivate
            user.Deactivate(user.Id);
            context.SaveChanges();

            // Assert - Verify deactivated
            var updatedUser = context.Users.Find(user.Id);
            updatedUser!.IsActive.Should().BeFalse("user should be deactivated");
        }

        /// <summary>
        /// SCENARIO: Update user asynchronously
        /// EXPECTED: User is updated
        /// WHY: Async operations should work
        /// </summary>
        [Fact]
        public async Task UpdateAsync_ShouldModifyUserAsynchronously()
        {
            // Arrange - Create and save a user
            using var context = CreateContext();
            var user = User.Create("asyncupdate@example.com", "Original", "Name");
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            // Act - Update through domain method
            user.UpdateInfo("asyncupdate@example.com", "AsyncUpdated", "Name", user.Id);
            var result = await context.SaveChangesAsync();

            // Assert - Verify updates
            result.Should().BeGreaterThan(0, "changes should be saved");
            var updatedUser = await context.Users.FindAsync(user.Id);
            updatedUser!.FirstName.Should().Be("AsyncUpdated");
        }

        #endregion

        #region Delete Tests

        /// <summary>
        /// SCENARIO: Remove a user from the database
        /// EXPECTED: User is deleted
        /// WHY: Basic delete operation validation
        /// </summary>
        [Fact]
        public void Delete_ShouldRemoveUserFromDatabase()
        {
            // Arrange - Create and save a user
            using var context = CreateContext();
            var user = User.Create("delete@example.com", "Delete", "Me");
            context.Users.Add(user);
            context.SaveChanges();
            var userId = user.Id;

            // Act - Remove
            context.Users.Remove(user);
            var result = context.SaveChanges();

            // Assert - Verify deleted
            result.Should().BeGreaterThan(0, "changes should be saved");
            var deletedUser = context.Users.Find(userId);
            deletedUser.Should().BeNull("user should be deleted");
        }

        /// <summary>
        /// SCENARIO: Remove multiple users
        /// EXPECTED: All users are deleted
        /// WHY: Batch delete should work
        /// </summary>
        [Fact]
        public void Delete_ShouldRemoveMultipleUsers()
        {
            // Arrange - Create and save multiple users
            using var context = CreateContext();
            var users = new[]
            {
                User.Create("delete1@example.com", "Delete", "One"),
                User.Create("delete2@example.com", "Delete", "Two")
            };
            context.Users.AddRange(users);
            context.SaveChanges();

            // Act - Remove all
            context.Users.RemoveRange(users);
            var result = context.SaveChanges();

            // Assert - Verify all deleted
            result.Should().Be(2, "two users should be deleted");
            context.Users.Should().BeEmpty("all users should be deleted");
        }

        /// <summary>
        /// SCENARIO: Delete user asynchronously
        /// EXPECTED: User is deleted
        /// WHY: Async operations should work
        /// </summary>
        [Fact]
        public async Task DeleteAsync_ShouldRemoveUserAsynchronously()
        {
            // Arrange - Create and save a user
            using var context = CreateContext();
            var user = User.Create("asyncdelete@example.com", "Async", "Delete");
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();
            var userId = user.Id;

            // Act - Remove asynchronously
            context.Users.Remove(user);
            var result = await context.SaveChangesAsync();

            // Assert - Verify deleted
            result.Should().BeGreaterThan(0, "changes should be saved");
            var deletedUser = await context.Users.FindAsync(userId);
            deletedUser.Should().BeNull("user should be deleted");
        }

        #endregion

        #region Event CRUD Tests

        /// <summary>
        /// SCENARIO: Create and read an event
        /// EXPECTED: Event is persisted and can be retrieved
        /// WHY: Verify Event entity CRUD
        /// </summary>
        [Fact]
        public async Task Event_ShouldSupportCrudOperations()
        {
            // Arrange - Create a user first (for CreatedBy)
            using var context = CreateContext();
            var user = User.Create("eventcreator@example.com", "Event", "Creator");
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            // Create event using factory method
            var eventEntity = Event.Create(
                "Test Event",
                DateTime.UtcNow.AddDays(7),
                1000,
                user.Id,
                "Test Description");

            // Act - Add event
            await context.Events.AddAsync(eventEntity);
            var saveResult = await context.SaveChangesAsync();

            // Assert - Verify persistence
            saveResult.Should().BeGreaterThan(0);
            
            var retrievedEvent = await context.Events.FindAsync(eventEntity.Id);
            retrievedEvent.Should().NotBeNull();
            retrievedEvent!.Name.Should().Be("Test Event");
            retrievedEvent.TotalPointsPool.Should().Be(1000);
            retrievedEvent.Status.Should().Be(EventStatus.Draft);
        }

        #endregion

        #region Product CRUD Tests

        /// <summary>
        /// SCENARIO: Create and read a product with related inventory
        /// EXPECTED: Product and inventory are persisted
        /// WHY: Verify Product entity with relationships
        /// </summary>
        [Fact]
        public async Task Product_ShouldSupportCrudWithRelatedEntities()
        {
            // Arrange - Create a user first
            using var context = CreateContext();
            var user = User.Create("productcreator@example.com", "Product", "Creator");
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            // Create product
            var product = Product.Create(
                "Premium Headphones",
                user.Id,
                "High-quality wireless headphones");

            // Create inventory for product
            var inventory = InventoryItem.Create(product.Id, 50, 10);

            // Act - Add product and inventory
            await context.Products.AddAsync(product);
            await context.InventoryItems.AddAsync(inventory);
            var saveResult = await context.SaveChangesAsync();

            // Assert - Verify persistence
            saveResult.Should().BeGreaterThan(0);
            
            var retrievedProduct = await context.Products.FindAsync(product.Id);
            retrievedProduct.Should().NotBeNull();
            retrievedProduct!.Name.Should().Be("Premium Headphones");
            retrievedProduct.IsActive.Should().BeTrue();

            var retrievedInventory = await context.InventoryItems.FirstOrDefaultAsync(i => i.ProductId == product.Id);
            retrievedInventory.Should().NotBeNull();
            retrievedInventory!.QuantityAvailable.Should().Be(50);
        }

        #endregion
    }
}
