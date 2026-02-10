import { Injectable, Inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { tap, map, catchError } from 'rxjs/operators';
import { Observable, of, throwError, BehaviorSubject } from 'rxjs';
import { environment } from '../../environments/environment';

interface LoginRequest {
  email: string;
  password: string;
}

interface LoginResponse {
  success: boolean;
  message?: string;
  data?: {
    accessToken: string;
    refreshToken?: string;
    expiresAt?: string;
    user?: {
      id: string;
      email: string;
      firstName?: string;
      lastName?: string;
      role: string;
      roles?: string[];
    };
    role?: string;
  };
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly accessKey = 'rp_access_token';
  private readonly refreshKey = 'rp_refresh_token';
  private isBrowser: boolean;
  
  // Subject to broadcast user name changes to all subscribed components
  private userNameUpdated = new BehaviorSubject<{firstName: string, lastName: string} | null>(null);
  userNameUpdated$ = this.userNameUpdated.asObservable();

  constructor(
    private http: HttpClient,
    @Inject(PLATFORM_ID) platformId: Object
  ) {
    this.isBrowser = isPlatformBrowser(platformId);
  }
  login(payload: LoginRequest): Observable<LoginResponse> {
    return this.http.post<any>(`${environment.apiUrl}/api/v1/Auth/login`, payload)
      .pipe(
        map((raw: any) => {
          const d = raw?.Data ?? raw?.data ?? null;
          
          const normalized: LoginResponse = {
            success: raw?.Success ?? raw?.success ?? false,
            message: raw?.Message ?? raw?.message,
            data: (() => {
              if (!d) return undefined;
              
              // Extract roles array
              const rolesArray = d.Roles ?? d.roles ?? [];
              const primaryRole = Array.isArray(rolesArray) && rolesArray.length > 0 ? rolesArray[0] : null;
              
              // Build user object from data fields
              const userObj = {
                id: d.UserId ?? d.userId ?? null,
                email: d.Email ?? d.email ?? null,
                firstName: d.FirstName ?? d.firstName ?? null,
                lastName: d.LastName ?? d.lastName ?? null,
                role: primaryRole,
                roles: rolesArray
              };
              
              return {
                accessToken: d.AccessToken ?? d.accessToken ?? d.access_token ?? null,
                refreshToken: d.RefreshToken ?? d.refreshToken ?? d.refresh_token ?? null,
                expiresAt: d.ExpiresAt ?? d.expiresAt ?? d.expires_at ?? null,
                user: userObj,
                role: primaryRole
              } as any;
            })()
          };
          return normalized;
        }),
        tap(res => {
          if (this.isBrowser && res && res.data && res.data.accessToken) {
            localStorage.setItem(this.accessKey, res.data.accessToken);
            if (res.data.refreshToken) {
              localStorage.setItem(this.refreshKey, res.data.refreshToken);
            }
          }
        }),
        catchError(err => throwError(() => err))
      );
  }

  private clearLocal(): void {
    if (this.isBrowser) {
      localStorage.removeItem(this.accessKey);
      localStorage.removeItem(this.refreshKey);
    }
  }

  /**
   * Call API to revoke tokens and clear local storage. Returns an observable
   * that completes when the API call finishes. If no token exists, returns
   * an observable of null and still clears local storage.
   */
  logout(): Observable<any> {
    const token = this.getToken();
    this.clearLocal();
    if (!token) return of(null);

    const headers = new HttpHeaders({ Authorization: `Bearer ${token}` });
    return this.http.post(`${environment.apiUrl}/api/v1/Auth/logout`, {}, { headers })
      .pipe(
        tap(() => {
          // already cleared local storage above
        }),
        catchError(err => {
          // ensure local storage cleared even on error
          this.clearLocal();
          return throwError(() => err);
        })
      );
  }

  getToken(): string | null {
    if (!this.isBrowser) return null;
    return localStorage.getItem(this.accessKey);
  }

  isAuthenticated(): boolean {
    if (!this.isBrowser) return false;
    const token = this.getToken();
    if (!token) return false;
    
    // Check if token is expired
    const payload = this.getDecodedToken();
    if (!payload || !payload.exp) return false;
    
    // exp is in seconds, Date.now() is in milliseconds
    const expirationTime = payload.exp * 1000;
    const currentTime = Date.now();
    
    // Token is valid if not expired
    if (currentTime >= expirationTime) {
      // Token expired, clear it
      this.clearLocal();
      return false;
    }
    
    return true;
  }

  /**
   * Decodes and returns the JWT payload
   */
  getDecodedToken(): any {
    const token = this.getToken();
    if (!token) return null;
    
    try {
      const payload = token.split('.')[1];
      return JSON.parse(atob(payload));
    } catch {
      return null;
    }
  }

  /**
   * Gets user roles from the JWT token
   */
  getUserRoles(): string[] {
    const payload = this.getDecodedToken();
    if (!payload) return [];

    // Check both standard claim and short form
    const roleClaim = payload.role || payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
    
    if (Array.isArray(roleClaim)) {
      return roleClaim;
    } else if (typeof roleClaim === 'string') {
      return [roleClaim];
    }
    
    return [];
  }

  /**
   * Checks if current user has Admin role
   */
  isAdmin(): boolean {
    const roles = this.getUserRoles();
    return roles.some(r => r.toLowerCase() === 'admin' || r.toLowerCase() === 'administrator');
  }

  /**
   * Checks if current user has a specific role
   */
  hasRole(role: string): boolean {
    const roles = this.getUserRoles();
    return roles.some(r => r.toLowerCase() === role.toLowerCase());
  }

  /**
   * Updates user name in local storage and broadcasts change to subscribed components
   */
  updateUserName(firstName: string, lastName: string): void {
    if (this.isBrowser) {
      // Update localStorage user data
      const userDataStr = localStorage.getItem('user');
      if (userDataStr) {
        try {
          const userData = JSON.parse(userDataStr);
          userData.firstName = firstName;
          userData.lastName = lastName;
          userData.FirstName = firstName;
          userData.LastName = lastName;
          localStorage.setItem('user', JSON.stringify(userData));
        } catch (e) {
          console.error('Error updating user data in localStorage:', e);
        }
      }
      // Broadcast the name change to all subscribed components
      this.userNameUpdated.next({ firstName, lastName });
    }
  }
}
