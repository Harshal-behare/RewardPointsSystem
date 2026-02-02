import { Component, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';
import { PointsService, PointsAccountDto, PointsTransactionDto } from '../../../core/services/points.service';
import { EventService, EventDto } from '../../../core/services/event.service';
import { ProductService, ProductDto } from '../../../core/services/product.service';
import { RedemptionService, RedemptionDto } from '../../../core/services/redemption.service';
import { AuthService } from '../../../auth/auth.service';
import { ToastService } from '../../../core/services/toast.service';
import { IconComponent } from '../../../shared/components/icon/icon.component';
import { NgApexchartsModule, ApexNonAxisChartSeries, ApexChart, ApexResponsive, ApexLegend, ApexDataLabels, ApexPlotOptions, ApexFill, ApexStroke, ApexTooltip } from 'ng-apexcharts';

interface PointsBalance {
  total: number;
  available: number;
  pending: number;
  redeemed: number;
}

interface Event {
  id: string;
  name: string;
  date: string;
  location: string;
  points: number;
  image: string;
  status: 'Upcoming' | 'Completed';  // Employees only see Upcoming and Completed (not Draft)
  registered: boolean;
}

interface Product {
  id: string;
  name: string;
  points: number;
  image: string;
  stock: number;
  category: string;
}

interface Transaction {
  id: string;
  type: 'earned' | 'redeemed' | 'pending';
  description: string;
  points: number;
  date: string;
  status: string;
  source?: string;
  eventName?: string;
  eventRank?: number;
}

@Component({
  selector: 'app-employee-dashboard',
  standalone: true,
  imports: [IconComponent, NgApexchartsModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class EmployeeDashboardComponent implements OnInit {
  pointsBalance = signal<PointsBalance>({
    total: 0,
    available: 0,
    pending: 0,
    redeemed: 0
  });

  redemptionsSummary = signal({
    pending: 0,
    approved: 0,
    delivered: 0
  });

  // Redemptions Pie Chart Configuration
  redemptionsChartSeries: ApexNonAxisChartSeries = [0, 0, 0];
  redemptionsChartOptions: {
    chart: ApexChart;
    labels: string[];
    colors: string[];
    legend: ApexLegend;
    dataLabels: ApexDataLabels;
    plotOptions: ApexPlotOptions;
    fill: ApexFill;
    stroke: ApexStroke;
    tooltip: ApexTooltip;
    responsive: ApexResponsive[];
  } = {
    chart: {
      type: 'donut',
      height: 220,
      toolbar: { show: false }
    },
    labels: ['Pending', 'Approved', 'Delivered'],
    colors: ['#F39C12', '#27AE60', '#3498DB'],
    legend: {
      position: 'bottom',
      horizontalAlign: 'center',
      fontSize: '13px',
      markers: { strokeWidth: 0, fillColors: ['#F39C12', '#27AE60', '#3498DB'] },
      itemMargin: { horizontal: 10, vertical: 5 }
    },
    dataLabels: {
      enabled: true,
      formatter: (val: number, opts: any) => opts.w.globals.series[opts.seriesIndex],
      style: {
        fontSize: '12px',
        fontWeight: 600
      },
      dropShadow: { enabled: false }
    },
    plotOptions: {
      pie: {
        donut: {
          size: '55%',
          labels: {
            show: true,
            name: { fontSize: '14px', fontWeight: 600 },
            value: { fontSize: '20px', fontWeight: 700, color: '#2C3E50' },
            total: {
              show: true,
              label: 'Total',
              fontSize: '14px',
              color: '#7A7A7A',
              formatter: (w: any) => w.globals.seriesTotals.reduce((a: number, b: number) => a + b, 0)
            }
          }
        }
      }
    },
    fill: {
      type: 'solid'
    },
    stroke: {
      width: 2,
      colors: ['#fff']
    },
    tooltip: {
      y: {
        formatter: (val: number) => `${val} redemption${val !== 1 ? 's' : ''}`
      }
    },
    responsive: [
      {
        breakpoint: 480,
        options: {
          chart: { height: 200 },
          legend: { position: 'bottom' }
        }
      }
    ]
  };

  // Events Participation Chart Configuration
  eventsParticipationChartSeries: ApexNonAxisChartSeries = [0, 0];
  eventsParticipationChartOptions: {
    chart: ApexChart;
    labels: string[];
    colors: string[];
    legend: ApexLegend;
    dataLabels: ApexDataLabels;
    plotOptions: ApexPlotOptions;
    fill: ApexFill;
    stroke: ApexStroke;
    tooltip: ApexTooltip;
    responsive: ApexResponsive[];
  } = {
    chart: {
      type: 'donut',
      height: 220,
      toolbar: { show: false }
    },
    labels: ['Registered', 'Completed'],
    colors: ['#9B59B6', '#27AE60'],
    legend: {
      position: 'bottom',
      horizontalAlign: 'center',
      fontSize: '13px',
      markers: { strokeWidth: 0, fillColors: ['#9B59B6', '#27AE60'] },
      itemMargin: { horizontal: 10, vertical: 5 }
    },
    dataLabels: {
      enabled: true,
      formatter: (val: number, opts: any) => opts.w.globals.series[opts.seriesIndex],
      style: {
        fontSize: '12px',
        fontWeight: 600
      },
      dropShadow: { enabled: false }
    },
    plotOptions: {
      pie: {
        donut: {
          size: '55%',
          labels: {
            show: true,
            name: { fontSize: '14px', fontWeight: 600 },
            value: { fontSize: '20px', fontWeight: 700, color: '#2C3E50' },
            total: {
              show: true,
              label: 'Total',
              fontSize: '14px',
              color: '#7A7A7A',
              formatter: (w: any) => w.globals.seriesTotals.reduce((a: number, b: number) => a + b, 0)
            }
          }
        }
      }
    },
    fill: {
      type: 'solid'
    },
    stroke: {
      width: 2,
      colors: ['#fff']
    },
    tooltip: {
      y: {
        formatter: (val: number) => `${val} event${val !== 1 ? 's' : ''}`
      }
    },
    responsive: [
      {
        breakpoint: 480,
        options: {
          chart: { height: 200 },
          legend: { position: 'bottom' }
        }
      }
    ]
  };

  eventParticipationStats = signal({
    registered: 0,
    completed: 0
  });

  upcomingEvents = signal<Event[]>([]);
  featuredProducts = signal<Product[]>([]);
  recentTransactions = signal<Transaction[]>([]);
  isLoading = signal(true);
  currentUserId: string = '';
  userName: string = 'User';

  constructor(
    private router: Router,
    private pointsService: PointsService,
    private eventService: EventService,
    private productService: ProductService,
    private redemptionService: RedemptionService,
    private authService: AuthService,
    private toast: ToastService
  ) {
    // Get current user ID and name from JWT token
    const payload = this.authService.getDecodedToken();
    if (payload) {
      this.currentUserId = payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] ||
                          payload.sub ||
                          payload.userId || '';

      // Get user's first name from token
      const firstName = payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname'] ||
                       payload.given_name ||
                       payload.firstName ||
                       payload.FirstName || '';

      this.userName = firstName || 'User';
    }
  }

  ngOnInit(): void {
    this.loadDashboardData();
  }

  loadDashboardData(): void {
    this.isLoading.set(true);
    
    if (!this.currentUserId) {
      this.toast.error('User not authenticated');
      this.isLoading.set(false);
      return;
    }

    // Load points balance (including pending from database)
    this.pointsService.getPointsAccount(this.currentUserId).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          const account = response.data;
          // Update points balance with all values from API (including pending from DB)
          this.pointsBalance.update(current => ({
            ...current,
            total: account.totalEarned,
            available: account.currentBalance,
            pending: account.pendingPoints || 0,
            redeemed: account.totalRedeemed
          }));
        }
      },
      error: (error) => {
        console.error('Error loading points balance:', error);
      }
    });

    // Note: Pending points is now loaded from the account API (stored in database)

    // Load recent transactions - top 5 only
    this.pointsService.getUserTransactions(this.currentUserId, 1, 5).subscribe({
      next: (response) => {
        console.log('Dashboard transactions API response:', response);
        
        if (response.success && response.data) {
          // Handle different response structures
          let transactions: PointsTransactionDto[] = [];
          
          if (Array.isArray(response.data)) {
            transactions = response.data;
          } else if ((response.data as any).items && Array.isArray((response.data as any).items)) {
            transactions = (response.data as any).items;
          }
          
          console.log('Dashboard extracted transactions:', transactions);
          
          this.recentTransactions.set(transactions.slice(0, 5).map((t: PointsTransactionDto) => {
            const points = t.userPoints ?? t.points ?? 0;
            const dateStr = t.timestamp ?? t.createdAt ?? new Date().toISOString();
            // Backend returns 'Earned' not 'Credit' for TransactionType
            const isEarned = t.transactionType === 'Earned';
            
            // Determine the source for display
            let source = 'Direct';
            if (t.transactionSource) {
              source = t.transactionSource;
            } else if (t.eventName || t.eventId) {
              source = 'Event';
            } else if (t.redemptionId) {
              source = 'Redemption';
            }
            
            // Clean up description - remove IDs and technical details
            let cleanDescription = this.cleanDescription(t.description || (isEarned ? 'Points Earned' : 'Points Redeemed'));
            
            return {
              id: t.id,
              type: isEarned ? 'earned' as const : 'redeemed' as const,
              description: cleanDescription,
              points: isEarned ? points : -points,
              date: new Date(dateStr).toISOString().split('T')[0],
              status: 'Completed',
              source: source,
              eventName: t.eventName,
              eventRank: t.eventRank
            };
          }));
        }
      },
      error: (error) => {
        console.error('Error loading transactions:', error);
      }
    });

    // Load upcoming events - only show Upcoming events to employees (not Draft or Completed)
    this.eventService.getAllEvents().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          const events = Array.isArray(response.data) ? response.data : 
                        (response.data as any).items || [];
          const upcomingEvents = events
            .filter((e: EventDto) => e.status === 'Upcoming')
            .slice(0, 3)
            .map((e: EventDto) => ({
              id: e.id,
              name: e.name,
              date: new Date(e.eventDate).toISOString().split('T')[0],
              location: 'Event Location',
              points: e.totalPointsPool,
              image: 'https://images.unsplash.com/photo-1540575467063-178a50c2df87?w=400',
              status: 'Upcoming' as const,
              registered: false
            }));
          this.upcomingEvents.set(upcomingEvents);
          
          // Check registration status for each event
          this.checkEventRegistrations();
          
          // Load event participation stats for pie chart
          this.loadEventParticipationStats(events);
        }
      },
      error: (error) => {
        console.error('Error loading events:', error);
      }
    });

    // Load my redemptions summary
    this.redemptionService.getMyRedemptions().subscribe({
      next: (response) => {
        console.log('Dashboard redemptions API response:', response);
        if (response.success && response.data) {
          // Handle both array and paginated response formats
          const redemptions = Array.isArray(response.data) ? response.data :
                             (response.data as any).items || [];
          console.log('Redemptions data:', redemptions);
          // Use case-insensitive comparison for status
          const pending = redemptions.filter((r: RedemptionDto) => r.status?.toLowerCase() === 'pending').length;
          const approved = redemptions.filter((r: RedemptionDto) => r.status?.toLowerCase() === 'approved').length;
          const delivered = redemptions.filter((r: RedemptionDto) => r.status?.toLowerCase() === 'delivered').length;
          
          console.log('Redemption counts:', { pending, approved, delivered });
          
          this.redemptionsSummary.set({
            pending,
            approved,
            delivered
          });
          
          // Update pie chart data
          this.redemptionsChartSeries = [pending, approved, delivered];
        }
      },
      error: (error) => {
        console.error('Error loading redemptions:', error);
      }
    });

    // Load featured products
    this.productService.getProducts().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          const products = Array.isArray(response.data) ? response.data :
                          (response.data as any).items || [];
          const featured = products
            .filter((p: ProductDto) => p.isActive && p.isInStock)
            .slice(0, 4)
            .map((p: ProductDto) => ({
              id: p.id,
              name: p.name,
              points: p.currentPointsCost,
              image: p.imageUrl || 'https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=400',
              stock: p.stockQuantity || 0,
              category: p.categoryName || 'General'
            }));
          this.featuredProducts.set(featured);
        }
      },
      error: (error) => {
        console.error('Error loading products:', error);
      },
      complete: () => {
        this.isLoading.set(false);
      }
    });
  }

  private checkEventRegistrations(): void {
    if (!this.currentUserId) return;
    
    const events = this.upcomingEvents();
    events.forEach(event => {
      this.eventService.getEventParticipants(event.id).subscribe({
        next: (response) => {
          if (response.success && response.data) {
            const isRegistered = response.data.some((p: any) => p.userId === this.currentUserId);
            if (isRegistered) {
              // Update the event's registered status
              const updatedEvents = this.upcomingEvents().map(e => 
                e.id === event.id ? { ...e, registered: true } : e
              );
              this.upcomingEvents.set(updatedEvents);
            }
          }
        },
        error: (error) => {
          console.error(`Error checking registration for event ${event.id}:`, error);
        }
      });
    });
  }

  private loadEventParticipationStats(events: EventDto[]): void {
    if (!this.currentUserId) return;
    
    let registeredCount = 0;
    let completedCount = 0;
    let processed = 0;
    const totalEvents = events.length;
    
    if (totalEvents === 0) {
      this.eventParticipationStats.set({ registered: 0, completed: 0 });
      this.eventsParticipationChartSeries = [0, 0];
      return;
    }
    
    events.forEach(event => {
      this.eventService.getEventParticipants(event.id).subscribe({
        next: (response) => {
          if (response.success && response.data) {
            const isParticipant = response.data.some((p: any) => p.userId === this.currentUserId);
            if (isParticipant) {
              if (event.status === 'Completed') {
                completedCount++;
              } else if (event.status === 'Upcoming' || event.status === 'Active') {
                registeredCount++;
              }
            }
          }
          processed++;
          
          // Update stats when all events are processed
          if (processed === totalEvents) {
            this.eventParticipationStats.set({
              registered: registeredCount,
              completed: completedCount
            });
            this.eventsParticipationChartSeries = [registeredCount, completedCount];
          }
        },
        error: () => {
          processed++;
          if (processed === totalEvents) {
            this.eventParticipationStats.set({
              registered: registeredCount,
              completed: completedCount
            });
            this.eventsParticipationChartSeries = [registeredCount, completedCount];
          }
        }
      });
    });
  }

  navigateToEvents(): void {
    this.router.navigate(['/employee/events']);
  }

  navigateToProducts(): void {
    this.router.navigate(['/employee/products']);
  }

  navigateToAccount(): void {
    this.router.navigate(['/employee/account']);
  }

  registerForEvent(event: Event): void {
    if (event.registered) {
      this.toast.info('You are already registered for this event');
      return;
    }
    
    this.eventService.registerParticipant(event.id, { eventId: event.id, userId: this.currentUserId }).subscribe({
      next: (response) => {
        if (response.success) {
          // Update the event locally
          const updatedEvents = this.upcomingEvents().map(e =>
            e.id === event.id ? { ...e, registered: true } : e
          );
          this.upcomingEvents.set(updatedEvents);
          this.toast.success(`Successfully registered for ${event.name}!`);
        } else {
          this.toast.error(response.message || 'Failed to register for event');
        }
      },
      error: (error) => {
        console.error('Error registering for event:', error);
        this.toast.error('Failed to register for event');
      }
    });
  }

  redeemProduct(product: Product): void {
    const balance = this.pointsBalance();
    if (balance.available >= product.points) {
      this.router.navigate(['/employee/products']);
    } else {
      this.toast.warning('Insufficient points!');
    }
  }

  // Helper method to clean up descriptions by removing IDs and technical details
  private cleanDescription(description: string): string {
    if (!description) return 'Points Transaction';
    
    // Remove Redemption ID patterns like "Redemption ID: a7eb3f21-99b6-4abc-af1d-b01d6477e553"
    let cleaned = description.replace(/\s*-?\s*Redemption ID:\s*[a-f0-9-]+/gi, '');
    
    // Remove UUID patterns standalone
    cleaned = cleaned.replace(/[a-f0-9]{8}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{12}/gi, '');
    
    // Clean up any double spaces or trailing dashes
    cleaned = cleaned.replace(/\s+-\s*$/g, '').replace(/\s+/g, ' ').trim();
    
    // If description becomes empty or too short after cleaning, provide a default
    if (cleaned.length < 3) {
      cleaned = 'Points Transaction';
    }
    
    return cleaned;
  }
}
