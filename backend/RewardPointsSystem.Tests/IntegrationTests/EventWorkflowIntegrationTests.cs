using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using RewardPointsSystem.Domain.Entities.Core;
using RewardPointsSystem.Domain.Entities.Events;
using RewardPointsSystem.Application.Interfaces;
using RewardPointsSystem.Application.Services.Core;
using RewardPointsSystem.Application.Services.Events;
using RewardPointsSystem.Application.Services.Accounts;
using RewardPointsSystem.Application.Services.Orchestrators;
using RewardPointsSystem.Application.DTOs.Admin;
using RewardPointsSystem.Infrastructure.Data;
using RewardPointsSystem.Infrastructure.Repositories;
using RewardPointsSystem.Tests.TestHelpers;
using Xunit;

namespace RewardPointsSystem.Tests.IntegrationTests
{
    /// <summary>
    /// Integration Tests for Event Workflows
    /// 
    /// These tests verify complete event lifecycle scenarios:
    /// - Event creation and publishing
    /// - Participant registration
    /// - Event status transitions
    /// - Points awarding after event completion
    /// 
    /// WHAT WE'RE TESTING:
    /// Integration between EventService, EventParticipationService, 
    /// EventRewardOrchestrator, and UserPointsAccountService
    /// 
    /// WHY THESE TESTS MATTER:
    /// Events are the primary way employees earn points. These tests
    /// ensure the complete workflow from event creation to points award.
    /// </summary>
    public class EventWorkflowIntegrationTests : IDisposable
    {
        private readonly RewardPointsDbContext _context;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserService _userService;
        private readonly EventService _eventService;
        private readonly EventParticipationService _participationService;
        private readonly UserPointsAccountService _accountService;
        private readonly UserPointsTransactionService _transactionService;
        private readonly PointsAwardingService _pointsAwardingService;
        private readonly EventRewardOrchestrator _rewardOrchestrator;
        private readonly Mock<IAdminBudgetService> _mockBudgetService;
        private Guid _adminUserId;

        public EventWorkflowIntegrationTests()
        {
            var options = new DbContextOptionsBuilder<RewardPointsDbContext>()
                .UseInMemoryDatabase(databaseName: $"EventWorkflowTests_{Guid.NewGuid()}")
                .Options;

            _context = new RewardPointsDbContext(options);
            _unitOfWork = TestDbContextFactory.CreateInMemoryUnitOfWork();
            
            _userService = new UserService(_unitOfWork);
            _eventService = new EventService(_unitOfWork);
            _participationService = new EventParticipationService(_unitOfWork);
            _accountService = new UserPointsAccountService(_unitOfWork);
            _transactionService = new UserPointsTransactionService(_unitOfWork);
            _mockBudgetService = new Mock<IAdminBudgetService>();
            _mockBudgetService.Setup(x => x.ValidatePointsAwardAsync(It.IsAny<Guid>(), It.IsAny<int>()))
                .ReturnsAsync(new BudgetValidationResult { IsAllowed = true });
            _pointsAwardingService = new PointsAwardingService(_unitOfWork, _mockBudgetService.Object);
            _rewardOrchestrator = new EventRewardOrchestrator(
                _eventService,
                _participationService,
                _pointsAwardingService,
                _accountService,
                _transactionService);

            InitializeAdminUserAsync().Wait();
        }

        private async Task InitializeAdminUserAsync()
        {
            var admin = await _userService.CreateUserAsync("admin@company.com", "Admin", "User");
            _adminUserId = admin.Id;
        }

        public void Dispose()
        {
            _context?.Dispose();
        }

        #region Event Lifecycle Tests

        /// <summary>
        /// SCENARIO: Admin creates a new event and publishes it for employees
        /// WORKFLOW: Create event (Draft) → Publish (Upcoming) → Verify visible
        /// EXPECTED: Event starts as Draft, becomes Upcoming when published
        /// WHY: Events go through a review process before being visible to employees
        /// </summary>
        [Fact]
        public async Task CreateAndPublishEvent_ShouldTransitionFromDraftToUpcoming()
        {
            // Step 1: Create event (starts as Draft)
            var eventEntity = await _eventService.CreateEventAsync(
                "Sales Competition 2024",
                "Annual sales competition with prizes",
                DateTime.UtcNow.AddDays(30),
                5000);

            eventEntity.Should().NotBeNull("event should be created");
            eventEntity.Status.Should().Be(EventStatus.Draft, "new events start as Draft");

            // Step 2: Publish the event
            await _eventService.PublishEventAsync(eventEntity.Id);

            // Verify: Event is now Upcoming
            var publishedEvent = await _eventService.GetEventByIdAsync(eventEntity.Id);
            publishedEvent.Status.Should().Be(EventStatus.Upcoming, "published events should be Upcoming");
        }

        /// <summary>
        /// SCENARIO: Complete event lifecycle from creation to completion
        /// WORKFLOW: Create → Publish → Activate → Complete
        /// EXPECTED: Event transitions through all statuses correctly
        /// WHY: Events follow a defined lifecycle for proper management
        /// </summary>
        [Fact]
        public async Task EventLifecycle_ShouldTransitionThroughAllStatuses()
        {
            // Create event
            var eventEntity = await _eventService.CreateEventAsync(
                "Team Building Event",
                "Fun team activities",
                DateTime.UtcNow.AddDays(7),
                2000);

            eventEntity.Status.Should().Be(EventStatus.Draft);

            // Publish
            await _eventService.PublishEventAsync(eventEntity.Id);
            var afterPublish = await _eventService.GetEventByIdAsync(eventEntity.Id);
            afterPublish.Status.Should().Be(EventStatus.Upcoming);

            // Activate
            await _eventService.ActivateEventAsync(eventEntity.Id);
            var afterActivate = await _eventService.GetEventByIdAsync(eventEntity.Id);
            afterActivate.Status.Should().Be(EventStatus.Active);

            // Complete
            await _eventService.CompleteEventAsync(eventEntity.Id);
            var afterComplete = await _eventService.GetEventByIdAsync(eventEntity.Id);
            afterComplete.Status.Should().Be(EventStatus.Completed);
        }

        #endregion

        #region Participant Registration Tests

        /// <summary>
        /// SCENARIO: Employee registers for an upcoming event
        /// WORKFLOW: Create employee → Create event → Publish → Register
        /// EXPECTED: Employee is successfully registered and can participate
        /// WHY: Employees need to register before participating in events
        /// </summary>
        [Fact]
        public async Task EmployeeRegistration_ShouldRegisterForUpcomingEvent()
        {
            // Setup: Create employee and event
            var employee = await _userService.CreateUserAsync("employee@company.com", "Test", "Employee");
            await _accountService.CreateAccountAsync(employee.Id);

            var eventEntity = await _eventService.CreateEventAsync(
                "Quarterly Review",
                "Q1 Performance Review Event",
                DateTime.UtcNow.AddDays(14),
                1000);
            await _eventService.PublishEventAsync(eventEntity.Id);

            // Act: Register for the event
            await _participationService.RegisterParticipantAsync(eventEntity.Id, employee.Id);

            // Verify: Employee is registered
            var isRegistered = await _participationService.IsUserRegisteredAsync(eventEntity.Id, employee.Id);
            isRegistered.Should().BeTrue("employee should be registered");

            var participants = await _participationService.GetEventParticipantsAsync(eventEntity.Id);
            participants.Should().HaveCount(1, "event should have one participant");
        }

        /// <summary>
        /// SCENARIO: Multiple employees register for the same event
        /// WORKFLOW: Create multiple employees → Register all for event
        /// EXPECTED: All employees are registered and counted as participants
        /// WHY: Events typically have multiple participants
        /// </summary>
        [Fact]
        public async Task MultipleEmployees_ShouldRegisterForSameEvent()
        {
            // Setup: Create multiple employees
            var employee1 = await _userService.CreateUserAsync("emp1@company.com", "First", "Employee");
            var employee2 = await _userService.CreateUserAsync("emp2@company.com", "Second", "Employee");
            var employee3 = await _userService.CreateUserAsync("emp3@company.com", "Third", "Employee");

            foreach (var emp in new[] { employee1, employee2, employee3 })
            {
                await _accountService.CreateAccountAsync(emp.Id);
            }

            var eventEntity = await _eventService.CreateEventAsync(
                "Company Meeting",
                "All-hands meeting",
                DateTime.UtcNow.AddDays(7),
                3000);
            await _eventService.PublishEventAsync(eventEntity.Id);

            // Act: Register all employees
            await _participationService.RegisterParticipantAsync(eventEntity.Id, employee1.Id);
            await _participationService.RegisterParticipantAsync(eventEntity.Id, employee2.Id);
            await _participationService.RegisterParticipantAsync(eventEntity.Id, employee3.Id);

            // Verify: All are registered
            var participants = await _participationService.GetEventParticipantsAsync(eventEntity.Id);
            participants.Should().HaveCount(3, "all three employees should be registered");
        }

        #endregion

        #region Points Award Workflow Tests

        /// <summary>
        /// SCENARIO: Award points to event winners after event completion
        /// WORKFLOW: Complete event → Award points to winner → Verify points credited
        /// EXPECTED: Winner's account is credited with award points
        /// WHY: This is the core reward mechanism for employee recognition
        /// </summary>
        [Fact]
        public async Task AwardPointsToWinner_ShouldCreditParticipantAccount()
        {
            // Setup: Create employee, event, and register
            var winner = await _userService.CreateUserAsync("winner@company.com", "Contest", "Winner");
            await _accountService.CreateAccountAsync(winner.Id);
            var initialBalance = await _accountService.GetBalanceAsync(winner.Id);

            var eventEntity = await _eventService.CreateEventAsync(
                "Innovation Contest",
                "Best new idea wins!",
                DateTime.UtcNow.AddDays(1),
                2000);
            await _eventService.PublishEventAsync(eventEntity.Id);
            await _participationService.RegisterParticipantAsync(eventEntity.Id, winner.Id);
            await _eventService.ActivateEventAsync(eventEntity.Id);
            await _eventService.CompleteEventAsync(eventEntity.Id);

            // Act: Award points to the winner
            var result = await _rewardOrchestrator.ProcessEventRewardAsync(
                eventEntity.Id,
                winner.Id,
                1000,   // points
                1,      // position
                _adminUserId);

            // Verify: Winner received points
            result.Success.Should().BeTrue("award should succeed");
            
            var newBalance = await _accountService.GetBalanceAsync(winner.Id);
            newBalance.Should().Be(initialBalance + 1000, "winner should receive 1000 points");
        }

        /// <summary>
        /// SCENARIO: Award points to multiple participants with different ranks
        /// WORKFLOW: Complete event → Award 1st, 2nd, 3rd place
        /// EXPECTED: Each participant receives points based on their rank
        /// WHY: Events often have tiered rewards for different placements
        /// </summary>
        [Fact]
        public async Task AwardPointsToMultipleParticipants_ShouldCreditBasedOnRank()
        {
            // Setup: Create three employees
            var first = await _userService.CreateUserAsync("first@company.com", "First", "Place");
            var second = await _userService.CreateUserAsync("second@company.com", "Second", "Place");
            var third = await _userService.CreateUserAsync("third@company.com", "Third", "Place");

            foreach (var emp in new[] { first, second, third })
            {
                await _accountService.CreateAccountAsync(emp.Id);
            }

            // Setup: Create and complete event
            var eventEntity = await _eventService.CreateEventAsync(
                "Sales Competition",
                "Top 3 get prizes",
                DateTime.UtcNow.AddDays(1),
                3000);
            await _eventService.PublishEventAsync(eventEntity.Id);
            
            foreach (var emp in new[] { first, second, third })
            {
                await _participationService.RegisterParticipantAsync(eventEntity.Id, emp.Id);
            }
            
            await _eventService.ActivateEventAsync(eventEntity.Id);
            await _eventService.CompleteEventAsync(eventEntity.Id);

            // Act: Award points based on ranking
            await _rewardOrchestrator.ProcessEventRewardAsync(eventEntity.Id, first.Id, 1000, 1, _adminUserId);
            await _rewardOrchestrator.ProcessEventRewardAsync(eventEntity.Id, second.Id, 500, 2, _adminUserId);
            await _rewardOrchestrator.ProcessEventRewardAsync(eventEntity.Id, third.Id, 250, 3, _adminUserId);

            // Verify: Each received correct points
            (await _accountService.GetBalanceAsync(first.Id)).Should().Be(1000, "1st place: 1000 points");
            (await _accountService.GetBalanceAsync(second.Id)).Should().Be(500, "2nd place: 500 points");
            (await _accountService.GetBalanceAsync(third.Id)).Should().Be(250, "3rd place: 250 points");
        }

        #endregion
    }
}
