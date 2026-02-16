using RestSharp;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RewardPointsSystem.E2ETests.Helpers;

/// <summary>
/// Helper for setting up test data via API calls.
/// Avoids using UI for data setup to reduce flakiness and improve test speed.
/// </summary>
public class ApiSetupHelper
{
    private readonly RestClient _client;
    private string? _adminToken;
    private string? _employeeToken;
    
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public ApiSetupHelper(string apiBaseUrl)
    {
        _client = new RestClient(apiBaseUrl);
    }

    /// <summary>
    /// Authenticates as admin and caches the token.
    /// </summary>
    public async Task<string> GetAdminTokenAsync()
    {
        if (!string.IsNullOrEmpty(_adminToken))
            return _adminToken;

        var config = Base.TestConfiguration.Instance;
        _adminToken = await LoginAsync(config.AdminEmail, config.AdminPassword);
        return _adminToken;
    }

    /// <summary>
    /// Authenticates as employee and caches the token.
    /// </summary>
    public async Task<string> GetEmployeeTokenAsync()
    {
        if (!string.IsNullOrEmpty(_employeeToken))
            return _employeeToken;

        var config = Base.TestConfiguration.Instance;
        _employeeToken = await LoginAsync(config.EmployeeEmail, config.EmployeePassword);
        return _employeeToken;
    }

    /// <summary>
    /// Checks if the API backend is reachable.
    /// </summary>
    public async Task<bool> IsApiAvailableAsync()
    {
        try
        {
            var request = new RestRequest("", Method.Get);
            request.Timeout = TimeSpan.FromSeconds(5);
            var response = await _client.ExecuteAsync(request);
            return (int)response.StatusCode > 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Logs in and returns the access token.
    /// </summary>
    public async Task<string> LoginAsync(string email, string password)
    {
        var request = new RestRequest("auth/login", Method.Post);
        request.AddJsonBody(new { email, password });

        var response = await _client.ExecuteAsync(request);
        
        if ((int)response.StatusCode == 0)
            throw new ApiNotAvailableException("API backend is not reachable. Start the backend before running API-dependent tests.");
        
        if (!response.IsSuccessful)
            throw new InvalidOperationException($"Login failed: {response.StatusCode} - {response.Content}");

        var result = JsonSerializer.Deserialize<ApiResponse<LoginResponse>>(response.Content!, JsonOptions);
        return result?.Data?.AccessToken ?? throw new InvalidOperationException("No access token in response");
    }

    /// <summary>
    /// Creates a test event via API.
    /// </summary>
    public async Task<int> CreateEventAsync(CreateEventRequest request)
    {
        var token = await GetAdminTokenAsync();
        var apiRequest = new RestRequest("events", Method.Post);
        apiRequest.AddHeader("Authorization", $"Bearer {token}");
        apiRequest.AddJsonBody(request);

        var response = await _client.ExecuteAsync(apiRequest);
        
        if (!response.IsSuccessful)
            throw new InvalidOperationException($"Create event failed: {response.StatusCode} - {response.Content}");

        var result = JsonSerializer.Deserialize<ApiResponse<EventResponse>>(response.Content!, JsonOptions);
        return result?.Data?.Id ?? throw new InvalidOperationException("No event ID in response");
    }

    /// <summary>
    /// Creates a test product via API.
    /// </summary>
    public async Task<int> CreateProductAsync(CreateProductRequest request)
    {
        var token = await GetAdminTokenAsync();
        var apiRequest = new RestRequest("products", Method.Post);
        apiRequest.AddHeader("Authorization", $"Bearer {token}");
        apiRequest.AddJsonBody(request);

        var response = await _client.ExecuteAsync(apiRequest);
        
        if (!response.IsSuccessful)
            throw new InvalidOperationException($"Create product failed: {response.StatusCode} - {response.Content}");

        var result = JsonSerializer.Deserialize<ApiResponse<ProductResponse>>(response.Content!, JsonOptions);
        return result?.Data?.Id ?? throw new InvalidOperationException("No product ID in response");
    }

    /// <summary>
    /// Awards points to a user via API.
    /// </summary>
    public async Task AwardPointsAsync(int userId, int points, string reason)
    {
        var token = await GetAdminTokenAsync();
        var apiRequest = new RestRequest("points/award", Method.Post);
        apiRequest.AddHeader("Authorization", $"Bearer {token}");
        apiRequest.AddJsonBody(new { userId, points, reason });

        var response = await _client.ExecuteAsync(apiRequest);
        
        if (!response.IsSuccessful)
            throw new InvalidOperationException($"Award points failed: {response.StatusCode} - {response.Content}");
    }

    /// <summary>
    /// Gets current user's points balance.
    /// </summary>
    public async Task<int> GetUserPointsAsync(string token)
    {
        var apiRequest = new RestRequest("points/balance", Method.Get);
        apiRequest.AddHeader("Authorization", $"Bearer {token}");

        var response = await _client.ExecuteAsync(apiRequest);
        
        if (!response.IsSuccessful)
            throw new InvalidOperationException($"Get points failed: {response.StatusCode} - {response.Content}");

        var result = JsonSerializer.Deserialize<ApiResponse<PointsBalanceResponse>>(response.Content!, JsonOptions);
        return result?.Data?.Balance ?? 0;
    }

    /// <summary>
    /// Deletes a test event via API.
    /// </summary>
    public async Task DeleteEventAsync(int eventId)
    {
        var token = await GetAdminTokenAsync();
        var apiRequest = new RestRequest($"events/{eventId}", Method.Delete);
        apiRequest.AddHeader("Authorization", $"Bearer {token}");

        await _client.ExecuteAsync(apiRequest);
    }

    /// <summary>
    /// Deletes a test product via API.
    /// </summary>
    public async Task DeleteProductAsync(int productId)
    {
        var token = await GetAdminTokenAsync();
        var apiRequest = new RestRequest($"products/{productId}", Method.Delete);
        apiRequest.AddHeader("Authorization", $"Bearer {token}");

        await _client.ExecuteAsync(apiRequest);
    }

    // Response DTOs
    private class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
    }

    private class LoginResponse
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
    }

    private class EventResponse
    {
        public int Id { get; set; }
    }

    private class ProductResponse
    {
        public int Id { get; set; }
    }

    private class PointsBalanceResponse
    {
        public int Balance { get; set; }
    }
}

/// <summary>
/// Exception thrown when the API backend is not available.
/// Tests catching this should pass gracefully (skip the API-dependent portion).
/// </summary>
public class ApiNotAvailableException : Exception
{
    public ApiNotAvailableException(string message) : base(message) { }
    public ApiNotAvailableException(string message, Exception innerException) : base(message, innerException) { }
}

// Request DTOs for API setup
public class CreateEventRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int PointsReward { get; set; }
    public int MaxParticipants { get; set; }
}

public class CreateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int PointsCost { get; set; }
    public int Stock { get; set; }
    public string? ImageUrl { get; set; }
}
