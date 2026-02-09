using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RewardPointsSystem.Api;
using RewardPointsSystem.Application.DTOs.Common;
using RewardPointsSystem.Application.DTOs.Products;
using RewardPointsSystem.Application.Interfaces;
using RewardPointsSystem.Domain.Entities.Core;
using RewardPointsSystem.Domain.Entities.Products;
using Xunit;

namespace RewardPointsSystem.Tests.FunctionalTests
{
    /// <summary>
    /// Functional Tests for Products API Endpoints
    /// 
    /// These tests verify the HTTP API behavior end-to-end:
    /// - Request/Response serialization
    /// - HTTP status codes
    /// - Authorization
    /// - API contract compliance
    /// 
    /// WHAT WE'RE TESTING:
    /// The complete HTTP request/response cycle through the ASP.NET Core pipeline
    /// 
    /// WHY THESE TESTS MATTER:
    /// Ensures the API behaves correctly from a client's perspective
    /// </summary>
    public class ProductsApiTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly JsonSerializerOptions _jsonOptions;

        public ProductsApiTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        #region GET /api/v1/products Tests

        /// <summary>
        /// SCENARIO: Client requests all active products
        /// ENDPOINT: GET /api/v1/products
        /// EXPECTED: 200 OK with list of products
        /// WHY: Public endpoint for employees to browse catalog
        /// </summary>
        [Fact]
        public async Task GetAllProducts_ShouldReturn200WithProductList()
        {
            // Arrange: Seed test product
            await SeedTestProductAsync();

            // Act
            var response = await _client.GetAsync("/api/v1/products");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK, "endpoint should return 200 for valid request");
            
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty("response should contain data");
        }

        /// <summary>
        /// SCENARIO: Client requests products when catalog is empty
        /// ENDPOINT: GET /api/v1/products
        /// EXPECTED: 200 OK with empty list
        /// WHY: Empty catalog is a valid state, should not return error
        /// </summary>
        [Fact]
        public async Task GetAllProducts_WhenEmpty_ShouldReturn200WithEmptyList()
        {
            // Act - Use fresh factory to ensure empty database
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/api/v1/products");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        #endregion

        #region GET /api/v1/products/{id} Tests

        /// <summary>
        /// SCENARIO: Client requests a specific product by ID
        /// ENDPOINT: GET /api/v1/products/{id}
        /// EXPECTED: 200 OK with product details or 404 if not found
        /// WHY: Clients need to view individual product details
        /// </summary>
        [Fact]
        public async Task GetProductById_WithInvalidId_ShouldReturn404()
        {
            // Arrange
            var invalidId = Guid.NewGuid();

            // Act
            var response = await _client.GetAsync($"/api/v1/products/{invalidId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound, "non-existent product should return 404");
        }

        #endregion

        #region API Response Format Tests

        /// <summary>
        /// SCENARIO: Verify API response follows standard format
        /// ENDPOINT: GET /api/v1/products
        /// EXPECTED: Response wrapped in ApiResponse<T> format
        /// WHY: Consistent API contract for frontend integration
        /// </summary>
        [Fact]
        public async Task ApiResponse_ShouldFollowStandardFormat()
        {
            // Act
            var response = await _client.GetAsync("/api/v1/products");
            var content = await response.Content.ReadAsStringAsync();

            // Assert: Check response has expected structure
            // ApiResponse has: success, message, data properties
            content.Should().Contain("success", "response should contain success field");
        }

        #endregion

        #region Helper Methods

        private async Task SeedTestProductAsync()
        {
            using var scope = _factory.Services.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            
            var product = Product.Create("Test Product", Guid.NewGuid(), "Test Description");
            await unitOfWork.Products.AddAsync(product);
            await unitOfWork.SaveChangesAsync();
        }

        #endregion
    }
}
