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

// Event Status Summary
export interface EventStatusSummary {
  draft: number;
  upcoming: number;
  active: number;
  completed: number;
}

// Redemption Summary
export interface RedemptionSummary {
  pending: number;
  approved: number;
  rejected: number;
  delivered: number;
  cancelled: number;
}

// Inventory Alert
export interface InventoryAlert {
  productId: string;
  productName: string;
  currentStock: number;
  reorderLevel: number;
  alertType: 'Low Stock' | 'Out of Stock';
}

// Points System Summary
export interface PointsSummary {
  totalPointsDistributed: number;
  totalPointsRedeemed: number;
  totalPointsInCirculation: number;
  pendingPoints: number;
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

// Admin Budget Response Interface
export interface AdminBudgetResponse {
  id: string;
  monthYear: number;
  monthYearDisplay: string;
  budgetLimit: number;
  pointsAwarded: number;
  remainingBudget: number;
  usagePercentage: number;
  isHardLimit: boolean;
  warningThreshold: number;
  isOverBudget: boolean;
  isWarningZone: boolean;
  createdAt: string;
  updatedAt: string;
}

// Set Budget Request Interface
export interface SetBudgetRequest {
  budgetLimit: number;
  isHardLimit: boolean;
  warningThreshold: number;
}

// Budget History Item Interface
export interface BudgetHistoryItem {
  monthYear: number;
  monthYearDisplay: string;
  budgetLimit: number;
  pointsAwarded: number;
  remainingBudget: number;
  usagePercentage: number;
  wasOverBudget: boolean;
}

// Budget Validation Result Interface
export interface BudgetValidationResult {
  isAllowed: boolean;
  isWarning: boolean;
  message: string | null;
  remainingBudget: number;
  pointsToAward: number;
  pointsAfterAward: number;
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

  // Get Points Summary
  getPointsSummary(): Observable<ApiResponse<PointsSummary>> {
    return this.api.get<PointsSummary>('Points/summary');
  }

  // Budget Management
  /**
   * Get current admin's monthly budget status
   */
  getBudget(): Observable<ApiResponse<AdminBudgetResponse | null>> {
    return this.api.get<AdminBudgetResponse | null>('Admin/budget');
  }

  /**
   * Set or update monthly budget limit
   */
  setBudget(request: SetBudgetRequest): Observable<ApiResponse<AdminBudgetResponse>> {
    return this.api.put<AdminBudgetResponse>('Admin/budget', request);
  }

  /**
   * Get budget usage history for last N months
   */
  getBudgetHistory(months: number = 12): Observable<ApiResponse<BudgetHistoryItem[]>> {
    return this.api.get<BudgetHistoryItem[]>('Admin/budget/history', { months });
  }
}
