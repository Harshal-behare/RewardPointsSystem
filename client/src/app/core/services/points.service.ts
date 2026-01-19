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
  lastTransactionDate?: string;
}

export interface PointsTransactionDto {
  id: string;
  userId: string;
  transactionType: 'Credit' | 'Debit';
  points: number;
  description: string;
  eventName?: string;
  createdAt: string;
  balanceAfter: number;
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
  reason?: string;
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
