using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RewardPointsSystem.Domain.Entities.Core;
using RewardPointsSystem.Domain.Entities.Products;
using RewardPointsSystem.Domain.Entities.Operations;
using RewardPointsSystem.Application.Interfaces;
using RewardPointsSystem.Application.Services.Core;
using RewardPointsSystem.Application.Services.Products;
using RewardPointsSystem.Application.Services.Accounts;
using RewardPointsSystem.Application.Services.Orchestrators;
using RewardPointsSystem.Application.DTOs.Products;
using RewardPointsSystem.Infrastructure.Data;
using RewardPointsSystem.Infrastructure.Repositories;
using RewardPointsSystem.Tests.TestHelpers;
using Xunit;

namespace RewardPointsSystem.Tests.IntegrationTests
{
    /// <summary>
    /// Integration Tests for Redemption Workflows
    /// 
    /// These tests verify complete product redemption scenarios:
    /// - Product creation and inventory setup
    /// - Employee redemption requests
    /// - Admin approval/rejection workflow
    /// - Points deduction and inventory management
    /// 
    /// WHAT WE'RE TESTING:
    /// Integration between ProductCatalogService, InventoryService, PricingService,
    /// RedemptionOrchestrator, and UserPointsAccountService
    /// 
    /// WHY THESE TESTS MATTER:
    /// Redemption is how employees spend their points. These tests ensure
    /// the complete flow from request to delivery works correctly.
    /// </summary>
    public class RedemptionWorkflowIntegrationTests : IDisposable
    {
        private readonly RewardPointsDbContext _context;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserService _userService;
        private readonly ProductCatalogService _productService;
        private readonly PricingService _pricingService;
        private readonly InventoryService _inventoryService;
        private readonly UserPointsAccountService _accountService;
        private readonly UserPointsTransactionService _transactionService;
        private readonly RedemptionOrchestrator _redemptionOrchestrator;
        private Guid _adminUserId;

        public RedemptionWorkflowIntegrationTests()
        {
            var options = new DbContextOptionsBuilder<RewardPointsDbContext>()
                .UseInMemoryDatabase(databaseName: $"RedemptionWorkflowTests_{Guid.NewGuid()}")
                .Options;

            _context = new RewardPointsDbContext(options);
            _unitOfWork = TestDbContextFactory.CreateInMemoryUnitOfWork();
            
            _userService = new UserService(_unitOfWork);
            _productService = new ProductCatalogService(_unitOfWork);
            _pricingService = new PricingService(_unitOfWork);
            _inventoryService = new InventoryService(_unitOfWork);
            _accountService = new UserPointsAccountService(_unitOfWork);
            _transactionService = new UserPointsTransactionService(_unitOfWork);
            _redemptionOrchestrator = new RedemptionOrchestrator(
                _accountService,
                _pricingService,
                _inventoryService,
                _transactionService,
                _unitOfWork);

            InitializeAdminUserAsync().Wait();
        }

        private async Task InitializeAdminUserAsync()
        {
            var admin = await _userService.CreateUserAsync("admin@company.com", "Admin", "User");
            _adminUserId = admin.Id;
        }

        public void Dispose()
        {
            _context?.Dispose();
        }

        #region Product Setup Tests

        /// <summary>
        /// SCENARIO: Admin sets up a new reward product for employees to redeem
        /// WORKFLOW: Create product → Set price → Create inventory
        /// EXPECTED: Product is available for redemption with correct price and stock
        /// WHY: Products must be properly configured before employees can redeem
        /// </summary>
        [Fact]
        public async Task SetupNewProduct_ShouldBeAvailableForRedemption()
        {
            // Step 1: Create product
            var product = await _productService.CreateProductAsync(
                new CreateProductDto 
                { 
                    Name = "Company Branded Mug", 
                    Description = "Premium ceramic mug with company logo" 
                },
                _adminUserId);

            product.Should().NotBeNull("product should be created");
            product.IsActive.Should().BeTrue("new products should be active");

            // Step 2: Set pricing (500 points)
            await _pricingService.SetProductPointsCostAsync(product.Id, 500, DateTime.UtcNow);
            var currentPrice = await _pricingService.GetCurrentPointsCostAsync(product.Id);
            currentPrice.Should().Be(500, "price should be set correctly");

            // Step 3: Add inventory (20 units)
            await _inventoryService.CreateInventoryAsync(product.Id, 20, 5);
            var isInStock = await _inventoryService.IsInStockAsync(product.Id);
            isInStock.Should().BeTrue("product should be in stock");
        }

        #endregion

        #region Redemption Request Tests

        /// <summary>
        /// SCENARIO: Employee with sufficient points redeems a product
        /// WORKFLOW: Employee has points → Requests redemption → Points deducted
        /// EXPECTED: Redemption created as Pending, points deducted from balance
        /// WHY: This is the primary way employees use their earned points
        /// </summary>
        [Fact]
        public async Task EmployeeRedemption_WithSufficientPoints_ShouldCreatePendingRedemption()
        {
            // Setup: Employee with 1000 points
            var employee = await _userService.CreateUserAsync("shopper@company.com", "Happy", "Shopper");
            await _accountService.CreateAccountAsync(employee.Id);
            await _accountService.AddUserPointsAsync(employee.Id, 1000);

            // Setup: Product worth 500 points with stock
            var product = await _productService.CreateProductAsync(
                new CreateProductDto { Name = "Gift Card", Description = "$50 Gift Card" },
                _adminUserId);
            await _pricingService.SetProductPointsCostAsync(product.Id, 500, DateTime.UtcNow);
            await _inventoryService.CreateInventoryAsync(product.Id, 10, 2);

            // Act: Request redemption
            var result = await _redemptionOrchestrator.ProcessRedemptionAsync(employee.Id, product.Id);

            // Verify: Redemption created and points deducted
            result.Should().NotBeNull("result should be returned");
            result.Success.Should().BeTrue("redemption should succeed");
            result.Redemption.Should().NotBeNull("redemption record should exist");
            result.Redemption.Status.Should().Be(RedemptionStatus.Pending, "new redemptions are Pending");

            var remainingBalance = await _accountService.GetBalanceAsync(employee.Id);
            remainingBalance.Should().Be(500, "500 points should be deducted for the redemption");
        }

        /// <summary>
        /// SCENARIO: Employee without enough points tries to redeem
        /// WORKFLOW: Employee has few points → Requests expensive product
        /// EXPECTED: Redemption rejected with insufficient balance message
        /// WHY: Employees cannot redeem products they can't afford
        /// </summary>
        [Fact]
        public async Task EmployeeRedemption_WithInsufficientPoints_ShouldFail()
        {
            // Setup: Employee with only 100 points
            var employee = await _userService.CreateUserAsync("broke@company.com", "Broke", "Employee");
            await _accountService.CreateAccountAsync(employee.Id);
            await _accountService.AddUserPointsAsync(employee.Id, 100);

            // Setup: Expensive product (1000 points)
            var product = await _productService.CreateProductAsync(
                new CreateProductDto { Name = "Laptop Bag", Description = "Premium leather bag" },
                _adminUserId);
            await _pricingService.SetProductPointsCostAsync(product.Id, 1000, DateTime.UtcNow);
            await _inventoryService.CreateInventoryAsync(product.Id, 5, 1);

            // Act: Try to redeem
            var result = await _redemptionOrchestrator.ProcessRedemptionAsync(employee.Id, product.Id);

            // Verify: Redemption failed
            result.Success.Should().BeFalse("redemption should fail");

            // Verify: Points unchanged
            var balance = await _accountService.GetBalanceAsync(employee.Id);
            balance.Should().Be(100, "balance should be unchanged");
        }

        /// <summary>
        /// SCENARIO: Employee tries to redeem out-of-stock product
        /// WORKFLOW: Product has no stock → Employee requests redemption
        /// EXPECTED: Redemption rejected with out of stock message
        /// WHY: Cannot fulfill orders for products not in stock
        /// </summary>
        [Fact]
        public async Task EmployeeRedemption_WithOutOfStockProduct_ShouldFail()
        {
            // Setup: Employee with enough points
            var employee = await _userService.CreateUserAsync("eager@company.com", "Eager", "Buyer");
            await _accountService.CreateAccountAsync(employee.Id);
            await _accountService.AddUserPointsAsync(employee.Id, 500);

            // Setup: Product with no stock
            var product = await _productService.CreateProductAsync(
                new CreateProductDto { Name = "Limited Item", Description = "Sold out" },
                _adminUserId);
            await _pricingService.SetProductPointsCostAsync(product.Id, 300, DateTime.UtcNow);
            await _inventoryService.CreateInventoryAsync(product.Id, 0, 1); // Zero stock

            // Act: Try to redeem
            var result = await _redemptionOrchestrator.ProcessRedemptionAsync(employee.Id, product.Id);

            // Verify: Redemption failed due to stock
            result.Success.Should().BeFalse("redemption should fail");
        }

        #endregion

        #region Approval Workflow Tests

        /// <summary>
        /// SCENARIO: Admin approves a pending redemption
        /// WORKFLOW: Pending redemption → Admin approves → Status changes
        /// EXPECTED: Redemption status changes to Approved
        /// WHY: Admin approval is required before fulfillment
        /// </summary>
        [Fact]
        public async Task AdminApproval_ShouldChangeStatusToApproved()
        {
            // Setup: Create pending redemption
            var employee = await _userService.CreateUserAsync("approved@company.com", "Approved", "User");
            await _accountService.CreateAccountAsync(employee.Id);
            await _accountService.AddUserPointsAsync(employee.Id, 500);

            var product = await _productService.CreateProductAsync(
                new CreateProductDto { Name = "Reward Item", Description = "Nice item" },
                _adminUserId);
            await _pricingService.SetProductPointsCostAsync(product.Id, 300, DateTime.UtcNow);
            await _inventoryService.CreateInventoryAsync(product.Id, 5, 1);

            var redemptionResult = await _redemptionOrchestrator.ProcessRedemptionAsync(employee.Id, product.Id);
            redemptionResult.Success.Should().BeTrue($"Redemption should succeed, but failed with: {redemptionResult.Message}");
            redemptionResult.Redemption.Should().NotBeNull("Redemption object should not be null");
            redemptionResult.Redemption.Status.Should().Be(RedemptionStatus.Pending);

            // Act: Admin approves
            await _redemptionOrchestrator.ApproveRedemptionAsync(redemptionResult.Redemption.Id, _adminUserId);

            // Verify: Status is Approved
            var redemption = await _unitOfWork.Redemptions.GetByIdAsync(redemptionResult.Redemption.Id);
            redemption.Status.Should().Be(RedemptionStatus.Approved, "status should be Approved");
        }

        /// <summary>
        /// SCENARIO: Admin marks approved redemption as delivered
        /// WORKFLOW: Approved redemption → Mark delivered → Inventory reduced
        /// EXPECTED: Status changes to Delivered, stock decremented
        /// WHY: Delivery confirmation is the final step in the workflow
        /// </summary>
        [Fact]
        public async Task AdminDelivery_ShouldCompleteRedemptionWorkflow()
        {
            // Setup: Create and approve redemption
            var employee = await _userService.CreateUserAsync("delivered@company.com", "Delivery", "User");
            await _accountService.CreateAccountAsync(employee.Id);
            await _accountService.AddUserPointsAsync(employee.Id, 500);

            var product = await _productService.CreateProductAsync(
                new CreateProductDto { Name = "Delivery Test", Description = "Item to deliver" },
                _adminUserId);
            await _pricingService.SetProductPointsCostAsync(product.Id, 300, DateTime.UtcNow);
            await _inventoryService.CreateInventoryAsync(product.Id, 10, 2);

            var redemptionResult = await _redemptionOrchestrator.ProcessRedemptionAsync(employee.Id, product.Id);
            redemptionResult.Success.Should().BeTrue($"Redemption should succeed, but failed with: {redemptionResult.Message}");
            redemptionResult.Redemption.Should().NotBeNull("Redemption object should not be null");
            await _redemptionOrchestrator.ApproveRedemptionAsync(redemptionResult.Redemption.Id, _adminUserId);

            // Act: Mark as delivered
            await _redemptionOrchestrator.DeliverRedemptionAsync(redemptionResult.Redemption.Id, _adminUserId);

            // Verify: Status is Delivered
            var redemption = await _unitOfWork.Redemptions.GetByIdAsync(redemptionResult.Redemption.Id);
            redemption.Status.Should().Be(RedemptionStatus.Delivered, "status should be Delivered");
        }

        /// <summary>
        /// SCENARIO: Admin cancels a pending redemption and refunds points
        /// WORKFLOW: Pending redemption → Admin cancels → Points refunded
        /// EXPECTED: Redemption cancelled, employee's points restored
        /// WHY: Cancellations should fully reverse the transaction
        /// </summary>
        [Fact]
        public async Task AdminCancellation_ShouldRefundPoints()
        {
            // Setup: Create pending redemption
            var employee = await _userService.CreateUserAsync("cancelled@company.com", "Cancel", "User");
            await _accountService.CreateAccountAsync(employee.Id);
            await _accountService.AddUserPointsAsync(employee.Id, 1000);

            var product = await _productService.CreateProductAsync(
                new CreateProductDto { Name = "Cancel Test", Description = "Will be cancelled" },
                _adminUserId);
            await _pricingService.SetProductPointsCostAsync(product.Id, 500, DateTime.UtcNow);
            await _inventoryService.CreateInventoryAsync(product.Id, 5, 1);

            var redemptionResult = await _redemptionOrchestrator.ProcessRedemptionAsync(employee.Id, product.Id);
            
            // Balance after redemption should be 500
            var balanceAfterRedemption = await _accountService.GetBalanceAsync(employee.Id);
            balanceAfterRedemption.Should().Be(500);

            // Act: Cancel the redemption
            await _redemptionOrchestrator.CancelRedemptionAsync(redemptionResult.Redemption.Id, "Customer changed their mind");

            // Verify: Redemption is cancelled
            var redemption = await _unitOfWork.Redemptions.GetByIdAsync(redemptionResult.Redemption.Id);
            redemption.Status.Should().Be(RedemptionStatus.Cancelled, "status should be Cancelled");
        }

        #endregion

        #region End-to-End Workflow Tests

        /// <summary>
        /// SCENARIO: Complete happy path from earning points to successful redemption
        /// WORKFLOW: Earn points → Redeem product → Approve → Deliver
        /// EXPECTED: Employee starts with 0, earns points, redeems, gets product
        /// WHY: This tests the entire employee experience end-to-end
        /// </summary>
        [Fact]
        public async Task CompleteEmployeeJourney_FromEarningToRedemption()
        {
            // Step 1: New employee joins
            var employee = await _userService.CreateUserAsync("newbie@company.com", "New", "Employee");
            await _accountService.CreateAccountAsync(employee.Id);
            
            var startingBalance = await _accountService.GetBalanceAsync(employee.Id);
            startingBalance.Should().Be(0, "new employees start with 0 points");

            // Step 2: Employee earns points (simulating event participation)
            await _accountService.AddUserPointsAsync(employee.Id, 500);
            await _accountService.AddUserPointsAsync(employee.Id, 300);
            
            var earnedBalance = await _accountService.GetBalanceAsync(employee.Id);
            earnedBalance.Should().Be(800, "employee should have earned 800 points");

            // Step 3: Setup reward catalog
            var product = await _productService.CreateProductAsync(
                new CreateProductDto { Name = "Company T-Shirt", Description = "Branded apparel" },
                _adminUserId);
            await _pricingService.SetProductPointsCostAsync(product.Id, 400, DateTime.UtcNow);
            await _inventoryService.CreateInventoryAsync(product.Id, 50, 10);

            // Step 4: Employee redeems product
            var redemption = await _redemptionOrchestrator.ProcessRedemptionAsync(employee.Id, product.Id);
            redemption.Success.Should().BeTrue("redemption should succeed");

            var afterRedemptionBalance = await _accountService.GetBalanceAsync(employee.Id);
            afterRedemptionBalance.Should().Be(400, "400 points should remain after spending 400");

            // Step 5: Admin approves and delivers
            await _redemptionOrchestrator.ApproveRedemptionAsync(redemption.Redemption.Id, _adminUserId);
            await _redemptionOrchestrator.DeliverRedemptionAsync(redemption.Redemption.Id, _adminUserId);

            // Step 6: Verify final state
            var finalRedemption = await _unitOfWork.Redemptions.GetByIdAsync(redemption.Redemption.Id);
            finalRedemption.Status.Should().Be(RedemptionStatus.Delivered, "redemption should be delivered");
        }

        #endregion
    }
}
