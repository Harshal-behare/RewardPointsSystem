using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using RewardPointsSystem.Infrastructure.Repositories;
using RewardPointsSystem.Application.Services.Events;
using RewardPointsSystem.Application.Services.Core;
using Xunit;

namespace RewardPointsSystem.Tests.UnitTests
{
    public class EventParticipationServiceTests : IDisposable
    {
        private readonly InMemoryUnitOfWork _unitOfWork;
        private readonly EventService _eventService;
        private readonly UserService _userService;
        private readonly EventParticipationService _participationService;

        public EventParticipationServiceTests()
        {
            _unitOfWork = new InMemoryUnitOfWork();
            _eventService = new EventService(_unitOfWork);
            _userService = new UserService(_unitOfWork);
            _participationService = new EventParticipationService(_unitOfWork);
        }

        public void Dispose()
        {
            _unitOfWork?.Dispose();
        }

        [Fact]
        public async Task RegisterParticipantAsync_WithValidData_ShouldRegisterParticipant()
        {
            // Arrange
            var user = await _userService.CreateUserAsync("user@test.com", "John", "Doe");
            var eventObj = await _eventService.CreateEventAsync("Event", "Description", DateTime.UtcNow.AddDays(1), 100);

            // Act
            await _participationService.RegisterParticipantAsync(eventObj.Id, user.Id);

            // Assert
            var isRegistered = await _participationService.IsUserRegisteredAsync(eventObj.Id, user.Id);
            isRegistered.Should().BeTrue();
        }

        [Fact]
        public async Task RegisterParticipantAsync_WithNonExistentEvent_ShouldThrowException()
        {
            // Arrange
            var user = await _userService.CreateUserAsync("user@test.com", "John", "Doe");

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _participationService.RegisterParticipantAsync(Guid.NewGuid(), user.Id));
        }

        [Fact]
        public async Task RegisterParticipantAsync_WithNonExistentUser_ShouldThrowException()
        {
            // Arrange
            var eventObj = await _eventService.CreateEventAsync("Event", "Description", DateTime.UtcNow.AddDays(1), 100);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _participationService.RegisterParticipantAsync(eventObj.Id, Guid.NewGuid()));
        }

        [Fact]
        public async Task RegisterParticipantAsync_WithInactiveUser_ShouldThrowException()
        {
            // Arrange
            var user = await _userService.CreateUserAsync("user@test.com", "John", "Doe");
            var eventObj = await _eventService.CreateEventAsync("Event", "Description", DateTime.UtcNow.AddDays(1), 100);
            await _userService.DeactivateUserAsync(user.Id);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _participationService.RegisterParticipantAsync(eventObj.Id, user.Id));
        }

        [Fact]
        public async Task RegisterParticipantAsync_WithCancelledEvent_ShouldThrowException()
        {
            // Arrange
            var user = await _userService.CreateUserAsync("user@test.com", "John", "Doe");
            var eventObj = await _eventService.CreateEventAsync("Event", "Description", DateTime.UtcNow.AddDays(1), 100);
            await _eventService.CancelEventAsync(eventObj.Id);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _participationService.RegisterParticipantAsync(eventObj.Id, user.Id));
        }

        [Fact]
        public async Task RegisterParticipantAsync_WithDuplicateRegistration_ShouldThrowException()
        {
            // Arrange
            var user = await _userService.CreateUserAsync("user@test.com", "John", "Doe");
            var eventObj = await _eventService.CreateEventAsync("Event", "Description", DateTime.UtcNow.AddDays(1), 100);
            await _participationService.RegisterParticipantAsync(eventObj.Id, user.Id);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _participationService.RegisterParticipantAsync(eventObj.Id, user.Id));
        }


        [Fact]
        public async Task IsUserRegisteredAsync_WithRegisteredUser_ShouldReturnTrue()
        {
            // Arrange
            var user = await _userService.CreateUserAsync("user@test.com", "John", "Doe");
            var eventObj = await _eventService.CreateEventAsync("Event", "Description", DateTime.UtcNow.AddDays(1), 100);
            await _participationService.RegisterParticipantAsync(eventObj.Id, user.Id);

            // Act
            var result = await _participationService.IsUserRegisteredAsync(eventObj.Id, user.Id);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsUserRegisteredAsync_WithNonRegisteredUser_ShouldReturnFalse()
        {
            // Arrange
            var user = await _userService.CreateUserAsync("user@test.com", "John", "Doe");
            var eventObj = await _eventService.CreateEventAsync("Event", "Description", DateTime.UtcNow.AddDays(1), 100);

            // Act
            var result = await _participationService.IsUserRegisteredAsync(eventObj.Id, user.Id);

            // Assert
            result.Should().BeFalse();
        }

    }
}
