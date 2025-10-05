# RewardPointsSystem

A production-grade, event-based reward points management system built with C# .NET 8.0. This system enables employees to earn points by participating in events and redeem those points for products from a catalog.

[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download)
[![Tests](https://img.shields.io/badge/tests-132%20passing-brightgreen.svg)](/)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)

## 🎯 Overview

RewardPointsSystem demonstrates clean architecture principles with strict adherence to the Single Responsibility Principle (SRP). It features a complete event-driven workflow where:

- 👥 **Employees** register and participate in company events
- 🎁 **Event Winners** receive reward points based on their performance
- 🛍️ **Point Redemption** allows employees to exchange points for products
- 📊 **Full Audit Trail** tracks all point movements and transactions
- 🔐 **Role-Based Access** supports Admin and Employee roles

## ✨ Key Features

### Event Management
- Create and manage events with dedicated point pools
- Track event lifecycle (Upcoming → Active → Completed/Cancelled)
- Register participants and track attendance
- Award points to event winners with validation

### Points & Accounts
- Automatic reward account creation for users
- Real-time balance tracking
- Point earning and redemption workflows
- Complete transaction history with timestamps

### Product Catalog
- Product catalog management
- Dynamic pricing with history support
- Real-time inventory tracking
- Stock reservation system

### Redemption Workflow
- Multi-stage redemption process (Pending → Approved → Delivered)
- Stock reservation during redemption
- Automatic refunds on cancellation
- Full transaction auditing

## 🏗️ Architecture

### Clean Three-Layer Architecture

```
┌─────────────────────────────────────────┐
│         Service Layer (14 Services)      │
│  ┌────────────┐ ┌──────────────────┐   │
│  │  Business  │ │   Orchestrators  │   │
│  │  Services  │ │  (Coordination)  │   │
│  └────────────┘ └──────────────────┘   │
└─────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────┐
│    Repository Layer (Unit of Work)      │
│         In-Memory Data Storage           │
└─────────────────────────────────────────┘
```

### Core Components

**11 Domain Models** organized by domain:
- Core: `User`, `Role`, `UserRole`
- Events: `Event`, `EventParticipant`
- Accounts: `PointsAccount`, `PointsTransaction`
- Products: `Product`, `ProductPricing`, `InventoryItem`
- Operations: `Redemption`

**14 Services** with single responsibilities:
- **User Management**: UserService, RoleService, UserRoleService
- **Event Operations**: EventService, EventParticipationService, PointsAwardingService
- **Account Management**: PointsAccountService, TransactionService
- **Product Catalog**: ProductCatalogService, PricingService, InventoryService
- **Orchestrators**: EventRewardOrchestrator, RedemptionOrchestrator
- **Administration**: AdminDashboardService

## 🚀 Getting Started

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- Any IDE supporting C# (Visual Studio, VS Code, Rider)

### Installation

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd RewardPointsSystem
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Build the solution**
   ```bash
   dotnet build
   ```

4. **Run the application**
   ```bash
   dotnet run --project RewardPointsSystem/RewardPointsSystem.csproj
   ```

### Running Tests

```bash
# Run all tests (132 tests)
dotnet test

# Run with detailed output
dotnet test --verbosity normal

# Run specific test class
dotnet test --filter "FullyQualifiedName~UserServiceTests"

# Run with code coverage
dotnet test --collect:"XPlat Code Coverage"
```

## 📊 Test Coverage

The project includes **132 comprehensive unit tests** with **100% pass rate**:

| Test File | Tests | Coverage |
|-----------|-------|----------|
| UserServiceTests | 14 | User CRUD, validation, duplicates |
| RoleServiceTests | 8 | Role management operations |
| UserRoleServiceTests | 9 | Role assignments |
| EventServiceTests | 18 | Event lifecycle, transitions |
| EventParticipationServiceTests | 11 | Participant tracking |
| PointsAccountServiceTests | 11 | Account and balance management |
| TransactionServiceTests | 7 | Transaction recording |
| PointsAwardingServiceTests | 11 | Points allocation, pool limits |
| ProductServicesTests | 27 | Catalog, pricing, inventory |
| OrchestratorTests | 16 | Full workflow orchestration |

**Test Framework**: xUnit + FluentAssertions  
**Test Approach**: Unit tests with real implementations and in-memory storage

## 🔑 Core Workflows

### 1. Event Reward Flow

```csharp
// Coordinate complete event-to-reward workflow
var result = await eventRewardOrchestrator.ProcessEventRewardAsync(
    eventId, 
    userId, 
    pointsAwarded: 500, 
    position: 1,
    adminId
);
```

**Orchestrated Steps:**
1. ✅ Validate event status (Active or Completed)
2. ✅ Verify user is registered participant
3. ✅ Check remaining points pool availability
4. ✅ Ensure no duplicate awards
5. ✅ Award points to participant
6. ✅ Create/update reward account
7. ✅ Record transaction for audit

### 2. Redemption Flow

```csharp
// Process product redemption
var result = await redemptionOrchestrator.ProcessRedemptionAsync(userId, productId);

// Approve redemption
await redemptionOrchestrator.ApproveRedemptionAsync(result.Redemption.Id);

// Mark as delivered
await redemptionOrchestrator.DeliverRedemptionAsync(result.Redemption.Id, "Shipped via FedEx");

// Or cancel with automatic refund
await redemptionOrchestrator.CancelRedemptionAsync(result.Redemption.Id);
```

**Orchestrated Steps:**
1. ✅ Verify user account and balance
2. ✅ Get current product price
3. ✅ Validate sufficient balance
4. ✅ Check product availability
5. ✅ Reserve stock
6. ✅ Deduct points
7. ✅ Create redemption record
8. ✅ Record transaction

## 📝 Usage Examples

### Create User and Award Points

```csharp
// Create user
var user = await userService.CreateUserAsync("john.doe@company.com", "John", "Doe");

// Create event
var event = await eventService.CreateEventAsync(
    "Q4 Sales Competition", 
    "Top performers win rewards",
    DateTime.UtcNow.AddDays(30),
    pointsPool: 10000
);

// Register participant
await participationService.RegisterParticipantAsync(event.Id, user.Id);

// Activate and complete event
await eventService.ActivateEventAsync(event.Id);
await eventService.CompleteEventAsync(event.Id);

// Award points (using orchestrator)
var result = await eventRewardOrchestrator.ProcessEventRewardAsync(
    event.Id, 
    user.Id, 
    500, // points
    1,   // position
    adminId
);
```

### Redeem Products

```csharp
// Create product
var product = await productCatalogService.CreateProductAsync(
    "Wireless Headphones",
    "Premium noise-cancelling headphones",
    "Electronics"
);

// Set pricing
await pricingService.SetProductPointsCostAsync(product.Id, 1000, DateTime.UtcNow);

// Add inventory
await inventoryService.CreateInventoryAsync(product.Id, quantity: 50, reorderLevel: 10);

// Redeem product
var redemption = await redemptionOrchestrator.ProcessRedemptionAsync(user.Id, product.Id);

if (redemption.Success)
{
    // Approve and deliver
    await redemptionOrchestrator.ApproveRedemptionAsync(redemption.Redemption.Id);
    await redemptionOrchestrator.DeliverRedemptionAsync(
        redemption.Redemption.Id, 
        "Delivered to office mailroom"
    );
}
```

## 🎯 Design Principles

### Single Responsibility Principle (SRP)

Every class has **exactly one reason to change**:

- ✅ Models store data only (no logic)
- ✅ Services perform one business operation
- ✅ Orchestrators coordinate workflows only
- ✅ No mixed concerns between domains
- ✅ Clear one-way dependency flow

### Key Architectural Decisions

1. **Service Isolation**: Individual services never call other services directly
2. **Orchestrator Pattern**: Complex workflows coordinated by orchestrators
3. **Automatic Account Creation**: Reward accounts created automatically when needed
4. **Stock Reservation**: Inventory reserved immediately during redemption
5. **Refund Auditing**: Cancellations create refund transactions for complete audit trail
6. **UTC Timestamps**: All operations use UTC for consistency
7. **Result Objects**: Orchestrators return detailed result objects with success/failure info

## 🔧 Configuration

### Service Registration

All services are automatically registered via dependency injection:

```csharp
// In Program.cs or Startup.cs
services.RegisterRewardPointsServices();
```

This registers all 14 services with **Scoped** lifetime.

## 📚 Documentation

Comprehensive documentation is available in the repository:

- **[WARP.md](WARP.md)** - Complete project guide with architecture, patterns, and conventions
- **[agdata-srp-architecture.md](agdata-srp-architecture.md)** - Detailed SRP implementation guide
- **[Project_Description.md](Project_Description.md)** - Business requirements and entity relationships
- **[UNIT_TESTS_IMPLEMENTATION_GUIDE.md](UNIT_TESTS_IMPLEMENTATION_GUIDE.md)** - Testing strategy and guidelines

## 🛡️ Validation Rules

### User Validations
- ✅ Unique email addresses (enforced at create/update)
- ✅ Required first and last names (no empty/whitespace)
- ✅ Valid email format

### Event Validations
- ✅ Event date cannot be in the past
- ✅ Points pool must be positive
- ✅ Cannot modify completed/cancelled events
- ✅ Only Active/Completed events can award points

### Points Validations
- ✅ Points must be positive integers
- ✅ User balance cannot go negative
- ✅ No duplicate awards per user per event
- ✅ Must be registered participant
- ✅ Pool must have sufficient balance

### Redemption Validations
- ✅ User must have reward account
- ✅ Sufficient point balance required
- ✅ Product must be in stock
- ✅ Valid status transitions (Pending → Approved → Delivered)
- ✅ Cannot cancel delivered redemptions

## 🗂️ Project Structure

```
RewardPointsSystem/
├── RewardPointsSystem/                # Main application
│   ├── Models/                        # Domain models (11 classes)
│   │   ├── Core/                      # User, Role, UserRole
│   │   ├── Events/                    # Event, EventParticipant
│   │   ├── Accounts/                  # PointsAccount, PointsTransaction
│   │   ├── Products/                  # Product, ProductPricing, InventoryItem
│   │   └── Operations/                # Redemption
│   ├── Services/                      # Service implementations (14 services)
│   │   ├── Users/                     # User management services
│   │   ├── Events/                    # Event management services
│   │   ├── Accounts/                  # Account & transaction services
│   │   ├── Products/                  # Product & inventory services
│   │   ├── Orchestrators/             # Workflow orchestrators
│   │   └── Admin/                     # Administrative services
│   ├── Interfaces/                    # Service interfaces (14 interfaces)
│   ├── Repositories/                  # Repository pattern implementation
│   ├── DTOs/                          # Data Transfer Objects
│   ├── Configuration/                 # Service registration
│   └── Program.cs                     # Application entry point
├── RewardPointsSystem.Tests/          # Test project
│   ├── UnitTests/                     # Unit tests (132 tests)
│   ├── Integration/                   # Integration tests
│   └── Helpers/                       # Test utilities
├── WARP.md                            # Complete project guide
├── agdata-srp-architecture.md         # SRP architecture guide
├── Project_Description.md             # Business requirements
└── README.md                          # This file
```

## 🔄 Technology Stack

- **.NET 8.0** - Modern C# framework
- **Microsoft.Extensions.DependencyInjection** - Built-in DI container
- **Microsoft.Extensions.Hosting** - Generic host for console apps
- **xUnit** - Testing framework
- **FluentAssertions** - Readable assertions
- **Moq** - Mocking framework
- **AutoFixture** - Test data generation

## 🎨 Naming Conventions

The project follows C# production-grade naming conventions:

| Element | Convention | Example |
|---------|-----------|---------|
| Classes, Methods, Properties | PascalCase | `UserService`, `CreateUserAsync()` |
| Interfaces | PascalCase with 'I' prefix | `IUserService` |
| Local variables, parameters | camelCase | `userId`, `pointsAwarded` |
| Private fields | _camelCase | `_unitOfWork`, `_logger` |
| Constants | ALL_CAPS | `MAX_POINTS` |
| Async methods | Suffix with "Async" | `ProcessRedemptionAsync()` |
| Booleans | Is/Has/Can/Should prefix | `IsActive`, `HasBalance` |

## 📈 Future Enhancements

Potential areas for expansion:

- [ ] Database integration (Entity Framework Core)
- [ ] RESTful API layer (ASP.NET Core Web API)
- [ ] Authentication & Authorization (JWT tokens)
- [ ] Email notifications for events and redemptions
- [ ] Reporting and analytics dashboard
- [ ] Point expiration policies
- [ ] Tiered reward levels
- [ ] External payment integration for hybrid redemptions

## 🤝 Contributing

Contributions are welcome! Please follow these guidelines:

1. Maintain SRP - each class should have one responsibility
2. Follow naming conventions strictly
3. Write unit tests for new features
4. Update documentation (WARP.md) for significant changes
5. Use async/await consistently
6. Validate inputs at service boundaries

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 👨‍💻 Author

Created as a demonstration of production-grade C# architecture with emphasis on:
- Clean code principles
- Single Responsibility Principle
- Test-driven development
- Enterprise-level design patterns

## 📞 Support

For questions or issues, please:
- Review the [WARP.md](WARP.md) documentation
- Check the [Project_Description.md](Project_Description.md) for business logic
- Examine the unit tests for usage examples
- Open an issue in the repository

---

**Built with ❤️ using C# and .NET 8.0**

