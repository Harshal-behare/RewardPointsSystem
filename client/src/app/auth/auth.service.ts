import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { tap, map, catchError } from 'rxjs/operators';
import { Observable, of, throwError } from 'rxjs';
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

  constructor(private http: HttpClient) {}
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
          if (res && res.data && res.data.accessToken) {
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
    localStorage.removeItem(this.accessKey);
    localStorage.removeItem(this.refreshKey);
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
    return localStorage.getItem(this.accessKey);
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }
}
