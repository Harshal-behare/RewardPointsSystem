import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CardComponent } from '../../../shared/components/card/card.component';
import { BadgeComponent } from '../../../shared/components/badge/badge.component';
import { ButtonComponent } from '../../../shared/components/button/button.component';
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
    FormsModule,
    CardComponent,
    BadgeComponent,
    ButtonComponent,
    KpiCardComponent
  ],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class AdminDashboardComponent implements OnInit {
  kpiData = [
    { icon: '👥', label: 'Total Users', value: 248, trend: 12, route: '/admin/users' },
    { icon: '📅', label: 'Total Events', value: 36, trend: 8, route: '/admin/events' },
    { icon: '🎁', label: 'Total Products', value: 124, trend: -3, route: '/admin/products' },
    { icon: '⭐', label: 'Points Distributed', value: '45.2K', trend: 15, route: '/admin/dashboard' },
    { icon: '⏳', label: 'Pending Redemptions', value: 18, trend: -5, route: '/admin/redemptions' },
  ];

  constructor(private router: Router) {}

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

  // Quick Action Modals
  showEventModal = false;
  showProductModal = false;
  newEvent = {
    name: '',
    description: '',
    eventDate: '',
    pointsPool: 0
  };
  newProduct = {
    name: '',
    description: '',
    category: '',
    pointsPrice: 0,
    stock: 0,
    imageUrl: ''
  };

  ngOnInit(): void {
    // Load dashboard data
  }

  navigateToPage(route: string): void {
    this.router.navigate([route]);
  }

  // Quick Action Methods
  openEventModal(): void {
    this.newEvent = {
      name: '',
      description: '',
      eventDate: '',
      pointsPool: 0
    };
    this.showEventModal = true;
  }

  closeEventModal(): void {
    this.showEventModal = false;
  }

  createEvent(): void {
    // TODO: Call API to create event
    console.log('Creating event:', this.newEvent);
    alert(`✓ Event "${this.newEvent.name}" created successfully!`);
    this.closeEventModal();
    // Optionally navigate to events page
    // this.router.navigate(['/admin/events']);
  }

  openProductModal(): void {
    this.newProduct = {
      name: '',
      description: '',
      category: 'Electronics',
      pointsPrice: 0,
      stock: 0,
      imageUrl: ''
    };
    this.showProductModal = true;
  }

  closeProductModal(): void {
    this.showProductModal = false;
  }

  createProduct(): void {
    // TODO: Call API to create product
    console.log('Creating product:', this.newProduct);
    alert(`✓ Product "${this.newProduct.name}" created successfully!`);
    this.closeProductModal();
    // Optionally navigate to products page
    // this.router.navigate(['/admin/products']);
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
