import { Component, OnInit, signal, computed, effect, model, DestroyRef, inject } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { DecimalPipe } from '@angular/common';
import { filter, takeUntil, forkJoin } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NgApexchartsModule } from 'ng-apexcharts';
import { CardComponent } from '../../../shared/components/card/card.component';
import { ButtonComponent } from '../../../shared/components/button/button.component';
import { KpiCardComponent } from './components/kpi-card/kpi-card.component';
import { IconComponent } from '../../../shared/components/icon/icon.component';
import { BadgeComponent } from '../../../shared/components/badge/badge.component';
import { AdminService, DashboardStats, EventStatusSummary, RedemptionSummary, InventoryAlert, PointsSummary, AdminBudgetResponse, SetBudgetRequest, BudgetHistoryItem } from '../../../core/services/admin.service';
import { EventService, CreateEventDto, EventDto } from '../../../core/services/event.service';
import { ProductService, CreateProductDto, CategoryDto } from '../../../core/services/product.service';
import { RedemptionService, RedemptionDto } from '../../../core/services/redemption.service';
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
    FormsModule,
    DecimalPipe,
    CardComponent,
    ButtonComponent,
    KpiCardComponent,
    IconComponent,
    BadgeComponent,
    NgApexchartsModule
  ],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class AdminDashboardComponent implements OnInit {
  private destroyRef = inject(DestroyRef);
  
  // Signals for reactive state management
  isLoading = signal(true);
  
  kpiData = signal<KpiData[]>([
    { icon: 'users', label: 'Active Users', value: 0, trend: 0, route: '/admin/users' },
    { icon: 'events', label: 'Active Events', value: 0, trend: 0, route: '/admin/events' },
    { icon: 'shopping-cart', label: 'Pending Redemptions', value: 0, trend: 0, route: '/admin/redemptions' },
    { icon: 'gift', label: 'Total Products', value: 0, trend: 0, route: '/admin/products' },
  ]);

  recentActivities = signal<RecentActivity[]>([]);

  chartData = signal({
    labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun'],
    pointsAwarded: [0, 0, 0, 0, 0, 0],
    redemptions: [0, 0, 0, 0, 0, 0],
  });

  // New dashboard sections
  eventStatusSummary = signal<EventStatusSummary>({
    draft: 0,
    upcoming: 0,
    active: 0,
    completed: 0
  });

  redemptionSummary = signal<RedemptionSummary>({
    pending: 0,
    approved: 0,
    rejected: 0,
    delivered: 0,
    cancelled: 0
  });

  inventoryAlerts = signal<InventoryAlert[]>([]);

  pointsSummary = signal<PointsSummary>({
    totalPointsDistributed: 0,
    totalPointsRedeemed: 0,
    totalPointsInCirculation: 0,
    pendingPoints: 0
  });

  eventsNeedingAttention = signal<EventDto[]>([]);

  // Budget Management State
  adminBudget = signal<AdminBudgetResponse | null>(null);
  budgetHistory = signal<BudgetHistoryItem[]>([]);
  showBudgetModal = signal(false);
  budgetModalMode = signal<'create' | 'edit'>('create');
  budgetForm = signal({
    budgetLimit: 50000,
    isHardLimit: false,
    warningThreshold: 80
  });
  budgetValidationErrors = signal<string[]>([]);

  // Budget computed properties
  budgetUsageClass = computed(() => {
    const budget = this.adminBudget();
    if (!budget) return '';
    if (budget.isOverBudget) return 'over-budget';
    if (budget.isWarningZone) return 'warning-zone';
    return 'within-budget';
  });

  // Chart Options
  redemptionChartOptions = computed(() => ({
    series: [
      this.redemptionSummary().pending,
      this.redemptionSummary().approved,
      this.redemptionSummary().delivered,
      this.redemptionSummary().rejected,
      this.redemptionSummary().cancelled
    ],
    chart: {
      type: 'donut' as const,
      height: 300,
      events: {
        dataPointSelection: () => {
          this.navigateToPage('/admin/redemptions');
        }
      }
    },
    labels: ['Pending', 'Approved', 'Delivered', 'Rejected', 'Cancelled'],
    colors: ['#F39C12', '#27AE60', '#3498DB', '#E74C3C', '#95A5A6'],
    legend: {
      position: 'bottom' as const
    },
    responsive: [{
      breakpoint: 480,
      options: {
        chart: { height: 250 },
        legend: { position: 'bottom' as const }
      }
    }]
  }));

  eventChartOptions = computed(() => ({
    series: [
      this.eventStatusSummary().draft,
      this.eventStatusSummary().upcoming,
      this.eventStatusSummary().active,
      this.eventStatusSummary().completed
    ],
    chart: {
      type: 'donut' as const,
      height: 300,
      events: {
        dataPointSelection: () => {
          this.navigateToPage('/admin/events');
        }
      }
    },
    labels: ['Draft', 'Upcoming', 'Active', 'Completed'],
    colors: ['#95A5A6', '#3498DB', '#27AE60', '#34495E'],
    legend: {
      position: 'bottom' as const
    },
    responsive: [{
      breakpoint: 480,
      options: {
        chart: { height: 250 },
        legend: { position: 'bottom' as const }
      }
    }]
  }));

  // Quick Action Modals
  showEventModal = signal(false);
  showProductModal = signal(false);
  eventModalValidationErrors = signal<string[]>([]);
  productModalValidationErrors = signal<string[]>([]);
  categories = signal<CategoryDto[]>([]);

  newEvent: any = {
    name: '',
    description: '',
    eventDate: '',
    eventEndDate: '',
    maxParticipants: 0,
    pointsPool: 0,
    firstPlacePoints: 0,
    secondPlacePoints: 0,
    thirdPlacePoints: 0,
    registrationStartDate: '',
    registrationEndDate: '',
    location: '',
    virtualLink: '',
    imageUrl: ''
  };

  newProduct: any = {
    name: '',
    description: '',
    categoryId: '',
    pointsPrice: 1,
    stock: 0,
    imageUrl: ''
  };

  constructor(
    private router: Router,
    private adminService: AdminService,
    private eventService: EventService,
    private productService: ProductService,
    private redemptionService: RedemptionService,
    private toast: ToastService
  ) {
    // Subscribe to route changes with automatic cleanup
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(() => {
      this.loadDashboardData();
    });
  }

  ngOnInit(): void {
    this.loadDashboardData();
    this.loadCategories();
  }

  loadCategories(): void {
    this.productService.getCategories().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.categories.set(response.data);
        }
      },
      error: (error) => {
        console.error('Error loading categories:', error);
      }
    });
  }

  loadDashboardData(): void {
    this.isLoading.set(true);

    // Load all dashboard data in parallel using forkJoin
    forkJoin({
      dashboardStats: this.adminService.getDashboardStats(),
      inventoryAlerts: this.adminService.getInventoryAlerts(),
      pointsSummary: this.adminService.getPointsSummary(),
      allEvents: this.eventService.getAllEventsAdmin(),
      allRedemptions: this.redemptionService.getRedemptions(),
      allProducts: this.productService.getAllProductsAdmin(),
      adminBudget: this.adminService.getBudget(),
      budgetHistory: this.adminService.getBudgetHistory(6)
    }).subscribe({
      next: (results) => {
        // Update KPI data
        if (results.dashboardStats.success && results.dashboardStats.data) {
          this.updateKpiData(results.dashboardStats.data);
        }

        // Update event status summary
        this.updateEventStatusSummary(results.allEvents);

        // Update redemption summary
        this.updateRedemptionSummary(results.allRedemptions);

        // Update inventory alerts - filter products with stock < 10
        if (results.inventoryAlerts.success && results.inventoryAlerts.data && results.inventoryAlerts.data.length > 0) {
          // Filter to only show items with stock < 10
          const lowStockAlerts = results.inventoryAlerts.data.filter((alert: InventoryAlert) => alert.currentStock < 10);
          this.inventoryAlerts.set(lowStockAlerts);
        } else if (results.allProducts.success && results.allProducts.data) {
          // Fallback: create inventory alerts from products with stock < 10
          const products = Array.isArray(results.allProducts.data) ? results.allProducts.data : [];
          const lowStockProducts = products
            .filter((p: any) => (p.stockQuantity ?? p.stock ?? 0) < 10 && p.isActive !== false)
            .map((p: any) => ({
              productId: p.id,
              productName: p.name,
              currentStock: p.stockQuantity ?? p.stock ?? 0,
              reorderLevel: 10,
              alertType: (p.stockQuantity ?? p.stock ?? 0) === 0 ? 'Out of Stock' : 'Low Stock'
            } as InventoryAlert));
          this.inventoryAlerts.set(lowStockProducts);
        }

        // Update points summary
        if (results.pointsSummary.success && results.pointsSummary.data) {
          this.pointsSummary.set(results.pointsSummary.data);
        }

        // Update events needing attention
        this.updateEventsNeedingAttention(results.allEvents);

        // Update admin budget
        if (results.adminBudget.success && results.adminBudget.data !== undefined) {
          this.adminBudget.set(results.adminBudget.data);
        }

        // Update budget history
        if (results.budgetHistory.success && results.budgetHistory.data) {
          this.budgetHistory.set(results.budgetHistory.data);
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
      {
        icon: 'users',
        label: 'Active Users',
        value: stats.totalActiveUsers || 0,
        trend: 0,
        route: '/admin/users'
      },
      {
        icon: 'events',
        label: 'Active Events',
        value: stats.activeEvents || 0,
        trend: 0,
        route: '/admin/events'
      },
      {
        icon: 'shopping-cart',
        label: 'Pending Redemptions',
        value: stats.pendingRedemptions || 0,
        trend: 0,
        route: '/admin/redemptions'
      },
      {
        icon: 'gift',
        label: 'Total Products',
        value: stats.activeProducts || stats.totalProducts || 0,
        trend: 0,
        route: '/admin/products'
      },
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
      { icon: 'users', label: 'Active Users', value: 0, trend: 0, route: '/admin/users' },
      { icon: 'events', label: 'Active Events', value: 0, trend: 0, route: '/admin/events' },
      { icon: 'shopping-cart', label: 'Pending Redemptions', value: 0, trend: 0, route: '/admin/redemptions' },
      { icon: 'gift', label: 'Total Products', value: 0, trend: 0, route: '/admin/products' },
    ]);
    this.recentActivities.set([]);
    this.chartData.set({
      labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun'],
      pointsAwarded: [0, 0, 0, 0, 0, 0],
      redemptions: [0, 0, 0, 0, 0, 0],
    });
  }

  private updateEventStatusSummary(eventsResponse: any): void {
    if (!eventsResponse.success || !eventsResponse.data) return;

    const events = Array.isArray(eventsResponse.data) ? eventsResponse.data : (eventsResponse.data as any).items || [];

    const summary = {
      draft: events.filter((e: EventDto) => e.status === 'Draft').length,
      upcoming: events.filter((e: EventDto) => e.status === 'Upcoming').length,
      active: events.filter((e: EventDto) => e.status === 'Active').length,
      completed: events.filter((e: EventDto) => e.status === 'Completed').length
    };

    this.eventStatusSummary.set(summary);
  }

  private updateRedemptionSummary(redemptionsResponse: any): void {
    if (!redemptionsResponse.success || !redemptionsResponse.data) return;

    const redemptions = Array.isArray(redemptionsResponse.data) ? redemptionsResponse.data : (redemptionsResponse.data as any).items || [];

    const summary = {
      pending: redemptions.filter((r: RedemptionDto) => r.status === 'Pending').length,
      approved: redemptions.filter((r: RedemptionDto) => r.status === 'Approved').length,
      rejected: redemptions.filter((r: RedemptionDto) => r.status === 'Rejected').length,
      delivered: redemptions.filter((r: RedemptionDto) => r.status === 'Delivered').length,
      cancelled: redemptions.filter((r: RedemptionDto) => r.status === 'Cancelled').length
    };

    this.redemptionSummary.set(summary);
  }

  private updateEventsNeedingAttention(eventsResponse: any): void {
    if (!eventsResponse.success || !eventsResponse.data) return;

    const events = Array.isArray(eventsResponse.data) ? eventsResponse.data : (eventsResponse.data as any).items || [];
    const now = new Date();
    const sevenDaysFromNow = new Date(now.getTime() + 7 * 24 * 60 * 60 * 1000);

    // Filter events that need attention:
    // 1. Completed events without winners (no points awarded yet)
    // 2. Upcoming events starting within 7 days
    // 3. Currently active events
    const needingAttention = events.filter((e: EventDto) => {
      if (e.status === 'Active') return true;

      if (e.status === 'Completed' && e.remainingPoints === e.totalPointsPool) {
        // Event completed but no winners assigned yet
        return true;
      }

      if (e.status === 'Upcoming') {
        const eventDate = new Date(e.eventDate);
        if (eventDate <= sevenDaysFromNow) {
          return true;
        }
      }

      return false;
    }).slice(0, 5); // Limit to top 5

    this.eventsNeedingAttention.set(needingAttention);
  }

  formatNumber(num: number | undefined | null): string {
    if (num === undefined || num === null) return '0';
    if (num >= 1000000) return (num / 1000000).toFixed(1) + 'M';
    if (num >= 1000) return (num / 1000).toFixed(1) + 'K';
    return num.toString();
  }

  getTotalPrizes(): number {
    return (this.newEvent.firstPlacePoints || 0) + 
           (this.newEvent.secondPlacePoints || 0) + 
           (this.newEvent.thirdPlacePoints || 0);
  }

  getRemainingPool(): number {
    return (this.newEvent.pointsPool || 0) - this.getTotalPrizes();
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
      eventEndDate: '',
      maxParticipants: 0,
      pointsPool: 0,
      firstPlacePoints: 0,
      secondPlacePoints: 0,
      thirdPlacePoints: 0,
      registrationStartDate: '',
      registrationEndDate: '',
      location: '',
      virtualLink: '',
      imageUrl: ''
    };
    this.eventModalValidationErrors.set([]);
    this.showEventModal.set(true);
  }

  closeEventModal(): void {
    this.showEventModal.set(false);
  }

  validateEventModal(): boolean {
    const errors: string[] = [];

    // Event name: 3-200 chars
    const name = (this.newEvent.name || '').trim();
    if (!name) {
      errors.push('Event name is required');
    } else if (name.length < 3 || name.length > 200) {
      errors.push('Event name must be between 3 and 200 characters');
    }

    // Description: max 1000 chars
    const description = (this.newEvent.description || '').trim();
    if (description && description.length > 1000) {
      errors.push('Description cannot exceed 1000 characters');
    }

    // Event date required
    if (!this.newEvent.eventDate) {
      errors.push('Event start date is required');
    }

    // Points pool: 1-1,000,000
    if (!this.newEvent.pointsPool || this.newEvent.pointsPool < 1) {
      errors.push('Points pool must be at least 1');
    } else if (this.newEvent.pointsPool > 1000000) {
      errors.push('Points pool cannot exceed 1,000,000');
    }

    // Prize distribution validation (if any prize is set)
    const first = this.newEvent.firstPlacePoints || 0;
    const second = this.newEvent.secondPlacePoints || 0;
    const third = this.newEvent.thirdPlacePoints || 0;

    if (first > 0 || second > 0 || third > 0) {
      if (first > 0 && second > 0 && first <= second) {
        errors.push('1st place points must be greater than 2nd place points');
      }
      if (second > 0 && third > 0 && second <= third) {
        errors.push('2nd place points must be greater than 3rd place points');
      }
      if (first > 0 && third > 0 && first <= third) {
        errors.push('1st place points must be greater than 3rd place points');
      }
    }

    this.eventModalValidationErrors.set(errors);
    return errors.length === 0;
  }

  validateProductModal(): boolean {
    const errors: string[] = [];

    // Product name: 2-200 chars
    const name = (this.newProduct.name || '').trim();
    if (!name) {
      errors.push('Product name is required');
    } else if (name.length < 2 || name.length > 200) {
      errors.push('Product name must be between 2 and 200 characters');
    }

    // Description: max 1000 chars
    const description = (this.newProduct.description || '').trim();
    if (description && description.length > 1000) {
      errors.push('Description cannot exceed 1000 characters');
    }

    // Category required
    if (!this.newProduct.categoryId) {
      errors.push('Category is required');
    }

    // Points price: > 0
    if (!this.newProduct.pointsPrice || this.newProduct.pointsPrice <= 0) {
      errors.push('Points price must be greater than 0');
    }

    // Stock: >= 0
    if (this.newProduct.stock !== undefined && this.newProduct.stock < 0) {
      errors.push('Stock quantity cannot be negative');
    }

    // Image URL validation (if provided)
    const imageUrl = (this.newProduct.imageUrl || '').trim();
    if (imageUrl) {
      if (imageUrl.length > 500) {
        errors.push('Image URL cannot exceed 500 characters');
      }
      if (!imageUrl.startsWith('http://') && !imageUrl.startsWith('https://')) {
        errors.push('Image URL must start with http:// or https://');
      }
    }

    this.productModalValidationErrors.set(errors);
    return errors.length === 0;
  }

  createEvent(): void {
    if (!this.validateEventModal()) {
      return;
    }

    const event = this.newEvent;
    const eventData: CreateEventDto = {
      name: event.name.trim(),
      description: event.description?.trim() || '',
      eventDate: new Date(event.eventDate).toISOString(),
      eventEndDate: event.eventEndDate ? new Date(event.eventEndDate).toISOString() : undefined,
      maxParticipants: event.maxParticipants || undefined,
      totalPointsPool: event.pointsPool,
      firstPlacePoints: event.firstPlacePoints || undefined,
      secondPlacePoints: event.secondPlacePoints || undefined,
      thirdPlacePoints: event.thirdPlacePoints || undefined,
      registrationStartDate: event.registrationStartDate ? new Date(event.registrationStartDate).toISOString() : undefined,
      registrationEndDate: event.registrationEndDate ? new Date(event.registrationEndDate).toISOString() : undefined,
      location: event.location?.trim() || undefined,
      virtualLink: event.virtualLink?.trim() || undefined,
      bannerImageUrl: event.imageUrl?.trim() || undefined
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
        this.toast.showValidationErrors(error);
      }
    });
  }

  openProductModal(): void {
    this.newProduct = {
      name: '',
      description: '',
      categoryId: '',
      pointsPrice: 1,
      stock: 0,
      imageUrl: ''
    };
    this.productModalValidationErrors.set([]);
    this.showProductModal.set(true);
  }

  closeProductModal(): void {
    this.showProductModal.set(false);
  }

  createProduct(): void {
    if (!this.validateProductModal()) {
      return;
    }

    const product = this.newProduct;
    const productData: CreateProductDto = {
      name: product.name.trim(),
      description: product.description?.trim() || '',
      categoryId: product.categoryId || undefined,
      pointsPrice: product.pointsPrice,
      stockQuantity: product.stock,
      imageUrl: product.imageUrl?.trim() || 'https://via.placeholder.com/150'
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
        this.toast.showValidationErrors(error);
      }
    });
  }

  // Budget Management Methods
  openBudgetModal(mode: 'create' | 'edit' = 'create'): void {
    this.budgetModalMode.set(mode);
    this.budgetValidationErrors.set([]);

    if (mode === 'edit' && this.adminBudget()) {
      const budget = this.adminBudget()!;
      this.budgetForm.set({
        budgetLimit: budget.budgetLimit,
        isHardLimit: budget.isHardLimit,
        warningThreshold: budget.warningThreshold
      });
    } else {
      this.budgetForm.set({
        budgetLimit: 50000,
        isHardLimit: false,
        warningThreshold: 80
      });
    }

    this.showBudgetModal.set(true);
  }

  closeBudgetModal(): void {
    this.showBudgetModal.set(false);
  }

  validateBudgetForm(): boolean {
    const errors: string[] = [];
    const form = this.budgetForm();

    if (!form.budgetLimit || form.budgetLimit <= 0) {
      errors.push('Budget limit must be greater than 0');
    }

    if (form.budgetLimit > 10000000) {
      errors.push('Budget limit cannot exceed 10,000,000 points');
    }

    if (form.warningThreshold < 0 || form.warningThreshold > 100) {
      errors.push('Warning threshold must be between 0 and 100');
    }

    this.budgetValidationErrors.set(errors);
    return errors.length === 0;
  }

  saveBudget(): void {
    if (!this.validateBudgetForm()) {
      return;
    }

    const form = this.budgetForm();
    const request: SetBudgetRequest = {
      budgetLimit: form.budgetLimit,
      isHardLimit: form.isHardLimit,
      warningThreshold: form.warningThreshold
    };

    this.adminService.setBudget(request).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.adminBudget.set(response.data);
          this.toast.success('Budget updated successfully!');
          this.closeBudgetModal();
          // Refresh budget history
          this.adminService.getBudgetHistory(6).subscribe({
            next: (historyResponse) => {
              if (historyResponse.success && historyResponse.data) {
                this.budgetHistory.set(historyResponse.data);
              }
            }
          });
        } else {
          this.toast.error(response.message || 'Failed to save budget');
        }
      },
      error: (error) => {
        console.error('Error saving budget:', error);
        this.toast.showValidationErrors(error);
      }
    });
  }

  updateBudgetFormField(field: string, value: any): void {
    this.budgetForm.update(form => ({
      ...form,
      [field]: value
    }));
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
