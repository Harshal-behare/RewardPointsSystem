import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';

interface PointTransaction {
  id: number;
  date: string;
  description: string;
  points: number;
  type: 'earned' | 'redeemed' | 'expired';
  status: string;
  eventName?: string;
  rank?: number;
}

interface RedemptionRecord {
  id: number;
  date: string;
  productName: string;
  points: number;
  status: 'pending' | 'approved' | 'shipped' | 'delivered' | 'rejected';
  trackingNumber?: string;
  deliveryAddress: string;
}

@Component({
  selector: 'app-employee-account',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './account.component.html',
  styleUrl: './account.component.scss'
})
export class EmployeeAccountComponent implements OnInit {
  activeTab: 'points' | 'redemptions' = 'points';
  
  pointsHistory: PointTransaction[] = [];
  redemptionHistory: RedemptionRecord[] = [];

  // Summary stats
  totalEarned: number = 2450;
  totalRedeemed: number = 600;
  currentBalance: number = 1850;

  ngOnInit(): void {
    this.loadPointsHistory();
    this.loadRedemptionHistory();
  }

  loadPointsHistory(): void {
    this.pointsHistory = [
      {
        id: 1,
        date: '2024-01-28',
        description: 'Attended Q4 Sales Meeting',
        points: 250,
        type: 'earned',
        status: 'Completed',
        eventName: 'Q4 Sales Meeting'
      },
      {
        id: 2,
        date: '2024-01-25',
        description: 'Redeemed: Wireless Mouse',
        points: -150,
        type: 'redeemed',
        status: 'Approved'
      },
      {
        id: 3,
        date: '2024-01-22',
        description: 'Completed Training Module: Leadership Basics',
        points: 100,
        type: 'earned',
        status: 'Completed'
      },
      {
        id: 4,
        date: '2024-01-20',
        description: 'Participated in Team Building Event',
        points: 350,
        type: 'earned',
        status: 'Completed',
        eventName: 'Team Building Workshop'
      },
      {
        id: 5,
        date: '2024-01-15',
        description: 'Won Innovation Challenge',
        points: 500,
        type: 'earned',
        status: 'Completed',
        eventName: 'Innovation Challenge Finals',
        rank: 1
      },
      {
        id: 6,
        date: '2024-01-10',
        description: 'Redeemed: Coffee Maker',
        points: -350,
        type: 'redeemed',
        status: 'Delivered'
      },
      {
        id: 7,
        date: '2024-01-05',
        description: 'Attended Product Launch Event',
        points: 400,
        type: 'earned',
        status: 'Completed',
        eventName: 'Product Launch 2024'
      },
      {
        id: 8,
        date: '2023-12-28',
        description: 'Year-End Bonus Points',
        points: 300,
        type: 'earned',
        status: 'Completed'
      },
      {
        id: 9,
        date: '2023-12-20',
        description: 'Redeemed: Fitness Tracker',
        points: -200,
        type: 'redeemed',
        status: 'Delivered'
      },
      {
        id: 10,
        date: '2023-12-15',
        description: 'Completed Safety Training',
        points: 150,
        type: 'earned',
        status: 'Completed'
      }
    ];
  }

  loadRedemptionHistory(): void {
    this.redemptionHistory = [
      {
        id: 1,
        date: '2024-01-28',
        productName: 'Wireless Mouse - Logitech MX Master 3',
        points: 150,
        status: 'shipped',
        trackingNumber: 'TRK123456789',
        deliveryAddress: '123 Main St, Apt 4B, New York, NY 10001'
      },
      {
        id: 2,
        date: '2024-01-25',
        productName: 'Premium Coffee Maker',
        points: 350,
        status: 'delivered',
        trackingNumber: 'TRK987654321',
        deliveryAddress: '123 Main St, Apt 4B, New York, NY 10001'
      },
      {
        id: 3,
        date: '2024-01-20',
        productName: '$50 Amazon Gift Card',
        points: 400,
        status: 'approved',
        deliveryAddress: 'Email: john.doe@company.com'
      },
      {
        id: 4,
        date: '2024-01-15',
        productName: 'Fitness Tracker Band',
        points: 250,
        status: 'delivered',
        trackingNumber: 'TRK456789123',
        deliveryAddress: '123 Main St, Apt 4B, New York, NY 10001'
      },
      {
        id: 5,
        date: '2024-01-10',
        productName: 'Wireless Bluetooth Speaker',
        points: 300,
        status: 'pending',
        deliveryAddress: '123 Main St, Apt 4B, New York, NY 10001'
      }
    ];
  }

  switchTab(tab: 'points' | 'redemptions'): void {
    this.activeTab = tab;
  }

  getStatusClass(status: string): string {
    const statusMap: { [key: string]: string } = {
      'Completed': 'status-completed',
      'Approved': 'status-approved',
      'Delivered': 'status-delivered',
      'pending': 'status-pending',
      'approved': 'status-approved',
      'shipped': 'status-shipped',
      'delivered': 'status-delivered',
      'rejected': 'status-rejected'
    };
    return statusMap[status] || '';
  }

  getTransactionIcon(type: string): string {
    const icons: { [key: string]: string } = {
      'earned': '✅',
      'redeemed': '🎁',
      'expired': '⏰'
    };
    return icons[type] || '📝';
  }
}
