using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using RewardPointsSystem.Domain.Entities.Events;
using RewardPointsSystem.Domain.Entities.Core;
using RewardPointsSystem.Domain.Exceptions;
using RewardPointsSystem.Infrastructure.Repositories;
using RewardPointsSystem.Application.Services.Events;
using RewardPointsSystem.Application.DTOs;
using Xunit;

namespace RewardPointsSystem.Tests.UnitTests
{
    /// <summary>
    /// Unit tests for EventService
    /// 
    /// These tests verify that the event lifecycle management works correctly.
    /// Events follow this state machine: Draft → Upcoming → Active → Completed
    /// 
    /// Key scenarios tested:
    /// - Creating events with valid and invalid data
    /// - Event status transitions (publish, activate, complete, revert to draft)
    /// - Event retrieval (by ID, upcoming events)
    /// - Event updates
    /// - Error handling for invalid operations
    /// </summary>
    public class EventServiceTests : IDisposable
    {
        private readonly InMemoryUnitOfWork _unitOfWork;
        private readonly EventService _eventService;

        public EventServiceTests()
        {
            _unitOfWork = new InMemoryUnitOfWork();
            _eventService = new EventService(_unitOfWork);
        }

        public void Dispose()
        {
            _unitOfWork?.Dispose();
        }

        #region Event Creation Tests

        /// <summary>
        /// SCENARIO: Create an event with all valid information
        /// EXPECTED: Event is created successfully with correct details and Draft status
        /// WHY: This is the basic happy path for creating a new event in the system
        /// </summary>
        [Fact]
        public async Task CreateEvent_WithValidData_ShouldCreateEventInDraftStatus()
        {
            // Arrange - Set up the event details
            var eventName = "Annual Sales Competition";
            var eventDescription = "Yearly competition for top sales performers";
            var eventDate = DateTime.UtcNow.AddDays(30);
            var pointsPool = 1000;

            // Act - Create the event
            var createdEvent = await _eventService.CreateEventAsync(
                eventName, 
                eventDescription, 
                eventDate, 
                pointsPool);

            // Assert - Verify the event was created correctly
            createdEvent.Should().NotBeNull("event should be created");
            createdEvent.Name.Should().Be(eventName, "event name should match");
            createdEvent.Description.Should().Be(eventDescription, "event description should match");
            createdEvent.EventDate.Should().Be(eventDate, "event date should match");
            createdEvent.TotalPointsPool.Should().Be(pointsPool, "points pool should match");
            createdEvent.Status.Should().Be(EventStatus.Draft, "new events start in Draft status");
            createdEvent.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5), "creation time should be recent");
        }

        /// <summary>
        /// SCENARIO: Try to create an event without a name
        /// EXPECTED: System rejects the request with appropriate error
        /// WHY: Event name is required for identification and display
        /// </summary>
        [Theory]
        [InlineData("", "Description", 100)]
        [InlineData("   ", "Description", 100)]
        public async Task CreateEvent_WithMissingName_ShouldRejectRequest(string name, string description, int pointsPool)
        {
            // Arrange - Set up the event date
            var futureDate = DateTime.UtcNow.AddDays(7);

            // Act & Assert - Try to create event and expect error
            await Assert.ThrowsAsync<InvalidEventDataException>(async () =>
                await _eventService.CreateEventAsync(name, description, futureDate, pointsPool));
        }

        /// <summary>
        /// SCENARIO: Try to create an event without a description
        /// EXPECTED: System rejects the request with appropriate error
        /// WHY: Description is required so participants understand the event
        /// </summary>
        [Theory]
        [InlineData("Event Name", "", 100)]
        [InlineData("Event Name", "   ", 100)]
        public async Task CreateEvent_WithMissingDescription_ShouldRejectRequest(string name, string description, int pointsPool)
        {
            // Arrange - Set up the event date
            var futureDate = DateTime.UtcNow.AddDays(7);

            // Act & Assert - Try to create event and expect error
            await Assert.ThrowsAsync<InvalidEventDataException>(async () =>
                await _eventService.CreateEventAsync(name, description, futureDate, pointsPool));
        }

        /// <summary>
        /// SCENARIO: Try to create an event with zero or negative points pool
        /// EXPECTED: System rejects the request with appropriate error
        /// WHY: Events must have a positive number of points to award
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(-100)]
        [InlineData(-1)]
        public async Task CreateEvent_WithInvalidPointsPool_ShouldRejectRequest(int invalidPointsPool)
        {
            // Arrange - Set up valid name and description
            var name = "Test Event";
            var description = "Test Description";
            var futureDate = DateTime.UtcNow.AddDays(7);

            // Act & Assert - Try to create event and expect error
            await Assert.ThrowsAsync<InvalidEventDataException>(async () =>
                await _eventService.CreateEventAsync(name, description, futureDate, invalidPointsPool));
        }

        /// <summary>
        /// SCENARIO: Try to create an event with a date in the past
        /// EXPECTED: System rejects the request with appropriate error
        /// WHY: Events must be scheduled for the future
        /// </summary>
        [Fact]
        public async Task CreateEvent_WithPastDate_ShouldRejectRequest()
        {
            // Arrange - Set up event with past date
            var name = "Past Event";
            var description = "This event is in the past";
            var pastDate = DateTime.UtcNow.AddDays(-5);
            var pointsPool = 500;

            // Act & Assert - Try to create event and expect error
            await Assert.ThrowsAsync<InvalidEventDataException>(async () =>
                await _eventService.CreateEventAsync(name, description, pastDate, pointsPool));
        }

        #endregion

        #region Event Retrieval Tests

        /// <summary>
        /// SCENARIO: Retrieve an event that exists in the system
        /// EXPECTED: The correct event is returned with all details
        /// WHY: Users need to view event details after creation
        /// </summary>
        [Fact]
        public async Task GetEventById_WhenEventExists_ShouldReturnEvent()
        {
            // Arrange - Create an event first
            var createdEvent = await _eventService.CreateEventAsync(
                "Findable Event", 
                "This event should be found", 
                DateTime.UtcNow.AddDays(10), 
                500);

            // Act - Try to retrieve the event
            var retrievedEvent = await _eventService.GetEventByIdAsync(createdEvent.Id);

            // Assert - Verify we got the right event
            retrievedEvent.Should().NotBeNull("event should be found");
            retrievedEvent.Id.Should().Be(createdEvent.Id, "IDs should match");
            retrievedEvent.Name.Should().Be("Findable Event", "name should match");
        }

        /// <summary>
        /// SCENARIO: Try to retrieve an event that doesn't exist
        /// EXPECTED: Returns null (not found)
        /// WHY: System should gracefully handle requests for non-existent events
        /// </summary>
        [Fact]
        public async Task GetEventById_WhenEventDoesNotExist_ShouldReturnNull()
        {
            // Arrange - Use a random ID that doesn't exist
            var nonExistentId = Guid.NewGuid();

            // Act - Try to retrieve the non-existent event
            var result = await _eventService.GetEventByIdAsync(nonExistentId);

            // Assert - Should return null
            result.Should().BeNull("non-existent events should return null");
        }

        /// <summary>
        /// SCENARIO: Get all upcoming events (events visible to employees)
        /// EXPECTED: Only published (Upcoming status) events are returned
        /// WHY: Employees should only see events they can register for
        /// </summary>
        [Fact]
        public async Task GetUpcomingEvents_ShouldReturnOnlyPublishedEvents()
        {
            // Arrange - Create multiple events in different states
            var draftEvent = await _eventService.CreateEventAsync("Draft Event", "Still in draft", DateTime.UtcNow.AddDays(5), 100);
            var upcomingEvent1 = await _eventService.CreateEventAsync("Upcoming Event 1", "Published event", DateTime.UtcNow.AddDays(10), 200);
            var upcomingEvent2 = await _eventService.CreateEventAsync("Upcoming Event 2", "Another published event", DateTime.UtcNow.AddDays(15), 300);
            
            // Publish some events (Draft → Upcoming)
            await _eventService.PublishEventAsync(upcomingEvent1.Id);
            await _eventService.PublishEventAsync(upcomingEvent2.Id);

            // Act - Get upcoming events
            var upcomingEvents = await _eventService.GetUpcomingEventsAsync();

            // Assert - Should only see published events
            upcomingEvents.Should().HaveCount(2, "only 2 events are in Upcoming status");
            upcomingEvents.Should().Contain(e => e.Id == upcomingEvent1.Id, "first published event should be included");
            upcomingEvents.Should().Contain(e => e.Id == upcomingEvent2.Id, "second published event should be included");
            upcomingEvents.Should().NotContain(e => e.Id == draftEvent.Id, "draft event should not be included");
        }

        #endregion

        #region Event Status Transition Tests

        /// <summary>
        /// SCENARIO: Publish a draft event to make it visible to employees
        /// EXPECTED: Event status changes from Draft to Upcoming
        /// WHY: This is how admins make events available for employee registration
        /// </summary>
        [Fact]
        public async Task PublishEvent_FromDraftStatus_ShouldMakeEventUpcoming()
        {
            // Arrange - Create an event (starts in Draft status)
            var draftEvent = await _eventService.CreateEventAsync(
                "Event to Publish", 
                "Will be published", 
                DateTime.UtcNow.AddDays(7), 
                500);
            
            draftEvent.Status.Should().Be(EventStatus.Draft, "event should start in Draft");

            // Act - Publish the event
            await _eventService.PublishEventAsync(draftEvent.Id);

            // Assert - Verify status changed
            var publishedEvent = await _eventService.GetEventByIdAsync(draftEvent.Id);
            publishedEvent.Status.Should().Be(EventStatus.Upcoming, "status should change to Upcoming after publishing");
        }

        /// <summary>
        /// SCENARIO: Try to publish an event that is already Upcoming
        /// EXPECTED: System rejects the operation with appropriate error
        /// WHY: Events can only be published from Draft status
        /// </summary>
        [Fact]
        public async Task PublishEvent_WhenAlreadyUpcoming_ShouldRejectOperation()
        {
            // Arrange - Create and publish an event
            var eventEntity = await _eventService.CreateEventAsync("Event", "Desc", DateTime.UtcNow.AddDays(7), 500);
            await _eventService.PublishEventAsync(eventEntity.Id);

            // Act & Assert - Try to publish again
            await Assert.ThrowsAsync<InvalidEventStateException>(async () =>
                await _eventService.PublishEventAsync(eventEntity.Id));
        }

        /// <summary>
        /// SCENARIO: Activate an upcoming event to indicate it's in progress
        /// EXPECTED: Event status changes from Upcoming to Active
        /// WHY: This marks that the event has started
        /// </summary>
        [Fact]
        public async Task ActivateEvent_FromUpcomingStatus_ShouldMakeEventActive()
        {
            // Arrange - Create and publish an event
            var eventEntity = await _eventService.CreateEventAsync(
                "Event to Activate", 
                "Will be activated", 
                DateTime.UtcNow.AddDays(7), 
                500);
            await _eventService.PublishEventAsync(eventEntity.Id);

            // Act - Activate the event
            await _eventService.ActivateEventAsync(eventEntity.Id);

            // Assert - Verify status changed
            var activatedEvent = await _eventService.GetEventByIdAsync(eventEntity.Id);
            activatedEvent.Status.Should().Be(EventStatus.Active, "status should change to Active");
        }

        /// <summary>
        /// SCENARIO: Try to activate an event that is still in Draft status
        /// EXPECTED: System rejects the operation with appropriate error
        /// WHY: Events must be published before they can be activated
        /// </summary>
        [Fact]
        public async Task ActivateEvent_FromDraftStatus_ShouldRejectOperation()
        {
            // Arrange - Create event but don't publish it
            var draftEvent = await _eventService.CreateEventAsync("Draft Event", "Still draft", DateTime.UtcNow.AddDays(7), 500);

            // Act & Assert - Try to activate directly
            await Assert.ThrowsAsync<InvalidEventStateException>(async () =>
                await _eventService.ActivateEventAsync(draftEvent.Id));
        }

        /// <summary>
        /// SCENARIO: Try to activate a non-existent event
        /// EXPECTED: System returns not found error
        /// WHY: Cannot perform operations on events that don't exist
        /// </summary>
        [Fact]
        public async Task ActivateEvent_WhenEventDoesNotExist_ShouldThrowNotFound()
        {
            // Arrange - Use a random ID
            var nonExistentId = Guid.NewGuid();

            // Act & Assert - Try to activate non-existent event
            await Assert.ThrowsAsync<EventNotFoundException>(async () =>
                await _eventService.ActivateEventAsync(nonExistentId));
        }

        /// <summary>
        /// SCENARIO: Complete an active event
        /// EXPECTED: Event status changes to Completed and completion time is recorded
        /// WHY: Completing an event enables point awarding to participants
        /// </summary>
        [Fact]
        public async Task CompleteEvent_FromActiveStatus_ShouldMarkEventCompleted()
        {
            // Arrange - Create, publish, and activate an event
            var eventEntity = await _eventService.CreateEventAsync(
                "Event to Complete", 
                "Will be completed", 
                DateTime.UtcNow.AddDays(7), 
                500);
            await _eventService.PublishEventAsync(eventEntity.Id);
            await _eventService.ActivateEventAsync(eventEntity.Id);

            // Act - Complete the event
            await _eventService.CompleteEventAsync(eventEntity.Id);

            // Assert - Verify status and completion time
            var completedEvent = await _eventService.GetEventByIdAsync(eventEntity.Id);
            completedEvent.Status.Should().Be(EventStatus.Completed, "status should change to Completed");
            completedEvent.CompletedAt.Should().NotBeNull("completion time should be recorded");
            completedEvent.CompletedAt!.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5), "completion time should be recent");
        }

        /// <summary>
        /// SCENARIO: Complete an upcoming event directly (skip active phase)
        /// EXPECTED: Event can be completed from Upcoming status
        /// WHY: Some events may complete without an active phase (e.g., one-time events)
        /// </summary>
        [Fact]
        public async Task CompleteEvent_FromUpcomingStatus_ShouldMarkEventCompleted()
        {
            // Arrange - Create and publish an event (but don't activate)
            var eventEntity = await _eventService.CreateEventAsync(
                "Quick Event", 
                "Completes from upcoming", 
                DateTime.UtcNow.AddDays(7), 
                500);
            await _eventService.PublishEventAsync(eventEntity.Id);

            // Act - Complete directly from upcoming
            await _eventService.CompleteEventAsync(eventEntity.Id);

            // Assert - Verify completion
            var completedEvent = await _eventService.GetEventByIdAsync(eventEntity.Id);
            completedEvent.Status.Should().Be(EventStatus.Completed, "event should be completed");
        }

        /// <summary>
        /// SCENARIO: Try to complete a draft event
        /// EXPECTED: System rejects the operation with appropriate error
        /// WHY: Draft events cannot be completed without publishing first
        /// </summary>
        [Fact]
        public async Task CompleteEvent_FromDraftStatus_ShouldRejectOperation()
        {
            // Arrange - Create event but don't publish it
            var draftEvent = await _eventService.CreateEventAsync("Draft Event", "Still draft", DateTime.UtcNow.AddDays(7), 500);

            // Act & Assert - Try to complete directly
            await Assert.ThrowsAsync<InvalidEventStateException>(async () =>
                await _eventService.CompleteEventAsync(draftEvent.Id));
        }

        /// <summary>
        /// SCENARIO: Revert a published event back to draft (hide from employees)
        /// EXPECTED: Event status changes from Upcoming back to Draft
        /// WHY: Admins may need to hide an event to make changes before re-publishing
        /// </summary>
        [Fact]
        public async Task RevertToDraft_FromUpcomingStatus_ShouldHideEventFromEmployees()
        {
            // Arrange - Create and publish an event
            var eventEntity = await _eventService.CreateEventAsync(
                "Event to Revert", 
                "Will be reverted", 
                DateTime.UtcNow.AddDays(7), 
                500);
            await _eventService.PublishEventAsync(eventEntity.Id);
            
            // Verify it's upcoming
            var upcomingEvent = await _eventService.GetEventByIdAsync(eventEntity.Id);
            upcomingEvent.Status.Should().Be(EventStatus.Upcoming);

            // Act - Revert to draft
            await _eventService.RevertToDraftAsync(eventEntity.Id);

            // Assert - Verify status reverted
            var draftEvent = await _eventService.GetEventByIdAsync(eventEntity.Id);
            draftEvent.Status.Should().Be(EventStatus.Draft, "status should revert to Draft");
        }

        /// <summary>
        /// SCENARIO: Try to revert a completed event
        /// EXPECTED: System rejects the operation with appropriate error
        /// WHY: Completed events cannot be reverted (they're finalized)
        /// </summary>
        [Fact]
        public async Task RevertToDraft_FromCompletedStatus_ShouldRejectOperation()
        {
            // Arrange - Create, publish, and complete an event
            var eventEntity = await _eventService.CreateEventAsync("Event", "Desc", DateTime.UtcNow.AddDays(7), 500);
            await _eventService.PublishEventAsync(eventEntity.Id);
            await _eventService.CompleteEventAsync(eventEntity.Id);

            // Act & Assert - Try to revert completed event
            await Assert.ThrowsAsync<InvalidEventStateException>(async () =>
                await _eventService.RevertToDraftAsync(eventEntity.Id));
        }

        #endregion

        #region Event Update Tests

        /// <summary>
        /// SCENARIO: Update an event's details
        /// EXPECTED: Event is updated with new values
        /// WHY: Admins need to modify event details after creation
        /// </summary>
        [Fact]
        public async Task UpdateEvent_WithValidData_ShouldUpdateEventDetails()
        {
            // Arrange - Create an event
            var originalEvent = await _eventService.CreateEventAsync(
                "Original Name", 
                "Original Description", 
                DateTime.UtcNow.AddDays(7), 
                500);

            var updateDto = new UpdateEventDto
            {
                Name = "Updated Name",
                Description = "Updated Description",
                TotalPointsPool = 1000
            };

            // Act - Update the event
            var updatedEvent = await _eventService.UpdateEventAsync(originalEvent.Id, updateDto);

            // Assert - Verify updates
            updatedEvent.Name.Should().Be("Updated Name", "name should be updated");
            updatedEvent.Description.Should().Be("Updated Description", "description should be updated");
            updatedEvent.TotalPointsPool.Should().Be(1000, "points pool should be updated");
        }

        /// <summary>
        /// SCENARIO: Try to update a non-existent event
        /// EXPECTED: System returns not found error
        /// WHY: Cannot update events that don't exist
        /// </summary>
        [Fact]
        public async Task UpdateEvent_WhenEventDoesNotExist_ShouldThrowNotFound()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            var updateDto = new UpdateEventDto { Name = "New Name" };

            // Act & Assert
            await Assert.ThrowsAsync<EventNotFoundException>(async () =>
                await _eventService.UpdateEventAsync(nonExistentId, updateDto));
        }

        /// <summary>
        /// SCENARIO: Try to update a completed event (except status to completed)
        /// EXPECTED: System rejects the modification
        /// WHY: Completed events are finalized and should not be modified
        /// </summary>
        [Fact]
        public async Task UpdateEvent_WhenEventIsCompleted_ShouldRejectModification()
        {
            // Arrange - Create, publish, and complete an event
            var eventEntity = await _eventService.CreateEventAsync("Event", "Desc", DateTime.UtcNow.AddDays(7), 500);
            await _eventService.PublishEventAsync(eventEntity.Id);
            await _eventService.CompleteEventAsync(eventEntity.Id);

            var updateDto = new UpdateEventDto { Name = "New Name" };

            // Act & Assert - Try to update completed event
            await Assert.ThrowsAsync<InvalidEventStateException>(async () =>
                await _eventService.UpdateEventAsync(eventEntity.Id, updateDto));
        }

        #endregion

        #region Event Deletion Tests

        /// <summary>
        /// SCENARIO: Delete an event
        /// EXPECTED: Event is removed from the system
        /// WHY: Admins need to be able to remove events
        /// </summary>
        [Fact]
        public async Task DeleteEvent_WhenEventExists_ShouldRemoveEvent()
        {
            // Arrange - Create an event
            var eventEntity = await _eventService.CreateEventAsync(
                "Event to Delete", 
                "Will be deleted", 
                DateTime.UtcNow.AddDays(7), 
                500);
            var eventId = eventEntity.Id;

            // Act - Delete the event
            await _eventService.DeleteEventAsync(eventId);

            // Assert - Verify event no longer exists
            var deletedEvent = await _eventService.GetEventByIdAsync(eventId);
            deletedEvent.Should().BeNull("event should no longer exist after deletion");
        }

        /// <summary>
        /// SCENARIO: Try to delete a non-existent event
        /// EXPECTED: System returns not found error
        /// WHY: Cannot delete events that don't exist
        /// </summary>
        [Fact]
        public async Task DeleteEvent_WhenEventDoesNotExist_ShouldThrowNotFound()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act & Assert
            await Assert.ThrowsAsync<EventNotFoundException>(async () =>
                await _eventService.DeleteEventAsync(nonExistentId));
        }

        #endregion
    }
}
