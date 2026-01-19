import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { PointsService, PointsAccountDto, PointsTransactionDto } from '../../../core/services/points.service';
import { EventService, EventDto } from '../../../core/services/event.service';
import { ProductService, ProductDto } from '../../../core/services/product.service';
import { AuthService } from '../../../auth/auth.service';
import { ToastService } from '../../../core/services/toast.service';

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
  status: 'Upcoming' | 'Active' | 'Completed';
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
}

@Component({
  selector: 'app-employee-dashboard',
  standalone: true,
  imports: [CommonModule],
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

  upcomingEvents = signal<Event[]>([]);
  featuredProducts = signal<Product[]>([]);
  recentTransactions = signal<Transaction[]>([]);
  isLoading = signal(true);
  currentUserId: string = '';

  constructor(
    private router: Router,
    private pointsService: PointsService,
    private eventService: EventService,
    private productService: ProductService,
    private authService: AuthService,
    private toast: ToastService
  ) {
    // Get current user ID from JWT token
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
    this.loadDashboardData();
  }

  loadDashboardData(): void {
    this.isLoading.set(true);
    
    if (!this.currentUserId) {
      this.toast.error('User not authenticated');
      this.isLoading.set(false);
      return;
    }

    // Load points balance
    this.pointsService.getPointsAccount(this.currentUserId).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          const account = response.data;
          this.pointsBalance.set({
            total: account.totalEarned,
            available: account.currentBalance,
            pending: 0, // Not available from API yet
            redeemed: account.totalRedeemed
          });
        }
      },
      error: (error) => {
        console.error('Error loading points balance:', error);
      }
    });

    // Load recent transactions
    this.pointsService.getUserTransactions(this.currentUserId, 1, 5).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          const transactions = Array.isArray(response.data) ? response.data : 
                              (response.data as any).items || [];
          this.recentTransactions.set(transactions.map((t: PointsTransactionDto) => ({
            id: t.id,
            type: t.transactionType === 'Credit' ? 'earned' as const : 'redeemed' as const,
            description: t.description,
            points: t.transactionType === 'Credit' ? t.points : -t.points,
            date: new Date(t.createdAt).toISOString().split('T')[0],
            status: 'Completed'
          })));
        }
      },
      error: (error) => {
        console.error('Error loading transactions:', error);
      }
    });

    // Load upcoming events
    this.eventService.getAllEvents().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          const events = Array.isArray(response.data) ? response.data : 
                        (response.data as any).items || [];
          const upcomingEvents = events
            .filter((e: EventDto) => e.status === 'Published' || e.status === 'Draft')
            .slice(0, 3)
            .map((e: EventDto) => ({
              id: e.id,
              name: e.name,
              date: new Date(e.eventDate).toISOString().split('T')[0],
              location: 'Event Location',
              points: e.totalPointsPool,
              image: 'https://images.unsplash.com/photo-1540575467063-178a50c2df87?w=400',
              status: e.status === 'Published' ? 'Upcoming' as const : e.status === 'Draft' ? 'Active' as const : 'Completed' as const,
              registered: false
            }));
          this.upcomingEvents.set(upcomingEvents);
        }
      },
      error: (error) => {
        console.error('Error loading events:', error);
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
    this.router.navigate(['/employee/events']);
  }

  redeemProduct(product: Product): void {
    const balance = this.pointsBalance();
    if (balance.available >= product.points) {
      this.router.navigate(['/employee/products']);
    } else {
      this.toast.warning('Insufficient points!');
    }
  }
}
