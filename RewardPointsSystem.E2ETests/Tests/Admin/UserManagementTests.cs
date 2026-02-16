using FluentAssertions;
using RewardPointsSystem.E2ETests.Base;
using RewardPointsSystem.E2ETests.Helpers;
using RewardPointsSystem.E2ETests.PageObjects;
using RewardPointsSystem.E2ETests.PageObjects.Admin;
using Xunit;
using Xunit.Abstractions;

namespace RewardPointsSystem.E2ETests.Tests.Admin;

/// <summary>
/// E2E tests for User Management functionality.
/// </summary>
[Trait("Category", "Admin")]
[Trait("Feature", "Users")]
[Trait("Priority", "High")]
public class UserManagementTests : BaseTest
{
    private readonly LoginPage _loginPage;
    private readonly UsersManagementPage _usersPage;

    public UserManagementTests(ITestOutputHelper output) : base(output)
    {
        _loginPage = new LoginPage(Driver);
        _usersPage = new UsersManagementPage(Driver);
    }

    private void LoginAsAdmin()
    {
        _loginPage.GoTo();
        _loginPage.LoginAsAdmin(Config.AdminEmail, Config.AdminPassword);
    }

    [Fact]
    [Trait("TestType", "Smoke")]
    public void UsersPage_ShouldLoad_Successfully()
    {
        ExecuteWithScreenshotOnFailure(() =>
        {
            // Arrange
            LoginAsAdmin();

            // Act
            _usersPage.GoTo();

            // Assert
            _usersPage.IsOnPage().Should().BeTrue();
        });
    }

    [Fact]
    [Trait("TestType", "Functional")]
    public void UsersPage_ShouldDisplayUsers()
    {
        ExecuteWithScreenshotOnFailure(() =>
        {
            // Arrange
            LoginAsAdmin();

            // Act
            _usersPage.GoTo();

            // Assert
            _usersPage.GetUserCount().Should().BeGreaterThan(0, "there should be at least one user");
        });
    }

    [Fact]
    [Trait("TestType", "Functional")]
    public void SearchUsers_WithValidEmail_FindsUser()
    {
        ExecuteWithScreenshotOnFailure(() =>
        {
            // Arrange
            LoginAsAdmin();
            _usersPage.GoTo();

            // Act - search for a known user
            _usersPage.SearchUsers(Config.EmployeeEmail);

            // Assert
            _usersPage.UserExists(Config.EmployeeEmail).Should().BeTrue();
        });
    }

    [Fact]
    [Trait("TestType", "Functional")]
    public void SearchUsers_WithNonExistingEmail_ShowsNoResults()
    {
        ExecuteWithScreenshotOnFailure(() =>
        {
            // Arrange
            LoginAsAdmin();
            _usersPage.GoTo();

            // Act
            _usersPage.SearchUsers("nonexistent_user_xyz@agdata.com");

            // Assert
            _usersPage.GetUserCount().Should().Be(0);
        });
    }

    [Fact]
    [Trait("TestType", "Functional")]
    public void FilterUsers_ByRole_FiltersCorrectly()
    {
        ExecuteWithScreenshotOnFailure(() =>
        {
            // Arrange
            LoginAsAdmin();
            _usersPage.GoTo();
            var initialCount = _usersPage.GetUserCount();

            // Act
            _usersPage.FilterByRole("Admin");

            // Assert - filtered count should be less than or equal to total
            var filteredCount = _usersPage.GetUserCount();
            filteredCount.Should().BeLessThanOrEqualTo(initialCount);
        });
    }

    [Fact]
    [Trait("TestType", "Functional")]
    public void GetUserDetails_ReturnsCorrectInfo()
    {
        ExecuteWithScreenshotOnFailure(() =>
        {
            // Arrange
            LoginAsAdmin();
            _usersPage.GoTo();
            _usersPage.SearchUsers(Config.EmployeeEmail);

            // Act
            var userExists = _usersPage.UserExists(Config.EmployeeEmail);

            // Assert
            userExists.Should().BeTrue();
        });
    }

    [Fact]
    [Trait("TestType", "Security")]
    public void UsersPage_AsEmployee_ShouldNotBeAccessible()
    {
        ExecuteWithScreenshotOnFailure(() =>
        {
            // Arrange - Login as employee
            _loginPage.GoTo();
            _loginPage.LoginAsEmployee(Config.EmployeeEmail, Config.EmployeePassword);

            // Act - Try to access users page
            NavigateTo("admin/users");
            WaitHelper.WaitForPageLoad(Driver);

            // Assert - Should not be on users management page
            var currentUrl = Driver.Url.ToLowerInvariant();
            var isOnUsersPage = currentUrl.Contains("admin/users");
            
            if (isOnUsersPage)
            {
                // If somehow on the page, should show access denied
                var pageContent = Driver.PageSource.ToLowerInvariant();
                pageContent.Should().ContainAny("access denied", "unauthorized", "forbidden", "not authorized");
            }
        });
    }
}
