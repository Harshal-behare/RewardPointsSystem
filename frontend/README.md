# Reward Points System - Frontend

Modern Angular 21 frontend for the Reward Points System, featuring separate Admin and Employee portals with responsive design.

## Technology Stack

- **Angular 21** - Latest Angular framework
- **Tailwind CSS** - Utility-first CSS framework
- **ApexCharts** - Interactive charts and visualizations
- **Angular Material** - UI component library
- **RxJS** - Reactive programming

## Features

### Admin Portal (`/admin`)
- **Dashboard** - KPIs, charts, recent activity, quick actions
- **Events** - Full event lifecycle management
- **Products** - Category and product management
- **Users** - User and role management
- **Redemptions** - Approve/reject redemption requests

### Employee Portal (`/employee`)
- **Dashboard** - Personal points summary, upcoming events
- **Events** - Browse and register for events
- **Products** - Browse catalog and redeem points
- **Account** - Transaction history and pending redemptions

## Getting Started

### Prerequisites
- Node.js 18+
- npm or yarn
- Angular CLI: `npm install -g @angular/cli`

### Installation

```bash
npm install
```

### Development Server

```bash
npm start
# or
ng serve
```

Navigate to `http://localhost:4200/`. The app reloads on source changes.

### Build

```bash
npm run build
# or
ng build
```

Build artifacts are stored in `dist/`.

### Running Tests

```bash
npm test
# or
ng test
```

## Project Structure

```
src/app/
├── auth/                  # Authentication (login, guards, JWT)
├── core/                  # Core services (API, toast, modals)
├── features/
│   ├── admin/             # Admin pages
│   │   ├── dashboard/
│   │   ├── events/
│   │   ├── products/
│   │   ├── users/
│   │   ├── redemptions/
│   │   └── profile/
│   └── employee/          # Employee pages
│       ├── dashboard/
│       ├── events/
│       ├── products/
│       ├── account/
│       └── profile/
├── layouts/               # Admin & Employee layouts
└── shared/                # Reusable components
```

## Environment

Configure API URL in `src/environments/`:

```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000/api/v1'
};
```
