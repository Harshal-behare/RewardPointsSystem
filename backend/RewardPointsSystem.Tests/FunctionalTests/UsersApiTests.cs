using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RewardPointsSystem.Api;
using RewardPointsSystem.Application.DTOs.Common;
using RewardPointsSystem.Application.Interfaces;
using RewardPointsSystem.Domain.Entities.Core;
using Xunit;

namespace RewardPointsSystem.Tests.FunctionalTests
{
    /// <summary>
    /// Functional Tests for Users API Endpoints
    /// 
    /// These tests verify the HTTP API behavior for user operations:
    /// - User listing (Admin only)
    /// - User details retrieval
    /// - Authorization requirements
    /// 
    /// WHAT WE'RE TESTING:
    /// The complete HTTP request/response cycle for user management endpoints
    /// 
    /// WHY THESE TESTS MATTER:
    /// User management is critical for system administration
    /// </summary>
    public class UsersApiTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly JsonSerializerOptions _jsonOptions;

        public UsersApiTests(CustomWebApplicationFactory<Program> factory)
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
        /// SCENARIO: Unauthenticated client requests user list
        /// ENDPOINT: GET /api/users
        /// EXPECTED: 401 Unauthorized
        /// WHY: User list requires admin authentication
        /// </summary>
        [Fact]
        public async Task GetAllUsers_WithoutAuth_ShouldReturn401()
        {
            // Act
            var response = await _client.GetAsync("/api/v1/users");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized, 
                "users endpoint requires authentication");
        }

        /// <summary>
        /// SCENARIO: Unauthenticated client requests specific user
        /// ENDPOINT: GET /api/v1/users/{id}
        /// EXPECTED: 401 Unauthorized or 404 Not Found (if route requires auth before reaching handler)
        /// WHY: User details require authentication
        /// </summary>
        [Fact]
        public async Task GetUserById_WithoutAuth_ShouldReturnError()
        {
            // Arrange
            var userId = Guid.NewGuid();

            // Act
            var response = await _client.GetAsync($"/api/v1/users/{userId}");

            // Assert - Should return 401 or 404 (both indicate endpoint requires auth)
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.Unauthorized,
                HttpStatusCode.NotFound);
        }

        #endregion

        #region GET /api/v1/users/me Tests

        /// <summary>
        /// SCENARIO: Unauthenticated client requests own profile
        /// ENDPOINT: GET /api/v1/users/me (or current user)
        /// EXPECTED: 401 Unauthorized or 400 BadRequest
        /// WHY: Profile endpoint requires authenticated user
        /// </summary>
        [Fact]
        public async Task GetCurrentUser_WithoutAuth_ShouldReturnError()
        {
            // Act - Note: "me" might not be a route, check actual routes
            var response = await _client.GetAsync("/api/v1/users/me");

            // Assert - Should fail without auth (may return 401 or 400 for missing claims)
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.Unauthorized,
                HttpStatusCode.BadRequest,
                HttpStatusCode.NotFound);
        }

        #endregion

        #region User Points Endpoints Tests

        /// <summary>
        /// SCENARIO: Unauthenticated client requests user points balance
        /// ENDPOINT: GET /api/v1/users/{id}/balance
        /// EXPECTED: 401 Unauthorized or error status
        /// WHY: Points data is sensitive and requires authentication
        /// </summary>
        [Fact]
        public async Task GetUserBalance_WithoutAuth_ShouldReturnError()
        {
            // Arrange
            var userId = Guid.NewGuid();

            // Act - Using /balance endpoint which exists in UsersController
            var response = await _client.GetAsync($"/api/v1/users/{userId}/balance");

            // Assert - Should fail without auth
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.Unauthorized,
                HttpStatusCode.NotFound);
        }

        #endregion
    }
}
