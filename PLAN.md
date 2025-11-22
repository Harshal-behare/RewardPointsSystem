# Reward Points System - Web API Implementation Plan

## 📋 Project Overview

This document outlines the complete transformation of the RewardPointsSystem from a console application to a production-ready ASP.NET Core Web API with comprehensive features including JWT authentication, comprehensive DTOs, Fluent Validation, AutoMapper, and Swagger documentation.

---

## 🎯 Milestones to Achieve

### ✅ **Milestone 1: Core API Setup**
- Creation of DTOs
- Identification of API Endpoints
- Implement API Controllers

### ✅ **Milestone 2: Production Features**
- Data Annotation or Fluent Validation
- Data validation and mapping entities via AutoMapper
- Configure Authentication and Authorization
- Best authentication applied and rationale
- Setup Swagger for API documentation
- Test API with Swagger and Postman

---

## 📅 PHASE-BY-PHASE IMPLEMENTATION

---

## **PHASE 1: PROJECT TRANSFORMATION & SETUP** ⚙️

**Status:** ✅ Completed  
**Estimated Time:** 1-2 hours

### **Step 1.1: Convert to ASP.NET Core Web API**

#### Tasks:
- [x] Modify `RewardPointsSystem.Api.csproj` to use `Microsoft.NET.Sdk.Web`
- [x] Add required NuGet packages
- [x] Transform Program.cs from console app to Web API
- [x] Configure middleware pipeline

#### Required NuGet Packages:
```xml
<!-- Core Web API -->
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="8.0.0" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />

<!-- Validation & Mapping -->
<PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="12.0.1" />
<PackageReference Include="FluentValidation.AspNetCore" Version="11.3.0" />

<!-- Additional Utilities -->
<PackageReference Include="Microsoft.AspNetCore.Mvc.Versioning" Version="5.1.0" />
<PackageReference Include="Microsoft.AspNetCore.Mvc.Versioning.ApiExplorer" Version="5.1.0" />
```

### **Step 1.2: Configure appsettings.json**

#### Configuration Sections:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "..."
  },
  "JwtSettings": {
    "SecretKey": "your-256-bit-secret-key-here",
    "Issuer": "RewardPointsAPI",
    "Audience": "RewardPointsClient",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:3000", "http://localhost:4200"],
    "AllowCredentials": true
  },
  "ApiSettings": {
    "Version": "v1",
    "Title": "Reward Points System API",
    "Description": "Production-grade reward points management API"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

#### Expected Outcomes:
- ✅ Project converted to Web API SDK
- ✅ All necessary packages installed
- ✅ Program.cs configured as Web API entry point
- ✅ Configuration files ready for Web API features

---

## **PHASE 2: CREATE COMPREHENSIVE DTOs** 📦

**Status:** ✅ Completed  
**Estimated Time:** 2-3 hours

### **Step 2.1: DTO Organization Structure**

```
Application/DTOs/
├── Common/
│   ├── ApiResponse.cs
│   ├── PagedResponse.cs
│   ├── ErrorResponse.cs
│   └── ValidationErrorResponse.cs
├── Auth/
│   ├── LoginRequestDto.cs
│   ├── LoginResponseDto.cs
│   ├── RegisterRequestDto.cs
│   ├── RefreshTokenRequestDto.cs
│   └── TokenResponseDto.cs
├── Users/
│   └── UserResponseDTOs.cs (existing - contains all user DTOs)
├── Roles/
│   └── RoleDTOs.cs (existing - contains all role DTOs)
├── Events/
│   └── EventResponseDTOs.cs (existing - contains all event DTOs)
├── Points/
│   └── PointsDTOs.cs (existing - contains all points DTOs)
├── Products/
│   └── ProductResponseDTOs.cs (existing - contains all product DTOs)
└── Redemptions/
    └── RedemptionDTOs.cs (existing - contains all redemption DTOs)
```

### **Step 2.2: DTO Categories**

#### **1. Common DTOs (Cross-cutting)**
- **ApiResponse<T>**: Standard API response wrapper
- **PagedResponse<T>**: Paginated data response
- **ErrorResponse**: Error details structure
- **ValidationErrorResponse**: Validation error collection

#### **2. Authentication DTOs**
- **AuthDTOs.cs** (existing - contains all authentication DTOs)
  - LoginRequestDto: Email + Password
  - LoginResponseDto: User info + tokens
  - RegisterRequestDto: Registration data
  - RefreshTokenRequestDto: Token refresh
  - TokenResponseDto: JWT token data

#### **3. User Domain DTOs**
- **UserDTOs.cs** (existing - contains create/update DTOs)
- **UserResponseDTOs.cs** (existing - contains response DTOs)
  - UserResponseDto: Basic user info
  - UserDetailsDto: Full user details with relationships
  - UserBalanceDto: User with points balance

#### **4. Role & Permission DTOs**
- **RoleDTOs.cs** (existing - contains all role DTOs)
  - CreateRoleDto: Role creation
  - RoleResponseDto: Role information
  - AssignRoleDto: User-role assignment
  - UserRoleResponseDto: User with roles

#### **5. Event Domain DTOs**
- **EventDTOs.cs** (existing - contains create/update DTOs)
- **EventResponseDTOs.cs** (existing - contains response DTOs)
  - EventResponseDto: Basic event info
  - EventDetailsDto: Full event with participants
  - RegisterParticipantDto: Participant registration
  - AwardPointsDto: Bulk points awarding
  - EventParticipantResponseDto: Participant info

#### **6. Points & Account DTOs**
- **PointsDTOs.cs** (existing - contains all points DTOs)
  - PointsAccountResponseDto: Account details
  - TransactionResponseDto: Transaction details
  - AddPointsDto: Add points request
  - DeductPointsDto: Deduct points request

#### **7. Product Domain DTOs**
- **ProductDTOs.cs** (existing - contains create/update DTOs)
- **ProductResponseDTOs.cs** (existing - contains response DTOs)
  - ProductResponseDto: Basic product info
  - ProductDetailsDto: Product with pricing & inventory
  - SetPricingDto: Pricing configuration
  - UpdateInventoryDto: Inventory management
  - InventoryResponseDto: Inventory status

#### **8. Redemption DTOs**
- **RedemptionDTOs.cs** (existing - contains all redemption DTOs)
  - CreateRedemptionDto: Redemption request
  - RedemptionResponseDto: Basic redemption info
  - RedemptionDetailsDto: Full redemption details
  - ApproveRedemptionDto: Approval data
  - CancelRedemptionDto: Cancellation data

#### Expected Outcomes:
- ✅ 40+ DTOs created covering all domains
- ✅ Organized folder structure
- ✅ Consistent naming conventions
- ✅ Clear separation between request and response DTOs
- ✅ Ready for validation and mapping

---

## **PHASE 3: API ENDPOINTS IDENTIFICATION** 🔗

**Status:** ✅ Completed  
**Estimated Time:** 2-3 hours  
**Actual Time:** 2 hours  
**Completed:** 2025-11-14

### **Step 3.1: RESTful API Design**

#### **Endpoint Categories:**

### **1. Authentication & Authorization (Public)**
```
POST   /api/v1/auth/register         - Register new user
POST   /api/v1/auth/login            - Login and get JWT token
POST   /api/v1/auth/refresh          - Refresh JWT token
POST   /api/v1/auth/logout           - Logout user
GET    /api/v1/auth/me               - Get current user info
```

### **2. Users Management**
```
GET    /api/v1/users                 - Get all users (Admin only) [Paginated]
GET    /api/v1/users/{id}            - Get user by ID
GET    /api/v1/users/me              - Get current user profile
POST   /api/v1/users                 - Create user (Admin only)
PUT    /api/v1/users/{id}            - Update user
DELETE /api/v1/users/{id}            - Soft delete user (Admin only)
GET    /api/v1/users/{id}/balance    - Get user points balance
GET    /api/v1/users/search          - Search users by email/name
```

### **3. Roles Management (Admin only)**
```
GET    /api/v1/roles                 - Get all roles
GET    /api/v1/roles/{id}            - Get role by ID
POST   /api/v1/roles                 - Create role
PUT    /api/v1/roles/{id}            - Update role
DELETE /api/v1/roles/{id}            - Delete role
POST   /api/v1/users/{userId}/roles  - Assign role to user
DELETE /api/v1/users/{userId}/roles/{roleId} - Revoke role from user
GET    /api/v1/users/{userId}/roles  - Get user's roles
```

### **4. Events Management**
```
GET    /api/v1/events                      - Get all events [Paginated, Filtered]
GET    /api/v1/events/{id}                 - Get event details
POST   /api/v1/events                      - Create event (Admin only)
PUT    /api/v1/events/{id}                 - Update event (Admin only)
DELETE /api/v1/events/{id}                 - Cancel event (Admin only)
PATCH  /api/v1/events/{id}/activate        - Activate event (Admin only)
PATCH  /api/v1/events/{id}/complete        - Complete event (Admin only)
PATCH  /api/v1/events/{id}/publish         - Publish event (Admin only)
GET    /api/v1/events/{id}/participants    - Get event participants
POST   /api/v1/events/{id}/participants    - Register for event
DELETE /api/v1/events/{id}/participants/{userId} - Unregister from event
POST   /api/v1/events/{id}/award-points    - Award points to winners (Admin only)
GET    /api/v1/events/upcoming             - Get upcoming events
GET    /api/v1/events/active               - Get active events
```

### **5. Points & Transactions**
```
GET    /api/v1/points/accounts/{userId}       - Get user points account
GET    /api/v1/points/transactions/{userId}   - Get user transactions [Paginated]
GET    /api/v1/points/transactions            - Get all transactions (Admin only)
POST   /api/v1/points/award                   - Award points (Admin only)
POST   /api/v1/points/deduct                  - Deduct points (Admin only)
GET    /api/v1/points/leaderboard             - Get points leaderboard
GET    /api/v1/points/summary                 - Get points summary (Admin only)
```

### **6. Products Catalog**
```
GET    /api/v1/products                    - Get all products [Paginated, Filtered]
GET    /api/v1/products/{id}               - Get product details
POST   /api/v1/products                    - Create product (Admin only)
PUT    /api/v1/products/{id}               - Update product (Admin only)
DELETE /api/v1/products/{id}               - Delete product (Admin only)
PATCH  /api/v1/products/{id}/activate      - Activate product (Admin only)
PATCH  /api/v1/products/{id}/deactivate    - Deactivate product (Admin only)
GET    /api/v1/products/{id}/pricing       - Get product pricing history
POST   /api/v1/products/{id}/pricing       - Set product price (Admin only)
GET    /api/v1/products/{id}/inventory     - Get product inventory
PATCH  /api/v1/products/{id}/inventory     - Update inventory (Admin only)
GET    /api/v1/products/categories         - Get product categories
GET    /api/v1/products/search             - Search products
```

### **7. Redemptions**
```
GET    /api/v1/redemptions                 - Get all redemptions [Filtered by user/admin]
GET    /api/v1/redemptions/{id}            - Get redemption details
POST   /api/v1/redemptions                 - Create redemption request
PATCH  /api/v1/redemptions/{id}/approve    - Approve redemption (Admin only)
PATCH  /api/v1/redemptions/{id}/deliver    - Mark as delivered (Admin only)
PATCH  /api/v1/redemptions/{id}/cancel     - Cancel redemption
GET    /api/v1/redemptions/my-redemptions  - Get current user's redemptions
GET    /api/v1/redemptions/pending         - Get pending redemptions (Admin only)
GET    /api/v1/redemptions/history         - Get redemption history [Paginated]
```

### **8. Admin Dashboard**
```
GET    /api/v1/admin/dashboard             - Get dashboard statistics
GET    /api/v1/admin/reports/points        - Points summary report
GET    /api/v1/admin/reports/users         - User activity report
GET    /api/v1/admin/reports/redemptions   - Redemptions report
GET    /api/v1/admin/reports/events        - Events report
GET    /api/v1/admin/alerts/inventory      - Low stock alerts
GET    /api/v1/admin/alerts/points         - Points pool alerts
```

### **Step 3.2: HTTP Status Codes Strategy**

| Status Code | Usage |
|-------------|-------|
| 200 OK | Successful GET, PUT, PATCH, DELETE |
| 201 Created | Successful POST (resource created) |
| 204 No Content | Successful DELETE (no content to return) |
| 400 Bad Request | Validation errors, malformed request |
| 401 Unauthorized | Missing or invalid authentication |
| 403 Forbidden | Authenticated but insufficient permissions |
| 404 Not Found | Resource doesn't exist |
| 409 Conflict | Duplicate resource, business rule violation |
| 422 Unprocessable Entity | Validation failed (semantic errors) |
| 500 Internal Server Error | Unexpected server error |

### Expected Outcomes:
- ✅ 50+ RESTful endpoints defined
- ✅ Clear HTTP verb usage (GET, POST, PUT, PATCH, DELETE)
- ✅ Consistent URL structure and naming
- ✅ Authorization requirements documented
- ✅ API versioning strategy (/api/v1/)

---

## **PHASE 4: IMPLEMENT API CONTROLLERS** 🎮

**Status:** ✅ Completed  
**Estimated Time:** 4-6 hours  
**Actual Time:** 5 hours  
**Completed:** 2025-11-14

### **Step 4.1: Controller Structure**

```
RewardPointsSystem.Api/Controllers/ (to be created)
├── v1/
│   ├── AuthController.cs
│   ├── UsersController.cs
│   ├── RolesController.cs
│   ├── EventsController.cs
│   ├── PointsController.cs
│   ├── ProductsController.cs
│   ├── RedemptionsController.cs
│   └── AdminController.cs
└── BaseApiController.cs
```

### **Step 4.2: Base Controller Setup**

```csharp
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public abstract class BaseApiController : ControllerBase
{
    protected IActionResult Success<T>(T data, string message = null)
    {
        return Ok(new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message
        });
    }

    protected IActionResult Created<T>(T data, string message = "Resource created successfully")
    {
        return StatusCode(201, new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message
        });
    }

    protected IActionResult Error(string message, int statusCode = 400)
    {
        return StatusCode(statusCode, new ErrorResponse
        {
            Success = false,
            Message = message,
            Timestamp = DateTime.UtcNow
        });
    }
}
```

### **Step 4.3: Controller Implementation Guidelines**

#### **Each Controller Should:**
1. Inherit from `BaseApiController`
2. Use constructor dependency injection
3. Implement XML documentation comments
4. Use async/await for all operations
5. Return consistent response types
6. Handle exceptions via middleware
7. Apply proper authorization attributes
8. Use model binding and validation
9. Implement pagination where applicable
10. Follow RESTful conventions

#### **Example Controller Structure:**
```csharp
/// <summary>
/// Manages user-related operations
/// </summary>
[ApiVersion("1.0")]
public class UsersController : BaseApiController
{
    private readonly IUserService _userService;
    private readonly IMapper _mapper;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        IUserService userService,
        IMapper mapper,
        ILogger<UsersController> logger)
    {
        _userService = userService;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Get all users (Admin only)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(PagedResponse<UserResponseDto>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 401)]
    [ProducesResponseType(typeof(ErrorResponse), 403)]
    public async Task<IActionResult> GetAllUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        // Implementation
    }
}
```

### **Step 4.4: Controllers to Implement**

#### **1. AuthController**
- Register
- Login
- Refresh token
- Logout
- Get current user

#### **2. UsersController**
- CRUD operations
- Search users
- Get user balance
- User profile management

#### **3. RolesController**
- Role CRUD
- Assign/revoke roles
- Get user roles

#### **4. EventsController**
- Event CRUD
- Event lifecycle management
- Participant registration
- Award points

#### **5. PointsController**
- View accounts
- View transactions
- Award/deduct points
- Leaderboard

#### **6. ProductsController**
- Product CRUD
- Pricing management
- Inventory management
- Product search

#### **7. RedemptionsController**
- Create redemption
- Approve/cancel/deliver
- View redemptions
- Redemption history

#### **8. AdminController**
- Dashboard stats
- Reports
- Alerts
- System overview

### Expected Outcomes:
- ✅ 8 controllers implemented
- ✅ 50+ endpoint implementations
- ✅ Consistent API response structure
- ✅ Proper error handling
- ✅ XML documentation for Swagger
- ✅ Authorization properly configured

---

## **PHASE 5: DATA VALIDATION** ✅

**Status:** ✅ Completed  
**Estimated Time:** 2-3 hours  
**Actual Time:** 2 hours  
**Completed:** 2025-11-13

### **Step 5.1: Why Fluent Validation?**

#### **Advantages over Data Annotations:**
1. ✅ **Separation of Concerns**: Validation logic separate from DTOs
2. ✅ **More Flexible**: Complex, conditional validation rules
3. ✅ **Testable**: Easy to unit test validators independently
4. ✅ **Reusable**: Share validation logic across multiple DTOs
5. ✅ **Async Support**: Async validation (database checks)
6. ✅ **Better Composition**: Combine validators easily
7. ✅ **Cleaner DTOs**: DTOs remain simple POCOs
8. ✅ **Custom Messages**: Rich, context-aware error messages
9. ✅ **Rule Chaining**: Chain multiple rules with When/Unless
10. ✅ **Industry Standard**: Widely adopted in .NET ecosystem

### **Step 5.2: Validator Organization**

```
Application/Validators/ (to be created)
├── Auth/
│   ├── LoginRequestDtoValidator.cs
│   ├── RegisterRequestDtoValidator.cs
│   └── RefreshTokenRequestDtoValidator.cs
├── Users/
│   ├── CreateUserDtoValidator.cs
│   ├── UpdateUserDtoValidator.cs
│   └── UserBalanceDtoValidator.cs
├── Roles/
│   ├── CreateRoleDtoValidator.cs
│   └── AssignRoleDtoValidator.cs
├── Events/
│   ├── CreateEventDtoValidator.cs
│   ├── UpdateEventDtoValidator.cs
│   ├── RegisterParticipantDtoValidator.cs
│   └── AwardPointsDtoValidator.cs
├── Points/
│   ├── AddPointsDtoValidator.cs
│   └── DeductPointsDtoValidator.cs
├── Products/
│   ├── CreateProductDtoValidator.cs
│   ├── ProductUpdateDtoValidator.cs
│   ├── SetPricingDtoValidator.cs
│   └── UpdateInventoryDtoValidator.cs
├── Redemptions/
│   ├── CreateRedemptionDtoValidator.cs
│   └── ApproveRedemptionDtoValidator.cs
└── Common/
    └── PaginationValidator.cs
```

### **Step 5.3: Validation Rules Library**

#### **Common Validation Rules:**

**String Validations:**
- NotEmpty() / NotNull()
- MinimumLength() / MaximumLength()
- EmailAddress()
- Matches(regex) - for patterns
- Must() - custom validation

**Numeric Validations:**
- GreaterThan() / GreaterThanOrEqualTo()
- LessThan() / LessThanOrEqualTo()
- InclusiveBetween(min, max)
- Must() - custom logic

**Date Validations:**
- GreaterThan(DateTime.Now) - future dates
- LessThan(DateTime.Now) - past dates
- Must() - custom date logic

**Conditional Validations:**
- When() - apply rule conditionally
- Unless() - inverse of When
- DependentRules() - rules that depend on others

**Async Validations:**
- MustAsync() - async database checks
- Custom async validators

#### **Example Validator:**
```csharp
public class CreateUserDtoValidator : AbstractValidator<CreateUserDto>
{
    private readonly IUserService _userService;

    public CreateUserDtoValidator(IUserService userService)
    {
        _userService = userService;

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .Length(1, 100).WithMessage("First name must be between 1 and 100 characters")
            .Matches("^[a-zA-Z ]+$").WithMessage("First name can only contain letters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .Length(1, 100).WithMessage("Last name must be between 1 and 100 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(255).WithMessage("Email cannot exceed 255 characters")
            .MustAsync(BeUniqueEmail).WithMessage("Email already exists");
    }

    private async Task<bool> BeUniqueEmail(string email, CancellationToken cancellationToken)
    {
        var existingUser = await _userService.GetUserByEmailAsync(email);
        return existingUser == null;
    }
}
```

### **Step 5.4: Validation Configuration**

```csharp
// In Program.cs
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<CreateUserDtoValidator>();
```

### **Step 5.5: Validators to Implement (25+ validators)**

1. **Auth Validators (3)**
   - LoginRequestDtoValidator
   - RegisterRequestDtoValidator
   - RefreshTokenRequestDtoValidator

2. **User Validators (3)**
   - CreateUserDtoValidator
   - UpdateUserDtoValidator
   - UserBalanceDtoValidator

3. **Role Validators (2)**
   - CreateRoleDtoValidator
   - AssignRoleDtoValidator

4. **Event Validators (4)**
   - CreateEventDtoValidator
   - UpdateEventDtoValidator
   - RegisterParticipantDtoValidator
   - AwardPointsDtoValidator

5. **Points Validators (2)**
   - AddPointsDtoValidator
   - DeductPointsDtoValidator

6. **Product Validators (4)**
   - CreateProductDtoValidator
   - ProductUpdateDtoValidator
   - SetPricingDtoValidator
   - UpdateInventoryDtoValidator

7. **Redemption Validators (2)**
   - CreateRedemptionDtoValidator
   - ApproveRedemptionDtoValidator

8. **Common Validators (1)**
   - PaginationValidator

### Expected Outcomes:
- ✅ 25+ FluentValidation validators implemented
- ✅ Comprehensive validation rules for all DTOs
- ✅ Async validation for database checks
- ✅ Custom error messages
- ✅ Automatic validation via middleware
- ✅ Testable validation logic

---

## **PHASE 6: AUTOMAPPER CONFIGURATION** 🗺️

**Status:** ✅ Completed  
**Estimated Time:** 2-3 hours  
**Actual Time:** 2.5 hours  
**Completed:** 2025-11-22

### **Step 6.1: AutoMapper Benefits**

#### **Why AutoMapper?**
1. ✅ **Reduces Boilerplate**: Eliminates manual mapping code
2. ✅ **Maintainable**: Centralized mapping configuration
3. ✅ **Type-Safe**: Compile-time checking
4. ✅ **Testable**: Easy to test mapping configurations
5. ✅ **Performance**: Optimized mapping execution
6. ✅ **Flattening**: Automatic nested object flattening
7. ✅ **Convention-Based**: Maps by convention (same property names)
8. ✅ **Extensible**: Custom resolvers and converters

### **Step 6.2: Mapping Profile Organization**

```
Application/MappingProfiles/ (to be created)
├── UserMappingProfile.cs
├── RoleMappingProfile.cs
├── EventMappingProfile.cs
├── PointsMappingProfile.cs
├── ProductMappingProfile.cs
├── RedemptionMappingProfile.cs
└── CommonMappingProfile.cs
```

### **Step 6.3: Mapping Profiles to Implement**

#### **1. UserMappingProfile**
```csharp
public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        // Entity to DTO
        CreateMap<User, UserResponseDto>();
        CreateMap<User, UserDetailsDto>()
            .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.UserRoles.Select(ur => ur.Role)));
        CreateMap<User, UserBalanceDto>()
            .ForMember(dest => dest.Balance, opt => opt.MapFrom(src => src.PointsAccount.CurrentBalance));

        // DTO to Entity
        CreateMap<CreateUserDto, User>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));
        
        CreateMap<UpdateUserDto, User>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
    }
}
```

#### **2. RoleMappingProfile**
- Role ↔ RoleResponseDto
- CreateRoleDto → Role
- UserRole ↔ UserRoleResponseDto

#### **3. EventMappingProfile**
- Event ↔ EventResponseDto
- Event ↔ EventDetailsDto (with participants)
- CreateEventDto → Event
- UpdateEventDto → Event
- EventParticipant ↔ EventParticipantResponseDto

#### **4. PointsMappingProfile**
- PointsAccount ↔ PointsAccountResponseDto
- PointsTransaction ↔ TransactionResponseDto
- AddPointsDto → PointsTransaction
- DeductPointsDto → PointsTransaction

#### **5. ProductMappingProfile**
- Product ↔ ProductResponseDto
- Product ↔ ProductDetailsDto (with pricing & inventory)
- CreateProductDto → Product
- ProductUpdateDto → Product
- ProductPricing ↔ PricingDto
- InventoryItem ↔ InventoryResponseDto

#### **6. RedemptionMappingProfile**
- Redemption ↔ RedemptionResponseDto
- Redemption ↔ RedemptionDetailsDto (with product & user)
- CreateRedemptionDto → Redemption

#### **7. CommonMappingProfile**
- Pagination mappings
- Common type conversions

### **Step 6.4: Advanced Mapping Scenarios**

#### **Custom Value Resolvers:**
```csharp
public class PointsBalanceResolver : IValueResolver<User, UserBalanceDto, int>
{
    public int Resolve(User source, UserBalanceDto destination, int destMember, ResolutionContext context)
    {
        return source.PointsAccount?.CurrentBalance ?? 0;
    }
}

// Usage in profile:
CreateMap<User, UserBalanceDto>()
    .ForMember(dest => dest.Balance, opt => opt.MapFrom<PointsBalanceResolver>());
```

#### **Conditional Mapping:**
```csharp
CreateMap<UpdateUserDto, User>()
    .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
```

#### **Nested Object Mapping:**
```csharp
CreateMap<Redemption, RedemptionDetailsDto>()
    .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
    .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User.Email));
```

### **Step 6.5: Configuration**

```csharp
// In Program.cs
builder.Services.AddAutoMapper(typeof(Program).Assembly);

// Or specify assemblies
builder.Services.AddAutoMapper(cfg => {
    cfg.AddMaps(typeof(UserMappingProfile).Assembly);
});
```

### Expected Outcomes:
- ✅ 7 mapping profiles implemented
- ✅ 50+ entity-DTO mappings configured
- ✅ Custom resolvers for complex scenarios
- ✅ Bidirectional mappings (Entity ↔ DTO)
- ✅ Flattening of nested objects
- ✅ Null-safe mappings

---

## **PHASE 7: AUTHENTICATION & AUTHORIZATION** 🔐

**Status:** ✅ Completed  
**Estimated Time:** 3-4 hours  
**Actual Time:** 3 hours  
**Completed:** 2025-11-22

### **Step 7.1: Why JWT Authentication?**

#### **JWT Advantages:**
1. ✅ **Stateless**: No server-side session storage required
2. ✅ **Scalable**: Perfect for distributed/microservices architecture
3. ✅ **Secure**: Cryptographically signed and optionally encrypted
4. ✅ **Cross-Platform**: Works with web, mobile, desktop clients
5. ✅ **Self-Contained**: Token contains all user claims
6. ✅ **Industry Standard**: RFC 7519, widely adopted
7. ✅ **RESTful**: Aligns with stateless REST principles
8. ✅ **Performance**: No database lookup for authentication
9. ✅ **Flexibility**: Works across domains
10. ✅ **Mobile-Friendly**: Easy to implement in mobile apps

#### **JWT vs Other Authentication Methods:**

| Feature | JWT | Session Cookies | OAuth2 |
|---------|-----|----------------|--------|
| Stateless | ✅ Yes | ❌ No | ✅ Yes |
| Scalability | ✅ Excellent | ⚠️ Limited | ✅ Excellent |
| Mobile Support | ✅ Native | ⚠️ Challenging | ✅ Native |
| Cross-Domain | ✅ Easy | ⚠️ CORS issues | ✅ Easy |
| Setup Complexity | ⚠️ Moderate | ✅ Simple | ❌ Complex |
| Token Refresh | ✅ Built-in | N/A | ✅ Built-in |
| Best For | APIs | Traditional web | Third-party auth |

### **Step 7.2: JWT Implementation**

#### **JWT Settings Configuration:**
```json
{
  "JwtSettings": {
    "SecretKey": "your-super-secret-key-min-256-bits-long-for-production",
    "Issuer": "RewardPointsAPI",
    "Audience": "RewardPointsClient",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  }
}
```

#### **JWT Settings Class:**
```csharp
public class JwtSettings
{
    public string SecretKey { get; set; }
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public int AccessTokenExpirationMinutes { get; set; }
    public int RefreshTokenExpirationDays { get; set; }
}
```

### **Step 7.3: Token Service Implementation**

#### **ITokenService Interface:**
```csharp
public interface ITokenService
{
    string GenerateAccessToken(User user, IEnumerable<Role> roles);
    string GenerateRefreshToken();
    ClaimsPrincipal ValidateToken(string token);
    Task<string> RefreshAccessTokenAsync(string refreshToken);
}
```

#### **TokenService Implementation:**
```csharp
public class TokenService : ITokenService
{
    private readonly JwtSettings _jwtSettings;
    private readonly IUserService _userService;

    public string GenerateAccessToken(User user, IEnumerable<Role> roles)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role.Name));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    // Additional methods...
}
```

### **Step 7.4: Authentication Middleware Configuration**

```csharp
// In Program.cs
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
builder.Services.AddSingleton(jwtSettings);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

// Add middleware
app.UseAuthentication();
app.UseAuthorization();
```

### **Step 7.5: Authorization Policies**

#### **Role-Based Authorization:**
```csharp
[Authorize] // Requires any authenticated user
[Authorize(Roles = "Admin")] // Admin only
[Authorize(Roles = "Admin,Employee")] // Admin OR Employee
[AllowAnonymous] // Public endpoint
```

#### **Policy-Based Authorization:**
```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("EmployeeOrAdmin", policy => policy.RequireRole("Admin", "Employee"));
    options.AddPolicy("CanAwardPoints", policy => 
        policy.RequireRole("Admin").RequireClaim("Permission", "AwardPoints"));
});

// Usage:
[Authorize(Policy = "AdminOnly")]
public async Task<IActionResult> GetAdminDashboard() { }
```

### **Step 7.6: Refresh Token Implementation**

#### **Refresh Token Flow:**
1. User logs in → receives access token + refresh token
2. Access token expires (15 min)
3. Client sends refresh token to /auth/refresh
4. Server validates refresh token
5. Server issues new access token + refresh token
6. Old refresh token is invalidated

#### **Refresh Token Storage:**
```csharp
// Add to User entity or create RefreshToken entity
public class RefreshToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Token { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsRevoked { get; set; }
    public string CreatedByIp { get; set; }
    
    public User User { get; set; }
}
```

### **Step 7.7: Security Best Practices**

1. **Token Storage (Client-Side):**
   - ✅ Store in memory or secure storage
   - ⚠️ Avoid localStorage (XSS vulnerable)
   - ✅ Consider httpOnly cookies for refresh tokens

2. **Token Expiration:**
   - Access Token: 15 minutes (short-lived)
   - Refresh Token: 7 days (can be revoked)

3. **Token Revocation:**
   - Implement refresh token blacklist
   - Store refresh tokens in database
   - Mark as revoked on logout

4. **HTTPS Only:**
   - Always use HTTPS in production
   - Tokens should never be sent over HTTP

5. **Rate Limiting:**
   - Limit login attempts
   - Limit refresh token requests

### Expected Outcomes:
- ✅ JWT authentication fully configured
- ✅ Access + refresh token mechanism
- ✅ Role-based authorization
- ✅ Policy-based authorization
- ✅ Token service implementation

---

## **PHASE 8: SWAGGER/OPENAPI DOCUMENTATION** 

**Status:** Completed  
**Estimated Time:** 2-3 hours  
**Actual Time:** 1 hour  
**Completed:** 2025-11-22

### **Step 8.1: Swagger/OpenAPI Setup**

#### **Package Installation:**
```xml
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="8.0.0" />
```

### **Step 8.2: Swagger Configuration**

```csharp
// In Program.cs
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Reward Points System API",
        Description = "Production-grade reward points management API with JWT authentication",
        Contact = new OpenApiContact
        {
            Name = "Development Team",
            Email = "dev@rewardpoints.com"
        },
        License = new OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    // JWT Authentication in Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // XML Documentation
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);

    // Custom operation filters
    options.OperationFilter<SwaggerDefaultValues>();
});

// Enable Swagger middleware
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Reward Points API v1");
    options.RoutePrefix = string.Empty; // Swagger at root
    options.DocumentTitle = "Reward Points API Documentation";
});
```

### **Step 8.3: XML Documentation**

#### **Enable XML Documentation in csproj:**
```xml
<PropertyGroup>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <NoWarn>$(NoWarn);1591</NoWarn>
</PropertyGroup>
```

#### **XML Documentation Examples:**
```csharp
/// <summary>
/// Get all users with pagination (Admin only)
/// </summary>
/// <param name="page">Page number (default: 1)</param>
/// <param name="pageSize">Items per page (default: 10)</param>
/// <returns>Paginated list of users</returns>
/// <response code="200">Returns paginated user list</response>
/// <response code="401">User is not authenticated</response>
/// <response code="403">User lacks admin privileges</response>
[HttpGet]
[Authorize(Roles = "Admin")]
[ProducesResponseType(typeof(PagedResponse<UserResponseDto>), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
public async Task<IActionResult> GetAllUsers(
    [FromQuery] int page = 1, 
    [FromQuery] int pageSize = 10)
{
    // Implementation
}
```

### **Step 8.4: Swagger Customizations**

#### **Custom Operation Filter:**
```csharp
public class SwaggerDefaultValues : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var apiDescription = context.ApiDescription;
        
        operation.Deprecated |= apiDescription.IsDeprecated();

        foreach (var responseType in context.ApiDescription.SupportedResponseTypes)
        {
            var responseKey = responseType.IsDefaultResponse 
                ? "default" 
                : responseType.StatusCode.ToString();
            
            var response = operation.Responses[responseKey];

            foreach (var contentType in response.Content.Keys)
            {
                if (responseType.ApiResponseFormats.All(x => x.MediaType != contentType))
                {
                    response.Content.Remove(contentType);
                }
            }
        }

        if (operation.Parameters == null)
            return;

        foreach (var parameter in operation.Parameters)
        {
            var description = apiDescription.ParameterDescriptions
                .First(p => p.Name == parameter.Name);

            parameter.Description ??= description.ModelMetadata?.Description;

            if (parameter.Schema.Default == null && 
                description.DefaultValue != null)
            {
                parameter.Schema.Default = new OpenApiString(
                    description.DefaultValue.ToString());
            }

            parameter.Required |= description.IsRequired;
        }
    }
}
```

### **Step 8.5: API Documentation Structure**

#### **Endpoint Documentation Checklist:**
- [ ] Summary describing the endpoint
- [ ] Detailed description of functionality
- [ ] Parameter descriptions
- [ ] Response type documentation
- [ ] Status code documentation
- [ ] Authorization requirements
- [ ] Request/response examples
- [ ] Tags for grouping

#### **Example Complete Documentation:**
```csharp
/// <summary>
/// Creates a new event for points distribution
/// </summary>
/// <remarks>
/// Sample request:
/// 
///     POST /api/v1/events
///     {
///         "name": "Q4 Sales Competition",
///         "description": "Top sales performers earn reward points",
///         "eventDate": "2024-12-31T23:59:59Z",
///         "totalPointsPool": 10000
///     }
/// 
/// </remarks>
/// <param name="createEventDto">Event creation data</param>
/// <returns>Created event details</returns>
/// <response code="201">Event created successfully</response>
/// <response code="400">Invalid event data</response>
/// <response code="401">User not authenticated</response>
/// <response code="403">User lacks admin privileges</response>
/// <response code="422">Validation failed</response>
[HttpPost]
[Authorize(Roles = "Admin")]
[ProducesResponseType(typeof(ApiResponse<EventResponseDto>), StatusCodes.Status201Created)]
[ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
[ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status422UnprocessableEntity)]
public async Task<IActionResult> CreateEvent([FromBody] CreateEventDto createEventDto)
{
    // Implementation
}
```

### **Step 8.6: Swagger UI Enhancements**

#### **Features to Implement:**
1. ✅ JWT token input field
2. ✅ Try-it-out functionality
3. ✅ Request/response examples
4. ✅ Schema documentation
5. ✅ Grouped endpoints by controller
6. ✅ Model definitions
7. ✅ Download OpenAPI spec

### Expected Outcomes:
- ✅ Swagger UI accessible at root URL
- ✅ JWT authentication integrated
- ✅ All endpoints documented
- ✅ XML comments for all public APIs
- ✅ Request/response examples
- ✅ Interactive API testing
- ✅ OpenAPI specification generated

---

## **PHASE 9: TESTING & VALIDATION** 🧪

**Status:** 🔄 Pending  
**Estimated Time:** 2-3 hours

### **Step 9.1: Swagger UI Testing**

#### **Testing Workflow:**
1. **Authentication Flow**
   - Register new user
   - Login with credentials
   - Copy JWT token
   - Click "Authorize" button
   - Paste token (with "Bearer " prefix)
   - Test protected endpoints

2. **CRUD Operations Testing**
   - Create resources (POST)
   - Read resources (GET)
   - Update resources (PUT/PATCH)
   - Delete resources (DELETE)

3. **Authorization Testing**
   - Test admin-only endpoints
   - Test employee endpoints
   - Verify 403 Forbidden responses

4. **Validation Testing**
   - Test with invalid data
   - Verify validation errors
   - Check error messages

5. **Edge Cases**
   - Empty requests
   - Null values
   - Out-of-range values
   - Duplicate entries

### **Step 9.2: Postman Collection Setup**

#### **Collection Structure:**
```
Reward Points API.postman_collection.json
├── Environment Variables
│   ├── {{baseUrl}} = http://localhost:5000
│   ├── {{accessToken}} = (auto-set after login)
│   └── {{userId}} = (auto-set after user creation)
├── Auth
│   ├── Register
│   ├── Login (sets accessToken variable)
│   ├── Refresh Token
│   └── Get Current User
├── Users
│   ├── Get All Users
│   ├── Get User By ID
│   ├── Create User
│   ├── Update User
│   └── Delete User
├── Roles
│   ├── Get All Roles
│   ├── Create Role
│   └── Assign Role
├── Events
│   ├── Get All Events
│   ├── Create Event
│   ├── Activate Event
│   └── Award Points
├── Products
│   ├── Get All Products
│   ├── Create Product
│   ├── Update Pricing
│   └── Update Inventory
├── Redemptions
│   ├── Create Redemption
│   ├── Approve Redemption
│   └── Get My Redemptions
└── Admin
    ├── Dashboard Stats
    └── Reports
```

#### **Postman Pre-request Script (for auth):**
```javascript
// Auto-attach Bearer token
pm.request.headers.add({
    key: 'Authorization',
    value: 'Bearer ' + pm.environment.get('accessToken')
});
```

#### **Postman Test Scripts:**
```javascript
// After login request
pm.test("Status code is 200", function () {
    pm.response.to.have.status(200);
});

pm.test("Response has access token", function () {
    var jsonData = pm.response.json();
    pm.expect(jsonData.data.accessToken).to.exist;
    pm.environment.set("accessToken", jsonData.data.accessToken);
});

pm.test("Response time is less than 500ms", function () {
    pm.expect(pm.response.responseTime).to.be.below(500);
});
```

### **Step 9.3: Test Scenarios**

#### **1. Authentication Test Suite**
- ✅ Register with valid data
- ✅ Register with duplicate email (should fail)
- ✅ Login with valid credentials
- ✅ Login with invalid credentials (should fail)
- ✅ Refresh token with valid token
- ✅ Refresh token with expired token (should fail)
- ✅ Access protected endpoint without token (should fail)

#### **2. User Management Test Suite**
- ✅ Create user as admin
- ✅ Create user as non-admin (should fail)
- ✅ Get user by ID
- ✅ Get non-existent user (should return 404)
- ✅ Update user details
- ✅ Delete user as admin
- ✅ Get user balance

#### **3. Event Management Test Suite**
- ✅ Create event with valid data
- ✅ Create event with past date (should fail)
- ✅ Activate event
- ✅ Register participant
- ✅ Award points to participant
- ✅ Complete event
- ✅ Cancel event

#### **4. Product Catalog Test Suite**
- ✅ Create product
- ✅ Update product details
- ✅ Set product pricing
- ✅ Update inventory
- ✅ Get products with pagination
- ✅ Search products

#### **5. Redemption Test Suite**
- ✅ Create redemption with sufficient balance
- ✅ Create redemption with insufficient balance (should fail)
- ✅ Create redemption for out-of-stock product (should fail)
- ✅ Approve redemption as admin
- ✅ Deliver redemption
- ✅ Cancel redemption (refund points)

#### **6. Error Handling Test Suite**
- ✅ 400 Bad Request (validation errors)
- ✅ 401 Unauthorized (no token)
- ✅ 403 Forbidden (insufficient permissions)
- ✅ 404 Not Found (resource doesn't exist)
- ✅ 409 Conflict (duplicate resource)
- ✅ 422 Unprocessable Entity (semantic errors)
- ✅ 500 Internal Server Error

### **Step 9.4: Performance Testing**

#### **Metrics to Monitor:**
- Response time (target: < 500ms)
- Concurrent users (target: 100+)
- Requests per second
- Memory usage
- Database query performance

### Expected Outcomes:
- ✅ All endpoints tested via Swagger
- ✅ Postman collection created with 50+ requests
- ✅ Authentication flow validated
- ✅ Authorization policies verified
- ✅ Error handling confirmed
- ✅ Edge cases tested
- ✅ Performance benchmarks established

---

## **PHASE 10: ERROR HANDLING & MIDDLEWARE** 🛡️

**Status:** 🔄 Pending  
**Estimated Time:** 2-3 hours

### **Step 10.1: Global Exception Handler**

#### **Exception Middleware Implementation:**
```csharp
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message) = exception switch
        {
            DomainException domainEx => (StatusCodes.Status400BadRequest, domainEx.Message),
            InvalidOperationException => (StatusCodes.Status400BadRequest, exception.Message),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized access"),
            ValidationException => (StatusCodes.Status422UnprocessableEntity, exception.Message),
            _ => (StatusCodes.Status500InternalServerError, "An internal error occurred")
        };

        context.Response.StatusCode = statusCode;

        var response = new ErrorResponse
        {
            Success = false,
            Message = message,
            StatusCode = statusCode,
            Timestamp = DateTime.UtcNow,
            Path = context.Request.Path
        };

        return context.Response.WriteAsJsonAsync(response);
    }
}
```

### **Step 10.2: Domain Exception Mapping**

#### **Exception to HTTP Status Code Mapping:**
```csharp
public static class ExceptionMapper
{
    private static readonly Dictionary<Type, int> ExceptionStatusCodes = new()
    {
        // Domain Exceptions
        { typeof(DomainException), StatusCodes.Status400BadRequest },
        { typeof(UserNotFoundException), StatusCodes.Status404NotFound },
        { typeof(DuplicateUserException), StatusCodes.Status409Conflict },
        { typeof(InvalidEventStateException), StatusCodes.Status400BadRequest },
        { typeof(InsufficientPointsException), StatusCodes.Status400BadRequest },
        { typeof(InsufficientStockException), StatusCodes.Status400BadRequest },
        { typeof(ProductNotFoundException), StatusCodes.Status404NotFound },
        
        // Standard Exceptions
        { typeof(ValidationException), StatusCodes.Status422UnprocessableEntity },
        { typeof(KeyNotFoundException), StatusCodes.Status404NotFound },
        { typeof(InvalidOperationException), StatusCodes.Status400BadRequest },
        { typeof(UnauthorizedAccessException), StatusCodes.Status401Unauthorized },
        { typeof(ArgumentException), StatusCodes.Status400BadRequest },
        { typeof(ArgumentNullException), StatusCodes.Status400BadRequest }
    };

    public static int GetStatusCode(Exception exception)
    {
        var exceptionType = exception.GetType();
        return ExceptionStatusCodes.GetValueOrDefault(
            exceptionType, 
            StatusCodes.Status500InternalServerError);
    }
}
```

### **Step 10.3: Additional Middleware**

#### **1. Request Logging Middleware**
```csharp
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public async Task InvokeAsync(HttpContext context)
    {
        var requestId = Guid.NewGuid().ToString();
        context.Items["RequestId"] = requestId;

        _logger.LogInformation(
            "Request {RequestId}: {Method} {Path} started at {Time}",
            requestId,
            context.Request.Method,
            context.Request.Path,
            DateTime.UtcNow);

        var sw = Stopwatch.StartNew();
        await _next(context);
        sw.Stop();

        _logger.LogInformation(
            "Request {RequestId}: {Method} {Path} completed with {StatusCode} in {ElapsedMs}ms",
            requestId,
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            sw.ElapsedMilliseconds);
    }
}
```

#### **2. CORS Configuration**
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins(
                builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>())
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Apply middleware
app.UseCors("AllowSpecificOrigins");
```

#### **3. Rate Limiting (Optional)**
```csharp
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("fixed", options =>
    {
        options.PermitLimit = 100;
        options.Window = TimeSpan.FromMinutes(1);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 5;
    });
});

app.UseRateLimiter();
```

#### **4. Response Compression**
```csharp
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<GzipCompressionProvider>();
    options.Providers.Add<BrotliCompressionProvider>();
});

app.UseResponseCompression();
```

### **Step 10.4: Middleware Pipeline Order**

```csharp
var app = builder.Build();

// 1. Exception handling (first!)
app.UseMiddleware<ExceptionHandlingMiddleware>();

// 2. Request logging
app.UseMiddleware<RequestLoggingMiddleware>();

// 3. HTTPS redirection
app.UseHttpsRedirection();

// 4. Response compression
app.UseResponseCompression();

// 5. CORS
app.UseCors("AllowSpecificOrigins");

// 6. Rate limiting
app.UseRateLimiter();

// 7. Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// 8. Swagger (development only)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 9. Map controllers
app.MapControllers();

app.Run();
```

### **Step 10.5: Error Response Models**

```csharp
public class ErrorResponse
{
    public bool Success { get; set; } = false;
    public string Message { get; set; }
    public int StatusCode { get; set; }
    public DateTime Timestamp { get; set; }
    public string Path { get; set; }
    public string TraceId { get; set; }
}

public class ValidationErrorResponse : ErrorResponse
{
    public Dictionary<string, string[]> Errors { get; set; }
}
```

### Expected Outcomes:
- ✅ Global exception handler implemented
- ✅ Domain exceptions mapped to HTTP status codes
- ✅ Consistent error responses
- ✅ Request/response logging
- ✅ CORS configured
- ✅ Rate limiting (optional)
- ✅ Response compression
- ✅ Proper middleware pipeline order

---

## 🎯 FINAL EXPECTED OUTCOMES

After completing all 10 phases, the system will have:

### **Architecture & Structure**
✅ ASP.NET Core Web API with Clean Architecture  
✅ 4-layer design (API → Application → Domain → Infrastructure)  
✅ Proper separation of concerns  
✅ Dependency injection throughout  

### **DTOs & Mapping**
✅ 40+ comprehensive DTOs covering all domains  
✅ Organized DTO structure by domain  
✅ Request/Response DTO separation  
✅ 7 AutoMapper profiles with 50+ mappings  

### **API Endpoints**
✅ 50+ RESTful endpoints  
✅ Consistent URL structure (/api/v1/)  
✅ 8 controllers (Auth, Users, Roles, Events, Points, Products, Redemptions, Admin)  
✅ Proper HTTP verb usage  
✅ API versioning support  

### **Validation**
✅ FluentValidation for all input DTOs  
✅ 25+ validators with comprehensive rules  
✅ Async validation for database checks  
✅ Custom validation messages  
✅ Conditional validation rules  

### **Authentication & Authorization**
✅ JWT-based authentication  
✅ Access token + refresh token mechanism  
✅ Role-based authorization (Admin, Employee)  
✅ Policy-based authorization  
✅ Secure token handling  
✅ Token expiration & refresh strategy  

### **Documentation**
✅ Swagger/OpenAPI integration  
✅ Interactive API documentation  
✅ JWT authentication in Swagger UI  
✅ XML documentation for all endpoints  
✅ Request/response examples  

### **Testing**
✅ Swagger UI testing capability  
✅ Postman collection with 50+ requests  
✅ Pre-configured environment variables  
✅ Test scripts for automation  
✅ Complete test coverage of all endpoints  

### **Error Handling & Middleware**
✅ Global exception handler  
✅ Consistent error responses  
✅ Domain exception mapping  
✅ Request/response logging  
✅ CORS configuration  
✅ Rate limiting (optional)  
✅ Response compression  

### **Security**
✅ HTTPS enforcement  
✅ JWT secret key management  
✅ Role-based access control  
✅ Token expiration policies  
✅ Refresh token revocation  
✅ Secure password handling  

### **Performance**
✅ Async/await throughout  
✅ Response compression  
✅ Efficient AutoMapper configuration  
✅ Pagination for large datasets  
✅ Optimized database queries  

### **Production Readiness**
✅ Configuration management (appsettings.json)  
✅ Environment-specific settings  
✅ Logging infrastructure  
✅ Error tracking  
✅ Health checks (optional)  
✅ Deployment-ready architecture  

---

## ⏱️ ESTIMATED TIMELINE

| Phase | Tasks | Estimated Time | Difficulty |
|-------|-------|----------------|------------|
| **Phase 1** | Project Setup & Transformation | 1-2 hours | ⭐⭐ |
| **Phase 2** | Create Comprehensive DTOs | 2-3 hours | ⭐⭐ |
| **Phase 3** | API Endpoints Identification | 2-3 hours | ⭐⭐⭐ |
| **Phase 4** | Implement API Controllers | 4-6 hours | ⭐⭐⭐⭐ |
| **Phase 5** | Data Validation (FluentValidation) | 2-3 hours | ⭐⭐⭐ |
| **Phase 6** | AutoMapper Configuration | 2-3 hours | ⭐⭐⭐ |
| **Phase 7** | Authentication & Authorization | 3-4 hours | ⭐⭐⭐⭐ |
| **Phase 8** | Swagger Documentation | 1-2 hours | ⭐⭐ |
| **Phase 9** | Testing & Validation | 2-3 hours | ⭐⭐⭐ |
| **Phase 10** | Error Handling & Middleware | 2-3 hours | ⭐⭐⭐ |

**Total Estimated Time: 21-32 hours**

### Timeline Breakdown:
- **Week 1** (8-10 hours): Phases 1-2 (Foundation)
- **Week 2** (8-10 hours): Phases 3-5 (Core API)
- **Week 3** (5-12 hours): Phases 6-8 (Features & Auth)

---

## 📝 PROGRESS TRACKING

### Phase Completion Checklist

- [x] **Phase 1**: Project Transformation ✅
- [x] **Phase 2**: DTOs Created ✅
- [x] **Phase 3**: API Endpoints Defined ✅
- [x] **Phase 4**: Controllers Implemented ✅
- [x] **Phase 5**: Validation Configured ✅
- [x] **Phase 6**: AutoMapper Setup ✅
- [x] **Phase 7**: Auth & Authorization ✅
- [x] **Phase 8**: Swagger Documentation ✅
- [ ] **Phase 9**: Testing Complete
- [ ] **Phase 10**: Middleware & Error Handling

---

## 🚀 NEXT STEPS AFTER COMPLETION

1. **Deploy to Cloud**
   - Azure App Service
   - AWS Elastic Beanstalk
   - Docker containerization

2. **Add Advanced Features**
   - Email notifications (SendGrid, MailKit)
   - Background jobs (Hangfire, Quartz)
   - Caching (Redis)
   - Real-time updates (SignalR)

3. **Enhance Security**
   - Two-factor authentication
   - OAuth2 integration (Google, Microsoft)
   - API key management
   - Audit logging

4. **Monitoring & Observability**
   - Application Insights
   - Serilog structured logging
   - Health checks endpoint
   - Performance monitoring

5. **CI/CD Pipeline**
   - GitHub Actions
   - Azure DevOps
   - Automated testing
   - Automated deployment

---

## 📚 RESOURCES & REFERENCES

### Documentation
- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [JWT Best Practices](https://tools.ietf.org/html/rfc8725)
- [RESTful API Design](https://restfulapi.net/)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)

### Libraries
- [FluentValidation](https://docs.fluentvalidation.net/)
- [AutoMapper](https://docs.automapper.org/)
- [Swashbuckle](https://github.com/domaindrivendev/Swashbuckle.AspNetCore)
- [JWT Bearer](https://www.nuget.org/packages/Microsoft.AspNetCore.Authentication.JwtBearer/)

---

**Last Updated:** 2025-11-22  
**Version:** 2.2  
**Status:** Phase 1-8 Completed ✅  
**Build Status:** ✅ SUCCESS  
**Next Phase:** Phase 9 - Testing & Deployment  
**Ready for Demo:** ✅ YES
