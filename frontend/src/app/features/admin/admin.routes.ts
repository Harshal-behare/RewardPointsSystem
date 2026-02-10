import { Routes } from '@angular/router';
import { AdminLayoutComponent } from '../../layouts/admin-layout/admin-layout.component';

export const adminRoutes: Routes = [
  {
    path: '',
    component: AdminLayoutComponent,
    children: [
      {
        path: 'dashboard',
        loadComponent: () => import('./dashboard/dashboard.component')
          .then(m => m.AdminDashboardComponent)
      },
      {
        path: 'events',
        loadComponent: () => import('./events/events.component')
          .then(m => m.AdminEventsComponent)
      },
      {
        path: 'products',
        loadComponent: () => import('./products/products.component')
          .then(m => m.AdminProductsComponent)
      },
      {
        path: 'users',
        loadComponent: () => import('./users/users.component')
          .then(m => m.AdminUsersComponent)
      },
      {
        path: 'profile',
        loadComponent: () => import('./profile/profile.component')
          .then(m => m.AdminProfileComponent)
      },
      {
        path: 'redemptions',
        loadComponent: () => import('./redemptions/redemptions.component')
          .then(m => m.AdminRedemptionsComponent)
      },
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full'
      }
    ]
  }
];
