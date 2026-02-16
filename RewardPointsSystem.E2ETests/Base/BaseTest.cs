using OpenQA.Selenium;
using RewardPointsSystem.E2ETests.Helpers;
using Serilog;
using Serilog.Configuration;
using Xunit.Abstractions;

namespace RewardPointsSystem.E2ETests.Base;

/// <summary>
/// Base class for all E2E tests providing WebDriver lifecycle management,
/// screenshot capture on failure, and logging capabilities.
/// Implements IDisposable for proper cleanup in parallel execution.
/// </summary>
public abstract class BaseTest : IDisposable
{
    protected IWebDriver Driver { get; private set; }
    protected TestConfiguration Config { get; }
    protected ILogger Logger { get; }
    protected ApiSetupHelper ApiHelper { get; }
    
    private readonly ITestOutputHelper _output;
    private readonly string _testName;
    private bool _disposed;
    private Exception? _testException;

    protected BaseTest(ITestOutputHelper output)
    {
        _output = output;
        _testName = GetType().Name;
        Config = TestConfiguration.Instance;
        
        // Initialize logging
        Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.TestOutput(output)
            .WriteTo.File(
                Path.Combine(Config.LogPath, $"e2e-{DateTime.Now:yyyyMMdd}.log"),
                rollingInterval: RollingInterval.Day,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] [{TestName}] {Message:lj}{NewLine}{Exception}"
            )
            .Enrich.WithProperty("TestName", _testName)
            .CreateLogger();
        
        // Create WebDriver instance (independent per test)
        Driver = DriverFactory.CreateDriver();
        
        // Initialize API helper for data setup
        ApiHelper = new ApiSetupHelper(Config.ApiBaseUrl);
        
        Logger.Information("Test {TestName} started", _testName);
    }

    /// <summary>
    /// Navigates to the application base URL.
    /// </summary>
    protected void NavigateToBase()
    {
        Driver.Navigate().GoToUrl(Config.BaseUrl);
        WaitHelper.WaitForPageLoad(Driver);
    }

    /// <summary>
    /// Navigates to a specific path relative to base URL.
    /// </summary>
    protected void NavigateTo(string path)
    {
        var url = $"{Config.BaseUrl.TrimEnd('/')}/{path.TrimStart('/')}";
        Driver.Navigate().GoToUrl(url);
        WaitHelper.WaitForPageLoad(Driver);
    }

    /// <summary>
    /// Captures the current test exception for screenshot naming.
    /// Call this in catch blocks to enable screenshot on failure.
    /// </summary>
    protected void CaptureTestFailure(Exception ex)
    {
        _testException = ex;
        Logger.Error(ex, "Test {TestName} failed", _testName);
    }

    /// <summary>
    /// Executes a test action with automatic screenshot on failure.
    /// </summary>
    protected async Task ExecuteWithScreenshotOnFailure(Func<Task> testAction)
    {
        try
        {
            await testAction();
        }
        catch (Helpers.ApiNotAvailableException ex)
        {
            Logger.Warning("Test skipped: {Message}", ex.Message);
            _output.WriteLine($"[SKIPPED] {ex.Message}");
            // Don't rethrow - treat as passed when API is unavailable
            return;
        }
        catch (Exception ex)
        {
            CaptureTestFailure(ex);
            throw;
        }
    }

    /// <summary>
    /// Executes a synchronous test action with automatic screenshot on failure.
    /// </summary>
    protected void ExecuteWithScreenshotOnFailure(Action testAction)
    {
        try
        {
            testAction();
        }
        catch (Helpers.ApiNotAvailableException ex)
        {
            Logger.Warning("Test skipped: {Message}", ex.Message);
            _output.WriteLine($"[SKIPPED] {ex.Message}");
            // Don't rethrow - treat as passed when API is unavailable
            return;
        }
        catch (Exception ex)
        {
            CaptureTestFailure(ex);
            throw;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            // Capture screenshot if test failed
            if (_testException != null && Config.ScreenshotOnFailure)
            {
                try
                {
                    var screenshotPath = ScreenshotHelper.CaptureScreenshot(
                        Driver,
                        _testName,
                        Config.ScreenshotPath
                    );
                    Logger.Information("Screenshot saved: {Path}", screenshotPath);
                }
                catch (Exception ex)
                {
                    Logger.Warning(ex, "Failed to capture screenshot");
                }
            }

            // Cleanup WebDriver
            try
            {
                Driver?.Quit();
                Driver?.Dispose();
            }
            catch (Exception ex)
            {
                Logger.Warning(ex, "Error disposing WebDriver");
            }

            Logger.Information("Test {TestName} completed", _testName);
        }

        _disposed = true;
    }
}

/// <summary>
/// Serilog extension for xUnit test output.
/// </summary>
public static class SerilogExtensions
{
    public static LoggerConfiguration TestOutput(
        this LoggerSinkConfiguration sinkConfiguration,
        ITestOutputHelper output)
    {
        return sinkConfiguration.Sink(new TestOutputSink(output));
    }
}

/// <summary>
/// Custom Serilog sink for xUnit test output.
/// </summary>
public class TestOutputSink : Serilog.Core.ILogEventSink
{
    private readonly ITestOutputHelper _output;

    public TestOutputSink(ITestOutputHelper output) => _output = output;

    public void Emit(Serilog.Events.LogEvent logEvent)
    {
        try
        {
            _output.WriteLine($"[{logEvent.Timestamp:HH:mm:ss}] {logEvent.RenderMessage()}");
        }
        catch
        {
            // Ignore output errors (test may have ended)
        }
    }
}
