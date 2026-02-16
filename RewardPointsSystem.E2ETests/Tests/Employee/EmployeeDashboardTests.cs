using FluentAssertions;
using RewardPointsSystem.E2ETests.Base;
using RewardPointsSystem.E2ETests.Helpers;
using RewardPointsSystem.E2ETests.PageObjects;
using RewardPointsSystem.E2ETests.PageObjects.Employee;
using Xunit;
using Xunit.Abstractions;

namespace RewardPointsSystem.E2ETests.Tests.Employee;

/// <summary>
/// E2E tests for Employee Dashboard functionality.
/// </summary>
[Trait("Category", "Employee")]
[Trait("Priority", "High")]
public class EmployeeDashboardTests : BaseTest
{
    private readonly LoginPage _loginPage;
    private readonly EmployeeDashboardPage _dashboardPage;

    public EmployeeDashboardTests(ITestOutputHelper output) : base(output)
    {
        _loginPage = new LoginPage(Driver);
        _dashboardPage = new EmployeeDashboardPage(Driver);
    }

    private void LoginAsEmployee()
    {
        _loginPage.GoTo();
        _loginPage.LoginAsEmployee(Config.EmployeeEmail, Config.EmployeePassword);
    }

    [Fact]
    [Trait("TestType", "Smoke")]
    public void EmployeeDashboard_ShouldLoad_WithWelcomeMessage()
    {
        ExecuteWithScreenshotOnFailure(() =>
        {
            // Arrange & Act
            LoginAsEmployee();

            // Assert
            _dashboardPage.IsOnDashboard().Should().BeTrue();
            // Dashboard title is "Welcome back, {userName}!" 
            var welcomeMsg = _dashboardPage.GetWelcomeMessage();
            (welcomeMsg.Contains("Welcome", StringComparison.OrdinalIgnoreCase) ||
             welcomeMsg.Contains("Dashboard", StringComparison.OrdinalIgnoreCase))
                .Should().BeTrue($"expected welcome/dashboard title, got: '{welcomeMsg}'");
        });
    }

    [Fact]
    [Trait("TestType", "Functional")]
    public void EmployeeDashboard_ShouldDisplay_PointsBalance()
    {
        ExecuteWithScreenshotOnFailure(() =>
        {
            // Arrange & Act
            LoginAsEmployee();

            // Assert
            _dashboardPage.IsPointsBalanceDisplayed().Should().BeTrue();
        });
    }

    [Fact]
    [Trait("TestType", "Functional")]
    public void EmployeeDashboard_PointsSummary_ShowsAllCategories()
    {
        ExecuteWithScreenshotOnFailure(() =>
        {
            // Arrange & Act
            LoginAsEmployee();

            // Assert - Points should be non-negative
            var summary = _dashboardPage.GetPointsSummary();
            summary.Earned.Should().BeGreaterThanOrEqualTo(0);
            summary.Current.Should().BeGreaterThanOrEqualTo(0);
            summary.Pending.Should().BeGreaterThanOrEqualTo(0);
            summary.Redeemed.Should().BeGreaterThanOrEqualTo(0);
        });
    }

    [Fact]
    [Trait("TestType", "Navigation")]
    public void EmployeeDashboard_ClickEvents_NavigatesToEventsPage()
    {
        ExecuteWithScreenshotOnFailure(() =>
        {
            // Arrange
            LoginAsEmployee();

            // Act
            _dashboardPage.ClickEventsAction();

            // Assert
            Driver.Url.Should().Contain("events");
        });
    }

    [Fact]
    [Trait("TestType", "Navigation")]
    public void EmployeeDashboard_ClickProducts_NavigatesToProductsPage()
    {
        ExecuteWithScreenshotOnFailure(() =>
        {
            // Arrange
            LoginAsEmployee();

            // Act
            _dashboardPage.ClickProductsAction();

            // Assert
            Driver.Url.Should().Contain("products");
        });
    }

    [Fact]
    [Trait("TestType", "Navigation")]
    public void EmployeeDashboard_ClickHistory_NavigatesToAccountPage()
    {
        ExecuteWithScreenshotOnFailure(() =>
        {
            // Arrange
            LoginAsEmployee();

            // Act
            _dashboardPage.ClickHistoryAction();

            // Assert
            Driver.Url.Should().Contain("account");
        });
    }

    [Fact]
    [Trait("TestType", "Functional")]
    public void EmployeeDashboard_ShowsUpcomingEvents()
    {
        ExecuteWithScreenshotOnFailure(() =>
        {
            // Arrange & Act
            LoginAsEmployee();

            // Assert
            _dashboardPage.IsUpcomingEventsDisplayed().Should().BeTrue();
        });
    }
}
