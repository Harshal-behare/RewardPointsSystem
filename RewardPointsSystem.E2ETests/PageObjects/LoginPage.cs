using OpenQA.Selenium;
using RewardPointsSystem.E2ETests.Helpers;

namespace RewardPointsSystem.E2ETests.PageObjects;

/// <summary>
/// Page Object for the Login page.
/// Uses stable selectors (ID, data-test attributes).
/// </summary>
public class LoginPage : BasePage
{
    // Locators - using IDs where available, fallback to data-test
    private static readonly By EmailInput = By.Id("email");
    private static readonly By PasswordInput = By.Id("password");
    private static readonly By LoginButton = By.CssSelector("button.login-btn");
    private static readonly By ErrorMessage = By.CssSelector(".error");
    private static readonly By ErrorList = By.CssSelector(".error-list");
    private static readonly By LoadingIndicator = By.CssSelector("button.login-btn[disabled]");

    public LoginPage(IWebDriver driver) : base(driver) { }

    /// <summary>
    /// Navigates to the login page.
    /// </summary>
    public LoginPage GoTo()
    {
        NavigateTo("login");
        return this;
    }

    /// <summary>
    /// Enters email in the email field.
    /// </summary>
    public LoginPage EnterEmail(string email)
    {
        TypeText(EmailInput, email);
        return this;
    }

    /// <summary>
    /// Enters password in the password field.
    /// </summary>
    public LoginPage EnterPassword(string password)
    {
        TypeText(PasswordInput, password);
        return this;
    }

    /// <summary>
    /// Clicks the login button.
    /// </summary>
    public LoginPage ClickLogin()
    {
        SafeClick(LoginButton);
        return this;
    }

    /// <summary>
    /// Performs complete login action.
    /// </summary>
    public void Login(string email, string password)
    {
        EnterEmail(email);
        EnterPassword(password);
        ClickLogin();
        WaitForLoadingToComplete();
    }

    /// <summary>
    /// Performs login and waits for successful redirect to dashboard.
    /// </summary>
    public bool LoginAndWaitForDashboard(string email, string password)
    {
        Login(email, password);
        return WaitForNavigation("dashboard", TimeSpan.FromSeconds(15));
    }

    /// <summary>
    /// Performs login as admin and navigates to admin dashboard.
    /// </summary>
    public bool LoginAsAdmin(string email, string password)
    {
        Login(email, password);
        return WaitForNavigation("admin/dashboard", TimeSpan.FromSeconds(15));
    }

    /// <summary>
    /// Performs login as employee and navigates to employee dashboard.
    /// </summary>
    public bool LoginAsEmployee(string email, string password)
    {
        Login(email, password);
        return WaitForNavigation("employee/dashboard", TimeSpan.FromSeconds(15));
    }

    /// <summary>
    /// Gets the error message displayed on the page.
    /// </summary>
    public string GetErrorMessage()
    {
        try
        {
            WaitHelper.WaitForElement(Driver, ErrorMessage, TimeSpan.FromSeconds(5));
            return GetText(ErrorMessage);
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Gets all validation errors displayed.
    /// </summary>
    public List<string> GetValidationErrors()
    {
        try
        {
            // Wait briefly for validation to appear after field interaction
            System.Threading.Thread.Sleep(300);
            var errorElements = Driver.FindElements(By.CssSelector(".error span, .error-list .error, .error"));
            return errorElements.Select(e => e.Text).Where(t => !string.IsNullOrEmpty(t)).ToList();
        }
        catch
        {
            return new List<string>();
        }
    }

    /// <summary>
    /// Checks if error message is displayed.
    /// </summary>
    public bool HasError()
        => IsDisplayed(ErrorMessage) || IsDisplayed(ErrorList);

    /// <summary>
    /// Checks if the login button is enabled.
    /// </summary>
    public bool IsLoginButtonEnabled()
    {
        var button = WaitHelper.WaitForElement(Driver, LoginButton);
        return button.Enabled && !button.GetAttribute("class")?.Contains("disabled") == true;
    }

    /// <summary>
    /// Checks if currently loading.
    /// </summary>
    public bool IsLoading()
    {
        var button = WaitHelper.WaitForElement(Driver, LoginButton);
        return button.Text.Contains("Signing in");
    }

    /// <summary>
    /// Gets email field value.
    /// </summary>
    public string GetEmailValue()
        => GetInputValue(EmailInput);

    /// <summary>
    /// Gets password field value (masked).
    /// </summary>
    public string GetPasswordValue()
        => GetInputValue(PasswordInput);

    /// <summary>
    /// Verifies if on login page.
    /// </summary>
    public bool IsOnLoginPage()
        => CurrentUrl.Contains("/login") && IsDisplayed(LoginButton);
}
