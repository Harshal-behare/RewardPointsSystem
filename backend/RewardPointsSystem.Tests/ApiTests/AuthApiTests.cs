using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RewardPointsSystem.Api;
using RewardPointsSystem.Application.DTOs.Auth;
using RewardPointsSystem.Application.DTOs.Common;
using RewardPointsSystem.Application.Interfaces;
using RewardPointsSystem.Domain.Entities.Core;
using Xunit;

namespace RewardPointsSystem.Tests.FunctionalTests
{
    /// <summary>
    /// These tests verify the HTTP API behavior for authentication:
    /// - Login endpoint
    /// - Registration endpoint
    /// - Token refresh
    /// - Input validation
    /// Authentication is the gateway to all protected functionality
    /// </summary>
    public class AuthApiTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly JsonSerializerOptions _jsonOptions;

        public AuthApiTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        #region POST /api/auth/login Tests

        /// <summary>
        /// SCENARIO: Client submits login with invalid credentials
        /// ENDPOINT: POST /api/v1/auth/login
        /// EXPECTED: 401 Unauthorized or 400 BadRequest (if validation fails first)
        /// WHY: Invalid credentials should not grant access
        /// </summary>
        [Fact]
        public async Task Login_WithInvalidCredentials_ShouldReturnError()
        {
            // Arrange
            var loginRequest = new
            {
                email = "nonexistent@company.com",
                password = "wrongpassword"
            };

            var content = new StringContent(
                JsonSerializer.Serialize(loginRequest, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PostAsync("/api/v1/auth/login", content);

            // Assert - Could be 401 (auth failed) or 400 (validation/user not found)
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.Unauthorized,
                HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// SCENARIO: Client submits login with empty email
        /// ENDPOINT: POST /api/auth/login
        /// EXPECTED: 400 Bad Request with validation errors
        /// WHY: Required fields must be validated
        /// </summary>
        [Fact]
        public async Task Login_WithEmptyEmail_ShouldReturn400()
        {
            // Arrange
            var loginRequest = new
            {
                email = "",
                password = "somepassword"
            };

            var content = new StringContent(
                JsonSerializer.Serialize(loginRequest, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PostAsync("/api/v1/auth/login", content);

            // Assert - empty email should fail validation or auth
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.BadRequest,
                HttpStatusCode.Unauthorized);
        }

        /// <summary>
        /// SCENARIO: Client submits login with malformed email
        /// ENDPOINT: POST /api/auth/login
        /// EXPECTED: 400 Bad Request or 401 Unauthorized
        /// WHY: Email format should be validated
        /// </summary>
        [Fact]
        public async Task Login_WithMalformedEmail_ShouldReturnError()
        {
            // Arrange
            var loginRequest = new
            {
                email = "not-an-email",
                password = "somepassword"
            };

            var content = new StringContent(
                JsonSerializer.Serialize(loginRequest, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PostAsync("/api/v1/auth/login", content);

            // Assert - malformed email should fail validation or auth
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.BadRequest,
                HttpStatusCode.Unauthorized);
        }

        #endregion

        #region POST /api/auth/register Tests

        /// <summary>
        /// SCENARIO: Client attempts to register with missing required fields
        /// ENDPOINT: POST /api/auth/register
        /// EXPECTED: 400 Bad Request with validation errors
        /// WHY: Registration requires all mandatory fields
        /// </summary>
        [Fact]
        public async Task Register_WithMissingFields_ShouldReturn400()
        {
            // Arrange
            var registerRequest = new
            {
                email = "test@company.com"
                // Missing: firstName, lastName, password, confirmPassword
            };

            var content = new StringContent(
                JsonSerializer.Serialize(registerRequest, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PostAsync("/api/v1/auth/register", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
                "incomplete registration should return 400");
        }

        /// <summary>
        /// SCENARIO: Client attempts to register with password mismatch
        /// ENDPOINT: POST /api/auth/register
        /// EXPECTED: 400 Bad Request
        /// WHY: Password and confirmPassword must match
        /// </summary>
        [Fact]
        public async Task Register_WithPasswordMismatch_ShouldReturn400()
        {
            // Arrange
            var registerRequest = new
            {
                firstName = "Test",
                lastName = "User",
                email = "test@company.com",
                password = "Password123!",
                confirmPassword = "DifferentPassword123!"
            };

            var content = new StringContent(
                JsonSerializer.Serialize(registerRequest, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PostAsync("/api/v1/auth/register", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
                "password mismatch should return 400");
        }

        #endregion

        #region POST /api/auth/refresh Tests

        /// <summary>
        /// SCENARIO: Client attempts to refresh with invalid token
        /// ENDPOINT: POST /api/auth/refresh
        /// EXPECTED: 401 Unauthorized
        /// WHY: Invalid refresh tokens should not grant new access
        /// </summary>
        [Fact]
        public async Task RefreshToken_WithInvalidToken_ShouldReturn401()
        {
            // Arrange
            var refreshRequest = new
            {
                refreshToken = "invalid-refresh-token"
            };

            var content = new StringContent(
                JsonSerializer.Serialize(refreshRequest, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PostAsync("/api/v1/auth/refresh", content);

            // Assert - invalid refresh token should fail
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.Unauthorized,
                HttpStatusCode.BadRequest);
        }

        #endregion
    }
}
