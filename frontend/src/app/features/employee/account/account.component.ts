import { Component, OnInit, signal, computed } from '@angular/core';
import { NgClass, TitleCasePipe } from '@angular/common';
import { PointsService, PointsAccountDto, PointsTransactionDto } from '../../../core/services/points.service';
import { RedemptionService, RedemptionDto } from '../../../core/services/redemption.service';
import { AuthService } from '../../../auth/auth.service';
import { ToastService } from '../../../core/services/toast.service';
import { ConfirmDialogService } from '../../../core/services/confirm-dialog.service';
import { IconComponent } from '../../../shared/components/icon/icon.component';

interface PointTransaction {
  id: string;
  date: string;
  description: string;
  eventDescription?: string;  // Event's description for display
  points: number;
  type: 'earned' | 'redeemed' | 'expired';
  status: string;
  source?: string;
  eventName?: string;
  rank?: number;
}

interface RedemptionRecord {
  id: string;
  date: string;
  productName: string;
  points: number;
  quantity: number;
  status: 'pending' | 'approved' | 'rejected' | 'cancelled' | 'delivered';
  trackingNumber?: string;
  rejectionReason?: string;
  isRefunded?: boolean;  // Indicates if points were refunded (for cancelled/rejected)
}

@Component({
  selector: 'app-employee-account',
  standalone: true,
  imports: [NgClass, TitleCasePipe, IconComponent],
  templateUrl: './account.component.html',
  styleUrl: './account.component.scss'
})
export class EmployeeAccountComponent implements OnInit {
  activeTab: 'points' | 'redemptions' = 'points';
  
  pointsHistory = signal<PointTransaction[]>([]);
  redemptionHistory = signal<RedemptionRecord[]>([]);

  // Summary stats
  totalEarned = signal<number>(0);
  totalRedeemed = signal<number>(0);
  currentBalance = signal<number>(0);
  pendingPoints = signal<number>(0);

  // Loading states
  isLoadingPoints = signal<boolean>(true);
  isLoadingRedemptions = signal<boolean>(true);
  isLoadingAccount = signal<boolean>(true);

  // Pagination for Earning History
  earningsCurrentPage = signal(1);
  earningsPageSize = signal(10);
  
  paginatedPointsHistory = computed(() => {
    const history = this.pointsHistory();
    const start = (this.earningsCurrentPage() - 1) * this.earningsPageSize();
    const end = start + this.earningsPageSize();
    return history.slice(start, end);
  });
  
  earningsTotalPages = computed(() => Math.ceil(this.pointsHistory().length / this.earningsPageSize()));

  // Pagination for Redemption History
  redemptionsCurrentPage = signal(1);
  redemptionsPageSize = signal(10);
  
  paginatedRedemptionHistory = computed(() => {
    const history = this.redemptionHistory();
    const start = (this.redemptionsCurrentPage() - 1) * this.redemptionsPageSize();
    const end = start + this.redemptionsPageSize();
    return history.slice(start, end);
  });
  
  redemptionsTotalPages = computed(() => Math.ceil(this.redemptionHistory().length / this.redemptionsPageSize()));

  // Current user
  currentUserId: string = '';

  constructor(
    private pointsService: PointsService,
    private redemptionService: RedemptionService,
    private authService: AuthService,
    private toast: ToastService,
    private confirmDialog: ConfirmDialogService
  ) {
    // Extract user ID from JWT token
    const token = this.authService.getToken();
    if (token) {
      try {
        const payload = JSON.parse(atob(token.split('.')[1]));
        this.currentUserId = payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] || 
                            payload.sub || 
                            payload.userId || '';
      } catch (e) {
        console.error('Error parsing token:', e);
      }
    }
  }

  ngOnInit(): void {
    if (!this.currentUserId) {
      this.toast.error('User not authenticated');
      return;
    }
    this.loadAccountSummary();
    this.loadPointsHistory();
    this.loadRedemptionHistory();
  }

  loadAccountSummary(): void {
    this.isLoadingAccount.set(true);
    this.pointsService.getPointsAccount(this.currentUserId).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.totalEarned.set(response.data.totalEarned);
          this.totalRedeemed.set(response.data.totalRedeemed);
          this.currentBalance.set(response.data.currentBalance);
          // Use pendingPoints from API (stored in DB)
          this.pendingPoints.set(response.data.pendingPoints || 0);
        }
        this.isLoadingAccount.set(false);
      },
      error: (error) => {
        console.error('Error loading account summary:', error);
        this.toast.error('Failed to load account summary');
        this.isLoadingAccount.set(false);
      }
    });
  }

  loadPointsHistory(): void {
    this.isLoadingPoints.set(true);
    this.pointsService.getUserTransactions(this.currentUserId, 1, 50).subscribe({
      next: (response) => {
        console.log('Points history API response:', response);
        
        if (response.success && response.data) {
          // Handle different response structures
          let transactions: PointsTransactionDto[] = [];
          
          if (Array.isArray(response.data)) {
            transactions = response.data;
          } else if ((response.data as any).items && Array.isArray((response.data as any).items)) {
            transactions = (response.data as any).items;
          }
          
          console.log('Extracted transactions:', transactions);
          
          // Filter only EARNED transactions for Earning History
          // Backend returns 'Earned' for TransactionType (not 'Credit')
          // Exclude refund transactions as they belong in Redemption History:
          // 1. TransactionSource = 'Redemption' (new refunds)
          // 2. Description contains 'refund' (legacy refunds with wrong source)
          const earnedTransactions = transactions.filter((t: PointsTransactionDto) => {
            if (t.transactionType !== 'Earned') return false;
            if (t.transactionSource === 'Redemption') return false;
            // Also filter out any transaction with 'refund' in description (legacy data)
            const desc = (t.description || '').toLowerCase();
            if (desc.includes('refund')) return false;
            return true;
          });
          
          const mappedTransactions: PointTransaction[] = earnedTransactions.map((t: PointsTransactionDto) => {
            const points = t.userPoints ?? t.points ?? 0;
            const dateStr = t.timestamp ?? t.createdAt ?? new Date().toISOString();
            
            // Determine the source for display
            let source = 'Direct';
            if (t.transactionSource) {
              source = t.transactionSource;
            } else if (t.eventName || t.eventId) {
              source = 'Event';
            }
            
            // Parse description to extract event name and rank
            const { description, eventName, rank } = this.parseTransactionDescription(
              t.description || 'Points Earned', 
              t.eventName, 
              t.eventRank
            );
            
            // Use event description from backend if available, otherwise use parsed description
            const displayDescription = t.eventDescription || description;
            
            return {
              id: t.id,
              date: new Date(dateStr).toISOString().split('T')[0],
              description: displayDescription,
              eventDescription: t.eventDescription,
              points: points,
              type: 'earned' as const,
              status: 'Completed',
              source: source,
              eventName: eventName,
              rank: rank
            };
          });

          console.log('Mapped earning transactions:', mappedTransactions);
          this.pointsHistory.set(mappedTransactions);
        } else {
          console.log('No data in response or response not successful');
        }
        this.isLoadingPoints.set(false);
      },
      error: (error) => {
        console.error('Error loading points history:', error);
        this.toast.error('Failed to load earning history');
        this.isLoadingPoints.set(false);
      }
    });
  }

  // Helper method to parse description and extract event name and rank
  private parseTransactionDescription(
    description: string, 
    apiEventName?: string, 
    apiEventRank?: number
  ): { description: string; eventName?: string; rank?: number } {
    if (!description) {
      return { description: 'Points Earned', eventName: apiEventName, rank: apiEventRank };
    }
    
    let eventName = apiEventName;
    let rank = apiEventRank;
    let cleanDesc = description;
    
    // Pattern: "Winner of event: EventName (Rank #1)" or similar
    const winnerPattern = /Winner of event:\s*([^(]+)\s*\(Rank #(\d+)\)/i;
    const winnerMatch = description.match(winnerPattern);
    
    if (winnerMatch) {
      eventName = eventName || winnerMatch[1].trim();
      rank = rank || parseInt(winnerMatch[2], 10);
      cleanDesc = 'Event Winner';
    }
    
    // Pattern: "Redemption cancellation refund - Redemption ID: xxx"
    const refundPattern = /Redemption (cancellation|rejection) refund/i;
    if (refundPattern.test(description)) {
      cleanDesc = 'Points Refunded';
    }
    
    // Clean up any remaining UUIDs and technical details
    cleanDesc = cleanDesc.replace(/\s*-?\s*Redemption ID:\s*[a-f0-9-]+/gi, '');
    cleanDesc = cleanDesc.replace(/[a-f0-9]{8}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{12}/gi, '');
    cleanDesc = cleanDesc.replace(/\s+-\s*$/g, '').replace(/\s+/g, ' ').trim();
    
    // If description becomes empty or too short, provide a default
    if (cleanDesc.length < 3) {
      cleanDesc = 'Points Earned';
    }
    
    return { description: cleanDesc, eventName, rank };
  }

  // Helper method to clean up descriptions by removing IDs and technical details
  private cleanDescription(description: string): string {
    if (!description) return 'Points Earned';
    
    // Remove Redemption ID patterns like "Redemption ID: a7eb3f21-99b6-4abc-af1d-b01d6477e553"
    let cleaned = description.replace(/\s*-?\s*Redemption ID:\s*[a-f0-9-]+/gi, '');
    
    // Remove UUID patterns standalone
    cleaned = cleaned.replace(/[a-f0-9]{8}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{12}/gi, '');
    
    // Clean up any double spaces or trailing dashes
    cleaned = cleaned.replace(/\s+-\s*$/g, '').replace(/\s+/g, ' ').trim();
    
    // If description becomes empty or too short after cleaning, provide a default
    if (cleaned.length < 3) {
      cleaned = 'Points Earned';
    }
    
    return cleaned;
  }

  loadRedemptionHistory(): void {
    this.isLoadingRedemptions.set(true);
    this.redemptionService.getMyRedemptions().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          const redemptions = Array.isArray(response.data) ? response.data : 
                             (response.data as any).items || [];
          
          const mappedRedemptions: RedemptionRecord[] = redemptions.map((r: RedemptionDto) => {
            // Map the backend status to frontend status - backend now has separate Rejected status
            const displayStatus = this.mapRedemptionStatus(r.status);
            
            // Mark as refunded if cancelled or rejected (points were returned)
            const isRefunded = displayStatus === 'cancelled' || displayStatus === 'rejected';
            
            return {
              id: r.id,
              date: new Date(r.requestedAt).toISOString().split('T')[0],
              productName: r.productName || 'Product',
              points: r.pointsSpent,
              quantity: r.quantity || 1,
              status: displayStatus,
              rejectionReason: r.rejectionReason,
              isRefunded: isRefunded
            };
          });

          this.redemptionHistory.set(mappedRedemptions);
          
          // Note: pendingPoints is now loaded from the account API (stored in database)
          // This ensures consistency across the system
        }
        this.isLoadingRedemptions.set(false);
      },
      error: (error) => {
        console.error('Error loading redemption history:', error);
        this.toast.error('Failed to load redemption history');
        this.isLoadingRedemptions.set(false);
      }
    });
  }

  private mapRedemptionStatus(status: string | number): 'pending' | 'approved' | 'rejected' | 'cancelled' | 'delivered' {
    // Handle numeric status values from backend enum
    if (typeof status === 'number') {
      const numericMap: { [key: number]: 'pending' | 'approved' | 'rejected' | 'cancelled' | 'delivered' } = {
        0: 'pending',    // RedemptionStatus.Pending
        1: 'approved',   // RedemptionStatus.Approved
        2: 'delivered',  // RedemptionStatus.Delivered
        3: 'cancelled',  // RedemptionStatus.Cancelled
        4: 'rejected'    // RedemptionStatus.Rejected
      };
      return numericMap[status] || 'pending';
    }

    // Handle string status values
    const statusMap: { [key: string]: 'pending' | 'approved' | 'rejected' | 'cancelled' | 'delivered' } = {
      'Pending': 'pending',
      'Approved': 'approved',
      'Delivered': 'delivered',
      'Rejected': 'rejected',
      'Cancelled': 'cancelled'
    };
    return statusMap[status] || 'pending';
  }

  switchTab(tab: 'points' | 'redemptions'): void {
    this.activeTab = tab;
  }

  // Pagination methods for Earnings
  goToEarningsPage(page: number): void {
    if (page >= 1 && page <= this.earningsTotalPages()) {
      this.earningsCurrentPage.set(page);
    }
  }
  
  nextEarningsPage(): void {
    if (this.earningsCurrentPage() < this.earningsTotalPages()) {
      this.earningsCurrentPage.set(this.earningsCurrentPage() + 1);
    }
  }
  
  previousEarningsPage(): void {
    if (this.earningsCurrentPage() > 1) {
      this.earningsCurrentPage.set(this.earningsCurrentPage() - 1);
    }
  }
  
  getEarningsPageNumbers(): number[] {
    const total = this.earningsTotalPages();
    const current = this.earningsCurrentPage();
    const pages: number[] = [];
    
    if (total <= 7) {
      for (let i = 1; i <= total; i++) pages.push(i);
    } else {
      pages.push(1);
      if (current > 3) pages.push(-1);
      for (let i = Math.max(2, current - 1); i <= Math.min(total - 1, current + 1); i++) {
        pages.push(i);
      }
      if (current < total - 2) pages.push(-1);
      pages.push(total);
    }
    return pages;
  }

  // Pagination methods for Redemptions
  goToRedemptionsPage(page: number): void {
    if (page >= 1 && page <= this.redemptionsTotalPages()) {
      this.redemptionsCurrentPage.set(page);
    }
  }
  
  nextRedemptionsPage(): void {
    if (this.redemptionsCurrentPage() < this.redemptionsTotalPages()) {
      this.redemptionsCurrentPage.set(this.redemptionsCurrentPage() + 1);
    }
  }
  
  previousRedemptionsPage(): void {
    if (this.redemptionsCurrentPage() > 1) {
      this.redemptionsCurrentPage.set(this.redemptionsCurrentPage() - 1);
    }
  }
  
  getRedemptionsPageNumbers(): number[] {
    const total = this.redemptionsTotalPages();
    const current = this.redemptionsCurrentPage();
    const pages: number[] = [];
    
    if (total <= 7) {
      for (let i = 1; i <= total; i++) pages.push(i);
    } else {
      pages.push(1);
      if (current > 3) pages.push(-1);
      for (let i = Math.max(2, current - 1); i <= Math.min(total - 1, current + 1); i++) {
        pages.push(i);
      }
      if (current < total - 2) pages.push(-1);
      pages.push(total);
    }
    return pages;
  }

  async cancelRedemption(redemption: RedemptionRecord): Promise<void> {
    if (redemption.status !== 'pending') {
      this.toast.warning('Only pending redemptions can be cancelled');
      return;
    }

    const confirmed = await this.confirmDialog.confirmCancellation('Are you sure you want to cancel this redemption? Your points will be refunded.');
    if (confirmed) {
      this.redemptionService.cancelRedemption(redemption.id, { cancellationReason: 'Cancelled by employee' }).subscribe({
        next: (response) => {
          if (response.success) {
            this.toast.success('Redemption cancelled successfully. Points have been refunded to your account.');
            this.loadRedemptionHistory();
            this.loadAccountSummary(); // Refresh points as they should be returned
          } else {
            this.toast.error(response.message || 'Failed to cancel redemption');
          }
        },
        error: (error) => {
          console.error('Error cancelling redemption:', error);
          this.toast.error('Failed to cancel redemption');
        }
      });
    }
  }

  getStatusClass(status: string): string {
    const statusMap: { [key: string]: string } = {
      'Completed': 'status-completed',
      'Approved': 'status-approved',
      'pending': 'status-pending',
      'approved': 'status-approved',
      'delivered': 'status-delivered',
      'rejected': 'status-rejected',
      'cancelled': 'status-cancelled'
    };
    return statusMap[status] || '';
  }

  getTransactionIcon(type: string): string {
    const icons: { [key: string]: string } = {
      'earned': 'check',
      'redeemed': 'gift',
      'expired': 'pending'
    };
    return icons[type] || 'edit';
  }

  isLoading(): boolean {
    return this.isLoadingPoints() || this.isLoadingRedemptions() || this.isLoadingAccount();
  }
}
