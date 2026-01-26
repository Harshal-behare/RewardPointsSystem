import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { PointsService, PointsAccountDto, PointsTransactionDto } from '../../../core/services/points.service';
import { EventService, EventDto } from '../../../core/services/event.service';
import { ProductService, ProductDto } from '../../../core/services/product.service';
import { RedemptionService, RedemptionDto } from '../../../core/services/redemption.service';
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
    private redemptionService: RedemptionService,
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
          // Update points balance, pending will be updated by redemption call
          this.pointsBalance.update(current => ({
            ...current,
            total: account.totalEarned,
            available: account.currentBalance,
            redeemed: account.totalRedeemed
          }));
        }
      },
      error: (error) => {
        console.error('Error loading points balance:', error);
      }
    });

    // Load user's redemptions to calculate pending points
    this.redemptionService.getMyRedemptions().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          const redemptions = Array.isArray(response.data) ? response.data : 
                             (response.data as any).items || [];
          
          // Calculate pending points (Pending or Approved status = not yet delivered)
          const pendingPointsValue = redemptions
            .filter((r: RedemptionDto) => {
              const status = (r.status || '').toLowerCase();
              return status === 'pending' || status === 'approved';
            })
            .reduce((sum: number, r: RedemptionDto) => sum + r.pointsSpent, 0);
          
          this.pointsBalance.update(current => ({
            ...current,
            pending: pendingPointsValue
          }));
        }
      },
      error: (error) => {
        console.error('Error loading redemptions for pending points:', error);
      }
    });

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
