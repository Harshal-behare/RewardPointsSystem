import { Component, OnInit, signal, computed, DestroyRef, inject } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CardComponent } from '../../../shared/components/card/card.component';
import { BadgeComponent } from '../../../shared/components/badge/badge.component';
import { IconComponent } from '../../../shared/components/icon/icon.component';
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
  quantity: number;
  status: 'Pending' | 'Approved' | 'Rejected' | 'Cancelled' | 'Delivered';
  requestedAt: Date;
  approvedAt?: Date;
  deliveredAt?: Date;
  notes?: string;
  rejectionReason?: string;
}

interface Stats {
  total: number;
  pending: number;
  approved: number;
  rejected: number;
  cancelled: number;
  delivered: number;
}

@Component({
  selector: 'app-admin-redemptions',
  standalone: true,
  imports: [
    DatePipe,
    FormsModule,
    CardComponent,
    BadgeComponent,
    IconComponent
  ],
  templateUrl: './redemptions.component.html',
  styleUrl: './redemptions.component.scss'
})
export class AdminRedemptionsComponent implements OnInit {
  private destroyRef = inject(DestroyRef);
  
  // Filter state
  currentFilter = signal<'All' | 'Pending' | 'Approved' | 'Rejected' | 'Cancelled' | 'Delivered'>('All');
  
  // Search and Sort state
  searchQuery = signal('');
  sortField = signal<'userName' | 'productName' | 'pointsSpent' | 'requestedAt' | 'status'>('requestedAt');
  sortDirection = signal<'asc' | 'desc'>('desc');
  
  // Stats
  stats = signal<Stats>({
    total: 0,
    pending: 0,
    approved: 0,
    rejected: 0,
    cancelled: 0,
    delivered: 0
  });

  // Modal states
  showApproveModal = signal(false);
  showRejectModal = signal(false);
  showDetailsModal = signal(false);
  showDeliverModal = signal(false);
  selectedRequest = signal<RedemptionRequest | null>(null);
  
  // Form fields
  rejectionReason = signal('');
  rejectionError = signal('');

  redemptionRequests = signal<RedemptionRequest[]>([]);
  
  // Computed filtered requests with search and sort
  filteredRequests = computed(() => {
    let result = [...this.redemptionRequests()];
    
    // Apply status filter
    if (this.currentFilter() !== 'All') {
      result = result.filter(r => r.status === this.currentFilter());
    }
    
    // Apply search filter
    const query = this.searchQuery().toLowerCase().trim();
    if (query) {
      result = result.filter(r => 
        r.userName.toLowerCase().includes(query) ||
        r.userEmail.toLowerCase().includes(query) ||
        r.productName.toLowerCase().includes(query)
      );
    }
    
    // Apply sorting
    result.sort((a, b) => {
      let comparison = 0;
      const field = this.sortField();
      
      switch (field) {
        case 'userName':
          comparison = a.userName.localeCompare(b.userName);
          break;
        case 'productName':
          comparison = a.productName.localeCompare(b.productName);
          break;
        case 'pointsSpent':
          comparison = a.pointsSpent - b.pointsSpent;
          break;
        case 'requestedAt':
          comparison = new Date(a.requestedAt).getTime() - new Date(b.requestedAt).getTime();
          break;
        case 'status':
          comparison = a.status.localeCompare(b.status);
          break;
      }
      
      return this.sortDirection() === 'asc' ? comparison : -comparison;
    });
    
    return result;
  });

  // Pagination
  currentPage = signal(1);
  pageSize = signal(10);
  
  // Computed paginated requests
  paginatedRequests = computed(() => {
    const filtered = this.filteredRequests();
    const start = (this.currentPage() - 1) * this.pageSize();
    const end = start + this.pageSize();
    return filtered.slice(start, end);
  });
  
  totalPages = computed(() => Math.ceil(this.filteredRequests().length / this.pageSize()));

  constructor(
    private router: Router,
    private redemptionService: RedemptionService,
    private toast: ToastService
  ) {
    // Subscribe to route changes with automatic cleanup
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(() => {
      this.loadRedemptions();
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
        } else {
          this.toast.error('Failed to load redemptions');
          this.redemptionRequests.set([]);
          this.calculateStats();
        }
      },
      error: (error) => {
        console.error('Error loading redemptions:', error);
        this.toast.error('Failed to load redemptions from server');
        this.redemptionRequests.set([]);
        this.calculateStats();
      }
    });
  }

  private mapRedemptionToDisplay(redemption: any): RedemptionRequest {
    // Map status - now backend has separate Rejected status
    const status = this.mapRedemptionStatus(redemption.status);
    const rejectionReason = redemption.rejectionReason || redemption.cancellationReason;
    
    return {
      id: redemption.id || redemption.redemptionId,
      userId: redemption.userId,
      userName: redemption.userName || redemption.userFullName || 'Unknown User',
      userEmail: redemption.userEmail || '',
      productId: redemption.productId,
      productName: redemption.productName || 'Unknown Product',
      pointsSpent: redemption.pointsSpent || redemption.pointsCost || 0,
      quantity: redemption.quantity || 1,
      status: status,
      requestedAt: new Date(redemption.requestedAt || redemption.createdAt),
      approvedAt: redemption.approvedAt ? new Date(redemption.approvedAt) : undefined,
      deliveredAt: redemption.processedAt ? new Date(redemption.processedAt) : undefined,
      notes: redemption.notes || redemption.adminNotes,
      rejectionReason: rejectionReason
    };
  }

  private mapRedemptionStatus(status: string | number): 'Pending' | 'Approved' | 'Rejected' | 'Cancelled' | 'Delivered' {
    if (typeof status === 'number') {
      // Backend enum: Pending=0, Approved=1, Delivered=2, Cancelled=3, Rejected=4
      const statusMap: { [key: number]: 'Pending' | 'Approved' | 'Rejected' | 'Cancelled' | 'Delivered' } = {
        0: 'Pending',
        1: 'Approved',
        2: 'Delivered',
        3: 'Cancelled',
        4: 'Rejected'
      };
      return statusMap[status] || 'Pending';
    }
    // String mapping
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
        quantity: 1,
        status: 'Pending',
        requestedAt: new Date('2026-01-10T10:30:00'),
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
        quantity: 2,
        status: 'Pending',
        requestedAt: new Date('2026-01-12T14:15:00')
      },
      {
        id: '3',
        userId: 'user-003',
        userName: 'Mike Johnson',
        userEmail: 'mike.j@company.com',
        productId: 'prod-003',
        productName: 'Fitness Tracker',
        pointsSpent: 2500,
        quantity: 1,
        status: 'Approved',
        requestedAt: new Date('2026-01-09T09:00:00'),
        approvedAt: new Date('2026-01-09T15:00:00')
      }
    ]);
    this.toast.warning('Using demo data - API unavailable');
  }

  calculateStats(): void {
    const requests = this.redemptionRequests();
    this.stats.set({
      total: requests.length,
      pending: requests.filter(r => r.status === 'Pending').length,
      approved: requests.filter(r => r.status === 'Approved').length,
      rejected: requests.filter(r => r.status === 'Rejected').length,
      cancelled: requests.filter(r => r.status === 'Cancelled').length,
      delivered: requests.filter(r => r.status === 'Delivered').length
    });
  }

  setFilter(filter: 'All' | 'Pending' | 'Approved' | 'Rejected' | 'Cancelled' | 'Delivered'): void {
    this.currentFilter.set(filter);
    this.currentPage.set(1); // Reset to first page when filter changes
  }

  // Search method
  onSearchChange(): void {
    this.currentPage.set(1); // Reset to first page when search changes
  }
  
  // Sort methods
  toggleSort(field: 'userName' | 'productName' | 'pointsSpent' | 'requestedAt' | 'status'): void {
    if (this.sortField() === field) {
      this.sortDirection.set(this.sortDirection() === 'asc' ? 'desc' : 'asc');
    } else {
      this.sortField.set(field);
      this.sortDirection.set('asc');
    }
  }
  
  getSortIcon(field: string): string {
    if (this.sortField() !== field) return '↕️';
    return this.sortDirection() === 'asc' ? '↑' : '↓';
  }
  
  clearSearch(): void {
    this.searchQuery.set('');
    this.currentPage.set(1);
  }

  // Pagination methods
  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages()) {
      this.currentPage.set(page);
    }
  }
  
  nextPage(): void {
    if (this.currentPage() < this.totalPages()) {
      this.currentPage.set(this.currentPage() + 1);
    }
  }
  
  previousPage(): void {
    if (this.currentPage() > 1) {
      this.currentPage.set(this.currentPage() - 1);
    }
  }
  
  getPageNumbers(): number[] {
    const total = this.totalPages();
    const current = this.currentPage();
    const pages: number[] = [];
    
    if (total <= 7) {
      for (let i = 1; i <= total; i++) pages.push(i);
    } else {
      pages.push(1);
      if (current > 3) pages.push(-1); // ellipsis
      for (let i = Math.max(2, current - 1); i <= Math.min(total - 1, current + 1); i++) {
        pages.push(i);
      }
      if (current < total - 2) pages.push(-1); // ellipsis
      pages.push(total);
    }
    return pages;
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
            
            // Recalculate stats
            this.calculateStats();
            
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

    if (reason.length > 500) {
      this.rejectionError.set('Reason cannot exceed 500 characters');
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
            
            // Recalculate stats
            this.calculateStats();
            
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

  // Deliver Modal Methods
  openDeliverModal(request: RedemptionRequest): void {
    this.selectedRequest.set(request);
    this.showDeliverModal.set(true);
  }

  closeDeliverModal(): void {
    this.showDeliverModal.set(false);
    this.selectedRequest.set(null);
  }

  confirmDeliver(): void {
    const request = this.selectedRequest();
    if (request) {
      this.redemptionService.deliverRedemption(request.id).subscribe({
        next: (response) => {
          if (response.success) {
            const updatedRequest = {
              ...request,
              status: 'Delivered' as const,
              deliveredAt: new Date()
            };
            
            // Update the request in the list
            const updatedRequests = this.redemptionRequests().map(r => 
              r.id === request.id ? updatedRequest : r
            );
            this.redemptionRequests.set(updatedRequests);
            
            // Recalculate stats
            this.calculateStats();
            
            this.toast.success(`Redemption marked as delivered for ${request.userName}.`);
          } else {
            this.toast.error(response.message || 'Failed to mark redemption as delivered');
          }
        },
        error: (error) => {
          console.error('Error delivering redemption:', error);
          this.toast.showValidationErrors(error);
        }
      });
    }
    this.closeDeliverModal();
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
