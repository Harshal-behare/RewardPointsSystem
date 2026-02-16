using OpenQA.Selenium;

namespace RewardPointsSystem.E2ETests.Helpers;

/// <summary>
/// Helper for capturing screenshots during test execution.
/// Screenshots are organized by date and test name for easy debugging.
/// </summary>
public static class ScreenshotHelper
{
    /// <summary>
    /// Captures a screenshot and saves it to the specified directory.
    /// </summary>
    /// <param name="driver">WebDriver instance</param>
    /// <param name="testName">Name of the test for filename</param>
    /// <param name="baseDirectory">Base directory for screenshots</param>
    /// <param name="suffix">Optional suffix for the filename</param>
    /// <returns>Full path to the saved screenshot</returns>
    public static string CaptureScreenshot(
        IWebDriver driver,
        string testName,
        string baseDirectory,
        string? suffix = null)
    {
        try
        {
            // Ensure directory exists
            var dateFolder = DateTime.Now.ToString("yyyy-MM-dd");
            var fullPath = Path.Combine(baseDirectory, dateFolder);
            Directory.CreateDirectory(fullPath);

            // Generate unique filename
            var timestamp = DateTime.Now.ToString("HHmmss_fff");
            var sanitizedTestName = SanitizeFileName(testName);
            var fileName = string.IsNullOrEmpty(suffix)
                ? $"{sanitizedTestName}_{timestamp}.png"
                : $"{sanitizedTestName}_{suffix}_{timestamp}.png";

            var filePath = Path.Combine(fullPath, fileName);

            // Capture screenshot
            var screenshot = ((ITakesScreenshot)driver).GetScreenshot();
            screenshot.SaveAsFile(filePath);

            return filePath;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to capture screenshot: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Captures a screenshot of a specific element.
    /// </summary>
    public static string CaptureElementScreenshot(
        IWebDriver driver,
        IWebElement element,
        string testName,
        string baseDirectory)
    {
        try
        {
            var dateFolder = DateTime.Now.ToString("yyyy-MM-dd");
            var fullPath = Path.Combine(baseDirectory, dateFolder);
            Directory.CreateDirectory(fullPath);

            var timestamp = DateTime.Now.ToString("HHmmss_fff");
            var sanitizedTestName = SanitizeFileName(testName);
            var fileName = $"{sanitizedTestName}_element_{timestamp}.png";
            var filePath = Path.Combine(fullPath, fileName);

            var screenshot = ((ITakesScreenshot)element).GetScreenshot();
            screenshot.SaveAsFile(filePath);

            return filePath;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to capture element screenshot: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Captures a full page screenshot by scrolling through the page.
    /// </summary>
    public static List<string> CaptureFullPageScreenshots(
        IWebDriver driver,
        string testName,
        string baseDirectory)
    {
        var screenshots = new List<string>();
        var js = (IJavaScriptExecutor)driver;
        
        // Get page dimensions
        var totalHeight = Convert.ToInt64(js.ExecuteScript("return document.body.scrollHeight"));
        var viewportHeight = Convert.ToInt64(js.ExecuteScript("return window.innerHeight"));
        
        long currentPosition = 0;
        int partNumber = 1;

        while (currentPosition < totalHeight)
        {
            js.ExecuteScript($"window.scrollTo(0, {currentPosition})");
            Thread.Sleep(100); // Brief pause for rendering
            
            var path = CaptureScreenshot(driver, testName, baseDirectory, $"part{partNumber}");
            screenshots.Add(path);
            
            currentPosition += viewportHeight;
            partNumber++;
        }

        // Scroll back to top
        js.ExecuteScript("window.scrollTo(0, 0)");
        
        return screenshots;
    }

    private static string SanitizeFileName(string fileName)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return string.Join("_", fileName.Split(invalid, StringSplitOptions.RemoveEmptyEntries));
    }
}
