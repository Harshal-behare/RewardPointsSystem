# RewardPointsSystem - Project Workflow Documentation

## How The System Works in Real Life

**Document Version:** 1.2
**Last Updated:** February 2026

**Changes in v1.2:**
- Added points pool auto-calculation rule (TotalPointsPool = 1st + 2nd + 3rd place points)
- Added rank points validation (1st > 2nd > 3rd)
- Added Admin Monthly Points Budget feature for tracking awarded points

**Changes in v1.1:**
- Added one-way event flow rule (no backward status transitions)
- Added employee deactivation prerequisites (pending redemptions must be resolved)
- Added admin protection rules (last admin cannot be deactivated or demoted)

---

## Table of Contents

1. [System Overview](#system-overview)
2. [User Roles & Permissions](#user-roles--permissions)
3. [Core Business Workflows](#core-business-workflows)
4. [Real-Life Scenarios with Examples](#real-life-scenarios-with-examples)
5. [Technical Architecture](#technical-architecture)
6. [API Reference Summary](#api-reference-summary)

---

## System Overview

The **RewardPointsSystem** is an enterprise-grade employee recognition platform that allows organizations to:

- Reward employees with points for participating in company events
- Allow employees to redeem points for products/prizes
- Track points transactions and maintain audit trails
- Generate reports on employee engagement and rewards

### The Big Picture

```
┌─────────────────────────────────────────────────────────────────────┐
│                         COMPANY AGDATA                              │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│   ┌─────────────┐    Creates Events     ┌─────────────────────────┐ │
│   │   ADMIN     │ ──────────────────►   │  EVENTS                 │ │
│   │ (HR/Manager)│                       │  - Team Competition     │ │
│   └─────────────┘                       │  - Sales Challenge      │ │
│         │                               │  - Innovation Day       │ │
│         │ Awards Points                 └───────────┬─────────────┘ │
│         ▼                                           │               │
│   ┌─────────────┐    Participates      ┌───────────▼─────────────┐  │
│   │  EMPLOYEE   │ ◄────────────────────│  EVENT WINNERS          │  │
│   │  (Harshal)  │                      │  - 1st: 1000 pts        │  │
│   └─────────────┘                      │  - 2nd: 500 pts         │  │
│         │                              │  - 3rd: 250 pts         │ │
│         │ Redeems Points               └────────────────────────┘ │
│         ▼                                                           │
│   ┌─────────────────────────────────────────────────────────────┐   │
│   │                    PRODUCT CATALOG                          │   │
│   │   ┌─────────┐  ┌─────────┐  ┌─────────┐  ┌─────────┐       │   │
│   │   │ Coffee  │  │ T-Shirt │  │ Headset │  │ Day Off │       │   │
│   │   │ 100 pts │  │ 500 pts │  │1500 pts │  │5000 pts │       │   │
│   │   └─────────┘  └─────────┘  └─────────┘  └─────────┘       │   │
│   └─────────────────────────────────────────────────────────────┘   │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
```

---

## User Roles & Permissions

### Admin Role

HR managers, team leads, or designated administrators who:

- Create and manage events
- Award points to event winners
- Approve/reject redemption requests
- Manage product catalog
- View reports and analytics
- Manage user accounts

**Admin Protection Rules:**
- The last remaining admin CANNOT be deactivated
- The last remaining admin CANNOT have their role changed to Employee
- There must always be at least 1 active admin in the system
- Admin role change/deactivation is only allowed when admin count >= 2

---

## Admin Monthly Points Budget

### Overview

Admins can set a monthly points budget to track and control how many points they award. This helps prevent over-awarding and provides visibility into points distribution.

### How It Works

```
┌─────────────────────────────────────────────────────────────────┐
│                  ADMIN MONTHLY POINTS BUDGET                     │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│   Admin: Sarah (HR Manager)                                     │
│   Month: February 2026                                          │
│                                                                 │
│   ┌─────────────────────────────────────────────────────────┐   │
│   │ Monthly Budget:        50,000 points                    │   │
│   │ Points Awarded:        32,500 points  (65%)             │   │
│   │ Remaining Budget:      17,500 points                    │   │
│   │                                                         │   │
│   │ ████████████████████░░░░░░░░░░  65% used                │   │
│   └─────────────────────────────────────────────────────────┘   │
│                                                                 │
│   Recent Awards This Month:                                     │
│   ┌─────────────────────────────────────────────────────────┐   │
│   │ Feb 15 │ Sales Challenge Winners │ 2,500 pts            │   │
│   │ Feb 10 │ Innovation Day Awards   │ 5,000 pts            │   │
│   │ Feb 05 │ Team Performance Bonus  │ 10,000 pts           │   │
│   │ Feb 01 │ Monthly Recognitions    │ 15,000 pts           │   │
│   └─────────────────────────────────────────────────────────┘   │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### Budget Configuration

| Setting | Description | Default |
|---------|-------------|---------|
| Monthly Budget | Max points admin can award per month | No limit |
| Warning Threshold | Alert when budget usage reaches % | 80% |
| Hard Limit | Block awarding when budget exceeded | Optional |
| Reset Day | Day of month budget resets | 1st |

### Budget Rules

1. **Budget Period:** Resets on the 1st of each month at 00:00 UTC
2. **Tracking:** All points awarded by admin count against their budget
3. **Warning:** Notification when 80% of budget is used
4. **Soft Limit:** Warning when exceeding budget (can still award)
5. **Hard Limit (Optional):** Blocks awarding when budget exceeded

### Budget Validation Flow

```
┌─────────────────────────────────────────────────────────────────┐
│                    POINTS AWARD VALIDATION                       │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│   Admin attempts to award points                                │
│                    │                                            │
│                    ▼                                            │
│   ┌─────────────────────────────────┐                          │
│   │ Check admin's monthly budget    │                          │
│   └─────────────────────────────────┘                          │
│                    │                                            │
│         ┌─────────┴─────────┐                                  │
│         │                   │                                   │
│   No Budget Set        Budget Exists                            │
│         │                   │                                   │
│         ▼                   ▼                                   │
│   ┌───────────┐    ┌─────────────────────────────┐             │
│   │  ALLOWED  │    │ Check: Awarded + New <= Limit│             │
│   └───────────┘    └─────────────────────────────┘             │
│                             │                                   │
│              ┌──────────────┼──────────────┐                   │
│              │              │              │                    │
│         Within          Warning        Exceeds                  │
│         Budget          Zone (80%+)    Limit                    │
│              │              │              │                    │
│              ▼              ▼              ▼                    │
│        ┌─────────┐   ┌───────────┐   ┌───────────┐             │
│        │ ALLOWED │   │ ALLOWED + │   │ BLOCKED   │             │
│        │         │   │ WARNING   │   │(if hard)  │             │
│        └─────────┘   └───────────┘   │ or WARN   │             │
│                                      │(if soft)  │             │
│                                      └───────────┘             │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### API Endpoints for Budget Management

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/Admin/budget` | Get current admin's budget status |
| PUT | `/api/v1/Admin/budget` | Set/update monthly budget limit |
| GET | `/api/v1/Admin/budget/history` | Get budget usage history |

### Database Schema Addition

```
┌───────────────────────────────┐
│    AdminMonthlyBudgets        │
├───────────────────────────────┤
│ Id (PK, GUID)                 │
│ AdminUserId (FK)              │◄─── Which admin
│ MonthYear (int)               │◄─── YYYYMM format (e.g., 202602)
│ BudgetLimit (int)             │◄─── Monthly points limit
│ PointsAwarded (int)           │◄─── Points awarded this month
│ IsHardLimit (bool)            │◄─── Block or warn on exceed
│ WarningThreshold (int)        │◄─── Percentage (default 80)
│ CreatedAt                     │
│ UpdatedAt                     │
└───────────────────────────────┘

Unique: (AdminUserId, MonthYear)
```

---

### Employee Role

Regular employees who:

- View and register for events
- Earn points by winning events
- Browse product catalog
- Request redemptions
- View their points balance and transaction history

---

## User Deactivation Rules

### Employee Deactivation Prerequisites

Before an employee can be deactivated, the following conditions MUST be met:

1. **No Pending Redemptions:** All redemption requests must be in a final state
   - Admin must approve or reject ALL pending redemptions first
   - Pending redemptions block deactivation

```
┌─────────────────────────────────────────────────────────────────┐
│                    EMPLOYEE DEACTIVATION FLOW                    │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│   Admin attempts to deactivate employee                         │
│                    │                                            │
│                    ▼                                            │
│   ┌─────────────────────────────────┐                          │
│   │ Check for pending redemptions   │                          │
│   └─────────────────────────────────┘                          │
│                    │                                            │
│         ┌─────────┴─────────┐                                  │
│         │                   │                                   │
│    Has Pending          No Pending                              │
│         │                   │                                   │
│         ▼                   ▼                                   │
│   ┌───────────┐      ┌─────────────┐                           │
│   │  BLOCKED  │      │  ALLOWED    │                           │
│   │           │      │             │                           │
│   │ Must first│      │ Employee    │                           │
│   │ approve or│      │ can be      │                           │
│   │ reject all│      │ deactivated │                           │
│   │ pending   │      │             │                           │
│   └───────────┘      └─────────────┘                           │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### Admin Deactivation Prerequisites

Before an admin can be deactivated or demoted:

1. **Admin Count Check:** Must have at least 2 active admins
   - Last admin cannot be deactivated
   - Last admin cannot have role changed

2. **No Pending Redemptions:** Same rule as employees applies

```
┌─────────────────────────────────────────────────────────────────┐
│                     ADMIN DEACTIVATION FLOW                      │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│   Admin attempts to deactivate/demote another admin             │
│                    │                                            │
│                    ▼                                            │
│   ┌─────────────────────────────────┐                          │
│   │ Check active admin count        │                          │
│   └─────────────────────────────────┘                          │
│                    │                                            │
│         ┌─────────┴─────────┐                                  │
│         │                   │                                   │
│    Count = 1           Count >= 2                               │
│    (Last Admin)             │                                   │
│         │                   ▼                                   │
│         ▼           ┌─────────────────────────────┐            │
│   ┌───────────┐     │ Check for pending redemptions│            │
│   │  BLOCKED  │     └─────────────────────────────┘            │
│   │           │                 │                               │
│   │ Cannot    │      ┌─────────┴─────────┐                     │
│   │ deactivate│      │                   │                      │
│   │ or change │ Has Pending          No Pending                 │
│   │ last admin│      │                   │                      │
│   └───────────┘      ▼                   ▼                      │
│                ┌───────────┐      ┌─────────────┐              │
│                │  BLOCKED  │      │  ALLOWED    │              │
│                └───────────┘      └─────────────┘              │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## Core Business Workflows

### Workflow 1: Event Lifecycle (One-Way Status Transitions)

```
┌─────────┐    ┌──────────┐    ┌────────┐    ┌───────────┐
│  DRAFT  │───►│ UPCOMING │───►│ ACTIVE │───►│ COMPLETED │
└─────────┘    └──────────┘    └────────┘    └───────────┘
    │               │              │               │
    │               │              │               │
Admin creates   AUTO: On        AUTO: On       AUTO: After
event           registration    event date     event end
                start date                     date
```

**IMPORTANT: One-Way Flow Only**
- Events can ONLY move forward through statuses: Draft → Upcoming → Active → Completed
- NO backward transitions allowed (e.g., cannot go from Active back to Upcoming)
- Once an event reaches Completed status, it is final and cannot be changed

**Status Transition Rules:**

| From | To | Trigger | Reversible |
|------|-----|---------|------------|
| Draft | Upcoming | Auto when RegistrationStartDate is reached | NO |
| Upcoming | Active | Auto when EventDate is reached | NO |
| Active | Completed | Auto after event ends | NO |

**Visibility:** Draft = Admin only. Others = All employees.

**Registration Window:** Only between RegistrationStartDate and RegistrationEndDate. MaxParticipants unlimited if not set.

### Workflow 2: Points Earning (Event Awards)

After event completes, admin assigns top 3 ranks and awards points.

#### Points Pool Calculation Rules

**Auto-Calculation:**
```
TotalPointsPool = FirstPlacePoints + SecondPlacePoints + ThirdPlacePoints
```

**Validation Rules:**
| Rule | Validation | Example |
|------|------------|---------|
| Rank Order | 1st > 2nd > 3rd | 1000 > 500 > 250 ✓ |
| Positive Values | All points > 0 | No zero or negative values |
| Auto-Sum | Pool = Sum of ranks | 1000 + 500 + 250 = 1750 |

**Example:**
```
┌─────────────────────────────────────────────────────────────────┐
│                    EVENT POINTS CONFIGURATION                    │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│   1st Place Points:  [1000]                                     │
│   2nd Place Points:  [500]   (must be < 1st)                    │
│   3rd Place Points:  [250]   (must be < 2nd)                    │
│                                                                 │
│   ─────────────────────────────────────────────────             │
│   Total Points Pool: 1750   (auto-calculated)                   │
│                                                                 │
│   ⚠ If 2nd >= 1st: Error "2nd place must be less than 1st"     │
│   ⚠ If 3rd >= 2nd: Error "3rd place must be less than 2nd"     │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

**Points Account Fields:**
- **CurrentBalance:** Points available to spend
- **PendingPoints:** Points locked in pending redemptions
- **TotalEarned:** Lifetime points earned
- **TotalRedeemed:** Lifetime points spent

### Workflow 3: Points Redemption (Complete Flow)

**Redemption Statuses:** Pending, Approved, Rejected, Cancelled, Delivered

```
Employee Request → PENDING → APPROVED → DELIVERED
                      │          │
                      │          └─ (Admin marks delivered after giving product)
                      │
                      ├─→ CANCELLED (Employee cancels while pending)
                      │
                      └─→ REJECTED (Admin rejects with reason)
```

**Status Actions:**
| Status | Who Can Act | Available Actions |
|--------|-------------|-------------------|
| Pending | Employee | Cancel |
| Pending | Admin | Approve or Reject (reason required) |
| Approved | Admin | Mark as Delivered |
| Rejected/Cancelled/Delivered | None | Final states |

**Points Flow:**
- Request Created: PendingPoints += cost
- Approved: CurrentBalance -= cost, PendingPoints -= cost, TotalRedeemed += cost
- Rejected/Cancelled: PendingPoints -= cost (points returned)

**Inventory Flow:**
- Approved: QuantityAvailable -= qty, QuantityReserved += qty
- Delivered: QuantityReserved -= qty

---

## Real-Life Scenarios with Examples

### Scenario 1: Monthly Sales Competition

**Context:** Company AGDATA runs a monthly sales competition to motivate the sales team.

#### Step 1: Admin Creates the Event

```
Admin: Harshal (HR Manager)
Action: Create Event

Event Details:
┌────────────────────────────────────────────┐
│ Name: January Sales Challenge              │
│ Date: January 31, 2026                     │
│ Location: Main Office                      │
│ Max Participants: 50                       │
│                                            │
│ Prize Distribution:                        │
│   1st Place: 1000 points                   │
│   2nd Place: 750 points  (< 1st ✓)         │
│   3rd Place: 500 points  (< 2nd ✓)         │
│   ─────────────────────────                │
│   Total Points Pool: 2250 (auto-calculated)│
│                                            │
└────────────────────────────────────────────┘
```

**API Call (Backend):**

```http
POST /api/v1/Events
Authorization: Bearer {admin_token}
Content-Type: application/json

{
  "name": "January Sales Challenge",
  "description": "Monthly sales competition for Q1",
  "eventDate": "2026-01-31T09:00:00",
  "location": "Main Office",
  "maxParticipants": 50,
  "firstPlacePoints": 1000,
  "secondPlacePoints": 750,
  "thirdPlacePoints": 500
}

// Note: totalPointsPool is auto-calculated as 1000 + 750 + 500 = 2250
// Validation: 1st (1000) > 2nd (750) > 3rd (500) ✓
```

**Response:**

```json
{
  "success": true,
  "data": {
    "id": "guid",
    "name": "January Sales Challenge",
    "status": "Draft",
    "createdAt": "2026-01-15T10:30:00Z"
  }
}
```

#### Step 2: Admin Publishes the Event

```http
POST /api/v1/Events/evt-001/publish
Authorization: Bearer {admin_token}
```

**Result:** Event status changes from "Draft" to "Upcoming". Employees can now see and register.

#### Step 3: Employees Register

**Employee: John (Sales Rep)**

```
John opens the Employee Portal:
1. Goes to "Events" page
2. Sees "January Sales Challenge" in upcoming events
3. Clicks "Register"
```

**API Call:**

```http
POST /api/v1/Events/evt-001/register
Authorization: Bearer {john_token}
```

**Database State:**

```
EventParticipants Table:
┌─────────────┬───────────┬──────────────┬──────────┐
│ EventId     │ UserId    │ RegisteredAt │ Status   │
├─────────────┼───────────┼──────────────┼──────────┤
│ evt-001     │ john-001  │ 2026-01-16   │ Registered│
│ evt-001     │ mary-002  │ 2026-01-16   │ Registered│
│ evt-001     │ bob-003   │ 2026-01-17   │ Registered│
└─────────────┴───────────┴──────────────┴──────────┘
```

#### Step 4: Event Completes - Admin Awards Points

```
Competition Results:
  1st Place: John (Sales: $150,000)
  2nd Place: Mary (Sales: $120,000)
  3rd Place: Bob (Sales: $95,000)
```

**Admin Action:** Award points to winners

```http
POST /api/v1/Points/award
Authorization: Bearer {admin_token}
Content-Type: application/json

{
  "eventId": "evt-001",
  "awards": [
    { "userId": "john-001", "points": 1000, "rank": 1 },
    { "userId": "mary-002", "points": 750, "rank": 2 },
    { "userId": "bob-003", "points": 500, "rank": 3 }
  ]
}
```

**Result - John's Account:**

```
Before:
┌────────────────────────────────────────┐
│ Current Balance: 500 points            │
│ Total Earned: 2,500 points             │
│ Total Redeemed: 2,000 points           │
└────────────────────────────────────────┘

After:
┌────────────────────────────────────────┐
│ Current Balance: 1,500 points (+1000)  │
│ Total Earned: 3,500 points             │
│ Total Redeemed: 2,000 points           │
└────────────────────────────────────────┘

Transaction Record:
┌────────────────────────────────────────────────────────┐
│ Type: Earned                                           │
│ Points: +1000                                          │
│ Source: January Sales Challenge - 1st Place           │
│ Timestamp: 2026-01-31T17:00:00Z                        │
│ Balance After: 1,500                                   │
└────────────────────────────────────────────────────────┘
```

---

### Scenario 2: Employee Redeems Points for a Product

**Context:** John wants to redeem his points for a company hoodie.

#### Step 1: John Browses the Product Catalog

```
John's View (Employee Portal > Products):
┌─────────────────────────────────────────────────────────────────┐
│                    PRODUCT CATALOG                              │
├─────────────────────────────────────────────────────────────────┤
│ ┌─────────────┐  ┌─────────────┐  ┌─────────────┐              │
│ │ [Image]     │  │ [Image]     │  │ [Image]     │              │
│ │ Company Mug │  │ Hoodie      │  │ Bluetooth   │              │
│ │ 200 pts     │  │ 800 pts     │  │ Earbuds     │              │
│ │ In Stock: 15│  │ In Stock: 8 │  │ 1500 pts    │              │
│ │ [Redeem]    │  │ [Redeem]    │  │ In Stock: 3 │              │
│ └─────────────┘  └─────────────┘  │ [Redeem]    │              │
│                                    └─────────────┘              │
│                                                                 │
│ Your Balance: 1,500 points                                      │
└─────────────────────────────────────────────────────────────────┘
```

#### Step 2: John Requests Redemption

```
John clicks "Redeem" on Hoodie:
┌─────────────────────────────────────────┐
│ REDEEM PRODUCT                          │
├─────────────────────────────────────────┤
│ Product: Company Hoodie                 │
│ Points Cost: 800                        │
│ Quantity: 1                             │
│                                         │
│ Your Balance: 1,500 points              │
│ After Redemption: 700 points            │
│                                         │
│ [Cancel]          [Confirm Redemption]  │
└─────────────────────────────────────────┘
```

**API Call:**

```http
POST /api/v1/Redemptions
Authorization: Bearer {john_token}
Content-Type: application/json

{
  "productId": "prod-hoodie-001",
  "quantity": 1
}
```

**System Actions (Behind the Scenes):**

```
1. Validate: John has 1,500 pts >= 800 pts required ✓
2. Validate: Hoodie in stock (8 available) ✓
3. Reserve inventory: 8 - 1 = 7 available
4. Create redemption record (Status: Pending)
5. Update pending points: +800 (reserved until approval)
```

**Response:**

```json
{
  "success": true,
  "data": {
    "id": "red-001",
    "productName": "Company Hoodie",
    "pointsSpent": 800,
    "status": "Pending",
    "createdAt": "2026-01-31T18:30:00Z"
  },
  "message": "Redemption request submitted. Awaiting admin approval."
}
```

#### Step 3: Admin Reviews and Approves

**Admin Portal View:**

```
┌─────────────────────────────────────────────────────────────────┐
│                  PENDING REDEMPTIONS                            │
├─────────────────────────────────────────────────────────────────┤
│ ID       │ Employee │ Product       │ Points │ Date       │     │
├──────────┼──────────┼───────────────┼────────┼────────────┼─────┤
│ red-001  │ John     │ Company Hoodie│ 800    │ 2026-01-31 │ [✓] │
│ red-002  │ Mary     │ Coffee Mug    │ 200    │ 2026-01-31 │ [✓] │
└─────────────────────────────────────────────────────────────────┘
```

**Admin Action:** Click approve (✓) on John's request

```http
PUT /api/v1/Redemptions/red-001/approve
Authorization: Bearer {admin_token}
```

**System Actions:**

```
1. Update redemption status: Pending → Approved
2. Deduct points from John's account: 1,500 - 800 = 700
3. Release pending points: -800
4. Finalize inventory reduction
5. Record transaction
6. Set approver info (Admin: Sarah, Time: 2026-01-31T19:00:00Z)
```

**John's Updated Account:**

```
┌────────────────────────────────────────────────────┐
│ Current Balance: 700 points                        │
│ Total Earned: 3,500 points                         │
│ Total Redeemed: 2,800 points                       │
│                                                    │
│ Recent Transactions:                               │
│ ┌────────────────────────────────────────────────┐ │
│ │ -800 pts │ Redeemed: Company Hoodie │ Jan 31   │ │
│ │ +1000 pts│ Earned: Sales Challenge   │ Jan 31   │ │
│ │ -200 pts │ Redeemed: Coffee Mug      │ Jan 15   │ │
│ └────────────────────────────────────────────────┘ │
└────────────────────────────────────────────────────┘
```

---

### Scenario 3: New Employee Onboarding

**Context:** A new employee, Alice, joins the company.

#### Step 1: Admin Creates User Account

```http
POST /api/v1/Users
Authorization: Bearer {admin_token}
Content-Type: application/json

{
  "email": "alice@company.com",
  "firstName": "Alice",
  "lastName": "Johnson",
  "roleId": 2
}
```

**System Actions:**

```
1. Create User record
2. Generate temporary password
3. Assign "Employee" role
4. Create UserPointsAccount with 0 balance
5. Send welcome email with login credentials (if email service configured)
```

#### Step 2: Alice Logs In and Changes Password

**Login Flow:**

```
┌─────────────────────────────────────────┐
│              LOGIN                       │
├─────────────────────────────────────────┤
│ Email: alice@agdata.com                │
│ Password: ********                      │
│                                         │
│           [LOGIN]                       │
└─────────────────────────────────────────┘
```

```http
POST /api/v1/Auth/login
Content-Type: application/json

{
  "email": "alice@company.com",
  "password": "TempPass123!"
}
```

**Response:**

```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    "refreshToken": "a1b2c3d4e5f6...",
    "expiresIn": 1800,
    "user": {
      "id": "alice-001",
      "email": "alice@company.com",
      "firstName": "Alice",
      "lastName": "Johnson",
      "role": "Employee"
    }
  }
}
```

#### Step 3: Alice Views Her Dashboard

**Employee Dashboard:**

```
┌─────────────────────────────────────────────────────────────────┐
│  Welcome, Alice!                                                │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐ │
│  │ POINTS BALANCE  │  │ EVENTS JOINED   │  │ REDEMPTIONS     │ │
│  │                 │  │                 │  │                 │ │
│  │      0 pts      │  │       0         │  │       0         │ │
│  │                 │  │                 │  │                 │ │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘ │
│                                                                 │
│  UPCOMING EVENTS                                                │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │ February Innovation Day │ Feb 15, 2026 │ 2000 pts │ [Join] │ │
│  │ Team Building Challenge │ Feb 28, 2026 │ 1500 pts │ [Join] │ │
│  └────────────────────────────────────────────────────────────┘ │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

---

### Scenario 4: Admin Dashboard & Reporting

**Context:** Sarah (HR Manager) reviews monthly engagement metrics.

#### Admin Dashboard View

```
┌─────────────────────────────────────────────────────────────────────┐
│                    ADMIN DASHBOARD                                  │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  KEY METRICS (January 2026)                                         │
│  ┌──────────────┐ ┌──────────────┐ ┌──────────────┐ ┌─────────────┐│
│  │ Total Users  │ │ Points Given │ │ Redemptions  │ │ Events Held ││
│  │     156      │ │   45,000     │ │     89       │ │      5      ││
│  │   +12 ↑      │ │  +15,000 ↑   │ │    +23 ↑     │ │    +2 ↑     ││
│  └──────────────┘ └──────────────┘ └──────────────┘ └─────────────┘│
│                                                                     │
│  LEADERBOARD                          LOW INVENTORY ALERTS          │
│  ┌────────────────────────────────┐  ┌────────────────────────────┐│
│  │ 1. John S.    │ 3,500 pts     │  │ ⚠ Bluetooth Earbuds: 3     ││
│  │ 2. Mary K.    │ 2,800 pts     │  │ ⚠ Company Hoodie: 7        ││
│  │ 3. Bob T.     │ 2,100 pts     │  │ ✓ Coffee Mug: 45           ││
│  │ 4. Alice J.   │ 1,500 pts     │  └────────────────────────────┘│
│  │ 5. Tom H.     │ 1,200 pts     │                                 │
│  └────────────────────────────────┘  PENDING APPROVALS: 3          │
│                                                                     │
│  POINTS DISTRIBUTION (Last 30 Days)                                 │
│  ┌────────────────────────────────────────────────────────────────┐│
│  │ ████████████████████░░░░░░░░░░░░░░░░░░░░░░░░░  45%             ││
│  │ Events                                         (20,250 pts)     ││
│  │ ████████████░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░  28%             ││
│  │ Performance Bonuses                            (12,600 pts)     ││
│  │ ████████░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░  18%             ││
│  │ Recognition Awards                             (8,100 pts)      ││
│  │ ████░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░   9%             ││
│  │ Other                                          (4,050 pts)      ││
│  └────────────────────────────────────────────────────────────────┘│
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
```

**API Calls for Dashboard:**

```http
GET /api/v1/Admin/dashboard
GET /api/v1/Admin/reports/points?startDate=2026-01-01&endDate=2026-01-31
GET /api/v1/Admin/alerts/inventory
GET /api/v1/Points/leaderboard?top=10
```

---

### Scenario 5: Rejection Flow

**Context:** An employee tries to redeem a product, but admin rejects it.

#### Reason for Rejection

```
Employee: Bob
Product: Bluetooth Earbuds (1500 pts)
Issue: Bob already redeemed this item last month (policy: max 1 per quarter)
```

**Admin Action:**

```http
PUT /api/v1/Redemptions/red-003/reject
Authorization: Bearer {admin_token}
Content-Type: application/json

{
  "reason": "Policy violation: Item already redeemed this quarter. Please wait until April."
}
```

**System Actions:**

```
1. Update redemption status: Pending → Rejected
2. Return reserved points to Bob's account
3. Release inventory reservation
4. Record rejection reason and timestamp
```

**Bob's Notification (in portal):**

```
┌─────────────────────────────────────────────────────────────────┐
│ ⚠ REDEMPTION REJECTED                                           │
├─────────────────────────────────────────────────────────────────┤
│ Product: Bluetooth Earbuds                                      │
│ Points: 1,500 (returned to your account)                        │
│ Reason: Policy violation: Item already redeemed this quarter.   │
│         Please wait until April.                                │
│ Date: January 31, 2026                                          │
└─────────────────────────────────────────────────────────────────┘
```

---

## Technical Architecture

### System Components

```
┌─────────────────────────────────────────────────────────────────────┐
│                           CLIENT LAYER                              │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │                   Angular 21 SPA                             │   │
│  │   ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐   │   │
│  │   │  Auth    │  │  Admin   │  │ Employee │  │  Shared  │   │   │
│  │   │ Module   │  │ Features │  │ Features │  │Components│   │   │
│  │   └──────────┘  └──────────┘  └──────────┘  └──────────┘   │   │
│  └─────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────┘
                                   │
                                   │ HTTPS (JWT Auth)
                                   ▼
┌─────────────────────────────────────────────────────────────────────┐
│                           API LAYER                                 │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │                   ASP.NET Core 8 Web API                     │   │
│  │   ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐   │   │
│  │   │  Auth    │  │  Events  │  │  Points  │  │ Products │   │   │
│  │   │Controller│  │Controller│  │Controller│  │Controller│   │   │
│  │   └──────────┘  └──────────┘  └──────────┘  └──────────┘   │   │
│  └─────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────┘
                                   │
                                   │ Dependency Injection
                                   ▼
┌─────────────────────────────────────────────────────────────────────┐
│                         APPLICATION LAYER                           │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │                    Business Logic Services                   │   │
│  │   ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐   │   │
│  │   │  User    │  │  Event   │  │ Points   │  │Redemption│   │   │
│  │   │ Service  │  │ Service  │  │ Service  │  │Orchestratr│  │   │
│  │   └──────────┘  └──────────┘  └──────────┘  └──────────┘   │   │
│  └─────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────┘
                                   │
                                   │ Repository Pattern
                                   ▼
┌─────────────────────────────────────────────────────────────────────┐
│                       INFRASTRUCTURE LAYER                          │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │   Entity Framework Core 9 + SQL Server                       │   │
│  │   ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐   │   │
│  │   │  Users   │  │  Events  │  │ Points   │  │ Products │   │   │
│  │   │  Table   │  │  Table   │  │ Tables   │  │  Tables  │   │   │
│  │   └──────────┘  └──────────┘  └──────────┘  └──────────┘   │   │
│  └─────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────┘
```

### Database Entity Relationships (Actual Schema)

**Database:** RewardPointsDB (SQL Server)
**Total Tables:** 13

#### Complete Table Schema

```
┌─────────────────────────────────────────────────────────────────────────┐
│                              USERS MODULE                                │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  ┌─────────────────────┐         ┌─────────────────┐                   │
│  │       Users         │         │      Roles      │                   │
│  ├─────────────────────┤         ├─────────────────┤                   │
│  │ Id (PK, GUID)       │───┐     │ Id (PK, GUID)   │                   │
│  │ Email (unique)      │   │     │ Name (unique)   │                   │
│  │ FirstName           │   │     │ Description     │                   │
│  │ LastName            │   │     │ IsActive        │                   │
│  │ PasswordHash        │   │     │ CreatedAt       │                   │
│  │ IsActive            │   │     └────────┬────────┘                   │
│  │ CreatedAt           │   │              │                            │
│  │ UpdatedAt           │   │   ┌──────────┴──────────┐                 │
│  │ UpdatedBy (FK)      │   │   │     UserRoles       │                 │
│  │ PasswordResetToken  │   └──►├─────────────────────┤                 │
│  │ PasswordResetExpiry │       │ UserId (PK, FK)     │                 │
│  └─────────────────────┘       │ RoleId (PK, FK)     │                 │
│            │                   │ AssignedAt          │                 │
│            │                   │ AssignedBy (FK)     │                 │
│            │                   │ IsActive            │                 │
│            │                   └─────────────────────┘                 │
│            │                                                           │
│            ▼                                                           │
│  ┌─────────────────────┐                                               │
│  │   RefreshTokens     │                                               │
│  ├─────────────────────┤                                               │
│  │ Id (PK, GUID)       │                                               │
│  │ UserId (FK)         │                                               │
│  │ Token (unique)      │                                               │
│  │ ExpiresAt           │                                               │
│  │ CreatedAt           │                                               │
│  │ CreatedByIp         │                                               │
│  │ IsRevoked           │                                               │
│  │ RevokedAt           │                                               │
│  │ RevokedByIp         │                                               │
│  │ ReplacedByToken     │                                               │
│  │ RevocationReason    │                                               │
│  └─────────────────────┘                                               │
└─────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────┐
│                             POINTS MODULE                                │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  ┌───────────────────────────┐                                          │
│  │    UserPointsAccounts     │                                          │
│  ├───────────────────────────┤                                          │
│  │ Id (PK, GUID)             │                                          │
│  │ UserId (FK, unique)       │◄─── One per user                        │
│  │ CurrentBalance (>= 0)     │                                          │
│  │ TotalEarned (>= 0)        │                                          │
│  │ TotalRedeemed (>= 0)      │                                          │
│  │ PendingPoints (default 0) │                                          │
│  │ CreatedAt                 │                                          │
│  │ LastUpdatedAt             │                                          │
│  │ UpdatedBy (FK)            │                                          │
│  └────────────┬──────────────┘                                          │
│               │                                                         │
│               │ 1:N                                                     │
│               ▼                                                         │
│  ┌───────────────────────────┐                                          │
│  │  UserPointsTransactions   │                                          │
│  ├───────────────────────────┤                                          │
│  │ Id (PK, GUID)             │                                          │
│  │ UserId (FK)               │                                          │
│  │ UserPoints                │◄─── +/- points amount                   │
│  │ TransactionType           │◄─── Earned/Redeemed/Adjusted            │
│  │ TransactionSource         │◄─── Event/Redemption/Admin              │
│  │ SourceId (GUID)           │◄─── Reference to Event/Redemption       │
│  │ Description               │                                          │
│  │ Timestamp                 │                                          │
│  │ BalanceAfter              │                                          │
│  └───────────────────────────┘                                          │
└─────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────┐
│                             EVENTS MODULE                                │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  ┌───────────────────────────┐       ┌───────────────────────────┐     │
│  │          Events           │       │    EventParticipants      │     │
│  ├───────────────────────────┤       ├───────────────────────────┤     │
│  │ Id (PK, GUID)             │──────►│ Id (PK, GUID)             │     │
│  │ Name                      │       │ EventId (FK)              │     │
│  │ Description               │       │ UserId (FK)               │     │
│  │ EventDate                 │       │ PointsAwarded             │     │
│  │ Status                    │       │ EventRank                 │     │
│  │ TotalPointsPool (> 0)     │       │ RegisteredAt              │     │
│  │ CreatedBy (FK)            │       │ AwardedAt                 │     │
│  │ CreatedAt                 │       │ AwardedBy (FK)            │     │
│  │ CompletedAt               │       │ AttendanceStatus          │     │
│  │ BannerImageUrl            │       │ CheckedInAt               │     │
│  │ Location                  │       └───────────────────────────┘     │
│  │ MaxParticipants           │                                         │
│  │ RegistrationStartDate     │       Unique: (EventId, UserId)         │
│  │ RegistrationEndDate       │                                         │
│  │ VirtualLink               │                                         │
│  └───────────────────────────┘                                         │
└─────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────┐
│                            PRODUCTS MODULE                               │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  ┌───────────────────┐                                                  │
│  │ ProductCategories │                                                  │
│  ├───────────────────┤                                                  │
│  │ Id (PK, GUID)     │                                                  │
│  │ Name (unique)     │                                                  │
│  │ Description       │                                                  │
│  │ DisplayOrder      │                                                  │
│  │ IsActive          │                                                  │
│  └────────┬──────────┘                                                  │
│           │                                                             │
│           │ 1:N                                                         │
│           ▼                                                             │
│  ┌───────────────────────────┐       ┌───────────────────────────┐     │
│  │        Products           │       │     ProductPricings       │     │
│  ├───────────────────────────┤       ├───────────────────────────┤     │
│  │ Id (PK, GUID)             │──────►│ Id (PK, GUID)             │     │
│  │ Name                      │       │ ProductId (FK)            │     │
│  │ Description               │       │ PointsCost (> 0)          │     │
│  │ Category (legacy string)  │       │ EffectiveFrom             │     │
│  │ CategoryId (FK)           │       │ EffectiveTo               │     │
│  │ ImageUrl                  │       │ IsActive                  │     │
│  │ IsActive                  │       └───────────────────────────┘     │
│  │ CreatedAt                 │                                         │
│  │ CreatedBy (FK)            │       ┌───────────────────────────┐     │
│  └────────────┬──────────────┘       │     InventoryItems        │     │
│               │                      ├───────────────────────────┤     │
│               └─────────────────────►│ Id (PK, GUID)             │     │
│                                      │ ProductId (FK, unique)    │     │
│                                      │ QuantityAvailable (>= 0)  │     │
│                                      │ QuantityReserved (>= 0)   │     │
│                                      │ ReorderLevel              │     │
│                                      │ LastRestocked             │     │
│                                      │ LastUpdated               │     │
│                                      │ UpdatedBy (FK)            │     │
│                                      └───────────────────────────┘     │
└─────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────┐
│                          REDEMPTIONS MODULE                              │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  ┌───────────────────────────┐                                          │
│  │       Redemptions         │                                          │
│  ├───────────────────────────┤                                          │
│  │ Id (PK, GUID)             │                                          │
│  │ UserId (FK)               │◄─── Who requested                       │
│  │ ProductId (FK)            │◄─── What they want                      │
│  │ PointsSpent               │                                          │
│  │ Quantity (> 0)            │                                          │
│  │ Status                    │◄─── Pending/Approved/Rejected           │
│  │ RequestedAt               │                                          │
│  │ ApprovedAt                │                                          │
│  │ ApprovedBy (FK)           │◄─── Admin who approved                  │
│  │ ProcessedAt               │                                          │
│  │ ProcessedBy (FK)          │                                          │
│  │ RejectionReason           │                                          │
│  └───────────────────────────┘                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

#### Database Constraints Summary

| Table | Constraint | Description |
|-------|------------|-------------|
| Events | TotalPointsPool > 0 | Must have positive points pool |
| InventoryItems | QuantityAvailable >= 0 | Cannot go negative |
| InventoryItems | QuantityReserved >= 0 | Cannot go negative |
| ProductPricings | PointsCost > 0 | Must cost something |
| Redemptions | Quantity > 0 | Must redeem at least 1 |
| UserPointsAccounts | CurrentBalance >= 0 | Cannot have negative balance |
| UserPointsAccounts | TotalEarned >= 0 | Cannot be negative |
| UserPointsAccounts | TotalRedeemed >= 0 | Cannot be negative |

#### Unique Indexes

| Table | Columns | Purpose |
|-------|---------|---------|
| Users | Email | One account per email |
| Roles | Name | Unique role names |
| ProductCategories | Name | Unique category names |
| UserPointsAccounts | UserId | One points account per user |
| InventoryItems | ProductId | One inventory record per product |
| EventParticipants | EventId, UserId | One registration per user per event |
| RefreshTokens | Token | Unique refresh tokens |

---

## API Reference Summary

### Authentication

| Method | Endpoint                     | Description           |
| ------ | ---------------------------- | --------------------- |
| POST   | /api/v1/Auth/register        | Register new user     |
| POST   | /api/v1/Auth/login           | User login            |
| POST   | /api/v1/Auth/refresh         | Refresh access token  |
| POST   | /api/v1/Auth/logout          | Logout user           |
| POST   | /api/v1/Auth/change-password | Change password       |
| GET    | /api/v1/Auth/me              | Get current user info |

### Users (Admin only)

| Method | Endpoint           | Description      |
| ------ | ------------------ | ---------------- |
| GET    | /api/v1/Users      | List all users   |
| GET    | /api/v1/Users/{id} | Get user details |
| POST   | /api/v1/Users      | Create new user  |
| PUT    | /api/v1/Users/{id} | Update user      |

### Events

| Method | Endpoint                         | Description           |
| ------ | -------------------------------- | --------------------- |
| GET    | /api/v1/Events                   | List all events       |
| GET    | /api/v1/Events/{id}              | Get event details     |
| POST   | /api/v1/Events                   | Create event (Admin)  |
| PUT    | /api/v1/Events/{id}              | Update event (Admin)  |
| POST   | /api/v1/Events/{id}/publish      | Publish event (Admin) |
| POST   | /api/v1/Events/{id}/register     | Register for event    |
| GET    | /api/v1/Events/{id}/participants | List participants     |

### Points

| Method | Endpoint                    | Description                |
| ------ | --------------------------- | -------------------------- |
| GET    | /api/v1/Points/account      | Get current user's account |
| GET    | /api/v1/Points/transactions | Get transaction history    |
| POST   | /api/v1/Points/award        | Award points (Admin)       |
| POST   | /api/v1/Points/deduct       | Deduct points (Admin)      |

### Products

| Method | Endpoint              | Description            |
| ------ | --------------------- | ---------------------- |
| GET    | /api/v1/Products      | List all products      |
| GET    | /api/v1/Products/{id} | Get product details    |
| POST   | /api/v1/Products      | Create product (Admin) |
| PUT    | /api/v1/Products/{id} | Update product (Admin) |

### Redemptions

| Method | Endpoint                         | Description        |
| ------ | -------------------------------- | ------------------ |
| GET    | /api/v1/Redemptions              | List redemptions   |
| POST   | /api/v1/Redemptions              | Request redemption |
| PUT    | /api/v1/Redemptions/{id}/approve | Approve (Admin)    |
| PUT    | /api/v1/Redemptions/{id}/reject  | Reject (Admin)     |

### Admin Dashboard

| Method | Endpoint                       | Description          |
| ------ | ------------------------------ | -------------------- |
| GET    | /api/v1/Admin/dashboard        | Dashboard stats      |
| GET    | /api/v1/Admin/reports/points   | Points report        |
| GET    | /api/v1/Admin/reports/users    | User activity report |
| GET    | /api/v1/Admin/alerts/inventory | Low stock alerts     |

---

## Glossary

| Term               | Definition                                                     |
| ------------------ | -------------------------------------------------------------- |
| **Points**         | Virtual currency earned by employees and redeemed for products |
| **Event**          | Company activity where employees can earn points               |
| **Redemption**     | Process of exchanging points for a product                     |
| **Points Pool**    | Total points available for distribution in an event            |
| **Pending Points** | Points reserved during a pending redemption request            |

---

_Document maintained by the Development Team. For questions, contact the system administrator._
