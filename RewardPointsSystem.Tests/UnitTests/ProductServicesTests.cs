using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using RewardPointsSystem.Infrastructure.Repositories;
using RewardPointsSystem.Application.Services.Products;
using Xunit;

namespace RewardPointsSystem.Tests.UnitTests
{
    public class ProductCatalogServiceTests : IDisposable
    {
        private readonly InMemoryUnitOfWork _unitOfWork;
        private readonly ProductCatalogService _productService;

        public ProductCatalogServiceTests()
        {
            _unitOfWork = new InMemoryUnitOfWork();
            _productService = new ProductCatalogService(_unitOfWork);
        }

        public void Dispose()
        {
            _unitOfWork?.Dispose();
        }

        [Fact]
        public async Task CreateProductAsync_WithValidData_ShouldCreateProduct()
        {
            // Arrange
            var name = "Laptop";
            var description = "High-end laptop";
            var category = "Electronics";

            // Act
            var result = await _productService.CreateProductAsync(name, description, category);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(name);
            result.Description.Should().Be(description);
            result.Category.Should().Be(category);
            result.IsActive.Should().BeTrue();
            result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Theory]
        [InlineData(null, "Description", "Category")]
        [InlineData("", "Description", "Category")]
        [InlineData("   ", "Description", "Category")]
        public async Task CreateProductAsync_WithInvalidName_ShouldThrowException(string name, string description, string category)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _productService.CreateProductAsync(name, description, category));
        }

        [Theory]
        [InlineData("Name", null, "Category")]
        [InlineData("Name", "", "Category")]
        [InlineData("Name", "   ", "Category")]
        public async Task CreateProductAsync_WithInvalidDescription_ShouldThrowException(string name, string description, string category)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _productService.CreateProductAsync(name, description, category));
        }

        [Theory]
        [InlineData("Name", "Description", null)]
        [InlineData("Name", "Description", "")]
        [InlineData("Name", "Description", "   ")]
        public async Task CreateProductAsync_WithInvalidCategory_ShouldThrowException(string name, string description, string category)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _productService.CreateProductAsync(name, description, category));
        }


        [Fact]
        public async Task GetActiveProductsAsync_ShouldReturnOnlyActiveProducts()
        {
            // Arrange
            var product1 = await _productService.CreateProductAsync("Product1", "Desc1", "Cat1");
            var product2 = await _productService.CreateProductAsync("Product2", "Desc2", "Cat2");
            var product3 = await _productService.CreateProductAsync("Product3", "Desc3", "Cat3");
            await _productService.DeactivateProductAsync(product2.Id);

            // Act
            var result = await _productService.GetActiveProductsAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(p => p.Id == product1.Id);
            result.Should().Contain(p => p.Id == product3.Id);
            result.Should().NotContain(p => p.Id == product2.Id);
        }



        [Fact]
        public async Task DeactivateProductAsync_WithNonExistentProduct_ShouldThrowException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _productService.DeactivateProductAsync(Guid.NewGuid()));
        }
    }

    public class PricingServiceTests : IDisposable
    {
        private readonly InMemoryUnitOfWork _unitOfWork;
        private readonly ProductCatalogService _productService;
        private readonly PricingService _pricingService;

        public PricingServiceTests()
        {
            _unitOfWork = new InMemoryUnitOfWork();
            _productService = new ProductCatalogService(_unitOfWork);
            _pricingService = new PricingService(_unitOfWork);
        }

        public void Dispose()
        {
            _unitOfWork?.Dispose();
        }

        [Fact]
        public async Task SetProductPointsCostAsync_WithValidData_ShouldSetPrice()
        {
            // Arrange
            var product = await _productService.CreateProductAsync("Product", "Desc", "Cat");
            var pointsCost = 500;

            // Act
            await _pricingService.SetProductPointsCostAsync(product.Id, pointsCost, DateTime.UtcNow);

            // Assert
            var currentCost = await _pricingService.GetCurrentPointsCostAsync(product.Id);
            currentCost.Should().Be(pointsCost);
        }

        [Fact]
        public async Task SetProductPointsCostAsync_WithNonExistentProduct_ShouldThrowException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _pricingService.SetProductPointsCostAsync(Guid.NewGuid(), 100, DateTime.UtcNow));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-100)]
        public async Task SetProductPointsCostAsync_WithInvalidPointsCost_ShouldThrowException(int pointsCost)
        {
            // Arrange
            var product = await _productService.CreateProductAsync("Product", "Desc", "Cat");

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _pricingService.SetProductPointsCostAsync(product.Id, pointsCost, DateTime.UtcNow));
        }

        [Fact]
        public async Task GetCurrentPointsCostAsync_WithExistingPrice_ShouldReturnCost()
        {
            // Arrange
            var product = await _productService.CreateProductAsync("Product", "Desc", "Cat");
            await _pricingService.SetProductPointsCostAsync(product.Id, 750, DateTime.UtcNow);

            // Act
            var result = await _pricingService.GetCurrentPointsCostAsync(product.Id);

            // Assert
            result.Should().Be(750);
        }

        [Fact]
        public async Task GetCurrentPointsCostAsync_WithNoPriceSet_ShouldThrowException()
        {
            // Arrange
            var product = await _productService.CreateProductAsync("Product", "Desc", "Cat");

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _pricingService.GetCurrentPointsCostAsync(product.Id));
        }

        [Fact]
        public async Task UpdatePrice_ShouldUseLatestPrice()
        {
            // Arrange
            var product = await _productService.CreateProductAsync("Product", "Desc", "Cat");
            await _pricingService.SetProductPointsCostAsync(product.Id, 500, DateTime.UtcNow.AddDays(-1));
            await _pricingService.SetProductPointsCostAsync(product.Id, 600, DateTime.UtcNow);

            // Act
            var result = await _pricingService.GetCurrentPointsCostAsync(product.Id);

            // Assert
            result.Should().Be(600);
        }
    }

    public class InventoryServiceTests : IDisposable
    {
        private readonly InMemoryUnitOfWork _unitOfWork;
        private readonly ProductCatalogService _productService;
        private readonly InventoryService _inventoryService;

        public InventoryServiceTests()
        {
            _unitOfWork = new InMemoryUnitOfWork();
            _productService = new ProductCatalogService(_unitOfWork);
            _inventoryService = new InventoryService(_unitOfWork);
        }

        public void Dispose()
        {
            _unitOfWork?.Dispose();
        }

        [Fact]
        public async Task CreateInventoryAsync_WithValidData_ShouldCreateInventory()
        {
            // Arrange
            var product = await _productService.CreateProductAsync("Product", "Desc", "Cat");

            // Act
            var result = await _inventoryService.CreateInventoryAsync(product.Id, 100, 10);

            // Assert
            result.Should().NotBeNull();
            result.ProductId.Should().Be(product.Id);
            result.QuantityAvailable.Should().Be(100);
            result.ReorderLevel.Should().Be(10);
        }

        [Fact]
        public async Task CreateInventoryAsync_WithNonExistentProduct_ShouldThrowException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _inventoryService.CreateInventoryAsync(Guid.NewGuid(), 10, 2));
        }

        [Theory]
        [InlineData(-1, 5)]
        [InlineData(10, -1)]
        public async Task CreateInventoryAsync_WithNegativeQuantities_ShouldThrowException(int quantity, int reorderLevel)
        {
            // Arrange
            var product = await _productService.CreateProductAsync("Product", "Desc", "Cat");

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _inventoryService.CreateInventoryAsync(product.Id, quantity, reorderLevel));
        }

        [Fact]
        public async Task IsInStockAsync_WithAvailableStock_ShouldReturnTrue()
        {
            // Arrange
            var product = await _productService.CreateProductAsync("Product", "Desc", "Cat");
            await _inventoryService.CreateInventoryAsync(product.Id, 50, 5);

            // Act
            var result = await _inventoryService.IsInStockAsync(product.Id);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsInStockAsync_WithZeroStock_ShouldReturnFalse()
        {
            // Arrange
            var product = await _productService.CreateProductAsync("Product", "Desc", "Cat");
            await _inventoryService.CreateInventoryAsync(product.Id, 0, 5);

            // Act
            var result = await _inventoryService.IsInStockAsync(product.Id);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ReserveStockAsync_WithSufficientStock_ShouldReserve()
        {
            // Arrange
            var product = await _productService.CreateProductAsync("Product", "Desc", "Cat");
            await _inventoryService.CreateInventoryAsync(product.Id, 50, 5);

            // Act
            await _inventoryService.ReserveStockAsync(product.Id, 10);

            // Assert - verify no exception thrown
            var isInStock = await _inventoryService.IsInStockAsync(product.Id);
            isInStock.Should().BeTrue();
        }

        [Fact]
        public async Task ReserveStockAsync_WithInsufficientStock_ShouldThrowException()
        {
            // Arrange
            var product = await _productService.CreateProductAsync("Product", "Desc", "Cat");
            await _inventoryService.CreateInventoryAsync(product.Id, 5, 2);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _inventoryService.ReserveStockAsync(product.Id, 10));
        }

        [Fact]
        public async Task ReleaseReservationAsync_ShouldReleaseStock()
        {
            // Arrange
            var product = await _productService.CreateProductAsync("Product", "Desc", "Cat");
            await _inventoryService.CreateInventoryAsync(product.Id, 50, 5);
            await _inventoryService.ReserveStockAsync(product.Id, 15);

            // Act
            await _inventoryService.ReleaseReservationAsync(product.Id, 5);

            // Assert - verify no exception thrown
            var isInStock = await _inventoryService.IsInStockAsync(product.Id);
            isInStock.Should().BeTrue();
        }

        [Fact]
        public async Task AddStockAsync_ShouldIncreaseQuantity()
        {
            // Arrange
            var product = await _productService.CreateProductAsync("Product", "Desc", "Cat");
            await _inventoryService.CreateInventoryAsync(product.Id, 50, 5);

            // Act
            await _inventoryService.AddStockAsync(product.Id, 50);

            // Assert - verify still in stock after adding
            var isInStock = await _inventoryService.IsInStockAsync(product.Id);
            isInStock.Should().BeTrue();
        }
    }
}
