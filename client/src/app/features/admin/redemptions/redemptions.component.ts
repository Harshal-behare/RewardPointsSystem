import { Component, OnInit, signal, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs';
import { CardComponent } from '../../../shared/components/card/card.component';
import { BadgeComponent } from '../../../shared/components/badge/badge.component';
import { RedemptionService } from '../../../core/services/redemption.service';
import { ToastService } from '../../../core/services/toast.service';

interface RedemptionRequest {
  id: string;
  userId: string;
  userName: string;
  userEmail: string;
  productId: string;
  productName: string;
  pointsSpent: number;
  status: 'Pending' | 'Approved' | 'Rejected' | 'Delivered' | 'Cancelled';
  requestedAt: Date;
  approvedAt?: Date;
  deliveredAt?: Date;
  deliveryAddress?: string;
  notes?: string;
  rejectionReason?: string;  // Add rejection reason field
}

interface Stats {
  total: number;
  pending: number;
  approved: number;
  rejected: number;
}

@Component({
  selector: 'app-admin-redemptions',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    CardComponent,
    BadgeComponent
  ],
  templateUrl: './redemptions.component.html',
  styleUrls: ['./redemptions.component.scss']
})
export class AdminRedemptionsComponent implements OnInit {
  // Filter state
  currentFilter = signal<'All' | 'Pending' | 'Approved' | 'Rejected'>('All');
  
  // Stats
  stats = signal<Stats>({
    total: 0,
    pending: 0,
    approved: 0,
    rejected: 0
  });

  // Modal states
  showApproveModal = signal(false);
  showRejectModal = signal(false);
  showDetailsModal = signal(false);
  selectedRequest = signal<RedemptionRequest | null>(null);
  
  // Form fields
  rejectionReason = signal('');
  rejectionError = signal('');

  redemptionRequests = signal<RedemptionRequest[]>([]);
  filteredRequests = signal<RedemptionRequest[]>([]);

  constructor(
    private router: Router,
    private redemptionService: RedemptionService,
    private toast: ToastService
  ) {
    // Use effect to reload data when navigating back to redemptions
    effect(() => {
      this.router.events.pipe(
        filter(event => event instanceof NavigationEnd)
      ).subscribe(() => {
        this.loadRedemptions();
      });
    });
  }

  ngOnInit(): void {
    this.loadRedemptions();
  }

  loadRedemptions(): void {
    this.redemptionService.getRedemptions().subscribe({
      next: (response) => {
        console.log('Redemptions response:', response);  // Debug log
        if (response.success && response.data) {
          // Handle both array and paged response formats
          let data: any[] = [];
          if (Array.isArray(response.data)) {
            data = response.data;
          } else if ((response.data as any).items) {
            data = (response.data as any).items;
          } else if ((response.data as any).Items) {
            data = (response.data as any).Items;
          }
          
          this.redemptionRequests.set(data.map((r: any) => this.mapRedemptionToDisplay(r)));
          this.calculateStats();
          this.filterRequests();
        } else {
          this.toast.error('Failed to load redemptions');
          this.redemptionRequests.set([]);
          this.calculateStats();
          this.filterRequests();
        }
      },
      error: (error) => {
        console.error('Error loading redemptions:', error);
        this.toast.error('Failed to load redemptions from server');
        this.redemptionRequests.set([]);
        this.calculateStats();
        this.filterRequests();
      }
    });
  }

  private mapRedemptionToDisplay(redemption: any): RedemptionRequest {
    // Determine if this is a rejected redemption (Cancelled with rejectionReason)
    let status = this.mapRedemptionStatus(redemption.status);
    const rejectionReason = redemption.rejectionReason || redemption.cancellationReason;
    
    // If status is Cancelled but has a rejection reason, treat it as Rejected
    if (status === 'Cancelled' && rejectionReason) {
      status = 'Rejected';
    }
    
    return {
      id: redemption.id || redemption.redemptionId,
      userId: redemption.userId,
      userName: redemption.userName || redemption.userFullName || 'Unknown User',
      userEmail: redemption.userEmail || '',
      productId: redemption.productId,
      productName: redemption.productName || 'Unknown Product',
      pointsSpent: redemption.pointsSpent || redemption.pointsCost || 0,
      status: status,
      requestedAt: new Date(redemption.requestedAt || redemption.createdAt),
      approvedAt: redemption.approvedAt ? new Date(redemption.approvedAt) : undefined,
      deliveredAt: redemption.deliveredAt ? new Date(redemption.deliveredAt) : undefined,
      deliveryAddress: redemption.deliveryAddress || redemption.shippingAddress,
      notes: redemption.notes || redemption.adminNotes,
      rejectionReason: rejectionReason
    };
  }

  private mapRedemptionStatus(status: string | number): 'Pending' | 'Approved' | 'Rejected' | 'Delivered' | 'Cancelled' {
    if (typeof status === 'number') {
      // Backend enum: Pending=0, Approved=1, Delivered=2, Cancelled=3
      const statusMap: { [key: number]: 'Pending' | 'Approved' | 'Rejected' | 'Delivered' | 'Cancelled' } = {
        0: 'Pending',
        1: 'Approved',
        2: 'Delivered',
        3: 'Cancelled'
      };
      return statusMap[status] || 'Pending';
    }
    // String mapping - treat Cancelled with RejectionReason as Rejected
    const statusStr = (status || '').toLowerCase();
    if (statusStr === 'pending') return 'Pending';
    if (statusStr === 'approved') return 'Approved';
    if (statusStr === 'delivered') return 'Delivered';
    if (statusStr === 'cancelled') return 'Cancelled';
    if (statusStr === 'rejected') return 'Rejected';
    return 'Pending';
  }

  private loadFallbackData(): void {
    this.redemptionRequests.set([
      {
        id: '1',
        userId: 'user-001',
        userName: 'John Doe',
        userEmail: 'john.doe@company.com',
        productId: 'prod-001',
        productName: 'Wireless Headphones',
        pointsSpent: 5000,
        status: 'Pending',
        requestedAt: new Date('2026-01-10T10:30:00'),
        deliveryAddress: '123 Main St, Suite 100, Cityville, ST 12345',
        notes: 'Please deliver during office hours (9 AM - 5 PM)'
      },
      {
        id: '2',
        userId: 'user-002',
        userName: 'Jane Smith',
        userEmail: 'jane.smith@company.com',
        productId: 'prod-002',
        productName: 'Coffee Maker',
        pointsSpent: 3500,
        status: 'Pending',
        requestedAt: new Date('2026-01-12T14:15:00'),
        deliveryAddress: '456 Oak Avenue, Apt 5B, Townsburg, ST 54321'
      },
      {
        id: '3',
        userId: 'user-003',
        userName: 'Mike Johnson',
        userEmail: 'mike.j@company.com',
        productId: 'prod-003',
        productName: 'Fitness Tracker',
        pointsSpent: 2500,
        status: 'Approved',
        requestedAt: new Date('2026-01-09T09:00:00'),
        approvedAt: new Date('2026-01-09T15:00:00'),
        deliveryAddress: '789 Elm Street, Building C, Villageton, ST 98765'
      }
    ]);
    this.toast.warning('Using demo data - API unavailable');
  }

  calculateStats(): void {
    const requests = this.redemptionRequests();
    this.stats.set({
      total: requests.length,
      pending: requests.filter(r => r.status === 'Pending').length,
      approved: requests.filter(r => r.status === 'Approved' || r.status === 'Delivered').length,
      rejected: requests.filter(r => r.status === 'Rejected' || r.status === 'Cancelled').length
    });
  }

  setFilter(filter: 'All' | 'Pending' | 'Approved' | 'Rejected'): void {
    this.currentFilter.set(filter);
    this.filterRequests();
  }

  filterRequests(): void {
    const filter = this.currentFilter();
    const requests = this.redemptionRequests();
    
    if (filter === 'All') {
      this.filteredRequests.set([...requests]);
    } else if (filter === 'Approved') {
      this.filteredRequests.set(requests.filter(r => 
        r.status === 'Approved' || r.status === 'Delivered'
      ));
    } else if (filter === 'Rejected') {
      this.filteredRequests.set(requests.filter(r => 
        r.status === 'Rejected' || r.status === 'Cancelled'
      ));
    } else {
      this.filteredRequests.set(requests.filter(r => r.status === filter));
    }
  }

  getUserInitials(userName: string): string {
    return userName.split(' ').map(n => n[0]).join('');
  }

  getStatusVariant(status: string): 'success' | 'warning' | 'info' | 'danger' | 'secondary' {
    const variantMap: { [key: string]: 'success' | 'warning' | 'info' | 'danger' | 'secondary' } = {
      'Pending': 'warning',
      'Approved': 'info',
      'Delivered': 'success',
      'Rejected': 'danger',
      'Cancelled': 'secondary'
    };
    return variantMap[status] || 'secondary';
  }

  // Approve Modal Methods
  openApproveModal(request: RedemptionRequest): void {
    this.selectedRequest.set(request);
    this.showApproveModal.set(true);
  }

  closeApproveModal(): void {
    this.showApproveModal.set(false);
    this.selectedRequest.set(null);
  }

  confirmApprove(): void {
    const request = this.selectedRequest();
    if (request) {
      this.redemptionService.approveRedemption(request.id).subscribe({
        next: (response) => {
          if (response.success) {
            const updatedRequest = {
              ...request,
              status: 'Approved' as const,
              approvedAt: new Date()
            };
            
            // Update the request in the list
            const updatedRequests = this.redemptionRequests().map(r => 
              r.id === request.id ? updatedRequest : r
            );
            this.redemptionRequests.set(updatedRequests);
            
            // Recalculate stats and filter
            this.calculateStats();
            this.filterRequests();
            
            this.toast.success(`Redemption approved! ${request.pointsSpent} points deducted from ${request.userName}'s account.`);
          } else {
            this.toast.error(response.message || 'Failed to approve redemption');
          }
        },
        error: (error) => {
          console.error('Error approving redemption:', error);
          // Show backend validation errors
          this.toast.showValidationErrors(error);
        }
      });
    }
    this.closeApproveModal();
  }

  // Reject Modal Methods
  openRejectModal(request: RedemptionRequest): void {
    this.selectedRequest.set(request);
    this.rejectionReason.set('');
    this.rejectionError.set('');
    this.showRejectModal.set(true);
  }

  closeRejectModal(): void {
    this.showRejectModal.set(false);
    this.selectedRequest.set(null);
    this.rejectionReason.set('');
    this.rejectionError.set('');
  }

  confirmReject(): void {
    // Validate rejection reason
    const reason = this.rejectionReason().trim();
    if (!reason || reason.length === 0) {
      this.rejectionError.set('Please provide a reason for rejecting this request');
      return;
    }

    if (reason.length < 10) {
      this.rejectionError.set('Reason must be at least 10 characters long');
      return;
    }

    const request = this.selectedRequest();
    if (request) {
      this.redemptionService.rejectRedemption(request.id, reason).subscribe({
        next: (response) => {
          if (response.success) {
            const updatedRequest = {
              ...request,
              status: 'Rejected' as const,
              rejectionReason: reason
            };
            
            // Update the request in the list
            const updatedRequests = this.redemptionRequests().map(r => 
              r.id === request.id ? updatedRequest : r
            );
            this.redemptionRequests.set(updatedRequests);
            
            // Recalculate stats and filter
            this.calculateStats();
            this.filterRequests();
            
            this.closeRejectModal();
            this.toast.success(`Redemption rejected. ${request.pointsSpent} points have been refunded to ${request.userName}'s account.`);
          } else {
            this.rejectionError.set(response.message || 'Failed to reject redemption');
          }
        },
        error: (error) => {
          console.error('Error rejecting redemption:', error);
          // Show backend validation errors in the modal
          const messages = error?.error?.errors 
            ? Object.values(error.error.errors).flat().join(' ')
            : error?.error?.message || error?.message || 'Failed to reject redemption';
          this.rejectionError.set(messages as string);
        }
      });
    }
  }

  // Details Modal Methods
  openDetailsModal(request: RedemptionRequest): void {
    this.selectedRequest.set(request);
    this.showDetailsModal.set(true);
  }

  closeDetailsModal(): void {
    this.showDetailsModal.set(false);
    this.selectedRequest.set(null);
  }
}
