# Infrastructure Unit Tests

This directory contains comprehensive unit tests for the RewardPointsSystem infrastructure layer, specifically focused on Entity Framework Core and database configuration.

## Test Coverage

### 1. RewardPointsDbContextTests.cs
**Test Case 1: RewardPointsDbContext is correctly configured with the SQL Server connection string.**

Tests included:
- `DbContext_ShouldBeConfiguredWithSqlServer` - Verifies that the DbContext is configured with SQL Server provider and correct connection string
- `DbContext_ShouldBeInstantiatedWithValidOptions` - Tests DbContext instantiation with valid options
- `DbContext_ShouldHaveAllDbSetsInitialized` - Verifies all DbSet properties are properly initialized
- `DbContext_ShouldCreateModelWithEntityConfigurations` - Tests that the model is created with entity configurations
- `DbContext_ShouldApplySqlServerConfigurations` - Verifies SQL Server specific configurations including retry logic

**Total Tests: 5**

### 2. NavigationPropertiesTests.cs
**Test Case 2: Navigation properties in domain entities are correctly mapped and can be accessed.**

Tests included:
- `User_PointsAccount_OneToOne_ShouldBeCorrectlyMapped` - Tests one-to-one relationship between User and PointsAccount
- `User_UserRoles_OneToMany_ShouldBeCorrectlyMapped` - Tests one-to-many relationship between User and UserRoles
- `User_EventParticipations_OneToMany_ShouldBeCorrectlyMapped` - Tests one-to-many relationship between User and EventParticipations
- `User_Redemptions_OneToMany_ShouldBeCorrectlyMapped` - Tests one-to-many relationship between User and Redemptions
- `Product_InventoryItem_OneToOne_ShouldBeCorrectlyMapped` - Tests one-to-one relationship between Product and InventoryItem
- `Product_ProductPricing_OneToMany_ShouldBeCorrectlyMapped` - Tests one-to-many relationship between Product and ProductPricing
- `Event_EventParticipants_OneToMany_ShouldBeCorrectlyMapped` - Tests one-to-many relationship between Event and EventParticipants
- `PointsTransaction_User_ManyToOne_ShouldBeCorrectlyMapped` - Tests many-to-one relationship between PointsTransaction and User

**Total Tests: 8**

### 3. ServiceRegistrationTests.cs
**Test Case 3: The RegisterRewardPointsServices method correctly registers DbContext and other services with configuration.**

Tests included:
- `RegisterRewardPointsServices_ShouldRegisterDbContext` - Verifies DbContext is registered in DI container
- `RegisterRewardPointsServices_ShouldConfigureDbContextWithCorrectConnectionString` - Tests connection string configuration
- `RegisterRewardPointsServices_ShouldRegisterAllRequiredServices` - Verifies all required services are registered
- `AddInfrastructure_ShouldRegisterDbContext` - Tests AddInfrastructure method registration
- `AddInfrastructure_ShouldConfigureDbContextWithSqlServer` - Verifies SQL Server provider configuration
- `AddInfrastructure_ShouldRegisterDbContextAsScoped` - Tests that DbContext is registered with correct lifetime (Scoped)
- `RegisterRewardPointsServices_ShouldHandleMissingConnectionString` - Tests behavior with missing connection string
- `ServiceRegistration_ShouldAllowMultipleDbContextsInDifferentScopes` - Verifies different scopes get different DbContext instances
- `AddInfrastructure_ShouldConfigureSqlServerRetryLogic` - Tests SQL Server retry logic configuration

**Total Tests: 9**

### 4. ConfigurationLoadingTests.cs
**Test Case 4: The application correctly loads the database connection string from appsettings.json.**

Tests included:
- `Configuration_ShouldLoadConnectionStringFromInMemorySettings` - Tests loading connection string from configuration
- `Configuration_ShouldReturnNullForMissingConnectionString` - Tests behavior with missing connection string
- `Configuration_ShouldLoadMultipleConnectionStrings` - Tests loading multiple connection strings
- `Configuration_ShouldHandleComplexConnectionString` - Tests handling of complex connection strings
- `Configuration_ShouldAccessConnectionStringViaIndexer` - Tests accessing configuration via indexer syntax
- `Configuration_ShouldLoadFromJsonFile` - Tests loading configuration from actual JSON file
- `Configuration_ShouldHandleOptionalJsonFile` - Tests optional JSON file behavior
- `Configuration_ShouldOverrideWithEnvironmentVariables` - Tests configuration override mechanism
- `Configuration_ShouldAccessNestedConfigurationValues` - Tests accessing nested configuration values
- `Configuration_ShouldGetSection` - Tests GetSection method for configuration sections

**Total Tests: 10**

### 5. CrudOperationsTests.cs
**Test Case 5: Basic CRUD operations for an entity (e.g., User) function correctly via RewardPointsDbContext.**

Tests included:

#### Create Operations (3 tests)
- `Create_ShouldAddUserToDatabase` - Tests adding a single user
- `Create_ShouldAddMultipleUsers` - Tests adding multiple users at once
- `CreateAsync_ShouldAddUserAsynchronously` - Tests asynchronous user creation

#### Read Operations (6 tests)
- `Read_ShouldRetrieveUserById` - Tests retrieving user by ID
- `Read_ShouldRetrieveUserByEmail` - Tests retrieving user by email
- `Read_ShouldRetrieveAllUsers` - Tests retrieving all users
- `ReadAsync_ShouldRetrieveUserAsynchronously` - Tests asynchronous user retrieval
- `Read_ShouldReturnNullForNonExistentUser` - Tests behavior with non-existent user
- `Read_ShouldQueryWithLinq` - Tests LINQ query capabilities

#### Update Operations (4 tests)
- `Update_ShouldModifyUserProperties` - Tests updating user properties
- `Update_ShouldModifyUserWithoutExplicitUpdate` - Tests automatic change tracking
- `Update_ShouldToggleUserActiveStatus` - Tests updating boolean properties
- `UpdateAsync_ShouldModifyUserAsynchronously` - Tests asynchronous updates

#### Delete Operations (3 tests)
- `Delete_ShouldRemoveUserFromDatabase` - Tests deleting a single user
- `Delete_ShouldRemoveMultipleUsers` - Tests deleting multiple users
- `DeleteAsync_ShouldRemoveUserAsynchronously` - Tests asynchronous deletion

#### Complex Operations (3 tests)
- `ComplexOperation_ShouldCreateUserWithPointsAccount` - Tests creating related entities
- `ComplexOperation_ShouldDeleteUserAndCascadePointsAccount` - Tests cascade delete behavior
- `ComplexOperation_ShouldSaveMultipleEntitiesInOneTransaction` - Tests batch save operations

**Total Tests: 19**

## Summary

**Total Test Files: 5**
**Total Test Cases: 51**

All tests use the InMemory database provider for fast, isolated testing without requiring a real SQL Server instance.

## Running the Tests

To run all infrastructure tests:
```bash
dotnet test --filter "FullyQualifiedName~Infrastructure"
```

To run a specific test class:
```bash
dotnet test --filter "FullyQualifiedName~RewardPointsDbContextTests"
```

## Dependencies

- xUnit - Testing framework
- FluentAssertions - Assertion library
- Microsoft.EntityFrameworkCore.InMemory - In-memory database provider for testing
- Microsoft.Extensions.Configuration.Json - Configuration support
- Microsoft.Extensions.DependencyInjection - Dependency injection support
