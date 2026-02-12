using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using RewardPointsSystem.Api;
using RewardPointsSystem.Application.Interfaces;
using RewardPointsSystem.Domain.Entities.Core;
using RewardPointsSystem.Tests.FunctionalTests;
using Xunit;

namespace RewardPointsSystem.Tests.ApiTests
{
    /// <summary>
    /// Base class for authenticated API tests
    /// 
    /// Provides helper methods for:
    /// - Registering users
    /// - Logging in and obtaining JWT tokens
    /// - Creating authenticated HttpClient instances
    /// 
    /// WHY: Reduces boilerplate in API tests that require authentication
    /// </summary>
    public abstract class AuthenticatedApiTestBase : IClassFixture<CustomWebApplicationFactory<Program>>, IAsyncLifetime
    {
        protected readonly HttpClient _client;
        protected readonly CustomWebApplicationFactory<Program> _factory;
        protected readonly JsonSerializerOptions _jsonOptions;

        // Default test credentials
        protected const string AdminEmail = "admin@test.com";
        protected const string AdminPassword = "Admin123!";
        protected const string EmployeeEmail = "employee@test.com";
        protected const string EmployeePassword = "Employee123!";

        protected string? AdminAccessToken { get; private set; }
        protected string? EmployeeAccessToken { get; private set; }

        protected AuthenticatedApiTestBase(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public virtual async Task InitializeAsync()
        {
            // Seed admin and employee users directly in the database
            await SeedTestUsersAsync();
            
            // Login both users to get tokens
            AdminAccessToken = await LoginAsync(AdminEmail, AdminPassword);
            EmployeeAccessToken = await LoginAsync(EmployeeEmail, EmployeePassword);
        }

        public virtual Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Seeds test users directly in the database
        /// </summary>
        protected virtual async Task SeedTestUsersAsync()
        {
            using var scope = _factory.Services.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            var systemUserId = Guid.NewGuid(); // System user for assigning roles

            // Create Admin role if doesn't exist
            var adminRole = await unitOfWork.Roles.SingleOrDefaultAsync(r => r.Name == "Admin");
            if (adminRole == null)
            {
                adminRole = Role.Create("Admin", "Administrator with full access");
                await unitOfWork.Roles.AddAsync(adminRole);
            }

            // Create Employee role if doesn't exist
            var employeeRole = await unitOfWork.Roles.SingleOrDefaultAsync(r => r.Name == "Employee");
            if (employeeRole == null)
            {
                employeeRole = Role.Create("Employee", "Standard employee user");
                await unitOfWork.Roles.AddAsync(employeeRole);
            }

            // Create Admin user if doesn't exist
            var adminUser = await unitOfWork.Users.SingleOrDefaultAsync(u => u.Email == AdminEmail);
            if (adminUser == null)
            {
                adminUser = User.Create(AdminEmail, "Admin", "User");
                adminUser.SetPasswordHash(passwordHasher.HashPassword(AdminPassword));
                // User.Create already sets IsActive = true, no need to call Activate()
                await unitOfWork.Users.AddAsync(adminUser);
                await unitOfWork.SaveChangesAsync();

                // Assign admin role
                var adminUserRole = UserRole.Assign(adminUser.Id, adminRole.Id, systemUserId);
                await unitOfWork.UserRoles.AddAsync(adminUserRole);
            }

            // Create Employee user if doesn't exist
            var employeeUser = await unitOfWork.Users.SingleOrDefaultAsync(u => u.Email == EmployeeEmail);
            if (employeeUser == null)
            {
                employeeUser = User.Create(EmployeeEmail, "Employee", "User");
                employeeUser.SetPasswordHash(passwordHasher.HashPassword(EmployeePassword));
                // User.Create already sets IsActive = true, no need to call Activate()
                await unitOfWork.Users.AddAsync(employeeUser);
                await unitOfWork.SaveChangesAsync();

                // Assign employee role
                var employeeUserRole = UserRole.Assign(employeeUser.Id, employeeRole.Id, systemUserId);
                await unitOfWork.UserRoles.AddAsync(employeeUserRole);
            }

            await unitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// Hashes a password using the actual password hasher service
        /// </summary>
        private string HashPasswordWithService(string password)
        {
            using var scope = _factory.Services.CreateScope();
            var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            return passwordHasher.HashPassword(password);
        }

        /// <summary>
        /// Login and return access token
        /// </summary>
        protected async Task<string?> LoginAsync(string email, string password)
        {
            var loginRequest = new
            {
                email = email,
                password = password
            };

            var content = new StringContent(
                JsonSerializer.Serialize(loginRequest, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            var response = await _client.PostAsync("/api/v1/auth/login", content);
            
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            // API returns ApiResponse<LoginResponseDto>, need to unwrap the data
            var apiResponse = JsonSerializer.Deserialize<ApiResponseWrapper<LoginResponse>>(responseContent, _jsonOptions);
            return apiResponse?.Data?.AccessToken;
        }

        /// <summary>
        /// Register a new user via API and return the response
        /// </summary>
        protected async Task<HttpResponseMessage> RegisterUserAsync(
            string firstName,
            string lastName,
            string email,
            string password)
        {
            var registerRequest = new
            {
                firstName = firstName,
                lastName = lastName,
                email = email,
                password = password,
                confirmPassword = password
            };

            var content = new StringContent(
                JsonSerializer.Serialize(registerRequest, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            return await _client.PostAsync("/api/v1/auth/register", content);
        }

        /// <summary>
        /// Creates an HttpClient with the admin's authentication token
        /// </summary>
        protected HttpClient CreateAdminClient()
        {
            var client = _factory.CreateClient();
            if (!string.IsNullOrEmpty(AdminAccessToken))
            {
                client.DefaultRequestHeaders.Authorization = 
                    new AuthenticationHeaderValue("Bearer", AdminAccessToken);
            }
            return client;
        }

        /// <summary>
        /// Creates an HttpClient with the employee's authentication token
        /// </summary>
        protected HttpClient CreateEmployeeClient()
        {
            var client = _factory.CreateClient();
            if (!string.IsNullOrEmpty(EmployeeAccessToken))
            {
                client.DefaultRequestHeaders.Authorization = 
                    new AuthenticationHeaderValue("Bearer", EmployeeAccessToken);
            }
            return client;
        }

        /// <summary>
        /// Creates an HttpClient with a custom authentication token
        /// </summary>
        protected HttpClient CreateAuthenticatedClient(string accessToken)
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", accessToken);
            return client;
        }

        /// <summary>
        /// Creates an unauthenticated HttpClient
        /// </summary>
        protected HttpClient CreateAnonymousClient()
        {
            return _factory.CreateClient();
        }

        /// <summary>
        /// Helper to serialize object to JSON StringContent
        /// </summary>
        protected StringContent CreateJsonContent(object data)
        {
            return new StringContent(
                JsonSerializer.Serialize(data, _jsonOptions),
                Encoding.UTF8,
                "application/json");
        }

        /// <summary>
        /// Helper to deserialize HTTP response content
        /// </summary>
        protected async Task<T?> DeserializeResponseAsync<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(content, _jsonOptions);
        }

        /// <summary>
        /// API response wrapper class matching the server's ApiResponse<T> format
        /// </summary>
        private class ApiResponseWrapper<T>
        {
            public bool Success { get; set; }
            public string? Message { get; set; }
            public T? Data { get; set; }
            public object? Errors { get; set; }
        }

        /// <summary>
        /// Helper response class for login
        /// </summary>
        private class LoginResponse
        {
            public Guid UserId { get; set; }
            public string? Email { get; set; }
            public string? FirstName { get; set; }
            public string? LastName { get; set; }
            public string? AccessToken { get; set; }
            public string? RefreshToken { get; set; }
            public DateTime ExpiresAt { get; set; }
            public string[]? Roles { get; set; }
        }
    }
}
