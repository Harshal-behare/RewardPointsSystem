using OpenQA.Selenium;
using RewardPointsSystem.E2ETests.Helpers;

namespace RewardPointsSystem.E2ETests.PageObjects.Admin;

/// <summary>
/// Page Object for the Admin Dashboard page.
/// </summary>
public class AdminDashboardPage : BasePage
{
    // Locators
    private static readonly By PageHeader = By.CssSelector(".page-header h1");
    private static readonly By KpiCards = By.CssSelector(".kpi-section app-kpi-card");
    private static readonly By QuickActions = By.CssSelector(".quick-actions-floating .quick-action-item");
    private static readonly By RedemptionsAction = By.XPath("//div[contains(@class,'quick-action-item')][.//span[contains(text(),'Redemptions')]]");
    private static readonly By UsersAction = By.XPath("//div[contains(@class,'quick-action-item')][.//span[contains(text(),'Manage Users')]]");
    private static readonly By CreateEventAction = By.XPath("//div[contains(@class,'quick-action-item')][.//span[contains(text(),'Create Event')]]");
    private static readonly By AddProductAction = By.XPath("//div[contains(@class,'quick-action-item')][.//span[contains(text(),'Add Product')]]");
    private static readonly By BudgetSection = By.CssSelector(".budget-section");

    public AdminDashboardPage(IWebDriver driver) : base(driver) { }

    /// <summary>
    /// Navigates to the admin dashboard.
    /// </summary>
    public AdminDashboardPage GoTo()
    {
        NavigateTo("admin/dashboard");
        return this;
    }

    /// <summary>
    /// Gets the page header text.
    /// </summary>
    public string GetHeaderText()
        => GetText(PageHeader);

    /// <summary>
    /// Checks if on admin dashboard.
    /// </summary>
    public bool IsOnDashboard()
        => CurrentUrl.Contains("admin/dashboard") && GetHeaderText().Contains("Admin Dashboard");

    /// <summary>
    /// Gets count of KPI cards.
    /// </summary>
    public int GetKpiCardCount()
        => Driver.FindElements(KpiCards).Count;

    /// <summary>
    /// Clicks the Redemptions quick action.
    /// </summary>
    public void ClickRedemptionsAction()
    {
        var action = WaitHelper.WaitForClickable(Driver, RedemptionsAction);
        action.Click();
        WaitForNavigation("redemptions");
    }

    /// <summary>
    /// Clicks the Manage Users quick action.
    /// </summary>
    public void ClickUsersAction()
    {
        var action = WaitHelper.WaitForClickable(Driver, UsersAction);
        action.Click();
        WaitForNavigation("users");
    }

    /// <summary>
    /// Clicks the Create Event quick action.
    /// </summary>
    public void ClickCreateEventAction()
    {
        var action = WaitHelper.WaitForClickable(Driver, CreateEventAction);
        action.Click();
    }

    /// <summary>
    /// Clicks the Add Product quick action.
    /// </summary>
    public void ClickAddProductAction()
    {
        var action = WaitHelper.WaitForClickable(Driver, AddProductAction);
        action.Click();
    }

    /// <summary>
    /// Checks if budget section is displayed.
    /// </summary>
    public bool IsBudgetSectionDisplayed()
        => IsDisplayed(BudgetSection);

    /// <summary>
    /// Gets quick action count.
    /// </summary>
    public int GetQuickActionCount()
        => Driver.FindElements(QuickActions).Count;
}
