# Frontend Folder Structure Documentation

## 📁 Complete Folder Structure

```
client/src/app/
│
├── auth/                          # ✅ EXISTING - Authentication Module (Keep as is)
│   ├── login/
│   │   ├── login.component.ts
│   │   ├── login.component.html
│   │   └── login.component.css
│   └── auth.service.ts
│
├── core/                          # 🆕 Core Module - Singleton Services & App-Wide Logic
│   ├── services/
│   │   ├── api.service.ts         # Central HTTP service
│   │   ├── storage.service.ts     # LocalStorage/SessionStorage wrapper
│   │   └── notification.service.ts # Toast/Alert notifications
│   ├── guards/
│   │   ├── auth.guard.ts          # Protect authenticated routes
│   │   ├── role.guard.ts          # Role-based access (Admin/Employee)
│   │   └── guest.guard.ts         # Prevent logged-in users from accessing login
│   ├── interceptors/
│   │   ├── auth.interceptor.ts    # Add JWT token to requests
│   │   ├── error.interceptor.ts   # Global error handling
│   │   └── loading.interceptor.ts # Show/hide loading spinner
│   ├── models/
│   │   ├── user.model.ts          # User interface/class
│   │   ├── event.model.ts         # Event interface
│   │   ├── product.model.ts       # Product interface
│   │   ├── points.model.ts        # Points transaction interface
│   │   ├── redemption.model.ts    # Redemption interface
│   │   └── api-response.model.ts  # Standard API response wrapper
│   └── constants/
│       ├── api-endpoints.ts       # All API endpoint URLs
│       ├── app-constants.ts       # App-wide constants
│       └── theme-colors.ts        # Design system colors (#27AE60, etc.)
│
├── shared/                        # 🆕 Shared Module - Reusable Components & Utilities
│   ├── components/
│   │   ├── button/                # Reusable green button component
│   │   ├── card/                  # Card wrapper component
│   │   ├── modal/                 # Modal/Dialog component
│   │   ├── table/                 # Data table component
│   │   ├── badge/                 # Status badge component
│   │   ├── search-bar/            # Search input component
│   │   ├── filter-dropdown/       # Filter dropdown component
│   │   ├── loading-spinner/       # Loading indicator
│   │   ├── confirmation-dialog/   # Confirm action dialog
│   │   └── form-field/            # Consistent form field wrapper
│   ├── directives/
│   │   ├── tooltip.directive.ts   # Tooltip on hover
│   │   └── click-outside.directive.ts
│   ├── pipes/
│   │   ├── date-format.pipe.ts    # Format dates consistently
│   │   ├── points-format.pipe.ts  # Format point numbers
│   │   └── status-color.pipe.ts   # Map status to color
│   └── utils/
│       ├── validators.ts          # Custom form validators
│       └── helpers.ts             # Helper functions
│
├── layouts/                       # 🆕 Layout Components - Page Wrappers
│   ├── admin-layout/
│   │   ├── admin-layout.component.ts
│   │   ├── admin-layout.component.html
│   │   └── admin-layout.component.scss  # Left sidebar + top nav
│   ├── employee-layout/
│   │   ├── employee-layout.component.ts
│   │   ├── employee-layout.component.html
│   │   └── employee-layout.component.scss
│   ├── auth-layout/
│   │   ├── auth-layout.component.ts
│   │   ├── auth-layout.component.html
│   │   └── auth-layout.component.scss   # Centered card layout
│   └── components/
│       ├── sidebar/               # Reusable sidebar component
│       ├── topbar/                # Reusable top navigation
│       └── footer/                # Optional footer
│
├── features/                      # 🆕 Feature Modules - Organized by User Role
│   │
│   ├── admin/                     # Admin Feature Module
│   │   ├── dashboard/
│   │   │   ├── dashboard.component.ts
│   │   │   ├── dashboard.component.html
│   │   │   ├── dashboard.component.scss
│   │   │   └── components/
│   │   │       ├── kpi-card/      # KPI display cards
│   │   │       ├── points-chart/  # Chart for points trends
│   │   │       └── recent-activity/ # Activity feed
│   │   │
│   │   ├── events/
│   │   │   ├── events.component.ts
│   │   │   ├── events.component.html
│   │   │   ├── events.component.scss
│   │   │   └── components/
│   │   │       ├── event-table/   # Events data table
│   │   │       ├── event-form/    # Create/Edit event form
│   │   │       └── participants-modal/ # Manage participants
│   │   │
│   │   ├── products/
│   │   │   ├── products.component.ts
│   │   │   ├── products.component.html
│   │   │   ├── products.component.scss
│   │   │   └── components/
│   │   │       ├── product-grid/  # Product grid/table view
│   │   │       ├── product-form/  # Add/Edit product form
│   │   │       └── stock-modal/   # Manage stock levels
│   │   │
│   │   ├── users/
│   │   │   ├── users.component.ts
│   │   │   ├── users.component.html
│   │   │   ├── users.component.scss
│   │   │   └── components/
│   │   │       ├── user-table/    # Users data table
│   │   │       └── user-form/     # Add/Edit user form
│   │   │
│   │   ├── profile/
│   │   │   ├── profile.component.ts
│   │   │   ├── profile.component.html
│   │   │   └── profile.component.scss
│   │   │
│   │   ├── components/            # Admin-specific shared components
│   │   │   └── admin-stats-card/
│   │   │
│   │   └── services/
│   │       ├── admin-dashboard.service.ts
│   │       ├── admin-events.service.ts
│   │       ├── admin-products.service.ts
│   │       └── admin-users.service.ts
│   │
│   └── employee/                  # Employee Feature Module
│       ├── dashboard/
│       │   ├── dashboard.component.ts
│       │   ├── dashboard.component.html
│       │   ├── dashboard.component.scss
│       │   └── components/
│       │       ├── points-balance-card/
│       │       ├── recommended-products/
│       │       └── upcoming-events/
│       │
│       ├── events/
│       │   ├── events.component.ts
│       │   ├── events.component.html
│       │   ├── events.component.scss
│       │   └── components/
│       │       ├── event-card/     # Individual event card
│       │       ├── event-filters/  # Search & filter bar
│       │       └── event-details-modal/
│       │
│       ├── products/
│       │   ├── products.component.ts
│       │   ├── products.component.html
│       │   ├── products.component.scss
│       │   └── components/
│       │       ├── product-card/   # Individual product card
│       │       ├── product-filters/
│       │       └── redeem-modal/   # Confirmation modal
│       │
│       ├── account/
│       │   ├── account.component.ts
│       │   ├── account.component.html
│       │   ├── account.component.scss
│       │   └── components/
│       │       ├── points-history-table/
│       │       └── redemption-history-table/
│       │
│       ├── profile/
│       │   ├── profile.component.ts
│       │   ├── profile.component.html
│       │   └── profile.component.scss
│       │
│       ├── components/            # Employee-specific shared components
│       │   └── employee-card/
│       │
│       └── services/
│           ├── employee-dashboard.service.ts
│           ├── employee-events.service.ts
│           ├── employee-products.service.ts
│           └── employee-account.service.ts
│
├── dashboard/                     # ✅ EXISTING - Can be removed or repurposed
│   └── dashboard.component.ts     # Consider moving to features/
│
├── app.component.ts               # Root component
├── app.routes.ts                  # Main routing configuration
└── app.module.ts                  # Root module (if using modules)
```

---

## 📚 Detailed Explanation

### 1. **auth/** (✅ EXISTING - Keep as is)
- **Purpose**: Handles authentication functionality
- **Current Status**: Working login page - DO NOT MODIFY
- **Contains**: Login component and auth service
- **Why**: Authentication is a distinct feature that should remain isolated

---

### 2. **core/** (🆕 NEW)
**Purpose**: Application-wide singleton services, guards, models, and constants

#### **Why this structure?**
- **Single Responsibility**: Core services are instantiated once and shared across the entire app
- **Security**: Guards protect routes based on authentication and roles
- **Consistency**: Interceptors handle cross-cutting concerns (auth tokens, errors, loading)
- **Type Safety**: Models provide TypeScript interfaces for all data structures
- **Maintainability**: Constants centralize configuration (API URLs, colors, etc.)

#### **What goes here:**
- ✅ Services used throughout the app (API, storage, notifications)
- ✅ Route guards (auth.guard.ts, role.guard.ts)
- ✅ HTTP interceptors (add JWT, handle errors)
- ✅ TypeScript interfaces/classes for data models
- ✅ Constants that don't change (API endpoints, theme colors)

#### **What doesn't go here:**
- ❌ Feature-specific services (those go in features/)
- ❌ UI components (those go in shared/ or features/)

---

### 3. **shared/** (🆕 NEW)
**Purpose**: Reusable UI components, directives, pipes, and utilities used across multiple features

#### **Why this structure?**
- **DRY Principle**: Write once, use everywhere
- **Consistency**: Ensures UI components look and behave the same throughout the app
- **Efficiency**: Changes to shared components automatically propagate everywhere
- **Design System**: Implements your green theme (#27AE60) consistently

#### **What goes here:**
- ✅ Reusable UI components (buttons, cards, modals, tables)
- ✅ Custom directives (tooltips, click-outside detection)
- ✅ Custom pipes (date formatting, number formatting)
- ✅ Utility functions (validators, helpers)

#### **Examples:**
```typescript
// shared/components/button/button.component.ts
// A green button used everywhere in the app

// shared/pipes/date-format.pipe.ts
// Formats dates consistently: "Jan 15, 2026"

// shared/components/modal/modal.component.ts
// Reusable modal for confirmations, forms, etc.
```

---

### 4. **layouts/** (🆕 NEW)
**Purpose**: Define page structure and navigation for different user roles

#### **Why this structure?**
- **Separation of Concerns**: Layout logic separate from page content
- **Role-Based UI**: Different layouts for Admin (data-heavy) vs Employee (card-based)
- **Navigation**: Sidebar and topbar are consistent across pages within a role
- **Code Reuse**: One layout wraps all pages for that role

#### **What goes here:**
- ✅ **admin-layout**: Left sidebar + top nav for admin pages
- ✅ **employee-layout**: Left sidebar + top nav for employee pages
- ✅ **auth-layout**: Centered card layout for login page
- ✅ **components**: Reusable navbar, sidebar, footer components

#### **How it works:**
```typescript
// app.routes.ts
{
  path: 'admin',
  component: AdminLayoutComponent,  // Wrapper with sidebar
  children: [
    { path: 'dashboard', component: AdminDashboardComponent },
    { path: 'events', component: AdminEventsComponent },
    // ... admin layout wraps all these pages
  ]
}
```

---

### 5. **features/** (🆕 NEW)
**Purpose**: Feature modules organized by user role (Admin vs Employee)

#### **Why this structure?**
- **Scalability**: Each feature is self-contained and easy to find
- **Role Separation**: Admin and Employee features are clearly separated
- **Lazy Loading**: Features can be loaded on-demand for better performance
- **Team Collaboration**: Multiple developers can work on different features without conflicts
- **Component Isolation**: Page-specific components stay with their pages

#### **Structure Pattern:**
Each feature page follows this pattern:
```
feature-name/
  ├── feature-name.component.ts    # Main page component
  ├── feature-name.component.html  # Page template
  ├── feature-name.component.scss  # Page styles
  ├── components/                  # Page-specific components
  └── services/                    # Feature-specific services (optional)
```

#### **Admin Features:**
1. **dashboard/**: KPI cards, charts, recent activity
2. **events/**: Event table, create/edit forms, participant management
3. **products/**: Product grid/table, stock management
4. **users/**: User table, add/edit users, role assignment
5. **profile/**: Admin profile settings

#### **Employee Features:**
1. **dashboard/**: Points balance, recommended products, upcoming events
2. **events/**: Card grid of events with search/filter
3. **products/**: Card grid of products with redeem functionality
4. **account/**: Points history table, redemption history table
5. **profile/**: Employee profile settings

---

## 🎯 Key Design Decisions

### 1. **Feature-Based Over Type-Based**
❌ **Bad** (Type-based):
```
components/
  ├── admin-dashboard.component.ts
  ├── admin-events.component.ts
  ├── employee-dashboard.component.ts
  └── employee-events.component.ts
services/
  ├── admin.service.ts
  └── employee.service.ts
```

✅ **Good** (Feature-based):
```
features/
  ├── admin/
  │   ├── dashboard/
  │   └── events/
  └── employee/
      ├── dashboard/
      └── events/
```

**Why?** Features are easier to locate, scale, and lazy-load.

---

### 2. **Nested Components**
Each feature page has its own `components/` folder for page-specific components.

**Example:**
```
features/admin/events/
  ├── events.component.ts          # Main page
  └── components/
      ├── event-table/             # Used only in events page
      ├── event-form/              # Used only in events page
      └── participants-modal/      # Used only in events page
```

**Why?** Page-specific components are co-located with the page, not mixed with shared components.

---

### 3. **Lazy Loading Support**
This structure supports Angular's lazy loading:

```typescript
// app.routes.ts
{
  path: 'admin',
  loadChildren: () => import('./features/admin/admin.routes')
    .then(m => m.ADMIN_ROUTES)
},
{
  path: 'employee',
  loadChildren: () => import('./features/employee/employee.routes')
    .then(m => m.EMPLOYEE_ROUTES)
}
```

**Benefits:**
- Faster initial load time
- Admin code isn't loaded for employees (and vice versa)
- Better performance

---

## 🚀 How to Use This Structure

### For Admin Dashboard:
1. Route: `/admin/dashboard`
2. Layout: `AdminLayoutComponent` (sidebar + topbar)
3. Page: `features/admin/dashboard/dashboard.component.ts`
4. Components: KPI cards, charts (in `dashboard/components/`)
5. Services: `admin-dashboard.service.ts`

### For Employee Events:
1. Route: `/employee/events`
2. Layout: `EmployeeLayoutComponent` (sidebar + topbar)
3. Page: `features/employee/events/events.component.ts`
4. Components: Event cards, filters (in `events/components/`)
5. Services: `employee-events.service.ts`

---

## 📋 Next Steps

1. ✅ **Folder structure created** (Done)
2. 🔄 **Create routing configuration** for admin/employee
3. 🔄 **Build layout components** (admin-layout, employee-layout)
4. 🔄 **Create shared components** (button, card, modal, table)
5. 🔄 **Build core services** (API service, auth guard, interceptors)
6. 🔄 **Implement feature pages** one by one
7. 🔄 **Add styling** (SCSS with theme colors)

---

## 🎨 Design System Integration

This structure supports your design requirements:

- **Colors**: Defined in `core/constants/theme-colors.ts`
- **Components**: Shared button/card components use green theme
- **Spacing**: Utility classes in global SCSS
- **Typography**: Inter/Poppins fonts applied globally

---

## ⚠️ Important Notes

1. **Login page is safe**: The `auth/` folder remains untouched
2. **Old dashboard**: The existing `dashboard/` folder can be removed or repurposed
3. **Standalone components**: This structure works with Angular 17+ standalone components
4. **Modular approach**: Each feature can be developed and tested independently

---

## 🔒 Access Control

```typescript
// Routes with role-based guards
{
  path: 'admin',
  canActivate: [AuthGuard, RoleGuard],
  data: { role: 'Admin' },
  loadChildren: () => import('./features/admin/admin.routes')
},
{
  path: 'employee',
  canActivate: [AuthGuard, RoleGuard],
  data: { role: 'Employee' },
  loadChildren: () => import('./features/employee/employee.routes')
}
```

This structure ensures a **scalable, maintainable, and well-organized** Angular application! 🎉
