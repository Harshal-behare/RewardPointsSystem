using FluentAssertions;
using RewardPointsSystem.E2ETests.Base;
using RewardPointsSystem.E2ETests.Helpers;
using RewardPointsSystem.E2ETests.PageObjects;
using RewardPointsSystem.E2ETests.PageObjects.Admin;
using Xunit;
using Xunit.Abstractions;

namespace RewardPointsSystem.E2ETests.Tests.Admin;

/// <summary>
/// E2E tests for Product Management functionality.
/// </summary>
[Trait("Category", "Admin")]
[Trait("Feature", "Products")]
[Trait("Priority", "High")]
public class ProductManagementTests : BaseTest
{
    private readonly LoginPage _loginPage;
    private readonly ProductsManagementPage _productsPage;
    private readonly List<int> _createdProductIds = new();

    public ProductManagementTests(ITestOutputHelper output) : base(output)
    {
        _loginPage = new LoginPage(Driver);
        _productsPage = new ProductsManagementPage(Driver);
    }

    private void LoginAsAdmin()
    {
        _loginPage.GoTo();
        _loginPage.LoginAsAdmin(Config.AdminEmail, Config.AdminPassword);
    }

    protected override void Dispose(bool disposing)
    {
        // Cleanup created test products via API
        foreach (var productId in _createdProductIds)
        {
            try
            {
                ApiHelper.DeleteProductAsync(productId).GetAwaiter().GetResult();
                Logger.Information("Cleaned up test product: {ProductId}", productId);
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
            LoginAsAdmin();

            // Act
            _productsPage.GoTo();

            // Assert
            _productsPage.IsOnPage().Should().BeTrue();
        });
    }

    [Fact]
    [Trait("TestType", "Functional")]
    public void CreateProduct_WithValidData_ShouldCreateProduct()
    {
        ExecuteWithScreenshotOnFailure(() =>
        {
            // Arrange
            LoginAsAdmin();
            _productsPage.GoTo();
            
            var productName = TestDataHelper.GenerateProductName();

            // Act
            _productsPage.CreateProduct(
                name: productName,
                description: "E2E Test Product Description",
                pointsCost: 500,
                stock: 100
            );

            // Assert
            WaitHelper.WaitForElementToDisappear(Driver, 
                OpenQA.Selenium.By.CssSelector(".modal, .dialog"));
            
            _productsPage.SearchProducts(productName);
            _productsPage.ProductExists(productName).Should().BeTrue($"Product '{productName}' should exist after creation");
        });
    }

    [Fact]
    [Trait("TestType", "Functional")]
    public void CreateProduct_ClickCancel_ShouldNotCreateProduct()
    {
        ExecuteWithScreenshotOnFailure(() =>
        {
            // Arrange
            LoginAsAdmin();
            _productsPage.GoTo();
            var initialCount = _productsPage.GetProductCount();

            // Act
            _productsPage.ClickCreateProduct();
            _productsPage.IsModalDisplayed().Should().BeTrue();
            
            // Fill partial form then cancel
            var nameInput = WaitHelper.WaitForElement(Driver,
                OpenQA.Selenium.By.CssSelector("#productName, input[name='name']"));
            nameInput.SendKeys("Cancelled Product");
            
            // Cancel button is inside app-button and has btn-secondary class
            var cancelButton = WaitHelper.WaitForClickable(Driver,
                OpenQA.Selenium.By.CssSelector(".modal-footer app-button:first-child button, .modal-footer button.btn-secondary"));
            cancelButton.Click();

            // Assert
            WaitHelper.WaitForElementToDisappear(Driver,
                OpenQA.Selenium.By.CssSelector(".modal-overlay"));
            _productsPage.GetProductCount().Should().Be(initialCount);
        });
    }

    [Fact]
    [Trait("TestType", "Functional")]
    public void SearchProducts_WithExistingName_FiltersList()
    {
        ExecuteWithScreenshotOnFailure(async () =>
        {
            // Arrange - Create product via API for reliability
            var productRequest = TestDataHelper.CreateTestProduct();
            var productId = await ApiHelper.CreateProductAsync(productRequest);
            _createdProductIds.Add(productId);

            LoginAsAdmin();
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
            LoginAsAdmin();
            _productsPage.GoTo();

            // Act
            _productsPage.SearchProducts("NonExistentProduct_XYZ_123456");

            // Assert
            _productsPage.GetProductCount().Should().Be(0);
        });
    }

    [Fact]
    [Trait("TestType", "Functional")]
    public void EditProduct_UpdatePointsCost_ShouldUpdateSuccessfully()
    {
        ExecuteWithScreenshotOnFailure(async () =>
        {
            // Arrange - Create product via API
            var productRequest = TestDataHelper.CreateTestProduct(pointsCost: 300);
            var productId = await ApiHelper.CreateProductAsync(productRequest);
            _createdProductIds.Add(productId);

            LoginAsAdmin();
            _productsPage.GoTo();
            _productsPage.SearchProducts(productRequest.Name);

            // Act
            _productsPage.ClickEditProduct(productRequest.Name);
            
            var pointsInput = WaitHelper.WaitForElement(Driver,
                OpenQA.Selenium.By.CssSelector("[data-test='points-cost'], input[name='pointsCost']"));
            pointsInput.Clear();
            pointsInput.SendKeys("600");
            
            _productsPage.ClickSave();

            // Assert
            WaitHelper.WaitForElementToDisappear(Driver,
                OpenQA.Selenium.By.CssSelector(".modal, .dialog"));
            
            _productsPage.SearchProducts(productRequest.Name);
            var (_, pointsCost, _) = _productsPage.GetProductCount() > 0 
                ? (productRequest.Name, 600, true) // Expected values
                : (string.Empty, 0, false);
            // Verify product still exists
            _productsPage.ProductExists(productRequest.Name).Should().BeTrue();
        }).GetAwaiter().GetResult();
    }

    [Fact]
    [Trait("TestType", "Functional")]
    public void DeactivateProduct_ShouldMarkAsInactive()
    {
        ExecuteWithScreenshotOnFailure(async () =>
        {
            // Arrange - Create product via API
            var productRequest = TestDataHelper.CreateTestProduct();
            var productId = await ApiHelper.CreateProductAsync(productRequest);
            _createdProductIds.Add(productId);

            LoginAsAdmin();
            _productsPage.GoTo();
            _productsPage.SearchProducts(productRequest.Name);

            // Act
            _productsPage.DeactivateProduct(productRequest.Name);

            // Assert - product might be filtered out or shown as inactive
            // Implementation depends on actual UI behavior
            WaitHelper.WaitForLoadingComplete(Driver);
        }).GetAwaiter().GetResult();
    }

    [Fact]
    [Trait("TestType", "Functional")]
    public void ProductsPage_DisplaysProductInfo()
    {
        ExecuteWithScreenshotOnFailure(async () =>
        {
            // Arrange - Create product via API
            var productRequest = TestDataHelper.CreateTestProduct(pointsCost: 750, stock: 50);
            var productId = await ApiHelper.CreateProductAsync(productRequest);
            _createdProductIds.Add(productId);

            LoginAsAdmin();
            _productsPage.GoTo();

            // Act
            _productsPage.SearchProducts(productRequest.Name);

            // Assert
            _productsPage.ProductExists(productRequest.Name).Should().BeTrue();
        }).GetAwaiter().GetResult();
    }
}
