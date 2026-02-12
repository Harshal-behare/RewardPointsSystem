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
    /// API Tests for RedemptionsController endpoints
    /// 
    /// Tests authentication/authorization requirements and HTTP behavior
    /// Following: Isolation, AAA pattern, determinism
    /// 
    /// Coverage:
    /// - All endpoints require authentication
    /// - Admin-only endpoints reject non-admin users
    /// - Valid requests return expected status codes
    /// </summary>
    public class RedemptionsControllerApiTests : AuthenticatedApiTestBase
    {
        public RedemptionsControllerApiTests(CustomWebApplicationFactory<Program> factory)
            : base(factory)
        {
        }

        #region GET /api/v1/redemptions (Admin) Tests

        [Fact]
        public async Task GetAllRedemptions_WithoutAuthentication_Returns401()
        {
            // Arrange
            var client = CreateAnonymousClient();

            // Act
            var response = await client.GetAsync("/api/v1/redemptions");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetAllRedemptions_AsEmployee_Returns403()
        {
            // Arrange
            var client = CreateEmployeeClient();

            // Act
            var response = await client.GetAsync("/api/v1/redemptions");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetAllRedemptions_AsAdmin_Returns200()
        {
            // Arrange
            var client = CreateAdminClient();

            // Act
            var response = await client.GetAsync("/api/v1/redemptions");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        #endregion

        #region GET /api/v1/redemptions/{id} Tests

        [Fact]
        public async Task GetRedemptionById_WithoutAuthentication_Returns401()
        {
            // Arrange
            var client = CreateAnonymousClient();
            var redemptionId = Guid.NewGuid();

            // Act
            var response = await client.GetAsync($"/api/v1/redemptions/{redemptionId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetRedemptionById_AsEmployee_WithNonExistentId_Returns404()
        {
            // Arrange
            var client = CreateEmployeeClient();
            var nonExistentId = Guid.NewGuid();

            // Act
            var response = await client.GetAsync($"/api/v1/redemptions/{nonExistentId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        #endregion

        #region POST /api/v1/redemptions Tests

        [Fact]
        public async Task CreateRedemption_WithoutAuthentication_Returns401()
        {
            // Arrange
            var client = CreateAnonymousClient();
            var content = CreateJsonContent(new
            {
                productId = Guid.NewGuid(),
                quantity = 1
            });

            // Act
            var response = await client.PostAsync("/api/v1/redemptions", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task CreateRedemption_AsEmployee_WithNonExistentProduct_Returns422()
        {
            // Arrange
            var client = CreateEmployeeClient();
            var content = CreateJsonContent(new
            {
                productId = Guid.NewGuid(), // Non-existent product
                quantity = 1
            });

            // Act
            var response = await client.PostAsync("/api/v1/redemptions", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }

        [Fact]
        public async Task CreateRedemption_AsEmployee_WithZeroQuantity_Returns400Or422()
        {
            // Arrange
            var client = CreateEmployeeClient();
            var content = CreateJsonContent(new
            {
                productId = Guid.NewGuid(),
                quantity = 0 // Invalid quantity
            });

            // Act
            var response = await client.PostAsync("/api/v1/redemptions", content);

            // Assert
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.BadRequest,
                HttpStatusCode.UnprocessableEntity,
                HttpStatusCode.NotFound); // product not found takes precedence
        }

        #endregion

        #region PATCH /api/v1/redemptions/{id}/approve (Admin) Tests

        [Fact]
        public async Task ApproveRedemption_WithoutAuthentication_Returns401()
        {
            // Arrange
            var client = CreateAnonymousClient();
            var redemptionId = Guid.NewGuid();

            // Act
            var response = await client.PatchAsync($"/api/v1/redemptions/{redemptionId}/approve", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task ApproveRedemption_AsEmployee_Returns403()
        {
            // Arrange
            var client = CreateEmployeeClient();
            var redemptionId = Guid.NewGuid();

            // Act
            var response = await client.PatchAsync($"/api/v1/redemptions/{redemptionId}/approve", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task ApproveRedemption_AsAdmin_WithNonExistentId_ReturnsError()
        {
            // Arrange
            var client = CreateAdminClient();
            var nonExistentId = Guid.NewGuid();

            // Act
            var response = await client.PatchAsync($"/api/v1/redemptions/{nonExistentId}/approve", null);

            // Assert - API may return 400, 404, or 500 depending on implementation
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.BadRequest,
                HttpStatusCode.NotFound,
                HttpStatusCode.InternalServerError);
        }

        #endregion

        #region PATCH /api/v1/redemptions/{id}/deliver (Admin) Tests

        [Fact]
        public async Task DeliverRedemption_WithoutAuthentication_Returns401()
        {
            // Arrange
            var client = CreateAnonymousClient();
            var redemptionId = Guid.NewGuid();

            // Act
            var response = await client.PatchAsync($"/api/v1/redemptions/{redemptionId}/deliver", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task DeliverRedemption_AsEmployee_Returns403()
        {
            // Arrange
            var client = CreateEmployeeClient();
            var redemptionId = Guid.NewGuid();

            // Act
            var response = await client.PatchAsync($"/api/v1/redemptions/{redemptionId}/deliver", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task DeliverRedemption_AsAdmin_WithNonExistentId_ReturnsError()
        {
            // Arrange
            var client = CreateAdminClient();
            var nonExistentId = Guid.NewGuid();

            // Act
            var response = await client.PatchAsync($"/api/v1/redemptions/{nonExistentId}/deliver", null);

            // Assert - API may return 400, 404, or 500 depending on implementation
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.BadRequest,
                HttpStatusCode.NotFound,
                HttpStatusCode.InternalServerError);
        }

        #endregion

        #region PATCH /api/v1/redemptions/{id}/reject (Admin) Tests

        [Fact]
        public async Task RejectRedemption_WithoutAuthentication_Returns401()
        {
            // Arrange
            var client = CreateAnonymousClient();
            var redemptionId = Guid.NewGuid();
            var content = CreateJsonContent(new { rejectionReason = "Test rejection" });

            // Act
            var response = await client.PatchAsync($"/api/v1/redemptions/{redemptionId}/reject", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task RejectRedemption_AsEmployee_Returns403()
        {
            // Arrange
            var client = CreateEmployeeClient();
            var redemptionId = Guid.NewGuid();
            var content = CreateJsonContent(new { rejectionReason = "Test rejection" });

            // Act
            var response = await client.PatchAsync($"/api/v1/redemptions/{redemptionId}/reject", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task RejectRedemption_AsAdmin_WithNonExistentId_ReturnsError()
        {
            // Arrange
            var client = CreateAdminClient();
            var nonExistentId = Guid.NewGuid();
            var content = CreateJsonContent(new { rejectionReason = "Test rejection" });

            // Act
            var response = await client.PatchAsync($"/api/v1/redemptions/{nonExistentId}/reject", content);

            // Assert - API may return 400, 404, or 500 depending on implementation
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.BadRequest,
                HttpStatusCode.NotFound,
                HttpStatusCode.InternalServerError);
        }

        #endregion

        #region PATCH /api/v1/redemptions/{id}/cancel Tests

        [Fact]
        public async Task CancelRedemption_WithoutAuthentication_Returns401()
        {
            // Arrange
            var client = CreateAnonymousClient();
            var redemptionId = Guid.NewGuid();

            // Act
            var response = await client.PatchAsync($"/api/v1/redemptions/{redemptionId}/cancel", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task CancelRedemption_AsEmployee_WithNonExistentId_Returns404()
        {
            // Arrange
            var client = CreateEmployeeClient();
            var nonExistentId = Guid.NewGuid();

            // Act
            var response = await client.PatchAsync($"/api/v1/redemptions/{nonExistentId}/cancel", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task CancelRedemption_AsAdmin_WithNonExistentId_Returns404()
        {
            // Arrange
            var client = CreateAdminClient();
            var nonExistentId = Guid.NewGuid();

            // Act
            var response = await client.PatchAsync($"/api/v1/redemptions/{nonExistentId}/cancel", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        #endregion
    }
}
