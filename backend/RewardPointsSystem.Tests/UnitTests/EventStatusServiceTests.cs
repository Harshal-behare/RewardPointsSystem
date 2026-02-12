using System;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Moq;
using RewardPointsSystem.Application.Services.Events;
using RewardPointsSystem.Application.Interfaces;
using RewardPointsSystem.Application.DTOs.Events;
using RewardPointsSystem.Domain.Entities.Events;

namespace RewardPointsSystem.Tests.UnitTests
{
    /// <summary>
    /// Unit tests for EventStatusService
    /// Tests event status transitions and validation
    /// Following: Isolation, AAA pattern, determinism, clear naming, mocking
    /// Coverage: 7 test cases
    /// </summary>
    public class EventStatusServiceTests
    {
        private readonly Mock<IEventService> _mockEventService;
        private readonly Mock<IEventQueryService> _mockEventQueryService;
        private readonly EventStatusService _eventStatusService;

        public EventStatusServiceTests()
        {
            _mockEventService = new Mock<IEventService>();
            _mockEventQueryService = new Mock<IEventQueryService>();
            _eventStatusService = new EventStatusService(
                _mockEventService.Object,
                _mockEventQueryService.Object);
        }

        #region Helper Methods

        private static Event CreateTestEvent(EventStatus status = EventStatus.Draft)
        {
            var eventEntity = Event.Create(
                name: "Test Event",
                eventDate: DateTime.UtcNow.AddDays(7),
                totalPointsPool: 1000,
                createdBy: Guid.NewGuid(),
                description: "Test Description");

            // Use reflection to set status since it's typically controlled by domain methods
            var statusProperty = typeof(Event).GetProperty("Status");
            if (statusProperty != null && statusProperty.CanWrite)
            {
                statusProperty.SetValue(eventEntity, status);
            }

            return eventEntity;
        }

        private static EventResponseDto CreateTestEventResponse(Guid eventId, string status)
        {
            return new EventResponseDto
            {
                Id = eventId,
                Name = "Test Event",
                Description = "Test Description",
                Status = status
            };
        }

        #endregion

        #region ChangeStatusAsync - Valid Transitions

        [Fact]
        public async Task ChangeStatusAsync_DraftToUpcoming_ReturnsSuccess()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var draftEvent = CreateTestEvent(EventStatus.Draft);
            var responseDto = CreateTestEventResponse(eventId, "Upcoming");

            _mockEventService.Setup(x => x.GetEventByIdAsync(eventId))
                .ReturnsAsync(draftEvent);
            _mockEventService.Setup(x => x.PublishEventAsync(eventId))
                .Returns(Task.CompletedTask);
            _mockEventQueryService.Setup(x => x.GetEventByIdAsync(eventId))
                .ReturnsAsync(responseDto);

            // Act
            var result = await _eventStatusService.ChangeStatusAsync(eventId, "upcoming");

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Status.Should().Be("Upcoming");
            _mockEventService.Verify(x => x.PublishEventAsync(eventId), Times.Once);
        }

        [Fact]
        public async Task ChangeStatusAsync_UpcomingToActive_ReturnsSuccess()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var upcomingEvent = CreateTestEvent(EventStatus.Upcoming);
            var responseDto = CreateTestEventResponse(eventId, "Active");

            _mockEventService.Setup(x => x.GetEventByIdAsync(eventId))
                .ReturnsAsync(upcomingEvent);
            _mockEventService.Setup(x => x.ActivateEventAsync(eventId))
                .Returns(Task.CompletedTask);
            _mockEventQueryService.Setup(x => x.GetEventByIdAsync(eventId))
                .ReturnsAsync(responseDto);

            // Act
            var result = await _eventStatusService.ChangeStatusAsync(eventId, "active");

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Status.Should().Be("Active");
            _mockEventService.Verify(x => x.ActivateEventAsync(eventId), Times.Once);
        }

        [Fact]
        public async Task ChangeStatusAsync_ActiveToCompleted_ReturnsSuccess()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var activeEvent = CreateTestEvent(EventStatus.Active);
            var responseDto = CreateTestEventResponse(eventId, "Completed");

            _mockEventService.Setup(x => x.GetEventByIdAsync(eventId))
                .ReturnsAsync(activeEvent);
            _mockEventService.Setup(x => x.CompleteEventAsync(eventId))
                .Returns(Task.CompletedTask);
            _mockEventQueryService.Setup(x => x.GetEventByIdAsync(eventId))
                .ReturnsAsync(responseDto);

            // Act
            var result = await _eventStatusService.ChangeStatusAsync(eventId, "completed");

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Status.Should().Be("Completed");
            _mockEventService.Verify(x => x.CompleteEventAsync(eventId), Times.Once);
        }

        #endregion

        #region ChangeStatusAsync - Invalid Transitions

        [Fact]
        public async Task ChangeStatusAsync_InvalidStatusTarget_ReturnsValidationError()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var draftEvent = CreateTestEvent(EventStatus.Draft);

            _mockEventService.Setup(x => x.GetEventByIdAsync(eventId))
                .ReturnsAsync(draftEvent);

            // Act
            var result = await _eventStatusService.ChangeStatusAsync(eventId, "invalid_status");

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorType.Should().Be(EventStatusErrorType.ValidationError);
            result.ErrorMessage.Should().Contain("Invalid status");
        }

        [Fact]
        public async Task ChangeStatusAsync_CompletedToDraft_ReturnsInvalidTransition()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var completedEvent = CreateTestEvent(EventStatus.Completed);

            _mockEventService.Setup(x => x.GetEventByIdAsync(eventId))
                .ReturnsAsync(completedEvent);

            // Act
            var result = await _eventStatusService.ChangeStatusAsync(eventId, "draft");

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorType.Should().Be(EventStatusErrorType.InvalidTransition);
            result.ErrorMessage.Should().Contain("Cannot change to Draft");
        }

        [Fact]
        public async Task ChangeStatusAsync_ServiceThrowsException_ReturnsInvalidTransition()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var draftEvent = CreateTestEvent(EventStatus.Draft);

            _mockEventService.Setup(x => x.GetEventByIdAsync(eventId))
                .ReturnsAsync(draftEvent);
            _mockEventService.Setup(x => x.PublishEventAsync(eventId))
                .ThrowsAsync(new InvalidOperationException("Cannot publish: missing required fields"));

            // Act
            var result = await _eventStatusService.ChangeStatusAsync(eventId, "upcoming");

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorType.Should().Be(EventStatusErrorType.InvalidTransition);
            result.ErrorMessage.Should().Contain("Cannot publish");
        }

        #endregion

        #region ChangeStatusAsync - Not Found

        [Fact]
        public async Task ChangeStatusAsync_NonExistentEvent_ReturnsNotFound()
        {
            // Arrange
            var eventId = Guid.NewGuid();

            _mockEventService.Setup(x => x.GetEventByIdAsync(eventId))
                .ReturnsAsync((Event?)null);

            // Act
            var result = await _eventStatusService.ChangeStatusAsync(eventId, "upcoming");

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorType.Should().Be(EventStatusErrorType.NotFound);
            result.ErrorMessage.Should().Contain("not found");
        }

        #endregion

        #region ChangeStatusAsync - Edge Cases

        [Fact]
        public async Task ChangeStatusAsync_PublishAlias_ReturnsSuccess()
        {
            // Arrange - "published" is alias for "upcoming"
            var eventId = Guid.NewGuid();
            var draftEvent = CreateTestEvent(EventStatus.Draft);
            var responseDto = CreateTestEventResponse(eventId, "Upcoming");

            _mockEventService.Setup(x => x.GetEventByIdAsync(eventId))
                .ReturnsAsync(draftEvent);
            _mockEventService.Setup(x => x.PublishEventAsync(eventId))
                .Returns(Task.CompletedTask);
            _mockEventQueryService.Setup(x => x.GetEventByIdAsync(eventId))
                .ReturnsAsync(responseDto);

            // Act
            var result = await _eventStatusService.ChangeStatusAsync(eventId, "published");

            // Assert
            result.Success.Should().BeTrue();
            _mockEventService.Verify(x => x.PublishEventAsync(eventId), Times.Once);
        }

        [Fact]
        public async Task ChangeStatusAsync_CaseInsensitiveStatus_ReturnsSuccess()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var upcomingEvent = CreateTestEvent(EventStatus.Upcoming);
            var responseDto = CreateTestEventResponse(eventId, "Active");

            _mockEventService.Setup(x => x.GetEventByIdAsync(eventId))
                .ReturnsAsync(upcomingEvent);
            _mockEventService.Setup(x => x.ActivateEventAsync(eventId))
                .Returns(Task.CompletedTask);
            _mockEventQueryService.Setup(x => x.GetEventByIdAsync(eventId))
                .ReturnsAsync(responseDto);

            // Act
            var result = await _eventStatusService.ChangeStatusAsync(eventId, "ACTIVE");

            // Assert
            result.Success.Should().BeTrue();
            _mockEventService.Verify(x => x.ActivateEventAsync(eventId), Times.Once);
        }

        [Fact]
        public async Task ChangeStatusAsync_NullStatus_ReturnsValidationError()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var draftEvent = CreateTestEvent(EventStatus.Draft);

            _mockEventService.Setup(x => x.GetEventByIdAsync(eventId))
                .ReturnsAsync(draftEvent);

            // Act
            var result = await _eventStatusService.ChangeStatusAsync(eventId, null!);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorType.Should().Be(EventStatusErrorType.ValidationError);
        }

        #endregion
    }
}
