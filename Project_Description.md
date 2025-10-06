# Points-Based Redemption System - Project Overview

## What This Project Is

This is a **Points-Based Redemption System** built in C#. It's a business application where users can earn points through various events and redeem those points for products from a catalog. The entire system runs in-memory without any database.

---

## Core Entities

### User
Represents a person who uses the system. Users can:
- Have a unique email or employee ID
- Accumulate points through events
- Redeem points for products
- Have account status (active/inactive)

### Event
Represents activities or actions that trigger point allocation. Events:
- Have a name and description
- Are associated with point values
- Can be linked to multiple point transactions
- Track when and why points were earned

### Product
Items available in the catalog that users can redeem with their points. Products have:
- Name and description
- Required points for redemption
- Stock quantity
- Availability status

### Redemption
A transaction record when a user exchanges points for a product. Each redemption captures:
- Which user made the redemption
- Which product was redeemed
- How many points were spent
- When the redemption occurred
- Transaction status

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
- Creating new events
- Managing event details
- Linking events to point allocations

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

## In-Memory Data Storage

The system uses in-memory collections (like List<T> or Dictionary<TKey, TValue>) to store data:
- All users are stored in memory
- All products are stored in memory
- All events are stored in memory
- All redemptions are stored in memory
- All points transactions are stored in memory

The data simulates CRUD operations (Create, Read, Update, Delete) without connecting to a database. When the application stops, all data is lost.

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

## Notes

This project demonstrates C# programming concepts including classes, objects, inheritance, interfaces, collections, and business logic implementation. It focuses on building a working system entirely in memory, without external dependencies like databases or APIs.
