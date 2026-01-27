import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { ApiResponse } from '../models/api-response.model';

// Points Interfaces
export interface PointsAccountDto {
  userId: string;
  userName?: string;
  userEmail?: string;
  currentBalance: number;
  totalEarned: number;
  totalRedeemed: number;
  pendingPoints?: number;  // Points reserved for pending redemptions
  lastTransactionDate?: string;
}

export interface PointsTransactionDto {
  id: string;
  userId: string;
  transactionType: string;  // 'Credit' | 'Debit'
  userPoints: number;       // Backend sends userPoints
  points?: number;          // Alias for compatibility
  description: string;
  eventName?: string;
  eventDescription?: string;  // Event's description from backend
  eventId?: string;
  eventRank?: number;
  redemptionId?: string;
  transactionSource?: string;  // 'Event', 'Redemption', 'AdminAward', 'Direct'
  balanceAfter?: number;
  timestamp: string;        // Backend sends timestamp
  createdAt?: string;       // Alias for compatibility
}

export interface LeaderboardEntryDto {
  rank: number;
  userId: string;
  userName: string;
  userEmail?: string;
  totalEarned: number;
  currentBalance: number;
}

export interface AwardPointsDto {
  userId: string;
  points: number;
  description?: string;
  eventId?: string;
}

export interface DeductPointsDto {
  userId: string;
  points: number;
  description: string;
}

export interface PointsSummaryDto {
  totalPointsInSystem: number;
  totalPointsAwarded: number;
  totalPointsRedeemed: number;
  averageBalancePerUser: number;
}

@Injectable({
  providedIn: 'root'
})
export class PointsService {
  constructor(private api: ApiService) {}

  // Get user points account
  getPointsAccount(userId: string): Observable<ApiResponse<PointsAccountDto>> {
    return this.api.get<PointsAccountDto>(`Points/accounts/${userId}`);
  }

  // Get user transactions
  getUserTransactions(userId: string, page?: number, pageSize?: number): Observable<ApiResponse<PointsTransactionDto[]>> {
    return this.api.get<PointsTransactionDto[]>(`Points/transactions/${userId}`, { page, pageSize });
  }

  // Get all transactions (admin)
  getAllTransactions(page?: number, pageSize?: number): Observable<ApiResponse<PointsTransactionDto[]>> {
    return this.api.get<PointsTransactionDto[]>('Points/transactions', { page, pageSize });
  }

  // Award points
  awardPoints(data: AwardPointsDto): Observable<ApiResponse<any>> {
    return this.api.post<any>('Points/award', data);
  }

  // Deduct points
  deductPoints(data: DeductPointsDto): Observable<ApiResponse<any>> {
    return this.api.post<any>('Points/deduct', data);
  }

  // Get leaderboard
  getLeaderboard(top?: number): Observable<ApiResponse<LeaderboardEntryDto[]>> {
    return this.api.get<LeaderboardEntryDto[]>('Points/leaderboard', { top });
  }

  // Get points summary (admin)
  getPointsSummary(): Observable<ApiResponse<PointsSummaryDto>> {
    return this.api.get<PointsSummaryDto>('Points/summary');
  }
}
