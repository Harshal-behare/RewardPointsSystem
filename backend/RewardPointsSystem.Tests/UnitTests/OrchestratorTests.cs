using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using RewardPointsSystem.Domain.Entities.Operations;
using RewardPointsSystem.Domain.Entities.Core;
using RewardPointsSystem.Infrastructure.Repositories;
using RewardPointsSystem.Application.Services.Accounts;
using RewardPointsSystem.Application.Services.Events;
using RewardPointsSystem.Application.Services.Orchestrators;
using RewardPointsSystem.Application.Services.Products;
using RewardPointsSystem.Application.Services.Core;
using RewardPointsSystem.Application.DTOs.Products;
using RewardPointsSystem.Application.Interfaces;
using RewardPointsSystem.Application.DTOs.Admin;
using RewardPointsSystem.Tests.TestHelpers;
using Xunit;

namespace RewardPointsSystem.Tests.UnitTests
{
    /// <summary>
    /// Unit tests for Event Reward Orchestrator
    /// 
    /// The EventRewardOrchestrator coordinates awarding points to participants
    /// after an event is completed. It handles the full workflow including:
    /// - Validating the event exists and is completed
    /// - Verifying the user participated in the event
    /// - Checking the points pool has enough points
    /// - Crediting points to the user's account
    /// - Creating transaction records
    /// 
    /// Key scenarios tested:
    /// - Successfully awarding points to a participant
    /// - Handling non-existent events or users
    /// - Handling users who didn't participate
    /// - Handling insufficient points pool
    /// </summary>
    public class EventRewardOrchestratorTests : IDisposable
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserService _userService;
        private readonly EventService _eventService;
        private readonly EventParticipationService _participationService;
        private readonly UserPointsAccountService _accountService;
        private readonly UserPointsTransactionService _transactionService;
        private readonly PointsAwardingService _pointsAwardingService;
        private readonly EventRewardOrchestrator _orchestrator;
        private readonly Mock<IAdminBudgetService> _mockBudgetService;

        public EventRewardOrchestratorTests()
        {
            _unitOfWork = TestDbContextFactory.CreateInMemoryUnitOfWork();
            _userService = new UserService(_unitOfWork);
            _eventService = new EventService(_unitOfWork);
            _participationService = new EventParticipationService(_unitOfWork);
            _accountService = new UserPointsAccountService(_unitOfWork);
            _transactionService = new UserPointsTransactionService(_unitOfWork);
            _mockBudgetService = new Mock<IAdminBudgetService>();
            _mockBudgetService.Setup(x => x.ValidatePointsAwardAsync(It.IsAny<Guid>(), It.IsAny<int>()))
                .ReturnsAsync(new BudgetValidationResult { IsAllowed = true });
            _pointsAwardingService = new PointsAwardingService(_unitOfWork, _mockBudgetService.Object);
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

        /// <summary>
        /// SCENARIO: Award points to a user who participated in a completed event
        /// EXPECTED: Points are credited to user's account and transaction is recorded
        /// WHY: This is the main use case - rewarding participants after an event
        /// </summary>
        [Fact]
        public async Task ProcessEventReward_WhenUserParticipatedInCompletedEvent_ShouldAwardPoints()
        {
            // Arrange - Create user with account, event, and register participation
            var user = await _userService.CreateUserAsync("participant@test.com", "John", "Doe");
            await _accountService.CreateAccountAsync(user.Id);
            
            var eventEntity = await _eventService.CreateEventAsync("Sales Competition", "Annual sales event", DateTime.UtcNow.AddDays(1), 1000);
            await _eventService.PublishEventAsync(eventEntity.Id);
            await _participationService.RegisterParticipantAsync(eventEntity.Id, user.Id);
            await _eventService.ActivateEventAsync(eventEntity.Id);
            await _eventService.CompleteEventAsync(eventEntity.Id);

            // Act - Award points to the participant
            var result = await _orchestrator.ProcessEventRewardAsync(eventEntity.Id, user.Id, 500, 1, user.Id);

            // Assert - Verify success
            result.Should().NotBeNull("result should be returned");
            result.Success.Should().BeTrue("points should be awarded successfully");
            result.Message.Should().Contain("awarded", "message should confirm points were awarded");
        }

        /// <summary>
        /// SCENARIO: Try to award points for a non-existent event
        /// EXPECTED: Operation fails with appropriate error
        /// WHY: Cannot award points for events that don't exist
        /// </summary>
        [Fact]
        public async Task ProcessEventReward_WhenEventDoesNotExist_ShouldFail()
        {
            // Arrange - Create user but not event
            var user = await _userService.CreateUserAsync("user@test.com", "John", "Doe");
            await _accountService.CreateAccountAsync(user.Id);

            // Act - Try to award points for non-existent event
            var result = await _orchestrator.ProcessEventRewardAsync(Guid.NewGuid(), user.Id, 500, 1, user.Id);

            // Assert - Verify failure
            result.Should().NotBeNull("result should be returned");
            result.Success.Should().BeFalse("operation should fail");
        }

        /// <summary>
        /// SCENARIO: Try to award points to a user who didn't participate
        /// EXPECTED: Operation fails with appropriate error
        /// WHY: Only participants should receive event rewards
        /// </summary>
        [Fact]
        public async Task ProcessEventReward_WhenUserDidNotParticipate_ShouldFail()
        {
            // Arrange - Create user and event but don't register user as participant
            var user = await _userService.CreateUserAsync("nonparticipant@test.com", "John", "Doe");
            await _accountService.CreateAccountAsync(user.Id);
            
            var eventEntity = await _eventService.CreateEventAsync("Event", "Description", DateTime.UtcNow.AddDays(1), 1000);
            await _eventService.PublishEventAsync(eventEntity.Id);
            await _eventService.ActivateEventAsync(eventEntity.Id);
            await _eventService.CompleteEventAsync(eventEntity.Id);

            // Act - Try to award points to non-participant
            var result = await _orchestrator.ProcessEventRewardAsync(eventEntity.Id, user.Id, 500, 1, user.Id);

            // Assert - Verify failure
            result.Should().NotBeNull("result should be returned");
            result.Success.Should().BeFalse("non-participants should not receive rewards");
        }

        /// <summary>
        /// SCENARIO: Try to award more points than available in the event pool
        /// EXPECTED: Operation fails with insufficient pool error
        /// WHY: Cannot award more points than allocated for the event
        /// </summary>
        [Fact]
        public async Task ProcessEventReward_WhenExceedingPointsPool_ShouldFail()
        {
            // Arrange - Create event with limited points pool
            var user = await _userService.CreateUserAsync("user@test.com", "John", "Doe");
            await _accountService.CreateAccountAsync(user.Id);
            
            var eventEntity = await _eventService.CreateEventAsync("Small Event", "Limited pool", DateTime.UtcNow.AddDays(1), 500);
            await _eventService.PublishEventAsync(eventEntity.Id);
            await _participationService.RegisterParticipantAsync(eventEntity.Id, user.Id);
            await _eventService.ActivateEventAsync(eventEntity.Id);
            await _eventService.CompleteEventAsync(eventEntity.Id);

            // Act - Try to award more points than available
            var result = await _orchestrator.ProcessEventRewardAsync(eventEntity.Id, user.Id, 600, 1, user.Id);

            // Assert - Verify failure
            result.Should().NotBeNull("result should be returned");
            result.Success.Should().BeFalse("cannot exceed points pool");
        }
    }

    /// <summary>
    /// Unit tests for Redemption Orchestrator
    /// 
    /// The RedemptionOrchestrator coordinates the product redemption workflow:
    /// - Processing new redemption requests
    /// - Approving/rejecting redemptions
    /// - Marking redemptions as delivered
    /// - Cancelling redemptions
    /// 
    /// Key scenarios tested:
    /// - Successfully processing a redemption
    /// - Handling insufficient points balance
    /// - Handling out-of-stock products
    /// - Approving and delivering redemptions
    /// - Cancelling redemptions with refund
    /// </summary>
    public class RedemptionOrchestratorTests : IDisposable
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserService _userService;
        private readonly UserPointsAccountService _accountService;
        private readonly UserPointsTransactionService _transactionService;
        private readonly ProductCatalogService _productService;
        private readonly PricingService _pricingService;
        private readonly InventoryService _inventoryService;
        private readonly RedemptionOrchestrator _orchestrator;
        private Guid _adminUserId;

        public RedemptionOrchestratorTests()
        {
            _unitOfWork = TestDbContextFactory.CreateInMemoryUnitOfWork();
            _userService = new UserService(_unitOfWork);
            _accountService = new UserPointsAccountService(_unitOfWork);
            _transactionService = new UserPointsTransactionService(_unitOfWork);
            _productService = new ProductCatalogService(_unitOfWork);
            _pricingService = new PricingService(_unitOfWork);
            _inventoryService = new InventoryService(_unitOfWork);
            _orchestrator = new RedemptionOrchestrator(
                _accountService,
                _pricingService,
                _inventoryService,
                _transactionService,
                _unitOfWork);
            
            InitializeAdminUser().Wait();
        }

        private async Task InitializeAdminUser()
        {
            var admin = User.Create("admin@test.com", "Admin", "User");
            await _unitOfWork.Users.AddAsync(admin);
            await _unitOfWork.SaveChangesAsync();
            _adminUserId = admin.Id;
        }

        public void Dispose()
        {
            _unitOfWork?.Dispose();
        }

        /// <summary>
        /// SCENARIO: User with enough points redeems an in-stock product
        /// EXPECTED: Redemption is created in Pending status, points are deducted
        /// WHY: This is the main happy path for product redemption
        /// </summary>
        [Fact]
        public async Task ProcessRedemption_WhenUserHasEnoughPointsAndProductInStock_ShouldSucceed()
        {
            // Arrange - Create user with points and product with inventory
            var user = await _userService.CreateUserAsync("customer@test.com", "John", "Doe");
            await _accountService.CreateAccountAsync(user.Id);
            await _accountService.AddUserPointsAsync(user.Id, 1000);

            var product = await _productService.CreateProductAsync(
                new CreateProductDto { Name = "Gift Card", Description = "$50 Gift Card" }, 
                _adminUserId);
            await _pricingService.SetProductPointsCostAsync(product.Id, 500, DateTime.UtcNow);
            await _inventoryService.CreateInventoryAsync(product.Id, 10, 2);

            // Act - Process redemption
            var result = await _orchestrator.ProcessRedemptionAsync(user.Id, product.Id);

            // Assert - Verify success
            result.Should().NotBeNull("result should be returned");
            result.Success.Should().BeTrue("redemption should succeed");
            result.Redemption.Should().NotBeNull("redemption record should be created");
            result.Redemption.Status.Should().Be(RedemptionStatus.Pending, "new redemptions start as Pending");
        }

        /// <summary>
        /// SCENARIO: User with insufficient points tries to redeem a product
        /// EXPECTED: Redemption fails with insufficient balance error
        /// WHY: Users cannot redeem products they cannot afford
        /// </summary>
        [Fact]
        public async Task ProcessRedemption_WhenUserHasInsufficientPoints_ShouldFail()
        {
            // Arrange - Create user with few points
            var user = await _userService.CreateUserAsync("pooruser@test.com", "Poor", "User");
            await _accountService.CreateAccountAsync(user.Id);
            await _accountService.AddUserPointsAsync(user.Id, 100); // Only 100 points

            var product = await _productService.CreateProductAsync(
                new CreateProductDto { Name = "Expensive Item", Description = "Costs a lot" }, 
                _adminUserId);
            await _pricingService.SetProductPointsCostAsync(product.Id, 500, DateTime.UtcNow);
            await _inventoryService.CreateInventoryAsync(product.Id, 10, 2);

            // Act - Try to redeem
            var result = await _orchestrator.ProcessRedemptionAsync(user.Id, product.Id);

            // Assert - Verify failure
            result.Should().NotBeNull("result should be returned");
            result.Success.Should().BeFalse("redemption should fail");
            result.Message.Should().Contain("Insufficient", "message should indicate insufficient balance");
        }

        /// <summary>
        /// SCENARIO: User tries to redeem an out-of-stock product
        /// EXPECTED: Redemption fails with out of stock error
        /// WHY: Cannot redeem products that are not available
        /// </summary>
        [Fact]
        public async Task ProcessRedemption_WhenProductIsOutOfStock_ShouldFail()
        {
            // Arrange - Create product with no stock
            var user = await _userService.CreateUserAsync("customer@test.com", "John", "Doe");
            await _accountService.CreateAccountAsync(user.Id);
            await _accountService.AddUserPointsAsync(user.Id, 1000);

            var product = await _productService.CreateProductAsync(
                new CreateProductDto { Name = "Rare Item", Description = "Out of stock" }, 
                _adminUserId);
            await _pricingService.SetProductPointsCostAsync(product.Id, 500, DateTime.UtcNow);
            await _inventoryService.CreateInventoryAsync(product.Id, 0, 2); // No stock

            // Act - Try to redeem
            var result = await _orchestrator.ProcessRedemptionAsync(user.Id, product.Id);

            // Assert - Verify failure
            result.Should().NotBeNull("result should be returned");
            result.Success.Should().BeFalse("redemption should fail");
            result.Message.Should().Contain("out of stock", "message should indicate out of stock");
        }

        /// <summary>
        /// SCENARIO: Admin approves a pending redemption
        /// EXPECTED: Redemption status changes to Approved
        /// WHY: Admin review is required before fulfillment
        /// </summary>
        [Fact]
        public async Task ApproveRedemption_WhenRedemptionIsPending_ShouldChangeStatusToApproved()
        {
            // Arrange - Create a pending redemption
            var user = await _userService.CreateUserAsync("customer@test.com", "John", "Doe");
            await _accountService.CreateAccountAsync(user.Id);
            await _accountService.AddUserPointsAsync(user.Id, 1000);

            var product = await _productService.CreateProductAsync(
                new CreateProductDto { Name = "Product", Description = "Description" }, 
                _adminUserId);
            await _pricingService.SetProductPointsCostAsync(product.Id, 500, DateTime.UtcNow);
            await _inventoryService.CreateInventoryAsync(product.Id, 10, 2);

            var redemptionResult = await _orchestrator.ProcessRedemptionAsync(user.Id, product.Id);
            redemptionResult.Redemption.Status.Should().Be(RedemptionStatus.Pending);

            // Act - Approve the redemption
            await _orchestrator.ApproveRedemptionAsync(redemptionResult.Redemption.Id, _adminUserId);

            // Assert - Verify status changed
            var updatedRedemption = await _unitOfWork.Redemptions.GetByIdAsync(redemptionResult.Redemption.Id);
            updatedRedemption.Should().NotBeNull("redemption should exist");
            updatedRedemption.Status.Should().Be(RedemptionStatus.Approved, "status should be Approved");
        }

        /// <summary>
        /// SCENARIO: Admin marks an approved redemption as delivered
        /// EXPECTED: Redemption status changes to Delivered
        /// WHY: Final step in the redemption workflow
        /// </summary>
        [Fact]
        public async Task DeliverRedemption_WhenRedemptionIsApproved_ShouldChangeStatusToDelivered()
        {
            // Arrange - Create and approve a redemption
            var user = await _userService.CreateUserAsync("customer@test.com", "John", "Doe");
            await _accountService.CreateAccountAsync(user.Id);
            await _accountService.AddUserPointsAsync(user.Id, 1000);

            var product = await _productService.CreateProductAsync(
                new CreateProductDto { Name = "Product", Description = "Description" }, 
                _adminUserId);
            await _pricingService.SetProductPointsCostAsync(product.Id, 500, DateTime.UtcNow);
            await _inventoryService.CreateInventoryAsync(product.Id, 10, 2);

            var redemptionResult = await _orchestrator.ProcessRedemptionAsync(user.Id, product.Id);
            await _orchestrator.ApproveRedemptionAsync(redemptionResult.Redemption.Id, _adminUserId);

            // Act - Mark as delivered
            await _orchestrator.DeliverRedemptionAsync(redemptionResult.Redemption.Id, _adminUserId);

            // Assert - Verify status changed
            var updatedRedemption = await _unitOfWork.Redemptions.GetByIdAsync(redemptionResult.Redemption.Id);
            updatedRedemption.Should().NotBeNull("redemption should exist");
            updatedRedemption.Status.Should().Be(RedemptionStatus.Delivered, "status should be Delivered");
        }

        /// <summary>
        /// SCENARIO: Cancel a pending redemption
        /// EXPECTED: Redemption is cancelled and points are refunded
        /// WHY: Users or admins may need to cancel redemptions
        /// </summary>
        [Fact]
        public async Task CancelRedemption_WhenRedemptionIsPending_ShouldRefundPoints()
        {
            // Arrange - Create a pending redemption
            var user = await _userService.CreateUserAsync("customer@test.com", "John", "Doe");
            await _accountService.CreateAccountAsync(user.Id);
            await _accountService.AddUserPointsAsync(user.Id, 1000);

            var product = await _productService.CreateProductAsync(
                new CreateProductDto { Name = "Product", Description = "Description" }, 
                _adminUserId);
            await _pricingService.SetProductPointsCostAsync(product.Id, 500, DateTime.UtcNow);
            await _inventoryService.CreateInventoryAsync(product.Id, 10, 2);

            var redemptionResult = await _orchestrator.ProcessRedemptionAsync(user.Id, product.Id);
            
            // Balance should be reduced after redemption
            var balanceAfterRedemption = await _accountService.GetBalanceAsync(user.Id);

            // Act - Cancel the redemption
            await _orchestrator.CancelRedemptionAsync(redemptionResult.Redemption.Id, "Changed my mind");

            // Assert - Verify points are refunded
            var finalBalance = await _accountService.GetBalanceAsync(user.Id);
            finalBalance.Should().Be(1000, "points should be refunded to original amount");
        }

        /// <summary>
        /// SCENARIO: Try to approve a non-existent redemption
        /// EXPECTED: Operation fails with appropriate error
        /// WHY: Cannot approve redemptions that don't exist
        /// </summary>
        [Fact]
        public async Task ApproveRedemption_WhenRedemptionDoesNotExist_ShouldThrowError()
        {
            // Arrange - Use random ID
            var nonExistentId = Guid.NewGuid();

            // Act & Assert - Should throw
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _orchestrator.ApproveRedemptionAsync(nonExistentId, _adminUserId));
        }

        /// <summary>
        /// SCENARIO: Try to deliver a non-existent redemption
        /// EXPECTED: Operation fails with appropriate error
        /// WHY: Cannot deliver redemptions that don't exist
        /// </summary>
        [Fact]
        public async Task DeliverRedemption_WhenRedemptionDoesNotExist_ShouldThrowError()
        {
            // Arrange - Use random ID
            var nonExistentId = Guid.NewGuid();

            // Act & Assert - Should throw
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _orchestrator.DeliverRedemptionAsync(nonExistentId, _adminUserId));
        }

        /// <summary>
        /// SCENARIO: Try to cancel a non-existent redemption
        /// EXPECTED: Operation fails with appropriate error
        /// WHY: Cannot cancel redemptions that don't exist
        /// </summary>
        [Fact]
        public async Task CancelRedemption_WhenRedemptionDoesNotExist_ShouldThrowError()
        {
            // Arrange - Use random ID
            var nonExistentId = Guid.NewGuid();

            // Act & Assert - Should throw
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _orchestrator.CancelRedemptionAsync(nonExistentId, "Reason"));
        }
    }
}
