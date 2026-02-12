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
    /// API Tests for EventsController endpoints
    /// 
    /// Tests authentication/authorization requirements and HTTP behavior
    /// Following: Isolation, AAA pattern, determinism
    /// 
    /// Coverage:
    /// - Public endpoints accessible without authentication
    /// - Admin-only endpoints reject non-admin users
    /// - Valid requests return expected status codes
    /// </summary>
    public class EventsControllerApiTests : AuthenticatedApiTestBase
    {
        public EventsControllerApiTests(CustomWebApplicationFactory<Program> factory)
            : base(factory)
        {
        }

        #region GET /api/v1/events Tests

        [Fact]
        public async Task GetAllEvents_WithoutAuthentication_Returns200()
        {
            // Arrange
            var client = CreateAnonymousClient();

            // Act
            var response = await client.GetAsync("/api/v1/events");

            // Assert - public endpoint, should return 200
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetAllEvents_AsAuthenticatedUser_Returns200()
        {
            // Arrange
            var client = CreateEmployeeClient();

            // Act
            var response = await client.GetAsync("/api/v1/events");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        #endregion

        #region GET /api/v1/events/{id} Tests

        [Fact]
        public async Task GetEventById_WithNonExistentId_Returns404()
        {
            // Arrange
            var client = CreateAnonymousClient();
            var nonExistentId = Guid.NewGuid();

            // Act
            var response = await client.GetAsync($"/api/v1/events/{nonExistentId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetEventById_WithInvalidGuidFormat_Returns400Or404()
        {
            // Arrange
            var client = CreateAnonymousClient();

            // Act
            var response = await client.GetAsync("/api/v1/events/invalid-guid");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
        }

        #endregion

        #region POST /api/v1/events (Admin) Tests

        [Fact]
        public async Task CreateEvent_WithoutAuthentication_Returns401()
        {
            // Arrange
            var client = CreateAnonymousClient();
            var content = CreateJsonContent(new
            {
                name = "Test Event",
                description = "Test Description",
                eventDate = DateTime.UtcNow.AddDays(7),
                totalPointsPool = 1000
            });

            // Act
            var response = await client.PostAsync("/api/v1/events", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task CreateEvent_AsEmployee_Returns403()
        {
            // Arrange
            var client = CreateEmployeeClient();
            var content = CreateJsonContent(new
            {
                name = "Test Event",
                description = "Test Description",
                eventDate = DateTime.UtcNow.AddDays(7),
                totalPointsPool = 1000
            });

            // Act
            var response = await client.PostAsync("/api/v1/events", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task CreateEvent_AsAdmin_WithValidData_Returns201()
        {
            // Arrange
            var client = CreateAdminClient();
            var content = CreateJsonContent(new
            {
                name = "Integration Test Event",
                description = "Created during API test",
                eventDate = DateTime.UtcNow.AddDays(14),
                totalPointsPool = 500
            });

            // Act
            var response = await client.PostAsync("/api/v1/events", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task CreateEvent_AsAdmin_WithMissingRequiredFields_Returns400Or422()
        {
            // Arrange
            var client = CreateAdminClient();
            var content = CreateJsonContent(new
            {
                // Missing name, eventDate, totalPointsPool
                description = "Missing required fields"
            });

            // Act
            var response = await client.PostAsync("/api/v1/events", content);

            // Assert
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.BadRequest,
                HttpStatusCode.UnprocessableEntity);
        }

        #endregion

        #region PUT /api/v1/events/{id} (Admin) Tests

        [Fact]
        public async Task UpdateEvent_WithoutAuthentication_Returns401()
        {
            // Arrange
            var client = CreateAnonymousClient();
            var eventId = Guid.NewGuid();
            var content = CreateJsonContent(new
            {
                name = "Updated Event",
                description = "Updated Description",
                eventDate = DateTime.UtcNow.AddDays(7),
                totalPointsPool = 1000
            });

            // Act
            var response = await client.PutAsync($"/api/v1/events/{eventId}", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task UpdateEvent_AsEmployee_Returns403()
        {
            // Arrange
            var client = CreateEmployeeClient();
            var eventId = Guid.NewGuid();
            var content = CreateJsonContent(new
            {
                name = "Updated Event",
                description = "Updated Description",
                eventDate = DateTime.UtcNow.AddDays(7),
                totalPointsPool = 1000
            });

            // Act
            var response = await client.PutAsync($"/api/v1/events/{eventId}", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task UpdateEvent_AsAdmin_WithNonExistentId_Returns404()
        {
            // Arrange
            var client = CreateAdminClient();
            var nonExistentId = Guid.NewGuid();
            var content = CreateJsonContent(new
            {
                name = "Updated Event",
                description = "Updated Description",
                eventDate = DateTime.UtcNow.AddDays(7),
                totalPointsPool = 1000
            });

            // Act
            var response = await client.PutAsync($"/api/v1/events/{nonExistentId}", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        #endregion

        #region DELETE /api/v1/events/{id} (Admin) Tests

        [Fact]
        public async Task DeleteEvent_WithoutAuthentication_Returns401()
        {
            // Arrange
            var client = CreateAnonymousClient();
            var eventId = Guid.NewGuid();

            // Act
            var response = await client.DeleteAsync($"/api/v1/events/{eventId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task DeleteEvent_AsEmployee_Returns403()
        {
            // Arrange
            var client = CreateEmployeeClient();
            var eventId = Guid.NewGuid();

            // Act
            var response = await client.DeleteAsync($"/api/v1/events/{eventId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task DeleteEvent_AsAdmin_WithNonExistentId_Returns404()
        {
            // Arrange
            var client = CreateAdminClient();
            var nonExistentId = Guid.NewGuid();

            // Act
            var response = await client.DeleteAsync($"/api/v1/events/{nonExistentId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        #endregion

        #region PATCH /api/v1/events/{id}/status (Admin) Tests

        [Fact]
        public async Task ChangeEventStatus_WithoutAuthentication_Returns401()
        {
            // Arrange
            var client = CreateAnonymousClient();
            var eventId = Guid.NewGuid();
            var content = CreateJsonContent(new { status = "Active" });

            // Act
            var response = await client.PatchAsync($"/api/v1/events/{eventId}/status", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task ChangeEventStatus_AsEmployee_Returns403()
        {
            // Arrange
            var client = CreateEmployeeClient();
            var eventId = Guid.NewGuid();
            var content = CreateJsonContent(new { status = "Active" });

            // Act
            var response = await client.PatchAsync($"/api/v1/events/{eventId}/status", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task ChangeEventStatus_AsAdmin_WithNonExistentId_Returns404()
        {
            // Arrange
            var client = CreateAdminClient();
            var nonExistentId = Guid.NewGuid();
            var content = CreateJsonContent(new { status = "Active" });

            // Act
            var response = await client.PatchAsync($"/api/v1/events/{nonExistentId}/status", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        #endregion

        #region GET /api/v1/events/{id}/participants Tests

        [Fact]
        public async Task GetEventParticipants_WithoutAuthentication_Returns401()
        {
            // Arrange
            var client = CreateAnonymousClient();
            var eventId = Guid.NewGuid();

            // Act
            var response = await client.GetAsync($"/api/v1/events/{eventId}/participants");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetEventParticipants_AsEmployee_Returns200Or404()
        {
            // Arrange
            var client = CreateEmployeeClient();
            var eventId = Guid.NewGuid();

            // Act
            var response = await client.GetAsync($"/api/v1/events/{eventId}/participants");

            // Assert - 200 with empty list or 404 if event not found
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }

        #endregion

        #region POST /api/v1/events/{id}/participants Tests

        [Fact]
        public async Task RegisterParticipant_WithoutAuthentication_Returns401()
        {
            // Arrange
            var client = CreateAnonymousClient();
            var eventId = Guid.NewGuid();
            var content = CreateJsonContent(new { userId = Guid.NewGuid() });

            // Act
            var response = await client.PostAsync($"/api/v1/events/{eventId}/participants", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task RegisterParticipant_WithNonExistentEvent_Returns400()
        {
            // Arrange
            var client = CreateEmployeeClient();
            var nonExistentEventId = Guid.NewGuid();
            var content = CreateJsonContent(new { userId = Guid.NewGuid() });

            // Act
            var response = await client.PostAsync($"/api/v1/events/{nonExistentEventId}/participants", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        #endregion

        #region POST /api/v1/events/{id}/award-winners (Admin) Tests

        [Fact]
        public async Task AwardWinners_WithoutAuthentication_Returns401()
        {
            // Arrange
            var client = CreateAnonymousClient();
            var eventId = Guid.NewGuid();
            var content = CreateJsonContent(new
            {
                awards = new[]
                {
                    new { userId = Guid.NewGuid(), points = 100, rank = 1 }
                }
            });

            // Act
            var response = await client.PostAsync($"/api/v1/events/{eventId}/award-winners", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task AwardWinners_AsEmployee_Returns403()
        {
            // Arrange
            var client = CreateEmployeeClient();
            var eventId = Guid.NewGuid();
            var content = CreateJsonContent(new
            {
                awards = new[]
                {
                    new { userId = Guid.NewGuid(), points = 100, rank = 1 }
                }
            });

            // Act
            var response = await client.PostAsync($"/api/v1/events/{eventId}/award-winners", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task AwardWinners_AsAdmin_WithNonExistentEvent_Returns404()
        {
            // Arrange
            var client = CreateAdminClient();
            var nonExistentEventId = Guid.NewGuid();
            var content = CreateJsonContent(new
            {
                awards = new[]
                {
                    new { userId = Guid.NewGuid(), points = 100, rank = 1 }
                }
            });

            // Act
            var response = await client.PostAsync($"/api/v1/events/{nonExistentEventId}/award-winners", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        #endregion
    }
}
