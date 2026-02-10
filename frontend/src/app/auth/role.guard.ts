import { inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService } from './auth.service';

/**
 * Decodes JWT token payload
 */
function decodeToken(token: string): any {
  try {
    const payload = token.split('.')[1];
    return JSON.parse(atob(payload));
  } catch {
    return null;
  }
}

/**
 * Gets user roles from JWT token or localStorage
 */
function getUserRoles(authService: AuthService): string[] {
  const token = authService.getToken();
  if (!token) return [];

  const payload = decodeToken(token);
  if (!payload) return [];

  // JWT can have role as string or array
  // Check both 'role' and standard claim type
  const roleClaim = payload.role || payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
  
  if (Array.isArray(roleClaim)) {
    return roleClaim;
  } else if (typeof roleClaim === 'string') {
    return [roleClaim];
  }
  
  return [];
}

/**
 * Factory function to create role-based guards
 * @param allowedRoles - Array of roles that can access the route
 * @param redirectTo - URL to redirect unauthorized users to
 */
export function createRoleGuard(allowedRoles: string[], redirectTo: string = '/employee/dashboard'): CanActivateFn {
  return (route, state) => {
    const platformId = inject(PLATFORM_ID);
    const authService = inject(AuthService);
    const router = inject(Router);

    // During SSR, allow navigation and let client-side hydration handle auth
    if (!isPlatformBrowser(platformId)) {
      return true;
    }

    // First check if authenticated
    if (!authService.isAuthenticated()) {
      router.navigate(['/login'], { queryParams: { returnUrl: state.url } });
      return false;
    }

    // Check if user has any of the allowed roles
    const userRoles = getUserRoles(authService);
    const hasRole = allowedRoles.some(role => 
      userRoles.some(userRole => 
        userRole.toLowerCase() === role.toLowerCase()
      )
    );

    if (hasRole) {
      return true;
    }

    // User is authenticated but doesn't have required role
    console.warn(`Access denied: User roles [${userRoles.join(', ')}] do not include required roles [${allowedRoles.join(', ')}]`);
    router.navigate([redirectTo]);
    return false;
  };
}

/**
 * Admin-only route guard
 * Allows access only to users with Admin or Administrator role
 */
export const adminGuard: CanActivateFn = createRoleGuard(['Admin', 'Administrator'], '/employee/dashboard');

/**
 * Employee route guard
 * Allows access to users with Employee, Admin, or Administrator role
 * (Admins can access employee pages)
 */
export const employeeGuard: CanActivateFn = createRoleGuard(['Employee', 'Admin', 'Administrator'], '/login');
