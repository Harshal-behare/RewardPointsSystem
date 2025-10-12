using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using RewardPointsSystem.Domain.Entities.Accounts;
using RewardPointsSystem.Infrastructure.Repositories;
using RewardPointsSystem.Application.Services.Accounts;
using RewardPointsSystem.Application.Services.Users;
using Xunit;

namespace RewardPointsSystem.Tests.UnitTests
{
    public class PointsAccountServiceTests : IDisposable
    {
        private readonly InMemoryUnitOfWork _unitOfWork;
        private readonly UserService _userService;
        private readonly PointsAccountService _accountService;

        public PointsAccountServiceTests()
        {
            _unitOfWork = new InMemoryUnitOfWork();
            _userService = new UserService(_unitOfWork);
            _accountService = new PointsAccountService(_unitOfWork);
        }

        public void Dispose()
        {
            _unitOfWork?.Dispose();
        }

        [Fact]
        public async Task CreateAccountAsync_WithValidUser_ShouldCreateAccount()
        {
            // Arrange
            var user = await _userService.CreateUserAsync("user@test.com", "John", "Doe");

            // Act
            var account = await _accountService.CreateAccountAsync(user.Id);

            // Assert
            account.Should().NotBeNull();
            account.UserId.Should().Be(user.Id);
            account.CurrentBalance.Should().Be(0);
            account.TotalEarned.Should().Be(0);
            account.TotalRedeemed.Should().Be(0);
            account.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public async Task CreateAccountAsync_WithNonExistentUser_ShouldThrowException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _accountService.CreateAccountAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task CreateAccountAsync_WithDuplicateAccount_ShouldThrowException()
        {
            // Arrange
            var user = await _userService.CreateUserAsync("user@test.com", "John", "Doe");
            await _accountService.CreateAccountAsync(user.Id);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _accountService.CreateAccountAsync(user.Id));
        }


        [Fact]
        public async Task AddPointsAsync_WithValidAmount_ShouldIncreaseBalance()
        {
            // Arrange
            var user = await _userService.CreateUserAsync("user@test.com", "John", "Doe");
            await _accountService.CreateAccountAsync(user.Id);

            // Act
            await _accountService.AddPointsAsync(user.Id, 500);

            // Assert
            var balance = await _accountService.GetBalanceAsync(user.Id);
            balance.Should().Be(500);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-100)]
        public async Task AddPointsAsync_WithInvalidAmount_ShouldThrowException(int amount)
        {
            // Arrange
            var user = await _userService.CreateUserAsync("user@test.com", "John", "Doe");
            await _accountService.CreateAccountAsync(user.Id);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _accountService.AddPointsAsync(user.Id, amount));
        }

        [Fact]
        public async Task DeductPointsAsync_WithSufficientBalance_ShouldDecreaseBalance()
        {
            // Arrange
            var user = await _userService.CreateUserAsync("user@test.com", "John", "Doe");
            await _accountService.CreateAccountAsync(user.Id);
            await _accountService.AddPointsAsync(user.Id, 500);

            // Act
            await _accountService.DeductPointsAsync(user.Id, 200);

            // Assert
            var balance = await _accountService.GetBalanceAsync(user.Id);
            balance.Should().Be(300);
        }

        [Fact]
        public async Task DeductPointsAsync_WithInsufficientBalance_ShouldThrowException()
        {
            // Arrange
            var user = await _userService.CreateUserAsync("user@test.com", "John", "Doe");
            await _accountService.CreateAccountAsync(user.Id);
            await _accountService.AddPointsAsync(user.Id, 100);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _accountService.DeductPointsAsync(user.Id, 200));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-50)]
        public async Task DeductPointsAsync_WithInvalidAmount_ShouldThrowException(int amount)
        {
            // Arrange
            var user = await _userService.CreateUserAsync("user@test.com", "John", "Doe");
            await _accountService.CreateAccountAsync(user.Id);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _accountService.DeductPointsAsync(user.Id, amount));
        }

        [Fact]
        public async Task GetBalanceAsync_ShouldReturnCorrectBalance()
        {
            // Arrange
            var user = await _userService.CreateUserAsync("user@test.com", "John", "Doe");
            await _accountService.CreateAccountAsync(user.Id);
            await _accountService.AddPointsAsync(user.Id, 750);

            // Act
            var balance = await _accountService.GetBalanceAsync(user.Id);

            // Assert
            balance.Should().Be(750);
        }

        [Fact]
        public async Task HasSufficientBalanceAsync_WithSufficientBalance_ShouldReturnTrue()
        {
            // Arrange
            var user = await _userService.CreateUserAsync("user@test.com", "John", "Doe");
            await _accountService.CreateAccountAsync(user.Id);
            await _accountService.AddPointsAsync(user.Id, 500);

            // Act
            var result = await _accountService.HasSufficientBalanceAsync(user.Id, 300);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasSufficientBalanceAsync_WithInsufficientBalance_ShouldReturnFalse()
        {
            // Arrange
            var user = await _userService.CreateUserAsync("user@test.com", "John", "Doe");
            await _accountService.CreateAccountAsync(user.Id);
            await _accountService.AddPointsAsync(user.Id, 100);

            // Act
            var result = await _accountService.HasSufficientBalanceAsync(user.Id, 300);

            // Assert
            result.Should().BeFalse();
        }
    }

    public class TransactionServiceTests : IDisposable
    {
        private readonly InMemoryUnitOfWork _unitOfWork;
        private readonly UserService _userService;
        private readonly PointsAccountService _accountService;
        private readonly TransactionService _transactionService;

        public TransactionServiceTests()
        {
            _unitOfWork = new InMemoryUnitOfWork();
            _userService = new UserService(_unitOfWork);
            _accountService = new PointsAccountService(_unitOfWork);
            _transactionService = new TransactionService(_unitOfWork);
        }

        public void Dispose()
        {
            _unitOfWork?.Dispose();
        }

        [Fact]
        public async Task RecordEarnedPointsAsync_WithValidData_ShouldRecordTransaction()
        {
            // Arrange
            var user = await _userService.CreateUserAsync("user@test.com", "John", "Doe");
            await _accountService.CreateAccountAsync(user.Id);
            var eventId = Guid.NewGuid();

            // Act
            await _transactionService.RecordEarnedPointsAsync(user.Id, 100, eventId, "Test earned");

            // Assert
            var transactions = await _transactionService.GetUserTransactionsAsync(user.Id);
            transactions.Should().HaveCount(1);
            transactions.First().Points.Should().Be(100);
            transactions.First().Type.Should().Be(TransactionType.Earned);
        }


        [Fact]
        public async Task GetUserTransactionsAsync_WithMultipleTransactions_ShouldReturnAll()
        {
            // Arrange
            var user = await _userService.CreateUserAsync("user@test.com", "John", "Doe");
            await _accountService.CreateAccountAsync(user.Id);

            await _transactionService.RecordEarnedPointsAsync(user.Id, 100, Guid.NewGuid(), "Earned 1");
            await _transactionService.RecordEarnedPointsAsync(user.Id, 200, Guid.NewGuid(), "Earned 2");
            await _transactionService.RecordRedeemedPointsAsync(user.Id, 50, Guid.NewGuid(), "Redeemed 1");

            // Act
            var transactions = await _transactionService.GetUserTransactionsAsync(user.Id);

            // Assert
            transactions.Should().HaveCount(3);
            transactions.Where(t => t.Type == TransactionType.Earned).Should().HaveCount(2);
            transactions.Where(t => t.Type == TransactionType.Redeemed).Should().HaveCount(1);
        }

        [Fact]
        public async Task GetUserTransactionsAsync_WithNoTransactions_ShouldReturnEmpty()
        {
            // Arrange
            var user = await _userService.CreateUserAsync("user@test.com", "John", "Doe");
            await _accountService.CreateAccountAsync(user.Id);

            // Act
            var transactions = await _transactionService.GetUserTransactionsAsync(user.Id);

            // Assert
            transactions.Should().BeEmpty();
        }


        [Fact]
        public async Task RecordTransaction_ShouldSetCorrectTimestamp()
        {
            // Arrange
            var user = await _userService.CreateUserAsync("user@test.com", "John", "Doe");
            await _accountService.CreateAccountAsync(user.Id);

            // Act
            await _transactionService.RecordEarnedPointsAsync(user.Id, 100, Guid.NewGuid(), "Test");

            // Assert
            var transactions = await _transactionService.GetUserTransactionsAsync(user.Id);
            transactions.First().Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }
    }
}
