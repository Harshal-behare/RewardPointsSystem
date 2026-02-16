using OpenQA.Selenium;
using RewardPointsSystem.E2ETests.Helpers;

namespace RewardPointsSystem.E2ETests.PageObjects.Admin;

/// <summary>
/// Page Object for the Admin Users Management page.
/// </summary>
public class UsersManagementPage : BasePage
{
    // Locators - based on actual frontend HTML structure
    private static readonly By PageHeader = By.CssSelector("h2, .users-header h2");
    private static readonly By UsersTable = By.CssSelector(".users-table, table");
    private static readonly By UserRows = By.CssSelector(".users-table tbody tr, table tbody tr");
    private static readonly By SearchInput = By.CssSelector(".search-input, input[placeholder*='Search']");
    private static readonly By RoleFilter = By.XPath("//div[contains(@class,'filter-group')]//label[contains(text(),'Role')]/following-sibling::select | //div[contains(@class,'filter-group')][last()]//select");
    private static readonly By StatusFilter = By.XPath("//div[contains(@class,'filter-group')]//label[contains(text(),'Status')]/following-sibling::select | //div[contains(@class,'filter-group')][1]//select");
    private static readonly By AddUserButton = By.CssSelector(".quick-action-btn.primary");
    
    // Action buttons
    private static readonly By EditButton = By.CssSelector("[data-test='edit-btn'], .btn-edit");
    private static readonly By ToggleStatusButton = By.CssSelector("[data-test='toggle-status'], .btn-toggle");
    private static readonly By AssignRoleButton = By.CssSelector("[data-test='assign-role'], .btn-role");
    
    // Modal locators
    private static readonly By UserModal = By.CssSelector("[data-test='user-modal'], .modal, .dialog");
    private static readonly By RoleSelect = By.CssSelector("[data-test='role-select'], select[name='role']");
    private static readonly By SaveButton = By.CssSelector("[data-test='save-btn'], button[type='submit']");

    public UsersManagementPage(IWebDriver driver) : base(driver) { }

    /// <summary>
    /// Navigates to the users management page.
    /// </summary>
    public UsersManagementPage GoTo()
    {
        NavigateTo("admin/users");
        return this;
    }

    /// <summary>
    /// Checks if on users management page.
    /// </summary>
    public bool IsOnPage()
        => CurrentUrl.Contains("admin/users");

    /// <summary>
    /// Gets the count of users in the table (excludes no-results row).
    /// </summary>
    public int GetUserCount()
    {
        var rows = Driver.FindElements(UserRows);
        // Exclude the "no-results" row that appears when search returns empty
        return rows.Count(r => !r.Text.Contains("No users found") && !r.GetAttribute("innerHTML").Contains("no-results"));
    }

    /// <summary>
    /// Searches for users.
    /// </summary>
    public UsersManagementPage SearchUsers(string searchTerm)
    {
        TypeText(SearchInput, searchTerm);
        WaitForLoadingToComplete();
        return this;
    }

    /// <summary>
    /// Filters users by role.
    /// </summary>
    public UsersManagementPage FilterByRole(string role)
    {
        var select = WaitHelper.WaitForElement(Driver, RoleFilter);
        var selectElement = new OpenQA.Selenium.Support.UI.SelectElement(select);
        
        // Try exact match first, then partial match
        try
        {
            selectElement.SelectByText(role);
        }
        catch (OpenQA.Selenium.NoSuchElementException)
        {
            // Try finding option that contains the role text
            var options = selectElement.Options;
            var match = options.FirstOrDefault(o => o.Text.Contains(role, StringComparison.OrdinalIgnoreCase));
            if (match != null)
                selectElement.SelectByText(match.Text);
            else
                selectElement.SelectByIndex(1); // Select first non-default option
        }
        
        WaitForLoadingToComplete();
        return this;
    }

    /// <summary>
    /// Finds a user row by email.
    /// </summary>
    public IWebElement? FindUserRow(string email)
    {
        var rows = Driver.FindElements(UserRows);
        return rows.FirstOrDefault(r => r.Text.Contains(email, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Checks if a user exists.
    /// </summary>
    public bool UserExists(string email)
        => FindUserRow(email) != null;

    /// <summary>
    /// Clicks edit on a user row.
    /// </summary>
    public UsersManagementPage ClickEditUser(string email)
    {
        var row = FindUserRow(email) ?? throw new NoSuchElementException($"User '{email}' not found");
        var editButton = row.FindElement(EditButton);
        editButton.Click();
        WaitHelper.WaitForElement(Driver, UserModal);
        return this;
    }

    /// <summary>
    /// Toggles user status (active/inactive).
    /// </summary>
    public UsersManagementPage ToggleUserStatus(string email)
    {
        var row = FindUserRow(email) ?? throw new NoSuchElementException($"User '{email}' not found");
        var toggleButton = row.FindElement(ToggleStatusButton);
        toggleButton.Click();
        WaitForLoadingToComplete();
        return this;
    }

    /// <summary>
    /// Assigns a role to a user.
    /// </summary>
    public UsersManagementPage AssignRole(string email, string role)
    {
        var row = FindUserRow(email) ?? throw new NoSuchElementException($"User '{email}' not found");
        var assignButton = row.FindElement(AssignRoleButton);
        assignButton.Click();
        
        WaitHelper.WaitForElement(Driver, UserModal);
        
        var select = WaitHelper.WaitForElement(Driver, RoleSelect);
        var selectElement = new OpenQA.Selenium.Support.UI.SelectElement(select);
        selectElement.SelectByText(role);
        
        SafeClick(SaveButton);
        WaitForLoadingToComplete();
        return this;
    }

    /// <summary>
    /// Gets user details from a row.
    /// </summary>
    public (string Email, string Name, string Role, bool IsActive) GetUserDetails(string email)
    {
        var row = FindUserRow(email) ?? throw new NoSuchElementException($"User '{email}' not found");
        var cells = row.FindElements(By.TagName("td"));
        
        return (
            Email: email,
            Name: cells.Count > 0 ? cells[0].Text : string.Empty,
            Role: cells.Count > 2 ? cells[2].Text : string.Empty,
            IsActive: row.Text.Contains("Active", StringComparison.OrdinalIgnoreCase)
        );
    }
}
