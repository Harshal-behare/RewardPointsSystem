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
    /// API Tests for RolesController endpoints
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
    public class RolesControllerApiTests : AuthenticatedApiTestBase
    {
        public RolesControllerApiTests(CustomWebApplicationFactory<Program> factory)
            : base(factory)
        {
        }

        #region GET /api/v1/roles Tests

        [Fact]
        public async Task GetAllRoles_WithoutAuthentication_Returns401()
        {
            // Arrange
            var client = CreateAnonymousClient();

            // Act
            var response = await client.GetAsync("/api/v1/roles");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetAllRoles_AsEmployee_Returns403()
        {
            // Arrange
            var client = CreateEmployeeClient();

            // Act
            var response = await client.GetAsync("/api/v1/roles");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetAllRoles_AsAdmin_Returns200()
        {
            // Arrange
            var client = CreateAdminClient();

            // Act
            var response = await client.GetAsync("/api/v1/roles");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        #endregion

        #region GET /api/v1/roles/{id} Tests

        [Fact]
        public async Task GetRoleById_WithoutAuthentication_Returns401()
        {
            // Arrange
            var client = CreateAnonymousClient();
            var roleId = Guid.NewGuid();

            // Act
            var response = await client.GetAsync($"/api/v1/roles/{roleId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetRoleById_AsEmployee_Returns403()
        {
            // Arrange
            var client = CreateEmployeeClient();
            var roleId = Guid.NewGuid();

            // Act
            var response = await client.GetAsync($"/api/v1/roles/{roleId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetRoleById_AsAdmin_WithNonExistentId_Returns404()
        {
            // Arrange
            var client = CreateAdminClient();
            var nonExistentId = Guid.NewGuid();

            // Act
            var response = await client.GetAsync($"/api/v1/roles/{nonExistentId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        #endregion

        #region POST /api/v1/roles Tests

        [Fact]
        public async Task CreateRole_WithoutAuthentication_Returns401()
        {
            // Arrange
            var client = CreateAnonymousClient();
            var content = CreateJsonContent(new
            {
                name = "TestRole",
                description = "Test role description"
            });

            // Act
            var response = await client.PostAsync("/api/v1/roles", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task CreateRole_AsEmployee_Returns403()
        {
            // Arrange
            var client = CreateEmployeeClient();
            var content = CreateJsonContent(new
            {
                name = "TestRole",
                description = "Test role description"
            });

            // Act
            var response = await client.PostAsync("/api/v1/roles", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task CreateRole_AsAdmin_WithValidData_Returns201()
        {
            // Arrange
            var client = CreateAdminClient();
            // Role name can only contain letters and spaces (per validator)
            var uniqueRoleName = $"TestRole {DateTime.UtcNow.Ticks % 1000000}".Replace("0", "A").Replace("1", "B").Replace("2", "C").Replace("3", "D").Replace("4", "E").Replace("5", "F").Replace("6", "G").Replace("7", "H").Replace("8", "I").Replace("9", "J");
            var content = CreateJsonContent(new
            {
                name = uniqueRoleName,
                description = "Test role created during API test"
            });

            // Act
            var response = await client.PostAsync("/api/v1/roles", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task CreateRole_AsAdmin_WithMissingName_Returns400Or422()
        {
            // Arrange
            var client = CreateAdminClient();
            var content = CreateJsonContent(new
            {
                // Missing name
                description = "Test role description"
            });

            // Act
            var response = await client.PostAsync("/api/v1/roles", content);

            // Assert
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.BadRequest,
                HttpStatusCode.UnprocessableEntity);
        }

        #endregion

        #region PUT /api/v1/roles/{id} Tests

        [Fact]
        public async Task UpdateRole_WithoutAuthentication_Returns401()
        {
            // Arrange
            var client = CreateAnonymousClient();
            var roleId = Guid.NewGuid();
            var content = CreateJsonContent(new
            {
                name = "UpdatedRole",
                description = "Updated description"
            });

            // Act
            var response = await client.PutAsync($"/api/v1/roles/{roleId}", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task UpdateRole_AsAdmin_WithNonExistentId_Returns404()
        {
            // Arrange
            var client = CreateAdminClient();
            var nonExistentId = Guid.NewGuid();
            var content = CreateJsonContent(new
            {
                name = "UpdatedRole",
                description = "Updated description"
            });

            // Act
            var response = await client.PutAsync($"/api/v1/roles/{nonExistentId}", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        #endregion

        #region DELETE /api/v1/roles/{id} Tests

        [Fact]
        public async Task DeleteRole_WithoutAuthentication_Returns401()
        {
            // Arrange
            var client = CreateAnonymousClient();
            var roleId = Guid.NewGuid();

            // Act
            var response = await client.DeleteAsync($"/api/v1/roles/{roleId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task DeleteRole_AsEmployee_Returns403()
        {
            // Arrange
            var client = CreateEmployeeClient();
            var roleId = Guid.NewGuid();

            // Act
            var response = await client.DeleteAsync($"/api/v1/roles/{roleId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task DeleteRole_AsAdmin_WithNonExistentId_Returns404()
        {
            // Arrange
            var client = CreateAdminClient();
            var nonExistentId = Guid.NewGuid();

            // Act
            var response = await client.DeleteAsync($"/api/v1/roles/{nonExistentId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        #endregion

        #region POST /api/v1/roles/users/{userId}/assign Tests

        [Fact]
        public async Task AssignRoleToUser_WithoutAuthentication_Returns401()
        {
            // Arrange
            var client = CreateAnonymousClient();
            var userId = Guid.NewGuid();
            var content = CreateJsonContent(new { roleId = Guid.NewGuid() });

            // Act
            var response = await client.PostAsync($"/api/v1/roles/users/{userId}/assign", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task AssignRoleToUser_AsEmployee_Returns403()
        {
            // Arrange
            var client = CreateEmployeeClient();
            var userId = Guid.NewGuid();
            var content = CreateJsonContent(new { roleId = Guid.NewGuid() });

            // Act
            var response = await client.PostAsync($"/api/v1/roles/users/{userId}/assign", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task AssignRoleToUser_AsAdmin_WithNonExistentUser_Returns400()
        {
            // Arrange
            var client = CreateAdminClient();
            var nonExistentUserId = Guid.NewGuid();
            var content = CreateJsonContent(new { roleId = Guid.NewGuid() });

            // Act
            var response = await client.PostAsync($"/api/v1/roles/users/{nonExistentUserId}/assign", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        #endregion
    }
}
