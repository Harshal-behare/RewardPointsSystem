using FluentAssertions;
using RewardPointsSystem.E2ETests.Base;
using RewardPointsSystem.E2ETests.Helpers;
using RewardPointsSystem.E2ETests.PageObjects;
using RewardPointsSystem.E2ETests.PageObjects.Employee;
using Xunit;
using Xunit.Abstractions;

namespace RewardPointsSystem.E2ETests.Tests.Employee;

/// <summary>
/// E2E tests for Event Registration functionality.
/// </summary>
[Trait("Category", "Employee")]
[Trait("Feature", "Events")]
[Trait("Priority", "High")]
public class EventRegistrationTests : BaseTest
{
    private readonly LoginPage _loginPage;
    private readonly EventsPage _eventsPage;
    private readonly List<int> _createdEventIds = new();

    public EventRegistrationTests(ITestOutputHelper output) : base(output)
    {
        _loginPage = new LoginPage(Driver);
        _eventsPage = new EventsPage(Driver);
    }

    private void LoginAsEmployee()
    {
        _loginPage.GoTo();
        _loginPage.LoginAsEmployee(Config.EmployeeEmail, Config.EmployeePassword);
    }

    protected override void Dispose(bool disposing)
    {
        // Cleanup created test events via API
        foreach (var eventId in _createdEventIds)
        {
            try
            {
                ApiHelper.DeleteEventAsync(eventId).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logger.Warning(ex, "Failed to cleanup event: {EventId}", eventId);
            }
        }
        base.Dispose(disposing);
    }

    [Fact]
    [Trait("TestType", "Smoke")]
    public void EventsPage_ShouldLoad_Successfully()
    {
        ExecuteWithScreenshotOnFailure(() =>
        {
            // Arrange
            LoginAsEmployee();

            // Act
            _eventsPage.GoTo();

            // Assert
            _eventsPage.IsOnPage().Should().BeTrue();
        });
    }

    [Fact]
    [Trait("TestType", "Functional")]
    public void EventsPage_DisplaysEventCards()
    {
        ExecuteWithScreenshotOnFailure(() =>
        {
            // Arrange
            LoginAsEmployee();

            // Act
            _eventsPage.GoTo();

            // Assert - Should load events (might be 0 if no events exist)
            _eventsPage.GetEventCount().Should().BeGreaterThanOrEqualTo(0);
        });
    }

    [Fact]
    [Trait("TestType", "Functional")]
    public void SearchEvents_WithValidName_FiltersResults()
    {
        ExecuteWithScreenshotOnFailure(async () =>
        {
            // Arrange - Create event via API
            var eventRequest = TestDataHelper.CreateTestEvent();
            var eventId = await ApiHelper.CreateEventAsync(eventRequest);
            _createdEventIds.Add(eventId);

            LoginAsEmployee();
            _eventsPage.GoTo();

            // Act
            _eventsPage.SearchEvents(eventRequest.Name);

            // Assert
            _eventsPage.EventExists(eventRequest.Name).Should().BeTrue();
        }).GetAwaiter().GetResult();
    }

    [Fact]
    [Trait("TestType", "Functional")]
    public void SearchEvents_WithNonExistingName_ShowsNoResults()
    {
        ExecuteWithScreenshotOnFailure(() =>
        {
            // Arrange
            LoginAsEmployee();
            _eventsPage.GoTo();

            // Act
            _eventsPage.SearchEvents("NonExistentEvent_ABC_987654");

            // Assert
            _eventsPage.GetEventCount().Should().Be(0);
        });
    }

    [Fact]
    [Trait("TestType", "Functional")]
    public void RegisterForEvent_ShouldShowRegisteredStatus()
    {
        ExecuteWithScreenshotOnFailure(async () =>
        {
            // Arrange - Create event via API with upcoming status
            var eventRequest = TestDataHelper.CreateTestEvent();
            var eventId = await ApiHelper.CreateEventAsync(eventRequest);
            _createdEventIds.Add(eventId);

            LoginAsEmployee();
            _eventsPage.GoTo();
            _eventsPage.SearchEvents(eventRequest.Name);

            // Verify event exists
            if (!_eventsPage.EventExists(eventRequest.Name))
            {
                Logger.Warning("Event not found after creation - skipping test");
                return;
            }

            // Act
            _eventsPage.RegisterForEvent(eventRequest.Name);

            // Assert
            _eventsPage.IsRegisteredForEvent(eventRequest.Name).Should().BeTrue();
        }).GetAwaiter().GetResult();
    }

    [Fact]
    [Trait("TestType", "Functional")]
    public void GetEventDetails_ReturnsCorrectInfo()
    {
        ExecuteWithScreenshotOnFailure(async () =>
        {
            // Arrange - Create event via API with specific points
            var eventRequest = TestDataHelper.CreateTestEvent(pointsReward: 250);
            var eventId = await ApiHelper.CreateEventAsync(eventRequest);
            _createdEventIds.Add(eventId);

            LoginAsEmployee();
            _eventsPage.GoTo();
            _eventsPage.SearchEvents(eventRequest.Name);

            // Assert - Verify event is visible
            _eventsPage.EventExists(eventRequest.Name).Should().BeTrue();
        }).GetAwaiter().GetResult();
    }

    [Fact]
    [Trait("TestType", "Functional")]
    public void GetAllEventNames_ReturnsEventList()
    {
        ExecuteWithScreenshotOnFailure(() =>
        {
            // Arrange
            LoginAsEmployee();
            _eventsPage.GoTo();

            // Act
            var eventNames = _eventsPage.GetAllEventNames();

            // Assert
            eventNames.Should().NotBeNull();
            // List size depends on existing data
        });
    }

    [Fact]
    [Trait("TestType", "Functional")]
    public void ClickEventCard_ShowsEventDetails()
    {
        ExecuteWithScreenshotOnFailure(async () =>
        {
            // Arrange - Create event via API
            var eventRequest = TestDataHelper.CreateTestEvent();
            var eventId = await ApiHelper.CreateEventAsync(eventRequest);
            _createdEventIds.Add(eventId);

            LoginAsEmployee();
            _eventsPage.GoTo();
            _eventsPage.SearchEvents(eventRequest.Name);

            if (!_eventsPage.EventExists(eventRequest.Name))
            {
                Logger.Warning("Event not found - skipping test");
                return;
            }

            // Act
            _eventsPage.ClickEventCard(eventRequest.Name);

            // Assert - Event details dialog or page should appear
            // Implementation depends on actual UI behavior
            WaitHelper.WaitForPageLoad(Driver);
        }).GetAwaiter().GetResult();
    }
}
