using FluentAssertions;
using RewardPointsSystem.E2ETests.Base;
using RewardPointsSystem.E2ETests.Helpers;
using RewardPointsSystem.E2ETests.PageObjects;
using Xunit;
using Xunit.Abstractions;

namespace RewardPointsSystem.E2ETests.Tests;

/// <summary>
/// E2E tests for authentication functionality.
/// Each test is independent and uses its own WebDriver instance.
/// </summary>
[Trait("Category", "Authentication")]
[Trait("Priority", "High")]
public class AuthenticationTests : BaseTest
{
    private readonly LoginPage _loginPage;

    public AuthenticationTests(ITestOutputHelper output) : base(output)
    {
        _loginPage = new LoginPage(Driver);
    }

    [Fact]
    [Trait("TestType", "Smoke")]
    public void LoginPage_ShouldLoad_Successfully()
    {
        ExecuteWithScreenshotOnFailure(() =>
        {
            // Arrange & Act
            _loginPage.GoTo();

            // Assert
            _loginPage.IsOnLoginPage().Should().BeTrue();
            _loginPage.IsLoginButtonEnabled().Should().BeFalse("button should be disabled until form is valid");
        });
    }

    [Fact]
    [Trait("TestType", "Smoke")]
    public void Login_WithValidAdminCredentials_RedirectsToAdminDashboard()
    {
        ExecuteWithScreenshotOnFailure(() =>
        {
            // Arrange
            _loginPage.GoTo();

            // Act
            var success = _loginPage.LoginAsAdmin(Config.AdminEmail, Config.AdminPassword);

            // Assert
            success.Should().BeTrue("admin should be redirected to admin dashboard");
            Driver.Url.Should().Contain("admin/dashboard");
        });
    }

    [Fact]
    [Trait("TestType", "Smoke")]
    public void Login_WithValidEmployeeCredentials_RedirectsToEmployeeDashboard()
    {
        ExecuteWithScreenshotOnFailure(() =>
        {
            // Arrange
            _loginPage.GoTo();

            // Act
            var success = _loginPage.LoginAsEmployee(Config.EmployeeEmail, Config.EmployeePassword);

            // Assert
            success.Should().BeTrue("employee should be redirected to employee dashboard");
            Driver.Url.Should().Contain("dashboard");
        });
    }

    [Fact]
    [Trait("TestType", "Functional")]
    public void Login_WithInvalidEmail_ShowsValidationError()
    {
        ExecuteWithScreenshotOnFailure(() =>
        {
            // Arrange
            _loginPage.GoTo();

            // Act - enter invalid email then tab away to trigger dirty state
            _loginPage.EnterEmail("invalid-email");
            // Tab to password field to trigger blur/dirty on email field
            var passwordField = WaitHelper.WaitForElement(Driver, OpenQA.Selenium.By.Id("password"));
            passwordField.Click();
            System.Threading.Thread.Sleep(500);

            // Assert
            var errors = _loginPage.GetValidationErrors();
            errors.Should().Contain(e => e.Contains("Invalid email", StringComparison.OrdinalIgnoreCase) ||
                                        e.Contains("email format", StringComparison.OrdinalIgnoreCase) ||
                                        e.Contains("invalid", StringComparison.OrdinalIgnoreCase));
        });
    }

    [Fact]
    [Trait("TestType", "Functional")]
    public void Login_WithNonAgdataEmail_ShowsAgdataEmailError()
    {
        ExecuteWithScreenshotOnFailure(() =>
        {
            // Arrange
            _loginPage.GoTo();

            // Act - enter non-agdata email, then tab away to trigger dirty state
            _loginPage.EnterEmail("test@gmail.com");
            var passwordField = WaitHelper.WaitForElement(Driver, OpenQA.Selenium.By.Id("password"));
            passwordField.Click();
            System.Threading.Thread.Sleep(500);

            // Assert
            var errors = _loginPage.GetValidationErrors();
            errors.Should().Contain(e => e.Contains("@agdata.com", StringComparison.OrdinalIgnoreCase) ||
                                        e.Contains("agdata", StringComparison.OrdinalIgnoreCase));
        });
    }

    [Fact]
    [Trait("TestType", "Functional")]
    public void Login_WithShortPassword_ShowsPasswordLengthError()
    {
        ExecuteWithScreenshotOnFailure(() =>
        {
            // Arrange
            _loginPage.GoTo();

            // Act - enter short password then click email to trigger blur/dirty
            _loginPage.EnterEmail("test@agdata.com");
            _loginPage.EnterPassword("short");
            // Click email field to trigger blur on password field
            var emailField = WaitHelper.WaitForElement(Driver, OpenQA.Selenium.By.Id("email"));
            emailField.Click();
            System.Threading.Thread.Sleep(500);

            // Assert
            var errors = _loginPage.GetValidationErrors();
            errors.Should().Contain(e => e.Contains("8", StringComparison.OrdinalIgnoreCase) ||
                                        e.Contains("characters", StringComparison.OrdinalIgnoreCase) ||
                                        e.Contains("at least", StringComparison.OrdinalIgnoreCase));
        });
    }

    [Fact]
    [Trait("TestType", "Functional")]
    public void Login_WithPasswordMissingRequirements_ShowsPatternError()
    {
        ExecuteWithScreenshotOnFailure(() =>
        {
            // Arrange
            _loginPage.GoTo();

            // Act - password with only lowercase (doesn't meet pattern requirements)
            _loginPage.EnterEmail("test@agdata.com");
            _loginPage.EnterPassword("alllowercase");
            
            // Click email field to trigger blur/dirty on password field
            var emailField = WaitHelper.WaitForElement(Driver, OpenQA.Selenium.By.Id("email"));
            emailField.Click();
            System.Threading.Thread.Sleep(500);

            // Assert - Check for pattern error about password requirements
            var errors = _loginPage.GetValidationErrors();
            errors.Should().Contain(e => 
                e.Contains("uppercase", StringComparison.OrdinalIgnoreCase) ||
                e.Contains("number", StringComparison.OrdinalIgnoreCase) ||
                e.Contains("special", StringComparison.OrdinalIgnoreCase) ||
                e.Contains("Password must include", StringComparison.OrdinalIgnoreCase) ||
                e.Contains("must include", StringComparison.OrdinalIgnoreCase));
        });
    }

    [Fact]
    [Trait("TestType", "Functional")]
    public void Login_WithInvalidCredentials_ShowsErrorMessage()
    {
        ExecuteWithScreenshotOnFailure(() =>
        {
            // Arrange
            _loginPage.GoTo();

            // Act
            _loginPage.Login("nonexistent@agdata.com", "WrongPass@123");

            // Assert - wait for error to appear
            WaitHelper.WaitForElement(Driver, 
                OpenQA.Selenium.By.CssSelector(".error, .error-list"), 
                TimeSpan.FromSeconds(10));
            
            _loginPage.HasError().Should().BeTrue();
        });
    }

    [Fact]
    [Trait("TestType", "Functional")]
    public void Login_WithEmptyFields_DisablesLoginButton()
    {
        ExecuteWithScreenshotOnFailure(() =>
        {
            // Arrange & Act
            _loginPage.GoTo();

            // Assert
            _loginPage.IsLoginButtonEnabled().Should().BeFalse();
        });
    }

    [Fact]
    [Trait("TestType", "Functional")]
    public void Login_WithValidFormInput_EnablesLoginButton()
    {
        ExecuteWithScreenshotOnFailure(() =>
        {
            // Arrange
            _loginPage.GoTo();

            // Act
            _loginPage.EnterEmail("test@agdata.com");
            _loginPage.EnterPassword("ValidPass@123");

            // Assert
            _loginPage.IsLoginButtonEnabled().Should().BeTrue();
        });
    }

    [Fact]
    [Trait("TestType", "Security")]
    public void Login_PasswordField_ShouldBeMasked()
    {
        ExecuteWithScreenshotOnFailure(() =>
        {
            // Arrange
            _loginPage.GoTo();

            // Act
            _loginPage.EnterPassword("TestPassword@123");

            // Assert - password field type should be password (masked)
            var passwordInput = WaitHelper.WaitForElement(Driver, OpenQA.Selenium.By.Id("password"));
            passwordInput.GetAttribute("type").Should().Be("password");
        });
    }

    [Fact]
    [Trait("TestType", "Functional")]
    public void LoginPage_FromProtectedRoute_RedirectsBackAfterLogin()
    {
        ExecuteWithScreenshotOnFailure(() =>
        {
            // Arrange - try to access protected route
            NavigateTo("employee/dashboard");

            // Should redirect to login
            WaitHelper.WaitForUrlContains(Driver, "login");
            _loginPage.IsOnLoginPage().Should().BeTrue();

            // Act - login
            var success = _loginPage.LoginAsEmployee(Config.EmployeeEmail, Config.EmployeePassword);

            // Assert - should redirect back to dashboard
            success.Should().BeTrue();
            Driver.Url.Should().Contain("dashboard");
        });
    }
}

/// <summary>
/// Cross-browser authentication tests.
/// Runs the same tests across different browsers.
/// </summary>
[Trait("Category", "CrossBrowser")]
[Trait("Priority", "Medium")]
[Collection("CrossBrowserTests")]
public class CrossBrowserAuthenticationTests : IDisposable
{
    private readonly ITestOutputHelper _output;
    private OpenQA.Selenium.IWebDriver? _driver;

    public CrossBrowserAuthenticationTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Theory]
    [InlineData("Chrome")]
    // Edge disabled - network blocks driver download from msedgedriver.azureedge.net
    // [InlineData("Edge")]
    [Trait("TestType", "CrossBrowser")]
    public void Login_ShouldWork_AcrossBrowsers(string browser)
    {
        // Arrange
        _driver = DriverFactory.CreateDriver(browser);
        var loginPage = new LoginPage(_driver);
        var config = TestConfiguration.Instance;

        try
        {
            // Act
            loginPage.GoTo();
            var success = loginPage.LoginAndWaitForDashboard(config.EmployeeEmail, config.EmployeePassword);

            // Assert
            success.Should().BeTrue($"login should work in {browser}");
        }
        finally
        {
            _driver?.Quit();
        }
    }

    public void Dispose()
    {
        _driver?.Quit();
        _driver?.Dispose();
    }
}
