using OpenQA.Selenium;
using RewardPointsSystem.E2ETests.Helpers;

namespace RewardPointsSystem.E2ETests.PageObjects;

/// <summary>
/// Base class for all Page Objects providing common functionality.
/// Implements lightweight POM with DRY principles.
/// </summary>
public abstract class BasePage
{
    protected readonly IWebDriver Driver;
    protected readonly string BaseUrl;

    protected BasePage(IWebDriver driver)
    {
        Driver = driver;
        BaseUrl = Base.TestConfiguration.Instance.BaseUrl;
    }

    /// <summary>
    /// Current page URL.
    /// </summary>
    public string CurrentUrl => Driver.Url;

    /// <summary>
    /// Page title.
    /// </summary>
    public string Title => Driver.Title;

    /// <summary>
    /// Navigates to a relative path.
    /// </summary>
    protected void NavigateTo(string path)
    {
        Driver.Navigate().GoToUrl($"{BaseUrl.TrimEnd('/')}/{path.TrimStart('/')}");
        WaitForPageReady();
    }

    /// <summary>
    /// Waits for the page to be fully loaded.
    /// </summary>
    protected void WaitForPageReady()
    {
        WaitHelper.WaitForPageLoad(Driver);
        WaitHelper.WaitForAngular(Driver);
    }

    /// <summary>
    /// Finds element by data-test attribute (preferred stable locator).
    /// </summary>
    protected IWebElement FindByTestId(string testId)
        => WaitHelper.WaitForElement(Driver, ByTestId(testId));

    /// <summary>
    /// Finds element by data-test attribute, returns null if not found.
    /// </summary>
    protected IWebElement? TryFindByTestId(string testId, TimeSpan? timeout = null)
    {
        try
        {
            return WaitHelper.WaitForElement(Driver, ByTestId(testId), timeout ?? TimeSpan.FromSeconds(5));
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Creates By locator for data-test attribute.
    /// </summary>
    protected static By ByTestId(string testId)
        => By.CssSelector($"[data-test='{testId}']");

    /// <summary>
    /// Finds clickable element by data-test attribute.
    /// </summary>
    protected IWebElement FindClickableByTestId(string testId)
        => WaitHelper.WaitForClickable(Driver, ByTestId(testId));

    /// <summary>
    /// Clicks an element safely with retry logic.
    /// </summary>
    protected void SafeClick(By locator)
    {
        WaitHelper.RetryOnException(() =>
        {
            var element = WaitHelper.WaitForClickable(Driver, locator);
            element.Click();
            return true;
        });
    }

    /// <summary>
    /// Clicks an element by test ID safely.
    /// </summary>
    protected void ClickByTestId(string testId)
        => SafeClick(ByTestId(testId));

    /// <summary>
    /// Types text into an input field with clear.
    /// </summary>
    protected void TypeText(By locator, string text, bool clearFirst = true)
    {
        var element = WaitHelper.WaitForElement(Driver, locator);
        if (clearFirst)
        {
            element.Clear();
        }
        element.SendKeys(text);
    }

    /// <summary>
    /// Types text into an input by test ID.
    /// </summary>
    protected void TypeByTestId(string testId, string text, bool clearFirst = true)
        => TypeText(ByTestId(testId), text, clearFirst);

    /// <summary>
    /// Gets text content of an element.
    /// </summary>
    protected string GetText(By locator)
        => WaitHelper.WaitForElement(Driver, locator).Text;

    /// <summary>
    /// Gets text by test ID.
    /// </summary>
    protected string GetTextByTestId(string testId)
        => GetText(ByTestId(testId));

    /// <summary>
    /// Checks if element is displayed.
    /// </summary>
    protected bool IsDisplayed(By locator)
    {
        try
        {
            return Driver.FindElement(locator).Displayed;
        }
        catch (NoSuchElementException)
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if element by test ID is displayed.
    /// </summary>
    protected bool IsDisplayedByTestId(string testId)
        => IsDisplayed(ByTestId(testId));

    /// <summary>
    /// Waits for navigation to a specific URL path.
    /// </summary>
    protected bool WaitForNavigation(string urlPart, TimeSpan? timeout = null)
        => WaitHelper.WaitForUrlContains(Driver, urlPart, timeout);

    /// <summary>
    /// Scrolls element into view.
    /// </summary>
    protected void ScrollToElement(IWebElement element)
    {
        var js = (IJavaScriptExecutor)Driver;
        js.ExecuteScript("arguments[0].scrollIntoView({behavior: 'smooth', block: 'center'});", element);
    }

    /// <summary>
    /// Scrolls to element by test ID.
    /// </summary>
    protected void ScrollToTestId(string testId)
    {
        var element = FindByTestId(testId);
        ScrollToElement(element);
    }

    /// <summary>
    /// Gets value of an input field.
    /// </summary>
    protected string GetInputValue(By locator)
        => WaitHelper.WaitForElement(Driver, locator).GetAttribute("value") ?? string.Empty;

    /// <summary>
    /// Gets input value by test ID.
    /// </summary>
    protected string GetInputValueByTestId(string testId)
        => GetInputValue(ByTestId(testId));

    /// <summary>
    /// Waits for a toast/snackbar message.
    /// </summary>
    protected string? WaitForToastMessage(TimeSpan? timeout = null)
    {
        try
        {
            var toast = WaitHelper.WaitForElement(
                Driver,
                By.CssSelector("[data-test='toast-message'], .toast, .snackbar, .notification"),
                timeout ?? TimeSpan.FromSeconds(10)
            );
            return toast.Text;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Waits for loading indicator to disappear.
    /// </summary>
    protected void WaitForLoadingToComplete()
    {
        WaitHelper.WaitForLoadingComplete(Driver);
    }
}
