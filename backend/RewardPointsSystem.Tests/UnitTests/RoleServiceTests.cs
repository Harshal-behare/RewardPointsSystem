using System;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using RewardPointsSystem.Application.Services.Core;
using RewardPointsSystem.Application.Interfaces;
using RewardPointsSystem.Tests.TestHelpers;

namespace RewardPointsSystem.Tests.UnitTests
{
    /// <summary>
    /// Unit tests for RoleService
    /// Tests role creation, retrieval, update, and deletion
    /// Following: Isolation, AAA pattern, determinism, clear naming
    /// Coverage: 10 test cases
    /// </summary>
    public class RoleServiceTests : IDisposable
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly RoleService _roleService;

        public RoleServiceTests()
        {
            _unitOfWork = TestDbContextFactory.CreateCleanSqlServerUnitOfWork();
            _roleService = new RoleService(_unitOfWork);
        }

        public void Dispose()
        {
            _unitOfWork?.Dispose();
        }

        #region CreateRoleAsync Tests

        [Fact]
        public async Task CreateRoleAsync_WithValidData_CreatesRole()
        {
            // Arrange
            var name = "Manager";
            var description = "Manages employee teams";

            // Act
            var result = await _roleService.CreateRoleAsync(name, description);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(name);
            result.Description.Should().Be(description);
            result.IsActive.Should().BeTrue();
            result.Id.Should().NotBeEmpty();
        }

        [Fact]
        public async Task CreateRoleAsync_WithDuplicateName_ThrowsInvalidOperationException()
        {
            // Arrange
            var name = "Supervisor";
            var description = "Supervises team";
            await _roleService.CreateRoleAsync(name, description);

            // Act
            Func<Task> act = async () => await _roleService.CreateRoleAsync(name, "Different description");

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage($"*{name}*already exists*");
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task CreateRoleAsync_WithEmptyName_ThrowsArgumentException(string name)
        {
            // Arrange
            var description = "Some description";

            // Act
            Func<Task> act = async () => await _roleService.CreateRoleAsync(name, description);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*name*required*");
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task CreateRoleAsync_WithEmptyDescription_ThrowsArgumentException(string description)
        {
            // Arrange
            var name = "TestRole";

            // Act
            Func<Task> act = async () => await _roleService.CreateRoleAsync(name, description);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*description*required*");
        }

        #endregion

        #region GetAllRolesAsync Tests

        [Fact]
        public async Task GetAllRolesAsync_WithRoles_ReturnsAllRoles()
        {
            // Arrange
            await _roleService.CreateRoleAsync("Admin", "Administrator role");
            await _roleService.CreateRoleAsync("Employee", "Employee role");
            await _roleService.CreateRoleAsync("Manager", "Manager role");

            // Act
            var result = await _roleService.GetAllRolesAsync();

            // Assert
            result.Should().HaveCount(3);
        }

        [Fact]
        public async Task GetAllRolesAsync_WithNoRoles_ReturnsEmptyCollection()
        {
            // Act
            var result = await _roleService.GetAllRolesAsync();

            // Assert
            result.Should().BeEmpty();
        }

        #endregion

        #region GetRoleByIdAsync Tests

        [Fact]
        public async Task GetRoleByIdAsync_WithExistingId_ReturnsRole()
        {
            // Arrange
            var createdRole = await _roleService.CreateRoleAsync("Developer", "Software developer");

            // Act
            var result = await _roleService.GetRoleByIdAsync(createdRole.Id);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(createdRole.Id);
            result.Name.Should().Be("Developer");
        }

        [Fact]
        public async Task GetRoleByIdAsync_WithNonExistentId_ReturnsNull()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _roleService.GetRoleByIdAsync(nonExistentId);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetRoleByNameAsync Tests

        [Fact]
        public async Task GetRoleByNameAsync_WithExistingName_ReturnsRole()
        {
            // Arrange
            await _roleService.CreateRoleAsync("Tester", "Quality assurance tester");

            // Act
            var result = await _roleService.GetRoleByNameAsync("Tester");

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("Tester");
        }

        [Fact]
        public async Task GetRoleByNameAsync_WithNonExistentName_ReturnsNull()
        {
            // Act
            var result = await _roleService.GetRoleByNameAsync("NonExistent");

            // Assert
            result.Should().BeNull();
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task GetRoleByNameAsync_WithEmptyName_ThrowsArgumentException(string name)
        {
            // Act
            Func<Task> act = async () => await _roleService.GetRoleByNameAsync(name);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*name*required*");
        }

        #endregion

        #region UpdateRoleAsync Tests

        [Fact]
        public async Task UpdateRoleAsync_WithValidData_UpdatesRole()
        {
            // Arrange
            var createdRole = await _roleService.CreateRoleAsync("OldName", "Old description");
            var newName = "NewName";
            var newDescription = "New description";

            // Act
            var result = await _roleService.UpdateRoleAsync(createdRole.Id, newName, newDescription);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(newName);
            result.Description.Should().Be(newDescription);
        }

        [Fact]
        public async Task UpdateRoleAsync_WithNonExistentId_ThrowsInvalidOperationException()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            Func<Task> act = async () => await _roleService.UpdateRoleAsync(nonExistentId, "Name", "Description");

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage($"*{nonExistentId}*not found*");
        }

        [Theory]
        [InlineData("", "Description")]
        [InlineData(" ", "Description")]
        [InlineData(null, "Description")]
        public async Task UpdateRoleAsync_WithEmptyName_ThrowsArgumentException(string name, string description)
        {
            // Arrange
            var createdRole = await _roleService.CreateRoleAsync("ValidName", "Valid description");

            // Act
            Func<Task> act = async () => await _roleService.UpdateRoleAsync(createdRole.Id, name, description);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*Name*required*");
        }

        [Theory]
        [InlineData("Name", "")]
        [InlineData("Name", " ")]
        [InlineData("Name", null)]
        public async Task UpdateRoleAsync_WithEmptyDescription_ThrowsArgumentException(string name, string description)
        {
            // Arrange
            var createdRole = await _roleService.CreateRoleAsync("ValidName", "Valid description");

            // Act
            Func<Task> act = async () => await _roleService.UpdateRoleAsync(createdRole.Id, name, description);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*Description*required*");
        }

        #endregion

        #region DeleteRoleAsync Tests

        [Fact]
        public async Task DeleteRoleAsync_WithExistingRole_DeletesRole()
        {
            // Arrange
            var createdRole = await _roleService.CreateRoleAsync("ToDelete", "Will be deleted");

            // Act
            await _roleService.DeleteRoleAsync(createdRole.Id);

            // Assert
            var deletedRole = await _roleService.GetRoleByIdAsync(createdRole.Id);
            deletedRole.Should().BeNull();
        }

        [Fact]
        public async Task DeleteRoleAsync_WithNonExistentRole_ThrowsInvalidOperationException()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            Func<Task> act = async () => await _roleService.DeleteRoleAsync(nonExistentId);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage($"*{nonExistentId}*not found*");
        }

        #endregion
    }
}
