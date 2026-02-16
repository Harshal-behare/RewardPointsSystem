using FluentAssertions;
using RewardPointsSystem.E2ETests.Base;
using RewardPointsSystem.E2ETests.Helpers;
using RewardPointsSystem.E2ETests.PageObjects;
using RewardPointsSystem.E2ETests.PageObjects.Admin;
using Xunit;
using Xunit.Abstractions;

namespace RewardPointsSystem.E2ETests.Tests.Admin;

/// <summary>
/// E2E tests for Admin Dashboard functionality.
/// </summary>
[Trait("Category", "Admin")]
[Trait("Priority", "High")]
public class AdminDashboardTests : BaseTest
{
    private readonly LoginPage _loginPage;
    private readonly AdminDashboardPage _dashboardPage;

    public AdminDashboardTests(ITestOutputHelper output) : base(output)
    {
        _loginPage = new LoginPage(Driver);
        _dashboardPage = new AdminDashboardPage(Driver);
    }

    private void LoginAsAdmin()
    {
        _loginPage.GoTo();
        _loginPage.LoginAsAdmin(Config.AdminEmail, Config.AdminPassword);
    }

    [Fact]
    [Trait("TestType", "Smoke")]
    public void AdminDashboard_ShouldLoad_WithCorrectHeader()
    {
        ExecuteWithScreenshotOnFailure(() =>
        {
            // Arrange & Act
            LoginAsAdmin();

            // Assert
            _dashboardPage.IsOnDashboard().Should().BeTrue();
            _dashboardPage.GetHeaderText().Should().Contain("Admin Dashboard");
        });
    }

    [Fact]
    [Trait("TestType", "Functional")]
    public void AdminDashboard_ShouldDisplay_KPICards()
    {
        ExecuteWithScreenshotOnFailure(() =>
        {
            // Arrange & Act
            LoginAsAdmin();

            // Assert
            _dashboardPage.GetKpiCardCount().Should().BeGreaterThan(0);
        });
    }

    [Fact]
    [Trait("TestType", "Functional")]
    public void AdminDashboard_ShouldDisplay_QuickActions()
    {
        ExecuteWithScreenshotOnFailure(() =>
        {
            // Arrange & Act
            LoginAsAdmin();

            // Assert
            _dashboardPage.GetQuickActionCount().Should().BeGreaterThanOrEqualTo(4);
        });
    }

    [Fact]
    [Trait("TestType", "Functional")]
    public void AdminDashboard_ShouldDisplay_BudgetSection()
    {
        ExecuteWithScreenshotOnFailure(() =>
        {
            // Arrange & Act
            LoginAsAdmin();

            // Assert
            _dashboardPage.IsBudgetSectionDisplayed().Should().BeTrue();
        });
    }

    [Fact]
    [Trait("TestType", "Navigation")]
    public void AdminDashboard_ClickRedemptions_NavigatesToRedemptionsPage()
    {
        ExecuteWithScreenshotOnFailure(() =>
        {
            // Arrange
            LoginAsAdmin();

            // Act
            _dashboardPage.ClickRedemptionsAction();

            // Assert
            Driver.Url.Should().Contain("redemptions");
        });
    }

    [Fact]
    [Trait("TestType", "Navigation")]
    public void AdminDashboard_ClickUsers_NavigatesToUsersPage()
    {
        ExecuteWithScreenshotOnFailure(() =>
        {
            // Arrange
            LoginAsAdmin();

            // Act
            _dashboardPage.ClickUsersAction();

            // Assert
            Driver.Url.Should().Contain("users");
        });
    }

    [Fact]
    [Trait("TestType", "Authorization")]
    public void AdminDashboard_AsEmployee_ShouldRedirectToEmployeeDashboard()
    {
        ExecuteWithScreenshotOnFailure(() =>
        {
            // Arrange - Login as employee
            _loginPage.GoTo();
            _loginPage.LoginAsEmployee(Config.EmployeeEmail, Config.EmployeePassword);

            // Act - Try to access admin dashboard
            NavigateTo("admin/dashboard");

            // Assert - Should not be on admin dashboard
            // Either redirected or shows access denied
            WaitHelper.WaitForPageLoad(Driver);
            var currentUrl = Driver.Url.ToLowerInvariant();
            
            // Employee should not be able to access admin routes
            var isOnAdminDashboard = currentUrl.Contains("admin/dashboard");
            if (isOnAdminDashboard)
            {
                // If somehow on admin dashboard, should show access denied
                var pageContent = Driver.PageSource;
                pageContent.Should().ContainAny("access denied", "unauthorized", "forbidden");
            }
        });
    }
}
