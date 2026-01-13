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
		path: 'dashboard',
		loadComponent: () => import('./dashboard/dashboard.component').then(m => m.DashboardComponent)
	},
	{ 
		path: '', 
		redirectTo: 'login', 
		pathMatch: 'full' 
	}
];
