using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using RewardPointsSystem.Api;
using RewardPointsSystem.Tests.FunctionalTests;
using Xunit;

namespace RewardPointsSystem.Tests.ApiTests
{
    /// <summary>
    /// API Tests for AdminController endpoints
    /// 
    /// Tests authentication/authorization requirements for Admin-only endpoints
    /// Following: Isolation, AAA pattern, determinism
    /// 
    /// Coverage:
    /// - All endpoints require Admin role
    /// - Anonymous users get 401
    /// - Non-admin users get 403
    /// - Admin users can access endpoints
    /// </summary>
    public class AdminControllerApiTests : AuthenticatedApiTestBase
    {
        public AdminControllerApiTests(CustomWebApplicationFactory<Program> factory)
            : base(factory)
        {
        }

        #region GET /api/v1/admin/dashboard Tests

        [Fact]
        public async Task GetDashboard_WithoutAuthentication_Returns401()
        {
            // Arrange
            var client = CreateAnonymousClient();

            // Act
            var response = await client.GetAsync("/api/v1/admin/dashboard");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetDashboard_AsEmployee_Returns403()
        {
            // Arrange
            var client = CreateEmployeeClient();

            // Act
            var response = await client.GetAsync("/api/v1/admin/dashboard");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetDashboard_AsAdmin_Returns200()
        {
            // Arrange
            var client = CreateAdminClient();

            // Act
            var response = await client.GetAsync("/api/v1/admin/dashboard");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        #endregion

        #region GET /api/v1/admin/reports/points Tests

        [Fact]
        public async Task GetPointsReport_WithoutAuthentication_Returns401()
        {
            // Arrange
            var client = CreateAnonymousClient();

            // Act
            var response = await client.GetAsync("/api/v1/admin/reports/points");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetPointsReport_AsEmployee_Returns403()
        {
            // Arrange
            var client = CreateEmployeeClient();

            // Act
            var response = await client.GetAsync("/api/v1/admin/reports/points");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetPointsReport_AsAdmin_Returns200()
        {
            // Arrange
            var client = CreateAdminClient();

            // Act
            var response = await client.GetAsync("/api/v1/admin/reports/points");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetPointsReport_AsAdmin_WithDateRange_Returns200()
        {
            // Arrange
            var client = CreateAdminClient();
            var startDate = DateTime.UtcNow.AddMonths(-1).ToString("yyyy-MM-dd");
            var endDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

            // Act
            var response = await client.GetAsync($"/api/v1/admin/reports/points?startDate={startDate}&endDate={endDate}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        #endregion

        #region GET /api/v1/admin/reports/users Tests

        [Fact]
        public async Task GetUsersReport_WithoutAuthentication_Returns401()
        {
            // Arrange
            var client = CreateAnonymousClient();

            // Act
            var response = await client.GetAsync("/api/v1/admin/reports/users");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetUsersReport_AsEmployee_Returns403()
        {
            // Arrange
            var client = CreateEmployeeClient();

            // Act
            var response = await client.GetAsync("/api/v1/admin/reports/users");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetUsersReport_AsAdmin_Returns200()
        {
            // Arrange
            var client = CreateAdminClient();

            // Act
            var response = await client.GetAsync("/api/v1/admin/reports/users");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        #endregion

        #region GET /api/v1/admin/reports/redemptions Tests

        [Fact]
        public async Task GetRedemptionsReport_WithoutAuthentication_Returns401()
        {
            // Arrange
            var client = CreateAnonymousClient();

            // Act
            var response = await client.GetAsync("/api/v1/admin/reports/redemptions");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetRedemptionsReport_AsEmployee_Returns403()
        {
            // Arrange
            var client = CreateEmployeeClient();

            // Act
            var response = await client.GetAsync("/api/v1/admin/reports/redemptions");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetRedemptionsReport_AsAdmin_Returns200()
        {
            // Arrange
            var client = CreateAdminClient();

            // Act
            var response = await client.GetAsync("/api/v1/admin/reports/redemptions");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        #endregion

        #region GET /api/v1/admin/reports/events Tests

        [Fact]
        public async Task GetEventsReport_WithoutAuthentication_Returns401()
        {
            // Arrange
            var client = CreateAnonymousClient();

            // Act
            var response = await client.GetAsync("/api/v1/admin/reports/events");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetEventsReport_AsEmployee_Returns403()
        {
            // Arrange
            var client = CreateEmployeeClient();

            // Act
            var response = await client.GetAsync("/api/v1/admin/reports/events");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetEventsReport_AsAdmin_Returns200()
        {
            // Arrange
            var client = CreateAdminClient();

            // Act
            var response = await client.GetAsync("/api/v1/admin/reports/events");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        #endregion

        #region GET /api/v1/admin/alerts/inventory Tests

        [Fact]
        public async Task GetInventoryAlerts_WithoutAuthentication_Returns401()
        {
            // Arrange
            var client = CreateAnonymousClient();

            // Act
            var response = await client.GetAsync("/api/v1/admin/alerts/inventory");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetInventoryAlerts_AsEmployee_Returns403()
        {
            // Arrange
            var client = CreateEmployeeClient();

            // Act
            var response = await client.GetAsync("/api/v1/admin/alerts/inventory");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetInventoryAlerts_AsAdmin_Returns200()
        {
            // Arrange
            var client = CreateAdminClient();

            // Act
            var response = await client.GetAsync("/api/v1/admin/alerts/inventory");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        #endregion

        #region GET /api/v1/admin/budget Tests

        [Fact]
        public async Task GetBudget_WithoutAuthentication_Returns401()
        {
            // Arrange
            var client = CreateAnonymousClient();

            // Act
            var response = await client.GetAsync("/api/v1/admin/budget");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetBudget_AsEmployee_Returns403()
        {
            // Arrange
            var client = CreateEmployeeClient();

            // Act
            var response = await client.GetAsync("/api/v1/admin/budget");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetBudget_AsAdmin_Returns200()
        {
            // Arrange
            var client = CreateAdminClient();

            // Act
            var response = await client.GetAsync("/api/v1/admin/budget");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        #endregion

        #region PUT /api/v1/admin/budget Tests

        [Fact]
        public async Task SetBudget_WithoutAuthentication_Returns401()
        {
            // Arrange
            var client = CreateAnonymousClient();
            var content = CreateJsonContent(new
            {
                budgetLimit = 10000,
                isHardLimit = false,
                warningThreshold = 80
            });

            // Act
            var response = await client.PutAsync("/api/v1/admin/budget", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task SetBudget_AsEmployee_Returns403()
        {
            // Arrange
            var client = CreateEmployeeClient();
            var content = CreateJsonContent(new
            {
                budgetLimit = 10000,
                isHardLimit = false,
                warningThreshold = 80
            });

            // Act
            var response = await client.PutAsync("/api/v1/admin/budget", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task SetBudget_AsAdmin_WithValidData_Returns200()
        {
            // Arrange
            var client = CreateAdminClient();
            var content = CreateJsonContent(new
            {
                budgetLimit = 10000,
                isHardLimit = false,
                warningThreshold = 80
            });

            // Act
            var response = await client.PutAsync("/api/v1/admin/budget", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task SetBudget_AsAdmin_WithZeroBudget_Returns400Or422()
        {
            // Arrange
            var client = CreateAdminClient();
            var content = CreateJsonContent(new
            {
                budgetLimit = 0, // Invalid
                isHardLimit = false,
                warningThreshold = 80
            });

            // Act
            var response = await client.PutAsync("/api/v1/admin/budget", content);

            // Assert
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.BadRequest,
                HttpStatusCode.UnprocessableEntity);
        }

        [Fact]
        public async Task SetBudget_AsAdmin_WithInvalidThreshold_Returns400Or422()
        {
            // Arrange
            var client = CreateAdminClient();
            var content = CreateJsonContent(new
            {
                budgetLimit = 10000,
                isHardLimit = false,
                warningThreshold = 150 // Invalid - must be 0-100
            });

            // Act
            var response = await client.PutAsync("/api/v1/admin/budget", content);

            // Assert
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.BadRequest,
                HttpStatusCode.UnprocessableEntity);
        }

        #endregion

        #region GET /api/v1/admin/budget/validate Tests

        [Fact]
        public async Task ValidateBudget_WithoutAuthentication_Returns401()
        {
            // Arrange
            var client = CreateAnonymousClient();

            // Act
            var response = await client.GetAsync("/api/v1/admin/budget/validate?points=100");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task ValidateBudget_AsAdmin_Returns200()
        {
            // Arrange
            var client = CreateAdminClient();

            // Act
            var response = await client.GetAsync("/api/v1/admin/budget/validate?points=100");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        #endregion

        #region GET /api/v1/admin/budget/history Tests

        [Fact]
        public async Task GetBudgetHistory_WithoutAuthentication_Returns401()
        {
            // Arrange
            var client = CreateAnonymousClient();

            // Act
            var response = await client.GetAsync("/api/v1/admin/budget/history");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetBudgetHistory_AsAdmin_Returns200()
        {
            // Arrange
            var client = CreateAdminClient();

            // Act
            var response = await client.GetAsync("/api/v1/admin/budget/history");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        #endregion
    }
}
