using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using RewardPointsSystem.Domain.Entities.Products;
using RewardPointsSystem.Domain.Entities.Core;
using RewardPointsSystem.Domain.Exceptions;
using RewardPointsSystem.Infrastructure.Repositories;
using RewardPointsSystem.Application.Services.Products;
using RewardPointsSystem.Application.DTOs.Products;
using RewardPointsSystem.Application.DTOs;
using Xunit;

namespace RewardPointsSystem.Tests.UnitTests
{
    /// <summary>
    /// Unit tests for Product Services (ProductCatalogService, PricingService, InventoryService)
    /// 
    /// These tests verify that product management works correctly.
    /// The product system is split into three services following Single Responsibility Principle:
    /// - ProductCatalogService: Manages product information (name, description, category, image)
    /// - PricingService: Manages product pricing in points
    /// - InventoryService: Manages product stock levels
    /// 
    /// Key scenarios tested:
    /// - Creating products with valid and invalid data
    /// - Updating product details
    /// - Deactivating products (soft delete)
    /// - Setting and retrieving product prices
    /// - Managing inventory (stock levels, reservations)
    /// </summary>
    public class ProductCatalogServiceTests : IDisposable
    {
        private readonly InMemoryUnitOfWork _unitOfWork;
        private readonly ProductCatalogService _productService;
        private Guid _systemUserId;

        public ProductCatalogServiceTests()
        {
            _unitOfWork = new InMemoryUnitOfWork();
            _productService = new ProductCatalogService(_unitOfWork);
            
            // Create a system user for product creation
            InitializeSystemUser().Wait();
        }

        private async Task InitializeSystemUser()
        {
            var systemUser = User.Create("system@test.com", "System", "User");
            await _unitOfWork.Users.AddAsync(systemUser);
            await _unitOfWork.SaveChangesAsync();
            _systemUserId = systemUser.Id;
        }

        public void Dispose()
        {
            _unitOfWork?.Dispose();
        }

        #region Product Creation Tests

        /// <summary>
        /// SCENARIO: Create a product with all valid information
        /// EXPECTED: Product is created successfully with correct details
        /// WHY: This is the basic happy path for adding a new product to the catalog
        /// </summary>
        [Fact]
        public async Task CreateProduct_WithValidData_ShouldCreateActiveProduct()
        {
            // Arrange - Set up the product details
            var createDto = new CreateProductDto
            {
                Name = "Premium Laptop",
                Description = "High-performance laptop for professionals"
            };

            // Act - Create the product
            var product = await _productService.CreateProductAsync(createDto, _systemUserId);

            // Assert - Verify the product was created correctly
            product.Should().NotBeNull("product should be created");
            product.Name.Should().Be("Premium Laptop", "product name should match");
            product.Description.Should().Be("High-performance laptop for professionals", "description should match");
            product.IsActive.Should().BeTrue("new products are active by default");
            product.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5), "creation time should be recent");
        }

        /// <summary>
        /// SCENARIO: Try to create a product without a name
        /// EXPECTED: System rejects the request with appropriate error
        /// WHY: Product name is required for display in the catalog
        /// </summary>
        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CreateProduct_WithMissingName_ShouldRejectRequest(string invalidName)
        {
            // Arrange - Create DTO with missing name
            var createDto = new CreateProductDto
            {
                Name = invalidName,
                Description = "Valid description"
            };

            // Act & Assert - Try to create product and expect error
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _productService.CreateProductAsync(createDto, _systemUserId));
        }

        /// <summary>
        /// SCENARIO: Try to create a product without a creator user ID
        /// EXPECTED: System rejects the request with appropriate error
        /// WHY: All products must have an audit trail showing who created them
        /// </summary>
        [Fact]
        public async Task CreateProduct_WithoutCreator_ShouldRejectRequest()
        {
            // Arrange - Create DTO but use empty GUID for creator
            var createDto = new CreateProductDto
            {
                Name = "Test Product",
                Description = "Test Description"
            };

            // Act & Assert - Try to create with empty creator ID
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _productService.CreateProductAsync(createDto, Guid.Empty));
        }

        #endregion

        #region Product Retrieval Tests

        /// <summary>
        /// SCENARIO: Get all active products in the catalog
        /// EXPECTED: Only active products are returned
        /// WHY: Employees should only see products that are currently available
        /// </summary>
        [Fact]
        public async Task GetActiveProducts_ShouldReturnOnlyActiveProducts()
        {
            // Arrange - Create multiple products
            var product1 = await _productService.CreateProductAsync(
                new CreateProductDto { Name = "Product 1", Description = "Desc 1" }, _systemUserId);
            var product2 = await _productService.CreateProductAsync(
                new CreateProductDto { Name = "Product 2", Description = "Desc 2" }, _systemUserId);
            var product3 = await _productService.CreateProductAsync(
                new CreateProductDto { Name = "Product 3", Description = "Desc 3" }, _systemUserId);
            
            // Deactivate one product
            await _productService.DeactivateProductAsync(product2.Id);

            // Act - Get active products
            var activeProducts = await _productService.GetActiveProductsAsync();

            // Assert - Should only see active products
            activeProducts.Should().HaveCount(2, "only 2 products are active");
            activeProducts.Should().Contain(p => p.Id == product1.Id, "product 1 should be included");
            activeProducts.Should().Contain(p => p.Id == product3.Id, "product 3 should be included");
            activeProducts.Should().NotContain(p => p.Id == product2.Id, "deactivated product should not be included");
        }

        #endregion

        #region Product Update Tests

        /// <summary>
        /// SCENARIO: Update a product's details
        /// EXPECTED: Product is updated with new values
        /// WHY: Admins need to modify product information over time
        /// </summary>
        [Fact]
        public async Task UpdateProduct_WithValidData_ShouldUpdateProductDetails()
        {
            // Arrange - Create a product first
            var product = await _productService.CreateProductAsync(
                new CreateProductDto { Name = "Original Name", Description = "Original Desc" }, _systemUserId);

            var updateDto = new ProductUpdateDto
            {
                Name = "Updated Name",
                Description = "Updated Description"
            };

            // Act - Update the product
            var updatedProduct = await _productService.UpdateProductAsync(product.Id, updateDto);

            // Assert - Verify updates
            updatedProduct.Name.Should().Be("Updated Name", "name should be updated");
            updatedProduct.Description.Should().Be("Updated Description", "description should be updated");
        }

        /// <summary>
        /// SCENARIO: Try to update a non-existent product
        /// EXPECTED: System returns not found error
        /// WHY: Cannot update products that don't exist
        /// </summary>
        [Fact]
        public async Task UpdateProduct_WhenProductDoesNotExist_ShouldThrowError()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            var updateDto = new ProductUpdateDto { Name = "New Name" };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _productService.UpdateProductAsync(nonExistentId, updateDto));
        }

        #endregion

        #region Product Deactivation Tests

        /// <summary>
        /// SCENARIO: Deactivate a product (soft delete)
        /// EXPECTED: Product is marked as inactive but not deleted
        /// WHY: Products are soft-deleted to maintain historical redemption records
        /// </summary>
        [Fact]
        public async Task DeactivateProduct_WhenProductExists_ShouldMarkAsInactive()
        {
            // Arrange - Create an active product
            var product = await _productService.CreateProductAsync(
                new CreateProductDto { Name = "Product to Deactivate", Description = "Desc" }, _systemUserId);
            
            product.IsActive.Should().BeTrue("product starts as active");

            // Act - Deactivate the product
            await _productService.DeactivateProductAsync(product.Id);

            // Assert - Verify product is now inactive
            var activeProducts = await _productService.GetActiveProductsAsync();
            activeProducts.Should().NotContain(p => p.Id == product.Id, "deactivated product should not appear in active list");
        }

        /// <summary>
        /// SCENARIO: Try to deactivate a non-existent product
        /// EXPECTED: System returns not found error
        /// WHY: Cannot deactivate products that don't exist
        /// </summary>
        [Fact]
        public async Task DeactivateProduct_WhenProductDoesNotExist_ShouldThrowError()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _productService.DeactivateProductAsync(nonExistentId));
        }

        #endregion
    }

    /// <summary>
    /// Unit tests for PricingService
    /// 
    /// These tests verify that product pricing management works correctly.
    /// Products have their cost in points managed separately from the product catalog.
    /// </summary>
    public class PricingServiceTests : IDisposable
    {
        private readonly InMemoryUnitOfWork _unitOfWork;
        private readonly ProductCatalogService _productService;
        private readonly PricingService _pricingService;
        private Guid _systemUserId;

        public PricingServiceTests()
        {
            _unitOfWork = new InMemoryUnitOfWork();
            _productService = new ProductCatalogService(_unitOfWork);
            _pricingService = new PricingService(_unitOfWork);
            
            InitializeSystemUser().Wait();
        }

        private async Task InitializeSystemUser()
        {
            var systemUser = User.Create("system@test.com", "System", "User");
            await _unitOfWork.Users.AddAsync(systemUser);
            await _unitOfWork.SaveChangesAsync();
            _systemUserId = systemUser.Id;
        }

        public void Dispose()
        {
            _unitOfWork?.Dispose();
        }

        /// <summary>
        /// SCENARIO: Set a price (in points) for a product
        /// EXPECTED: The price is recorded and can be retrieved
        /// WHY: Products need prices in points for employees to redeem them
        /// </summary>
        [Fact]
        public async Task SetProductPrice_WithValidData_ShouldSetPrice()
        {
            // Arrange - Create a product first
            var product = await _productService.CreateProductAsync(
                new CreateProductDto { Name = "Product", Description = "Desc" }, _systemUserId);
            var pointsCost = 500;

            // Act - Set the price
            await _pricingService.SetProductPointsCostAsync(product.Id, pointsCost, DateTime.UtcNow);

            // Assert - Verify the price is set
            var currentCost = await _pricingService.GetCurrentPointsCostAsync(product.Id);
            currentCost.Should().Be(pointsCost, "price should match what was set");
        }

        /// <summary>
        /// SCENARIO: Try to set a price for a non-existent product
        /// EXPECTED: System returns not found error
        /// WHY: Cannot set prices for products that don't exist
        /// </summary>
        [Fact]
        public async Task SetProductPrice_WhenProductDoesNotExist_ShouldThrowError()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _pricingService.SetProductPointsCostAsync(nonExistentId, 100, DateTime.UtcNow));
        }

        /// <summary>
        /// SCENARIO: Try to set a zero or negative price
        /// EXPECTED: System rejects the request with appropriate error
        /// WHY: Prices must be positive (products cannot be free)
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(-100)]
        public async Task SetProductPrice_WithInvalidPrice_ShouldRejectRequest(int invalidPrice)
        {
            // Arrange - Create a product first
            var product = await _productService.CreateProductAsync(
                new CreateProductDto { Name = "Product", Description = "Desc" }, _systemUserId);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _pricingService.SetProductPointsCostAsync(product.Id, invalidPrice, DateTime.UtcNow));
        }

        /// <summary>
        /// SCENARIO: Update an existing product's price
        /// EXPECTED: The price is updated to the new value
        /// WHY: Prices may need to change over time
        /// </summary>
        [Fact]
        public async Task UpdateProductPrice_ShouldChangeThePrice()
        {
            // Arrange - Create product and set initial price
            var product = await _productService.CreateProductAsync(
                new CreateProductDto { Name = "Product", Description = "Desc" }, _systemUserId);
            await _pricingService.SetProductPointsCostAsync(product.Id, 500, DateTime.UtcNow);

            // Act - Update the price
            await _pricingService.UpdatePointsCostAsync(product.Id, 750);

            // Assert - Verify the price changed
            var currentCost = await _pricingService.GetCurrentPointsCostAsync(product.Id);
            currentCost.Should().Be(750, "price should be updated to new value");
        }

        /// <summary>
        /// SCENARIO: Get price for a product that has no pricing set
        /// EXPECTED: Returns 0 (no price set)
        /// WHY: Products without pricing should return 0, not throw errors
        /// </summary>
        [Fact]
        public async Task GetProductPrice_WhenNoPriceSet_ShouldReturnZero()
        {
            // Arrange - Create product but don't set a price
            var product = await _productService.CreateProductAsync(
                new CreateProductDto { Name = "Product", Description = "Desc" }, _systemUserId);

            // Act - Get the price
            var price = await _pricingService.GetCurrentPointsCostAsync(product.Id);

            // Assert - Should return 0
            price.Should().Be(0, "products without pricing return 0");
        }
    }

    /// <summary>
    /// Unit tests for InventoryService
    /// 
    /// These tests verify that product inventory management works correctly.
    /// Inventory tracks stock levels, reservations, and fulfillment.
    /// </summary>
    public class InventoryServiceTests : IDisposable
    {
        private readonly InMemoryUnitOfWork _unitOfWork;
        private readonly ProductCatalogService _productService;
        private readonly InventoryService _inventoryService;
        private Guid _systemUserId;

        public InventoryServiceTests()
        {
            _unitOfWork = new InMemoryUnitOfWork();
            _productService = new ProductCatalogService(_unitOfWork);
            _inventoryService = new InventoryService(_unitOfWork);
            
            InitializeSystemUser().Wait();
        }

        private async Task InitializeSystemUser()
        {
            var systemUser = User.Create("system@test.com", "System", "User");
            await _unitOfWork.Users.AddAsync(systemUser);
            await _unitOfWork.SaveChangesAsync();
            _systemUserId = systemUser.Id;
        }

        public void Dispose()
        {
            _unitOfWork?.Dispose();
        }

        /// <summary>
        /// SCENARIO: Create inventory for a new product
        /// EXPECTED: Inventory is created with the specified quantity and reorder level
        /// WHY: Products need inventory tracking for redemption management
        /// </summary>
        [Fact]
        public async Task CreateInventory_WithValidData_ShouldCreateInventoryRecord()
        {
            // Arrange - Create a product first
            var product = await _productService.CreateProductAsync(
                new CreateProductDto { Name = "Product", Description = "Desc" }, _systemUserId);
            var initialQuantity = 100;
            var reorderLevel = 10;

            // Act - Create inventory
            var inventory = await _inventoryService.CreateInventoryAsync(product.Id, initialQuantity, reorderLevel);

            // Assert - Verify inventory was created
            inventory.Should().NotBeNull("inventory should be created");
            inventory.ProductId.Should().Be(product.Id, "should be linked to correct product");
            inventory.QuantityAvailable.Should().Be(initialQuantity, "quantity should match");
            inventory.ReorderLevel.Should().Be(reorderLevel, "reorder level should match");
        }

        /// <summary>
        /// SCENARIO: Try to create inventory for a non-existent product
        /// EXPECTED: System returns not found error
        /// WHY: Cannot create inventory for products that don't exist
        /// </summary>
        [Fact]
        public async Task CreateInventory_WhenProductDoesNotExist_ShouldThrowError()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _inventoryService.CreateInventoryAsync(nonExistentId, 100, 10));
        }

        /// <summary>
        /// SCENARIO: Try to create duplicate inventory for a product
        /// EXPECTED: System rejects the request with appropriate error
        /// WHY: Each product can only have one inventory record
        /// </summary>
        [Fact]
        public async Task CreateInventory_WhenInventoryAlreadyExists_ShouldRejectRequest()
        {
            // Arrange - Create product and initial inventory
            var product = await _productService.CreateProductAsync(
                new CreateProductDto { Name = "Product", Description = "Desc" }, _systemUserId);
            await _inventoryService.CreateInventoryAsync(product.Id, 100, 10);

            // Act & Assert - Try to create inventory again
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _inventoryService.CreateInventoryAsync(product.Id, 50, 5));
        }

        /// <summary>
        /// SCENARIO: Check if a product is in stock
        /// EXPECTED: Returns true when quantity > 0, false otherwise
        /// WHY: System needs to know if products are available for redemption
        /// </summary>
        [Fact]
        public async Task IsInStock_WhenProductHasStock_ShouldReturnTrue()
        {
            // Arrange - Create product with inventory
            var product = await _productService.CreateProductAsync(
                new CreateProductDto { Name = "Product", Description = "Desc" }, _systemUserId);
            await _inventoryService.CreateInventoryAsync(product.Id, 100, 10);

            // Act - Check stock
            var isInStock = await _inventoryService.IsInStockAsync(product.Id);

            // Assert
            isInStock.Should().BeTrue("product has stock available");
        }

        /// <summary>
        /// SCENARIO: Check if a product with no inventory is in stock
        /// EXPECTED: Returns false
        /// WHY: Products without inventory records are considered out of stock
        /// </summary>
        [Fact]
        public async Task IsInStock_WhenNoInventoryRecord_ShouldReturnFalse()
        {
            // Arrange - Create product without inventory
            var product = await _productService.CreateProductAsync(
                new CreateProductDto { Name = "Product", Description = "Desc" }, _systemUserId);

            // Act - Check stock
            var isInStock = await _inventoryService.IsInStockAsync(product.Id);

            // Assert
            isInStock.Should().BeFalse("product has no inventory record");
        }

        /// <summary>
        /// SCENARIO: Add stock to an existing inventory
        /// EXPECTED: Quantity increases by the added amount
        /// WHY: Stock levels need to be replenished
        /// </summary>
        [Fact]
        public async Task AddStock_ShouldIncreaseQuantity()
        {
            // Arrange - Create product with initial inventory
            var product = await _productService.CreateProductAsync(
                new CreateProductDto { Name = "Product", Description = "Desc" }, _systemUserId);
            await _inventoryService.CreateInventoryAsync(product.Id, 100, 10);

            // Act - Add more stock
            await _inventoryService.AddStockAsync(product.Id, 50);

            // Assert - Verify stock increased (check via IsInStock still working)
            var isInStock = await _inventoryService.IsInStockAsync(product.Id);
            isInStock.Should().BeTrue("product should still have stock after adding more");
        }

        /// <summary>
        /// SCENARIO: Try to add zero or negative stock
        /// EXPECTED: System rejects the request with appropriate error
        /// WHY: Stock additions must be positive numbers
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(-50)]
        public async Task AddStock_WithInvalidQuantity_ShouldRejectRequest(int invalidQuantity)
        {
            // Arrange - Create product with inventory
            var product = await _productService.CreateProductAsync(
                new CreateProductDto { Name = "Product", Description = "Desc" }, _systemUserId);
            await _inventoryService.CreateInventoryAsync(product.Id, 100, 10);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _inventoryService.AddStockAsync(product.Id, invalidQuantity));
        }

        /// <summary>
        /// SCENARIO: Reserve stock for a pending redemption
        /// EXPECTED: Stock is reserved (cannot be used by other redemptions)
        /// WHY: When someone starts a redemption, the stock should be held for them
        /// </summary>
        [Fact]
        public async Task ReserveStock_WithSufficientQuantity_ShouldSucceed()
        {
            // Arrange - Create product with inventory
            var product = await _productService.CreateProductAsync(
                new CreateProductDto { Name = "Product", Description = "Desc" }, _systemUserId);
            await _inventoryService.CreateInventoryAsync(product.Id, 100, 10);

            // Act - Reserve some stock (should not throw)
            Func<Task> act = () => _inventoryService.ReserveStockAsync(product.Id, 5);

            // Assert - Should succeed without error
            await act.Should().NotThrowAsync("should be able to reserve available stock");
        }

        /// <summary>
        /// SCENARIO: Try to reserve more stock than available
        /// EXPECTED: System rejects with insufficient stock error
        /// WHY: Cannot reserve more than what's available
        /// </summary>
        [Fact]
        public async Task ReserveStock_WithInsufficientQuantity_ShouldRejectRequest()
        {
            // Arrange - Create product with limited inventory
            var product = await _productService.CreateProductAsync(
                new CreateProductDto { Name = "Product", Description = "Desc" }, _systemUserId);
            await _inventoryService.CreateInventoryAsync(product.Id, 5, 2);

            // Act & Assert - Try to reserve more than available
            await Assert.ThrowsAsync<InsufficientInventoryException>(async () =>
                await _inventoryService.ReserveStockAsync(product.Id, 10));
        }
    }
}
