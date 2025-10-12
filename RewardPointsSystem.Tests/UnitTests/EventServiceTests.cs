using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using RewardPointsSystem.Domain.Entities.Events;
using RewardPointsSystem.Infrastructure.Repositories;
using RewardPointsSystem.Application.Services.Events;
using RewardPointsSystem.Application.DTOs;
using Xunit;

namespace RewardPointsSystem.Tests.UnitTests
{
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

        [Fact]
        public async Task CreateEventAsync_WithValidData_ShouldCreateEvent()
        {
            // Arrange
            var name = "Annual Sales Competition";
            var description = "Yearly competition for top sales";
            var date = DateTime.UtcNow.AddDays(30);
            var pointsPool = 1000;

            // Act
            var result = await _eventService.CreateEventAsync(name, description, date, pointsPool);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(name);
            result.Description.Should().Be(description);
            result.EventDate.Should().Be(date);
            result.TotalPointsPool.Should().Be(pointsPool);
            result.Status.Should().Be(EventStatus.Upcoming);
            result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Theory]
        [InlineData(null, "Description", 10)]
        [InlineData("", "Description", 10)]
        [InlineData("   ", "Description", 10)]
        public async Task CreateEventAsync_WithInvalidName_ShouldThrowException(string name, string description, int pointsPool)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _eventService.CreateEventAsync(name, description, DateTime.UtcNow.AddDays(1), pointsPool));
        }

        [Theory]
        [InlineData("Event", null, 10)]
        [InlineData("Event", "", 10)]
        [InlineData("Event", "   ", 10)]
        public async Task CreateEventAsync_WithInvalidDescription_ShouldThrowException(string name, string description, int pointsPool)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _eventService.CreateEventAsync(name, description, DateTime.UtcNow.AddDays(1), pointsPool));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-100)]
        public async Task CreateEventAsync_WithInvalidPointsPool_ShouldThrowException(int pointsPool)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _eventService.CreateEventAsync("Event", "Description", DateTime.UtcNow.AddDays(1), pointsPool));
        }

        [Fact]
        public async Task CreateEventAsync_WithPastDate_ShouldThrowException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _eventService.CreateEventAsync("Event", "Description", DateTime.UtcNow.AddDays(-1), 100));
        }

        [Fact]
        public async Task GetEventByIdAsync_WithExistingEvent_ShouldReturnEvent()
        {
            // Arrange
            var created = await _eventService.CreateEventAsync("Test Event", "Description", DateTime.UtcNow.AddDays(5), 500);

            // Act
            var result = await _eventService.GetEventByIdAsync(created.Id);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(created.Id);
            result.Name.Should().Be("Test Event");
        }

        [Fact]
        public async Task GetEventByIdAsync_WithNonExistentEvent_ShouldReturnNull()
        {
            // Act
            var result = await _eventService.GetEventByIdAsync(Guid.NewGuid());

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetUpcomingEventsAsync_ShouldReturnOnlyUpcomingEvents()
        {
            // Arrange
            var event1 = await _eventService.CreateEventAsync("Event1", "Desc1", DateTime.UtcNow.AddDays(1), 100);
            var event2 = await _eventService.CreateEventAsync("Event2", "Desc2", DateTime.UtcNow.AddDays(2), 200);
            var event3 = await _eventService.CreateEventAsync("Event3", "Desc3", DateTime.UtcNow.AddDays(3), 300);

            await _eventService.ActivateEventAsync(event2.Id);

            // Act
            var result = await _eventService.GetUpcomingEventsAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(e => e.Id == event1.Id);
            result.Should().Contain(e => e.Id == event3.Id);
            result.Should().NotContain(e => e.Id == event2.Id);
        }

        [Fact]
        public async Task GetActiveEventsAsync_ShouldReturnOnlyActiveEvents()
        {
            // Arrange
            var event1 = await _eventService.CreateEventAsync("Event1", "Desc1", DateTime.UtcNow.AddDays(1), 100);
            var event2 = await _eventService.CreateEventAsync("Event2", "Desc2", DateTime.UtcNow.AddDays(2), 200);
            var event3 = await _eventService.CreateEventAsync("Event3", "Desc3", DateTime.UtcNow.AddDays(3), 300);

            await _eventService.ActivateEventAsync(event1.Id);
            await _eventService.ActivateEventAsync(event3.Id);

            // Act
            var result = await _eventService.GetActiveEventsAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(e => e.Id == event1.Id);
            result.Should().Contain(e => e.Id == event3.Id);
            result.Should().NotContain(e => e.Id == event2.Id);
        }

        [Fact]
        public async Task UpdateEventAsync_WithValidData_ShouldUpdateEvent()
        {
            // Arrange
            var event1 = await _eventService.CreateEventAsync("Old Name", "Old Desc", DateTime.UtcNow.AddDays(1), 100);
            var updateDto = new UpdateEventDto
            {
                Name = "New Name",
                Description = "New Desc",
                EventDate = DateTime.UtcNow.AddDays(10),
                TotalPointsPool = 500
            };

            // Act
            var result = await _eventService.UpdateEventAsync(event1.Id, updateDto);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("New Name");
            result.Description.Should().Be("New Desc");
            result.TotalPointsPool.Should().Be(500);
        }

        [Fact]
        public async Task UpdateEventAsync_WithNonExistentEvent_ShouldThrowException()
        {
            // Arrange
            var updateDto = new UpdateEventDto { Name = "New Name" };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _eventService.UpdateEventAsync(Guid.NewGuid(), updateDto));
        }

        [Fact]
        public async Task UpdateEventAsync_WithCompletedEvent_ShouldThrowException()
        {
            // Arrange
            var event1 = await _eventService.CreateEventAsync("Event", "Desc", DateTime.UtcNow.AddDays(1), 100);
            await _eventService.ActivateEventAsync(event1.Id);
            await _eventService.CompleteEventAsync(event1.Id);

            var updateDto = new UpdateEventDto { Name = "New Name" };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _eventService.UpdateEventAsync(event1.Id, updateDto));
        }

        [Fact]
        public async Task ActivateEventAsync_WithUpcomingEvent_ShouldActivateEvent()
        {
            // Arrange
            var event1 = await _eventService.CreateEventAsync("Event", "Desc", DateTime.UtcNow.AddDays(1), 100);

            // Act
            await _eventService.ActivateEventAsync(event1.Id);

            // Assert
            var activated = await _eventService.GetEventByIdAsync(event1.Id);
            activated.Status.Should().Be(EventStatus.Active);
        }

        [Fact]
        public async Task ActivateEventAsync_WithNonExistentEvent_ShouldThrowException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _eventService.ActivateEventAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task ActivateEventAsync_WithNonUpcomingEvent_ShouldThrowException()
        {
            // Arrange
            var event1 = await _eventService.CreateEventAsync("Event", "Desc", DateTime.UtcNow.AddDays(1), 100);
            await _eventService.ActivateEventAsync(event1.Id);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _eventService.ActivateEventAsync(event1.Id));
        }

        [Fact]
        public async Task CompleteEventAsync_WithActiveEvent_ShouldCompleteEvent()
        {
            // Arrange
            var event1 = await _eventService.CreateEventAsync("Event", "Desc", DateTime.UtcNow.AddDays(1), 100);
            await _eventService.ActivateEventAsync(event1.Id);

            // Act
            await _eventService.CompleteEventAsync(event1.Id);

            // Assert
            var completed = await _eventService.GetEventByIdAsync(event1.Id);
            completed.Status.Should().Be(EventStatus.Completed);
            completed.CompletedAt.Should().NotBeNull();
            completed.CompletedAt.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public async Task CompleteEventAsync_WithNonExistentEvent_ShouldThrowException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _eventService.CompleteEventAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task CompleteEventAsync_WithNonActiveEvent_ShouldThrowException()
        {
            // Arrange
            var event1 = await _eventService.CreateEventAsync("Event", "Desc", DateTime.UtcNow.AddDays(1), 100);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _eventService.CompleteEventAsync(event1.Id));
        }

        [Fact]
        public async Task CancelEventAsync_WithValidEvent_ShouldCancelEvent()
        {
            // Arrange
            var event1 = await _eventService.CreateEventAsync("Event", "Desc", DateTime.UtcNow.AddDays(1), 100);

            // Act
            await _eventService.CancelEventAsync(event1.Id);

            // Assert
            var cancelled = await _eventService.GetEventByIdAsync(event1.Id);
            cancelled.Status.Should().Be(EventStatus.Cancelled);
        }

        [Fact]
        public async Task CancelEventAsync_WithNonExistentEvent_ShouldThrowException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _eventService.CancelEventAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task CancelEventAsync_WithCompletedEvent_ShouldThrowException()
        {
            // Arrange
            var event1 = await _eventService.CreateEventAsync("Event", "Desc", DateTime.UtcNow.AddDays(1), 100);
            await _eventService.ActivateEventAsync(event1.Id);
            await _eventService.CompleteEventAsync(event1.Id);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _eventService.CancelEventAsync(event1.Id));
        }
    }
}
