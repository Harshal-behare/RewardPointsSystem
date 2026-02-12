using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RewardPointsSystem.Application.Services.Auth;
using RewardPointsSystem.Application.Interfaces;
using RewardPointsSystem.Application.Configuration;
using RewardPointsSystem.Application.DTOs.Auth;
using RewardPointsSystem.Domain.Entities.Core;

namespace RewardPointsSystem.Tests.UnitTests
{
    /// <summary>
    /// Unit tests for AuthService
    /// Tests authentication, registration, token management, and password operations
    /// Coverage: 26 test cases
    /// </summary>
    public class AuthServiceTests
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IRoleService> _mockRoleService;
        private readonly Mock<IUserRoleService> _mockUserRoleService;
        private readonly Mock<ITokenService> _mockTokenService;
        private readonly Mock<IPasswordHasher> _mockPasswordHasher;
        private readonly Mock<ILogger<AuthService>> _mockLogger;
        private readonly IOptions<JwtSettings> _jwtSettings;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _mockUserService = new Mock<IUserService>();
            _mockRoleService = new Mock<IRoleService>();
            _mockUserRoleService = new Mock<IUserRoleService>();
            _mockTokenService = new Mock<ITokenService>();
            _mockPasswordHasher = new Mock<IPasswordHasher>();
            _mockLogger = new Mock<ILogger<AuthService>>();

            _jwtSettings = Options.Create(new JwtSettings
            {
                SecretKey = "ThisIsAVerySecureSecretKeyForTesting123!",
                Issuer = "TestIssuer",
                Audience = "TestAudience",
                AccessTokenExpirationMinutes = 30,
                RefreshTokenExpirationDays = 7
            });

            _authService = new AuthService(
                _mockUserService.Object,
                _mockRoleService.Object,
                _mockUserRoleService.Object,
                _mockTokenService.Object,
                _mockPasswordHasher.Object,
                _jwtSettings,
                _mockLogger.Object);
        }

        #region Helper Methods

        private static User CreateTestUser(string email = "test@agdata.com", bool hasPassword = true)
        {
            var user = User.Create(email, "Test", "User");
            if (hasPassword)
            {
                user.SetPasswordHash("hashedpassword123");
            }
            return user;
        }

        private static User CreateInactiveUser(string email = "inactive@agdata.com")
        {
            var user = User.Create(email, "Inactive", "User");
            user.SetPasswordHash("hashedpassword123");
            user.Deactivate(Guid.NewGuid());
            return user;
        }

        private static Role CreateTestRole(string name = "Employee")
        {
            return Role.Create(name, $"{name} role description");
        }

        private void SetupTokenGeneration()
        {
            _mockTokenService.Setup(x => x.GenerateAccessToken(It.IsAny<User>(), It.IsAny<IEnumerable<Role>>()))
                .Returns("test_access_token");
            _mockTokenService.Setup(x => x.GenerateRefreshToken())
                .Returns("test_refresh_token");
            _mockTokenService.Setup(x => x.StoreRefreshTokenAsync(
                It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<string?>()))
                .Returns(Task.CompletedTask);
        }

        #endregion

        #region RegisterAsync Tests

        [Fact]
        public async Task RegisterAsync_WithValidData_ReturnsSuccessWithTokens()
        {
            // Arrange
            var dto = new RegisterRequestDto
            {
                Email = "newuser@agdata.com",
                FirstName = "New",
                LastName = "User",
                Password = "SecurePass123!",
                ConfirmPassword = "SecurePass123!"
            };
            var user = CreateTestUser(dto.Email);
            var employeeRole = CreateTestRole("Employee");

            _mockUserService.Setup(x => x.GetUserByEmailAsync(dto.Email))
                .Returns(Task.FromResult<User>(null!));
            _mockUserService.Setup(x => x.CreateUserAsync(dto.Email, dto.FirstName, dto.LastName))
                .ReturnsAsync(user);
            _mockUserService.Setup(x => x.UpdateUserAsync(It.IsAny<Guid>(), It.IsAny<UserUpdateDto>()))
                .ReturnsAsync(user);
            _mockUserService.Setup(x => x.GetUserByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(user);
            _mockPasswordHasher.Setup(x => x.HashPassword(dto.Password))
                .Returns("hashedpassword");
            _mockRoleService.Setup(x => x.GetRoleByNameAsync("Employee"))
                .ReturnsAsync(employeeRole);
            _mockUserRoleService.Setup(x => x.GetUserRolesAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new List<Role> { employeeRole });
            SetupTokenGeneration();

            // Act
            var result = await _authService.RegisterAsync(dto, "127.0.0.1");

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Email.Should().Be(dto.Email);
            result.Data.AccessToken.Should().NotBeNullOrEmpty();
            result.Data.RefreshToken.Should().NotBeNullOrEmpty();
            result.Data.Roles.Should().Contain("Employee");
        }

        [Fact]
        public async Task RegisterAsync_WithDuplicateEmail_ReturnsConflictError()
        {
            // Arrange
            var dto = new RegisterRequestDto
            {
                Email = "existing@agdata.com",
                FirstName = "Existing",
                LastName = "User",
                Password = "SecurePass123!",
                ConfirmPassword = "SecurePass123!"
            };
            var existingUser = CreateTestUser(dto.Email);

            _mockUserService.Setup(x => x.GetUserByEmailAsync(dto.Email))
                .ReturnsAsync(existingUser);

            // Act
            var result = await _authService.RegisterAsync(dto, "127.0.0.1");

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorType.Should().Be(AuthErrorType.Conflict);
            result.ErrorMessage.Should().Contain("already exists");
        }

        [Fact]
        public async Task RegisterAsync_WithPasswordMismatch_ReturnsValidationError()
        {
            // Arrange
            var dto = new RegisterRequestDto
            {
                Email = "test@agdata.com",
                FirstName = "Test",
                LastName = "User",
                Password = "SecurePass123!",
                ConfirmPassword = "DifferentPass123!"
            };

            // Act
            var result = await _authService.RegisterAsync(dto, "127.0.0.1");

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorType.Should().Be(AuthErrorType.ValidationError);
            result.ErrorMessage.Should().Contain("Passwords do not match");
        }

        [Fact]
        public async Task RegisterAsync_WithWeakPassword_ReturnsValidationError()
        {
            // Arrange
            var dto = new RegisterRequestDto
            {
                Email = "test@agdata.com",
                FirstName = "Test",
                LastName = "User",
                Password = "weak",
                ConfirmPassword = "weak"
            };

            // Act
            var result = await _authService.RegisterAsync(dto, "127.0.0.1");

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorType.Should().Be(AuthErrorType.ValidationError);
            result.ErrorMessage.Should().Contain("at least 8 characters");
        }

        [Fact]
        public async Task RegisterAsync_WithEmptyPassword_ReturnsValidationError()
        {
            // Arrange
            var dto = new RegisterRequestDto
            {
                Email = "test@agdata.com",
                FirstName = "Test",
                LastName = "User",
                Password = "",
                ConfirmPassword = ""
            };

            // Act
            var result = await _authService.RegisterAsync(dto, "127.0.0.1");

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorType.Should().Be(AuthErrorType.ValidationError);
        }

        [Fact]
        public async Task RegisterAsync_AssignsEmployeeRoleToNewUser()
        {
            // Arrange
            var dto = new RegisterRequestDto
            {
                Email = "newuser@agdata.com",
                FirstName = "New",
                LastName = "User",
                Password = "SecurePass123!",
                ConfirmPassword = "SecurePass123!"
            };
            var user = CreateTestUser(dto.Email);
            var employeeRole = CreateTestRole("Employee");

            _mockUserService.Setup(x => x.GetUserByEmailAsync(dto.Email))
                .Returns(Task.FromResult<User>(null!));
            _mockUserService.Setup(x => x.CreateUserAsync(dto.Email, dto.FirstName, dto.LastName))
                .ReturnsAsync(user);
            _mockUserService.Setup(x => x.UpdateUserAsync(It.IsAny<Guid>(), It.IsAny<UserUpdateDto>()))
                .ReturnsAsync(user);
            _mockUserService.Setup(x => x.GetUserByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(user);
            _mockPasswordHasher.Setup(x => x.HashPassword(dto.Password))
                .Returns("hashedpassword");
            _mockRoleService.Setup(x => x.GetRoleByNameAsync("Employee"))
                .ReturnsAsync(employeeRole);
            _mockUserRoleService.Setup(x => x.GetUserRolesAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new List<Role> { employeeRole });
            SetupTokenGeneration();

            // Act
            await _authService.RegisterAsync(dto, "127.0.0.1");

            // Assert
            _mockUserRoleService.Verify(x => x.AssignRoleAsync(
                It.IsAny<Guid>(),
                employeeRole.Id,
                It.IsAny<Guid>()), Times.Once);
        }

        #endregion

        #region LoginAsync Tests

        [Fact]
        public async Task LoginAsync_WithValidCredentials_ReturnsSuccessWithTokens()
        {
            // Arrange
            var dto = new LoginRequestDto
            {
                Email = "user@agdata.com",
                Password = "SecurePass123!"
            };
            var user = CreateTestUser(dto.Email, hasPassword: true);
            var roles = new List<Role> { CreateTestRole("Employee") };

            _mockUserService.Setup(x => x.GetUserByEmailAsync(dto.Email))
                .ReturnsAsync(user);
            _mockUserService.Setup(x => x.GetUserByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(user);
            _mockPasswordHasher.Setup(x => x.VerifyPassword(dto.Password, It.IsAny<string>()))
                .Returns(true);
            _mockUserRoleService.Setup(x => x.GetUserRolesAsync(It.IsAny<Guid>()))
                .ReturnsAsync(roles);
            SetupTokenGeneration();

            // Act
            var result = await _authService.LoginAsync(dto, "127.0.0.1");

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Email.Should().Be(dto.Email);
            result.Data.AccessToken.Should().NotBeNullOrEmpty();
            result.Data.RefreshToken.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task LoginAsync_WithNonExistentEmail_ReturnsBadRequest()
        {
            // Arrange
            var dto = new LoginRequestDto
            {
                Email = "nonexistent@agdata.com",
                Password = "AnyPassword123!"
            };

            _mockUserService.Setup(x => x.GetUserByEmailAsync(dto.Email))
                .Returns(Task.FromResult<User>(null!));

            // Act
            var result = await _authService.LoginAsync(dto, "127.0.0.1");

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorType.Should().Be(AuthErrorType.BadRequest);
            result.ErrorMessage.Should().Contain("Invalid email or password");
        }

        [Fact]
        public async Task LoginAsync_WithWrongPassword_ReturnsBadRequest()
        {
            // Arrange
            var dto = new LoginRequestDto
            {
                Email = "user@agdata.com",
                Password = "WrongPassword123!"
            };
            var user = CreateTestUser(dto.Email, hasPassword: true);

            _mockUserService.Setup(x => x.GetUserByEmailAsync(dto.Email))
                .ReturnsAsync(user);
            _mockPasswordHasher.Setup(x => x.VerifyPassword(dto.Password, It.IsAny<string>()))
                .Returns(false);

            // Act
            var result = await _authService.LoginAsync(dto, "127.0.0.1");

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorType.Should().Be(AuthErrorType.BadRequest);
            result.ErrorMessage.Should().Contain("Invalid email or password");
        }

        [Fact]
        public async Task LoginAsync_WithInactiveUser_ReturnsBadRequest()
        {
            // Arrange
            var dto = new LoginRequestDto
            {
                Email = "inactive@agdata.com",
                Password = "SecurePass123!"
            };
            var user = CreateInactiveUser(dto.Email);

            _mockUserService.Setup(x => x.GetUserByEmailAsync(dto.Email))
                .ReturnsAsync(user);

            // Act
            var result = await _authService.LoginAsync(dto, "127.0.0.1");

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorType.Should().Be(AuthErrorType.BadRequest);
            result.ErrorMessage.Should().Contain("inactive");
        }

        [Fact]
        public async Task LoginAsync_WithValidCredentials_ReturnsUserRoles()
        {
            // Arrange
            var dto = new LoginRequestDto
            {
                Email = "admin@agdata.com",
                Password = "SecurePass123!"
            };
            var user = CreateTestUser(dto.Email, hasPassword: true);
            var roles = new List<Role>
            {
                CreateTestRole("Admin"),
                CreateTestRole("Employee")
            };

            _mockUserService.Setup(x => x.GetUserByEmailAsync(dto.Email))
                .ReturnsAsync(user);
            _mockUserService.Setup(x => x.GetUserByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(user);
            _mockPasswordHasher.Setup(x => x.VerifyPassword(dto.Password, It.IsAny<string>()))
                .Returns(true);
            _mockUserRoleService.Setup(x => x.GetUserRolesAsync(It.IsAny<Guid>()))
                .ReturnsAsync(roles);
            SetupTokenGeneration();

            // Act
            var result = await _authService.LoginAsync(dto, "127.0.0.1");

            // Assert
            result.Success.Should().BeTrue();
            result.Data!.Roles.Should().HaveCount(2);
            result.Data.Roles.Should().Contain("Admin");
            result.Data.Roles.Should().Contain("Employee");
        }

        [Fact]
        public async Task LoginAsync_WithUserWithoutPassword_ReturnsBadRequest()
        {
            // Arrange
            var dto = new LoginRequestDto
            {
                Email = "nopassword@agdata.com",
                Password = "AnyPassword123!"
            };
            var user = CreateTestUser(dto.Email, hasPassword: false);

            _mockUserService.Setup(x => x.GetUserByEmailAsync(dto.Email))
                .ReturnsAsync(user);

            // Act
            var result = await _authService.LoginAsync(dto, "127.0.0.1");

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorType.Should().Be(AuthErrorType.BadRequest);
        }

        #endregion

        #region RefreshTokenAsync Tests

        [Fact]
        public async Task RefreshTokenAsync_WithValidToken_ReturnsNewTokens()
        {
            // Arrange
            var refreshToken = "valid_refresh_token";
            var userId = Guid.NewGuid();
            var user = CreateTestUser("user@agdata.com");
            var roles = new List<Role> { CreateTestRole("Employee") };

            _mockTokenService.Setup(x => x.ValidateRefreshTokenAsync(refreshToken))
                .ReturnsAsync(userId);
            _mockUserService.Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync(user);
            _mockUserRoleService.Setup(x => x.GetUserRolesAsync(It.IsAny<Guid>()))
                .ReturnsAsync(roles);
            _mockTokenService.Setup(x => x.GenerateAccessToken(user, roles))
                .Returns("new_access_token");
            _mockTokenService.Setup(x => x.GenerateRefreshToken())
                .Returns("new_refresh_token");
            _mockTokenService.Setup(x => x.StoreRefreshTokenAsync(
                It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<string?>()))
                .Returns(Task.CompletedTask);
            _mockTokenService.Setup(x => x.RevokeRefreshTokenAsync(
                refreshToken, It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>()))
                .ReturnsAsync(true);

            // Act
            var result = await _authService.RefreshTokenAsync(refreshToken, "127.0.0.1");

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.AccessToken.Should().Be("new_access_token");
            result.Data.RefreshToken.Should().Be("new_refresh_token");
        }

        [Fact]
        public async Task RefreshTokenAsync_WithInvalidToken_ReturnsUnauthorized()
        {
            // Arrange
            var refreshToken = "invalid_refresh_token";

            _mockTokenService.Setup(x => x.ValidateRefreshTokenAsync(refreshToken))
                .Returns(Task.FromResult<Guid?>(null));

            // Act
            var result = await _authService.RefreshTokenAsync(refreshToken, "127.0.0.1");

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorType.Should().Be(AuthErrorType.Unauthorized);
            result.ErrorMessage.Should().Contain("Invalid or expired");
        }

        [Fact]
        public async Task RefreshTokenAsync_WithInactiveUser_ReturnsUnauthorized()
        {
            // Arrange
            var refreshToken = "valid_refresh_token";
            var userId = Guid.NewGuid();
            var user = CreateInactiveUser("inactive@agdata.com");

            _mockTokenService.Setup(x => x.ValidateRefreshTokenAsync(refreshToken))
                .ReturnsAsync(userId);
            _mockUserService.Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync(user);

            // Act
            var result = await _authService.RefreshTokenAsync(refreshToken, "127.0.0.1");

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorType.Should().Be(AuthErrorType.Unauthorized);
            result.ErrorMessage.Should().Contain("inactive");
        }

        [Fact]
        public async Task RefreshTokenAsync_WithNonExistentUser_ReturnsUnauthorized()
        {
            // Arrange
            var refreshToken = "valid_refresh_token";
            var userId = Guid.NewGuid();

            _mockTokenService.Setup(x => x.ValidateRefreshTokenAsync(refreshToken))
                .ReturnsAsync(userId);
            _mockUserService.Setup(x => x.GetUserByIdAsync(userId))
                .Returns(Task.FromResult<User>(null!));

            // Act
            var result = await _authService.RefreshTokenAsync(refreshToken, "127.0.0.1");

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorType.Should().Be(AuthErrorType.Unauthorized);
        }

        #endregion

        #region LogoutAsync Tests

        [Fact]
        public async Task LogoutAsync_RevokesAllUserTokens()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var expectedRevokedCount = 3;

            _mockTokenService.Setup(x => x.RevokeAllUserRefreshTokensAsync(
                userId, It.IsAny<string?>(), It.IsAny<string?>()))
                .ReturnsAsync(expectedRevokedCount);

            // Act
            var result = await _authService.LogoutAsync(userId, "127.0.0.1");

            // Assert
            result.Should().Be(expectedRevokedCount);
            _mockTokenService.Verify(x => x.RevokeAllUserRefreshTokensAsync(
                userId, "127.0.0.1", "User logout"), Times.Once);
        }

        [Fact]
        public async Task LogoutAsync_WithNoTokens_ReturnsZero()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _mockTokenService.Setup(x => x.RevokeAllUserRefreshTokensAsync(
                userId, It.IsAny<string?>(), It.IsAny<string?>()))
                .ReturnsAsync(0);

            // Act
            var result = await _authService.LogoutAsync(userId, "127.0.0.1");

            // Assert
            result.Should().Be(0);
        }

        #endregion

        #region GetCurrentUserAsync Tests

        [Fact]
        public async Task GetCurrentUserAsync_WithValidUserId_ReturnsUserInfo()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = CreateTestUser("current@agdata.com");
            var roles = new List<Role> { CreateTestRole("Employee") };

            _mockUserService.Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync(user);
            _mockUserRoleService.Setup(x => x.GetUserRolesAsync(It.IsAny<Guid>()))
                .ReturnsAsync(roles);

            // Act
            var result = await _authService.GetCurrentUserAsync(userId);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Email.Should().Be("current@agdata.com");
            result.Data.FirstName.Should().Be("Test");
            result.Data.LastName.Should().Be("User");
            result.Data.Roles.Should().Contain("Employee");
        }

        [Fact]
        public async Task GetCurrentUserAsync_WithNonExistentUser_ReturnsNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _mockUserService.Setup(x => x.GetUserByIdAsync(userId))
                .Returns(Task.FromResult<User>(null!));

            // Act
            var result = await _authService.GetCurrentUserAsync(userId);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorType.Should().Be(AuthErrorType.NotFound);
            result.ErrorMessage.Should().Contain("not found");
        }

        [Fact]
        public async Task GetCurrentUserAsync_WithMultipleRoles_ReturnsAllRoles()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = CreateTestUser("admin@agdata.com");
            var roles = new List<Role>
            {
                CreateTestRole("Admin"),
                CreateTestRole("Employee"),
                CreateTestRole("Manager")
            };

            _mockUserService.Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync(user);
            _mockUserRoleService.Setup(x => x.GetUserRolesAsync(It.IsAny<Guid>()))
                .ReturnsAsync(roles);

            // Act
            var result = await _authService.GetCurrentUserAsync(userId);

            // Assert
            result.Success.Should().BeTrue();
            result.Data!.Roles.Should().HaveCount(3);
            result.Data.Roles.Should().Contain("Admin");
            result.Data.Roles.Should().Contain("Employee");
            result.Data.Roles.Should().Contain("Manager");
        }

        #endregion

        #region ChangePasswordAsync Tests

        [Fact]
        public async Task ChangePasswordAsync_WithValidData_ReturnsSuccess()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var dto = new ChangePasswordRequestDto
            {
                CurrentPassword = "OldPassword123!",
                NewPassword = "NewPassword456!"
            };
            var user = CreateTestUser("user@agdata.com", hasPassword: true);

            _mockUserService.Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync(user);
            _mockPasswordHasher.Setup(x => x.VerifyPassword(dto.CurrentPassword, It.IsAny<string>()))
                .Returns(true);
            _mockPasswordHasher.Setup(x => x.HashPassword(dto.NewPassword))
                .Returns("new_hashed_password");
            _mockUserService.Setup(x => x.UpdateUserAsync(userId, It.IsAny<UserUpdateDto>()))
                .ReturnsAsync(user);

            // Act
            var result = await _authService.ChangePasswordAsync(userId, dto);

            // Assert
            result.Success.Should().BeTrue();
        }

        [Fact]
        public async Task ChangePasswordAsync_WithWrongCurrentPassword_ReturnsBadRequest()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var dto = new ChangePasswordRequestDto
            {
                CurrentPassword = "WrongPassword123!",
                NewPassword = "NewPassword456!"
            };
            var user = CreateTestUser("user@agdata.com", hasPassword: true);

            _mockUserService.Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync(user);
            _mockPasswordHasher.Setup(x => x.VerifyPassword(dto.CurrentPassword, It.IsAny<string>()))
                .Returns(false);

            // Act
            var result = await _authService.ChangePasswordAsync(userId, dto);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorType.Should().Be(AuthErrorType.BadRequest);
            result.ErrorMessage.Should().Contain("Current password is incorrect");
        }

        [Fact]
        public async Task ChangePasswordAsync_WithWeakNewPassword_ReturnsValidationError()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var dto = new ChangePasswordRequestDto
            {
                CurrentPassword = "OldPassword123!",
                NewPassword = "weak"
            };
            var user = CreateTestUser("user@agdata.com", hasPassword: true);

            _mockUserService.Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync(user);
            _mockPasswordHasher.Setup(x => x.VerifyPassword(dto.CurrentPassword, It.IsAny<string>()))
                .Returns(true);

            // Act
            var result = await _authService.ChangePasswordAsync(userId, dto);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorType.Should().Be(AuthErrorType.ValidationError);
            result.ErrorMessage.Should().Contain("at least 8 characters");
        }

        [Fact]
        public async Task ChangePasswordAsync_WithNonExistentUser_ReturnsNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var dto = new ChangePasswordRequestDto
            {
                CurrentPassword = "OldPassword123!",
                NewPassword = "NewPassword456!"
            };

            _mockUserService.Setup(x => x.GetUserByIdAsync(userId))
                .Returns(Task.FromResult<User>(null!));

            // Act
            var result = await _authService.ChangePasswordAsync(userId, dto);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorType.Should().Be(AuthErrorType.NotFound);
            result.ErrorMessage.Should().Contain("not found");
        }

        [Fact]
        public async Task ChangePasswordAsync_WithEmptyCurrentPassword_ReturnsValidationError()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var dto = new ChangePasswordRequestDto
            {
                CurrentPassword = "",
                NewPassword = "NewPassword456!"
            };

            // Act
            var result = await _authService.ChangePasswordAsync(userId, dto);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorType.Should().Be(AuthErrorType.ValidationError);
        }

        [Fact]
        public async Task ChangePasswordAsync_WithEmptyNewPassword_ReturnsValidationError()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var dto = new ChangePasswordRequestDto
            {
                CurrentPassword = "OldPassword123!",
                NewPassword = ""
            };

            // Act
            var result = await _authService.ChangePasswordAsync(userId, dto);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorType.Should().Be(AuthErrorType.ValidationError);
        }

        #endregion
    }
}
