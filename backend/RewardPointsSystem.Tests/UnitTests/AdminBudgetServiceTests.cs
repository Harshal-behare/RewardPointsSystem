using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using RewardPointsSystem.Application.Services.Admin;
using RewardPointsSystem.Application.DTOs.Admin;
using RewardPointsSystem.Application.Interfaces;
using RewardPointsSystem.Domain.Entities.Core;
using RewardPointsSystem.Tests.TestHelpers;

namespace RewardPointsSystem.Tests.UnitTests
{
    /// <summary>
    /// Unit tests for AdminBudgetService
    /// Tests budget management, validation, and tracking
    /// Following: Isolation, AAA pattern, determinism, no shared state
    /// Coverage: 10 test cases
    /// </summary>
    public class AdminBudgetServiceTests : IDisposable
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly AdminBudgetService _budgetService;
        private readonly Guid _adminUserId;

        public AdminBudgetServiceTests()
        {
            _unitOfWork = TestDbContextFactory.CreateCleanSqlServerUnitOfWork();
            _budgetService = new AdminBudgetService(_unitOfWork);
            
            // Create a test admin user for FK constraints
            var adminUser = User.Create($"admin_{Guid.NewGuid()}@test.com", "Test", "Admin");
            adminUser.SetPasswordHash("testhash");
            _unitOfWork.Users.AddAsync(adminUser).Wait();
            _unitOfWork.SaveChangesAsync().Wait();
            _adminUserId = adminUser.Id;
        }

        public void Dispose()
        {
            _unitOfWork?.Dispose();
        }

        #region GetCurrentBudgetAsync Tests

        [Fact]
        public async Task GetCurrentBudgetAsync_WithNoBudget_ReturnsNull()
        {
            // Act
            var result = await _budgetService.GetCurrentBudgetAsync(_adminUserId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetCurrentBudgetAsync_WithExistingBudget_ReturnsBudget()
        {
            // Arrange
            var setBudgetDto = new SetBudgetDto
            {
                BudgetLimit = 10000,
                IsHardLimit = false,
                WarningThreshold = 80
            };
            await _budgetService.SetBudgetAsync(_adminUserId, setBudgetDto);

            // Act
            var result = await _budgetService.GetCurrentBudgetAsync(_adminUserId);

            // Assert
            result.Should().NotBeNull();
            result!.BudgetLimit.Should().Be(10000);
            result.PointsAwarded.Should().Be(0);
            result.RemainingBudget.Should().Be(10000);
        }

        #endregion

        #region SetBudgetAsync Tests

        [Fact]
        public async Task SetBudgetAsync_WithValidAmount_SetsBudget()
        {
            // Arrange
            var dto = new SetBudgetDto
            {
                BudgetLimit = 5000,
                IsHardLimit = true,
                WarningThreshold = 75
            };

            // Act
            var result = await _budgetService.SetBudgetAsync(_adminUserId, dto);

            // Assert
            result.Should().NotBeNull();
            result.BudgetLimit.Should().Be(5000);
            result.IsHardLimit.Should().BeTrue();
            result.WarningThreshold.Should().Be(75);
        }

        [Fact]
        public async Task SetBudgetAsync_UpdatesExistingBudget()
        {
            // Arrange
            var initialDto = new SetBudgetDto { BudgetLimit = 5000, IsHardLimit = false, WarningThreshold = 80 };
            await _budgetService.SetBudgetAsync(_adminUserId, initialDto);

            var updateDto = new SetBudgetDto { BudgetLimit = 8000, IsHardLimit = true, WarningThreshold = 90 };

            // Act
            var result = await _budgetService.SetBudgetAsync(_adminUserId, updateDto);

            // Assert
            result.BudgetLimit.Should().Be(8000);
            result.IsHardLimit.Should().BeTrue();
            result.WarningThreshold.Should().Be(90);
        }

        [Fact]
        public async Task SetBudgetAsync_WithZeroBudget_ThrowsArgumentException()
        {
            // Arrange
            var dto = new SetBudgetDto { BudgetLimit = 0, IsHardLimit = false, WarningThreshold = 80 };

            // Act
            Func<Task> act = async () => await _budgetService.SetBudgetAsync(_adminUserId, dto);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*Budget limit must be positive*");
        }

        [Fact]
        public async Task SetBudgetAsync_WithInvalidThreshold_ThrowsArgumentException()
        {
            // Arrange
            var dto = new SetBudgetDto { BudgetLimit = 5000, IsHardLimit = false, WarningThreshold = 150 };

            // Act
            Func<Task> act = async () => await _budgetService.SetBudgetAsync(_adminUserId, dto);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*Warning threshold must be between 0 and 100*");
        }

        #endregion

        #region ValidatePointsAwardAsync Tests

        [Fact]
        public async Task ValidatePointsAwardAsync_WithinBudget_ReturnsAllowed()
        {
            // Arrange
            var dto = new SetBudgetDto { BudgetLimit = 10000, IsHardLimit = false, WarningThreshold = 80 };
            await _budgetService.SetBudgetAsync(_adminUserId, dto);

            // Act
            var result = await _budgetService.ValidatePointsAwardAsync(_adminUserId, 5000);

            // Assert
            result.IsAllowed.Should().BeTrue();
            result.IsWarning.Should().BeFalse();
            result.RemainingBudget.Should().Be(5000);
            result.PointsAfterAward.Should().Be(5000);
        }

        [Fact]
        public async Task ValidatePointsAwardAsync_ExceedsSoftLimit_ReturnsWarning()
        {
            // Arrange
            var dto = new SetBudgetDto { BudgetLimit = 10000, IsHardLimit = false, WarningThreshold = 80 };
            await _budgetService.SetBudgetAsync(_adminUserId, dto);
            await _budgetService.RecordPointsAwardedAsync(_adminUserId, 5000);

            // Act - awarding 6000 more will exceed the 10000 limit
            var result = await _budgetService.ValidatePointsAwardAsync(_adminUserId, 6000);

            // Assert
            result.IsAllowed.Should().BeTrue(); // Soft limit allows exceeding
            result.IsWarning.Should().BeTrue();
            result.Message.Should().Contain("exceed");
        }

        [Fact]
        public async Task ValidatePointsAwardAsync_ExceedsHardLimit_ReturnsNotAllowed()
        {
            // Arrange
            var dto = new SetBudgetDto { BudgetLimit = 10000, IsHardLimit = true, WarningThreshold = 80 };
            await _budgetService.SetBudgetAsync(_adminUserId, dto);
            await _budgetService.RecordPointsAwardedAsync(_adminUserId, 5000);

            // Act - awarding 6000 more will exceed the 10000 hard limit
            var result = await _budgetService.ValidatePointsAwardAsync(_adminUserId, 6000);

            // Assert
            result.IsAllowed.Should().BeFalse(); // Hard limit blocks
            result.IsWarning.Should().BeFalse();
            result.Message.Should().Contain("Cannot award");
            result.Message.Should().Contain("hard budget limit");
        }

        [Fact]
        public async Task ValidatePointsAwardAsync_ApproachingThreshold_ReturnsWarning()
        {
            // Arrange
            var dto = new SetBudgetDto { BudgetLimit = 10000, IsHardLimit = false, WarningThreshold = 80 };
            await _budgetService.SetBudgetAsync(_adminUserId, dto);
            await _budgetService.RecordPointsAwardedAsync(_adminUserId, 7000);

            // Act - awarding 1500 more will be at 85% (above 80% threshold)
            var result = await _budgetService.ValidatePointsAwardAsync(_adminUserId, 1500);

            // Assert
            result.IsAllowed.Should().BeTrue();
            result.IsWarning.Should().BeTrue();
            result.Message.Should().Contain("85");  // Usage percentage
        }

        [Fact]
        public async Task ValidatePointsAwardAsync_NoBudgetSet_ReturnsAllowed()
        {
            // Act
            var result = await _budgetService.ValidatePointsAwardAsync(_adminUserId, 10000);

            // Assert
            result.IsAllowed.Should().BeTrue();
            result.IsWarning.Should().BeFalse();
            result.RemainingBudget.Should().Be(int.MaxValue);
        }

        #endregion

        #region RecordPointsAwardedAsync Tests

        [Fact]
        public async Task RecordPointsAwardedAsync_ValidPoints_RecordsPoints()
        {
            // Arrange
            var dto = new SetBudgetDto { BudgetLimit = 10000, IsHardLimit = false, WarningThreshold = 80 };
            await _budgetService.SetBudgetAsync(_adminUserId, dto);

            // Act
            await _budgetService.RecordPointsAwardedAsync(_adminUserId, 3000);

            // Assert
            var budget = await _budgetService.GetCurrentBudgetAsync(_adminUserId);
            budget!.PointsAwarded.Should().Be(3000);
            budget.RemainingBudget.Should().Be(7000);
        }

        [Fact]
        public async Task RecordPointsAwardedAsync_MultipleAwards_CumulativeTotal()
        {
            // Arrange
            var dto = new SetBudgetDto { BudgetLimit = 10000, IsHardLimit = false, WarningThreshold = 80 };
            await _budgetService.SetBudgetAsync(_adminUserId, dto);

            // Act
            await _budgetService.RecordPointsAwardedAsync(_adminUserId, 2000);
            await _budgetService.RecordPointsAwardedAsync(_adminUserId, 1500);
            await _budgetService.RecordPointsAwardedAsync(_adminUserId, 500);

            // Assert
            var budget = await _budgetService.GetCurrentBudgetAsync(_adminUserId);
            budget!.PointsAwarded.Should().Be(4000);
            budget.RemainingBudget.Should().Be(6000);
        }

        [Fact]
        public async Task RecordPointsAwardedAsync_ZeroPoints_ThrowsArgumentException()
        {
            // Arrange
            var dto = new SetBudgetDto { BudgetLimit = 10000, IsHardLimit = false, WarningThreshold = 80 };
            await _budgetService.SetBudgetAsync(_adminUserId, dto);

            // Act
            Func<Task> act = async () => await _budgetService.RecordPointsAwardedAsync(_adminUserId, 0);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*Points must be positive*");
        }

        #endregion

        #region GetBudgetHistoryAsync Tests

        [Fact]
        public async Task GetBudgetHistoryAsync_WithNoHistory_ReturnsEmptyCollection()
        {
            // Act
            var result = await _budgetService.GetBudgetHistoryAsync(_adminUserId);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetBudgetHistoryAsync_WithCurrentMonthBudget_ReturnsHistory()
        {
            // Arrange
            var dto = new SetBudgetDto { BudgetLimit = 10000, IsHardLimit = false, WarningThreshold = 80 };
            await _budgetService.SetBudgetAsync(_adminUserId, dto);
            await _budgetService.RecordPointsAwardedAsync(_adminUserId, 5000);

            // Act
            var result = await _budgetService.GetBudgetHistoryAsync(_adminUserId);

            // Assert
            var history = result.ToList();
            history.Should().HaveCount(1);
            history[0].BudgetLimit.Should().Be(10000);
            history[0].PointsAwarded.Should().Be(5000);
            history[0].UsagePercentage.Should().Be(50);
        }

        #endregion
    }
}
