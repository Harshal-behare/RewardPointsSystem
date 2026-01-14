import { Routes } from '@angular/router';
import { EmployeeLayoutComponent } from '../../layouts/employee-layout/employee-layout.component';

export const employeeRoutes: Routes = [
  {
    path: '',
    component: EmployeeLayoutComponent,
    children: [
      {
        path: 'dashboard',
        loadComponent: () => import('./dashboard/dashboard.component').then(m => m.EmployeeDashboardComponent)
      },
      {
        path: 'events',
        loadComponent: () => import('./events/events.component').then(m => m.EmployeeEventsComponent)
      },
      {
        path: 'products',
        loadComponent: () => import('./products/products.component').then(m => m.EmployeeProductsComponent)
      },
      {
        path: 'account',
        loadComponent: () => import('./account/account.component').then(m => m.EmployeeAccountComponent)
      },
      {
        path: 'profile',
        loadComponent: () => import('./profile/profile.component').then(m => m.EmployeeProfileComponent)
      },
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full'
      }
    ]
  }
];
