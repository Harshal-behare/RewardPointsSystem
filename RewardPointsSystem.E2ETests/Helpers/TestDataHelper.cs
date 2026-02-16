namespace RewardPointsSystem.E2ETests.Helpers;

/// <summary>
/// Helper for generating consistent and unique test data.
/// Ensures test isolation and repeatability.
/// </summary>
public static class TestDataHelper
{
    private static readonly Random Random = new();
    
    /// <summary>
    /// Generates a unique test identifier.
    /// </summary>
    public static string UniqueId() => Guid.NewGuid().ToString("N")[..8];

    /// <summary>
    /// Generates a unique email for testing.
    /// </summary>
    public static string GenerateEmail(string prefix = "test")
        => $"{prefix}_{UniqueId()}@agdata.com";

    /// <summary>
    /// Generates a valid test password.
    /// </summary>
    public static string GeneratePassword()
        => $"Test@{Random.Next(1000, 9999)}Pass!";

    /// <summary>
    /// Generates a unique event name.
    /// </summary>
    public static string GenerateEventName()
        => $"Test Event {UniqueId()}";

    /// <summary>
    /// Generates a unique product name.
    /// </summary>
    public static string GenerateProductName()
        => $"Test Product {UniqueId()}";

    /// <summary>
    /// Creates a test event request with default values.
    /// </summary>
    public static CreateEventRequest CreateTestEvent(
        string? name = null,
        int pointsReward = 100,
        int maxParticipants = 50)
    {
        return new CreateEventRequest
        {
            Name = name ?? GenerateEventName(),
            Description = $"Test event created at {DateTime.UtcNow:O}",
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(7),
            PointsReward = pointsReward,
            MaxParticipants = maxParticipants
        };
    }

    /// <summary>
    /// Creates a test product request with default values.
    /// </summary>
    public static CreateProductRequest CreateTestProduct(
        string? name = null,
        int pointsCost = 500,
        int stock = 100)
    {
        return new CreateProductRequest
        {
            Name = name ?? GenerateProductName(),
            Description = $"Test product created at {DateTime.UtcNow:O}",
            PointsCost = pointsCost,
            Stock = stock
        };
    }

    /// <summary>
    /// Creates registration data for testing.
    /// </summary>
    public static (string Email, string Password, string FirstName, string LastName) CreateRegistrationData()
    {
        var id = UniqueId();
        return (
            Email: $"user_{id}@agdata.com",
            Password: GeneratePassword(),
            FirstName: $"Test{id}",
            LastName: "User"
        );
    }

    /// <summary>
    /// Generates a list of test items.
    /// </summary>
    public static List<T> GenerateList<T>(Func<int, T> generator, int count)
    {
        return Enumerable.Range(0, count).Select(generator).ToList();
    }

    /// <summary>
    /// Generates future dates for events.
    /// </summary>
    public static (DateTime Start, DateTime End) GenerateFutureDateRange(int daysFromNow = 7, int duration = 3)
    {
        var start = DateTime.UtcNow.Date.AddDays(daysFromNow);
        var end = start.AddDays(duration);
        return (start, end);
    }

    /// <summary>
    /// Generates past dates for testing history.
    /// </summary>
    public static (DateTime Start, DateTime End) GeneratePastDateRange(int daysAgo = 30, int duration = 3)
    {
        var start = DateTime.UtcNow.Date.AddDays(-daysAgo);
        var end = start.AddDays(duration);
        return (start, end);
    }
}
