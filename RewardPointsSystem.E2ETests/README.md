# RewardPointsSystem E2E Tests

Comprehensive Selenium-based end-to-end test suite for the RewardPointsSystem frontend application.

## Features

- **Page Object Model (POM)**: Clean separation between test logic and page interactions
- **Cross-Browser Support**: Chrome, Firefox, Edge
- **CI/CD Ready**: Headless mode, configurable timeouts, screenshot on failure
- **Parallel Execution**: Thread-safe tests with independent WebDriver instances
- **API-Based Data Setup**: Avoid UI for test data creation to reduce flakiness
- **Explicit Waits**: No Thread.Sleep - all waits are explicit and condition-based
- **Comprehensive Logging**: Serilog integration with file and console output
- **Screenshot on Failure**: Automatic screenshot capture for failed tests
- **Environment Configuration**: Support for multiple environments via appsettings

## Project Structure

```
RewardPointsSystem.E2ETests/
├── Base/
│   ├── BaseTest.cs           # Base class for all tests
│   ├── DriverFactory.cs      # WebDriver creation with browser options
│   └── TestConfiguration.cs  # Configuration management
├── PageObjects/
│   ├── BasePage.cs           # Base page object with common methods
│   ├── LoginPage.cs          # Login page interactions
│   ├── Admin/
│   │   ├── AdminDashboardPage.cs
│   │   ├── EventsManagementPage.cs
│   │   ├── ProductsManagementPage.cs
│   │   ├── UsersManagementPage.cs
│   │   └── RedemptionsManagementPage.cs
│   └── Employee/
│       ├── EmployeeDashboardPage.cs
│       ├── EventsPage.cs
│       ├── ProductsCatalogPage.cs
│       └── MyAccountPage.cs
├── Tests/
│   ├── AuthenticationTests.cs
│   ├── Admin/
│   │   ├── AdminDashboardTests.cs
│   │   ├── EventManagementTests.cs
│   │   ├── ProductManagementTests.cs
│   │   ├── UserManagementTests.cs
│   │   └── RedemptionApprovalTests.cs
│   └── Employee/
│       ├── EmployeeDashboardTests.cs
│       ├── EventRegistrationTests.cs
│       ├── ProductRedemptionTests.cs
│       └── ProfileTests.cs
├── Helpers/
│   ├── WaitHelper.cs         # Explicit wait utilities
│   ├── ScreenshotHelper.cs   # Screenshot capture
│   ├── TestDataHelper.cs     # Test data generation
│   └── ApiSetupHelper.cs     # API-based test data setup
├── appsettings.e2e.json      # Test configuration
├── xunit.runner.json         # xUnit runner configuration
└── TestCollections.cs        # Test collection definitions
```

## Prerequisites

- .NET 8.0 SDK
- Chrome, Firefox, or Edge browser
- Running RewardPointsSystem backend API
- Running RewardPointsSystem frontend

## Configuration

### appsettings.e2e.json

```json
{
  "TestSettings": {
    "BaseUrl": "http://localhost:4200",
    "ApiBaseUrl": "http://localhost:5153/api",
    "Browser": "Chrome",
    "Headless": true,
    "ImplicitWaitSeconds": 10,
    "ExplicitWaitSeconds": 30
  },
  "TestCredentials": {
    "AdminEmail": "Harshal.Behare@agdata.com",
    "AdminPassword": "Harshal@123"
  }
}
```

### Environment Variables

For CI/CD, use environment variables with `E2E_` prefix:

```bash
E2E_ADMIN_EMAIL=admin@agdata.com
E2E_ADMIN_PASSWORD=SecurePassword123
E2E_CONNECTION_STRING=Server=...
```

## Running Tests

### Run all tests
```bash
dotnet test
```

### Run specific category
```bash
dotnet test --filter "Category=Authentication"
dotnet test --filter "Category=Admin"
dotnet test --filter "Category=Employee"
```

### Run by priority
```bash
dotnet test --filter "Priority=High"
```

### Run by test type
```bash
dotnet test --filter "TestType=Smoke"
dotnet test --filter "TestType=Functional"
```

### Run in specific browser
```bash
# Set environment variable
$env:E2E_BROWSER="Firefox"
dotnet test
```

### Run with visible browser (non-headless)
```bash
$env:E2E_HEADLESS="false"
dotnet test
```

### Run specific test
```bash
dotnet test --filter "FullyQualifiedName~LoginPage_ShouldLoad_Successfully"
```

## Test Categories

| Category | Description |
|----------|-------------|
| Authentication | Login, logout, session management |
| Admin | Admin-specific functionality |
| Employee | Employee-specific functionality |
| CrossBrowser | Tests that run across multiple browsers |

## Test Types

| Type | Description |
|------|-------------|
| Smoke | Critical path tests - run first |
| Functional | Feature functionality tests |
| Navigation | Page navigation tests |
| Security | Authorization and access control |
| Integration | Tests requiring multiple components |

## Best Practices Implemented

1. **Stable Locators**: Prefer `data-test` attributes, IDs, then CSS selectors
2. **Single Responsibility**: Each test verifies one specific behavior
3. **Independent Tests**: No test order dependency
4. **No Thread.Sleep**: All waits are explicit condition-based waits
5. **API Data Setup**: Create test data via API, not UI
6. **Idempotent Cleanup**: Tests clean up their data in Dispose
7. **Screenshot on Failure**: Automatic debugging aid
8. **Meaningful Naming**: Test names describe expected behavior
9. **DRY Principles**: Common functionality in base classes
10. **Environment Isolation**: Configuration per environment

## Adding New Tests

### 1. Create Page Object (if needed)

```csharp
public class NewFeaturePage : BasePage
{
    private static readonly By SubmitButton = By.CssSelector("[data-test='submit-btn']");
    
    public NewFeaturePage(IWebDriver driver) : base(driver) { }
    
    public void ClickSubmit() => SafeClick(SubmitButton);
}
```

### 2. Create Test Class

```csharp
[Trait("Category", "NewFeature")]
public class NewFeatureTests : BaseTest
{
    public NewFeatureTests(ITestOutputHelper output) : base(output) { }
    
    [Fact]
    [Trait("TestType", "Functional")]
    public void NewFeature_WhenCondition_ShouldBehavior()
    {
        ExecuteWithScreenshotOnFailure(() =>
        {
            // Arrange
            // Act
            // Assert
        });
    }
}
```

## CI/CD Integration

### GitHub Actions Example

```yaml
- name: Run E2E Tests
  run: dotnet test backend/RewardPointsSystem.E2ETests --logger "trx;LogFileName=results.trx"
  env:
    E2E_HEADLESS: "true"
    E2E_ADMIN_EMAIL: ${{ secrets.E2E_ADMIN_EMAIL }}
    E2E_ADMIN_PASSWORD: ${{ secrets.E2E_ADMIN_PASSWORD }}
```

## Troubleshooting

### Tests fail with element not found
- Check if the application is running
- Verify selectors match current DOM structure
- Increase explicit wait timeout

### Tests are flaky
- Ensure proper waits before assertions
- Use API for data setup instead of UI
- Check for race conditions in the application

### Screenshots not captured
- Verify screenshot directory has write permissions
- Check if WebDriver is still active when screenshot is attempted

## License

MIT
