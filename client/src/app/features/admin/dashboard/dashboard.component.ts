import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardComponent } from '../../../shared/components/card/card.component';
import { BadgeComponent } from '../../../shared/components/badge/badge.component';
import { KpiCardComponent } from './components/kpi-card/kpi-card.component';

interface RecentActivity {
  id: number;
  type: string;
  description: string;
  timestamp: string;
  user: string;
}

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    CardComponent,
    BadgeComponent,
    KpiCardComponent
  ],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class AdminDashboardComponent implements OnInit {
  kpiData = [
    { icon: '👥', label: 'Total Users', value: 248, trend: 12 },
    { icon: '📅', label: 'Total Events', value: 36, trend: 8 },
    { icon: '🎁', label: 'Total Products', value: 124, trend: -3 },
    { icon: '⭐', label: 'Points Distributed', value: '45.2K', trend: 15 },
    { icon: '⏳', label: 'Pending Redemptions', value: 18, trend: -5 },
  ];

  recentActivities: RecentActivity[] = [
    {
      id: 1,
      type: 'Event Created',
      description: 'Summer Sales Challenge event was created',
      timestamp: '2 hours ago',
      user: 'John Doe'
    },
    {
      id: 2,
      type: 'Product Added',
      description: 'New product "Wireless Headphones" added to catalog',
      timestamp: '4 hours ago',
      user: 'Jane Smith'
    },
    {
      id: 3,
      type: 'Points Awarded',
      description: '500 points awarded to 15 users for completing training',
      timestamp: '6 hours ago',
      user: 'Admin'
    },
    {
      id: 4,
      type: 'Redemption Approved',
      description: 'Product redemption approved for employee #1245',
      timestamp: '1 day ago',
      user: 'Admin'
    },
    {
      id: 5,
      type: 'User Registered',
      description: '3 new employees joined the system',
      timestamp: '2 days ago',
      user: 'System'
    },
  ];

  chartData = {
    labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun'],
    pointsAwarded: [3200, 4100, 3800, 5200, 4800, 5500],
    redemptions: [1200, 1500, 1800, 2100, 2400, 2200],
  };

  ngOnInit(): void {
    // Load dashboard data
  }

  getActivityTypeClass(type: string): 'success' | 'warning' | 'info' | 'danger' | 'secondary' {
    const typeMap: { [key: string]: 'success' | 'warning' | 'info' | 'danger' | 'secondary' } = {
      'Event Created': 'success',
      'Product Added': 'info',
      'Points Awarded': 'success',
      'Redemption Approved': 'warning',
      'User Registered': 'secondary'
    };
    return typeMap[type] || 'secondary';
  }
}
