# Reward Points System API - Issues Fixed & User Guide 🔧

**Date:** November 22, 2025  
**Version:** 1.0  
**Status:** ✅ All Major Issues Resolved

---

## 📋 Issues Identified & Fixed

### **Issue 1: Role Not Found ❌**

**Problem:** When registering a new user, the system tried to assign an "Employee" role, but no roles existed in the database.

**Root Cause:** No database seeding for initial roles.

**Solution Implemented:** ✅
1. Created `DatabaseSeeder.cs` that automatically seeds:
   - **Admin** role - System Administrator with full access
   - **Employee** role - Regular employee with limited access
   - **Manager** role - Manager with elevated access
   - **Product Categories** - 7 default categories (Electronics, Office Supplies, Gift Cards, etc.)

2. Updated `Program.cs` to run the seeder on application startup.

3. **Result:** Roles are now automatically created when the API starts for the first time.

---

###**Issue 2: 401 Unauthorized When Creating Products ❌**

**Problem:** User got 401 Unauthorized error when trying to create a product.

**Root Causes:**
1. **Missing "Bearer" prefix** in Authorization header
2. **Insufficient permissions** - Product creation requires Admin role, but registered user has Employee role

**Solution:** ✅

#### **Fix 1: Authorization Header Format**
In Swagger, you must include "Bearer" prefix:

**❌ WRONG:**
```
Authorization: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**✅ CORRECT:**
```
Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**In Swagger UI:**
1. Click the "Authorize" button (🔒)
2. Enter: `Bearer <your-access-token>`
3. Click "Authorize"
4. Click "Close"

#### **Fix 2: Admin User Creation**
Since product creation requires Admin role, you need an admin user.

**Option A: Manually Assign Admin Role** (Recommended for first user)

Run this SQL in your database:
```sql
-- Step 1: Find the user ID and Admin role ID
SELECT Id, Email, FirstName FROM Users WHERE Email = 'harshal.behare@agdata.com';
SELECT Id, Name FROM Roles WHERE Name = 'Admin';

-- Step 2: Assign Admin role (replace GUIDs with actual values from above)
INSERT INTO UserRoles (Id, UserId, RoleId, AssignedBy, AssignedAt, IsActive, IsRevoked)
VALUES (
    NEWID(),  -- Random GUID for UserRole ID
    'YOUR_USER_ID_FROM_STEP_1',
    'ADMIN_ROLE_ID_FROM_STEP_1',
    'YOUR_USER_ID_FROM_STEP_1',  -- Self-assigned
    GETUTCDATE(),
    1,  -- IsActive
    0   -- IsRevoked
);
```

**Option B: Register as Admin via API** (For subsequent admins)
Once you have one admin user, they can assign admin roles to others via the API.

---

### **Issue 3: Product and ProductCategory Relationship ❌**

**Problem:** Product creation didn't properly use ProductCategory relationship.

**Solution Implemented:** ✅

1. **Updated `CreateProductDto`:**
```json
{
  "name": "Wireless Headphones",
  "description": "Premium noise-canceling headphones",
  "categoryId": "guid-of-category",  // ✅ Now uses CategoryId instead of string
  "imageUrl": "https://example.com/headphones.jpg",
  "pointsPrice": 500,                 // ✅ Added
  "stockQuantity": 100                // ✅ Added
}
```

2. **Product creation now automatically:**
   - Links to ProductCategory via `CategoryId`
   - Creates ProductPricing record
   - Creates InventoryItem record
   - All in one transaction

3. **CategoryId is optional** - Can be null for uncategorized products

---

### **Issue 4: Incorrect Data Validation ❌**

**Problem:** Some validators had async rules that don't work with ASP.NET's validation pipeline.

**Solution Implemented:** ✅

1. Removed async email uniqueness check from validators
2. Email uniqueness is still validated in the `AuthController`
3. All validators now use synchronous rules only

**Fixed Validators:**
- ✅ `RegisterRequestDtoValidator`
- ✅ `CreateUserDtoValidator`
- ✅ `CreateProductDtoValidator`

---

## 🚀 How To Use The API Now

### **Step 1: Start the API**

```bash
cd RewardPointsSystem.Api
dotnet run
```

**Expected Output:**
```
========================================
  Reward Points System API
========================================
Environment: Development
Swagger UI: http://localhost:5000
========================================
Database seeding completed successfully
```

---

### **Step 2: Register a User**

**Endpoint:** `POST /api/v1/auth/register`

**Request:**
```json
{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@company.com",
  "password": "SecurePass@123",
  "confirmPassword": "SecurePass@123"
}
```

**Password Requirements:**
- ✅ Minimum 8 characters
- ✅ At least one uppercase letter
- ✅ At least one lowercase letter
- ✅ At least one number
- ✅ At least one special character

**Response:**
```json
{
  "success": true,
  "message": "User registered successfully",
  "data": {
    "userId": "...",
    "email": "john.doe@company.com",
    "firstName": "John",
    "lastName": "Doe",
    "accessToken": "eyJhbGci...",
    "refreshToken": "...",
    "expiresAt": "2025-11-22T...",
    "roles": ["Employee"]
  }
}
```

**🔑 COPY THE ACCESS TOKEN!**

---

### **Step 3: Authorize in Swagger**

1. **Click "Authorize" button** at the top right
2. **Enter:** `Bearer <paste-your-access-token-here>`
3. **Click "Authorize"**
4. **Click "Close"**

**✅ You're now authenticated!** The lock icons (🔒) should be closed.

---

### **Step 4: Upgrade to Admin (First Time Setup)**

Run this SQL query to make yourself an admin:

```sql
-- Get your user and admin role IDs
DECLARE @UserId UNIQUEIDENTIFIER = (SELECT Id FROM Users WHERE Email = 'YOUR_EMAIL');
DECLARE @AdminRoleId UNIQUEIDENTIFIER = (SELECT Id FROM Roles WHERE Name = 'Admin');
DECLARE @Now DATETIME2 = GETUTCDATE();

-- Assign Admin role
INSERT INTO UserRoles (Id, UserId, RoleId, AssignedBy, AssignedAt, IsActive, IsRevoked)
VALUES (NEWID(), @UserId, @AdminRoleId, @UserId, @Now, 1, 0);
```

**Important:** After assigning the role, you need to **logout and login again** to get a new token with the Admin role.

---

### **Step 5: Get Product Categories**

**Endpoint:** `GET /api/v1/product-categories` (if you created this controller, otherwise use the seeded category IDs from database)

**Seeded Categories:**
- Electronics
- Office Supplies
- Gift Cards
- Apparel
- Home & Living
- Health & Wellness
- Books & Media

Query the database to get category IDs:
```sql
SELECT Id, Name, Description FROM ProductCategories;
```

---

### **Step 6: Create a Product (Admin Only)**

**Endpoint:** `POST /api/v1/products`

**Request:**
```json
{
  "name": "Wireless Mouse",
  "description": "Ergonomic wireless mouse with precision tracking",
  "categoryId": "GUID-FROM-PRODUCTCATEGORIES-TABLE",
  "imageUrl": "https://example.com/images/mouse.jpg",
  "pointsPrice": 250,
  "stockQuantity": 50
}
```

**Response:**
```json
{
  "success": true,
  "message": "Product created successfully",
  "data": {
    "id": "...",
    "name": "Wireless Mouse",
    "description": "Ergonomic wireless mouse with precision tracking",
    "categoryId": "...",
    "categoryName": "Electronics",
    "imageUrl": "https://example.com/images/mouse.jpg",
    "currentPointsCost": 250,
    "isActive": true,
    "isInStock": true,
    "stockQuantity": 50,
    "createdAt": "2025-11-22T..."
  }
}
```

---

### **Step 7: View Products**

**Endpoint:** `GET /api/v1/products`

**No authentication required for viewing!**

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": "...",
      "name": "Wireless Mouse",
      "description": "...",
      "categoryId": "...",
      "categoryName": "Electronics",
      "currentPointsCost": 250,
      "isActive": true,
      "isInStock": true,
      "stockQuantity": 50
    }
  ]
}
```

---

## 📊 Database Relationships Fixed

### **Product ↔ ProductCategory**
```
Product.CategoryId → ProductCategory.Id
Product.ProductCategory (navigation property)
ProductCategory.Products (collection navigation property)
```

### **Product ↔ ProductPricing**
```
ProductPricing.ProductId → Product.Id
Product.PricingHistory (collection)
```

### **Product ↔ InventoryItem**
```
InventoryItem.ProductId → Product.Id (One-to-One)
Product.Inventory (navigation property)
```

### **User ↔ Role (Many-to-Many)**
```
User → UserRoles ← Role
UserRole.UserId → User.Id
UserRole.RoleId → Role.Id
```

---

## 🔐 Authentication & Authorization Summary

### **Token Usage:**
- **Access Token:** Valid for 15 minutes, used for API requests
- **Refresh Token:** Valid for 7 days, used to get new access tokens

### **Authorization Levels:**

| Endpoint | Authentication | Authorization |
|----------|----------------|---------------|
| `POST /auth/register` | ❌ No | - |
| `POST /auth/login` | ❌ No | - |
| `GET /auth/me` | ✅ Yes | Any authenticated user |
| `POST /products` | ✅ Yes | **Admin** only |
| `GET /products` | ❌ No | - |
| `POST /events` | ✅ Yes | **Admin** only |
| `GET /points/balance` | ✅ Yes | Authenticated user (own balance) |
| `POST /redemptions` | ✅ Yes | Authenticated user |

### **Roles:**
- **Employee:** Can view products, check their points, redeem products
- **Admin:** Full access, can create products, events, manage users
- **Manager:** Elevated access (for future features)

---

## 🧪 Complete Test Flow

### **1. Register User**
```bash
POST /api/v1/auth/register
{
  "email": "test@company.com",
  "password": "Test@123",
  "confirmPassword": "Test@123",
  "firstName": "Test",
  "lastName": "User"
}
```

### **2. Make User Admin (SQL)**
```sql
DECLARE @UserId UNIQUEIDENTIFIER = (SELECT Id FROM Users WHERE Email = 'test@company.com');
DECLARE @AdminRoleId UNIQUEIDENTIFIER = (SELECT Id FROM Roles WHERE Name = 'Admin');
INSERT INTO UserRoles (Id, UserId, RoleId, AssignedBy, AssignedAt, IsActive, IsRevoked)
VALUES (NEWID(), @UserId, @AdminRoleId, @UserId, GETUTCDATE(), 1, 0);
```

### **3. Login Again**
```bash
POST /api/v1/auth/login
{
  "email": "test@company.com",
  "password": "Test@123"
}
```
Copy the new `accessToken` (now with Admin role)

### **4. Authorize in Swagger**
```
Bearer <new-access-token>
```

### **5. Get a Category ID**
```sql
SELECT TOP 1 Id FROM ProductCategories WHERE Name = 'Electronics';
```

### **6. Create Product**
```bash
POST /api/v1/products
{
  "name": "Test Product",
  "description": "Test description",
  "categoryId": "GUID-FROM-ABOVE",
  "pointsPrice": 100,
  "stockQuantity": 10
}
```

### **7. View Products**
```bash
GET /api/v1/products
```

✅ **Success!** You should see your product in the list.

---

## ⚠️ Common Errors & Solutions

### **Error: "Role 'Employee' not found"**
**Solution:** Restart the API. The database seeder will create roles automatically.

### **Error: 401 Unauthorized**
**Solutions:**
1. Make sure you clicked "Authorize" in Swagger
2. Check you included "Bearer " prefix
3. Check token hasn't expired (15 min lifetime)
4. Get a new token using `/auth/refresh` or `/auth/login`

### **Error: 403 Forbidden**
**Solution:** Your user doesn't have the required role (probably need Admin). Follow Step 4 to upgrade to Admin.

### **Error: "Category not found"**
**Solution:** Use a valid CategoryId from the ProductCategories table, or set it to `null`.

### **Error: "Password does not meet requirements"**
**Solution:** Password must have:
- 8+ characters
- Uppercase letter
- Lowercase letter
- Number
- Special character (!@#$%^&*)

---

## 🎯 Next Steps

1. ✅ **Database Seeding** - Automatically creates roles and categories
2. ✅ **Fixed Product Creation** - Now uses CategoryId and creates pricing/inventory
3. ✅ **Fixed Validators** - No more async validation errors
4. ✅ **Proper Relationships** - Product ↔ Category ↔ Pricing ↔ Inventory

### **Recommended:**
1. Create a migration for the database changes
2. Add a ProductCategoriesController to manage categories via API
3. Add admin user creation endpoint (admin-only, to create other admins)
4. Consider adding a "SeedSampleData" endpoint for demo purposes

---

## 📝 Database Migration Required

After these code changes, create a migration:

```bash
cd RewardPointsSystem.Infrastructure
dotnet ef migrations add FixProductRelationshipsAndSeeding --startup-project ../RewardPointsSystem.Api
dotnet ef database update --startup-project ../RewardPointsSystem.Api
```

---

## ✅ Summary

**Issues Fixed:**
1. ✅ Role not found - Database seeding implemented
2. ✅ 401 Unauthorized - Authorization header format documented
3. ✅ Product creation - Updated to use CategoryId with pricing & inventory
4. ✅ Async validators - Removed to fix validation pipeline
5. ✅ Database relationships - Properly configured Product ↔ Category ↔ Pricing ↔ Inventory

**API is now ready for demo!** 🎉

---

**For questions or issues, check:**
- DEMO_GUIDE.md - Complete demo script
- QUICK_START.md - Quick reference
- This guide - API fixes and usage

**Happy coding! 🚀**
