import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const token = typeof localStorage !== 'undefined' ? localStorage.getItem('rp_access_token') : null;

  // Add auth header if token exists
  if (token) {
    req = req.clone({
      setHeaders: { Authorization: `Bearer ${token}` }
    });
  }

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      console.error(`[HTTP ${error.status}]`, error.url, error.message);

      switch (error.status) {
        case 401:
          // Token expired or invalid
          if (typeof localStorage !== 'undefined') {
            localStorage.removeItem('rp_access_token');
            localStorage.removeItem('rp_refresh_token');
            localStorage.removeItem('rp_user');
          }
          router.navigate(['/login'], {
            queryParams: { message: 'Session expired. Please login again.' }
          });
          break;

        case 403:
          console.error('Permission denied');
          break;

        case 0:
          console.error('Network error - API might be down');
          break;
      }

      return throwError(() => error);
    })
  );
};
