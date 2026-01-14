import { Routes } from '@angular/router';

export const routes: Routes = [
	{
		path: 'login',
		loadComponent: () => import('./auth/login/login.component').then(m => m.LoginComponent)
	},
	{
		path: 'admin',
		loadChildren: () => import('./features/admin/admin.routes').then(m => m.adminRoutes)
	},
	{
		path: 'employee',
		loadChildren: () => import('./features/employee/employee.routes').then(m => m.employeeRoutes)
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
