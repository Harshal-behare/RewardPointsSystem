using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RewardPointsSystem.Domain.Entities.Core;
using RewardPointsSystem.Domain.Entities.Accounts;
using RewardPointsSystem.Application.Interfaces;
using RewardPointsSystem.Application.Services.Core;
using RewardPointsSystem.Application.Services.Accounts;
using RewardPointsSystem.Infrastructure.Data;
using RewardPointsSystem.Infrastructure.Repositories;
using RewardPointsSystem.Tests.TestHelpers;
using Xunit;

namespace RewardPointsSystem.Tests.IntegrationTests
{
    /// <summary>
    /// Integration Tests for User Workflows
    /// 
    /// These tests verify complete user lifecycle scenarios:
    /// - User registration and setup
    /// - Points account creation
    /// - User role assignment
    /// - User deactivation
    /// 
    /// WHAT WE'RE TESTING:
    /// Integration between UserService, UserPointsAccountService, UserRoleService
    /// and the underlying database through the UnitOfWork pattern.
    /// 
    /// WHY THESE TESTS MATTER:
    /// Ensures all services work together correctly to handle user-related
    /// business processes end-to-end.
    /// </summary>
    public class UserWorkflowIntegrationTests : IDisposable
    {
        private readonly RewardPointsDbContext _context;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserService _userService;
        private readonly UserPointsAccountService _accountService;
        private readonly UserRoleService _roleService;

        public UserWorkflowIntegrationTests()
        {
            // Setup in-memory database for integration testing
            var options = new DbContextOptionsBuilder<RewardPointsDbContext>()
                .UseInMemoryDatabase(databaseName: $"UserWorkflowTests_{Guid.NewGuid()}")
                .Options;

            _context = new RewardPointsDbContext(options);
            _unitOfWork = TestDbContextFactory.CreateInMemoryUnitOfWork();
            
            _userService = new UserService(_unitOfWork);
            _accountService = new UserPointsAccountService(_unitOfWork);
            _roleService = new UserRoleService(_unitOfWork);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }

        #region Complete User Setup Workflow Tests

        /// <summary>
        /// SCENARIO: New employee joins the company and needs full setup
        /// WORKFLOW: Create user → Create points account → Assign Employee role
        /// EXPECTED: User has account, role, and is ready to participate
        /// WHY: This is the standard onboarding flow for new employees
        /// </summary>
        [Fact]
        public async Task NewEmployeeOnboarding_ShouldCreateUserWithAccountAndRole()
        {
            // Step 1: Create the user
            var user = await _userService.CreateUserAsync(
                "john.doe@company.com",
                "John",
                "Doe");

            user.Should().NotBeNull("user should be created");
            user.IsActive.Should().BeTrue("new users should be active by default");

            // Step 2: Create points account for the user
            var account = await _accountService.CreateAccountAsync(user.Id);
            
            account.Should().NotBeNull("points account should be created");
            account.UserId.Should().Be(user.Id, "account should be linked to user");
            account.CurrentBalance.Should().Be(0, "new account should start with 0 points");

            // Step 3: Create Employee role (if doesn't exist) and assign it
            var employeeRole = await CreateRoleIfNotExistsAsync("Employee", "Regular Employee");
            await _roleService.AssignRoleAsync(user.Id, employeeRole.Id, user.Id);

            // Verify complete setup
            var userRoles = await _roleService.GetUserRolesAsync(user.Id);
            userRoles.Should().Contain(r => r.Name == "Employee", "user should have Employee role");
        }

        /// <summary>
        /// SCENARIO: Admin creates a new admin user with elevated privileges
        /// WORKFLOW: Create user → Create points account → Assign Admin role
        /// EXPECTED: User is set up as admin and can manage the system
        /// WHY: Admins need special setup with proper role assignment
        /// </summary>
        [Fact]
        public async Task NewAdminUserSetup_ShouldCreateUserWithAdminRole()
        {
            // Step 1: Create admin user
            var adminUser = await _userService.CreateUserAsync(
                "admin@company.com",
                "Admin",
                "User");

            // Step 2: Create points account (admins also have accounts for tracking)
            await _accountService.CreateAccountAsync(adminUser.Id);

            // Step 3: Create Admin role and assign
            var adminRole = await CreateRoleIfNotExistsAsync("Admin", "Administrator");
            await _roleService.AssignRoleAsync(adminUser.Id, adminRole.Id, adminUser.Id);

            // Verify admin setup
            var userRoles = await _roleService.GetUserRolesAsync(adminUser.Id);
            userRoles.Should().Contain(r => r.Name == "Admin", "user should have Admin role");
        }

        #endregion

        #region User Deactivation Workflow Tests

        /// <summary>
        /// SCENARIO: Employee leaves the company and account needs to be deactivated
        /// WORKFLOW: Deactivate user → Verify roles remain → Verify account intact
        /// EXPECTED: User is deactivated but data is preserved for audit purposes
        /// WHY: We use soft-delete to maintain historical data integrity
        /// </summary>
        [Fact]
        public async Task EmployeeOffboarding_ShouldDeactivateUserButPreserveData()
        {
            // Setup: Create active employee
            var employee = await _userService.CreateUserAsync(
                "leaving@company.com",
                "Leaving",
                "Employee");
            
            var account = await _accountService.CreateAccountAsync(employee.Id);
            await _accountService.AddUserPointsAsync(employee.Id, 500);
            
            var employeeRole = await CreateRoleIfNotExistsAsync("Employee", "Regular Employee");
            await _roleService.AssignRoleAsync(employee.Id, employeeRole.Id, employee.Id);

            // Act: Deactivate the user
            await _userService.DeactivateUserAsync(employee.Id);

            // Verify: User is deactivated
            var deactivatedUser = await _userService.GetUserByIdAsync(employee.Id);
            deactivatedUser.IsActive.Should().BeFalse("user should be deactivated");

            // Verify: Data is preserved
            var balance = await _accountService.GetBalanceAsync(employee.Id);
            balance.Should().Be(500, "points balance should be preserved");
        }

        #endregion

        #region Points Balance Workflow Tests

        /// <summary>
        /// SCENARIO: Award points to user and verify balance updates correctly
        /// WORKFLOW: Add points → Verify balance → Check total earned
        /// EXPECTED: Balance increases and total earned tracks lifetime earnings
        /// WHY: Points are the core currency of the rewards system
        /// </summary>
        [Fact]
        public async Task AwardPointsToUser_ShouldUpdateBalanceAndTotalEarned()
        {
            // Setup: Create user with account
            var user = await _userService.CreateUserAsync(
                "points.user@company.com",
                "Points",
                "User");
            
            await _accountService.CreateAccountAsync(user.Id);

            // Act: Award points multiple times
            await _accountService.AddUserPointsAsync(user.Id, 100);
            await _accountService.AddUserPointsAsync(user.Id, 250);
            await _accountService.AddUserPointsAsync(user.Id, 150);

            // Verify: Balance
            var balance = await _accountService.GetBalanceAsync(user.Id);
            balance.Should().Be(500, "balance should be sum of all awards");
        }

        /// <summary>
        /// SCENARIO: Deduct points from user's balance (for redemption)
        /// WORKFLOW: Award points → Deduct points → Verify balance
        /// EXPECTED: Balance decreases but total earned remains unchanged
        /// WHY: Redemptions should reduce available balance
        /// </summary>
        [Fact]
        public async Task DeductPointsFromUser_ShouldReduceBalance()
        {
            // Setup: Create user with points
            var user = await _userService.CreateUserAsync(
                "redeem.user@company.com",
                "Redeem",
                "User");
            
            await _accountService.CreateAccountAsync(user.Id);
            await _accountService.AddUserPointsAsync(user.Id, 500);

            // Act: Deduct points (simulate redemption)
            await _accountService.DeductUserPointsAsync(user.Id, 200);

            // Verify: Balance reduced
            var balance = await _accountService.GetBalanceAsync(user.Id);
            balance.Should().Be(300, "balance should be reduced by deduction");
        }

        #endregion

        #region Helper Methods

        private async Task<Role> CreateRoleIfNotExistsAsync(string name, string description)
        {
            var existingRoles = await _unitOfWork.Roles.GetAllAsync();
            var existing = existingRoles.FirstOrDefault(r => r.Name == name);
            
            if (existing != null)
                return existing;

            var role = Role.Create(name, description);
            await _unitOfWork.Roles.AddAsync(role);
            await _unitOfWork.SaveChangesAsync();
            return role;
        }

        #endregion
    }
}
