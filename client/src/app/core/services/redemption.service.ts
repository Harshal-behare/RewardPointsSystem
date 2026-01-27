import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { ApiResponse } from '../models/api-response.model';

// Redemption Interfaces
export interface RedemptionDto {
  id: string;
  userId: string;
  userName?: string;
  userEmail?: string;
  productId: string;
  productName?: string;
  productCategory?: string;
  pointsSpent: number;
  quantity?: number;
  status: 'Pending' | 'Approved' | 'Rejected' | 'Cancelled';
  requestedAt: string;
  approvedAt?: string;
  approvedBy?: string;
  approvedByName?: string;
  processedAt?: string;
  rejectionReason?: string;
}

export interface CreateRedemptionDto {
  userId: string;
  productId: string;
  quantity?: number;
}

export interface ApproveRedemptionDto {
  notes?: string;
}

export interface CancelRedemptionDto {
  cancellationReason: string;
}

export interface RejectRedemptionDto {
  rejectionReason: string;
}

@Injectable({
  providedIn: 'root'
})
export class RedemptionService {
  constructor(private api: ApiService) {}

  // Get all redemptions (admin sees all, user sees own)
  getRedemptions(): Observable<ApiResponse<RedemptionDto[]>> {
    return this.api.get<RedemptionDto[]>('Redemptions');
  }

  // Get redemption by ID
  getRedemptionById(id: string): Observable<ApiResponse<RedemptionDto>> {
    return this.api.get<RedemptionDto>(`Redemptions/${id}`);
  }

  // Get my redemptions (current user)
  getMyRedemptions(): Observable<ApiResponse<RedemptionDto[]>> {
    return this.api.get<RedemptionDto[]>('Redemptions/my-redemptions');
  }

  // Get pending redemptions (admin)
  getPendingRedemptions(): Observable<ApiResponse<RedemptionDto[]>> {
    return this.api.get<RedemptionDto[]>('Redemptions/pending');
  }

  // Create redemption request
  createRedemption(data: CreateRedemptionDto): Observable<ApiResponse<RedemptionDto>> {
    return this.api.post<RedemptionDto>('Redemptions', data);
  }

  // Approve redemption
  approveRedemption(id: string, data?: ApproveRedemptionDto): Observable<ApiResponse<RedemptionDto>> {
    return this.api.patch<RedemptionDto>(`Redemptions/${id}/approve`, data || {});
  }

  // Reject redemption
  rejectRedemption(id: string, reason: string): Observable<ApiResponse<RedemptionDto>> {
    return this.api.patch<RedemptionDto>(`Redemptions/${id}/reject`, { rejectionReason: reason });
  }

  // Cancel redemption
  cancelRedemption(id: string, data: CancelRedemptionDto): Observable<ApiResponse<RedemptionDto>> {
    return this.api.patch<RedemptionDto>(`Redemptions/${id}/cancel`, data);
  }
}
