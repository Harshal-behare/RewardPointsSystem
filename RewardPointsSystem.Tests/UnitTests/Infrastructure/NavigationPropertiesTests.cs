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
    /// Test Case 2: Navigation properties in domain entities are correctly mapped and can be accessed.
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

        [Fact]
        public void User_PointsAccount_OneToOne_ShouldBeCorrectlyMapped()
        {
            // Arrange
            using var context = CreateContext();

            var user = new User
            {
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe"
            };

            var pointsAccount = new PointsAccount
            {
                UserId = user.Id,
                User = user
            };

            user.PointsAccount = pointsAccount;

            // Act
            context.Users.Add(user);
            context.SaveChanges();

            // Assert
            var savedUser = context.Users
                .Include(u => u.PointsAccount)
                .FirstOrDefault(u => u.Id == user.Id);

            savedUser.Should().NotBeNull();
            savedUser!.PointsAccount.Should().NotBeNull();
            savedUser.PointsAccount.UserId.Should().Be(user.Id);
            savedUser.PointsAccount.User.Should().NotBeNull();
        }

        [Fact]
        public void User_UserRoles_OneToMany_ShouldBeCorrectlyMapped()
        {
            // Arrange
            using var context = CreateContext();

            var user = new User
            {
                Email = "test@example.com",
                FirstName = "Jane",
                LastName = "Doe"
            };

            var role = new Role
            {
                Name = "Admin",
                Description = "Administrator role"
            };

            var userRole = new UserRole
            {
                UserId = user.Id,
                RoleId = role.Id,
                User = user,
                Role = role,
                AssignedBy = Guid.NewGuid()
            };

            user.UserRoles.Add(userRole);
            role.UserRoles.Add(userRole);

            // Act
            context.Users.Add(user);
            context.Roles.Add(role);
            context.SaveChanges();

            // Assert
            var savedUser = context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefault(u => u.Id == user.Id);

            savedUser.Should().NotBeNull();
            savedUser!.UserRoles.Should().HaveCount(1);
            savedUser.UserRoles.First().Role.Should().NotBeNull();
            savedUser.UserRoles.First().Role.Name.Should().Be("Admin");
        }

        [Fact]
        public void User_EventParticipations_OneToMany_ShouldBeCorrectlyMapped()
        {
            // Arrange
            using var context = CreateContext();

            var user = new User
            {
                Email = "participant@example.com",
                FirstName = "Event",
                LastName = "Participant"
            };

            var eventEntity = new Event
            {
                Name = "Test Event",
                Description = "Test Description",
                EventDate = DateTime.UtcNow.AddDays(7),
                Status = EventStatus.Upcoming,
                TotalPointsPool = 1000,
                CreatedBy = user.Id
            };

            var eventParticipant = new EventParticipant
            {
                EventId = eventEntity.Id,
                UserId = user.Id,
                User = user,
                Event = eventEntity
            };

            user.EventParticipations.Add(eventParticipant);

            // Act
            context.Users.Add(user);
            context.Events.Add(eventEntity);
            context.SaveChanges();

            // Assert
            var savedUser = context.Users
                .Include(u => u.EventParticipations)
                    .ThenInclude(ep => ep.Event)
                .FirstOrDefault(u => u.Id == user.Id);

            savedUser.Should().NotBeNull();
            savedUser!.EventParticipations.Should().HaveCount(1);
            savedUser.EventParticipations.First().Event.Should().NotBeNull();
            savedUser.EventParticipations.First().Event.Name.Should().Be("Test Event");
        }

        [Fact]
        public void User_Redemptions_OneToMany_ShouldBeCorrectlyMapped()
        {
            // Arrange
            using var context = CreateContext();

            var user = new User
            {
                Email = "redeemer@example.com",
                FirstName = "Redeem",
                LastName = "User"
            };

            var product = new Product
            {
                Name = "Test Product",
                Description = "Test Description",
                Category = "Electronics",
                ImageUrl = "https://example.com/image.jpg",
                CreatedBy = user.Id
            };

            var redemption = new Redemption
            {
                UserId = user.Id,
                ProductId = product.Id,
                PointsSpent = 100,
                Status = RedemptionStatus.Pending,
                DeliveryNotes = "Test delivery notes",
                User = user,
                Product = product
            };

            user.Redemptions.Add(redemption);

            // Act
            context.Users.Add(user);
            context.Products.Add(product);
            context.SaveChanges();

            // Assert
            var savedUser = context.Users
                .Include(u => u.Redemptions)
                    .ThenInclude(r => r.Product)
                .FirstOrDefault(u => u.Id == user.Id);

            savedUser.Should().NotBeNull();
            savedUser!.Redemptions.Should().HaveCount(1);
            savedUser.Redemptions.First().Product.Should().NotBeNull();
            savedUser.Redemptions.First().Product.Name.Should().Be("Test Product");
        }

        [Fact]
        public void Product_InventoryItem_OneToOne_ShouldBeCorrectlyMapped()
        {
            // Arrange
            using var context = CreateContext();

            var creator = new User
            {
                Email = "creator@example.com",
                FirstName = "Product",
                LastName = "Creator"
            };

            var product = new Product
            {
                Name = "Product with Inventory",
                Description = "Test",
                Category = "Electronics",
                ImageUrl = "https://example.com/inventory.jpg",
                CreatedBy = creator.Id
            };

            var inventoryItem = new InventoryItem
            {
                ProductId = product.Id,
                QuantityAvailable = 50,
                QuantityReserved = 10,
                ReorderLevel = 20,
                Product = product
            };

            product.Inventory = inventoryItem;

            // Act
            context.Users.Add(creator);
            context.Products.Add(product);
            context.SaveChanges();

            // Assert
            var savedProduct = context.Products
                .Include(p => p.Inventory)
                .FirstOrDefault(p => p.Id == product.Id);

            savedProduct.Should().NotBeNull();
            savedProduct!.Inventory.Should().NotBeNull();
            savedProduct.Inventory.QuantityAvailable.Should().Be(50);
            savedProduct.Inventory.Product.Should().NotBeNull();
        }

        [Fact]
        public void Product_ProductPricing_OneToMany_ShouldBeCorrectlyMapped()
        {
            // Arrange
            using var context = CreateContext();

            var creator = new User
            {
                Email = "creator@example.com",
                FirstName = "Price",
                LastName = "Creator"
            };

            var product = new Product
            {
                Name = "Product with Pricing",
                Description = "Test",
                Category = "Electronics",
                ImageUrl = "https://example.com/pricing.jpg",
                CreatedBy = creator.Id
            };

            var pricing1 = new ProductPricing
            {
                ProductId = product.Id,
                PointsCost = 100,
                EffectiveFrom = DateTime.UtcNow.AddDays(-30),
                IsActive = false,
                Product = product
            };

            var pricing2 = new ProductPricing
            {
                ProductId = product.Id,
                PointsCost = 150,
                EffectiveFrom = DateTime.UtcNow,
                IsActive = true,
                Product = product
            };

            product.PricingHistory.Add(pricing1);
            product.PricingHistory.Add(pricing2);

            // Act
            context.Users.Add(creator);
            context.Products.Add(product);
            context.SaveChanges();

            // Assert
            var savedProduct = context.Products
                .Include(p => p.PricingHistory)
                .FirstOrDefault(p => p.Id == product.Id);

            savedProduct.Should().NotBeNull();
            savedProduct!.PricingHistory.Should().HaveCount(2);
            savedProduct.PricingHistory.Should().Contain(p => p.IsActive && p.PointsCost == 150);
        }

        [Fact]
        public void Event_EventParticipants_OneToMany_ShouldBeCorrectlyMapped()
        {
            // Arrange
            using var context = CreateContext();

            var creator = new User
            {
                Email = "creator@example.com",
                FirstName = "Event",
                LastName = "Creator"
            };

            var participant1 = new User
            {
                Email = "participant1@example.com",
                FirstName = "Participant",
                LastName = "One"
            };

            var participant2 = new User
            {
                Email = "participant2@example.com",
                FirstName = "Participant",
                LastName = "Two"
            };

            var eventEntity = new Event
            {
                Name = "Multi-Participant Event",
                Description = "Test",
                EventDate = DateTime.UtcNow.AddDays(7),
                Status = EventStatus.Upcoming,
                TotalPointsPool = 1000,
                CreatedBy = creator.Id,
                Creator = creator
            };

            var ep1 = new EventParticipant
            {
                EventId = eventEntity.Id,
                UserId = participant1.Id,
                Event = eventEntity,
                User = participant1
            };

            var ep2 = new EventParticipant
            {
                EventId = eventEntity.Id,
                UserId = participant2.Id,
                Event = eventEntity,
                User = participant2
            };

            eventEntity.Participants.Add(ep1);
            eventEntity.Participants.Add(ep2);

            // Act
            context.Users.Add(creator);
            context.Users.Add(participant1);
            context.Users.Add(participant2);
            context.Events.Add(eventEntity);
            context.SaveChanges();

            // Assert
            var savedEvent = context.Events
                .Include(e => e.Participants)
                    .ThenInclude(ep => ep.User)
                .FirstOrDefault(e => e.Id == eventEntity.Id);

            savedEvent.Should().NotBeNull();
            savedEvent!.Participants.Should().HaveCount(2);
            savedEvent.Participants.Select(p => p.User.Email).Should().Contain(new[] 
            { 
                "participant1@example.com", 
                "participant2@example.com" 
            });
        }

        [Fact]
        public void PointsTransaction_User_ManyToOne_ShouldBeCorrectlyMapped()
        {
            // Arrange
            using var context = CreateContext();

            var user = new User
            {
                Email = "transaction@example.com",
                FirstName = "Transaction",
                LastName = "User"
            };

            var transaction = new PointsTransaction
            {
                UserId = user.Id,
                Points = 100,
                Type = TransactionType.Earned,
                Source = SourceType.Event,
                SourceId = Guid.NewGuid(),
                Description = "Test transaction",
                User = user
            };

            // Act
            context.Users.Add(user);
            context.PointsTransactions.Add(transaction);
            context.SaveChanges();

            // Assert
            var savedTransaction = context.PointsTransactions
                .Include(t => t.User)
                .FirstOrDefault(t => t.Id == transaction.Id);

            savedTransaction.Should().NotBeNull();
            savedTransaction!.User.Should().NotBeNull();
            savedTransaction.User.Email.Should().Be("transaction@example.com");
        }
    }
}
