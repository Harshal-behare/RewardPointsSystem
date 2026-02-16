using FluentAssertions;
using RewardPointsSystem.E2ETests.Base;
using RewardPointsSystem.E2ETests.Helpers;
using RewardPointsSystem.E2ETests.PageObjects;
using RewardPointsSystem.E2ETests.PageObjects.Admin;
using Xunit;
using Xunit.Abstractions;

namespace RewardPointsSystem.E2ETests.Tests.Admin;

/// <summary>
/// E2E tests for Event Management functionality.
/// Tests follow AAA pattern and are independent.
/// </summary>
[Trait("Category", "Admin")]
[Trait("Feature", "Events")]
[Trait("Priority", "High")]
public class EventManagementTests : BaseTest
{
    private readonly LoginPage _loginPage;
    private readonly EventsManagementPage _eventsPage;
    private readonly List<int> _createdEventIds = new();

    public EventManagementTests(ITestOutputHelper output) : base(output)
    {
        _loginPage = new LoginPage(Driver);
        _eventsPage = new EventsManagementPage(Driver);
    }

    private void LoginAsAdmin()
    {
        _loginPage.GoTo();
        _loginPage.LoginAsAdmin(Config.AdminEmail, Config.AdminPassword);
    }

    protected override void Dispose(bool disposing)
    {
        // Cleanup created test events via API
        foreach (var eventId in _createdEventIds)
        {
            try
            {
                ApiHelper.DeleteEventAsync(eventId).GetAwaiter().GetResult();
                Logger.Information("Cleaned up test event: {EventId}", eventId);
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
            LoginAsAdmin();

            // Act
            _eventsPage.GoTo();

            // Assert
            _eventsPage.IsOnPage().Should().BeTrue();
        });
    }

    [Fact]
    [Trait("TestType", "Functional")]
    public void CreateEvent_WithValidData_ShouldCreateEvent()
    {
        ExecuteWithScreenshotOnFailure(() =>
        {
            // Arrange
            LoginAsAdmin();
            _eventsPage.GoTo();
            
            var eventName = TestDataHelper.GenerateEventName();
            var (startDate, endDate) = TestDataHelper.GenerateFutureDateRange();

            // Act
            _eventsPage.CreateEvent(
                name: eventName,
                description: "E2E Test Event Description",
                startDate: startDate,
                endDate: endDate,
                points: 150
            );

            // Assert
            WaitHelper.WaitForElementToDisappear(Driver, 
                OpenQA.Selenium.By.CssSelector(".modal, .dialog"));
            
            // Search for the created event
            _eventsPage.SearchEvents(eventName);
            _eventsPage.EventExists(eventName).Should().BeTrue($"Event '{eventName}' should exist after creation");
        });
    }

    [Fact]
    [Trait("TestType", "Functional")]
    public void CreateEvent_ClickCancel_ShouldNotCreateEvent()
    {
        ExecuteWithScreenshotOnFailure(() =>
        {
            // Arrange
            LoginAsAdmin();
            _eventsPage.GoTo();
            var initialCount = _eventsPage.GetEventCount();

            // Act
            _eventsPage.ClickCreateEvent();
            _eventsPage.IsModalDisplayed().Should().BeTrue();
            _eventsPage.ClickCancel();

            // Assert
            _eventsPage.IsModalDisplayed().Should().BeFalse();
            _eventsPage.GetEventCount().Should().Be(initialCount);
        });
    }

    [Fact]
    [Trait("TestType", "Functional")]
    public void SearchEvents_WithExistingName_FiltersList()
    {
        ExecuteWithScreenshotOnFailure(async () =>
        {
            // Arrange - Create event via API for reliability
            var eventRequest = TestDataHelper.CreateTestEvent();
            var eventId = await ApiHelper.CreateEventAsync(eventRequest);
            _createdEventIds.Add(eventId);

            LoginAsAdmin();
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
            LoginAsAdmin();
            _eventsPage.GoTo();

            // Act
            _eventsPage.SearchEvents("NonExistentEvent_XYZ_123456");

            // Assert
            _eventsPage.GetEventCount().Should().Be(0);
        });
    }

    [Fact]
    [Trait("TestType", "Functional")]
    public void EditEvent_UpdateName_ShouldUpdateSuccessfully()
    {
        ExecuteWithScreenshotOnFailure(async () =>
        {
            // Arrange - Create event via API
            var eventRequest = TestDataHelper.CreateTestEvent();
            var eventId = await ApiHelper.CreateEventAsync(eventRequest);
            _createdEventIds.Add(eventId);

            LoginAsAdmin();
            _eventsPage.GoTo();
            _eventsPage.SearchEvents(eventRequest.Name);

            // Act
            var newName = TestDataHelper.GenerateEventName();
            _eventsPage.ClickEditEvent(eventRequest.Name);
            
            // Update the name in the form
            var nameInput = WaitHelper.WaitForElement(Driver, 
                OpenQA.Selenium.By.CssSelector("[data-test='event-name'], input[name='name'], #eventName"));
            nameInput.Clear();
            nameInput.SendKeys(newName);
            
            _eventsPage.ClickSave();

            // Assert
            WaitHelper.WaitForElementToDisappear(Driver, 
                OpenQA.Selenium.By.CssSelector(".modal, .dialog"));
            _eventsPage.SearchEvents(newName);
            _eventsPage.EventExists(newName).Should().BeTrue();
        }).GetAwaiter().GetResult();
    }

    [Fact]
    [Trait("TestType", "Functional")]
    public void DeleteEvent_Confirm_ShouldRemoveEvent()
    {
        ExecuteWithScreenshotOnFailure(async () =>
        {
            // Arrange - Create event via API
            var eventRequest = TestDataHelper.CreateTestEvent();
            var eventId = await ApiHelper.CreateEventAsync(eventRequest);
            // Don't add to cleanup list since we're deleting via UI

            LoginAsAdmin();
            _eventsPage.GoTo();
            _eventsPage.SearchEvents(eventRequest.Name);
            _eventsPage.EventExists(eventRequest.Name).Should().BeTrue();

            // Act
            _eventsPage.ClickDeleteEvent(eventRequest.Name);
            _eventsPage.ConfirmDelete();

            // Assert
            _eventsPage.SearchEvents(eventRequest.Name);
            _eventsPage.EventExists(eventRequest.Name).Should().BeFalse();
        }).GetAwaiter().GetResult();
    }
}
