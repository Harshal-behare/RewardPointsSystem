# RewardPointsSystem

A production-grade, full-stack reward points management system featuring an Angular 21 frontend and .NET 8.0 backend. This enterprise-ready application enables employees to earn points by participating in events and redeem those points for products from a catalog.

[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download)
[![Angular](https://img.shields.io/badge/Angular-21-red.svg)](https://angular.dev/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-2019+-orange.svg)](https://www.microsoft.com/sql-server)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)

## 🎯 Overview

RewardPointsSystem is a complete full-stack web application demonstrating clean architecture principles with a modern, responsive UI. It features:

- 🌐 **Modern Web Application** with separate Admin and Employee portals
- 👥 **Employees** register and participate in company events
- 🎁 **Event Winners** receive reward points based on their performance
- 🛍️ **Point Redemption** allows employees to exchange points for products
- 📊 **Interactive Dashboards** with charts and real-time statistics
- 🔐 **JWT Authentication** with role-based access control (Admin/Employee)
- 🎨 **Responsive Design** with modern UI components

## 🖥️ Live Application

| Portal | URL | Description |
|--------|-----|-------------|
| **Frontend** | `http://localhost:4200` | Angular web application |
| **Backend API** | `http://localhost:5000` | .NET REST API with Swagger |

## ✨ Key Features

### Admin Portal

- **Dashboard** - KPI cards, charts, recent activity, quick actions
- **Events Management** - Create, edit, manage event lifecycle, award points
- **Products Management** - Categories, product catalog, inventory control
- **Users Management** - Create users, assign roles, manage accounts
- **Redemptions** - Approve, reject, and track all redemption requests
- **Profile** - Admin profile settings and preferences

### Employee Portal

- **Dashboard** - Personal points summary, upcoming events, featured products
- **Events** - Browse and register for upcoming events
- **Products Catalog** - Browse products, redeem points with live balance
- **My Account** - Transaction history, pending redemptions, points breakdown
- **Profile** - Personal settings and preferences

### Event Management

- Create and manage events with dedicated point pools
- Track event lifecycle (Draft → Upcoming → Active → Completed/Cancelled)
- Register participants and track attendance
- Award points to top performers (1st, 2nd, 3rd place with custom points)

### Points & Accounts

- Automatic reward account creation for users
- Real-time balance tracking with pending points support
- Point earning and redemption workflows
- Complete transaction history with timestamps and event references

### Product Catalog

- Category-based product organization
- Product catalog with images and descriptions
- Real-time inventory tracking
- Search, filter, and pagination

### Redemption Workflow

- Multi-stage redemption process (Pending → Approved → Delivered)
- Stock reservation during redemption
- Automatic refunds on cancellation (Rejected/Cancelled)
- Full transaction auditing

## 🏗️ Architecture

### Full-Stack Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                    Frontend (Angular 21)                         │
│  ┌───────────┐  ┌───────────┐  ┌───────────┐  ┌───────────┐    │
│  │  Admin    │  │ Employee  │  │  Shared   │  │   Core    │    │
│  │  Portal   │  │  Portal   │  │Components │  │ Services  │    │
│  └───────────┘  └───────────┘  └───────────┘  └───────────┘    │
└────────────────────────────┬────────────────────────────────────┘
                             │ HTTP/REST
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│               Backend API (.NET 8.0 Web API)                     │
│              JWT Authentication + CORS + Swagger                 │
└────────────────────────────┬────────────────────────────────────┘
                             │
┌────────────────────────────┴────────────────────────────────────┐
│                   Clean Architecture Layers                      │
├─────────────────────────────────────────────────────────────────┤
│  API Layer         │ Controllers, Auth, Middleware              │
├─────────────────────────────────────────────────────────────────┤
│  Application Layer │ Services, DTOs, Validators, Use Cases      │
├─────────────────────────────────────────────────────────────────┤
│  Domain Layer      │ Entities, Value Objects, Exceptions        │
├─────────────────────────────────────────────────────────────────┤
│  Infrastructure    │ EF Core, Repositories, SQL Server          │
└─────────────────────────────────────────────────────────────────┘
```

### Frontend Structure (Angular 21)

```
frontend/src/app/
├── auth/                    # Authentication (Login, Guards, JWT)
├── core/                    # Core services (API, Toast, Modals)
├── features/
│   ├── admin/               # Admin-only pages
│   │   ├── dashboard/       # Admin dashboard with KPIs & charts
│   │   ├── events/          # Event management
│   │   ├── products/        # Product & category management
│   │   ├── users/           # User management
│   │   ├── redemptions/     # Redemption approvals
│   │   └── profile/         # Admin profile
│   └── employee/            # Employee pages
│       ├── dashboard/       # Employee dashboard
│       ├── events/          # Event registration
│       ├── products/        # Product catalog & redemption
│       ├── account/         # Points & transaction history
│       └── profile/         # Employee profile
├── layouts/                 # Admin & Employee layout components
└── shared/                  # Reusable components (Button, Card, Badge, etc.)
```

### Backend Structure (.NET Clean Architecture)

```
backend/
├── RewardPointsSystem.Api/           # API Layer (Controllers, Auth)
│   ├── Controllers/                   # REST API endpoints
│   │   ├── AuthController.cs         # Login, Register, JWT
│   │   ├── AdminController.cs        # Admin dashboard endpoints
│   │   ├── EmployeeController.cs     # Employee dashboard endpoints
│   │   ├── EventsController.cs       # Event CRUD + management
│   │   ├── ProductsController.cs     # Products + Categories
│   │   ├── UsersController.cs        # User management
│   │   ├── PointsController.cs       # Points & transactions
│   │   └── RedemptionsController.cs  # Redemption workflows
│   └── Configuration/                 # DI, CORS, JWT setup
│
├── RewardPointsSystem.Application/   # Business Logic Layer
│   ├── Services/                      # Business services
│   │   ├── Admin/                     # Admin dashboard service
│   │   ├── Employee/                  # Employee dashboard service
│   │   ├── Events/                    # Event services
│   │   ├── Products/                  # Product services
│   │   └── Orchestrators/             # Workflow coordinators
│   ├── DTOs/                          # Data Transfer Objects
│   ├── Validators/                    # FluentValidation validators
│   └── Interfaces/                    # Service interfaces
│
├── RewardPointsSystem.Domain/        # Core Business Models
│   └── Entities/                      # Domain entities
│       ├── User, Role, UserRole       # User management
│       ├── Event, EventParticipant    # Event system
│       ├── PointsAccount, PointsTransaction  # Points system
│       ├── Product, Category          # Product catalog
│       └── Redemption                 # Redemption operations
│
├── RewardPointsSystem.Infrastructure/ # Data Access Layer
│   ├── Data/                          # EF Core DbContext
│   ├── Repositories/                  # Repository implementations
│   └── Migrations/                    # Database migrations
│
└── RewardPointsSystem.Tests/         # Unit & Integration Tests
```

## 🚀 Getting Started

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- [Node.js 18+](https://nodejs.org/) with npm
- [SQL Server 2019+](https://www.microsoft.com/sql-server) (Express edition works)
- Angular CLI: `npm install -g @angular/cli`

### Quick Start

#### 1. Clone and Setup

```bash
git clone <repository-url>
cd RewardPointsSystem
```

#### 2. Database Setup

```bash
# Update connection string in backend/RewardPointsSystem.Api/appsettings.json
# Run migrations
cd backend/RewardPointsSystem.Api
dotnet ef database update
```

#### 3. Start Backend API

```bash
cd backend/RewardPointsSystem.Api
dotnet run
```

The API will be available at `http://localhost:5000` with Swagger UI.

#### 4. Start Frontend

```bash
cd frontend
npm install
npm start
```

The application will be available at `http://localhost:4200`.

### Default Login Credentials

| Role | Email | Password |
|------|-------|----------|
| Admin | system@agdata.com | System@123 |
| Employee | Harshal.Behare@agdata.com | Harshal@123 |

### Running Tests

```bash
# Backend tests
cd backend
dotnet test

# Frontend tests
cd frontend
npm test
```

## 🔑 API Documentation

The API includes comprehensive Swagger documentation available at `http://localhost:5000` when running.

### Key API Endpoints

| Category | Endpoints | Description |
|----------|-----------|-------------|
| **Auth** | `/api/v1/auth/*` | Login, Register, Refresh Token |
| **Users** | `/api/v1/users/*` | User CRUD, Role Assignment |
| **Events** | `/api/v1/events/*` | Event Management, Participants |
| **Products** | `/api/v1/products/*` | Product Catalog, Categories |
| **Points** | `/api/v1/points/*` | Points Accounts, Transactions |
| **Redemptions** | `/api/v1/redemptions/*` | Redemption Workflow |
| **Admin** | `/api/v1/admin/*` | Admin Dashboard Data |
| **Employee** | `/api/v1/employee/*` | Employee Dashboard Data |

See [API_DOCUMENTATION.md](API_DOCUMENTATION.md) for complete API reference.

## 📊 Test Coverage

The project includes comprehensive unit tests:

| Test File                      | Tests | Coverage                          |
| ------------------------------ | ----- | --------------------------------- |
| UserServiceTests               | 14    | User CRUD, validation, duplicates |
| RoleServiceTests               | 8     | Role management operations        |
| UserRoleServiceTests           | 9     | Role assignments                  |
| EventServiceTests              | 18    | Event lifecycle, transitions      |
| EventParticipationServiceTests | 11    | Participant tracking              |
| PointsAccountServiceTests      | 11    | Account and balance management    |
| TransactionServiceTests        | 7     | Transaction recording             |
| PointsAwardingServiceTests     | 11    | Points allocation, pool limits    |
| ProductServicesTests           | 27    | Catalog, pricing, inventory       |
| OrchestratorTests              | 16    | Full workflow orchestration       |

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

- **[ARCHITECTURE.md](ARCHITECTURE.md)** - Complete Clean Architecture guide with layer responsibilities
- **[EXCEPTIONS.md](EXCEPTIONS.md)** - Domain exception handling and error management
- **[agdata-srp-architecture.md](agdata-srp-architecture.md)** - Detailed SRP implementation and service specifications
- **[Project_Description.md](Project_Description.md)** - Business requirements and entity relationships

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
├── frontend/                          # Angular 21 Frontend
│   ├── src/app/
│   │   ├── auth/                      # Authentication module
│   │   ├── core/                      # Core services
│   │   ├── features/
│   │   │   ├── admin/                 # Admin portal pages
│   │   │   └── employee/              # Employee portal pages
│   │   ├── layouts/                   # Layout components
│   │   └── shared/                    # Shared components
│   └── package.json
│
├── backend/
│   ├── RewardPointsSystem.Api/        # API Layer (REST Controllers)
│   │   ├── Controllers/               # API endpoints
│   │   └── Configuration/             # DI, JWT, CORS setup
│   │
│   ├── RewardPointsSystem.Application/ # Business Logic Layer
│   │   ├── Services/                  # Business services
│   │   ├── DTOs/                      # Data Transfer Objects
│   │   ├── Validators/                # FluentValidation
│   │   └── Interfaces/                # Service contracts
│   │
│   ├── RewardPointsSystem.Domain/     # Domain Layer
│   │   └── Entities/                  # Core domain models
│   │
│   ├── RewardPointsSystem.Infrastructure/ # Data Access Layer
│   │   ├── Data/                      # EF Core DbContext
│   │   ├── Repositories/              # Repository pattern
│   │   └── Migrations/                # Database migrations
│   │
│   └── RewardPointsSystem.Tests/      # Test Project
│       ├── UnitTests/                 # Unit tests
│       └── IntegrationTests/          # Integration tests
│
├── Database/                          # SQL Scripts & Migrations
├── docs/                              # Additional documentation
├── API_DOCUMENTATION.md               # Complete API reference
├── DEMO_GUIDE.md                      # Demo walkthrough
└── README.md                          # This file
```

## 🔄 Technology Stack

### Frontend
- **Angular 21** - Modern TypeScript framework
- **Tailwind CSS** - Utility-first CSS framework
- **ApexCharts** - Interactive charts and graphs
- **Angular Material** - UI component library
- **RxJS** - Reactive programming

### Backend
- **.NET 8.0** - Modern C# framework
- **ASP.NET Core Web API** - RESTful API framework
- **Entity Framework Core** - ORM for SQL Server
- **FluentValidation** - Input validation
- **AutoMapper** - Object mapping
- **JWT Bearer** - Authentication

### Database
- **SQL Server 2019+** - Relational database
- **EF Core Migrations** - Database versioning

### Testing
- **xUnit** - Backend testing framework
- **Vitest** - Frontend testing framework
- **FluentAssertions** - Readable assertions

## 🎨 Naming Conventions

The project follows standard naming conventions:

| Element                      | Convention                 | Example                            |
| ---------------------------- | -------------------------- | ---------------------------------- |
| Classes, Methods, Properties | PascalCase                 | `UserService`, `CreateUserAsync()` |
| Interfaces                   | PascalCase with 'I' prefix | `IUserService`                     |
| Local variables, parameters  | camelCase                  | `userId`, `pointsAwarded`          |
| Private fields               | \_camelCase                | `_unitOfWork`, `_logger`           |
| Angular Components           | kebab-case files           | `user-list.component.ts`           |
| CSS Classes                  | kebab-case                 | `.user-card`, `.points-badge`      |

## 📈 Future Enhancements

Potential areas for expansion:

- [ ] Email notifications for events and redemptions
- [ ] Point expiration policies
- [ ] Tiered reward levels
- [ ] External payment integration for hybrid redemptions
- [ ] Mobile application (React Native / Flutter)
- [ ] Advanced reporting and analytics
- [ ] Bulk user import/export
- [ ] Event calendar integration

## 🤝 Contributing

Contributions are welcome! Please follow these guidelines:

1. Maintain Clean Architecture principles
2. Follow naming conventions strictly
3. Write unit tests for new features
4. Update documentation for significant changes
5. Use async/await consistently
6. Validate inputs at service boundaries

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 👨‍💻 Author

Created as a demonstration of production-grade full-stack development with emphasis on:

- Clean Architecture principles
- Modern Angular development
- RESTful API design
- Enterprise security patterns
- Test-driven development

## 📞 Support

For questions or issues, please:

- Check the [API_DOCUMENTATION.md](API_DOCUMENTATION.md) for API reference
- Check the [DEMO_GUIDE.md](DEMO_GUIDE.md) for demo walkthrough
- Check the [Project_Description.md](Project_Description.md) for business logic
- Open an issue in the repository

---

**Built with ❤️ using Angular 21, .NET 8.0, and SQL Server**
