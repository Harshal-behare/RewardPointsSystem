using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;
using WebDriverManager.Helpers;

namespace RewardPointsSystem.E2ETests.Base;

/// <summary>
/// Factory for creating WebDriver instances with cross-browser support.
/// Uses WebDriverManager for automatic driver version management.
/// </summary>
public static class DriverFactory
{
    private static readonly object _lock = new();

    /// <summary>
    /// Creates a WebDriver instance based on configuration.
    /// Thread-safe for parallel test execution.
    /// </summary>
    public static IWebDriver CreateDriver(string? browserOverride = null)
    {
        var config = TestConfiguration.Instance;
        var browser = browserOverride ?? config.Browser;
        var headless = config.Headless;

        IWebDriver driver = browser.ToLowerInvariant() switch
        {
            "chrome" => CreateChromeDriver(headless),
            "firefox" => CreateFirefoxDriver(headless),
            "edge" => CreateEdgeDriver(headless),
            _ => CreateChromeDriver(headless)
        };

        ConfigureDriver(driver, config);
        return driver;
    }

    private static IWebDriver CreateChromeDriver(bool headless)
    {
        lock (_lock)
        {
            new DriverManager().SetUpDriver(new ChromeConfig(), VersionResolveStrategy.MatchingBrowser);
        }

        var options = new ChromeOptions();
        
        // CI-friendly settings
        if (headless)
        {
            options.AddArgument("--headless=new");
        }
        
        // Common stability settings
        options.AddArguments(
            "--no-sandbox",
            "--disable-dev-shm-usage",
            "--disable-gpu",
            "--disable-extensions",
            "--disable-infobars",
            "--disable-notifications",
            "--disable-popup-blocking",
            "--start-maximized",
            "--window-size=1920,1080",
            "--ignore-certificate-errors",
            "--allow-insecure-localhost"
        );

        // Reduce flakiness
        options.AddUserProfilePreference("credentials_enable_service", false);
        options.AddUserProfilePreference("profile.password_manager_enabled", false);

        return new ChromeDriver(options);
    }

    private static IWebDriver CreateFirefoxDriver(bool headless)
    {
        lock (_lock)
        {
            new DriverManager().SetUpDriver(new FirefoxConfig());
        }

        var options = new FirefoxOptions();
        
        if (headless)
        {
            options.AddArgument("-headless");
        }
        
        options.AddArguments(
            "--width=1920",
            "--height=1080"
        );

        options.AcceptInsecureCertificates = true;

        return new FirefoxDriver(options);
    }

    private static IWebDriver CreateEdgeDriver(bool headless)
    {
        // Selenium 4.6+ has built-in Selenium Manager - no WebDriverManager needed
        var options = new EdgeOptions();
        
        if (headless)
        {
            options.AddArgument("--headless=new");
        }
        
        options.AddArguments(
            "--no-sandbox",
            "--disable-dev-shm-usage",
            "--disable-gpu",
            "--start-maximized",
            "--window-size=1920,1080",
            "--ignore-certificate-errors",
            "--inprivate"
        );

        return new EdgeDriver(options);
    }

    private static void ConfigureDriver(IWebDriver driver, TestConfiguration config)
    {
        // Set timeouts
        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(config.ImplicitWaitSeconds);
        driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(config.PageLoadTimeoutSeconds);
        driver.Manage().Timeouts().AsynchronousJavaScript = TimeSpan.FromSeconds(config.ExplicitWaitSeconds);
        
        // Maximize window for consistent viewport
        try
        {
            driver.Manage().Window.Maximize();
        }
        catch
        {
            // Headless mode may not support maximize
            driver.Manage().Window.Size = new System.Drawing.Size(1920, 1080);
        }
    }
}
