using OpenQA.Selenium;
using RewardPointsSystem.E2ETests.Helpers;

namespace RewardPointsSystem.E2ETests.PageObjects.Admin;

/// <summary>
/// Page Object for the Admin Redemptions Management page.
/// </summary>
public class RedemptionsManagementPage : BasePage
{
    // Locators - based on actual frontend HTML structure
    private static readonly By PageHeader = By.CssSelector("h2, .redemptions-header h2");
    private static readonly By RedemptionsTable = By.CssSelector("table, .redemptions-list, .redemption-cards");
    private static readonly By RedemptionRows = By.CssSelector("table tbody tr, .redemption-row");
    private static readonly By FilterTabs = By.CssSelector(".filter-tabs .tab-button");
    private static readonly By StatCards = By.CssSelector(".stat-card.clickable");
    private static readonly By SearchInput = By.CssSelector(".search-input, input[placeholder*='Search']");
    
    // Action buttons (on each row) - using XPath for text matching
    private static readonly By ApproveButton = By.XPath(".//button[@data-test='approve-btn']|.//button[contains(@class,'btn-approve')]|.//button[contains(text(),'Approve')]");
    private static readonly By RejectButton = By.XPath(".//button[@data-test='reject-btn']|.//button[contains(@class,'btn-reject')]|.//button[contains(text(),'Reject')]");
    private static readonly By DeliverButton = By.XPath(".//button[@data-test='deliver-btn']|.//button[contains(@class,'btn-deliver')]|.//button[contains(text(),'Deliver')]");
    
    // Modal/Dialog
    private static readonly By ConfirmDialog = By.CssSelector("[data-test='confirm-dialog'], .modal, .dialog");
    private static readonly By ConfirmButton = By.CssSelector("[data-test='confirm-btn'], .btn-confirm");
    private static readonly By CommentInput = By.CssSelector("[data-test='comment-input'], textarea[name='comment']");

    public RedemptionsManagementPage(IWebDriver driver) : base(driver) { }

    /// <summary>
    /// Navigates to the redemptions management page.
    /// </summary>
    public RedemptionsManagementPage GoTo()
    {
        NavigateTo("admin/redemptions");
        return this;
    }

    /// <summary>
    /// Checks if on redemptions management page.
    /// </summary>
    public bool IsOnPage()
        => CurrentUrl.Contains("admin/redemptions");

    /// <summary>
    /// Gets the count of redemptions.
    /// </summary>
    public int GetRedemptionCount()
        => Driver.FindElements(RedemptionRows).Count;

    /// <summary>
    /// Filters by status using tab buttons or stat cards.
    /// </summary>
    public RedemptionsManagementPage FilterByStatus(string status)
    {
        // Try filter tabs first (tab buttons with text)
        var tabs = Driver.FindElements(FilterTabs);
        var targetTab = tabs.FirstOrDefault(t => t.Text.Contains(status, StringComparison.OrdinalIgnoreCase));
        
        if (targetTab != null)
        {
            targetTab.Click();
            WaitForLoadingToComplete();
            return this;
        }
        
        // Fallback to stat cards (clickable cards on top)
        var statCards = Driver.FindElements(StatCards);
        var targetCard = statCards.FirstOrDefault(c => c.Text.Contains(status, StringComparison.OrdinalIgnoreCase));
        
        if (targetCard != null)
        {
            targetCard.Click();
            WaitForLoadingToComplete();
            return this;
        }
        
        throw new NoSuchElementException($"Filter option '{status}' not found in tabs or stat cards");
    }

    /// <summary>
    /// Finds a redemption row by product name or user.
    /// </summary>
    public IWebElement? FindRedemptionRow(string searchText)
    {
        var rows = Driver.FindElements(RedemptionRows);
        return rows.FirstOrDefault(r => r.Text.Contains(searchText, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Approves a redemption.
    /// </summary>
    public RedemptionsManagementPage ApproveRedemption(string searchText)
    {
        var row = FindRedemptionRow(searchText) 
            ?? throw new NoSuchElementException($"Redemption '{searchText}' not found");
        
        var approveButton = row.FindElement(ApproveButton);
        approveButton.Click();
        
        // Handle confirmation if present
        try
        {
            var confirm = WaitHelper.WaitForClickable(Driver, ConfirmButton, TimeSpan.FromSeconds(3));
            confirm.Click();
        }
        catch { }
        
        WaitForLoadingToComplete();
        return this;
    }

    /// <summary>
    /// Rejects a redemption with a comment.
    /// </summary>
    public RedemptionsManagementPage RejectRedemption(string searchText, string? reason = null)
    {
        var row = FindRedemptionRow(searchText) 
            ?? throw new NoSuchElementException($"Redemption '{searchText}' not found");
        
        var rejectButton = row.FindElement(RejectButton);
        rejectButton.Click();
        
        // Handle confirmation dialog
        WaitHelper.WaitForElement(Driver, ConfirmDialog);
        
        if (!string.IsNullOrEmpty(reason))
        {
            TypeText(CommentInput, reason);
        }
        
        SafeClick(ConfirmButton);
        WaitForLoadingToComplete();
        return this;
    }

    /// <summary>
    /// Marks a redemption as delivered.
    /// </summary>
    public RedemptionsManagementPage DeliverRedemption(string searchText)
    {
        var row = FindRedemptionRow(searchText) 
            ?? throw new NoSuchElementException($"Redemption '{searchText}' not found");
        
        var deliverButton = row.FindElement(DeliverButton);
        deliverButton.Click();
        
        // Handle confirmation if present
        try
        {
            var confirm = WaitHelper.WaitForClickable(Driver, ConfirmButton, TimeSpan.FromSeconds(3));
            confirm.Click();
        }
        catch { }
        
        WaitForLoadingToComplete();
        return this;
    }

    /// <summary>
    /// Gets redemption details from a row.
    /// </summary>
    public (string User, string Product, int Points, string Status) GetRedemptionDetails(string searchText)
    {
        var row = FindRedemptionRow(searchText) 
            ?? throw new NoSuchElementException($"Redemption '{searchText}' not found");
        
        var cells = row.FindElements(By.TagName("td"));
        
        return (
            User: cells.Count > 0 ? cells[0].Text : string.Empty,
            Product: cells.Count > 1 ? cells[1].Text : string.Empty,
            Points: int.TryParse(cells.Count > 2 ? cells[2].Text : "0", out var p) ? p : 0,
            Status: cells.Count > 3 ? cells[3].Text : string.Empty
        );
    }

    /// <summary>
    /// Gets count of pending redemptions.
    /// </summary>
    public int GetPendingRedemptionCount()
    {
        FilterByStatus("Pending");
        return GetRedemptionCount();
    }
}
