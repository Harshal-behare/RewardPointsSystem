# Reward Points System - API Documentation

> **Base URL:** `/api/v1/`  
> **Authentication:** JWT Bearer Token  
> **Last Updated:** January 18, 2026

---

## Table of Contents
1. [Overview](#overview)
2. [Authentication APIs](#authentication-apis)
3. [User APIs](#user-apis)
4. [Points APIs](#points-apis)
5. [Products APIs](#products-apis)
6. [Redemptions APIs](#redemptions-apis)
7. [Events APIs](#events-apis)
8. [Roles APIs](#roles-apis)
9. [Admin APIs](#admin-apis)
10. [MVP Requirements](#mvp-api-requirements)

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
  "timestamp": "2026-01-18T12:00:00Z"
}
```

---

## Authentication APIs

| Method | Endpoint | Auth Required | Description |
|--------|----------|---------------|-------------|
| POST | `/auth/register` | ❌ No | Register new user |
| POST | `/auth/login` | ❌ No | Login with credentials |
| POST | `/auth/refresh` | ❌ No | Refresh access token |
| POST | `/auth/logout` | ✅ Yes | Logout user |
| GET | `/auth/me` | ✅ Yes | Get current user info |

### 1. Register User
**POST** `/api/v1/auth/register`

**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "SecurePass123!",
  "confirmPassword": "SecurePass123!",
  "firstName": "John",
  "lastName": "Doe"
}
```

**Response:** `201 Created`
```json
{
  "userId": "guid",
  "email": "user@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "accessToken": "jwt-token",
  "refreshToken": "refresh-token",
  "expiresAt": "2026-01-18T13:00:00Z",
  "roles": ["Employee"]
}
```

### 2. Login
**POST** `/api/v1/auth/login`

**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "SecurePass123!"
}
```

**Response:** `200 OK` (Same as register response)

### 3. Refresh Token
**POST** `/api/v1/auth/refresh`

**Request Body:**
```json
{
  "refreshToken": "refresh-token-here"
}
```

### 4. Logout
**POST** `/api/v1/auth/logout`  
**Auth Required:** ✅ Bearer Token

### 5. Get Current User
**GET** `/api/v1/auth/me`  
**Auth Required:** ✅ Bearer Token

---

## User APIs

| Method | Endpoint | Auth Required | Role | Description |
|--------|----------|---------------|------|-------------|
| GET | `/users` | ✅ Yes | Admin | Get all users (paginated) |
| GET | `/users/{id}` | ✅ Yes | Any | Get user by ID |
| POST | `/users` | ✅ Yes | Admin | Create new user |
| PUT | `/users/{id}` | ✅ Yes | Any | Update user |
| DELETE | `/users/{id}` | ✅ Yes | Admin | Soft delete user |
| GET | `/users/{id}/balance` | ✅ Yes | Any | Get user points balance |

### 1. Get All Users
**GET** `/api/v1/users?page=1&pageSize=10`  
**Auth Required:** ✅ Admin Only

### 2. Get User By ID
**GET** `/api/v1/users/{id}`

### 3. Create User
**POST** `/api/v1/users`  
**Auth Required:** ✅ Admin Only

**Request Body:**
```json
{
  "email": "newuser@example.com",
  "firstName": "Jane",
  "lastName": "Doe"
}
```

### 4. Update User
**PUT** `/api/v1/users/{id}`

**Request Body:**
```json
{
  "firstName": "Updated",
  "lastName": "Name",
  "email": "updated@example.com"
}
```

### 5. Delete User (Soft Delete)
**DELETE** `/api/v1/users/{id}`  
**Auth Required:** ✅ Admin Only

### 6. Get User Balance
**GET** `/api/v1/users/{id}/balance`

**Response:**
```json
{
  "userId": "guid",
  "firstName": "John",
  "lastName": "Doe",
  "email": "john@example.com",
  "currentBalance": 1500,
  "totalEarned": 3000,
  "totalRedeemed": 1500,
  "lastTransaction": "2026-01-18T10:00:00Z"
}
```

---

## Points APIs

| Method | Endpoint | Auth Required | Role | Description |
|--------|----------|---------------|------|-------------|
| GET | `/points/accounts/{userId}` | ✅ Yes | Any | Get user's points account |
| GET | `/points/transactions/{userId}` | ✅ Yes | Any | Get user's transactions |
| GET | `/points/transactions` | ✅ Yes | Admin | Get all transactions |
| POST | `/points/award` | ✅ Yes | Admin | Award points to user |
| POST | `/points/deduct` | ✅ Yes | Admin | Deduct points from user |
| GET | `/points/leaderboard` | ✅ Yes | Any | Get points leaderboard |
| GET | `/points/summary` | ✅ Yes | Admin | Get points system summary |

### 1. Get User Points Account
**GET** `/api/v1/points/accounts/{userId}`

### 2. Get User Transactions
**GET** `/api/v1/points/transactions/{userId}?page=1&pageSize=20`

### 3. Get All Transactions
**GET** `/api/v1/points/transactions?page=1&pageSize=50`  
**Auth Required:** ✅ Admin Only

### 4. Award Points
**POST** `/api/v1/points/award`  
**Auth Required:** ✅ Admin Only

**Request Body:**
```json
{
  "userId": "guid",
  "points": 100,
  "description": "Monthly bonus",
  "eventId": "optional-guid"
}
```

### 5. Deduct Points
**POST** `/api/v1/points/deduct`  
**Auth Required:** ✅ Admin Only

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

### 7. Get Points Summary
**GET** `/api/v1/points/summary`  
**Auth Required:** ✅ Admin Only

---

## Products APIs

| Method | Endpoint | Auth Required | Role | Description |
|--------|----------|---------------|------|-------------|
| GET | `/products` | ❌ No | Any | Get all products |
| GET | `/products/{id}` | ❌ No | Any | Get product by ID |
| POST | `/products` | ✅ Yes | Admin | Create new product |
| PUT | `/products/{id}` | ✅ Yes | Admin | Update product |
| DELETE | `/products/{id}` | ✅ Yes | Admin | Delete/deactivate product |
| GET | `/products/category/{categoryId}` | ❌ No | Any | Get products by category |

### 1. Get All Products
**GET** `/api/v1/products`

**Response:**
```json
[
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
```

### 2. Get Product By ID
**GET** `/api/v1/products/{id}`

### 3. Create Product
**POST** `/api/v1/products`  
**Auth Required:** ✅ Admin Only

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

### 4. Update Product
**PUT** `/api/v1/products/{id}`  
**Auth Required:** ✅ Admin Only

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

### 5. Delete Product
**DELETE** `/api/v1/products/{id}`  
**Auth Required:** ✅ Admin Only

*Soft deletes the product by setting `isActive` to false.*

### 6. Get Products by Category
**GET** `/api/v1/products/category/{categoryId}`

---

## Redemptions APIs

| Method | Endpoint | Auth Required | Role | Description |
|--------|----------|---------------|------|-------------|
| GET | `/redemptions` | ✅ Yes | Any | Get all redemptions |
| GET | `/redemptions/{id}` | ✅ Yes | Any | Get redemption by ID |
| POST | `/redemptions` | ✅ Yes | Any | Create redemption request |
| PATCH | `/redemptions/{id}/approve` | ✅ Yes | Admin | Approve redemption |
| PATCH | `/redemptions/{id}/deliver` | ✅ Yes | Admin | Mark as delivered |
| PATCH | `/redemptions/{id}/cancel` | ✅ Yes | Any | Cancel redemption |
| GET | `/redemptions/my-redemptions` | ✅ Yes | Any | Get current user's redemptions |
| GET | `/redemptions/pending` | ✅ Yes | Admin | Get pending redemptions |
| GET | `/redemptions/history` | ✅ Yes | Admin | Get redemption history |

### 1. Create Redemption
**POST** `/api/v1/redemptions`

**Request Body:**
```json
{
  "userId": "guid",
  "productId": "guid",
  "quantity": 1
}
```

### 2. Approve Redemption
**PATCH** `/api/v1/redemptions/{id}/approve`  
**Auth Required:** ✅ Admin Only

**Request Body:**
```json
{
  "approvedBy": "admin-user-guid"
}
```

### 3. Mark as Delivered
**PATCH** `/api/v1/redemptions/{id}/deliver`  
**Auth Required:** ✅ Admin Only

### 4. Cancel Redemption
**PATCH** `/api/v1/redemptions/{id}/cancel`

**Request Body:**
```json
{
  "cancellationReason": "Customer request"
}
```

---

## Events APIs

| Method | Endpoint | Auth Required | Role | Description |
|--------|----------|---------------|------|-------------|
| GET | `/events` | ❌ No | Any | Get all events |
| GET | `/events/{id}` | ❌ No | Any | Get event by ID |
| POST | `/events` | ✅ Yes | Admin | Create new event |
| PUT | `/events/{id}` | ✅ Yes | Admin | Update event |
| DELETE | `/events/{id}` | ✅ Yes | Admin | Cancel/delete event |
| PATCH | `/events/{id}/status` | ✅ Yes | Admin | Change event status |
| GET | `/events/admin/all` | ✅ Yes | Admin | Get all events (admin view) |
| POST | `/events/{id}/participants` | ✅ Yes | Any | Register for event |

### 1. Get All Events
**GET** `/api/v1/events`

**Response:**
```json
[
  {
    "id": "guid",
    "name": "Annual Celebration",
    "description": "Company anniversary event",
    "eventDate": "2026-03-15T00:00:00Z",
    "status": "Published",
    "totalPointsPool": 10000,
    "remainingPoints": 7500,
    "createdAt": "2026-01-01T00:00:00Z"
  }
]
```

### 2. Create Event
**POST** `/api/v1/events`  
**Auth Required:** ✅ Admin Only

**Request Body:**
```json
{
  "name": "New Event",
  "description": "Event description",
  "eventDate": "2026-06-01T00:00:00Z",
  "totalPointsPool": 5000
}
```

### 3. Update Event
**PUT** `/api/v1/events/{id}`  
**Auth Required:** ✅ Admin Only

**Request Body:**
```json
{
  "name": "Updated Event Name",
  "description": "Updated description",
  "eventDate": "2026-06-15T00:00:00Z",
  "totalPointsPool": 6000
}
```

### 4. Delete/Cancel Event
**DELETE** `/api/v1/events/{id}`  
**Auth Required:** ✅ Admin Only

*Cancels the event by changing its status to Cancelled.*

### 5. Change Event Status
**PATCH** `/api/v1/events/{id}/status`  
**Auth Required:** ✅ Admin Only

**Request Body:**
```json
{
  "status": "Published"
}
```

**Available Status Values:**
- `Draft` - Event is in planning
- `Published` - Event is visible to users
- `Cancelled` - Event has been cancelled
- `Completed` - Event has ended

### 6. Get All Events (Admin View)
**GET** `/api/v1/events/admin/all`  
**Auth Required:** ✅ Admin Only

*Returns all events including drafts and cancelled events.*

### 7. Register Participant
**POST** `/api/v1/events/{id}/participants`

**Request Body:**
```json
{
  "userId": "guid"
}
```

---

## Roles APIs

| Method | Endpoint | Auth Required | Role | Description |
|--------|----------|---------------|------|-------------|
| GET | `/roles` | ✅ Yes | Admin | Get all roles |
| GET | `/roles/{id}` | ✅ Yes | Admin | Get role by ID |
| POST | `/roles` | ✅ Yes | Admin | Create new role |
| PUT | `/roles/{id}` | ✅ Yes | Admin | Update role |
| DELETE | `/roles/{id}` | ✅ Yes | Admin | Delete role |
| POST | `/users/{userId}/roles` | ✅ Yes | Admin | Assign role to user |
| DELETE | `/users/{userId}/roles/{roleId}` | ✅ Yes | Admin | Revoke role from user |
| GET | `/users/{userId}/roles` | ✅ Yes | Admin | Get user's roles |

---

## Admin APIs

| Method | Endpoint | Auth Required | Role | Description |
|--------|----------|---------------|------|-------------|
| GET | `/admin/dashboard` | ✅ Yes | Admin | Get dashboard statistics |
| GET | `/admin/reports/points` | ✅ Yes | Admin | Get points report |
| GET | `/admin/reports/users` | ✅ Yes | Admin | Get users report |
| GET | `/admin/reports/redemptions` | ✅ Yes | Admin | Get redemptions report |
| GET | `/admin/reports/events` | ✅ Yes | Admin | Get events report |
| GET | `/admin/alerts/inventory` | ✅ Yes | Admin | Get low stock alerts |
| GET | `/admin/alerts/points` | ✅ Yes | Admin | Get points pool alerts |

### 1. Get Dashboard
**GET** `/api/v1/admin/dashboard`

### 2. Get Reports
**GET** `/api/v1/admin/reports/points?startDate=2026-01-01&endDate=2026-01-31`  
**GET** `/api/v1/admin/reports/users?startDate=2026-01-01&endDate=2026-01-31`  
**GET** `/api/v1/admin/reports/redemptions?startDate=2026-01-01&endDate=2026-01-31`  
**GET** `/api/v1/admin/reports/events?year=2026`

---

## MVP API Requirements

### 🟢 CRITICAL for MVP (Must Have)

| Category | Endpoint | Priority | Reason |
|----------|----------|----------|--------|
| **Auth** | POST `/auth/register` | ⭐⭐⭐ | User onboarding |
| **Auth** | POST `/auth/login` | ⭐⭐⭐ | User authentication |
| **Auth** | POST `/auth/refresh` | ⭐⭐⭐ | Token management |
| **Auth** | GET `/auth/me` | ⭐⭐⭐ | Current user info |
| **Users** | GET `/users/{id}` | ⭐⭐⭐ | User profile |
| **Users** | GET `/users/{id}/balance` | ⭐⭐⭐ | View points balance |
| **Points** | GET `/points/accounts/{userId}` | ⭐⭐⭐ | Points overview |
| **Points** | GET `/points/transactions/{userId}` | ⭐⭐⭐ | Transaction history |
| **Products** | GET `/products` | ⭐⭐⭐ | Browse products |
| **Products** | GET `/products/{id}` | ⭐⭐⭐ | Product details |
| **Redemptions** | POST `/redemptions` | ⭐⭐⭐ | Redeem products |
| **Redemptions** | GET `/redemptions/my-redemptions` | ⭐⭐⭐ | View my redemptions |

### 🟡 IMPORTANT for MVP (Should Have)

| Category | Endpoint | Priority | Reason |
|----------|----------|----------|--------|
| **Auth** | POST `/auth/logout` | ⭐⭐ | Session security |
| **Points** | POST `/points/award` | ⭐⭐ | Admin can award points |
| **Points** | GET `/points/leaderboard` | ⭐⭐ | Gamification |
| **Products** | POST `/products` | ⭐⭐ | Admin can add products |
| **Redemptions** | PATCH `/redemptions/{id}/approve` | ⭐⭐ | Admin approval flow |
| **Redemptions** | GET `/redemptions/pending` | ⭐⭐ | Admin view pending |
| **Admin** | GET `/admin/dashboard` | ⭐⭐ | Admin overview |

### 🔵 NICE TO HAVE (Can Defer)

| Category | Endpoint | Priority | Reason |
|----------|----------|----------|--------|
| **Events** | All event APIs | ⭐ | Feature can be added later |
| **Roles** | Role management APIs | ⭐ | Can use static roles initially |
| **Admin** | Report APIs | ⭐ | Can use dashboard initially |
| **Admin** | Alert APIs | ⭐ | Can be manual process |
| **Points** | POST `/points/deduct` | ⭐ | Edge case |

---

## MVP API Summary

**Minimum Viable API Count: 12 endpoints**

```
Authentication (4):
✅ POST /auth/register
✅ POST /auth/login  
✅ POST /auth/refresh
✅ GET  /auth/me

Users (2):
✅ GET /users/{id}
✅ GET /users/{id}/balance

Points (2):
✅ GET /points/accounts/{userId}
✅ GET /points/transactions/{userId}

Products (2):
✅ GET /products
✅ GET /products/{id}

Redemptions (2):
✅ POST /redemptions
✅ GET  /redemptions/my-redemptions
```

---

## HTTP Status Codes Used

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
