using Microsoft.Extensions.Configuration;

namespace RewardPointsSystem.E2ETests.Base;

/// <summary>
/// Centralized configuration management for E2E tests.
/// Supports environment-specific configuration and environment variables.
/// </summary>
public sealed class TestConfiguration
{
    private static readonly Lazy<TestConfiguration> _instance = new(() => new TestConfiguration());
    private readonly IConfiguration _configuration;

    public static TestConfiguration Instance => _instance.Value;

    private TestConfiguration()
    {
        var environment = Environment.GetEnvironmentVariable("TEST_ENVIRONMENT") ?? "e2e";
        
        _configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.e2e.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables(prefix: "E2E_")
            .Build();
    }

    // Test Settings
    public string BaseUrl => GetValue("TestSettings:BaseUrl", "http://localhost:4200");
    public string ApiBaseUrl => GetValue("TestSettings:ApiBaseUrl", "http://localhost:5153/api");
    public string Browser => GetValue("TestSettings:Browser", "Chrome");
    public bool Headless => GetBoolValue("TestSettings:Headless", true);
    public int ImplicitWaitSeconds => GetIntValue("TestSettings:ImplicitWaitSeconds", 10);
    public int ExplicitWaitSeconds => GetIntValue("TestSettings:ExplicitWaitSeconds", 30);
    public int PageLoadTimeoutSeconds => GetIntValue("TestSettings:PageLoadTimeoutSeconds", 60);
    public bool ScreenshotOnFailure => GetBoolValue("TestSettings:ScreenshotOnFailure", true);
    public string ScreenshotPath => GetValue("TestSettings:ScreenshotPath", "Screenshots");
    public string LogPath => GetValue("TestSettings:LogPath", "Logs");
    public int RetryCount => GetIntValue("TestSettings:RetryCount", 2);
    public bool ParallelExecution => GetBoolValue("TestSettings:ParallelExecution", true);
    public int MaxParallelThreads => GetIntValue("TestSettings:MaxParallelThreads", 4);

    // Test Credentials (prefer environment variables for CI/CD)
    public string AdminEmail => GetSecureValue("TestCredentials:AdminEmail", "E2E_ADMIN_EMAIL");
    public string AdminPassword => GetSecureValue("TestCredentials:AdminPassword", "E2E_ADMIN_PASSWORD");
    public string EmployeeEmail => GetSecureValue("TestCredentials:EmployeeEmail", "E2E_EMPLOYEE_EMAIL");
    public string EmployeePassword => GetSecureValue("TestCredentials:EmployeePassword", "E2E_EMPLOYEE_PASSWORD");

    // Database Settings
    public string ConnectionString => GetSecureValue("DatabaseSettings:ConnectionString", "E2E_CONNECTION_STRING");

    private string GetValue(string key, string defaultValue)
        => _configuration[key] ?? defaultValue;

    private int GetIntValue(string key, int defaultValue)
        => int.TryParse(_configuration[key], out var value) ? value : defaultValue;

    private bool GetBoolValue(string key, bool defaultValue)
        => bool.TryParse(_configuration[key], out var value) ? value : defaultValue;

    private string GetSecureValue(string configKey, string envKey)
    {
        // Prefer environment variable for sensitive data
        var envValue = Environment.GetEnvironmentVariable(envKey);
        if (!string.IsNullOrEmpty(envValue))
            return envValue;

        return _configuration[configKey] ?? string.Empty;
    }
}
