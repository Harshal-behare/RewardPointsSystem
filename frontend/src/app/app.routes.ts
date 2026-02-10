import { Routes } from '@angular/router';
import { authGuard } from './auth/auth.guard';
import { adminGuard, employeeGuard } from './auth/role.guard';

export const routes: Routes = [
	{
		path: 'login',
		loadComponent: () => import('./auth/login/login.component').then(m => m.LoginComponent)
	},
	{
		path: 'admin',
		loadChildren: () => import('./features/admin/admin.routes').then(m => m.adminRoutes),
		canActivate: [adminGuard]  // Only Admin role can access admin routes
	},
	{
		path: 'employee',
		loadChildren: () => import('./features/employee/employee.routes').then(m => m.employeeRoutes),
		canActivate: [employeeGuard]  // Employee, Admin can access employee routes
	},
	{
		path: 'dashboard',
		redirectTo: 'employee/dashboard',
		pathMatch: 'full'
	},
	{ 
		path: '', 
		redirectTo: 'login', 
		pathMatch: 'full' 
	}
];
