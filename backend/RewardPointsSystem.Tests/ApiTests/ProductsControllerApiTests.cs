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
    /// API Tests for ProductsController endpoints
    /// 
    /// Tests authentication/authorization requirements and HTTP behavior
    /// Following: Isolation, AAA pattern, determinism
    /// 
    /// Coverage:
    /// - Public endpoints accessible without authentication
    /// - Admin-only endpoints reject non-admin users
    /// - Valid requests return expected status codes
    /// </summary>
    public class ProductsControllerApiTests : AuthenticatedApiTestBase
    {
        public ProductsControllerApiTests(CustomWebApplicationFactory<Program> factory)
            : base(factory)
        {
        }

        #region GET /api/v1/products Tests

        [Fact]
        public async Task GetAllProducts_WithoutAuthentication_Returns200()
        {
            // Arrange
            var client = CreateAnonymousClient();

            // Act
            var response = await client.GetAsync("/api/v1/products");

            // Assert - public endpoint
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetAllProducts_AsEmployee_Returns200()
        {
            // Arrange
            var client = CreateEmployeeClient();

            // Act
            var response = await client.GetAsync("/api/v1/products");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        #endregion

        #region GET /api/v1/products/admin/all Tests

        [Fact]
        public async Task GetAllProductsAdmin_WithoutAuthentication_Returns401()
        {
            // Arrange
            var client = CreateAnonymousClient();

            // Act
            var response = await client.GetAsync("/api/v1/products/admin/all");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetAllProductsAdmin_AsEmployee_Returns403()
        {
            // Arrange
            var client = CreateEmployeeClient();

            // Act
            var response = await client.GetAsync("/api/v1/products/admin/all");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetAllProductsAdmin_AsAdmin_Returns200()
        {
            // Arrange
            var client = CreateAdminClient();

            // Act
            var response = await client.GetAsync("/api/v1/products/admin/all");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        #endregion

        #region GET /api/v1/products/{id} Tests

        [Fact]
        public async Task GetProductById_WithNonExistentId_Returns404()
        {
            // Arrange
            var client = CreateAnonymousClient();
            var nonExistentId = Guid.NewGuid();

            // Act
            var response = await client.GetAsync($"/api/v1/products/{nonExistentId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetProductById_WithInvalidGuidFormat_Returns400Or404()
        {
            // Arrange
            var client = CreateAnonymousClient();

            // Act
            var response = await client.GetAsync("/api/v1/products/invalid-guid");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
        }

        #endregion

        #region POST /api/v1/products (Admin) Tests

        [Fact]
        public async Task CreateProduct_WithoutAuthentication_Returns401()
        {
            // Arrange
            var client = CreateAnonymousClient();
            var content = CreateJsonContent(new
            {
                name = "Test Product",
                description = "Test Description",
                pointsCost = 100
            });

            // Act
            var response = await client.PostAsync("/api/v1/products", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task CreateProduct_AsEmployee_Returns403()
        {
            // Arrange
            var client = CreateEmployeeClient();
            var content = CreateJsonContent(new
            {
                name = "Test Product",
                description = "Test Description",
                pointsCost = 100
            });

            // Act
            var response = await client.PostAsync("/api/v1/products", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task CreateProduct_AsAdmin_WithMissingRequiredFields_Returns400Or422()
        {
            // Arrange
            var client = CreateAdminClient();
            var content = CreateJsonContent(new
            {
                // Missing name and pointsCost
                description = "Missing required fields"
            });

            // Act
            var response = await client.PostAsync("/api/v1/products", content);

            // Assert
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.BadRequest,
                HttpStatusCode.UnprocessableEntity);
        }

        #endregion

        #region PUT /api/v1/products/{id} (Admin) Tests

        [Fact]
        public async Task UpdateProduct_WithoutAuthentication_Returns401()
        {
            // Arrange
            var client = CreateAnonymousClient();
            var productId = Guid.NewGuid();
            var content = CreateJsonContent(new
            {
                name = "Updated Product",
                description = "Updated Description",
                pointsCost = 150
            });

            // Act
            var response = await client.PutAsync($"/api/v1/products/{productId}", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task UpdateProduct_AsEmployee_Returns403()
        {
            // Arrange
            var client = CreateEmployeeClient();
            var productId = Guid.NewGuid();
            var content = CreateJsonContent(new
            {
                name = "Updated Product",
                description = "Updated Description",
                pointsCost = 150
            });

            // Act
            var response = await client.PutAsync($"/api/v1/products/{productId}", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task UpdateProduct_AsAdmin_WithNonExistentId_Returns404()
        {
            // Arrange
            var client = CreateAdminClient();
            var nonExistentId = Guid.NewGuid();
            var content = CreateJsonContent(new
            {
                name = "Updated Product",
                description = "Updated Description",
                pointsCost = 150
            });

            // Act
            var response = await client.PutAsync($"/api/v1/products/{nonExistentId}", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        #endregion

        #region PATCH /api/v1/products/{id}/deactivate (Admin) Tests

        [Fact]
        public async Task DeactivateProduct_WithoutAuthentication_Returns401()
        {
            // Arrange
            var client = CreateAnonymousClient();
            var productId = Guid.NewGuid();

            // Act
            var response = await client.PatchAsync($"/api/v1/products/{productId}/deactivate", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task DeactivateProduct_AsEmployee_Returns403()
        {
            // Arrange
            var client = CreateEmployeeClient();
            var productId = Guid.NewGuid();

            // Act
            var response = await client.PatchAsync($"/api/v1/products/{productId}/deactivate", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task DeactivateProduct_AsAdmin_WithNonExistentId_Returns404()
        {
            // Arrange
            var client = CreateAdminClient();
            var nonExistentId = Guid.NewGuid();

            // Act
            var response = await client.PatchAsync($"/api/v1/products/{nonExistentId}/deactivate", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        #endregion

        #region GET /api/v1/products/category/{categoryId} Tests

        [Fact]
        public async Task GetProductsByCategory_WithNonExistentCategoryId_Returns200EmptyList()
        {
            // Arrange
            var client = CreateAnonymousClient();
            var nonExistentCategoryId = Guid.NewGuid();

            // Act
            var response = await client.GetAsync($"/api/v1/products/category/{nonExistentCategoryId}");

            // Assert - should return 200 with empty list, not 404
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        #endregion

        #region GET /api/v1/products/categories Tests

        [Fact]
        public async Task GetCategories_WithoutAuthentication_Returns200()
        {
            // Arrange
            var client = CreateAnonymousClient();

            // Act
            var response = await client.GetAsync("/api/v1/products/categories");

            // Assert - public endpoint
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        #endregion

        #region GET /api/v1/products/categories/admin/all Tests

        [Fact]
        public async Task GetAllCategoriesAdmin_WithoutAuthentication_Returns401()
        {
            // Arrange
            var client = CreateAnonymousClient();

            // Act
            var response = await client.GetAsync("/api/v1/products/categories/admin/all");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetAllCategoriesAdmin_AsAdmin_Returns200()
        {
            // Arrange
            var client = CreateAdminClient();

            // Act
            var response = await client.GetAsync("/api/v1/products/categories/admin/all");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        #endregion

        #region POST /api/v1/products/categories (Admin) Tests

        [Fact]
        public async Task CreateCategory_WithoutAuthentication_Returns401()
        {
            // Arrange
            var client = CreateAnonymousClient();
            var content = CreateJsonContent(new
            {
                name = "Test Category",
                description = "Test category description"
            });

            // Act
            var response = await client.PostAsync("/api/v1/products/categories", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task CreateCategory_AsEmployee_Returns403()
        {
            // Arrange
            var client = CreateEmployeeClient();
            var content = CreateJsonContent(new
            {
                name = "Test Category",
                description = "Test category description"
            });

            // Act
            var response = await client.PostAsync("/api/v1/products/categories", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task CreateCategory_AsAdmin_WithValidData_Returns201()
        {
            // Arrange
            var client = CreateAdminClient();
            var uniqueCategoryName = $"TestCategory_{Guid.NewGuid():N}";
            var content = CreateJsonContent(new
            {
                name = uniqueCategoryName,
                description = "Test category created during API test"
            });

            // Act
            var response = await client.PostAsync("/api/v1/products/categories", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        #endregion
    }
}
