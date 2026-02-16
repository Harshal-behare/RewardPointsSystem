using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace RewardPointsSystem.E2ETests.Helpers;

/// <summary>
/// Explicit wait helper utilities to avoid flaky tests.
/// Never use Thread.Sleep - always use explicit waits.
/// </summary>
public static class WaitHelper
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan DefaultPollingInterval = TimeSpan.FromMilliseconds(250);

    /// <summary>
    /// Waits for an element to be visible on the page.
    /// </summary>
    public static IWebElement WaitForElement(
        IWebDriver driver,
        By locator,
        TimeSpan? timeout = null)
    {
        var wait = CreateWait(driver, timeout);
        return wait.Until(d =>
        {
            try
            {
                var element = d.FindElement(locator);
                return element.Displayed ? element : null!;
            }
            catch (NoSuchElementException)
            {
                return null!;
            }
            catch (StaleElementReferenceException)
            {
                return null!;
            }
        });
    }

    /// <summary>
    /// Waits for an element to be clickable.
    /// </summary>
    public static IWebElement WaitForClickable(
        IWebDriver driver,
        By locator,
        TimeSpan? timeout = null)
    {
        var wait = CreateWait(driver, timeout);
        return wait.Until(d =>
        {
            try
            {
                var element = d.FindElement(locator);
                return element.Displayed && element.Enabled ? element : null!;
            }
            catch (NoSuchElementException)
            {
                return null!;
            }
            catch (StaleElementReferenceException)
            {
                return null!;
            }
        });
    }

    /// <summary>
    /// Waits for an element to disappear from the page.
    /// </summary>
    public static bool WaitForElementToDisappear(
        IWebDriver driver,
        By locator,
        TimeSpan? timeout = null)
    {
        var wait = CreateWait(driver, timeout);
        try
        {
            return wait.Until(d =>
            {
                try
                {
                    var element = d.FindElement(locator);
                    return !element.Displayed;
                }
                catch (NoSuchElementException)
                {
                    return true;
                }
                catch (StaleElementReferenceException)
                {
                    return true;
                }
            });
        }
        catch (WebDriverTimeoutException)
        {
            return false;
        }
    }

    /// <summary>
    /// Waits for text to be present in an element.
    /// </summary>
    public static bool WaitForText(
        IWebDriver driver,
        By locator,
        string text,
        TimeSpan? timeout = null)
    {
        var wait = CreateWait(driver, timeout);
        return wait.Until(d =>
        {
            try
            {
                var element = d.FindElement(locator);
                return element.Text.Contains(text, StringComparison.OrdinalIgnoreCase);
            }
            catch (NoSuchElementException)
            {
                return false;
            }
            catch (StaleElementReferenceException)
            {
                return false;
            }
        });
    }

    /// <summary>
    /// Waits for the page to fully load (document.readyState === 'complete').
    /// </summary>
    public static void WaitForPageLoad(IWebDriver driver, TimeSpan? timeout = null)
    {
        var wait = CreateWait(driver, timeout);
        wait.Until(d =>
        {
            var js = (IJavaScriptExecutor)d;
            return js.ExecuteScript("return document.readyState").ToString() == "complete";
        });
    }

    /// <summary>
    /// Waits for Angular to finish all pending HTTP requests and rendering.
    /// </summary>
    public static void WaitForAngular(IWebDriver driver, TimeSpan? timeout = null)
    {
        var wait = CreateWait(driver, timeout ?? TimeSpan.FromSeconds(60));
        
        // Wait for Angular to be defined and stable
        wait.Until(d =>
        {
            var js = (IJavaScriptExecutor)d;
            try
            {
                // Check if Angular is present and stable
                var script = @"
                    if (window.getAllAngularTestabilities) {
                        var testabilities = window.getAllAngularTestabilities();
                        if (testabilities && testabilities.length > 0) {
                            return testabilities.every(function(t) { return t.isStable(); });
                        }
                    }
                    return true;
                ";
                var result = js.ExecuteScript(script);
                return result != null && (bool)result;
            }
            catch
            {
                return true; // If Angular check fails, assume ready
            }
        });
    }

    /// <summary>
    /// Waits for a loading spinner to disappear.
    /// </summary>
    public static void WaitForLoadingComplete(
        IWebDriver driver,
        By? spinnerLocator = null,
        TimeSpan? timeout = null)
    {
        var locator = spinnerLocator ?? By.CssSelector("[data-test='loading-spinner'], .loading, .spinner");
        WaitForElementToDisappear(driver, locator, timeout);
    }

    /// <summary>
    /// Waits for URL to contain a specific text.
    /// </summary>
    public static bool WaitForUrlContains(
        IWebDriver driver,
        string urlPart,
        TimeSpan? timeout = null)
    {
        var wait = CreateWait(driver, timeout);
        return wait.Until(d => d.Url.Contains(urlPart, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Waits for a condition to be true.
    /// </summary>
    public static T WaitFor<T>(
        IWebDriver driver,
        Func<IWebDriver, T> condition,
        TimeSpan? timeout = null)
    {
        var wait = CreateWait(driver, timeout);
        return wait.Until(condition);
    }

    /// <summary>
    /// Waits for an element to have a specific attribute value.
    /// </summary>
    public static bool WaitForAttribute(
        IWebDriver driver,
        By locator,
        string attributeName,
        string expectedValue,
        TimeSpan? timeout = null)
    {
        var wait = CreateWait(driver, timeout);
        return wait.Until(d =>
        {
            try
            {
                var element = d.FindElement(locator);
                var actualValue = element.GetAttribute(attributeName);
                return actualValue?.Equals(expectedValue, StringComparison.OrdinalIgnoreCase) ?? false;
            }
            catch
            {
                return false;
            }
        });
    }

    /// <summary>
    /// Retry an action with exponential backoff.
    /// </summary>
    public static T RetryOnException<T>(
        Func<T> action,
        int maxRetries = 3,
        int baseDelayMs = 100)
    {
        Exception? lastException = null;
        
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                return action();
            }
            catch (StaleElementReferenceException ex)
            {
                lastException = ex;
                Thread.Sleep(baseDelayMs * (int)Math.Pow(2, i));
            }
            catch (ElementClickInterceptedException ex)
            {
                lastException = ex;
                Thread.Sleep(baseDelayMs * (int)Math.Pow(2, i));
            }
        }
        
        throw lastException!;
    }

    private static WebDriverWait CreateWait(IWebDriver driver, TimeSpan? timeout = null)
    {
        var wait = new WebDriverWait(driver, timeout ?? DefaultTimeout)
        {
            PollingInterval = DefaultPollingInterval
        };
        
        wait.IgnoreExceptionTypes(
            typeof(NoSuchElementException),
            typeof(StaleElementReferenceException),
            typeof(ElementNotInteractableException)
        );
        
        return wait;
    }
}
