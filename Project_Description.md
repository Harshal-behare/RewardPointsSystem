# Points-Based Redemption System - Project Overview

## What This Project Is

This is a **full-stack Points-Based Redemption System** featuring an **Angular 21 frontend** and **.NET 8.0 backend** with **SQL Server** database. It's an enterprise-ready business application where employees can earn points through company events and redeem those points for products from a catalog.

### Technology Stack
- **Frontend**: Angular 21, Tailwind CSS, ApexCharts
- **Backend**: .NET 8.0 Web API, Entity Framework Core
- **Database**: SQL Server with EF Core Migrations
- **Authentication**: JWT Bearer Tokens with Role-Based Access Control

### Architecture
The system follows a **4-layer Clean Architecture** design:
- **API Layer**: REST Controllers, JWT Authentication, CORS configuration
- **Application Layer**: Business logic, services, DTOs, and validators
- **Domain Layer**: Core business entities (pure models with no dependencies)
- **Infrastructure Layer**: Entity Framework Core with SQL Server

---

## Core Entities

### User
Represents a person who uses the system. Users can:
- Have a unique email or employee ID
- Accumulate points through events
- Redeem points for products
- Have account status (active/inactive)

### Event
Represents company activities where employees can earn points. Events have:
- Name, description, and location (physical or virtual)
- Event dates and registration period
- Points pool for distribution to winners
- 1st, 2nd, 3rd place point allocations
- Maximum participant limit
- Lifecycle status (Draft → Upcoming → Active → Completed/Cancelled)
- Image URL for display

### Category
Product categories for organization:
- Name and description
- Active/Inactive status
- Display order

### Product
Items available in the catalog that users can redeem with their points. Products have:
- Name and description
- Points price for redemption
- Stock quantity with real-time tracking
- Category association
- Featured flag for homepage display
- Image URL
- Active/Inactive status

### Redemption
A transaction record when a user exchanges points for a product. Each redemption captures:
- Which user made the redemption
- Which product was redeemed
- Quantity and total points spent
- Redemption status (Pending → Approved → Delivered, or Rejected/Cancelled)
- Notes and delivery information
- Timestamps for each status change

### Points Transaction
A record of points earned or spent. Tracks:
- The user involved
- Points added or deducted
- Related event (if points were earned)
- Related redemption (if points were spent)
- Transaction timestamp

---

## Business Logic

### User Management
The system needs to manage users by:
- Adding new users to the system
- Updating user information
- Retrieving user details
- Preventing duplicate users based on email or employee ID
- Tracking user point balances

### Event Management
The system handles events by:
- Creating new events with full details (Draft status)
- Managing event lifecycle:
  - **Draft**: Initial creation, can be edited freely
  - **Upcoming**: Published for employee registration
  - **Active**: Event is ongoing
  - **Completed**: Event finished, awards can be distributed
  - **Cancelled**: Event was cancelled
- Registering and unregistering participants
- Awarding points to top performers (1st, 2nd, 3rd place)
- Tracking event statistics

### Product Catalog Management
The system manages products by:
- Adding products to the catalog
- Updating product details (name, description, points required, stock)
- Removing products from the catalog
- Listing available products
- Validating stock levels before redemption

### Redemption Process
The core business logic where users redeem points for products:
- User selects a product from the catalog
- System validates user has sufficient points
- System checks product is in stock
- Points are deducted from user balance
- Stock quantity is reduced
- Redemption transaction is recorded
- Points transaction is logged

---

## Data Relationships

### User and Redemptions
- One user can have many redemptions
- Each redemption belongs to one user

### User and Points Transactions
- One user can have many points transactions
- Each transaction belongs to one user

### Event and Points Transactions
- One event can be linked to many points transactions
- Each points transaction (when earning points) is linked to one event

### Product and Redemptions
- One product can be redeemed multiple times (in different redemptions)
- Each redemption is for one specific product

### Redemption and Points Transaction
- Each redemption creates a corresponding points transaction (for deduction)
- The relationship tracks how points were spent

---

## Object-Oriented Principles Applied

### Encapsulation
Data is protected using private fields with controlled access through public properties and methods. Internal implementation details are hidden from external code.

### Inheritance
Hierarchical relationships between classes. For example, an Admin user type might inherit from a base User class, gaining all user properties while adding administrative capabilities.

### Interfaces
Defines contracts for services, allowing different implementations while maintaining consistent behavior. This enables flexibility and makes the code more maintainable.

### Polymorphism
Different classes can be treated through common interfaces, allowing for flexible and extensible code design.

---

## Business Rules

### Point Balance Validation
- Users cannot redeem products if they have insufficient points
- Point balance must always be non-negative
- All point transactions must be logged

### Product Availability
- Products cannot be redeemed if out of stock
- Stock levels must be validated before processing redemption
- Stock quantity must be updated after successful redemption

### User Validation
- Email addresses must be unique
- Employee IDs must be unique
- Required fields must be provided when creating users

### Redemption Validation
- User must exist and be active
- Product must exist and be available
- User must have sufficient points
- Product must be in stock

### Data Integrity
- All transactions must be recorded
- Point additions and deductions must be tracked
- Relationships between entities must be maintained
- No orphaned records should exist

---

## Database

The system uses **SQL Server** with **Entity Framework Core** for data persistence:

### Tables
- **Users** - User accounts with hashed passwords
- **Roles** - Admin and Employee roles
- **UserRoles** - Many-to-many user-role mapping
- **Events** - Event definitions and details
- **EventParticipants** - Event registrations and awards
- **PointsAccounts** - User point balances
- **PointsTransactions** - All point movements
- **Categories** - Product categories
- **Products** - Product catalog
- **Redemptions** - Redemption requests and status

### Features
- EF Core migrations for schema versioning
- Repository pattern for data access
- Unit of Work pattern for transactions
- Optimized queries with includes and filtering

---

## System Operations

### User Operations
- Add a new user
- Update existing user details
- Retrieve user by ID or email
- Get user's current point balance
- Prevent duplicate user creation

### Event Operations
- Create new events
- Manage event details
- Link events to point allocations

### Product Operations
- Add new products to catalog
- Update product information
- Delete products from catalog
- List all available products
- Check product stock availability

### Redemption Operations
- Process redemption requests
- Validate user eligibility
- Validate product availability
- Deduct points from user
- Reduce product stock
- Create redemption record
- Log points transaction

### Points Transaction Operations
- Record points earned from events
- Record points spent on redemptions
- Calculate user balance
- Retrieve transaction history

---

## Web Application

### Admin Portal Features
- **Dashboard**: KPI cards, charts (pie, donut), recent activity, quick actions
- **Events**: Full CRUD, lifecycle management, participant management, award points
- **Products**: Category management, product CRUD, stock management
- **Users**: User management, role assignments
- **Redemptions**: Approve, reject, deliver redemptions
- **Profile**: Admin profile settings

### Employee Portal Features
- **Dashboard**: Points summary, upcoming events, featured products, transaction chart
- **Events**: Browse events, register/unregister
- **Products**: Browse catalog, filter by category, redeem products
- **Account**: Transaction history, pending redemptions, cancel pending
- **Profile**: Employee profile settings

### UI Components
- Card, Button, Badge, Icon components
- Modal dialogs for forms
- Toast notifications
- Pagination and filtering
- Responsive design with Tailwind CSS
- ApexCharts for data visualization

---

## Notes

This project demonstrates full-stack development including:
- Clean Architecture in .NET
- Modern Angular development with signals
- JWT authentication and authorization
- RESTful API design
- Entity Framework Core with SQL Server
- Responsive web design
