using OpenQA.Selenium;
using RewardPointsSystem.E2ETests.Helpers;

namespace RewardPointsSystem.E2ETests.PageObjects.Employee;

/// <summary>
/// Page Object for the Employee Events page.
/// </summary>
public class EventsPage : BasePage
{
    // Locators - based on actual frontend HTML structure
    private static readonly By PageHeader = By.CssSelector("h1, .page-header h1");
    private static readonly By EventCards = By.CssSelector(".event-card, article.event-card");
    private static readonly By SearchInput = By.CssSelector(".search input, input[placeholder*='Search']");
    private static readonly By FilterTabs = By.CssSelector(".tabs .tab, .filter-bar .tab");
    private static readonly By StatFilters = By.CssSelector(".header-stats .stat");
    
    // Event card elements
    private static readonly By EventName = By.CssSelector(".event-name, .event-title, h3");
    private static readonly By EventDate = By.CssSelector(".event-date, .date");
    private static readonly By EventPoints = By.CssSelector(".event-points, .points, .reward-points");
    private static readonly By RegisterButton = By.XPath(".//button[@data-test='register-btn']|.//button[contains(@class,'btn-register')]|.//button[contains(text(),'Register')]");
    private static readonly By RegisteredBadge = By.CssSelector("[data-test='registered-badge'], .registered, .badge-success");
    
    // Modal
    private static readonly By EventModal = By.CssSelector("[data-test='event-modal'], .modal, .dialog");
    private static readonly By ConfirmRegisterButton = By.CssSelector("[data-test='confirm-register'], .btn-confirm");

    public EventsPage(IWebDriver driver) : base(driver) { }

    /// <summary>
    /// Navigates to the events page.
    /// </summary>
    public EventsPage GoTo()
    {
        NavigateTo("employee/events");
        return this;
    }

    /// <summary>
    /// Checks if on events page.
    /// </summary>
    public bool IsOnPage()
        => CurrentUrl.Contains("events");

    /// <summary>
    /// Gets the count of event cards.
    /// </summary>
    public int GetEventCount()
        => Driver.FindElements(EventCards).Count;

    /// <summary>
    /// Searches for events.
    /// </summary>
    public EventsPage SearchEvents(string searchTerm)
    {
        TypeText(SearchInput, searchTerm);
        WaitForLoadingToComplete();
        return this;
    }

    /// <summary>
    /// Finds an event card by name.
    /// </summary>
    public IWebElement? FindEventCard(string eventName)
    {
        var cards = Driver.FindElements(EventCards);
        return cards.FirstOrDefault(c => c.Text.Contains(eventName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Checks if an event exists.
    /// </summary>
    public bool EventExists(string eventName)
        => FindEventCard(eventName) != null;

    /// <summary>
    /// Registers for an event.
    /// </summary>
    public EventsPage RegisterForEvent(string eventName)
    {
        var card = FindEventCard(eventName) 
            ?? throw new NoSuchElementException($"Event '{eventName}' not found");
        
        var registerButton = card.FindElement(RegisterButton);
        ScrollToElement(registerButton);
        registerButton.Click();
        
        // Handle confirmation modal if present
        try
        {
            var confirm = WaitHelper.WaitForClickable(Driver, ConfirmRegisterButton, TimeSpan.FromSeconds(3));
            confirm.Click();
        }
        catch { }
        
        WaitForLoadingToComplete();
        return this;
    }

    /// <summary>
    /// Checks if already registered for an event.
    /// </summary>
    public bool IsRegisteredForEvent(string eventName)
    {
        var card = FindEventCard(eventName);
        if (card == null) return false;
        
        try
        {
            return card.FindElements(RegisteredBadge).Count > 0 
                || card.Text.Contains("Registered", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets event details from a card.
    /// </summary>
    public (string Name, string Date, int Points) GetEventDetails(string eventName)
    {
        var card = FindEventCard(eventName) 
            ?? throw new NoSuchElementException($"Event '{eventName}' not found");
        
        var name = card.FindElements(EventName).FirstOrDefault()?.Text ?? eventName;
        var date = card.FindElements(EventDate).FirstOrDefault()?.Text ?? string.Empty;
        var pointsText = card.FindElements(EventPoints).FirstOrDefault()?.Text ?? "0";
        var points = int.TryParse(new string(pointsText.Where(char.IsDigit).ToArray()), out var p) ? p : 0;
        
        return (name, date, points);
    }

    /// <summary>
    /// Clicks on an event card to view details.
    /// </summary>
    public EventsPage ClickEventCard(string eventName)
    {
        var card = FindEventCard(eventName) 
            ?? throw new NoSuchElementException($"Event '{eventName}' not found");
        
        card.Click();
        return this;
    }

    /// <summary>
    /// Gets list of all event names.
    /// </summary>
    public List<string> GetAllEventNames()
    {
        var cards = Driver.FindElements(EventCards);
        return cards
            .Select(c => c.FindElements(EventName).FirstOrDefault()?.Text ?? c.Text.Split('\n').FirstOrDefault() ?? string.Empty)
            .Where(n => !string.IsNullOrEmpty(n))
            .ToList();
    }
}
