# Phase 3: API Endpoints Specification

**Status:** ✅ Completed  
**Date:** 2025-11-14

## Overview
This document provides a comprehensive specification of all REST API endpoints for the Reward Points System. The API follows RESTful principles with versioning, consistent URL structure, and proper HTTP status codes.

---

## Base Configuration

- **Base URL:** `/api/v1`
- **API Version:** 1.0
- **Protocol:** HTTPS (production), HTTP (development)
- **Content-Type:** `application/json`
- **Authentication:** JWT Bearer Token

---

## 1. Authentication & Authorization (Public)

### 1.1 Register New User
```
POST /api/v1/auth/register
```
**Description:** Register a new user account  
**Authorization:** Public  
**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "SecurePass123!",
  "firstName": "John",
  "lastName": "Doe"
}
```
**Success Response (201):**
```json
{
  "success": true,
  "data": {
    "userId": "guid",
    "email": "user@example.com",
    "firstName": "John",
    "lastName": "Doe"
  },
  "message": "User registered successfully"
}
```

### 1.2 Login
```
POST /api/v1/auth/login
```
**Description:** Authenticate user and receive JWT tokens  
**Authorization:** Public  
**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "SecurePass123!"
}
```
**Success Response (200):**
```json
{
  "success": true,
  "data": {
    "accessToken": "jwt-token",
    "refreshToken": "refresh-token",
    "expiresIn": 900,
    "user": {
      "id": "guid",
      "email": "user@example.com",
      "firstName": "John",
      "lastName": "Doe",
      "roles": ["Employee"]
    }
  }
}
```

### 1.3 Refresh Token
```
POST /api/v1/auth/refresh
```
**Description:** Refresh access token using refresh token  
**Authorization:** Public  
**Request Body:**
```json
{
  "refreshToken": "refresh-token-string"
}
```

### 1.4 Logout
```
POST /api/v1/auth/logout
```
**Description:** Logout user and revoke tokens  
**Authorization:** Authenticated

### 1.5 Get Current User
```
GET /api/v1/auth/me
```
**Description:** Get current authenticated user information  
**Authorization:** Authenticated

---

## 2. Users Management

### 2.1 Get All Users (Paginated)
```
GET /api/v1/users?page=1&pageSize=10
```
**Description:** Get paginated list of all users  
**Authorization:** Admin only  
**Query Parameters:**
- `page` (optional, default: 1)
- `pageSize` (optional, default: 10)

**Success Response (200):**
```json
{
  "success": true,
  "data": {
    "items": [...],
    "totalCount": 100,
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 10
  }
}
```

### 2.2 Get User by ID
```
GET /api/v1/users/{id}
```
**Description:** Get specific user details  
**Authorization:** Authenticated (own profile) or Admin

### 2.3 Create User
```
POST /api/v1/users
```
**Description:** Create new user (admin operation)  
**Authorization:** Admin only  
**Request Body:**
```json
{
  "email": "newuser@example.com",
  "firstName": "Jane",
  "lastName": "Smith"
}
```

### 2.4 Update User
```
PUT /api/v1/users/{id}
```
**Description:** Update user information  
**Authorization:** Authenticated (own profile) or Admin  
**Request Body:**
```json
{
  "firstName": "Jane",
  "lastName": "Smith",
  "email": "jane.smith@example.com"
}
```

### 2.5 Delete User (Soft Delete)
```
DELETE /api/v1/users/{id}
```
**Description:** Deactivate user account  
**Authorization:** Admin only

### 2.6 Get User Points Balance
```
GET /api/v1/users/{id}/balance
```
**Description:** Get user's current points balance and transaction summary  
**Authorization:** Authenticated (own balance) or Admin

**Success Response (200):**
```json
{
  "success": true,
  "data": {
    "userId": "guid",
    "firstName": "John",
    "lastName": "Doe",
    "currentBalance": 1500,
    "totalEarned": 2000,
    "totalRedeemed": 500,
    "lastTransaction": "2024-11-14T10:30:00Z"
  }
}
```

---

## 3. Roles Management (Admin Only)

### 3.1 Get All Roles
```
GET /api/v1/roles
```
**Description:** Get all available roles  
**Authorization:** Admin only

### 3.2 Get Role by ID
```
GET /api/v1/roles/{id}
```
**Description:** Get specific role details  
**Authorization:** Admin only

### 3.3 Create Role
```
POST /api/v1/roles
```
**Description:** Create new role  
**Authorization:** Admin only

### 3.4 Update Role
```
PUT /api/v1/roles/{id}
```
**Description:** Update role information  
**Authorization:** Admin only

### 3.5 Delete Role
```
DELETE /api/v1/roles/{id}
```
**Description:** Delete role  
**Authorization:** Admin only

### 3.6 Assign Role to User
```
POST /api/v1/users/{userId}/roles
```
**Description:** Assign role to user  
**Authorization:** Admin only  
**Request Body:**
```json
{
  "roleId": "guid"
}
```

### 3.7 Revoke Role from User
```
DELETE /api/v1/users/{userId}/roles/{roleId}
```
**Description:** Remove role from user  
**Authorization:** Admin only

### 3.8 Get User's Roles
```
GET /api/v1/users/{userId}/roles
```
**Description:** Get all roles assigned to user  
**Authorization:** Admin only

---

## 4. Events Management

### 4.1 Get All Events
```
GET /api/v1/events?page=1&pageSize=10&status=Upcoming
```
**Description:** Get paginated and filtered list of events  
**Authorization:** Authenticated  
**Query Parameters:**
- `page` (optional)
- `pageSize` (optional)
- `status` (optional: Upcoming, Active, Completed, Cancelled)

### 4.2 Get Event by ID
```
GET /api/v1/events/{id}
```
**Description:** Get detailed event information  
**Authorization:** Authenticated

### 4.3 Create Event
```
POST /api/v1/events
```
**Description:** Create new event  
**Authorization:** Admin only  
**Request Body:**
```json
{
  "name": "Q4 Sales Competition",
  "description": "Top performers earn bonus points",
  "eventDate": "2024-12-31T23:59:59Z",
  "totalPointsPool": 10000,
  "maxParticipants": 50,
  "location": "Virtual"
}
```

### 4.4 Update Event
```
PUT /api/v1/events/{id}
```
**Description:** Update event details  
**Authorization:** Admin only

### 4.5 Cancel Event
```
DELETE /api/v1/events/{id}
```
**Description:** Cancel event  
**Authorization:** Admin only

### 4.6 Publish Event
```
PATCH /api/v1/events/{id}/publish
```
**Description:** Publish event (Draft → Published)  
**Authorization:** Admin only

### 4.7 Activate Event
```
PATCH /api/v1/events/{id}/activate
```
**Description:** Open registration for event  
**Authorization:** Admin only

### 4.8 Complete Event
```
PATCH /api/v1/events/{id}/complete
```
**Description:** Mark event as completed  
**Authorization:** Admin only

### 4.9 Get Event Participants
```
GET /api/v1/events/{id}/participants
```
**Description:** Get list of event participants  
**Authorization:** Authenticated

### 4.10 Register for Event
```
POST /api/v1/events/{id}/participants
```
**Description:** Register participant for event  
**Authorization:** Authenticated  
**Request Body:**
```json
{
  "userId": "guid",
  "eventId": "guid"
}
```

### 4.11 Unregister from Event
```
DELETE /api/v1/events/{id}/participants/{userId}
```
**Description:** Remove participant from event  
**Authorization:** Admin or Self

### 4.12 Award Points to Winners
```
POST /api/v1/events/{id}/award-points
```
**Description:** Award points to event winners  
**Authorization:** Admin only  
**Request Body:**
```json
{
  "eventId": "guid",
  "awards": [
    { "userId": "guid", "points": 500, "position": 1 },
    { "userId": "guid", "points": 300, "position": 2 }
  ]
}
```

### 4.13 Get Upcoming Events
```
GET /api/v1/events/upcoming
```
**Description:** Get all upcoming events  
**Authorization:** Authenticated

### 4.14 Get Active Events
```
GET /api/v1/events/active
```
**Description:** Get all active/in-progress events  
**Authorization:** Authenticated

---

## 5. Points & Transactions

### 5.1 Get User Points Account
```
GET /api/v1/points/accounts/{userId}
```
**Description:** Get user's points account details  
**Authorization:** Authenticated (own account) or Admin

### 5.2 Get User Transactions
```
GET /api/v1/points/transactions/{userId}?page=1&pageSize=20
```
**Description:** Get user's transaction history (paginated)  
**Authorization:** Authenticated (own transactions) or Admin

### 5.3 Get All Transactions
```
GET /api/v1/points/transactions?page=1&pageSize=50
```
**Description:** Get all system transactions  
**Authorization:** Admin only

### 5.4 Award Points
```
POST /api/v1/points/award
```
**Description:** Award points to user  
**Authorization:** Admin only  
**Request Body:**
```json
{
  "userId": "guid",
  "points": 100,
  "reason": "Monthly performance bonus"
}
```

### 5.5 Deduct Points
```
POST /api/v1/points/deduct
```
**Description:** Deduct points from user  
**Authorization:** Admin only  
**Request Body:**
```json
{
  "userId": "guid",
  "points": 50,
  "reason": "Point correction"
}
```

### 5.6 Get Points Leaderboard
```
GET /api/v1/points/leaderboard?top=10
```
**Description:** Get top users by points  
**Authorization:** Authenticated

### 5.7 Get Points Summary
```
GET /api/v1/points/summary
```
**Description:** Get system-wide points statistics  
**Authorization:** Admin only

---

## 6. Products Catalog

### 6.1 Get All Products
```
GET /api/v1/products?page=1&pageSize=20&category=Electronics
```
**Description:** Get paginated and filtered product list  
**Authorization:** Authenticated  
**Query Parameters:**
- `page` (optional)
- `pageSize` (optional)
- `category` (optional)
- `search` (optional)

### 6.2 Get Product by ID
```
GET /api/v1/products/{id}
```
**Description:** Get detailed product information  
**Authorization:** Authenticated

### 6.3 Create Product
```
POST /api/v1/products
```
**Description:** Create new product  
**Authorization:** Admin only  
**Request Body:**
```json
{
  "name": "Wireless Headphones",
  "description": "Premium noise-cancelling headphones",
  "category": "Electronics",
  "imageUrl": "https://example.com/image.jpg"
}
```

### 6.4 Update Product
```
PUT /api/v1/products/{id}
```
**Description:** Update product details  
**Authorization:** Admin only

### 6.5 Delete Product
```
DELETE /api/v1/products/{id}
```
**Description:** Delete/deactivate product  
**Authorization:** Admin only

### 6.6 Activate Product
```
PATCH /api/v1/products/{id}/activate
```
**Description:** Activate product for redemption  
**Authorization:** Admin only

### 6.7 Deactivate Product
```
PATCH /api/v1/products/{id}/deactivate
```
**Description:** Deactivate product  
**Authorization:** Admin only

### 6.8 Get Product Pricing History
```
GET /api/v1/products/{id}/pricing
```
**Description:** Get price change history  
**Authorization:** Admin only

### 6.9 Set Product Price
```
POST /api/v1/products/{id}/pricing
```
**Description:** Set new product price in points  
**Authorization:** Admin only  
**Request Body:**
```json
{
  "productId": "guid",
  "pointsCost": 500
}
```

### 6.10 Get Product Inventory
```
GET /api/v1/products/{id}/inventory
```
**Description:** Get product stock information  
**Authorization:** Admin only

### 6.11 Update Inventory
```
PATCH /api/v1/products/{id}/inventory
```
**Description:** Update product stock quantity  
**Authorization:** Admin only  
**Request Body:**
```json
{
  "productId": "guid",
  "quantity": 100,
  "operation": "Add" // or "Set"
}
```

### 6.12 Get Product Categories
```
GET /api/v1/products/categories
```
**Description:** Get all product categories  
**Authorization:** Authenticated

### 6.13 Search Products
```
GET /api/v1/products/search?query=headphones
```
**Description:** Search products by name/description  
**Authorization:** Authenticated

---

## 7. Redemptions

### 7.1 Get All Redemptions
```
GET /api/v1/redemptions?page=1&pageSize=20&status=Pending
```
**Description:** Get redemptions (filtered by user for employees, all for admin)  
**Authorization:** Authenticated  
**Query Parameters:**
- `status` (optional: Pending, Approved, Delivered, Cancelled)

### 7.2 Get Redemption by ID
```
GET /api/v1/redemptions/{id}
```
**Description:** Get detailed redemption information  
**Authorization:** Authenticated (own redemption) or Admin

### 7.3 Create Redemption Request
```
POST /api/v1/redemptions
```
**Description:** Create new redemption request  
**Authorization:** Authenticated  
**Request Body:**
```json
{
  "productId": "guid",
  "quantity": 1,
  "deliveryAddress": "123 Main St, City, State, ZIP"
}
```

### 7.4 Approve Redemption
```
PATCH /api/v1/redemptions/{id}/approve
```
**Description:** Approve redemption request  
**Authorization:** Admin only

### 7.5 Mark as Delivered
```
PATCH /api/v1/redemptions/{id}/deliver
```
**Description:** Mark redemption as delivered  
**Authorization:** Admin only

### 7.6 Cancel Redemption
```
PATCH /api/v1/redemptions/{id}/cancel
```
**Description:** Cancel redemption and refund points  
**Authorization:** Admin or Self (before approval)

### 7.7 Get My Redemptions
```
GET /api/v1/redemptions/my-redemptions?page=1&pageSize=10
```
**Description:** Get current user's redemption history  
**Authorization:** Authenticated

### 7.8 Get Pending Redemptions
```
GET /api/v1/redemptions/pending
```
**Description:** Get all pending redemptions  
**Authorization:** Admin only

### 7.9 Get Redemption History
```
GET /api/v1/redemptions/history?page=1&pageSize=50
```
**Description:** Get complete redemption history  
**Authorization:** Admin only

---

## 8. Admin Dashboard

### 8.1 Get Dashboard Statistics
```
GET /api/v1/admin/dashboard
```
**Description:** Get comprehensive dashboard statistics  
**Authorization:** Admin only  
**Success Response (200):**
```json
{
  "success": true,
  "data": {
    "totalUsers": 1500,
    "activeUsers": 1200,
    "totalPointsDistributed": 500000,
    "totalPointsRedeemed": 250000,
    "activeEvents": 5,
    "pendingRedemptions": 23,
    "lowStockProducts": 8
  }
}
```

### 8.2 Points Summary Report
```
GET /api/v1/admin/reports/points?startDate=2024-01-01&endDate=2024-12-31
```
**Description:** Get points distribution and redemption report  
**Authorization:** Admin only

### 8.3 User Activity Report
```
GET /api/v1/admin/reports/users?startDate=2024-01-01&endDate=2024-12-31
```
**Description:** Get user registration and activity statistics  
**Authorization:** Admin only

### 8.4 Redemptions Report
```
GET /api/v1/admin/reports/redemptions?startDate=2024-01-01&endDate=2024-12-31
```
**Description:** Get redemption statistics and trends  
**Authorization:** Admin only

### 8.5 Events Report
```
GET /api/v1/admin/reports/events?year=2024
```
**Description:** Get event participation and points distribution report  
**Authorization:** Admin only

### 8.6 Low Stock Alerts
```
GET /api/v1/admin/alerts/inventory
```
**Description:** Get products with low stock levels  
**Authorization:** Admin only

### 8.7 Points Pool Alerts
```
GET /api/v1/admin/alerts/points
```
**Description:** Get events with low points pool remaining  
**Authorization:** Admin only

---

## HTTP Status Codes

| Code | Meaning | Usage |
|------|---------|-------|
| **200** | OK | Successful GET, PUT, PATCH, DELETE |
| **201** | Created | Successful POST (resource created) |
| **204** | No Content | Successful DELETE with no response body |
| **400** | Bad Request | Malformed request, validation errors |
| **401** | Unauthorized | Missing or invalid authentication token |
| **403** | Forbidden | Authenticated but insufficient permissions |
| **404** | Not Found | Resource doesn't exist |
| **409** | Conflict | Duplicate resource, business rule violation |
| **422** | Unprocessable Entity | Semantic validation failed |
| **500** | Internal Server Error | Unexpected server error |

---

## Standard Response Format

### Success Response
```json
{
  "success": true,
  "data": { /* response data */ },
  "message": "Operation completed successfully"
}
```

### Error Response
```json
{
  "success": false,
  "message": "Error description",
  "statusCode": 400,
  "timestamp": "2024-11-14T10:30:00Z"
}
```

### Validation Error Response
```json
{
  "success": false,
  "message": "Validation failed",
  "statusCode": 422,
  "errors": {
    "Email": ["Email is required", "Invalid email format"],
    "FirstName": ["First name must be between 1 and 100 characters"]
  }
}
```

---

## Summary

### Total Endpoints: 66+

**Breakdown by Controller:**
- Authentication: 5 endpoints
- Users: 6 endpoints
- Roles: 8 endpoints
- Events: 14 endpoints
- Points: 7 endpoints
- Products: 13 endpoints
- Redemptions: 9 endpoints
- Admin: 7 endpoints

**Authorization Distribution:**
- Public: 3 endpoints
- Authenticated: 25 endpoints
- Admin Only: 38 endpoints

**HTTP Methods Distribution:**
- GET: 38 endpoints
- POST: 16 endpoints
- PUT: 5 endpoints
- PATCH: 10 endpoints
- DELETE: 7 endpoints

---

## Implementation Status

✅ Phase 3 Documentation: COMPLETE  
🔄 Phase 4 Implementation: IN PROGRESS (3/8 controllers done)  
- ✅ BaseApiController
- ✅ UsersController (6 endpoints)
- ✅ EventsController (4 endpoints)
- ✅ ProductsController (3 endpoints)
- ⏳ AuthController
- ⏳ RolesController
- ⏳ PointsController
- ⏳ RedemptionsController
- ⏳ AdminController

---

**Last Updated:** 2025-11-14  
**Document Version:** 1.0
