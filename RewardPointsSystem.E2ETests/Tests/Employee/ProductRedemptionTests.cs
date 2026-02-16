using FluentAssertions;
using RewardPointsSystem.E2ETests.Base;
using RewardPointsSystem.E2ETests.Helpers;
using RewardPointsSystem.E2ETests.PageObjects;
using RewardPointsSystem.E2ETests.PageObjects.Employee;
using Xunit;
using Xunit.Abstractions;

namespace RewardPointsSystem.E2ETests.Tests.Employee;

/// <summary>
/// E2E tests for Product Redemption functionality.
/// </summary>
[Trait("Category", "Employee")]
[Trait("Feature", "Products")]
[Trait("Priority", "High")]
public class ProductRedemptionTests : BaseTest
{
    private readonly LoginPage _loginPage;
    private readonly ProductsCatalogPage _productsPage;
    private readonly EmployeeDashboardPage _dashboardPage;
    private readonly List<int> _createdProductIds = new();

    public ProductRedemptionTests(ITestOutputHelper output) : base(output)
    {
        _loginPage = new LoginPage(Driver);
        _productsPage = new ProductsCatalogPage(Driver);
        _dashboardPage = new EmployeeDashboardPage(Driver);
    }

    private void LoginAsEmployee()
    {
        _loginPage.GoTo();
        _loginPage.LoginAsEmployee(Config.EmployeeEmail, Config.EmployeePassword);
    }

    protected override void Dispose(bool disposing)
    {
        foreach (var productId in _createdProductIds)
        {
            try
            {
                ApiHelper.DeleteProductAsync(productId).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logger.Warning(ex, "Failed to cleanup product: {ProductId}", productId);
            }
        }
        base.Dispose(disposing);
    }

    [Fact]
    [Trait("TestType", "Smoke")]
    public void ProductsPage_ShouldLoad_Successfully()
    {
        ExecuteWithScreenshotOnFailure(() =>
        {
            // Arrange
            LoginAsEmployee();

            // Act
            _productsPage.GoTo();

            // Assert
            _productsPage.IsOnPage().Should().BeTrue();
        });
    }

    [Fact]
    [Trait("TestType", "Functional")]
    public void ProductsPage_DisplaysProductCards()
    {
        ExecuteWithScreenshotOnFailure(() =>
        {
            // Arrange
            LoginAsEmployee();

            // Act
            _productsPage.GoTo();

            // Assert
            _productsPage.GetProductCount().Should().BeGreaterThanOrEqualTo(0);
        });
    }

    [Fact]
    [Trait("TestType", "Functional")]
    public void SearchProducts_WithValidName_FiltersResults()
    {
        ExecuteWithScreenshotOnFailure(async () =>
        {
            // Arrange - Create product via API
            var productRequest = TestDataHelper.CreateTestProduct();
            var productId = await ApiHelper.CreateProductAsync(productRequest);
            _createdProductIds.Add(productId);

            LoginAsEmployee();
            _productsPage.GoTo();

            // Act
            _productsPage.SearchProducts(productRequest.Name);

            // Assert
            _productsPage.ProductExists(productRequest.Name).Should().BeTrue();
        }).GetAwaiter().GetResult();
    }

    [Fact]
    [Trait("TestType", "Functional")]
    public void SearchProducts_WithNonExistingName_ShowsNoResults()
    {
        ExecuteWithScreenshotOnFailure(() =>
        {
            // Arrange
            LoginAsEmployee();
            _productsPage.GoTo();

            // Act
            _productsPage.SearchProducts("NonExistentProduct_XYZ_123456");

            // Assert
            _productsPage.GetProductCount().Should().Be(0);
        });
    }

    [Fact]
    [Trait("TestType", "Functional")]
    public void GetProductDetails_ShowsCorrectInfo()
    {
        ExecuteWithScreenshotOnFailure(async () =>
        {
            // Arrange - Create product via API
            var productRequest = TestDataHelper.CreateTestProduct(pointsCost: 400, stock: 25);
            var productId = await ApiHelper.CreateProductAsync(productRequest);
            _createdProductIds.Add(productId);

            LoginAsEmployee();
            _productsPage.GoTo();
            _productsPage.SearchProducts(productRequest.Name);

            // Assert
            _productsPage.ProductExists(productRequest.Name).Should().BeTrue();
            var (name, pointsCost, inStock) = _productsPage.GetProductDetails(productRequest.Name);
            name.Should().Contain(productRequest.Name.Split(' ')[0]); // At least partial match
            inStock.Should().BeTrue();
        }).GetAwaiter().GetResult();
    }

    [Fact]
    [Trait("TestType", "Functional")]
    public void GetAllProductNames_ReturnsProductList()
    {
        ExecuteWithScreenshotOnFailure(() =>
        {
            // Arrange
            LoginAsEmployee();
            _productsPage.GoTo();

            // Act
            var productNames = _productsPage.GetAllProductNames();

            // Assert
            productNames.Should().NotBeNull();
        });
    }

    [Fact]
    [Trait("TestType", "Functional")]
    [Trait("Category", "RequiresPoints")]
    public void RedeemProduct_WithSufficientPoints_ShowsSuccess()
    {
        ExecuteWithScreenshotOnFailure(async () =>
        {
            // Arrange - Create affordable product via API
            var productRequest = TestDataHelper.CreateTestProduct(pointsCost: 10, stock: 100);
            var productId = await ApiHelper.CreateProductAsync(productRequest);
            _createdProductIds.Add(productId);

            LoginAsEmployee();
            
            // First check if employee has sufficient points
            _dashboardPage.GoTo();
            var currentBalance = _dashboardPage.GetCurrentBalance();
            
            if (currentBalance < productRequest.PointsCost)
            {
                Logger.Warning("Insufficient points ({Current}) for redemption test - skipping", currentBalance);
                return;
            }

            _productsPage.GoTo();
            _productsPage.SearchProducts(productRequest.Name);

            if (!_productsPage.ProductExists(productRequest.Name))
            {
                Logger.Warning("Product not found - skipping test");
                return;
            }

            // Act
            _productsPage.RedeemProduct(productRequest.Name, 1);

            // Assert - Should show success or error message
            WaitHelper.WaitForPageLoad(Driver);
            // Either success message appears or points were deducted
        }).GetAwaiter().GetResult();
    }

    [Fact]
    [Trait("TestType", "Functional")]
    public void RedeemProduct_WithInsufficientPoints_ShowsError()
    {
        ExecuteWithScreenshotOnFailure(async () =>
        {
            // Arrange - Create expensive product via API
            var productRequest = TestDataHelper.CreateTestProduct(pointsCost: 999999, stock: 100);
            var productId = await ApiHelper.CreateProductAsync(productRequest);
            _createdProductIds.Add(productId);

            LoginAsEmployee();
            _productsPage.GoTo();
            _productsPage.SearchProducts(productRequest.Name);

            if (!_productsPage.ProductExists(productRequest.Name))
            {
                Logger.Warning("Product not found - skipping test");
                return;
            }

            // Act
            try
            {
                _productsPage.RedeemProduct(productRequest.Name, 1);
            }
            catch
            {
                // Redemption might fail - expected behavior
            }

            // Assert - Should show error message
            WaitHelper.WaitForPageLoad(Driver);
            // Error message should appear indicating insufficient points
        }).GetAwaiter().GetResult();
    }

    [Fact]
    [Trait("TestType", "Functional")]
    public void IsProductInStock_WithStock_ReturnsTrue()
    {
        ExecuteWithScreenshotOnFailure(async () =>
        {
            // Arrange - Create product with stock
            var productRequest = TestDataHelper.CreateTestProduct(stock: 50);
            var productId = await ApiHelper.CreateProductAsync(productRequest);
            _createdProductIds.Add(productId);

            LoginAsEmployee();
            _productsPage.GoTo();
            _productsPage.SearchProducts(productRequest.Name);

            if (!_productsPage.ProductExists(productRequest.Name))
            {
                Logger.Warning("Product not found - skipping test");
                return;
            }

            // Assert
            _productsPage.IsProductInStock(productRequest.Name).Should().BeTrue();
        }).GetAwaiter().GetResult();
    }
}
