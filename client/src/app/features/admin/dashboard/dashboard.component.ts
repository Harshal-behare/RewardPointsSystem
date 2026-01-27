import { Component, OnInit, signal, computed, effect, model } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, NavigationEnd } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { filter } from 'rxjs';
import { CardComponent } from '../../../shared/components/card/card.component';
import { ButtonComponent } from '../../../shared/components/button/button.component';
import { KpiCardComponent } from './components/kpi-card/kpi-card.component';
import { AdminService, DashboardStats } from '../../../core/services/admin.service';
import { EventService, CreateEventDto } from '../../../core/services/event.service';
import { ProductService, CreateProductDto } from '../../../core/services/product.service';
import { ToastService } from '../../../core/services/toast.service';

interface KpiData {
  icon: string;
  label: string;
  value: number | string;
  trend: number;
  route: string;
}

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
    ButtonComponent,
    KpiCardComponent
  ],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class AdminDashboardComponent implements OnInit {
  // Signals for reactive state management
  isLoading = signal(true);
  
  kpiData = signal<KpiData[]>([
    { icon: '👥', label: 'Total Users', value: 0, trend: 0, route: '/admin/users' },
    { icon: '📅', label: 'Total Events', value: 0, trend: 0, route: '/admin/events' },
    { icon: '🎁', label: 'Total Products', value: 0, trend: 0, route: '/admin/products' },
    { icon: '⭐', label: 'Points Distributed', value: '0', trend: 0, route: '/admin/dashboard' },
    { icon: '⏳', label: 'Pending Redemptions', value: 0, trend: 0, route: '/admin/redemptions' },
  ]);

  recentActivities = signal<RecentActivity[]>([]);

  chartData = signal({
    labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun'],
    pointsAwarded: [0, 0, 0, 0, 0, 0],
    redemptions: [0, 0, 0, 0, 0, 0],
  });

  // Quick Action Modals
  showEventModal = signal(false);
  showProductModal = signal(false);
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

  constructor(
    private router: Router,
    private adminService: AdminService,
    private eventService: EventService,
    private productService: ProductService,
    private toast: ToastService
  ) {
    // Use effect to reload data on route changes
    effect(() => {
      this.router.events.pipe(
        filter(event => event instanceof NavigationEnd)
      ).subscribe(() => {
        this.loadDashboardData();
      });
    });
  }

  ngOnInit(): void {
    this.loadDashboardData();
  }

  loadDashboardData(): void {
    this.isLoading.set(true);
    
    this.adminService.getDashboardStats().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          const stats = response.data;
          this.updateKpiData(stats);
        } else {
          this.useFallbackData();
        }
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading dashboard data:', error);
        this.useFallbackData();
        this.isLoading.set(false);
      }
    });
  }

  private updateKpiData(stats: DashboardStats): void {
    this.kpiData.set([
      { icon: '👥', label: 'Total Users', value: stats.totalUsers || 0, trend: 0, route: '/admin/users' },
      { icon: '📅', label: 'Total Events', value: stats.totalEvents || 0, trend: 0, route: '/admin/events' },
      { icon: '🎁', label: 'Total Products', value: stats.totalProducts || 0, trend: 0, route: '/admin/products' },
      { icon: '⭐', label: 'Points Distributed', value: this.formatNumber(stats.totalPointsDistributed || 0), trend: 0, route: '/admin/dashboard' },
      { icon: '⏳', label: 'Pending Redemptions', value: stats.pendingRedemptions || 0, trend: 0, route: '/admin/redemptions' },
    ]);
    
    // Clear recent activities - no endpoint available yet
    this.recentActivities.set([]);
    
    // Clear chart data - no historical data available yet
    this.chartData.set({
      labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun'],
      pointsAwarded: [0, 0, 0, 0, 0, 0],
      redemptions: [0, 0, 0, 0, 0, 0],
    });
  }

  private useFallbackData(): void {
    this.kpiData.set([
      { icon: '👥', label: 'Total Users', value: 0, trend: 0, route: '/admin/users' },
      { icon: '📅', label: 'Total Events', value: 0, trend: 0, route: '/admin/events' },
      { icon: '🎁', label: 'Total Products', value: 0, trend: 0, route: '/admin/products' },
      { icon: '⭐', label: 'Points Distributed', value: '0', trend: 0, route: '/admin/dashboard' },
      { icon: '⏳', label: 'Pending Redemptions', value: 0, trend: 0, route: '/admin/redemptions' },
    ]);
    this.recentActivities.set([]);
    this.chartData.set({
      labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun'],
      pointsAwarded: [0, 0, 0, 0, 0, 0],
      redemptions: [0, 0, 0, 0, 0, 0],
    });
  }

  private formatNumber(num: number): string {
    if (num >= 1000000) return (num / 1000000).toFixed(1) + 'M';
    if (num >= 1000) return (num / 1000).toFixed(1) + 'K';
    return num.toString();
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
    this.showEventModal.set(true);
  }

  closeEventModal(): void {
    this.showEventModal.set(false);
  }

  createEvent(): void {
    const event = this.newEvent;
    if (!event.name || !event.eventDate) {
      this.toast.error('Please fill in all required fields');
      return;
    }

    const eventData: CreateEventDto = {
      name: event.name,
      description: event.description,
      eventDate: new Date(event.eventDate).toISOString(),
      totalPointsPool: event.pointsPool
    };

    this.eventService.createEvent(eventData).subscribe({
      next: (response) => {
        if (response.success) {
          this.toast.success(`Event "${event.name}" created successfully!`);
          this.closeEventModal();
          this.loadDashboardData();
        } else {
          this.toast.error(response.message || 'Failed to create event');
        }
      },
      error: (error) => {
        console.error('Error creating event:', error);
        // Show backend validation errors
        this.toast.showValidationErrors(error);
      }
    });
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
    this.showProductModal.set(true);
  }

  closeProductModal(): void {
    this.showProductModal.set(false);
  }

  createProduct(): void {
    const product = this.newProduct;
    if (!product.name || product.pointsPrice <= 0) {
      this.toast.error('Please fill in all required fields');
      return;
    }

    const productData: CreateProductDto = {
      name: product.name,
      description: product.description,
      pointsPrice: product.pointsPrice,
      stockQuantity: product.stock,
      imageUrl: product.imageUrl || 'https://via.placeholder.com/150'
    };

    this.productService.createProduct(productData).subscribe({
      next: (response) => {
        if (response.success) {
          this.toast.success(`Product "${product.name}" created successfully!`);
          this.closeProductModal();
          this.loadDashboardData();
        } else {
          this.toast.error(response.message || 'Failed to create product');
        }
      },
      error: (error) => {
        console.error('Error creating product:', error);
        // Show backend validation errors
        this.toast.showValidationErrors(error);
      }
    });
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
