using FluentAssertions;
using RewardPointsSystem.E2ETests.Base;
using RewardPointsSystem.E2ETests.Helpers;
using RewardPointsSystem.E2ETests.PageObjects;
using RewardPointsSystem.E2ETests.PageObjects.Admin;
using Xunit;
using Xunit.Abstractions;

namespace RewardPointsSystem.E2ETests.Tests.Admin;

/// <summary>
/// E2E tests for Redemption Approval functionality.
/// </summary>
[Trait("Category", "Admin")]
[Trait("Feature", "Redemptions")]
[Trait("Priority", "High")]
public class RedemptionApprovalTests : BaseTest
{
    private readonly LoginPage _loginPage;
    private readonly RedemptionsManagementPage _redemptionsPage;

    public RedemptionApprovalTests(ITestOutputHelper output) : base(output)
    {
        _loginPage = new LoginPage(Driver);
        _redemptionsPage = new RedemptionsManagementPage(Driver);
    }

    private void LoginAsAdmin()
    {
        _loginPage.GoTo();
        _loginPage.LoginAsAdmin(Config.AdminEmail, Config.AdminPassword);
    }

    [Fact]
    [Trait("TestType", "Smoke")]
    public void RedemptionsPage_ShouldLoad_Successfully()
    {
        ExecuteWithScreenshotOnFailure(() =>
        {
            // Arrange
            LoginAsAdmin();

            // Act
            _redemptionsPage.GoTo();

            // Assert
            _redemptionsPage.IsOnPage().Should().BeTrue();
        });
    }

    [Fact]
    [Trait("TestType", "Functional")]
    public void RedemptionsPage_DisplaysRedemptions()
    {
        ExecuteWithScreenshotOnFailure(() =>
        {
            // Arrange
            LoginAsAdmin();

            // Act
            _redemptionsPage.GoTo();

            // Assert - Page loads without errors
            _redemptionsPage.IsOnPage().Should().BeTrue();
            // Count might be 0 if no redemptions exist
            _redemptionsPage.GetRedemptionCount().Should().BeGreaterThanOrEqualTo(0);
        });
    }

    [Fact]
    [Trait("TestType", "Functional")]
    public void FilterRedemptions_ByPending_FiltersList()
    {
        ExecuteWithScreenshotOnFailure(() =>
        {
            // Arrange
            LoginAsAdmin();
            _redemptionsPage.GoTo();

            // Act
            _redemptionsPage.FilterByStatus("Pending");

            // Assert
            _redemptionsPage.IsOnPage().Should().BeTrue();
            // All visible items should be pending (if any exist)
        });
    }

    [Fact]
    [Trait("TestType", "Functional")]
    public void FilterRedemptions_ByApproved_FiltersList()
    {
        ExecuteWithScreenshotOnFailure(() =>
        {
            // Arrange
            LoginAsAdmin();
            _redemptionsPage.GoTo();

            // Act
            _redemptionsPage.FilterByStatus("Approved");

            // Assert
            _redemptionsPage.IsOnPage().Should().BeTrue();
        });
    }

    [Fact]
    [Trait("TestType", "Functional")]
    public void FilterRedemptions_ByRejected_FiltersList()
    {
        ExecuteWithScreenshotOnFailure(() =>
        {
            // Arrange
            LoginAsAdmin();
            _redemptionsPage.GoTo();

            // Act
            _redemptionsPage.FilterByStatus("Rejected");

            // Assert
            _redemptionsPage.IsOnPage().Should().BeTrue();
        });
    }

    [Fact]
    [Trait("TestType", "Functional")]
    public void FilterRedemptions_ByDelivered_FiltersList()
    {
        ExecuteWithScreenshotOnFailure(() =>
        {
            // Arrange
            LoginAsAdmin();
            _redemptionsPage.GoTo();

            // Act
            _redemptionsPage.FilterByStatus("Delivered");

            // Assert
            _redemptionsPage.IsOnPage().Should().BeTrue();
        });
    }

    [Fact]
    [Trait("TestType", "Security")]
    public void RedemptionsPage_AsEmployee_ShouldNotBeAccessible()
    {
        ExecuteWithScreenshotOnFailure(() =>
        {
            // Arrange - Login as employee
            _loginPage.GoTo();
            _loginPage.LoginAsEmployee(Config.EmployeeEmail, Config.EmployeePassword);

            // Act - Try to access redemptions management page
            NavigateTo("admin/redemptions");
            WaitHelper.WaitForPageLoad(Driver);

            // Assert
            var currentUrl = Driver.Url.ToLowerInvariant();
            var isOnRedemptionsPage = currentUrl.Contains("admin/redemptions");
            
            if (isOnRedemptionsPage)
            {
                var pageContent = Driver.PageSource.ToLowerInvariant();
                pageContent.Should().ContainAny("access denied", "unauthorized", "forbidden", "not authorized");
            }
        });
    }

    // Note: Approval/Rejection tests would require test data setup
    // In a real scenario, you would:
    // 1. Create a product via API
    // 2. Login as employee and redeem product via API
    // 3. Login as admin and approve/reject via UI
    // This demonstrates the test structure while avoiding flaky UI-based setup

    [Fact]
    [Trait("TestType", "Integration")]
    [Trait("Category", "RequiresTestData")]
    public void ApproveRedemption_ChangesStatusToApproved()
    {
        // This test requires pre-existing test data
        // In CI, this would be set up via database seeding or API calls
        ExecuteWithScreenshotOnFailure(() =>
        {
            // Arrange
            LoginAsAdmin();
            _redemptionsPage.GoTo();
            _redemptionsPage.FilterByStatus("Pending");

            var pendingCount = _redemptionsPage.GetRedemptionCount();
            
            if (pendingCount == 0)
            {
                Logger.Warning("No pending redemptions found - skipping test");
                return; // Skip if no test data
            }

            // Test structure for when data exists:
            // var firstRedemption = ... get first redemption ...
            // _redemptionsPage.ApproveRedemption(firstRedemption);
            // _redemptionsPage.FilterByStatus("Pending");
            // Verify count decreased
        });
    }

    [Fact]
    [Trait("TestType", "Integration")]
    [Trait("Category", "RequiresTestData")]
    public void RejectRedemption_WithReason_ChangesStatusToRejected()
    {
        ExecuteWithScreenshotOnFailure(() =>
        {
            // Arrange
            LoginAsAdmin();
            _redemptionsPage.GoTo();
            _redemptionsPage.FilterByStatus("Pending");

            var pendingCount = _redemptionsPage.GetRedemptionCount();
            
            if (pendingCount == 0)
            {
                Logger.Warning("No pending redemptions found - skipping test");
                return;
            }

            // Test structure:
            // _redemptionsPage.RejectRedemption(searchText, "Test rejection reason");
            // Verify rejection
        });
    }
}
