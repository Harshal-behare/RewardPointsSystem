# AGDATA Reward Points System - Demo Presentation Guide

## Overview
**Company:** AGDATA Software Company
**System:** Internal Employee Reward Points System
**Tech Stack:** Angular 21 + .NET 8 + SQL Server

---

## Demo Login Credentials

| Role | Email | Password |
|------|-------|----------|
| **Admin** | Harshal.Behare@agdata.com | Harshal@123 |
| **Employee** | Sankalp.Chakre@agdata.com | Sankalp@123 |

Other demo employees use password: `Demo@123!`

---

## Pre-Demo Setup

1. Run `Database/DemoData.sql` in SQL Server Management Studio
2. Start the backend: `dotnet run --project RewardPointsSystem.Api/RewardPointsSystem.Api.csproj`
3. Start the frontend: `cd client && npm start`
4. Open browser to `http://localhost:4200`

---

## Demo Data Summary

| Entity | Count | Details |
|--------|-------|---------|
| Users | 7 | 2 Admins + 5 Employees |
| Products | 10 | Gift Cards, Tech, Merchandise, Perks |
| Events | 4 | Completed, Active, Upcoming, Draft |
| Redemptions | 5 | All status types |
| Transactions | 25+ | Credit and Debit history |

---

## Presentation Flow (25-30 minutes)

### Opening Statement (1 minute)

> "This is the **AGDATA Reward Points System** - an internal employee recognition platform. Employees earn points through participation in company events and excellent performance, then redeem points for rewards like gift cards, company merchandise, and work perks. Built with Angular 21 and .NET 8 following Clean Architecture principles."

---

### Section 1: Authentication & Security (3 minutes)

**Demo the Login Page:**

1. **Show the Login Form**
   - Point out the clean AGDATA branding
   - Explain this is for internal employees only

2. **Demonstrate Validation** (try these in order):
   - Enter `john@gmail.com` → Error: "Email must end with @agdata.com"
   - Enter `sankalp` → Error: "Invalid email format"
   - Enter correct email, password `123` → Error: "Password must be 8-20 characters with uppercase, lowercase, number, and special character"

3. **Successful Login**
   - Login as: `Sankalp.Chakre@agdata.com` / `Sankalp@123`
   - Point out: "JWT-based authentication with automatic token refresh"

---

### Section 2: Employee Dashboard (7 minutes)

**After logging in as Sankalp:**

#### 2.1 Dashboard Overview
- **Current Balance:** 1,450 points
- **Total Earned:** 2,200 points
- **Total Redeemed:** 550 points
- **Pending:** 200 points (locked in pending redemption)

> "Employees can see their complete points status at a glance. Pending points are locked until their redemption is approved or rejected."

#### 2.2 Points Transaction History
- Navigate to Points/Transaction History
- Show different transaction types:
  - "1st Place - AGDATA Code Sprint Q1 2026" (+1000)
  - "Employee of the Month" (+400)
  - "Redeemed: Amazon Gift Card" (-200)

> "Complete audit trail of all points earned and spent. Each transaction shows source, description, and running balance."

#### 2.3 Events Section
- Click on **Events**
- Show 3 visible events (Draft is hidden from employees):
  - **AGDATA Wellness Challenge** (Active) - Currently ongoing
  - **AGDATA Innovation Summit 2026** (Upcoming) - Registration open
  - **AGDATA Code Sprint Q1 2026** (Completed) - With winners shown

- Click on **AGDATA Innovation Summit 2026**:
  - Show event details, location, dates
  - Show registration button
  - Point out: "Registration only allowed between start and end dates"
  - Show participant count: "3 of 60 spots filled"

> "Employees can see all company events and register for upcoming ones. Registration is controlled by date windows."

#### 2.4 Products Catalog
- Navigate to **Products/Rewards**
- Show categories:
  - **Gift Cards:** Amazon Rs.500, Amazon Rs.1000, Swiggy Rs.300
  - **Tech & Gadgets:** Wireless Mouse (800 pts), Desk Organizer (500 pts)
  - **AGDATA Merchandise:** T-Shirt (250 pts), Hoodie (450 pts), Coffee Mug (100 pts)
  - **Work Perks:** Extra WFH Day (1500 pts), Udemy Course (600 pts)

- Filter by category to show functionality
- Click on **Extra Work From Home Day**:
  - Show product details
  - Point out: "1500 points required - Sankalp has 1450 available"
  - Try to redeem → Show "Insufficient balance" validation

> "Products are organized by category. The system validates balance before allowing redemption."

#### 2.5 My Redemptions
- Navigate to **My Redemptions**
- Show different statuses:
  - **Pending:** Amazon Gift Card Rs.500 (waiting for admin approval)
  - **Delivered:** Swiggy Voucher Rs.300 (completed)

> "Employees can track all their redemption requests and see the current status."

---

### Section 3: Admin Features (12 minutes)

**Logout and Login as Admin:**
- `Harshal.Behare@agdata.com` / `Harshal@123`

#### 3.1 Admin Dashboard
- Show key metrics:
  - **Total Users:** 7
  - **Active Users:** 7
  - **Total Points Distributed:** (calculated)
  - **Pending Redemptions:** 1
  - **Upcoming Events:** 1
  - **Low Stock Alerts:** (Extra WFH Day has only 3 left)

> "Admin dashboard provides a quick overview of system health and pending actions."

#### 3.2 User Management
- Navigate to **Users**
- Show user list with pagination
- Click on **Sankalp Chakre** to view details:
  - Points balance, transaction history
  - Role: Employee

**Demonstrate Validation - Pending Redemptions:**
- Try to **Deactivate Sankalp** → Error: "Cannot deactivate user with pending redemptions. Please approve or reject their 1 pending redemption(s) first."

> "Business rule: Users with pending redemptions cannot be deactivated - ensures no orphaned transactions."

**Demonstrate Last Admin Protection:**
- Go to **Priya Sharma** (the other admin)
- Try to deactivate her → Works fine (there are 2 admins)
- If you deactivate Priya, then try to deactivate Harshal → Error: "Cannot deactivate the last remaining admin"

> "System always ensures at least one admin exists - prevents lockout."

#### 3.3 Event Management
- Navigate to **Events** (Admin view)
- Show **4 events** including the Draft (hidden from employees)

**Event Status Flow Demo:**
```
Draft → Upcoming → Active → Completed
```
- Select **AGDATA Team Building Day** (Draft)
- Change status to **Upcoming** ✓
- Try to change **AGDATA Code Sprint** (Completed) back to Active → Error: "Event status can only move forward"

> "Events follow a one-way lifecycle. Once completed, they cannot be reopened."

**Create New Event Demo:**
- Click **Create Event**
- Fill in: "AGDATA Tech Talk Series"
- Set prizes:
  - 1st Place: 500 points
  - 2nd Place: 300 points
  - 3rd Place: 200 points
- Show **Total Points Pool auto-calculates to 1000**
- Try setting 2nd place higher than 1st → Error: "Prizes must be in descending order"

> "Points pool is automatically calculated. Validation ensures 1st > 2nd > 3rd."

#### 3.4 Award Winners Feature (Key Feature!)
- Go to **AGDATA Wellness Challenge** (Active event)
- Click **Award Winners** or complete the event
- Select winners from participants:
  - 1st Place: Sankalp Chakre - 800 points
  - 2nd Place: Amit Patel - 450 points
  - 3rd Place: Neha Gupta - 250 points
- Click **Award All** → Single API call awards all winners

**Validations shown:**
- Ranks must be unique (1, 2, 3)
- Points match configured prizes
- Users must be registered participants
- Total can't exceed remaining pool

> "Bulk award feature allows awarding all winners in one action. Points are immediately credited to accounts."

#### 3.5 Product Management
- Navigate to **Products**
- Show product list with stock levels
- Point out: **Extra WFH Day** has low stock (3 remaining) → Alert shown

- **Create New Product:**
  - Name: "AGDATA Backpack"
  - Category: AGDATA Merchandise
  - Points Cost: 350
  - Stock: 20

> "Admin can manage the complete product catalog including pricing and inventory."

#### 3.6 Redemption Management
- Navigate to **Redemptions**
- Show list with different statuses

**Demo the Full Workflow:**

1. **Pending → Approved:**
   - Find Sankalp's pending Amazon Gift Card request
   - Click **Approve**
   - Status changes to Approved

2. **Approved → Delivered:**
   - Find Amit's approved Wireless Mouse
   - Click **Mark as Delivered**
   - Status changes to Delivered

3. **Pending → Rejected:**
   - (If there's a pending one, or explain the rejected one)
   - Show Rahul's rejected WFH request with reason:
   > "Work from home day not available during the current sprint deadline period..."

> "Complete redemption lifecycle with approval workflow. Rejection requires explanation."

#### 3.7 Admin Budget Feature (New!)
- Navigate to **Budget** (Admin settings)
- Show current month's budget:
  - **Budget Limit:** 10,000 points
  - **Points Awarded:** 3,500 points
  - **Remaining:** 6,500 points
  - **Usage:** 35%
  - **Warning Threshold:** 80%

- Click **Budget History:**
  - Show last 3 months of budget usage
  - Previous month: 7,200 of 8,000 used (90%)

- **Edit Budget:**
  - Change limit to 12,000
  - Toggle **Hard Limit** on → "When enabled, blocks awarding points over budget"

> "Budget feature helps admins track and control monthly points distribution. Warning alerts when approaching limit."

#### 3.8 Reports (Quick Overview)
- Show **Points Report** - Distribution analytics
- Show **Redemption Report** - Product popularity
- Show **User Activity Report** - Engagement metrics

---

### Section 4: Technical Highlights (3 minutes)

#### Architecture
```
┌─────────────────┐
│   Angular 21    │ ← Frontend (Standalone Components, Signals)
├─────────────────┤
│   .NET 8 API    │ ← 65+ REST Endpoints
├─────────────────┤
│ Clean Architecture │
│  - API Layer       │
│  - Application (14 Services) │
│  - Domain (11 Entities) │
│  - Infrastructure (Repositories) │
├─────────────────┤
│   SQL Server    │
└─────────────────┘
```

#### Key Technical Points:
1. **JWT Authentication** - 30-min access tokens, 7-day refresh
2. **FluentValidation** - All requests validated server-side
3. **Standardized Responses** - `ApiResponse<T>` wrapper for all endpoints
4. **Swagger Documentation** - Available at `/swagger`
5. **Unit Tests** - 132 tests with 100% pass rate

---

## Quick Reference: Validation Rules

### Password Requirements
- 8-20 characters
- At least 1 uppercase letter
- At least 1 lowercase letter
- At least 1 number
- At least 1 special character

### Event Rules
- Status: Draft → Upcoming → Active → Completed (one-way only)
- Prizes: 1st > 2nd > 3rd (strictly descending)
- Points Pool = 1st + 2nd + 3rd (auto-calculated)
- Registration within date window only

### User Deactivation Rules
- Cannot deactivate user with pending redemptions
- Cannot deactivate last remaining admin (≥2 required)

### Redemption Rules
- Quantity: 1-10 per request
- Rejection reason: 10-500 characters required
- Points refunded on cancellation

### Budget Rules
- Budget limit: 1 to 10,000,000 points
- Warning threshold: 0-100%
- Hard limit blocks awarding when exceeded

---

## Demo Checklist

| # | Feature | Action | Validation/Result |
|---|---------|--------|-------------------|
| 1 | Login validation | Try invalid email | "Must end with @agdata.com" |
| 2 | Password validation | Try weak password | Shows requirements |
| 3 | Employee dashboard | Login as Sankalp | See points balance |
| 4 | Points history | View transactions | Credit/Debit entries |
| 5 | Event registration | Register for event | Date window check |
| 6 | Product redemption | Try expensive item | "Insufficient balance" |
| 7 | Redemption tracking | View My Redemptions | Multiple statuses |
| 8 | Admin dashboard | Login as Harshal | System metrics |
| 9 | User deactivation | Try deactivate Sankalp | Pending redemption error |
| 10 | Last admin protection | Try deactivate last | Protection error |
| 11 | Event status flow | Try reverse status | "One-way only" error |
| 12 | Event creation | Create with prizes | Auto-calculate pool |
| 13 | Award winners | Award 3 winners | Bulk points awarded |
| 14 | Approve redemption | Approve pending | Status updates |
| 15 | Reject with reason | Show rejected one | Reason displayed |
| 16 | Budget tracking | View budget page | Usage percentage |
| 17 | Low stock alert | Check products | WFH Day alert |

---

## Questions to Anticipate

**Q: How are passwords stored?**
> BCrypt hashing with salt. Never stored in plain text.

**Q: Can events be reopened after completion?**
> No, event status is one-way: Draft → Upcoming → Active → Completed

**Q: What happens to points if a user is deactivated?**
> Points history is preserved. User must have no pending redemptions first.

**Q: How does the budget feature work?**
> Each admin can set a monthly budget. Warning at threshold %. Optional hard limit blocks over-budget awards.

**Q: What's the tech stack?**
> Angular 21 frontend + .NET 8 backend + SQL Server. Clean Architecture with 4 layers.

**Q: How many API endpoints?**
> 65+ endpoints covering all features. Swagger documentation at /swagger.

---

## Closing Statement

> "The AGDATA Reward Points System provides a complete employee recognition solution with robust validation, role-based access, and comprehensive audit trails. The clean architecture ensures maintainability and scalability for future enhancements."

---

## Demo Files

| File | Purpose |
|------|---------|
| `Database/DemoData.sql` | Run to populate demo data |
| `presentation.md` | This presentation guide |
| `API_DOCUMENTATION.md` | Complete API reference (65+ endpoints) |
| `CLAUDE.md` | Development documentation |
