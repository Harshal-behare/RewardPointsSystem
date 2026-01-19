import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { ApiResponse, PaginatedResponse } from '../models/api-response.model';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private baseUrl = `${environment.apiUrl}/api/v1`;

  constructor(private http: HttpClient) {}

  private getHeaders(): HttpHeaders {
    const token = localStorage.getItem('rp_access_token');
    return new HttpHeaders({
      'Content-Type': 'application/json',
      ...(token ? { 'Authorization': `Bearer ${token}` } : {})
    });
  }

  // Normalize API response (handles both PascalCase and camelCase)
  private normalizeResponse<T>(raw: any): ApiResponse<T> {
    return {
      success: raw?.Success ?? raw?.success ?? false,
      data: raw?.Data ?? raw?.data ?? null,
      message: raw?.Message ?? raw?.message ?? null,
      error: raw?.Error ?? raw?.error ?? null
    };
  }

  get<T>(endpoint: string, params?: any): Observable<ApiResponse<T>> {
    let httpParams = new HttpParams();
    if (params) {
      Object.keys(params).forEach(key => {
        if (params[key] !== null && params[key] !== undefined) {
          httpParams = httpParams.set(key, params[key].toString());
        }
      });
    }
    
    return this.http.get<any>(`${this.baseUrl}/${endpoint}`, {
      headers: this.getHeaders(),
      params: httpParams
    }).pipe(map(raw => this.normalizeResponse<T>(raw)));
  }

  post<T>(endpoint: string, data: any): Observable<ApiResponse<T>> {
    return this.http.post<any>(`${this.baseUrl}/${endpoint}`, data, {
      headers: this.getHeaders()
    }).pipe(map(raw => this.normalizeResponse<T>(raw)));
  }

  put<T>(endpoint: string, data: any): Observable<ApiResponse<T>> {
    return this.http.put<any>(`${this.baseUrl}/${endpoint}`, data, {
      headers: this.getHeaders()
    }).pipe(map(raw => this.normalizeResponse<T>(raw)));
  }

  patch<T>(endpoint: string, data?: any): Observable<ApiResponse<T>> {
    return this.http.patch<any>(`${this.baseUrl}/${endpoint}`, data || {}, {
      headers: this.getHeaders()
    }).pipe(map(raw => this.normalizeResponse<T>(raw)));
  }

  delete<T>(endpoint: string): Observable<ApiResponse<T>> {
    return this.http.delete<any>(`${this.baseUrl}/${endpoint}`, {
      headers: this.getHeaders()
    }).pipe(map(raw => this.normalizeResponse<T>(raw)));
  }
}
