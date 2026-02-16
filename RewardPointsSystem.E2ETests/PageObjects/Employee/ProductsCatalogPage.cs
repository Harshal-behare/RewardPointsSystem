using OpenQA.Selenium;
using RewardPointsSystem.E2ETests.Helpers;

namespace RewardPointsSystem.E2ETests.PageObjects.Employee;

/// <summary>
/// Page Object for the Employee Products Catalog page.
/// </summary>
public class ProductsCatalogPage : BasePage
{
    // Locators
    private static readonly By PageHeader = By.CssSelector("h1, .page-header h1");
    private static readonly By ProductCards = By.CssSelector(".product-card, .product-item");
    private static readonly By SearchInput = By.CssSelector("[data-test='search-input'], input[placeholder*='Search']");
    private static readonly By CategoryFilter = By.CssSelector("[data-test='category-filter'], select[name='category']");
    private static readonly By SortDropdown = By.CssSelector("[data-test='sort-dropdown'], select[name='sort']");
    
    // Product card elements
    private static readonly By ProductName = By.CssSelector(".product-name, .product-title, h3");
    private static readonly By ProductPoints = By.CssSelector(".product-points, .points-cost, .price");
    private static readonly By ProductStock = By.CssSelector(".stock, .availability");
    private static readonly By RedeemButton = By.XPath(".//button[@data-test='redeem-btn']|.//button[contains(@class,'btn-redeem')]|.//button[contains(text(),'Redeem')]");
    private static readonly By OutOfStockBadge = By.CssSelector("[data-test='out-of-stock'], .out-of-stock, .badge-danger");
    
    // Modal
    private static readonly By RedeemModal = By.CssSelector("[data-test='redeem-modal'], .modal, .dialog");
    private static readonly By QuantityInput = By.CssSelector("[data-test='quantity'], input[name='quantity']");
    private static readonly By ConfirmRedeemButton = By.XPath("//button[@data-test='confirm-redeem']|//button[contains(@class,'btn-confirm')]|//button[contains(text(),'Confirm')]");
    private static readonly By CancelButton = By.CssSelector("[data-test='cancel-btn'], .btn-cancel");
    
    // Messages
    private static readonly By SuccessMessage = By.CssSelector("[data-test='success-message'], .success, .alert-success");
    private static readonly By ErrorMessage = By.CssSelector("[data-test='error-message'], .error, .alert-danger");

    public ProductsCatalogPage(IWebDriver driver) : base(driver) { }

    /// <summary>
    /// Navigates to the products catalog page.
    /// </summary>
    public ProductsCatalogPage GoTo()
    {
        NavigateTo("employee/products");
        return this;
    }

    /// <summary>
    /// Checks if on products page.
    /// </summary>
    public bool IsOnPage()
        => CurrentUrl.Contains("products");

    /// <summary>
    /// Gets the count of product cards.
    /// </summary>
    public int GetProductCount()
        => Driver.FindElements(ProductCards).Count;

    /// <summary>
    /// Searches for products.
    /// </summary>
    public ProductsCatalogPage SearchProducts(string searchTerm)
    {
        TypeText(SearchInput, searchTerm);
        WaitForLoadingToComplete();
        return this;
    }

    /// <summary>
    /// Finds a product card by name.
    /// </summary>
    public IWebElement? FindProductCard(string productName)
    {
        var cards = Driver.FindElements(ProductCards);
        return cards.FirstOrDefault(c => c.Text.Contains(productName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Checks if a product exists.
    /// </summary>
    public bool ProductExists(string productName)
        => FindProductCard(productName) != null;

    /// <summary>
    /// Redeems a product.
    /// </summary>
    public ProductsCatalogPage RedeemProduct(string productName, int quantity = 1)
    {
        var card = FindProductCard(productName) 
            ?? throw new NoSuchElementException($"Product '{productName}' not found");
        
        var redeemButton = card.FindElement(RedeemButton);
        ScrollToElement(redeemButton);
        redeemButton.Click();
        
        // Handle modal
        WaitHelper.WaitForElement(Driver, RedeemModal);
        
        if (quantity != 1)
        {
            var quantityElement = WaitHelper.WaitForElement(Driver, QuantityInput);
            quantityElement.Clear();
            quantityElement.SendKeys(quantity.ToString());
        }
        
        SafeClick(ConfirmRedeemButton);
        WaitForLoadingToComplete();
        return this;
    }

    /// <summary>
    /// Checks if product is in stock.
    /// </summary>
    public bool IsProductInStock(string productName)
    {
        var card = FindProductCard(productName);
        if (card == null) return false;
        
        try
        {
            return card.FindElements(OutOfStockBadge).Count == 0;
        }
        catch
        {
            return true;
        }
    }

    /// <summary>
    /// Gets product details.
    /// </summary>
    public (string Name, int PointsCost, bool InStock) GetProductDetails(string productName)
    {
        var card = FindProductCard(productName) 
            ?? throw new NoSuchElementException($"Product '{productName}' not found");
        
        var name = card.FindElements(ProductName).FirstOrDefault()?.Text ?? productName;
        var pointsText = card.FindElements(ProductPoints).FirstOrDefault()?.Text ?? "0";
        var points = int.TryParse(new string(pointsText.Where(char.IsDigit).ToArray()), out var p) ? p : 0;
        var inStock = card.FindElements(OutOfStockBadge).Count == 0;
        
        return (name, points, inStock);
    }

    /// <summary>
    /// Checks if success message is displayed.
    /// </summary>
    public bool HasSuccessMessage()
        => IsDisplayed(SuccessMessage);

    /// <summary>
    /// Gets success message text.
    /// </summary>
    public string GetSuccessMessage()
    {
        try
        {
            return GetText(SuccessMessage);
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Checks if error message is displayed.
    /// </summary>
    public bool HasErrorMessage()
        => IsDisplayed(ErrorMessage);

    /// <summary>
    /// Gets error message text.
    /// </summary>
    public string GetErrorMessage()
    {
        try
        {
            return GetText(ErrorMessage);
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Gets list of all product names.
    /// </summary>
    public List<string> GetAllProductNames()
    {
        var cards = Driver.FindElements(ProductCards);
        return cards
            .Select(c => c.FindElements(ProductName).FirstOrDefault()?.Text ?? string.Empty)
            .Where(n => !string.IsNullOrEmpty(n))
            .ToList();
    }

    /// <summary>
    /// Sorts products by criteria.
    /// </summary>
    public ProductsCatalogPage SortBy(string sortOption)
    {
        var select = WaitHelper.WaitForElement(Driver, SortDropdown);
        var selectElement = new OpenQA.Selenium.Support.UI.SelectElement(select);
        selectElement.SelectByText(sortOption);
        WaitForLoadingToComplete();
        return this;
    }
}
