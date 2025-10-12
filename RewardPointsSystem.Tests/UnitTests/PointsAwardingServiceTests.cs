using System;
using System.Threading.Tasks;
using FluentAssertions;
using RewardPointsSystem.Infrastructure.Repositories;
using RewardPointsSystem.Application.Services.Events;
using RewardPointsSystem.Application.Services.Users;
using Xunit;

namespace RewardPointsSystem.Tests.UnitTests
{
    public class PointsAwardingServiceTests : IDisposable
    {
        private readonly InMemoryUnitOfWork _unitOfWork;
        private readonly UserService _userService;
        private readonly EventService _eventService;
        private readonly EventParticipationService _participationService;
        private readonly PointsAwardingService _pointsAwardingService;

        public PointsAwardingServiceTests()
        {
            _unitOfWork = new InMemoryUnitOfWork();
            _userService = new UserService(_unitOfWork);
            _eventService = new EventService(_unitOfWork);
            _participationService = new EventParticipationService(_unitOfWork);
            _pointsAwardingService = new PointsAwardingService(_unitOfWork);
        }

        public void Dispose()
        {
            _unitOfWork?.Dispose();
        }

        [Fact]
        public async Task AwardPointsAsync_WithValidData_ShouldAwardPoints()
        {
            // Arrange
            var user = await _userService.CreateUserAsync("user@test.com", "John", "Doe");
            var eventObj = await _eventService.CreateEventAsync("Event", "Description", DateTime.UtcNow.AddDays(1), 1000);
            await _participationService.RegisterParticipantAsync(eventObj.Id, user.Id);
            await _eventService.ActivateEventAsync(eventObj.Id);
            await _eventService.CompleteEventAsync(eventObj.Id);

            // Act
            await _pointsAwardingService.AwardPointsAsync(eventObj.Id, user.Id, 500, 1);

            // Assert - verify no exception thrown
            var remaining = await _pointsAwardingService.GetRemainingPointsPoolAsync(eventObj.Id);
            remaining.Should().Be(500);
        }

        [Fact]
        public async Task AwardPointsAsync_WithNonExistentEvent_ShouldThrowException()
        {
            // Arrange
            var user = await _userService.CreateUserAsync("user@test.com", "John", "Doe");

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _pointsAwardingService.AwardPointsAsync(Guid.NewGuid(), user.Id, 100, 1));
        }

        [Fact]
        public async Task AwardPointsAsync_WithNonExistentUser_ShouldThrowException()
        {
            // Arrange
            var eventObj = await _eventService.CreateEventAsync("Event", "Description", DateTime.UtcNow.AddDays(1), 1000);
            await _eventService.ActivateEventAsync(eventObj.Id);
            await _eventService.CompleteEventAsync(eventObj.Id);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _pointsAwardingService.AwardPointsAsync(eventObj.Id, Guid.NewGuid(), 100, 1));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-100)]
        public async Task AwardPointsAsync_WithInvalidPoints_ShouldThrowException(int points)
        {
            // Arrange
            var user = await _userService.CreateUserAsync("user@test.com", "John", "Doe");
            var eventObj = await _eventService.CreateEventAsync("Event", "Description", DateTime.UtcNow.AddDays(1), 1000);
            await _eventService.ActivateEventAsync(eventObj.Id);
            await _eventService.CompleteEventAsync(eventObj.Id);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _pointsAwardingService.AwardPointsAsync(eventObj.Id, user.Id, points, 1));
        }

        [Fact]
        public async Task AwardPointsAsync_ExceedingPool_ShouldThrowException()
        {
            // Arrange
            var user = await _userService.CreateUserAsync("user@test.com", "John", "Doe");
            var eventObj = await _eventService.CreateEventAsync("Event", "Description", DateTime.UtcNow.AddDays(1), 500);
            await _eventService.ActivateEventAsync(eventObj.Id);
            await _eventService.CompleteEventAsync(eventObj.Id);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _pointsAwardingService.AwardPointsAsync(eventObj.Id, user.Id, 600, 1));
        }

        [Fact]
        public async Task AwardPointsAsync_WithNonCompletedEvent_ShouldThrowException()
        {
            // Arrange
            var user = await _userService.CreateUserAsync("user@test.com", "John", "Doe");
            var eventObj = await _eventService.CreateEventAsync("Event", "Description", DateTime.UtcNow.AddDays(1), 1000);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _pointsAwardingService.AwardPointsAsync(eventObj.Id, user.Id, 100, 1));
        }

        [Fact]
        public async Task AwardPointsAsync_DuplicateAward_ShouldThrowException()
        {
            // Arrange
            var user = await _userService.CreateUserAsync("user@test.com", "John", "Doe");
            var eventObj = await _eventService.CreateEventAsync("Event", "Description", DateTime.UtcNow.AddDays(1), 1000);
            await _participationService.RegisterParticipantAsync(eventObj.Id, user.Id);
            await _eventService.ActivateEventAsync(eventObj.Id);
            await _eventService.CompleteEventAsync(eventObj.Id);
            await _pointsAwardingService.AwardPointsAsync(eventObj.Id, user.Id, 300, 1);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _pointsAwardingService.AwardPointsAsync(eventObj.Id, user.Id, 200, 1));
        }

        [Fact]
        public async Task GetRemainingPointsPoolAsync_WithAwardedPoints_ShouldReturnCorrectRemaining()
        {
            // Arrange
            var user1 = await _userService.CreateUserAsync("user1@test.com", "John", "Doe");
            var user2 = await _userService.CreateUserAsync("user2@test.com", "Jane", "Smith");
            var eventObj = await _eventService.CreateEventAsync("Event", "Description", DateTime.UtcNow.AddDays(1), 1000);
            await _participationService.RegisterParticipantAsync(eventObj.Id, user1.Id);
            await _participationService.RegisterParticipantAsync(eventObj.Id, user2.Id);
            await _eventService.ActivateEventAsync(eventObj.Id);
            await _eventService.CompleteEventAsync(eventObj.Id);

            await _pointsAwardingService.AwardPointsAsync(eventObj.Id, user1.Id, 300, 1);
            await _pointsAwardingService.AwardPointsAsync(eventObj.Id, user2.Id, 200, 2);

            // Act
            var remaining = await _pointsAwardingService.GetRemainingPointsPoolAsync(eventObj.Id);

            // Assert
            remaining.Should().Be(500);
        }

        [Fact]
        public async Task GetRemainingPointsPoolAsync_WithNoAwards_ShouldReturnFullPool()
        {
            // Arrange
            var eventObj = await _eventService.CreateEventAsync("Event", "Description", DateTime.UtcNow.AddDays(1), 750);

            // Act
            var remaining = await _pointsAwardingService.GetRemainingPointsPoolAsync(eventObj.Id);

            // Assert
            remaining.Should().Be(750);
        }

        [Fact]
        public async Task GetRemainingPointsPoolAsync_WithNonExistentEvent_ShouldThrowException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _pointsAwardingService.GetRemainingPointsPoolAsync(Guid.NewGuid()));
        }

    }
}
