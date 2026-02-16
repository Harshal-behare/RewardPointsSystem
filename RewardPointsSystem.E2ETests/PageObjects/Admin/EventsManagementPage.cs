using OpenQA.Selenium;
using RewardPointsSystem.E2ETests.Helpers;

namespace RewardPointsSystem.E2ETests.PageObjects.Admin;

/// <summary>
/// Page Object for the Admin Events Management page.
/// </summary>
public class EventsManagementPage : BasePage
{
    // Locators - based on actual frontend HTML structure
    private static readonly By PageHeader = By.CssSelector("h2, .events-header h2");
    private static readonly By CreateEventButton = By.CssSelector(".quick-action-btn.primary, app-button");
    private static readonly By EventsTable = By.CssSelector(".events-table, table");
    private static readonly By EventRows = By.CssSelector(".events-table tbody tr, table tbody tr");
    private static readonly By SearchInput = By.CssSelector(".search-input, input[placeholder*='Search']");
    private static readonly By FilterTabs = By.CssSelector(".filter-tabs .filter-tab");
    private static readonly By AwardPointsButton = By.CssSelector(".quick-action-btn.award-btn");
    
    // Modal locators - based on actual frontend HTML
    // Note: modal uses <app-button> which renders a <button> element inside
    private static readonly By EventModal = By.CssSelector(".modal-overlay, .modal-content");
    private static readonly By EventNameInput = By.Id("eventName");
    private static readonly By EventDescriptionInput = By.Id("eventDescription");
    private static readonly By StartDateInput = By.Id("eventDate");
    private static readonly By EndDateInput = By.Id("eventEndDate");
    private static readonly By PointsPoolInput = By.Id("pointsPool");
    private static readonly By StatusSelect = By.Id("eventStatus");
    private static readonly By FirstPlacePointsInput = By.Id("firstPlacePoints");
    private static readonly By SecondPlacePointsInput = By.Id("secondPlacePoints");
    private static readonly By ThirdPlacePointsInput = By.Id("thirdPlacePoints");
    private static readonly By RegistrationStartInput = By.Id("registrationStartDate");
    private static readonly By RegistrationEndInput = By.Id("registrationEndDate");
    private static readonly By SaveButton = By.CssSelector(".modal-footer app-button:last-child button, .modal-footer button.btn-primary");
    private static readonly By CancelButton = By.CssSelector(".modal-footer app-button:first-child button, .modal-footer button.btn-secondary");

    public EventsManagementPage(IWebDriver driver) : base(driver) { }

    /// <summary>
    /// Navigates to the events management page.
    /// </summary>
    public EventsManagementPage GoTo()
    {
        NavigateTo("admin/events");
        return this;
    }

    /// <summary>
    /// Checks if on events management page.
    /// </summary>
    public bool IsOnPage()
        => CurrentUrl.Contains("admin/events");

    /// <summary>
    /// Clicks the create event button.
    /// </summary>
    public EventsManagementPage ClickCreateEvent()
    {
        SafeClick(CreateEventButton);
        WaitHelper.WaitForElement(Driver, EventModal);
        return this;
    }

    /// <summary>
    /// Fills in the event form with all required fields.
    /// Points pool must equal firstPlace + secondPlace + thirdPlace.
    /// </summary>
    public EventsManagementPage FillEventForm(
        string name,
        string description,
        DateTime startDate,
        DateTime endDate,
        int pointsReward)
    {
        TypeText(EventNameInput, name);
        TypeText(EventDescriptionInput, description);
        
        // Select status (Draft or Upcoming for new events)
        try
        {
            var statusSelect = WaitHelper.WaitForElement(Driver, StatusSelect, TimeSpan.FromSeconds(3));
            var selectElement = new OpenQA.Selenium.Support.UI.SelectElement(statusSelect);
            selectElement.SelectByText("Draft");
            ((OpenQA.Selenium.IJavaScriptExecutor)Driver).ExecuteScript(
                "arguments[0].dispatchEvent(new Event('change', { bubbles: true }));",
                statusSelect);
        }
        catch { /* Status may not be visible */ }
        
        // Date inputs - type="date" requires yyyy-MM-dd format (ISO date only)
        var startElement = WaitHelper.WaitForElement(Driver, StartDateInput);
        ((OpenQA.Selenium.IJavaScriptExecutor)Driver).ExecuteScript(
            $"arguments[0].value = '{startDate:yyyy-MM-dd}'; arguments[0].dispatchEvent(new Event('input', {{ bubbles: true }})); arguments[0].dispatchEvent(new Event('change', {{ bubbles: true }}));",
            startElement);
        
        var endElement = WaitHelper.WaitForElement(Driver, EndDateInput);
        ((OpenQA.Selenium.IJavaScriptExecutor)Driver).ExecuteScript(
            $"arguments[0].value = '{endDate:yyyy-MM-dd}'; arguments[0].dispatchEvent(new Event('input', {{ bubbles: true }})); arguments[0].dispatchEvent(new Event('change', {{ bubbles: true }}));",
            endElement);
        
        // Registration dates (registration opens before start, closes before or on event start)
        var regStart = startDate.AddDays(-14);
        var regEnd = startDate; // Registration closes on event start date (must be <= eventDate per validation)
        
        try
        {
            var regStartElement = WaitHelper.WaitForElement(Driver, RegistrationStartInput, TimeSpan.FromSeconds(3));
            ((OpenQA.Selenium.IJavaScriptExecutor)Driver).ExecuteScript(
                $"arguments[0].value = '{regStart:yyyy-MM-dd}'; arguments[0].dispatchEvent(new Event('input', {{ bubbles: true }})); arguments[0].dispatchEvent(new Event('change', {{ bubbles: true }}));",
                regStartElement);
                
            var regEndElement = WaitHelper.WaitForElement(Driver, RegistrationEndInput, TimeSpan.FromSeconds(3));
            ((OpenQA.Selenium.IJavaScriptExecutor)Driver).ExecuteScript(
                $"arguments[0].value = '{regEnd:yyyy-MM-dd}'; arguments[0].dispatchEvent(new Event('input', {{ bubbles: true }})); arguments[0].dispatchEvent(new Event('change', {{ bubbles: true }}));",
                regEndElement);
        }
        catch { /* Registration fields may not be visible */ }
        
        // Points pool
        TypeText(PointsPoolInput, pointsReward.ToString());
        
        // Prize distribution - must sum to pointsReward exactly
        // Split: 50% first, 30% second, 20% third
        int firstPlace = (int)(pointsReward * 0.5);
        int secondPlace = (int)(pointsReward * 0.3);
        int thirdPlace = pointsReward - firstPlace - secondPlace; // Ensure exact sum
        
        try
        {
            TypeText(FirstPlacePointsInput, firstPlace.ToString());
            TypeText(SecondPlacePointsInput, secondPlace.ToString());
            TypeText(ThirdPlacePointsInput, thirdPlace.ToString());
        }
        catch { /* Prize fields may not be visible */ }
        
        // Wait for form validation to update
        System.Threading.Thread.Sleep(500);
        
        return this;
    }

    /// <summary>
    /// Clicks the save button.
    /// </summary>
    public EventsManagementPage ClickSave()
    {
        SafeClick(SaveButton);
        WaitForLoadingToComplete();
        return this;
    }

    /// <summary>
    /// Clicks the cancel button.
    /// </summary>
    public EventsManagementPage ClickCancel()
    {
        SafeClick(CancelButton);
        return this;
    }

    /// <summary>
    /// Creates an event with the specified details.
    /// </summary>
    public void CreateEvent(string name, string description, DateTime startDate, DateTime endDate, int points)
    {
        ClickCreateEvent();
        FillEventForm(name, description, startDate, endDate, points);
        ClickSave();
    }

    /// <summary>
    /// Gets the count of events in the table (excludes no-results row).
    /// </summary>
    public int GetEventCount()
    {
        var rows = Driver.FindElements(EventRows);
        // Exclude the "no-results" row that appears when search returns empty
        return rows.Count(r => !r.Text.Contains("No events found") && !r.GetAttribute("innerHTML").Contains("no-results"));
    }

    /// <summary>
    /// Searches for events by name.
    /// </summary>
    public EventsManagementPage SearchEvents(string searchTerm)
    {
        TypeText(SearchInput, searchTerm);
        WaitForLoadingToComplete();
        return this;
    }

    /// <summary>
    /// Checks if an event with the specified name exists.
    /// </summary>
    public bool EventExists(string eventName)
    {
        var rows = Driver.FindElements(EventRows);
        return rows.Any(r => r.Text.Contains(eventName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Clicks edit on an event row.
    /// </summary>
    public EventsManagementPage ClickEditEvent(string eventName)
    {
        var row = Driver.FindElements(EventRows)
            .FirstOrDefault(r => r.Text.Contains(eventName, StringComparison.OrdinalIgnoreCase));
        
        if (row == null)
            throw new NoSuchElementException($"Event '{eventName}' not found");

        var editButton = row.FindElement(By.XPath(".//button[@data-test='edit-btn']|.//button[contains(@class,'btn-edit')]|.//button[contains(text(),'Edit')]|.//a[contains(@class,'btn-edit')]"));
        editButton.Click();
        WaitHelper.WaitForElement(Driver, EventModal);
        return this;
    }

    /// <summary>
    /// Clicks delete on an event row.
    /// </summary>
    public EventsManagementPage ClickDeleteEvent(string eventName)
    {
        var row = Driver.FindElements(EventRows)
            .FirstOrDefault(r => r.Text.Contains(eventName, StringComparison.OrdinalIgnoreCase));
        
        if (row == null)
            throw new NoSuchElementException($"Event '{eventName}' not found");

        var deleteButton = row.FindElement(By.XPath(".//button[@data-test='delete-btn']|.//button[contains(@class,'btn-delete')]|.//button[contains(text(),'Delete')]|.//button[contains(@class,'delete')]"));
        deleteButton.Click();
        return this;
    }

    /// <summary>
    /// Confirms deletion in the confirmation dialog.
    /// </summary>
    public EventsManagementPage ConfirmDelete()
    {
        var confirmButton = WaitHelper.WaitForClickable(
            Driver,
            By.XPath("//button[@data-test='confirm-delete']|//button[contains(@class,'btn-confirm')]|//button[contains(text(),'Confirm')]|//button[contains(text(),'Yes')]")
        );
        confirmButton.Click();
        WaitForLoadingToComplete();
        return this;
    }

    /// <summary>
    /// Checks if modal is displayed.
    /// </summary>
    public bool IsModalDisplayed()
        => IsDisplayed(EventModal);
}
