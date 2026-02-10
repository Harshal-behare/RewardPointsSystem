using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RewardPointsSystem.Api;
using RewardPointsSystem.Application.DTOs.Common;
using RewardPointsSystem.Application.Interfaces;
using Xunit;

namespace RewardPointsSystem.Tests.FunctionalTests
{
    /// <summary>
    /// Functional Tests for Redemptions API Endpoints
    /// 
    /// These tests verify the HTTP API behavior for redemption operations:
    /// - Redemption listing
    /// - Redemption creation
    /// - Admin approval workflow
    /// - Authorization requirements
    /// 
    /// WHAT WE'RE TESTING:
    /// The complete HTTP request/response cycle for redemption endpoints
    /// 
    /// WHY THESE TESTS MATTER:
    /// Redemptions are how employees spend points - critical path
    /// </summary>
    public class RedemptionsApiTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly JsonSerializerOptions _jsonOptions;

        public RedemptionsApiTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        #region Authorization Tests

        /// <summary>
        /// SCENARIO: Unauthenticated client requests redemption list
        /// ENDPOINT: GET /api/redemptions
        /// EXPECTED: 401 Unauthorized
        /// WHY: Redemption data requires authentication
        /// </summary>
        [Fact]
        public async Task GetRedemptions_WithoutAuth_ShouldReturn401()
        {
            // Act
            var response = await _client.GetAsync("/api/v1/redemptions");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
                "redemptions endpoint requires authentication");
        }

        /// <summary>
        /// SCENARIO: Unauthenticated client tries to create redemption
        /// ENDPOINT: POST /api/redemptions
        /// EXPECTED: 401 Unauthorized
        /// WHY: Creating redemptions requires authenticated user
        /// </summary>
        [Fact]
        public async Task CreateRedemption_WithoutAuth_ShouldReturn401()
        {
            // Arrange
            var redemptionRequest = new
            {
                productId = Guid.NewGuid(),
                quantity = 1
            };

            var content = new StringContent(
                JsonSerializer.Serialize(redemptionRequest, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PostAsync("/api/v1/redemptions", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
                "creating redemption requires authentication");
        }

        /// <summary>
        /// SCENARIO: Unauthenticated client requests specific redemption
        /// ENDPOINT: GET /api/redemptions/{id}
        /// EXPECTED: 401 Unauthorized
        /// WHY: Redemption details require authentication
        /// </summary>
        [Fact]
        public async Task GetRedemptionById_WithoutAuth_ShouldReturn401()
        {
            // Arrange
            var redemptionId = Guid.NewGuid();

            // Act
            var response = await _client.GetAsync($"/api/v1/redemptions/{redemptionId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
                "redemption details require authentication");
        }

        #endregion

        #region Admin Endpoints Authorization Tests

        /// <summary>
        /// SCENARIO: Unauthenticated client tries to approve redemption
        /// ENDPOINT: PATCH /api/v1/redemptions/{id}/approve
        /// EXPECTED: 401 Unauthorized
        /// WHY: Admin actions require admin authentication
        /// </summary>
        [Fact]
        public async Task ApproveRedemption_WithoutAuth_ShouldReturn401()
        {
            // Arrange
            var redemptionId = Guid.NewGuid();
            var approveRequest = new { };
            var content = new StringContent(
                JsonSerializer.Serialize(approveRequest, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            // Act - API uses PATCH not POST
            var request = new HttpRequestMessage(HttpMethod.Patch, $"/api/v1/redemptions/{redemptionId}/approve")
            {
                Content = content
            };
            var response = await _client.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
                "approval requires admin authentication");
        }

        /// <summary>
        /// SCENARIO: Unauthenticated client tries to reject redemption
        /// ENDPOINT: PATCH /api/v1/redemptions/{id}/reject
        /// EXPECTED: 401 Unauthorized
        /// WHY: Admin actions require admin authentication
        /// </summary>
        [Fact]
        public async Task RejectRedemption_WithoutAuth_ShouldReturn401()
        {
            // Arrange
            var redemptionId = Guid.NewGuid();
            var rejectRequest = new { rejectionReason = "Out of stock" };

            var content = new StringContent(
                JsonSerializer.Serialize(rejectRequest, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            // Act - API uses PATCH not POST
            var request = new HttpRequestMessage(HttpMethod.Patch, $"/api/v1/redemptions/{redemptionId}/reject")
            {
                Content = content
            };
            var response = await _client.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
                "rejection requires admin authentication");
        }

        /// <summary>
        /// SCENARIO: Unauthenticated client tries to deliver redemption
        /// ENDPOINT: PATCH /api/v1/redemptions/{id}/deliver
        /// EXPECTED: 401 Unauthorized
        /// WHY: Delivery confirmation requires admin authentication
        /// </summary>
        [Fact]
        public async Task DeliverRedemption_WithoutAuth_ShouldReturn401()
        {
            // Arrange
            var redemptionId = Guid.NewGuid();

            // Act - API uses PATCH not POST
            var request = new HttpRequestMessage(HttpMethod.Patch, $"/api/v1/redemptions/{redemptionId}/deliver");
            var response = await _client.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
                "delivery requires admin authentication");
        }

        #endregion

        #region User Redemption History Tests

        /// <summary>
        /// SCENARIO: Unauthenticated client requests own redemption history
        /// ENDPOINT: GET /api/v1/redemptions/my-redemptions
        /// EXPECTED: 401 Unauthorized
        /// WHY: Redemption history is private user data
        /// </summary>
        [Fact]
        public async Task GetMyRedemptions_WithoutAuth_ShouldReturn401()
        {
            // Act - Actual endpoint is /my-redemptions not /user/{userId}
            var response = await _client.GetAsync("/api/v1/redemptions/my-redemptions");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
                "user redemption history requires authentication");
        }

        #endregion
    }
}
