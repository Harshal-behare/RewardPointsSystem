import { Injectable } from '@angular/core';
import { Observable, map } from 'rxjs';
import { ApiService } from './api.service';
import { ApiResponse } from '../models/api-response.model';

// Dashboard Statistics Interface
export interface DashboardStats {
  totalUsers: number;
  totalActiveUsers: number;
  totalEvents: number;
  activeEvents: number;
  totalProducts: number;
  activeProducts: number;
  totalPointsDistributed: number;
  totalPointsRedeemed: number;
  pendingRedemptions: number;
  totalRedemptions: number;
}

// Points Report Interface
export interface PointsReport {
  totalPointsAwarded: number;
  totalPointsRedeemed: number;
  averagePointsPerUser: number;
  topEarners: Array<{
    userId: string;
    userName: string;
    totalEarned: number;
  }>;
}

// Recent Activity Interface
export interface RecentActivity {
  id: string;
  type: string;
  description: string;
  timestamp: string;
  user: string;
}

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  constructor(private api: ApiService) {}

  // Dashboard
  getDashboardStats(): Observable<ApiResponse<DashboardStats>> {
    return this.api.get<DashboardStats>('Admin/dashboard');
  }

  // Reports
  getPointsReport(startDate?: string, endDate?: string): Observable<ApiResponse<PointsReport>> {
    return this.api.get<PointsReport>('Admin/reports/points', { startDate, endDate });
  }

  getUsersReport(startDate?: string, endDate?: string): Observable<ApiResponse<any>> {
    return this.api.get<any>('Admin/reports/users', { startDate, endDate });
  }

  getRedemptionsReport(startDate?: string, endDate?: string): Observable<ApiResponse<any>> {
    return this.api.get<any>('Admin/reports/redemptions', { startDate, endDate });
  }

  getEventsReport(year?: number): Observable<ApiResponse<any>> {
    return this.api.get<any>('Admin/reports/events', { year });
  }

  // Alerts
  getInventoryAlerts(): Observable<ApiResponse<any[]>> {
    return this.api.get<any[]>('Admin/alerts/inventory');
  }

  getPointsAlerts(): Observable<ApiResponse<any[]>> {
    return this.api.get<any[]>('Admin/alerts/points');
  }

  // Admin count
  getAdminCount(): Observable<ApiResponse<{ count: number }>> {
    return this.api.get<{ count: number }>('Admin/admin-count');
  }
}
