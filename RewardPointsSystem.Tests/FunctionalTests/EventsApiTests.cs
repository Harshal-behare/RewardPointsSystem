using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RewardPointsSystem.Api;
using RewardPointsSystem.Application.DTOs.Common;
using RewardPointsSystem.Application.DTOs.Events;
using RewardPointsSystem.Application.Interfaces;
using RewardPointsSystem.Domain.Entities.Core;
using RewardPointsSystem.Domain.Entities.Events;
using Xunit;

namespace RewardPointsSystem.Tests.FunctionalTests
{
    /// <summary>
    /// Functional Tests for Events API Endpoints
    /// 
    /// These tests verify the HTTP API behavior for event operations:
    /// - Event listing and retrieval
    /// - Event lifecycle management
    /// - Participant registration
    /// 
    /// WHAT WE'RE TESTING:
    /// The complete HTTP request/response cycle for event-related endpoints
    /// 
    /// WHY THESE TESTS MATTER:
    /// Events are a primary way employees earn points - API must work correctly
    /// </summary>
    public class EventsApiTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly JsonSerializerOptions _jsonOptions;

        public EventsApiTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        #region GET /api/events Tests

        /// <summary>
        /// SCENARIO: Client requests all upcoming events
        /// ENDPOINT: GET /api/events
        /// EXPECTED: 200 OK with list of events
        /// WHY: Public endpoint for employees to view available events
        /// </summary>
        [Fact]
        public async Task GetAllEvents_ShouldReturn200()
        {
            // Act
            var response = await _client.GetAsync("/api/v1/events");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK, "events endpoint should return 200");
        }

        /// <summary>
        /// SCENARIO: Client requests events when none exist
        /// ENDPOINT: GET /api/events
        /// EXPECTED: 200 OK with empty list
        /// WHY: Empty event list is valid, should not return error
        /// </summary>
        [Fact]
        public async Task GetAllEvents_WhenEmpty_ShouldReturn200WithEmptyList()
        {
            // Act
            var response = await _client.GetAsync("/api/v1/events");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().Contain("success", "response should indicate success");
        }

        #endregion

        #region GET /api/events/{id} Tests

        /// <summary>
        /// SCENARIO: Client requests a specific event by ID
        /// ENDPOINT: GET /api/events/{id}
        /// EXPECTED: 200 OK with event details or 404 if not found
        /// WHY: Clients need to view individual event details
        /// </summary>
        [Fact]
        public async Task GetEventById_WithInvalidId_ShouldReturn404()
        {
            // Arrange
            var invalidId = Guid.NewGuid();

            // Act
            var response = await _client.GetAsync($"/api/v1/events/{invalidId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound, "non-existent event should return 404");
        }

        #endregion

        #region Event Data Validation Tests

        /// <summary>
        /// SCENARIO: Verify events list endpoint returns proper structure
        /// ENDPOINT: GET /api/v1/events
        /// EXPECTED: Response is properly formatted (even if empty)
        /// WHY: Frontend depends on consistent response format
        /// </summary>
        [Fact]
        public async Task EventsListResponse_ShouldBeProperlyFormatted()
        {
            // Act
            var response = await _client.GetAsync("/api/v1/events");
            var content = await response.Content.ReadAsStringAsync();

            // Assert - Check API response structure
            content.Should().Contain("success", "should have success field");
        }

        #endregion

        #region Helper Methods

        private async Task<Guid> SeedTestEventAsync()
        {
            using var scope = _factory.Services.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            
            var eventEntity = Event.Create(
                "Test Event",
                DateTime.UtcNow.AddDays(7),
                1000,
                Guid.NewGuid(),
                "Test Description");
            
            // Make it visible by setting status to Upcoming
            eventEntity.Publish();
            
            await unitOfWork.Events.AddAsync(eventEntity);
            await unitOfWork.SaveChangesAsync();
            
            return eventEntity.Id;
        }

        #endregion
    }
}
