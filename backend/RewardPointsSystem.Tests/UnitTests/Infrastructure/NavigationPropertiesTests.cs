using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RewardPointsSystem.Domain.Entities.Accounts;
using RewardPointsSystem.Domain.Entities.Core;
using RewardPointsSystem.Domain.Entities.Events;
using RewardPointsSystem.Domain.Entities.Operations;
using RewardPointsSystem.Domain.Entities.Products;
using RewardPointsSystem.Infrastructure.Data;
using Xunit;

namespace RewardPointsSystem.Tests.UnitTests.Infrastructure
{
    /// <summary>
    /// Tests for Entity Framework Navigation Properties
    /// 
    /// These tests verify that entity relationships are correctly mapped
    /// and can be accessed through EF Core navigation properties.
    /// 
    /// NOTE: Domain entities use factory methods and have private setters.
    /// Navigation properties are loaded through EF Core's Include() method.
    /// 
    /// Key relationships tested:
    /// - User ↔ UserPointsAccount (One-to-One)
    /// - User ↔ UserRoles (Many-to-Many through UserRole)
    /// - User ↔ EventParticipations (One-to-Many)
    /// - Event ↔ Participants (One-to-Many)
    /// - Product ↔ Inventory (One-to-One)
    /// - Product ↔ PricingHistory (One-to-Many)
    /// </summary>
    public class NavigationPropertiesTests
    {
        private RewardPointsDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<RewardPointsDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new RewardPointsDbContext(options);
        }

        #region User Navigation Property Tests

        /// <summary>
        /// SCENARIO: A user has a points account (one-to-one relationship)
        /// EXPECTED: Can navigate from User to UserPointsAccount
        /// WHY: Users need points accounts to earn and redeem points
        /// </summary>
        [Fact]
        public async Task User_ShouldNavigateToPointsAccount()
        {
            // Arrange - Create user and points account
            using var context = CreateContext();

            var user = User.Create("test@example.com", "John", "Doe");
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            var pointsAccount = UserPointsAccount.CreateForUser(user.Id);
            await context.UserPointsAccounts.AddAsync(pointsAccount);
            await context.SaveChangesAsync();

            // Act - Query user with points account included
            var savedUser = await context.Users
                .Include(u => u.UserPointsAccount)
                .FirstOrDefaultAsync(u => u.Id == user.Id);

            // Assert - Navigation should work
            savedUser.Should().NotBeNull("user should be found");
            savedUser!.UserPointsAccount.Should().NotBeNull("points account should be navigable");
            savedUser.UserPointsAccount!.UserId.Should().Be(user.Id, "account should be linked to user");
        }

        /// <summary>
        /// SCENARIO: A user has multiple role assignments
        /// EXPECTED: Can navigate from User to UserRoles
        /// WHY: Users can have multiple roles (admin, employee, etc.)
        /// </summary>
        [Fact]
        public async Task User_ShouldNavigateToRoles()
        {
            // Arrange - Create user and roles
            using var context = CreateContext();

            var user = User.Create("admin@example.com", "Admin", "User");
            await context.Users.AddAsync(user);

            var adminRole = Role.Create("Admin", "Administrator");
            var employeeRole = Role.Create("Employee", "Regular Employee");
            await context.Roles.AddRangeAsync(new[] { adminRole, employeeRole });
            await context.SaveChangesAsync();

            // Create role assignments
            var adminAssignment = UserRole.Assign(user.Id, adminRole.Id, user.Id);
            var employeeAssignment = UserRole.Assign(user.Id, employeeRole.Id, user.Id);
            await context.UserRoles.AddRangeAsync(new[] { adminAssignment, employeeAssignment });
            await context.SaveChangesAsync();

            // Act - Query user with roles included
            var savedUser = await context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == user.Id);

            // Assert - Navigation should work
            savedUser.Should().NotBeNull("user should be found");
            savedUser!.UserRoles.Should().HaveCount(2, "user should have 2 roles");
        }

        /// <summary>
        /// SCENARIO: A user participates in multiple events
        /// EXPECTED: Can navigate from User to EventParticipations
        /// WHY: Users can sign up for multiple events
        /// </summary>
        [Fact]
        public async Task User_ShouldNavigateToEventParticipations()
        {
            // Arrange - Create user and events
            using var context = CreateContext();

            var user = User.Create("participant@example.com", "Event", "Participant");
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            var event1 = Event.Create("Event 1", DateTime.UtcNow.AddDays(7), 500, user.Id, "Description 1");
            var event2 = Event.Create("Event 2", DateTime.UtcNow.AddDays(14), 1000, user.Id, "Description 2");
            await context.Events.AddRangeAsync(new[] { event1, event2 });
            await context.SaveChangesAsync();

            // Create participations
            var participation1 = EventParticipant.Register(event1.Id, user.Id);
            var participation2 = EventParticipant.Register(event2.Id, user.Id);
            await context.EventParticipants.AddRangeAsync(new[] { participation1, participation2 });
            await context.SaveChangesAsync();

            // Act - Query user with participations included
            var savedUser = await context.Users
                .Include(u => u.EventParticipations)
                    .ThenInclude(ep => ep.Event)
                .FirstOrDefaultAsync(u => u.Id == user.Id);

            // Assert - Navigation should work
            savedUser.Should().NotBeNull("user should be found");
            savedUser!.EventParticipations.Should().HaveCount(2, "user should have 2 participations");
        }

        #endregion

        #region Event Navigation Property Tests

        /// <summary>
        /// SCENARIO: An event has multiple participants
        /// EXPECTED: Can navigate from Event to Participants
        /// WHY: Events need to track who is participating
        /// </summary>
        [Fact]
        public async Task Event_ShouldNavigateToParticipants()
        {
            // Arrange - Create event creator and participants
            using var context = CreateContext();

            var creator = User.Create("creator@example.com", "Event", "Creator");
            var participant1 = User.Create("participant1@example.com", "First", "Participant");
            var participant2 = User.Create("participant2@example.com", "Second", "Participant");
            await context.Users.AddRangeAsync(new[] { creator, participant1, participant2 });
            await context.SaveChangesAsync();

            var eventEntity = Event.Create("Team Event", DateTime.UtcNow.AddDays(7), 1000, creator.Id, "Team building event");
            await context.Events.AddAsync(eventEntity);
            await context.SaveChangesAsync();

            // Create participations
            var p1 = EventParticipant.Register(eventEntity.Id, participant1.Id);
            var p2 = EventParticipant.Register(eventEntity.Id, participant2.Id);
            await context.EventParticipants.AddRangeAsync(new[] { p1, p2 });
            await context.SaveChangesAsync();

            // Act - Query event with participants included
            var savedEvent = await context.Events
                .Include(e => e.Participants)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(e => e.Id == eventEntity.Id);

            // Assert - Navigation should work
            savedEvent.Should().NotBeNull("event should be found");
            savedEvent!.Participants.Should().HaveCount(2, "event should have 2 participants");
        }

        /// <summary>
        /// SCENARIO: An event has a creator (user who created it)
        /// EXPECTED: Can navigate from Event to Creator
        /// WHY: Need to track who created each event
        /// </summary>
        [Fact]
        public async Task Event_ShouldNavigateToCreator()
        {
            // Arrange - Create event with creator
            using var context = CreateContext();

            var creator = User.Create("eventcreator@example.com", "Event", "Creator");
            await context.Users.AddAsync(creator);
            await context.SaveChangesAsync();

            var eventEntity = Event.Create("My Event", DateTime.UtcNow.AddDays(7), 500, creator.Id, "Event description");
            await context.Events.AddAsync(eventEntity);
            await context.SaveChangesAsync();

            // Act - Query event with creator included
            var savedEvent = await context.Events
                .Include(e => e.Creator)
                .FirstOrDefaultAsync(e => e.Id == eventEntity.Id);

            // Assert - Navigation should work
            savedEvent.Should().NotBeNull("event should be found");
            savedEvent!.Creator.Should().NotBeNull("creator should be navigable");
            savedEvent.Creator!.Email.Should().Be("eventcreator@example.com", "correct creator should be loaded");
        }

        #endregion

        #region Product Navigation Property Tests

        /// <summary>
        /// SCENARIO: A product has an inventory item (one-to-one)
        /// EXPECTED: Can navigate from Product to Inventory
        /// WHY: Products need inventory tracking
        /// </summary>
        [Fact]
        public async Task Product_ShouldNavigateToInventory()
        {
            // Arrange - Create product and inventory
            using var context = CreateContext();

            var creator = User.Create("productcreator@example.com", "Product", "Creator");
            await context.Users.AddAsync(creator);
            await context.SaveChangesAsync();

            var product = Product.Create("Laptop", creator.Id, "High-end laptop");
            await context.Products.AddAsync(product);
            await context.SaveChangesAsync();

            var inventory = InventoryItem.Create(product.Id, 50, 10);
            await context.InventoryItems.AddAsync(inventory);
            await context.SaveChangesAsync();

            // Act - Query product with inventory included
            var savedProduct = await context.Products
                .Include(p => p.Inventory)
                .FirstOrDefaultAsync(p => p.Id == product.Id);

            // Assert - Navigation should work
            savedProduct.Should().NotBeNull("product should be found");
            savedProduct!.Inventory.Should().NotBeNull("inventory should be navigable");
            savedProduct.Inventory!.QuantityAvailable.Should().Be(50, "correct inventory should be loaded");
        }

        /// <summary>
        /// SCENARIO: A product has pricing history (one-to-many)
        /// EXPECTED: Can navigate from Product to PricingHistory
        /// WHY: Products can have multiple price changes over time
        /// </summary>
        [Fact]
        public async Task Product_ShouldNavigateToPricingHistory()
        {
            // Arrange - Create product and pricing records
            using var context = CreateContext();

            var creator = User.Create("productcreator@example.com", "Product", "Creator");
            await context.Users.AddAsync(creator);
            await context.SaveChangesAsync();

            var product = Product.Create("Headphones", creator.Id, "Wireless headphones");
            await context.Products.AddAsync(product);
            await context.SaveChangesAsync();

            var pricing1 = ProductPricing.Create(product.Id, 100, DateTime.UtcNow.AddDays(-30));
            var pricing2 = ProductPricing.Create(product.Id, 120, DateTime.UtcNow);
            await context.ProductPricings.AddRangeAsync(new[] { pricing1, pricing2 });
            await context.SaveChangesAsync();

            // Act - Query product with pricing history included
            var savedProduct = await context.Products
                .Include(p => p.PricingHistory)
                .FirstOrDefaultAsync(p => p.Id == product.Id);

            // Assert - Navigation should work
            savedProduct.Should().NotBeNull("product should be found");
            savedProduct!.PricingHistory.Should().HaveCount(2, "product should have 2 pricing records");
        }

        /// <summary>
        /// SCENARIO: A product has redemptions (one-to-many)
        /// EXPECTED: Can navigate from Product to Redemptions
        /// WHY: Products track their redemption history
        /// </summary>
        [Fact]
        public async Task Product_ShouldNavigateToRedemptions()
        {
            // Arrange - Create product, user, and redemption
            using var context = CreateContext();

            var creator = User.Create("productcreator@example.com", "Product", "Creator");
            var customer = User.Create("customer@example.com", "Customer", "User");
            await context.Users.AddRangeAsync(new[] { creator, customer });
            await context.SaveChangesAsync();

            var product = Product.Create("Gift Card", creator.Id, "$50 Gift Card");
            await context.Products.AddAsync(product);
            await context.SaveChangesAsync();

            var redemption = Redemption.Create(customer.Id, product.Id, 500, 1);
            await context.Redemptions.AddAsync(redemption);
            await context.SaveChangesAsync();

            // Act - Query product with redemptions included
            var savedProduct = await context.Products
                .Include(p => p.Redemptions)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(p => p.Id == product.Id);

            // Assert - Navigation should work
            savedProduct.Should().NotBeNull("product should be found");
            savedProduct!.Redemptions.Should().HaveCount(1, "product should have 1 redemption");
            savedProduct.Redemptions.First().User.Should().NotBeNull("user should be navigable from redemption");
        }

        #endregion

        #region Redemption Navigation Property Tests

        /// <summary>
        /// SCENARIO: A redemption links to both user and product
        /// EXPECTED: Can navigate from Redemption to User and Product
        /// WHY: Redemptions connect users to products
        /// </summary>
        [Fact]
        public async Task Redemption_ShouldNavigateToUserAndProduct()
        {
            // Arrange - Create user, product, and redemption
            using var context = CreateContext();

            var creator = User.Create("creator@example.com", "Product", "Creator");
            var customer = User.Create("customer@example.com", "Customer", "User");
            await context.Users.AddRangeAsync(new[] { creator, customer });
            await context.SaveChangesAsync();

            var product = Product.Create("Reward Item", creator.Id, "Special reward");
            await context.Products.AddAsync(product);
            await context.SaveChangesAsync();

            var redemption = Redemption.Create(customer.Id, product.Id, 250, 1);
            await context.Redemptions.AddAsync(redemption);
            await context.SaveChangesAsync();

            // Act - Query redemption with user and product included
            var savedRedemption = await context.Redemptions
                .Include(r => r.User)
                .Include(r => r.Product)
                .FirstOrDefaultAsync(r => r.Id == redemption.Id);

            // Assert - Navigations should work
            savedRedemption.Should().NotBeNull("redemption should be found");
            savedRedemption!.User.Should().NotBeNull("user should be navigable");
            savedRedemption.User!.Email.Should().Be("customer@example.com", "correct user should be loaded");
            savedRedemption.Product.Should().NotBeNull("product should be navigable");
            savedRedemption.Product!.Name.Should().Be("Reward Item", "correct product should be loaded");
        }

        #endregion

        #region Transaction Navigation Property Tests

        /// <summary>
        /// SCENARIO: A user has multiple points transactions
        /// EXPECTED: Transactions are linked to the user
        /// WHY: Need to track all point changes for a user
        /// </summary>
        [Fact]
        public async Task UserPointsTransaction_ShouldNavigateToUser()
        {
            // Arrange - Create user and transactions
            using var context = CreateContext();

            var user = User.Create("transactionuser@example.com", "Transaction", "User");
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            var sourceId = Guid.NewGuid();
            var transaction1 = UserPointsTransaction.CreateEarned(
                user.Id, 100, TransactionOrigin.Event, 
                sourceId, 100, "Won event reward");
            var redemptionId = Guid.NewGuid();
            var transaction2 = UserPointsTransaction.CreateRedeemed(
                user.Id, 50, redemptionId, 50, "Redeemed points");
            await context.UserPointsTransactions.AddRangeAsync(new[] { transaction1, transaction2 });
            await context.SaveChangesAsync();

            // Act - Query transactions with user included
            var savedTransactions = await context.UserPointsTransactions
                .Include(t => t.User)
                .Where(t => t.UserId == user.Id)
                .ToListAsync();

            // Assert - Navigations should work
            savedTransactions.Should().HaveCount(2, "user should have 2 transactions");
            savedTransactions.Should().AllSatisfy(t => 
                t.User.Should().NotBeNull("user should be navigable"));
        }

        #endregion
    }
}
