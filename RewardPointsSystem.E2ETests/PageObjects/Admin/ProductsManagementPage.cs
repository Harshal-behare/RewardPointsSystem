using OpenQA.Selenium;
using RewardPointsSystem.E2ETests.Helpers;

namespace RewardPointsSystem.E2ETests.PageObjects.Admin;

/// <summary>
/// Page Object for the Admin Products Management page.
/// </summary>
public class ProductsManagementPage : BasePage
{
    // Locators
    private static readonly By PageHeader = By.CssSelector("h1, .page-header h1");
    private static readonly By CreateProductButton = By.CssSelector("button.quick-action-btn.primary");
    private static readonly By ProductsGrid = By.CssSelector(".products-grid, .product-list, table");
    private static readonly By ProductCards = By.CssSelector(".product-card, .product-item, table tbody tr");
    private static readonly By SearchInput = By.CssSelector(".search-input, input[placeholder*='Search']");
    
    // Modal locators - modal uses app-button which renders <button> inside
    private static readonly By ProductModal = By.CssSelector(".modal-overlay, .modal-content");
    private static readonly By ProductNameInput = By.Id("productName");
    private static readonly By ProductDescriptionInput = By.Id("productDescription");
    private static readonly By PointsCostInput = By.Id("pointsPrice");
    private static readonly By StockInput = By.Id("stock");
    private static readonly By CategorySelect = By.Id("category");
    private static readonly By SaveButton = By.CssSelector(".modal-footer app-button:last-child button, .modal-footer button.btn-primary");
    private static readonly By CancelButton = By.CssSelector(".modal-footer app-button:first-child button, .modal-footer button.btn-secondary");

    public ProductsManagementPage(IWebDriver driver) : base(driver) { }

    /// <summary>
    /// Navigates to the products management page.
    /// </summary>
    public ProductsManagementPage GoTo()
    {
        NavigateTo("admin/products");
        return this;
    }

    /// <summary>
    /// Checks if on products management page.
    /// </summary>
    public bool IsOnPage()
        => CurrentUrl.Contains("admin/products");

    /// <summary>
    /// Clicks the create product button.
    /// </summary>
    public ProductsManagementPage ClickCreateProduct()
    {
        SafeClick(CreateProductButton);
        WaitHelper.WaitForElement(Driver, ProductModal);
        return this;
    }

    /// <summary>
    /// Fills in the product form.
    /// </summary>
    public ProductsManagementPage FillProductForm(
        string name,
        string description,
        int pointsCost,
        int stock)
    {
        TypeText(ProductNameInput, name);
        TypeText(ProductDescriptionInput, description);
        
        // Wait for categories to load from API and select first available
        try
        {
            // Wait longer since categories load asynchronously from API
            var wait = new OpenQA.Selenium.Support.UI.WebDriverWait(Driver, TimeSpan.FromSeconds(10));
            wait.Until(d => 
            {
                var select = d.FindElement(CategorySelect);
                var options = select.FindElements(By.TagName("option"));
                return options.Count > 1; // More than just the placeholder
            });
            
            var categorySelect = Driver.FindElement(CategorySelect);
            var selectElement = new OpenQA.Selenium.Support.UI.SelectElement(categorySelect);
            
            // Select first non-empty option (index 0 is placeholder "Select a category")
            var nonEmptyOptions = selectElement.Options.Where(o => !string.IsNullOrEmpty(o.GetAttribute("value"))).ToList();
            if (nonEmptyOptions.Count > 0)
            {
                nonEmptyOptions[0].Click();
                // Trigger Angular change detection via dispatchEvent
                ((OpenQA.Selenium.IJavaScriptExecutor)Driver).ExecuteScript(
                    "arguments[0].dispatchEvent(new Event('change', { bubbles: true }));", 
                    categorySelect);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Category selection failed: {ex.Message}");
        }
        
        TypeText(PointsCostInput, pointsCost.ToString());
        TypeText(StockInput, stock.ToString());
        
        // Small wait for form validation to update
        System.Threading.Thread.Sleep(500);
        return this;
    }

    /// <summary>
    /// Clicks the save button.
    /// </summary>
    public ProductsManagementPage ClickSave()
    {
        SafeClick(SaveButton);
        WaitForLoadingToComplete();
        return this;
    }

    /// <summary>
    /// Creates a product with the specified details.
    /// </summary>
    public void CreateProduct(string name, string description, int pointsCost, int stock)
    {
        ClickCreateProduct();
        FillProductForm(name, description, pointsCost, stock);
        ClickSave();
    }

    /// <summary>
    /// Gets the count of products displayed.
    /// </summary>
    public int GetProductCount()
        => Driver.FindElements(ProductCards).Count;

    /// <summary>
    /// Searches for products by name.
    /// </summary>
    public ProductsManagementPage SearchProducts(string searchTerm)
    {
        TypeText(SearchInput, searchTerm);
        WaitForLoadingToComplete();
        return this;
    }

    /// <summary>
    /// Checks if a product with the specified name exists.
    /// </summary>
    public bool ProductExists(string productName)
    {
        var cards = Driver.FindElements(ProductCards);
        return cards.Any(c => c.Text.Contains(productName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Clicks edit on a product.
    /// </summary>
    public ProductsManagementPage ClickEditProduct(string productName)
    {
        var card = Driver.FindElements(ProductCards)
            .FirstOrDefault(c => c.Text.Contains(productName, StringComparison.OrdinalIgnoreCase));
        
        if (card == null)
            throw new NoSuchElementException($"Product '{productName}' not found");

        var editButton = card.FindElement(By.XPath(".//button[@data-test='edit-btn']|.//button[contains(@class,'btn-edit')]|.//button[contains(text(),'Edit')]|.//a[contains(@class,'btn-edit')]"));
        editButton.Click();
        WaitHelper.WaitForElement(Driver, ProductModal);
        return this;
    }

    /// <summary>
    /// Deactivates a product.
    /// </summary>
    public ProductsManagementPage DeactivateProduct(string productName)
    {
        var card = Driver.FindElements(ProductCards)
            .FirstOrDefault(c => c.Text.Contains(productName, StringComparison.OrdinalIgnoreCase));
        
        if (card == null)
            throw new NoSuchElementException($"Product '{productName}' not found");

        var deactivateButton = card.FindElement(
            By.XPath(".//button[@data-test='deactivate-btn']|.//button[contains(@class,'btn-deactivate')]|.//button[contains(text(),'Deactivate')]")
        );
        deactivateButton.Click();
        WaitForLoadingToComplete();
        return this;
    }

    /// <summary>
    /// Checks if modal is displayed.
    /// </summary>
    public bool IsModalDisplayed()
        => IsDisplayed(ProductModal);
}
