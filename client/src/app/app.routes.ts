import { Routes } from '@angular/router';
import { authGuard } from './auth/auth.guard';

export const routes: Routes = [
	{
		path: 'login',
		loadComponent: () => import('./auth/login/login.component').then(m => m.LoginComponent)
	},
	{
		path: 'admin',
		loadChildren: () => import('./features/admin/admin.routes').then(m => m.adminRoutes),
		canActivate: [authGuard]
	},
	{
		path: 'employee',
		loadChildren: () => import('./features/employee/employee.routes').then(m => m.employeeRoutes),
		canActivate: [authGuard]
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
