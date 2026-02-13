using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using RewardPointsSystem.Application.Services.Core;
using RewardPointsSystem.Application.Interfaces;
using RewardPointsSystem.Domain.Entities.Core;
using RewardPointsSystem.Tests.TestHelpers;

namespace RewardPointsSystem.Tests.UnitTests
{
    /// <summary>
    /// Unit tests for UserRoleService
    /// Tests user role assignment and retrieval operations
    /// Following: Isolation, AAA pattern, determinism, no shared state
    /// Coverage: 12 test cases
    /// </summary>
    public class UserRoleServiceTests : IDisposable
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserRoleService _userRoleService;

        public UserRoleServiceTests()
        {
            _unitOfWork = TestDbContextFactory.CreateCleanSqlServerUnitOfWork();
            _userRoleService = new UserRoleService(_unitOfWork);
        }

        public void Dispose()
        {
            _unitOfWork?.Dispose();
        }

        #region Helper Methods

        private async Task<User> CreateTestUserAsync(string email = "test@example.com")
        {
            var user = User.Create(email, "Test", "User");
            // User.Create already sets IsActive = true, no need to call Activate()
            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();
            return user;
        }

        private async Task<Role> CreateTestRoleAsync(string name = "TestRole")
        {
            var role = Role.Create(name, $"Description for {name}");
            await _unitOfWork.Roles.AddAsync(role);
            await _unitOfWork.SaveChangesAsync();
            return role;
        }

        #endregion

        #region AssignRoleAsync Tests

        [Fact]
        public async Task AssignRoleAsync_ValidData_AssignsRoleToUser()
        {
            // Arrange
            var user = await CreateTestUserAsync();
            var role = await CreateTestRoleAsync("Employee");
            var assignedBy = Guid.NewGuid();

            // Act
            await _userRoleService.AssignRoleAsync(user.Id, role.Id, assignedBy);

            // Assert
            var userRoles = await _userRoleService.GetUserRolesAsync(user.Id);
            userRoles.Should().HaveCount(1);
            userRoles.First().Name.Should().Be("Employee");
        }

        [Fact]
        public async Task AssignRoleAsync_DuplicateAssignment_ThrowsInvalidOperationException()
        {
            // Arrange
            var user = await CreateTestUserAsync();
            var role = await CreateTestRoleAsync("Admin");
            var assignedBy = Guid.NewGuid();
            await _userRoleService.AssignRoleAsync(user.Id, role.Id, assignedBy);

            // Act
            Func<Task> act = async () => await _userRoleService.AssignRoleAsync(user.Id, role.Id, assignedBy);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*already assigned*");
        }

        [Fact]
        public async Task AssignRoleAsync_NonExistentUser_ThrowsInvalidOperationException()
        {
            // Arrange
            var role = await CreateTestRoleAsync();
            var nonExistentUserId = Guid.NewGuid();
            var assignedBy = Guid.NewGuid();

            // Act
            Func<Task> act = async () => await _userRoleService.AssignRoleAsync(nonExistentUserId, role.Id, assignedBy);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*User*not found*");
        }

        [Fact]
        public async Task AssignRoleAsync_NonExistentRole_ThrowsInvalidOperationException()
        {
            // Arrange
            var user = await CreateTestUserAsync();
            var nonExistentRoleId = Guid.NewGuid();
            var assignedBy = Guid.NewGuid();

            // Act
            Func<Task> act = async () => await _userRoleService.AssignRoleAsync(user.Id, nonExistentRoleId, assignedBy);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Role*not found*");
        }

        #endregion

        #region RemoveRoleAsync Tests

        [Fact]
        public async Task RemoveRoleAsync_ValidData_RemovesRoleFromUser()
        {
            // Arrange
            var user = await CreateTestUserAsync();
            var role = await CreateTestRoleAsync("Employee");
            await _userRoleService.AssignRoleAsync(user.Id, role.Id, Guid.NewGuid());

            // Act
            await _userRoleService.RemoveRoleAsync(user.Id, role.Id);

            // Assert
            var userRoles = await _userRoleService.GetUserRolesAsync(user.Id);
            userRoles.Should().BeEmpty();
        }

        [Fact]
        public async Task RemoveRoleAsync_NotAssigned_ThrowsInvalidOperationException()
        {
            // Arrange
            var user = await CreateTestUserAsync();
            var role = await CreateTestRoleAsync();

            // Act
            Func<Task> act = async () => await _userRoleService.RemoveRoleAsync(user.Id, role.Id);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*not assigned*");
        }

        #endregion

        #region GetUserRolesAsync Tests

        [Fact]
        public async Task GetUserRolesAsync_UserWithRoles_ReturnsRoles()
        {
            // Arrange
            var user = await CreateTestUserAsync();
            var adminRole = await CreateTestRoleAsync("Admin");
            var employeeRole = await CreateTestRoleAsync("Employee");
            await _userRoleService.AssignRoleAsync(user.Id, adminRole.Id, Guid.NewGuid());
            await _userRoleService.AssignRoleAsync(user.Id, employeeRole.Id, Guid.NewGuid());

            // Act
            var roles = await _userRoleService.GetUserRolesAsync(user.Id);

            // Assert
            roles.Should().HaveCount(2);
            roles.Select(r => r.Name).Should().Contain("Admin");
            roles.Select(r => r.Name).Should().Contain("Employee");
        }

        [Fact]
        public async Task GetUserRolesAsync_UserWithNoRoles_ReturnsEmpty()
        {
            // Arrange
            var user = await CreateTestUserAsync();

            // Act
            var roles = await _userRoleService.GetUserRolesAsync(user.Id);

            // Assert
            roles.Should().BeEmpty();
        }

        #endregion

        #region IsUserInRoleAsync Tests

        [Fact]
        public async Task IsUserInRoleAsync_UserHasRole_ReturnsTrue()
        {
            // Arrange
            var user = await CreateTestUserAsync();
            var role = await CreateTestRoleAsync("Admin");
            await _userRoleService.AssignRoleAsync(user.Id, role.Id, Guid.NewGuid());

            // Act
            var result = await _userRoleService.IsUserInRoleAsync(user.Id, "Admin");

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsUserInRoleAsync_UserDoesNotHaveRole_ReturnsFalse()
        {
            // Arrange
            var user = await CreateTestUserAsync();
            await CreateTestRoleAsync("Admin");

            // Act
            var result = await _userRoleService.IsUserInRoleAsync(user.Id, "Admin");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsUserInRoleAsync_NonExistentRole_ReturnsFalse()
        {
            // Arrange
            var user = await CreateTestUserAsync();

            // Act
            var result = await _userRoleService.IsUserInRoleAsync(user.Id, "NonExistent");

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region GetUsersInRoleAsync Tests

        [Fact]
        public async Task GetUsersInRoleAsync_RoleWithUsers_ReturnsUsers()
        {
            // Arrange
            var role = await CreateTestRoleAsync("Admin");
            var user1 = await CreateTestUserAsync("admin1@example.com");
            var user2 = await CreateTestUserAsync("admin2@example.com");
            await _userRoleService.AssignRoleAsync(user1.Id, role.Id, Guid.NewGuid());
            await _userRoleService.AssignRoleAsync(user2.Id, role.Id, Guid.NewGuid());

            // Act
            var users = await _userRoleService.GetUsersInRoleAsync("Admin");

            // Assert
            users.Should().HaveCount(2);
            users.Select(u => u.Email).Should().Contain("admin1@example.com");
            users.Select(u => u.Email).Should().Contain("admin2@example.com");
        }

        [Fact]
        public async Task GetUsersInRoleAsync_RoleWithNoUsers_ReturnsEmpty()
        {
            // Arrange
            await CreateTestRoleAsync("EmptyRole");

            // Act
            var users = await _userRoleService.GetUsersInRoleAsync("EmptyRole");

            // Assert
            users.Should().BeEmpty();
        }

        #endregion
    }
}
