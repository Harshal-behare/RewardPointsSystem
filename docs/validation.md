# RewardPointsSystem - Validation Rules

**Document Version:** 1.1
**Last Updated:** February 2026

This document lists all form validations currently implemented in the system and validations that need to be added.

---

## Table of Contents

1. [Authentication Validations](#authentication-validations)
2. [User Management Validations](#user-management-validations)
3. [Event Validations](#event-validations)
4. [Product Validations](#product-validations)
5. [Redemption Validations](#redemption-validations)
6. [Category Validations](#category-validations)
7. [Validations To Be Added](#validations-to-be-added)

---

## Authentication Validations

### Login Form

| Field | Current Validations | Status |
|-------|---------------------|--------|
| Email | Required, Valid email format, Max 255 chars, Must end with @agdata.com | DONE |
| Password | Required, Min 8 chars, Max 20 chars, 1 uppercase, 1 lowercase, 1 number, 1 special char | DONE |

**Implementation:** `login.component.ts`

### Change Password Form (Admin & Employee Profile)

| Field | Current Validations | Status |
|-------|---------------------|--------|
| Current Password | Required | DONE |
| New Password | Required, Min 8 chars, Max 20 chars, 1 uppercase, 1 lowercase, 1 number, 1 special char | DONE |
| Confirm Password | Required, Must match new password | DONE |

**Implementation:** `admin/profile.component.ts`, `employee/profile.component.ts`

---

## User Management Validations

### Create/Edit User Form

| Field | Current Validations | Status |
|-------|---------------------|--------|
| Email | Required, Valid email format | DONE |
| First Name | Required, 2-100 characters | DONE |
| Last Name | Required, 2-100 characters | DONE |
| Role | Required, Must be valid role ID | DONE |
| Password (Create only) | Required, Min 8 chars, Max 20 chars, 1 uppercase, 1 lowercase, 1 number, 1 special char | DONE |

**Implementation:** `users.component.ts` (admin)

### User Deactivation

| Rule | Current Validations | Status |
|------|---------------------|--------|
| Pending Redemptions Check | Employee cannot be deactivated if they have pending redemptions | DONE |
| Last Admin Protection | Last admin cannot be deactivated | DONE |
| Last Admin Role Change | Last admin cannot have role changed to Employee | DONE |

**Implementation:** `users.component.ts` (frontend), `UserService.cs` (backend)

---

## Event Validations

### Create/Edit Event Form

| Field | Current Validations | Status |
|-------|---------------------|--------|
| Name | Required, 3-200 characters | DONE |
| Description | Optional, Max 1000 characters | DONE |
| Event Date | Required, Must be future date (for new events) | DONE |
| Event End Date | Optional, Must be after Event Date | DONE |
| Points Pool | Required, Must be > 0, Max 1,000,000 | DONE |
| Max Participants | Optional, Must be >= 0 if provided | DONE |
| Location | Optional, Max 500 characters | DONE |
| Virtual Link | Optional, Max 1000 characters, Valid URL format | DONE |
| Registration Start Date | Optional, Must be before Event Date | DONE |
| Registration End Date | Optional, Must be before Event Date, After Registration Start Date | DONE |
| Banner Image URL | Optional, Max 500 characters, Valid URL format | DONE |

### Event Status Transitions

| Rule | Current Validations | Status |
|------|---------------------|--------|
| One-Way Flow | Events can only move forward: Draft → Upcoming → Active → Completed | DONE |
| No Backward Transition | Cannot revert event to previous status | DONE |

**Implementation:** `events.component.ts` (frontend), `EventService.cs` (backend)

### Event Registration

| Rule | Current Validations | Status |
|------|---------------------|--------|
| Registration Window | Only allowed between RegistrationStartDate and RegistrationEndDate | DONE |
| Max Participants | Cannot exceed MaxParticipants if set | DONE |
| Duplicate Registration | User cannot register twice for same event | DONE |
| Event Status | Can only register for Upcoming events | DONE |

**Implementation:** `events.component.ts` (admin)

---

## Product Validations

### Create/Edit Product Form

| Field | Current Validations | Status |
|-------|---------------------|--------|
| Name | Required, 2-200 characters | DONE |
| Description | Optional, Max 1000 characters | DONE |
| Category ID | Optional, Must be valid category if provided | DONE |
| Points Price | Required, Must be > 0 | DONE |
| Stock Quantity | Required, Must be >= 0 | DONE |
| Image URL | Optional, Max 500 characters, Must start with http:// or https:// | DONE |
| Is Active | Boolean, defaults to true | DONE |

**Implementation:** `products.component.ts` (admin)

---

## Redemption Validations

### Create Redemption (Employee)

| Field | Current Validations | Status |
|-------|---------------------|--------|
| Product ID | Required, Must exist | DONE |
| Quantity | Required, Min 1, Max 10 | DONE |
| Sufficient Points | User must have enough available points | DONE |
| Stock Available | Product must have sufficient stock | DONE |
| Product Active | Product must be active | DONE |

### Reject Redemption (Admin)

| Field | Current Validations | Status |
|-------|---------------------|--------|
| Rejection Reason | Required, Min 10 characters, Max 500 characters | DONE |

### Status Transitions

| Rule | Current Validations | Status |
|------|---------------------|--------|
| Cancel | Only allowed when status is Pending | DONE |
| Approve | Only allowed when status is Pending | DONE |
| Reject | Only allowed when status is Pending, Reason required | DONE |
| Deliver | Only allowed when status is Approved | DONE |

**Implementation:** `redemptions.component.ts` (admin), `products.component.ts` (employee)

---

## Category Validations

### Create/Edit Category Form

| Field | Current Validations | Status |
|-------|---------------------|--------|
| Name | Required, 2-100 characters, Unique | DONE |
| Description | Optional, Max 500 characters | DONE |
| Display Order | Optional, Must be >= 0 | DONE |
| Is Active | Boolean, defaults to true | DONE |

**Implementation:** `products.component.ts` (admin - category modal)

---

## Validations To Be Added

### HIGH Priority (Backend + Frontend)

#### 1. User Deactivation - Pending Redemptions Check

**Rule:** Employee cannot be deactivated if they have pending redemptions.

**Frontend Validation:**
```typescript
// Before deactivating user
async canDeactivateUser(userId: string): Promise<boolean> {
  const pendingCount = await this.getPendingRedemptionsCount(userId);
  if (pendingCount > 0) {
    this.toast.error(`Cannot deactivate user. ${pendingCount} pending redemption(s) must be approved or rejected first.`);
    return false;
  }
  return true;
}
```

**Backend Validation:**
```csharp
// UserService.DeactivateUser()
var pendingRedemptions = await _unitOfWork.Redemptions
    .GetByUserIdAsync(userId, RedemptionStatus.Pending);
if (pendingRedemptions.Any())
{
    throw new ValidationException("Cannot deactivate user with pending redemptions. Please approve or reject all pending redemptions first.");
}
```

**Location:**
- Frontend: `users.component.ts`
- Backend: `UserService.cs`

---

#### 2. Last Admin Protection - Deactivation

**Rule:** The last remaining admin cannot be deactivated.

**Frontend Validation:**
```typescript
// Before deactivating admin
async canDeactivateAdmin(userId: string): Promise<boolean> {
  const activeAdminCount = await this.getActiveAdminCount();
  if (activeAdminCount <= 1) {
    this.toast.error('Cannot deactivate the last remaining admin.');
    return false;
  }
  return true;
}
```

**Backend Validation:**
```csharp
// UserService.DeactivateUser()
if (user.Roles.Contains("Admin"))
{
    var activeAdminCount = await _unitOfWork.Users.CountActiveAdminsAsync();
    if (activeAdminCount <= 1)
    {
        throw new ValidationException("Cannot deactivate the last remaining admin. At least one admin must exist in the system.");
    }
}
```

**Location:**
- Frontend: `users.component.ts`
- Backend: `UserService.cs`

---

#### 3. Last Admin Protection - Role Change

**Rule:** The last remaining admin cannot have their role changed to Employee.

**Frontend Validation:**
```typescript
// Before changing admin role
async canChangeAdminRole(userId: string, newRole: string): Promise<boolean> {
  if (newRole.toLowerCase() !== 'admin') {
    const activeAdminCount = await this.getActiveAdminCount();
    if (activeAdminCount <= 1) {
      this.toast.error('Cannot change role of the last remaining admin.');
      return false;
    }
  }
  return true;
}
```

**Backend Validation:**
```csharp
// UserService.UpdateUserRole()
if (currentRole == "Admin" && newRole != "Admin")
{
    var activeAdminCount = await _unitOfWork.Users.CountActiveAdminsAsync();
    if (activeAdminCount <= 1)
    {
        throw new ValidationException("Cannot change role of the last remaining admin. At least one admin must exist in the system.");
    }
}
```

**Location:**
- Frontend: `users.component.ts`
- Backend: `UserService.cs`

---

#### 4. Event Status - One-Way Flow

**Rule:** Event status can only move forward (Draft → Upcoming → Active → Completed). No backward transitions allowed.

**Frontend Validation:**
```typescript
// In event edit modal
canChangeStatus(currentStatus: string, newStatus: string): boolean {
  const statusOrder = ['Draft', 'Upcoming', 'Active', 'Completed'];
  const currentIndex = statusOrder.indexOf(currentStatus);
  const newIndex = statusOrder.indexOf(newStatus);

  if (newIndex <= currentIndex) {
    this.toast.error('Event status can only move forward.');
    return false;
  }
  return true;
}
```

**Backend Validation:**
```csharp
// EventService.UpdateEventStatus()
private static readonly Dictionary<EventStatus, EventStatus[]> AllowedTransitions = new()
{
    { EventStatus.Draft, new[] { EventStatus.Upcoming } },
    { EventStatus.Upcoming, new[] { EventStatus.Active } },
    { EventStatus.Active, new[] { EventStatus.Completed } },
    { EventStatus.Completed, Array.Empty<EventStatus>() }
};

public async Task UpdateEventStatusAsync(Guid eventId, EventStatus newStatus)
{
    var currentEvent = await GetByIdAsync(eventId);
    if (!AllowedTransitions[currentEvent.Status].Contains(newStatus))
    {
        throw new ValidationException($"Cannot transition event from {currentEvent.Status} to {newStatus}. Events can only move forward.");
    }
}
```

**Location:**
- Frontend: `events.component.ts`
- Backend: `EventService.cs`

---

### MEDIUM Priority

#### 5. Password Strength Validation (Login/Change Password)

**Rule:** Password must meet complexity requirements.

**Validations:**
- Min 8 characters
- Max 20 characters
- At least 1 uppercase letter
- At least 1 lowercase letter
- At least 1 number
- At least 1 special character (!@#$%^&*()_+-=[]{};\':"|,.<>/?)

**Status:** DONE

**Implementation:**
- `login.component.ts` - Login form password validation
- `admin/profile.component.ts` - Admin change password
- `employee/profile.component.ts` - Employee change password
- `users.component.ts` - Create user password validation

---

#### 6. Points Award Validation

**Rule:** Total points awarded cannot exceed event's remaining points pool.

**Validations:**
- Sum of all awards <= remaining points pool
- Cannot award more than event's total pool
- Cannot award to non-participants

**Status:** PARTIAL (backend has some validation)

---

## Validation Summary

### Currently Implemented: 50+ validations

| Area | Implemented | To Be Added |
|------|-------------|-------------|
| Authentication | 7 | 0 |
| User Management | 8 | 0 |
| Events | 17 | 0 |
| Products | 8 | 0 |
| Redemptions | 10 | 0 |
| Categories | 4 | 0 |

### All Critical Validations COMPLETED

| # | Validation | Frontend | Backend |
|---|------------|----------|---------|
| 1 | User deactivation - pending redemptions check | `users.component.ts` | `UserService.cs` |
| 2 | Last admin protection - deactivation block | `users.component.ts` | `UserService.cs` |
| 3 | Last admin protection - role change block | `users.component.ts` | `UserService.cs` |
| 4 | Event status - one-way flow enforcement | `events.component.ts` | `EventService.cs` |

---

## Admin Budget Validations

### Set Budget Form

| Field | Current Validations | Status |
|-------|---------------------|--------|
| Budget Limit | Required, Must be > 0, Max 10,000,000 | DONE |
| Warning Threshold | Must be between 0-100 percent | DONE |
| Is Hard Limit | Boolean, defaults to false | DONE |

**Implementation:** `SetBudgetDtoValidator.cs` (backend)

---

## Event Winner Award Validations

### Bulk Award Winners

| Rule | Current Validations | Status |
|------|---------------------|--------|
| Awards List | Cannot be empty | DONE |
| Ranks | Must be unique, 1-3 only | DONE |
| Points Match | Must match event prize configuration if set | DONE |
| Participant Check | User must be registered for event | DONE |
| Already Awarded | Cannot award twice to same user | DONE |
| Points Pool | Sum of awards cannot exceed remaining pool | DONE |

**Implementation:** `EventsController.cs`, `PointsAwardingService.cs`

---

## Event Unregister Validations

| Rule | Current Validations | Status |
|------|---------------------|--------|
| Authorization | Only self or admin can unregister | DONE |
| Event Status | Cannot unregister from Active/Completed events | DONE |
| Points Awarded | Cannot unregister if already awarded points | DONE |

**Implementation:** `EventsController.cs`, `EventParticipationService.cs`

---

_All validations are implemented on both frontend (for UX) and backend (for security)._
