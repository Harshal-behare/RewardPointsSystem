# Reward Points System - API Documentation

> **Base URL:** `/api/v1/`
> **Authentication:** JWT Bearer Token
> **Last Updated:** February 2026

---

## Table of Contents
1. [Overview](#overview)
2. [Authentication APIs](#authentication-apis)
3. [User APIs](#user-apis)
4. [Points APIs](#points-apis)
5. [Products APIs](#products-apis)
6. [Categories APIs](#categories-apis)
7. [Redemptions APIs](#redemptions-apis)
8. [Events APIs](#events-apis)
9. [Roles APIs](#roles-apis)
10. [Admin APIs](#admin-apis)
11. [Employee APIs](#employee-apis)

---

## Overview

This API follows Clean Architecture principles with:
- **Consistent Response Format**: All responses use `ApiResponse<T>` wrapper
- **Pagination**: Supports `PagedResponse<T>` for list endpoints
- **Role-Based Authorization**: Admin, Employee roles
- **JWT Authentication**: Access & Refresh token mechanism

### Standard Response Format
```json
{
  "success": true,
  "data": { },
  "message": "Operation message",
  "timestamp": "2026-02-01T12:00:00Z"
}
```

### Error Response Format
```json
{
  "success": false,
  "message": "Error description",
  "errors": ["Validation error 1", "Validation error 2"]
}
```

---

## Authentication APIs

| Method | Endpoint | Auth Required | Description |
|--------|----------|---------------|-------------|
| POST | `/auth/register` | No | Register new user |
| POST | `/auth/login` | No | Login with credentials |
| POST | `/auth/refresh` | No | Refresh access token |
| POST | `/auth/logout` | Yes | Logout user |
| GET | `/auth/me` | Yes | Get current user info |
| POST | `/auth/change-password` | Yes | Change user password |

### 1. Register User
**POST** `/api/v1/auth/register`

**Request Body:**
```json
{
  "email": "user@agdata.com",
  "password": "SecurePass123!",
  "confirmPassword": "SecurePass123!",
  "firstName": "John",
  "lastName": "Doe"
}
```

**Password Requirements:**
- Min 8 characters, Max 20 characters
- At least 1 uppercase letter
- At least 1 lowercase letter
- At least 1 number
- At least 1 special character

**Response:** `201 Created`
```json
{
  "success": true,
  "data": {
    "userId": "guid",
    "email": "user@agdata.com",
    "firstName": "John",
    "lastName": "Doe",
    "accessToken": "jwt-token",
    "refreshToken": "refresh-token",
    "expiresAt": "2026-02-01T13:00:00Z",
    "roles": ["Employee"]
  },
  "message": "User registered successfully"
}
```

### 2. Login
**POST** `/api/v1/auth/login`

**Request Body:**
```json
{
  "email": "@agdata.com",
  "password": "SecurePass123!"
}
```

**Response:** `200 OK`
```json
{
  "success": true,
  "data": {
    "userId": "guid",
    "email": "user@agdata.com",
    "firstName": "John",
    "lastName": "Doe",
    "accessToken": "jwt-token",
    "refreshToken": "refresh-token",
    "expiresAt": "2026-02-01T13:00:00Z",
    "roles": ["Employee"]
  },
  "message": "Login successful"
}
```

### 3. Refresh Token
**POST** `/api/v1/auth/refresh`

**Request Body:**
```json
{
  "refreshToken": "refresh-token-here"
}
```

**Response:** `200 OK`
```json
{
  "success": true,
  "data": {
    "accessToken": "new-jwt-token",
    "refreshToken": "new-refresh-token",
    "expiresAt": "2026-02-01T13:30:00Z"
  },
  "message": "Token refreshed successfully"
}
```

### 4. Logout
**POST** `/api/v1/auth/logout`
**Auth Required:** Bearer Token

**Response:** `200 OK`
```json
{
  "success": true,
  "message": "Logout successful"
}
```

### 5. Get Current User
**GET** `/api/v1/auth/me`
**Auth Required:** Bearer Token

**Response:** `200 OK`
```json
{
  "success": true,
  "data": {
    "userId": "guid",
    "email": "user@agdata.com",
    "firstName": "John",
    "lastName": "Doe",
    "roles": ["Employee"]
  }
}
```

### 6. Change Password
**POST** `/api/v1/auth/change-password`
**Auth Required:** Bearer Token

**Request Body:**
```json
{
  "currentPassword": "OldPassword123!",
  "newPassword": "NewPassword456!"
}
```

**Response:** `200 OK`
```json
{
  "success": true,
  "message": "Password changed successfully"
}
```

---

## User APIs

| Method | Endpoint | Auth Required | Role | Description |
|--------|----------|---------------|------|-------------|
| GET | `/users` | Yes | Admin | Get all users (paginated) |
| GET | `/users/{id}` | Yes | Any | Get user by ID |
| POST | `/users` | Yes | Admin | Create new user |
| PUT | `/users/{id}` | Yes | Any | Update user |
| PATCH | `/users/{id}/deactivate` | Yes | Admin | Deactivate user |
| GET | `/users/{id}/balance` | Yes | Any | Get user points balance |

### 1. Get All Users
**GET** `/api/v1/users?page=1&pageSize=10`
**Auth Required:** Admin Only

**Response:** `200 OK`
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "guid",
        "firstName": "John",
        "lastName": "Doe",
        "email": "john@agdata.com",
        "isActive": true,
        "createdAt": "2026-01-01T00:00:00Z",
        "roles": ["Employee"],
        "pointsBalance": 1500
      }
    ],
    "page": 1,
    "pageSize": 10,
    "totalCount": 50,
    "totalPages": 5
  }
}
```

### 2. Get User By ID
**GET** `/api/v1/users/{id}`

**Response:** `200 OK`
```json
{
  "success": true,
  "data": {
    "id": "guid",
    "firstName": "John",
    "lastName": "Doe",
    "email": "john@agdata.com",
    "isActive": true,
    "createdAt": "2026-01-01T00:00:00Z"
  }
}
```

### 3. Create User
**POST** `/api/v1/users`
**Auth Required:** Admin Only

**Request Body:**
```json
{
  "email": "newuser@agdata.com",
  "firstName": "Jane",
  "lastName": "Doe",
  "password": "SecurePass123!",
  "role": "Employee"
}
```

**Response:** `201 Created`

### 4. Update User
**PUT** `/api/v1/users/{id}`

**Request Body:**
```json
{
  "firstName": "Updated",
  "lastName": "Name",
  "email": "updated@agdata.com",
  "isActive": true
}
```

**Response:** `200 OK`

### 5. Deactivate User
**PATCH** `/api/v1/users/{id}/deactivate`
**Auth Required:** Admin Only

**Business Rules:**
- Cannot deactivate user with pending redemptions
- Cannot deactivate the last remaining admin

**Response:** `200 OK`
```json
{
  "success": true,
  "message": "User deactivated successfully"
}
```

### 6. Get User Balance
**GET** `/api/v1/users/{id}/balance`

**Response:** `200 OK`
```json
{
  "success": true,
  "data": {
    "userId": "guid",
    "firstName": "John",
    "lastName": "Doe",
    "email": "john@agdata.com",
    "currentBalance": 1500,
    "totalEarned": 3000,
    "totalRedeemed": 1500,
    "lastTransaction": "2026-01-18T10:00:00Z"
  }
}
```

---

## Points APIs

| Method | Endpoint | Auth Required | Role | Description |
|--------|----------|---------------|------|-------------|
| GET | `/points/accounts/{userId}` | Yes | Any | Get user's points account |
| GET | `/points/transactions/{userId}` | Yes | Any | Get user's transactions |
| GET | `/points/transactions` | Yes | Admin | Get all transactions |
| POST | `/points/award` | Yes | Admin | Award points to user |
| POST | `/points/deduct` | Yes | Admin | Deduct points from user |
| GET | `/points/leaderboard` | Yes | Any | Get points leaderboard |
| GET | `/points/summary` | Yes | Admin | Get points system summary |

### 1. Get User Points Account
**GET** `/api/v1/points/accounts/{userId}`

**Response:** `200 OK`
```json
{
  "success": true,
  "data": {
    "userId": "guid",
    "userName": "John Doe",
    "userEmail": "john@agdata.com",
    "currentBalance": 1500,
    "totalEarned": 3000,
    "totalRedeemed": 1500,
    "pendingPoints": 100,
    "lastTransaction": "2026-01-18T10:00:00Z",
    "createdAt": "2026-01-01T00:00:00Z"
  }
}
```

### 2. Get User Transactions
**GET** `/api/v1/points/transactions/{userId}?page=1&pageSize=20`

**Response:** `200 OK`
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "guid",
        "userId": "guid",
        "transactionType": "Credit",
        "userPoints": 100,
        "description": "Event participation award",
        "eventId": "guid",
        "eventName": "Annual Celebration",
        "eventDescription": "Company anniversary event",
        "eventRank": 1,
        "transactionSource": "Event",
        "balanceAfter": 1600,
        "timestamp": "2026-01-18T10:00:00Z"
      }
    ],
    "page": 1,
    "pageSize": 20,
    "totalCount": 50,
    "totalPages": 3
  }
}
```

### 3. Get All Transactions (Admin)
**GET** `/api/v1/points/transactions?page=1&pageSize=50`
**Auth Required:** Admin Only

### 4. Award Points
**POST** `/api/v1/points/award`
**Auth Required:** Admin Only

**Request Body:**
```json
{
  "userId": "guid",
  "points": 100,
  "description": "Monthly bonus",
  "eventId": "optional-guid"
}
```

**Response:** `200 OK`
```json
{
  "success": true,
  "message": "Successfully awarded 100 points"
}
```

### 5. Deduct Points
**POST** `/api/v1/points/deduct`
**Auth Required:** Admin Only

**Request Body:**
```json
{
  "userId": "guid",
  "points": 50,
  "description": "Penalty deduction"
}
```

### 6. Get Leaderboard
**GET** `/api/v1/points/leaderboard?top=10`

**Response:** `200 OK`
```json
{
  "success": true,
  "data": [
    {
      "userId": "guid",
      "userName": "John Doe",
      "userEmail": "john@agdata.com",
      "currentBalance": 5000,
      "totalEarned": 8000,
      "totalRedeemed": 3000
    }
  ]
}
```

### 7. Get Points Summary (Admin)
**GET** `/api/v1/points/summary`
**Auth Required:** Admin Only

**Response:** `200 OK`
```json
{
  "success": true,
  "data": {
    "totalUsers": 100,
    "totalPointsDistributed": 500000,
    "totalPointsRedeemed": 200000,
    "totalPointsInCirculation": 300000,
    "averageBalance": 3000
  }
}
```

---

## Products APIs

| Method | Endpoint | Auth Required | Role | Description |
|--------|----------|---------------|------|-------------|
| GET | `/products` | No | Any | Get all active products |
| GET | `/products/admin/all` | Yes | Admin | Get all products (including inactive) |
| GET | `/products/{id}` | No | Any | Get product by ID |
| POST | `/products` | Yes | Admin | Create new product |
| PUT | `/products/{id}` | Yes | Admin | Update product |
| PATCH | `/products/{id}/deactivate` | Yes | Admin | Deactivate product |
| GET | `/products/category/{categoryId}` | No | Any | Get products by category |

### 1. Get All Products
**GET** `/api/v1/products`

**Response:** `200 OK`
```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "name": "Gift Card $50",
      "description": "Amazon gift card",
      "categoryId": "guid",
      "categoryName": "Gift Cards",
      "imageUrl": "https://...",
      "currentPointsCost": 500,
      "isActive": true,
      "isInStock": true,
      "stockQuantity": 10,
      "createdAt": "2026-01-01T00:00:00Z"
    }
  ]
}
```

### 2. Get All Products (Admin)
**GET** `/api/v1/products/admin/all`
**Auth Required:** Admin Only

*Returns all products including inactive ones.*

### 3. Get Product By ID
**GET** `/api/v1/products/{id}`

### 4. Create Product
**POST** `/api/v1/products`
**Auth Required:** Admin Only

**Request Body:**
```json
{
  "name": "New Product",
  "description": "Product description",
  "categoryId": "guid",
  "imageUrl": "https://...",
  "pointsPrice": 300,
  "stockQuantity": 50
}
```

**Response:** `201 Created`

### 5. Update Product
**PUT** `/api/v1/products/{id}`
**Auth Required:** Admin Only

**Request Body:**
```json
{
  "name": "Updated Product Name",
  "description": "Updated description",
  "categoryId": "guid",
  "imageUrl": "https://...",
  "pointsPrice": 350,
  "stockQuantity": 40,
  "isActive": true
}
```

### 6. Deactivate Product
**PATCH** `/api/v1/products/{id}/deactivate`
**Auth Required:** Admin Only

*Products cannot be permanently deleted, only deactivated.*

### 7. Get Products by Category
**GET** `/api/v1/products/category/{categoryId}`

---

## Categories APIs

| Method | Endpoint | Auth Required | Role | Description |
|--------|----------|---------------|------|-------------|
| GET | `/products/categories` | No | Any | Get all active categories |
| GET | `/products/categories/admin/all` | Yes | Admin | Get all categories (including inactive) |
| POST | `/products/categories` | Yes | Admin | Create category |
| PUT | `/products/categories/{id}` | Yes | Admin | Update category |
| DELETE | `/products/categories/{id}` | Yes | Admin | Delete category |

### 1. Get All Categories
**GET** `/api/v1/products/categories`

**Response:** `200 OK`
```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "name": "Gift Cards",
      "description": "Digital gift cards",
      "displayOrder": 1,
      "isActive": true,
      "productCount": 5
    }
  ]
}
```

### 2. Get All Categories (Admin)
**GET** `/api/v1/products/categories/admin/all`
**Auth Required:** Admin Only

*Returns all categories including inactive ones.*

### 3. Create Category
**POST** `/api/v1/products/categories`
**Auth Required:** Admin Only

**Request Body:**
```json
{
  "name": "New Category",
  "description": "Category description",
  "displayOrder": 5
}
```

### 4. Update Category
**PUT** `/api/v1/products/categories/{id}`
**Auth Required:** Admin Only

**Request Body:**
```json
{
  "name": "Updated Category",
  "description": "Updated description",
  "displayOrder": 3,
  "isActive": true
}
```

### 5. Delete Category
**DELETE** `/api/v1/products/categories/{id}`
**Auth Required:** Admin Only

*Permanently deletes the category. Products in this category will become "Uncategorized".*

---

## Redemptions APIs

| Method | Endpoint | Auth Required | Role | Description |
|--------|----------|---------------|------|-------------|
| GET | `/redemptions` | Yes | Any | Get redemptions (filtered by role) |
| GET | `/redemptions/{id}` | Yes | Any | Get redemption by ID |
| POST | `/redemptions` | Yes | Any | Create redemption request |
| PATCH | `/redemptions/{id}/approve` | Yes | Admin | Approve redemption |
| PATCH | `/redemptions/{id}/deliver` | Yes | Admin | Mark as delivered |
| PATCH | `/redemptions/{id}/reject` | Yes | Admin | Reject redemption |
| PATCH | `/redemptions/{id}/cancel` | Yes | Any | Cancel redemption |
| GET | `/redemptions/my-redemptions` | Yes | Any | Get current user's redemptions |
| GET | `/redemptions/pending` | Yes | Admin | Get pending redemptions |
| GET | `/redemptions/user/{userId}/pending-count` | Yes | Admin | Get user's pending count |
| GET | `/redemptions/history` | Yes | Admin | Get redemption history |

### Redemption Status Flow
```
Pending → Approved → Delivered
Pending → Rejected (with reason)
Pending → Cancelled (by employee)
```

### 1. Get Redemptions
**GET** `/api/v1/redemptions?page=1&pageSize=20&status=Pending`

*Employees see only their own redemptions. Admins see all.*

**Response:** `200 OK`
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "guid",
        "userId": "guid",
        "userName": "John Doe",
        "userEmail": "john@agdata.com",
        "productId": "guid",
        "productName": "Gift Card $50",
        "pointsSpent": 500,
        "quantity": 1,
        "status": "Pending",
        "requestedAt": "2026-01-18T10:00:00Z",
        "approvedAt": null,
        "rejectionReason": null
      }
    ],
    "page": 1,
    "pageSize": 20,
    "totalCount": 10,
    "totalPages": 1
  }
}
```

### 2. Get Redemption By ID
**GET** `/api/v1/redemptions/{id}`

*Employees can only view their own redemptions.*

### 3. Create Redemption
**POST** `/api/v1/redemptions`

**Request Body:**
```json
{
  "userId": "guid",
  "productId": "guid",
  "quantity": 1
}
```

**Validation Rules:**
- User must have sufficient points
- Product must be active and in stock
- Quantity: Min 1, Max 10

**Response:** `201 Created`

### 4. Approve Redemption
**PATCH** `/api/v1/redemptions/{id}/approve`
**Auth Required:** Admin Only

**Request Body:**
```json
{
  "approvedBy": "admin-user-guid"
}
```

*Note: The approvedBy is automatically set from the authenticated admin's JWT.*

### 5. Mark as Delivered
**PATCH** `/api/v1/redemptions/{id}/deliver`
**Auth Required:** Admin Only

*Can only deliver redemptions with Approved status.*

### 6. Reject Redemption
**PATCH** `/api/v1/redemptions/{id}/reject`
**Auth Required:** Admin Only

**Request Body:**
```json
{
  "rejectionReason": "Product no longer available. Please select another item."
}
```

**Validation:** Rejection reason required, min 10 characters, max 500 characters.

### 7. Cancel Redemption
**PATCH** `/api/v1/redemptions/{id}/cancel`

**Request Body:**
```json
{
  "cancellationReason": "Changed my mind"
}
```

*Can only cancel redemptions with Pending status. Points are refunded.*

### 8. Get My Redemptions
**GET** `/api/v1/redemptions/my-redemptions?page=1&pageSize=10`

### 9. Get Pending Redemptions (Admin)
**GET** `/api/v1/redemptions/pending`
**Auth Required:** Admin Only

### 10. Get User Pending Redemptions Count
**GET** `/api/v1/redemptions/user/{userId}/pending-count`
**Auth Required:** Admin Only

*Used for user deactivation validation.*

**Response:** `200 OK`
```json
{
  "success": true,
  "data": {
    "count": 2
  }
}
```

### 11. Get Redemption History (Admin)
**GET** `/api/v1/redemptions/history?page=1&pageSize=50`
**Auth Required:** Admin Only

---

## Events APIs

| Method | Endpoint | Auth Required | Role | Description |
|--------|----------|---------------|------|-------------|
| GET | `/events` | No | Any | Get visible events |
| GET | `/events/all` | Yes | Admin | Get all events (including drafts) |
| GET | `/events/{id}` | No | Any | Get event by ID |
| POST | `/events` | Yes | Admin | Create new event |
| PUT | `/events/{id}` | Yes | Admin | Update event |
| DELETE | `/events/{id}` | Yes | Admin | Delete event |
| PATCH | `/events/{id}/status` | Yes | Admin | Change event status |
| GET | `/events/{id}/participants` | Yes | Any | Get event participants |
| POST | `/events/{id}/participants` | Yes | Any | Register for event |
| DELETE | `/events/{id}/participants/{userId}` | Yes | Any | Unregister participant |
| POST | `/events/{id}/award-winners` | Yes | Admin | Bulk award winners |

### Event Status Flow (One-Way Only)
```
Draft → Upcoming → Active → Completed
```
**Important:** Events can only move forward. No backward transitions allowed.

- **Draft**: Admin only visibility, event in planning
- **Upcoming**: Visible to employees, registration open
- **Active**: Event in progress
- **Completed**: Event finished, points can be awarded

### 1. Get All Events (Public)
**GET** `/api/v1/events`

*Returns events visible to employees (Upcoming, Active, Completed - excludes Draft).*

**Response:** `200 OK`
```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "name": "Annual Celebration",
      "description": "Company anniversary event",
      "eventDate": "2026-03-15T00:00:00Z",
      "eventEndDate": "2026-03-15T18:00:00Z",
      "status": "Upcoming",
      "totalPointsPool": 10000,
      "remainingPoints": 10000,
      "participantsCount": 25,
      "maxParticipants": 100,
      "registrationStartDate": "2026-02-01T00:00:00Z",
      "registrationEndDate": "2026-03-10T00:00:00Z",
      "location": "Main Office",
      "virtualLink": null,
      "bannerImageUrl": "https://...",
      "createdAt": "2026-01-01T00:00:00Z",
      "firstPlacePoints": 500,
      "secondPlacePoints": 300,
      "thirdPlacePoints": 200,
      "winners": []
    }
  ]
}
```

### 2. Get All Events (Admin)
**GET** `/api/v1/events/all`
**Auth Required:** Admin Only

*Returns all events including drafts.*

### 3. Get Event By ID
**GET** `/api/v1/events/{id}`

### 4. Create Event
**POST** `/api/v1/events`
**Auth Required:** Admin Only

**Request Body:**
```json
{
  "name": "New Event",
  "description": "Event description",
  "eventDate": "2026-06-01T09:00:00Z",
  "eventEndDate": "2026-06-01T18:00:00Z",
  "totalPointsPool": 5000,
  "maxParticipants": 50,
  "registrationStartDate": "2026-05-01T00:00:00Z",
  "registrationEndDate": "2026-05-28T00:00:00Z",
  "location": "Conference Room A",
  "virtualLink": "https://meet.example.com/event",
  "bannerImageUrl": "https://...",
  "firstPlacePoints": 500,
  "secondPlacePoints": 300,
  "thirdPlacePoints": 200
}
```

**Validation Rules:**
- Name: Required, 3-200 characters
- Description: Optional, max 1000 characters
- Event Date: Required, must be future date
- Points Pool: Required, must be > 0, max 1,000,000

**Response:** `201 Created`

### 5. Update Event
**PUT** `/api/v1/events/{id}`
**Auth Required:** Admin Only

*Cannot modify completed events (except status).*

### 6. Delete Event
**DELETE** `/api/v1/events/{id}`
**Auth Required:** Admin Only

### 7. Change Event Status
**PATCH** `/api/v1/events/{id}/status`
**Auth Required:** Admin Only

**Request Body:**
```json
{
  "status": "Upcoming"
}
```

**Valid Status Values:** `Draft`, `Upcoming`, `Active`, `Completed`

**Business Rule:** Status can only move forward, never backward.

### 8. Get Event Participants
**GET** `/api/v1/events/{id}/participants`
**Auth Required:** Bearer Token

**Response:** `200 OK`
```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "eventId": "guid",
      "userId": "guid",
      "userName": "John Doe",
      "userEmail": "john@agdata.com",
      "registeredAt": "2026-02-15T10:00:00Z",
      "status": "Registered",
      "pointsAwarded": null,
      "eventRank": null
    }
  ]
}
```

### 9. Register Participant
**POST** `/api/v1/events/{id}/participants`
**Auth Required:** Bearer Token

**Request Body:**
```json
{
  "userId": "guid"
}
```

**Validation Rules:**
- Registration only allowed between RegistrationStartDate and RegistrationEndDate
- Cannot exceed MaxParticipants if set
- User cannot register twice for same event
- Can only register for Upcoming events

### 10. Unregister Participant
**DELETE** `/api/v1/events/{id}/participants/{userId}`
**Auth Required:** Bearer Token

*Employees can unregister themselves. Admins can unregister anyone.*

**Validation Rules:**
- Cannot unregister from Active or Completed events
- Cannot unregister if already awarded points
- Only self or admin can perform unregistration

**Response:** `200 OK`
```json
{
  "success": true,
  "message": "Participant unregistered successfully"
}
```

### 11. Bulk Award Winners
**POST** `/api/v1/events/{id}/award-winners`
**Auth Required:** Admin Only

*Awards points to event winners in a single API call.*

**Request Body:**
```json
{
  "awards": [
    { "userId": "guid", "points": 500, "eventRank": 1 },
    { "userId": "guid", "points": 300, "eventRank": 2 },
    { "userId": "guid", "points": 200, "eventRank": 3 }
  ]
}
```

**Validation Rules:**
- Awards list cannot be empty
- Ranks must be unique (1, 2, or 3 only)
- Points must match event prize configuration if set
- User must be registered for the event
- Cannot award same user twice
- Sum of awards cannot exceed remaining points pool

**Response:** `200 OK`
```json
{
  "success": true,
  "data": {
    "id": "guid",
    "name": "Event Name",
    "status": "Completed",
    "remainingPoints": 0,
    "winners": [
      { "userId": "guid", "userName": "John Doe", "rank": 1, "points": 500 },
      { "userId": "guid", "userName": "Jane Doe", "rank": 2, "points": 300 },
      { "userId": "guid", "userName": "Bob Smith", "rank": 3, "points": 200 }
    ]
  },
  "message": "Winners awarded successfully"
}
```

---

## Roles APIs

| Method | Endpoint | Auth Required | Role | Description |
|--------|----------|---------------|------|-------------|
| GET | `/roles` | Yes | Admin | Get all roles |
| GET | `/roles/{id}` | Yes | Admin | Get role by ID |
| POST | `/roles` | Yes | Admin | Create new role |
| PUT | `/roles/{id}` | Yes | Admin | Update role |
| DELETE | `/roles/{id}` | Yes | Admin | Delete role |
| POST | `/roles/users/{userId}/assign` | Yes | Admin | Assign role to user |
| DELETE | `/roles/users/{userId}/roles/{roleId}` | Yes | Admin | Revoke role from user |
| GET | `/roles/users/{userId}/roles` | Yes | Admin | Get user's roles |

### 1. Get All Roles
**GET** `/api/v1/roles`
**Auth Required:** Admin Only

**Response:** `200 OK`
```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "name": "Admin",
      "description": "System administrator",
      "createdAt": "2026-01-01T00:00:00Z"
    },
    {
      "id": "guid",
      "name": "Employee",
      "description": "Regular employee",
      "createdAt": "2026-01-01T00:00:00Z"
    }
  ]
}
```

### 2. Get Role By ID
**GET** `/api/v1/roles/{id}`
**Auth Required:** Admin Only

### 3. Create Role
**POST** `/api/v1/roles`
**Auth Required:** Admin Only

**Request Body:**
```json
{
  "name": "Manager",
  "description": "Department manager"
}
```

### 4. Update Role
**PUT** `/api/v1/roles/{id}`
**Auth Required:** Admin Only

### 5. Delete Role
**DELETE** `/api/v1/roles/{id}`
**Auth Required:** Admin Only

### 6. Assign Role to User
**POST** `/api/v1/roles/users/{userId}/assign`
**Auth Required:** Admin Only

**Request Body:**
```json
{
  "roleId": "guid"
}
```

**Business Rule:** Cannot change role of last remaining admin.

### 7. Revoke Role from User
**DELETE** `/api/v1/roles/users/{userId}/roles/{roleId}`
**Auth Required:** Admin Only

### 8. Get User's Roles
**GET** `/api/v1/roles/users/{userId}/roles`
**Auth Required:** Admin Only

---

## Admin APIs

| Method | Endpoint | Auth Required | Role | Description |
|--------|----------|---------------|------|-------------|
| GET | `/admin/dashboard` | Yes | Admin | Get dashboard statistics |
| GET | `/admin/reports/points` | Yes | Admin | Get points report |
| GET | `/admin/reports/users` | Yes | Admin | Get users report |
| GET | `/admin/reports/redemptions` | Yes | Admin | Get redemptions report |
| GET | `/admin/reports/events` | Yes | Admin | Get events report |
| GET | `/admin/alerts/inventory` | Yes | Admin | Get low stock alerts |
| GET | `/admin/alerts/points` | Yes | Admin | Get points pool alerts |
| GET | `/admin/admin-count` | Yes | Admin | Get active admin count |
| GET | `/admin/budget` | Yes | Admin | Get current admin's budget status |
| PUT | `/admin/budget` | Yes | Admin | Set/update monthly budget limit |
| GET | `/admin/budget/history` | Yes | Admin | Get budget usage history |

### 1. Get Dashboard
**GET** `/api/v1/admin/dashboard`
**Auth Required:** Admin Only

**Response:** `200 OK`
```json
{
  "success": true,
  "data": {
    "totalUsers": 100,
    "activeUsers": 95,
    "totalPointsDistributed": 500000,
    "pendingRedemptions": 15,
    "upcomingEvents": 3,
    "lowStockProducts": 5
  }
}
```

### 2. Get Points Report
**GET** `/api/v1/admin/reports/points?startDate=2026-01-01&endDate=2026-01-31`
**Auth Required:** Admin Only

**Response:** `200 OK`
```json
{
  "success": true,
  "data": {
    "period": {
      "start": "2026-01-01T00:00:00Z",
      "end": "2026-01-31T00:00:00Z"
    },
    "totalUsers": 100,
    "totalPointsDistributed": 50000,
    "totalPointsRedeemed": 20000,
    "totalPointsInCirculation": 30000,
    "averageBalance": 300,
    "topEarners": [...]
  }
}
```

### 3. Get Users Report
**GET** `/api/v1/admin/reports/users?startDate=2026-01-01&endDate=2026-01-31`
**Auth Required:** Admin Only

### 4. Get Redemptions Report
**GET** `/api/v1/admin/reports/redemptions?startDate=2026-01-01&endDate=2026-01-31`
**Auth Required:** Admin Only

**Response:** `200 OK`
```json
{
  "success": true,
  "data": {
    "period": {...},
    "totalRedemptions": 50,
    "pendingRedemptions": 10,
    "approvedRedemptions": 35,
    "cancelledRedemptions": 5,
    "totalPointsSpent": 25000
  }
}
```

### 5. Get Events Report
**GET** `/api/v1/admin/reports/events?year=2026`
**Auth Required:** Admin Only

### 6. Get Inventory Alerts
**GET** `/api/v1/admin/alerts/inventory`
**Auth Required:** Admin Only

**Response:** `200 OK`
```json
{
  "success": true,
  "data": [
    {
      "productId": "guid",
      "productName": "Gift Card $50",
      "currentStock": 2,
      "reorderLevel": 5,
      "alertType": "Low Stock"
    }
  ]
}
```

### 7. Get Points Pool Alerts
**GET** `/api/v1/admin/alerts/points`
**Auth Required:** Admin Only

*Returns events with less than 20% remaining points.*

### 8. Get Active Admin Count
**GET** `/api/v1/admin/admin-count`
**Auth Required:** Admin Only

**Response:** `200 OK`
```json
{
  "success": true,
  "data": {
    "count": 3
  }
}
```

### 9. Get Current Budget Status
**GET** `/api/v1/admin/budget`
**Auth Required:** Admin Only

*Returns the current admin's monthly budget status and usage.*

**Response:** `200 OK`
```json
{
  "success": true,
  "data": {
    "adminUserId": "guid",
    "monthYear": 202602,
    "budgetLimit": 10000,
    "pointsAwarded": 3500,
    "remainingBudget": 6500,
    "usagePercentage": 35.0,
    "isHardLimit": false,
    "warningThreshold": 80,
    "isOverBudget": false,
    "isWarningZone": false
  }
}
```

### 10. Set/Update Monthly Budget
**PUT** `/api/v1/admin/budget`
**Auth Required:** Admin Only

*Sets or updates the current admin's monthly budget limit.*

**Request Body:**
```json
{
  "budgetLimit": 15000,
  "warningThreshold": 80,
  "isHardLimit": false
}
```

**Validation Rules:**
- Budget Limit: Required, must be > 0, max 10,000,000
- Warning Threshold: Must be between 0-100 percent
- Is Hard Limit: Boolean (when true, blocks awarding over budget)

**Response:** `200 OK`
```json
{
  "success": true,
  "data": {
    "adminUserId": "guid",
    "monthYear": 202602,
    "budgetLimit": 15000,
    "pointsAwarded": 3500,
    "remainingBudget": 11500,
    "usagePercentage": 23.33,
    "isHardLimit": false,
    "warningThreshold": 80,
    "isOverBudget": false,
    "isWarningZone": false
  },
  "message": "Budget updated successfully"
}
```

### 11. Get Budget History
**GET** `/api/v1/admin/budget/history`
**Auth Required:** Admin Only

*Returns the admin's budget usage history for the last 12 months.*

**Response:** `200 OK`
```json
{
  "success": true,
  "data": [
    {
      "monthYear": 202602,
      "monthName": "February 2026",
      "budgetLimit": 15000,
      "pointsAwarded": 3500,
      "usagePercentage": 23.33,
      "wasOverBudget": false
    },
    {
      "monthYear": 202601,
      "monthName": "January 2026",
      "budgetLimit": 10000,
      "pointsAwarded": 9500,
      "usagePercentage": 95.0,
      "wasOverBudget": false
    }
  ]
}
```

---

## Employee APIs

| Method | Endpoint | Auth Required | Role | Description |
|--------|----------|---------------|------|-------------|
| GET | `/employee/dashboard` | Yes | Any | Get employee dashboard |

### 1. Get Employee Dashboard
**GET** `/api/v1/employee/dashboard`
**Auth Required:** Bearer Token

**Response:** `200 OK`
```json
{
  "success": true,
  "data": {
    "pointsSummary": {
      "currentBalance": 1500,
      "totalEarned": 3000,
      "totalRedeemed": 1500,
      "pendingPoints": 100
    },
    "recentRedemptions": [...],
    "upcomingEvents": [...],
    "featuredProducts": [...]
  }
}
```

---

## HTTP Status Codes

| Code | Description |
|------|-------------|
| 200 | Success |
| 201 | Created |
| 400 | Bad Request |
| 401 | Unauthorized |
| 403 | Forbidden |
| 404 | Not Found |
| 409 | Conflict |
| 422 | Validation Error |
| 500 | Internal Server Error |

---

## API Summary

### Total Endpoints: 65+

| Category | Count | Description |
|----------|-------|-------------|
| Authentication | 6 | Login, register, tokens, password |
| Users | 6 | CRUD, balance, deactivation |
| Points | 7 | Accounts, transactions, awards |
| Products | 7 | Catalog management |
| Categories | 5 | Product categorization |
| Redemptions | 11 | Redemption lifecycle |
| Events | 11 | Event management, participation, awards |
| Roles | 8 | Role & permission management |
| Admin | 11 | Dashboard, reports, budget |
| Employee | 1 | Employee dashboard |

---

*All endpoints are prefixed with `/api/v1/`. Swagger documentation available at `/swagger`.*
