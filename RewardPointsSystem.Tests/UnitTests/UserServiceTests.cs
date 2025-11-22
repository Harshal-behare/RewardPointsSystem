using System;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using RewardPointsSystem.Application.Services.Core;
using RewardPointsSystem.Infrastructure.Repositories;
using RewardPointsSystem.Application.Interfaces;

namespace RewardPointsSystem.Tests.UnitTests
{
    /// <summary>
    /// Unit tests for UserService
    /// Tests user creation, validation, updates, and retrieval
    /// </summary>
    public class UserServiceTests : IDisposable
    {
        private readonly InMemoryUnitOfWork _unitOfWork;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _unitOfWork = new InMemoryUnitOfWork();
            _userService = new UserService(_unitOfWork);
        }

        public void Dispose()
        {
            _unitOfWork?.Dispose();
        }

        #region Create User Tests

        [Fact]
        public async Task CreateUserAsync_WithValidData_ShouldSucceed()
        {
            // Arrange
            var email = "test@example.com";
            var firstName = "John";
            var lastName = "Doe";

            // Act
            var result = await _userService.CreateUserAsync(email, firstName, lastName);

            // Assert
            result.Should().NotBeNull();
            result.Email.Should().Be(email);
            result.FirstName.Should().Be(firstName);
            result.LastName.Should().Be(lastName);
            result.IsActive.Should().BeTrue();
            result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public async Task CreateUserAsync_WithDuplicateEmail_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var email = "duplicate@example.com";
            await _userService.CreateUserAsync(email, "First", "User");

            // Act
            Func<Task> act = async () => await _userService.CreateUserAsync(email, "Second", "User");

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage($"*{email}*already exists*");
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task CreateUserAsync_WithEmptyOrWhitespaceFirstName_ShouldThrowArgumentException(string firstName)
        {
            // Arrange
            var email = "test@example.com";
            var lastName = "Doe";

            // Act
            Func<Task> act = async () => await _userService.CreateUserAsync(email, firstName, lastName);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*First name*required*");
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task CreateUserAsync_WithEmptyOrWhitespaceLastName_ShouldThrowArgumentException(string lastName)
        {
            // Arrange
            var email = "test@example.com";
            var firstName = "John";

            // Act
            Func<Task> act = async () => await _userService.CreateUserAsync(email, firstName, lastName);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*Last name*required*");
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task CreateUserAsync_WithEmptyOrWhitespaceEmail_ShouldThrowArgumentException(string email)
        {
            // Arrange
            var firstName = "John";
            var lastName = "Doe";

            // Act
            Func<Task> act = async () => await _userService.CreateUserAsync(email, firstName, lastName);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*Email*required*");
        }

        #endregion

        #region Update User Tests

        [Fact]
        public async Task UpdateUserAsync_WithValidData_ShouldUpdateSuccessfully()
        {
            // Arrange
            var user = await _userService.CreateUserAsync("original@example.com", "Original", "Name");
            var updateDto = new UserUpdateDto
            {
                Email = "updated@example.com",
                FirstName = "Updated",
                LastName = "User"
            };

            // Act
            var result = await _userService.UpdateUserAsync(user.Id, updateDto);

            // Assert
            result.Should().NotBeNull();
            result.Email.Should().Be(updateDto.Email);
            result.FirstName.Should().Be(updateDto.FirstName);
            result.LastName.Should().Be(updateDto.LastName);
            result.UpdatedAt.Should().NotBeNull();
            result.UpdatedAt.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public async Task UpdateUserAsync_WithDuplicateEmail_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var user1 = await _userService.CreateUserAsync("user1@example.com", "User", "One");
            var user2 = await _userService.CreateUserAsync("user2@example.com", "User", "Two");
            
            var updateDto = new UserUpdateDto
            {
                Email = "user1@example.com" // Try to use user1's email
            };

            // Act
            Func<Task> act = async () => await _userService.UpdateUserAsync(user2.Id, updateDto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*user1@example.com*already exists*");
        }

        [Fact]
        public async Task UpdateUserAsync_WithNonExistentUserId_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            var updateDto = new UserUpdateDto { FirstName = "Test" };

            // Act
            Func<Task> act = async () => await _userService.UpdateUserAsync(nonExistentId, updateDto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage($"*{nonExistentId}*not found*");
        }

        #endregion

        #region Retrieve User Tests

        [Fact]
        public async Task GetUserByIdAsync_WithValidId_ShouldReturnCorrectUser()
        {
            // Arrange
            var createdUser = await _userService.CreateUserAsync("test@example.com", "John", "Doe");

            // Act
            var result = await _userService.GetUserByIdAsync(createdUser.Id);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(createdUser.Id);
            result.Email.Should().Be(createdUser.Email);
            result.FirstName.Should().Be(createdUser.FirstName);
            result.LastName.Should().Be(createdUser.LastName);
        }

        [Fact]
        public async Task GetUserByIdAsync_WithNonExistentId_ShouldReturnNull()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _userService.GetUserByIdAsync(nonExistentId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetUserByEmailAsync_WithValidEmail_ShouldReturnCorrectUser()
        {
            // Arrange
            var email = "find@example.com";
            var createdUser = await _userService.CreateUserAsync(email, "Find", "Me");

            // Act
            var result = await _userService.GetUserByEmailAsync(email);

            // Assert
            result.Should().NotBeNull();
            result.Email.Should().Be(email);
            result.Id.Should().Be(createdUser.Id);
        }

        [Fact]
        public async Task GetUserByEmailAsync_WithNonExistentEmail_ShouldReturnNull()
        {
            // Arrange
            var nonExistentEmail = "doesnotexist@example.com";

            // Act
            var result = await _userService.GetUserByEmailAsync(nonExistentEmail);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetActiveUsersAsync_ShouldReturnOnlyActiveUsers()
        {
            // Arrange
            var user1 = await _userService.CreateUserAsync("active1@example.com", "Active", "One");
            var user2 = await _userService.CreateUserAsync("active2@example.com", "Active", "Two");
            var user3 = await _userService.CreateUserAsync("todeactivate@example.com", "To", "Deactivate");
            await _userService.DeactivateUserAsync(user3.Id);

            // Act
            var result = await _userService.GetActiveUsersAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().Contain(u => u.Id == user1.Id);
            result.Should().Contain(u => u.Id == user2.Id);
            result.Should().NotContain(u => u.Id == user3.Id);
        }

        #endregion

        #region Deactivate User Tests

        [Fact]
        public async Task DeactivateUserAsync_WithValidId_ShouldSetIsActiveToFalse()
        {
            // Arrange
            var user = await _userService.CreateUserAsync("deactivate@example.com", "Deactivate", "Me");
            user.IsActive.Should().BeTrue();

            // Act
            await _userService.DeactivateUserAsync(user.Id);

            // Assert
            var deactivatedUser = await _userService.GetUserByIdAsync(user.Id);
            deactivatedUser.IsActive.Should().BeFalse();
            deactivatedUser.UpdatedAt.Should().NotBeNull();
        }

        [Fact]
        public async Task DeactivateUserAsync_WithNonExistentId_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            Func<Task> act = async () => await _userService.DeactivateUserAsync(nonExistentId);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage($"*{nonExistentId}*not found*");
        }

        #endregion
    }
}
