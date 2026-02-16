using OpenQA.Selenium;
using RewardPointsSystem.E2ETests.Helpers;

namespace RewardPointsSystem.E2ETests.PageObjects.Employee;

/// <summary>
/// Page Object for the Employee My Account page.
/// </summary>
public class MyAccountPage : BasePage
{
    // Locators - based on actual frontend HTML
    private static readonly By PageTitle = By.CssSelector(".page-title, h1");
    private static readonly By PageSubtitle = By.CssSelector(".page-subtitle, .subtitle");
    private static readonly By SummarySection = By.CssSelector(".summary-section");
    private static readonly By ProfileSection = By.CssSelector(".summary-section, .user-info, .profile-section");
    private static readonly By UserEmailDisplay = By.CssSelector(".user-email, .email-display, .profile-email");
    private static readonly By CurrentBalanceValue = By.CssSelector(".summary-card.balance .summary-value");
    private static readonly By EarnedPointsValue = By.CssSelector(".summary-card.earned .summary-value");
    private static readonly By EditProfileButton = By.CssSelector(".btn-edit-profile, button[title='Edit']");
    private static readonly By HistoryFilter = By.CssSelector("select, .filter-select");
    
    // Points History Section
    private static readonly By TabsContainer = By.CssSelector(".tabs-container");
    private static readonly By TransactionRows = By.CssSelector(".data-table tbody tr");
    private static readonly By PointsHistoryTable = By.CssSelector(".data-table, .table-container");
    
    // Redemption History Section
    private static readonly By RedemptionHistorySection = By.CssSelector("[data-test='redemption-history'], .redemption-history");
    private static readonly By RedemptionRows = By.CssSelector("[data-test='redemption-row'], .redemption-item");
    
    // Edit Profile Modal
    private static readonly By EditModal = By.CssSelector("[data-test='edit-modal'], .modal, .dialog");
    private static readonly By FirstNameInput = By.CssSelector("[data-test='first-name'], input[name='firstName']");
    private static readonly By LastNameInput = By.CssSelector("[data-test='last-name'], input[name='lastName']");
    private static readonly By SaveButton = By.CssSelector("[data-test='save-btn'], button[type='submit']");
    
    // Tabs - based on actual frontend HTML
    private static readonly By PointsTab = By.XPath("//button[contains(@class,'tab-btn')][contains(.,'Earning')]|//button[contains(@class,'tab-btn')][contains(.,'Points')]");
    private static readonly By RedemptionsTab = By.XPath("//button[contains(@class,'tab-btn')][contains(.,'Redemption')]");

    public MyAccountPage(IWebDriver driver) : base(driver) { }

    /// <summary>
    /// Navigates to the account page.
    /// </summary>
    public MyAccountPage GoTo()
    {
        NavigateTo("employee/account");
        return this;
    }

    /// <summary>
    /// Checks if on account page.
    /// </summary>
    public bool IsOnPage()
        => CurrentUrl.Contains("account");

    /// <summary>
    /// Gets the page title.
    /// </summary>
    public string GetPageTitle()
        => GetText(PageTitle);

    /// <summary>
    /// Gets the current balance value.
    /// </summary>
    public string GetCurrentBalance()
        => GetText(CurrentBalanceValue);

    /// <summary>
    /// Clicks edit profile button.
    /// </summary>
    public MyAccountPage ClickEditProfile()
    {
        SafeClick(EditProfileButton);
        WaitHelper.WaitForElement(Driver, EditModal);
        return this;
    }

    /// <summary>
    /// Updates profile information.
    /// </summary>
    public MyAccountPage UpdateProfile(string firstName, string lastName)
    {
        ClickEditProfile();
        TypeText(FirstNameInput, firstName);
        TypeText(LastNameInput, lastName);
        SafeClick(SaveButton);
        WaitForLoadingToComplete();
        return this;
    }

    /// <summary>
    /// Gets count of transaction history rows.
    /// </summary>
    public int GetTransactionCount()
        => Driver.FindElements(TransactionRows).Count;

    /// <summary>
    /// Filters transaction history.
    /// </summary>
    public MyAccountPage FilterTransactions(string filterType)
    {
        var select = WaitHelper.WaitForElement(Driver, HistoryFilter);
        var selectElement = new OpenQA.Selenium.Support.UI.SelectElement(select);
        selectElement.SelectByText(filterType);
        WaitForLoadingToComplete();
        return this;
    }

    /// <summary>
    /// Clicks the points history tab.
    /// </summary>
    public MyAccountPage ClickPointsTab()
    {
        SafeClick(PointsTab);
        WaitForLoadingToComplete();
        return this;
    }

    /// <summary>
    /// Clicks the redemptions history tab.
    /// </summary>
    public MyAccountPage ClickRedemptionsTab()
    {
        SafeClick(RedemptionsTab);
        WaitForLoadingToComplete();
        return this;
    }

    /// <summary>
    /// Gets count of redemption history rows.
    /// </summary>
    public int GetRedemptionCount()
        => Driver.FindElements(RedemptionRows).Count;

    /// <summary>
    /// Gets transaction details from a row.
    /// </summary>
    public List<(string Description, int Points, string Date)> GetTransactions()
    {
        var rows = Driver.FindElements(TransactionRows);
        var transactions = new List<(string, int, string)>();
        
        foreach (var row in rows)
        {
            var cells = row.FindElements(By.TagName("td"));
            if (cells.Count >= 3)
            {
                var description = cells[0].Text;
                var pointsText = cells[1].Text;
                var date = cells[2].Text;
                int.TryParse(new string(pointsText.Where(c => char.IsDigit(c) || c == '-').ToArray()), out var points);
                transactions.Add((description, points, date));
            }
        }
        
        return transactions;
    }

    /// <summary>
    /// Cancels a pending redemption.
    /// </summary>
    public MyAccountPage CancelRedemption(string productName)
    {
        var rows = Driver.FindElements(RedemptionRows);
        var row = rows.FirstOrDefault(r => 
            r.Text.Contains(productName, StringComparison.OrdinalIgnoreCase) &&
            r.Text.Contains("Pending", StringComparison.OrdinalIgnoreCase));
        
        if (row == null)
            throw new NoSuchElementException($"Pending redemption for '{productName}' not found");
        
        var cancelButton = row.FindElement(By.XPath(".//button[contains(@class,'btn-cancel') or contains(@class,'cancel') or contains(text(),'Cancel')]"));
        cancelButton.Click();
        
        // Confirm cancellation
        try
        {
            var confirm = WaitHelper.WaitForClickable(
                Driver,
                By.CssSelector("[data-test='confirm-cancel'], .btn-confirm"),
                TimeSpan.FromSeconds(3)
            );
            confirm.Click();
        }
        catch { }
        
        WaitForLoadingToComplete();
        return this;
    }

    /// <summary>
    /// Checks if profile section is displayed.
    /// </summary>
    public bool IsProfileDisplayed()
        => IsDisplayed(ProfileSection);

    /// <summary>
    /// Gets the user email if displayed on the page.
    /// </summary>
    public string GetUserEmail()
    {
        try
        {
            return GetText(UserEmailDisplay);
        }
        catch
        {
            // If no email element found, return page title or empty
            return string.Empty;
        }
    }
}
