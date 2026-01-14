import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CardComponent } from '../../../shared/components/card/card.component';
import { BadgeComponent } from '../../../shared/components/badge/badge.component';

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
  currentFilter: 'All' | 'Pending' | 'Approved' | 'Rejected' = 'All';
  
  // Stats
  stats: Stats = {
    total: 0,
    pending: 0,
    approved: 0,
    rejected: 0
  };

  // Modal states
  showApproveModal = false;
  showRejectModal = false;
  showDetailsModal = false;
  selectedRequest: RedemptionRequest | null = null;
  
  // Form fields
  rejectionReason = '';
  rejectionError = '';

  // Sample redemption requests data
  redemptionRequests: RedemptionRequest[] = [
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
    },
    {
      id: '4',
      userId: 'user-004',
      userName: 'Sarah Williams',
      userEmail: 'sarah.w@company.com',
      productId: 'prod-004',
      productName: 'Desk Lamp',
      pointsSpent: 1500,
      status: 'Delivered',
      requestedAt: new Date('2026-01-08T11:20:00'),
      approvedAt: new Date('2026-01-08T16:00:00'),
      deliveredAt: new Date('2026-01-11T10:00:00'),
      deliveryAddress: '321 Pine Road, Unit 12, Hamlet City, ST 11111'
    },
    {
      id: '5',
      userId: 'user-005',
      userName: 'Tom Brown',
      userEmail: 'tom.brown@company.com',
      productId: 'prod-005',
      productName: 'Bluetooth Speaker',
      pointsSpent: 4000,
      status: 'Rejected',
      requestedAt: new Date('2026-01-07T13:45:00'),
      deliveryAddress: '555 Maple Drive, Floor 3, Boroughville, ST 22222',
      notes: 'Rejected due to insufficient points verification'
    },
    {
      id: '6',
      userId: 'user-006',
      userName: 'Emma Davis',
      userEmail: 'emma.davis@company.com',
      productId: 'prod-006',
      productName: 'Backpack',
      pointsSpent: 2000,
      status: 'Pending',
      requestedAt: new Date('2026-01-13T16:00:00'),
      deliveryAddress: '999 Cedar Lane, Suite 200, Metrocity, ST 33333'
    }
  ];

  filteredRequests: RedemptionRequest[] = [];

  ngOnInit(): void {
    this.calculateStats();
    this.filterRequests();
  }

  calculateStats(): void {
    this.stats.total = this.redemptionRequests.length;
    this.stats.pending = this.redemptionRequests.filter(r => r.status === 'Pending').length;
    this.stats.approved = this.redemptionRequests.filter(r => r.status === 'Approved' || r.status === 'Delivered').length;
    this.stats.rejected = this.redemptionRequests.filter(r => r.status === 'Rejected').length;
  }

  setFilter(filter: 'All' | 'Pending' | 'Approved' | 'Rejected'): void {
    this.currentFilter = filter;
    this.filterRequests();
  }

  filterRequests(): void {
    if (this.currentFilter === 'All') {
      this.filteredRequests = [...this.redemptionRequests];
    } else if (this.currentFilter === 'Approved') {
      this.filteredRequests = this.redemptionRequests.filter(r => 
        r.status === 'Approved' || r.status === 'Delivered'
      );
    } else {
      this.filteredRequests = this.redemptionRequests.filter(r => r.status === this.currentFilter);
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
    this.selectedRequest = request;
    this.showApproveModal = true;
  }

  closeApproveModal(): void {
    this.showApproveModal = false;
    this.selectedRequest = null;
  }

  confirmApprove(): void {
    if (this.selectedRequest) {
      console.log('Approving redemption:', this.selectedRequest.id);
      // TODO: Call API to approve redemption and deduct points
      this.selectedRequest.status = 'Approved';
      this.selectedRequest.approvedAt = new Date();
      
      // Update the request in the list
      const index = this.redemptionRequests.findIndex(r => r.id === this.selectedRequest?.id);
      if (index !== -1) {
        this.redemptionRequests[index] = { ...this.selectedRequest };
      }
      
      // Recalculate stats and filter
      this.calculateStats();
      this.filterRequests();
      
      // Show success message (TODO: use toast service)
      alert(`✓ Redemption approved successfully! ${this.selectedRequest.pointsSpent} points have been deducted from ${this.selectedRequest.userName}'s account.`);
    }
    this.closeApproveModal();
  }

  // Reject Modal Methods
  openRejectModal(request: RedemptionRequest): void {
    this.selectedRequest = request;
    this.rejectionReason = '';
    this.rejectionError = '';
    this.showRejectModal = true;
  }

  closeRejectModal(): void {
    this.showRejectModal = false;
    this.selectedRequest = null;
    this.rejectionReason = '';
    this.rejectionError = '';
  }

  confirmReject(): void {
    // Validate rejection reason
    if (!this.rejectionReason || this.rejectionReason.trim().length === 0) {
      this.rejectionError = 'Please provide a reason for rejecting this request';
      return;
    }

    if (this.rejectionReason.trim().length < 10) {
      this.rejectionError = 'Reason must be at least 10 characters long';
      return;
    }

    if (this.selectedRequest) {
      console.log('Rejecting redemption:', this.selectedRequest.id, 'Reason:', this.rejectionReason);
      // TODO: Call API to reject redemption with reason
      this.selectedRequest.status = 'Rejected';
      
      // Update the request in the list
      const index = this.redemptionRequests.findIndex(r => r.id === this.selectedRequest?.id);
      if (index !== -1) {
        this.redemptionRequests[index] = { ...this.selectedRequest };
      }
      
      // Recalculate stats and filter
      this.calculateStats();
      this.filterRequests();
      
      // Show success message (TODO: use toast service)
      alert(`Redemption request has been rejected. The employee will be notified.`);
    }
    this.closeRejectModal();
  }

  // Details Modal Methods
  openDetailsModal(request: RedemptionRequest): void {
    this.selectedRequest = request;
    this.showDetailsModal = true;
  }

  closeDetailsModal(): void {
    this.showDetailsModal = false;
    this.selectedRequest = null;
  }
}
