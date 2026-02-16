using OpenQA.Selenium;
using RewardPointsSystem.E2ETests.Helpers;

namespace RewardPointsSystem.E2ETests.PageObjects.Employee;

/// <summary>
/// Page Object for the Employee Dashboard page.
/// </summary>
public class EmployeeDashboardPage : BasePage
{
    // Locators - based on actual frontend HTML
    private static readonly By WelcomeTitle = By.CssSelector("h1.dashboard-title, .dashboard-title");
    private static readonly By PageTitle = By.CssSelector(".employee-dashboard h1");
    private static readonly By PointsBalanceSection = By.CssSelector(".points-balance-section");
    private static readonly By EarnedPointsCard = By.CssSelector(".main-balance .balance-value");
    private static readonly By CurrentBalanceCard = By.XPath("//div[contains(@class,'balance-card')][.//span[contains(@class,'available')]]//span[contains(@class,'balance-value')]");
    private static readonly By PendingPointsCard = By.XPath("//div[contains(@class,'balance-card')][.//span[contains(@class,'pending')]]//span[contains(@class,'balance-value')]");
    private static readonly By RedeemedPointsCard = By.XPath("//div[contains(@class,'balance-card')][.//span[contains(@class,'redeemed')]]//span[contains(@class,'balance-value')]");
    
    // Quick actions
    private static readonly By EventsQuickAction = By.XPath("//div[contains(@class,'quick-action-item')][.//span[contains(text(),'Events')]]");
    private static readonly By ProductsQuickAction = By.XPath("//div[contains(@class,'quick-action-item')][.//span[contains(text(),'Products')]]");
    private static readonly By HistoryQuickAction = By.XPath("//div[contains(@class,'quick-action-item')][.//span[contains(text(),'History')]]");
    
    // Sections
    private static readonly By UpcomingEventsSection = By.CssSelector(".events-list, [data-test='upcoming-events']");
    private static readonly By FeaturedProductsSection = By.CssSelector(".products-showcase, [data-test='featured-products']");

    public EmployeeDashboardPage(IWebDriver driver) : base(driver) { }

    /// <summary>
    /// Navigates to the employee dashboard.
    /// </summary>
    public EmployeeDashboardPage GoTo()
    {
        NavigateTo("employee/dashboard");
        return this;
    }

    /// <summary>
    /// Checks if on employee dashboard.
    /// </summary>
    public bool IsOnDashboard()
        => CurrentUrl.Contains("employee/dashboard") || CurrentUrl.Contains("dashboard");

    /// <summary>
    /// Gets the welcome message.
    /// </summary>
    public string GetWelcomeMessage()
        => GetText(WelcomeTitle);

    /// <summary>
    /// Gets earned points value.
    /// </summary>
    public int GetEarnedPoints()
    {
        var text = GetText(EarnedPointsCard);
        return int.TryParse(text.Replace(",", ""), out var points) ? points : 0;
    }

    /// <summary>
    /// Gets current balance value.
    /// </summary>
    public int GetCurrentBalance()
    {
        try
        {
            var balanceCards = Driver.FindElements(By.CssSelector(".balance-card .balance-value"));
            if (balanceCards.Count > 1)
            {
                var text = balanceCards[1].Text;
                return int.TryParse(text.Replace(",", ""), out var points) ? points : 0;
            }
        }
        catch { }
        return 0;
    }

    /// <summary>
    /// Gets pending points value.
    /// </summary>
    public int GetPendingPoints()
    {
        try
        {
            var balanceCards = Driver.FindElements(By.CssSelector(".balance-card .balance-value"));
            if (balanceCards.Count > 2)
            {
                var text = balanceCards[2].Text;
                return int.TryParse(text.Replace(",", ""), out var points) ? points : 0;
            }
        }
        catch { }
        return 0;
    }

    /// <summary>
    /// Gets redeemed points value.
    /// </summary>
    public int GetRedeemedPoints()
    {
        try
        {
            var balanceCards = Driver.FindElements(By.CssSelector(".balance-card .balance-value"));
            if (balanceCards.Count > 3)
            {
                var text = balanceCards[3].Text;
                return int.TryParse(text.Replace(",", ""), out var points) ? points : 0;
            }
        }
        catch { }
        return 0;
    }

    /// <summary>
    /// Clicks the Browse Events quick action.
    /// </summary>
    public void ClickEventsAction()
    {
        var action = WaitHelper.WaitForClickable(Driver, EventsQuickAction);
        action.Click();
        WaitForNavigation("events");
    }

    /// <summary>
    /// Clicks the Shop Products quick action.
    /// </summary>
    public void ClickProductsAction()
    {
        var action = WaitHelper.WaitForClickable(Driver, ProductsQuickAction);
        action.Click();
        WaitForNavigation("products");
    }

    /// <summary>
    /// Clicks the View History quick action.
    /// </summary>
    public void ClickHistoryAction()
    {
        var action = WaitHelper.WaitForClickable(Driver, HistoryQuickAction);
        action.Click();
        WaitForNavigation("account");
    }

    /// <summary>
    /// Checks if points balance section is displayed.
    /// </summary>
    public bool IsPointsBalanceDisplayed()
        => IsDisplayed(PointsBalanceSection);

    /// <summary>
    /// Checks if upcoming events section is displayed.
    /// </summary>
    public bool IsUpcomingEventsDisplayed()
        => IsDisplayed(UpcomingEventsSection);

    /// <summary>
    /// Gets all points summary.
    /// </summary>
    public (int Earned, int Current, int Pending, int Redeemed) GetPointsSummary()
    {
        return (
            Earned: GetEarnedPoints(),
            Current: GetCurrentBalance(),
            Pending: GetPendingPoints(),
            Redeemed: GetRedeemedPoints()
        );
    }
}
