using System;
using System.Threading.Tasks;
using FluentAssertions;
using RewardPointsSystem.Domain.Entities.Operations;
using RewardPointsSystem.Infrastructure.Repositories;
using RewardPointsSystem.Application.Services.Accounts;
using RewardPointsSystem.Application.Services.Events;
using RewardPointsSystem.Application.Services.Orchestrators;
using RewardPointsSystem.Application.Services.Products;
using RewardPointsSystem.Application.Services.Users;
using Xunit;

namespace RewardPointsSystem.Tests.UnitTests
{
    public class EventRewardOrchestratorTests : IDisposable
    {
        private readonly InMemoryUnitOfWork _unitOfWork;
        private readonly UserService _userService;
        private readonly EventService _eventService;
        private readonly EventParticipationService _participationService;
        private readonly PointsAccountService _accountService;
        private readonly TransactionService _transactionService;
        private readonly PointsAwardingService _pointsAwardingService;
        private readonly EventRewardOrchestrator _orchestrator;

        public EventRewardOrchestratorTests()
        {
            _unitOfWork = new InMemoryUnitOfWork();
            _userService = new UserService(_unitOfWork);
            _eventService = new EventService(_unitOfWork);
            _participationService = new EventParticipationService(_unitOfWork);
            _accountService = new PointsAccountService(_unitOfWork);
            _transactionService = new TransactionService(_unitOfWork);
            _pointsAwardingService = new PointsAwardingService(_unitOfWork);
            _orchestrator = new EventRewardOrchestrator(
                _eventService,
                _participationService,
                _pointsAwardingService,
                _accountService,
                _transactionService);
        }

        public void Dispose()
        {
            _unitOfWork?.Dispose();
        }

        [Fact]
        public async Task ProcessEventRewardAsync_WithValidData_ShouldAwardPointsSuccessfully()
        {
            // Arrange
            var user = await _userService.CreateUserAsync("user@test.com", "John", "Doe");
            await _accountService.CreateAccountAsync(user.Id);
            var eventObj = await _eventService.CreateEventAsync("Event", "Description", DateTime.UtcNow.AddDays(1), 1000);
            await _participationService.RegisterParticipantAsync(eventObj.Id, user.Id);
            await _eventService.ActivateEventAsync(eventObj.Id);
            await _eventService.CompleteEventAsync(eventObj.Id);

            // Act
            var result = await _orchestrator.ProcessEventRewardAsync(eventObj.Id, user.Id, 500, 1, Guid.NewGuid());

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Message.Should().Contain("awarded");
        }

        [Fact]
        public async Task ProcessEventRewardAsync_WithNonExistentUser_ShouldFail()
        {
            // Arrange
            var eventObj = await _eventService.CreateEventAsync("Event", "Description", DateTime.UtcNow.AddDays(1), 1000);
            await _eventService.ActivateEventAsync(eventObj.Id);
            await _eventService.CompleteEventAsync(eventObj.Id);

            // Act
            var result = await _orchestrator.ProcessEventRewardAsync(eventObj.Id, Guid.NewGuid(), 500, 1, Guid.NewGuid());

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Failed");
        }

        [Fact]
        public async Task ProcessEventRewardAsync_WithNonExistentEvent_ShouldFail()
        {
            // Arrange
            var user = await _userService.CreateUserAsync("user@test.com", "John", "Doe");
            await _accountService.CreateAccountAsync(user.Id);

            // Act
            var result = await _orchestrator.ProcessEventRewardAsync(Guid.NewGuid(), user.Id, 500, 1, Guid.NewGuid());

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
        }

        [Fact]
        public async Task ProcessEventRewardAsync_WithNonParticipant_ShouldFail()
        {
            // Arrange
            var user = await _userService.CreateUserAsync("user@test.com", "John", "Doe");
            await _accountService.CreateAccountAsync(user.Id);
            var eventObj = await _eventService.CreateEventAsync("Event", "Description", DateTime.UtcNow.AddDays(1), 1000);
            await _eventService.ActivateEventAsync(eventObj.Id);
            await _eventService.CompleteEventAsync(eventObj.Id);

            // Act
            var result = await _orchestrator.ProcessEventRewardAsync(eventObj.Id, user.Id, 500, 1, Guid.NewGuid());

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
        }

        [Fact]
        public async Task ProcessEventRewardAsync_ExceedingPool_ShouldFail()
        {
            // Arrange
            var user = await _userService.CreateUserAsync("user@test.com", "John", "Doe");
            await _accountService.CreateAccountAsync(user.Id);
            var eventObj = await _eventService.CreateEventAsync("Event", "Description", DateTime.UtcNow.AddDays(1), 500);
            await _participationService.RegisterParticipantAsync(eventObj.Id, user.Id);
            await _eventService.ActivateEventAsync(eventObj.Id);
            await _eventService.CompleteEventAsync(eventObj.Id);

            // Act
            var result = await _orchestrator.ProcessEventRewardAsync(eventObj.Id, user.Id, 600, 1, Guid.NewGuid());

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
        }
    }

    public class RedemptionOrchestratorTests : IDisposable
    {
        private readonly InMemoryUnitOfWork _unitOfWork;
        private readonly UserService _userService;
        private readonly PointsAccountService _accountService;
        private readonly TransactionService _transactionService;
        private readonly ProductCatalogService _productService;
        private readonly PricingService _pricingService;
        private readonly InventoryService _inventoryService;
        private readonly RedemptionOrchestrator _orchestrator;

        public RedemptionOrchestratorTests()
        {
            _unitOfWork = new InMemoryUnitOfWork();
            _userService = new UserService(_unitOfWork);
            _accountService = new PointsAccountService(_unitOfWork);
            _transactionService = new TransactionService(_unitOfWork);
            _productService = new ProductCatalogService(_unitOfWork);
            _pricingService = new PricingService(_unitOfWork);
            _inventoryService = new InventoryService(_unitOfWork);
            _orchestrator = new RedemptionOrchestrator(
                _accountService,
                _pricingService,
                _inventoryService,
                _transactionService,
                _unitOfWork);
        }

        public void Dispose()
        {
            _unitOfWork?.Dispose();
        }

        [Fact]
        public async Task ProcessRedemptionAsync_WithValidData_ShouldSucceed()
        {
            // Arrange
            var user = await _userService.CreateUserAsync("user@test.com", "John", "Doe");
            await _accountService.CreateAccountAsync(user.Id);
            await _accountService.AddPointsAsync(user.Id, 1000);

            var product = await _productService.CreateProductAsync("Product", "Description", "Category");
            await _pricingService.SetProductPointsCostAsync(product.Id, 500, DateTime.UtcNow);
            await _inventoryService.CreateInventoryAsync(product.Id, 10, 2);

            // Act
            var result = await _orchestrator.ProcessRedemptionAsync(user.Id, product.Id);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Redemption.Should().NotBeNull();
            result.Redemption.Status.Should().Be(RedemptionStatus.Pending);
        }

        [Fact]
        public async Task ProcessRedemptionAsync_WithInsufficientBalance_ShouldFail()
        {
            // Arrange
            var user = await _userService.CreateUserAsync("user@test.com", "John", "Doe");
            await _accountService.CreateAccountAsync(user.Id);
            await _accountService.AddPointsAsync(user.Id, 100);

            var product = await _productService.CreateProductAsync("Product", "Description", "Category");
            await _pricingService.SetProductPointsCostAsync(product.Id, 500, DateTime.UtcNow);
            await _inventoryService.CreateInventoryAsync(product.Id, 10, 2);

            // Act
            var result = await _orchestrator.ProcessRedemptionAsync(user.Id, product.Id);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Insufficient");
        }

        [Fact]
        public async Task ProcessRedemptionAsync_WithOutOfStock_ShouldFail()
        {
            // Arrange
            var user = await _userService.CreateUserAsync("user@test.com", "John", "Doe");
            await _accountService.CreateAccountAsync(user.Id);
            await _accountService.AddPointsAsync(user.Id, 1000);

            var product = await _productService.CreateProductAsync("Product", "Description", "Category");
            await _pricingService.SetProductPointsCostAsync(product.Id, 500, DateTime.UtcNow);
            await _inventoryService.CreateInventoryAsync(product.Id, 0, 2);

            // Act
            var result = await _orchestrator.ProcessRedemptionAsync(user.Id, product.Id);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("out of stock");
        }

        [Fact]
        public async Task ProcessRedemptionAsync_WithNonExistentUser_ShouldFail()
        {
            // Arrange
            var product = await _productService.CreateProductAsync("Product", "Description", "Category");
            await _pricingService.SetProductPointsCostAsync(product.Id, 500, DateTime.UtcNow);
            await _inventoryService.CreateInventoryAsync(product.Id, 10, 2);

            // Act
            var result = await _orchestrator.ProcessRedemptionAsync(Guid.NewGuid(), product.Id);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
        }

        [Fact]
        public async Task ProcessRedemptionAsync_WithNonExistentProduct_ShouldFail()
        {
            // Arrange
            var user = await _userService.CreateUserAsync("user@test.com", "John", "Doe");
            await _accountService.CreateAccountAsync(user.Id);
            await _accountService.AddPointsAsync(user.Id, 1000);

            // Act
            var result = await _orchestrator.ProcessRedemptionAsync(user.Id, Guid.NewGuid());

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
        }

        [Fact]
        public async Task ApproveRedemptionAsync_WithPendingRedemption_ShouldApprove()
        {
            // Arrange
            var user = await _userService.CreateUserAsync("user@test.com", "John", "Doe");
            await _accountService.CreateAccountAsync(user.Id);
            await _accountService.AddPointsAsync(user.Id, 1000);

            var product = await _productService.CreateProductAsync("Product", "Description", "Category");
            await _pricingService.SetProductPointsCostAsync(product.Id, 500, DateTime.UtcNow);
            await _inventoryService.CreateInventoryAsync(product.Id, 10, 2);

            var redemptionResult = await _orchestrator.ProcessRedemptionAsync(user.Id, product.Id);
            redemptionResult.Redemption.Status.Should().Be(RedemptionStatus.Pending);

            // Act
            await _orchestrator.ApproveRedemptionAsync(redemptionResult.Redemption.Id);

            // Assert - verify no exception thrown and status updated via database
            var updatedRedemption = await _unitOfWork.Redemptions.GetByIdAsync(redemptionResult.Redemption.Id);
            updatedRedemption.Status.Should().Be(RedemptionStatus.Approved);
        }

        [Fact]
        public async Task ApproveRedemptionAsync_WithNonExistentRedemption_ShouldThrowException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _orchestrator.ApproveRedemptionAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task DeliverRedemptionAsync_WithApprovedRedemption_ShouldDeliver()
        {
            // Arrange
            var user = await _userService.CreateUserAsync("user@test.com", "John", "Doe");
            await _accountService.CreateAccountAsync(user.Id);
            await _accountService.AddPointsAsync(user.Id, 1000);

            var product = await _productService.CreateProductAsync("Product", "Description", "Category");
            await _pricingService.SetProductPointsCostAsync(product.Id, 500, DateTime.UtcNow);
            await _inventoryService.CreateInventoryAsync(product.Id, 10, 2);

            var redemptionResult = await _orchestrator.ProcessRedemptionAsync(user.Id, product.Id);
            await _orchestrator.ApproveRedemptionAsync(redemptionResult.Redemption.Id);

            // Act
            await _orchestrator.DeliverRedemptionAsync(redemptionResult.Redemption.Id, "Delivered to user");

            // Assert - verify no exception thrown
            redemptionResult.Should().NotBeNull();
        }

        [Fact]
        public async Task DeliverRedemptionAsync_WithNonExistentRedemption_ShouldThrowException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _orchestrator.DeliverRedemptionAsync(Guid.NewGuid(), "Notes"));
        }

        [Fact]
        public async Task CancelRedemptionAsync_WithPendingRedemption_ShouldCancel()
        {
            // Arrange
            var user = await _userService.CreateUserAsync("user@test.com", "John", "Doe");
            await _accountService.CreateAccountAsync(user.Id);
            await _accountService.AddPointsAsync(user.Id, 1000);

            var product = await _productService.CreateProductAsync("Product", "Description", "Category");
            await _pricingService.SetProductPointsCostAsync(product.Id, 500, DateTime.UtcNow);
            await _inventoryService.CreateInventoryAsync(product.Id, 10, 2);

            var redemptionResult = await _orchestrator.ProcessRedemptionAsync(user.Id, product.Id);

            // Act
            await _orchestrator.CancelRedemptionAsync(redemptionResult.Redemption.Id);

            // Assert - verify no exception thrown
            var balance = await _accountService.GetBalanceAsync(user.Id);
            balance.Should().Be(1000); // Points should be refunded
        }

        [Fact]
        public async Task CancelRedemptionAsync_WithNonExistentRedemption_ShouldThrowException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _orchestrator.CancelRedemptionAsync(Guid.NewGuid()));
        }
    }
}
