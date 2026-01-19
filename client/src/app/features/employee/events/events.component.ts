import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { EventService } from '../../../core/services/event.service';
import { AuthService } from '../../../auth/auth.service';
import { ToastService } from '../../../core/services/toast.service';

interface DisplayEvent {
  id: string;
  name: string;
  description: string;
  eventDate: string;
  status: 'Upcoming' | 'Active' | 'Completed' | 'Cancelled';
  pointsPool: number;
  participantCount: number;
  maxParticipants?: number;
  imageUrl?: string;
  registered: boolean;
}

@Component({
  selector: 'app-employee-events',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './events.component.html',
  styleUrl: './events.component.scss'
})
export class EmployeeEventsComponent implements OnInit {
  events = signal<DisplayEvent[]>([]);
  filteredEvents = signal<DisplayEvent[]>([]);
  selectedFilter = signal<'all' | 'Upcoming' | 'Active' | 'Completed'>('all');
  searchQuery = signal('');
  selectedEvent = signal<DisplayEvent | null>(null);
  showDetailsModal = signal(false);
  currentUserId = signal<string>('');

  constructor(
    private eventService: EventService,
    private authService: AuthService,
    private toast: ToastService
  ) {}

  ngOnInit(): void {
    // Get current user ID from token or auth service
    const token = this.authService.getToken();
    if (token) {
      try {
        const payload = JSON.parse(atob(token.split('.')[1]));
        const userId = payload.sub || payload.userId || payload.nameid;
        if (userId) {
          this.currentUserId.set(userId);
        }
      } catch (error) {
        console.error('Error parsing token:', error);
      }
    }
    
    this.loadEvents();
  }

  loadEvents(): void {
    this.eventService.getAllEvents().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          // Map events and check registration status
          const mappedEvents = response.data.map((event: any) => this.mapEventToDisplay(event));
          this.events.set(mappedEvents);
          
          // Load registration status for each event
          this.loadRegistrationStatus();
        } else {
          this.toast.error('Failed to load events');
        }
        this.applyFilters();
      },
      error: (error) => {
        console.error('Error loading events:', error);
        this.toast.error('Failed to load events');
        this.applyFilters();
      }
    });
  }

  private mapEventToDisplay(event: any): DisplayEvent {
    return {
      id: event.id || event.eventId,
      name: event.name || event.title || '',
      description: event.description || '',
      eventDate: event.eventDate || event.startDate || '',
      status: this.normalizeStatus(event.status),
      pointsPool: event.totalPointsPool || event.pointsPool || 0,
      participantCount: event.participantsCount || event.participantCount || 0,
      maxParticipants: event.maxParticipants,
      imageUrl: event.imageUrl || event.bannerImageUrl,
      registered: false
    };
  }

  private normalizeStatus(status: string): 'Upcoming' | 'Active' | 'Completed' | 'Cancelled' {
    const statusLower = (status || '').toLowerCase();
    if (statusLower === 'upcoming' || statusLower === 'published') return 'Upcoming';
    if (statusLower === 'active' || statusLower === 'inprogress') return 'Active';
    if (statusLower === 'completed') return 'Completed';
    if (statusLower === 'cancelled') return 'Cancelled';
    return 'Upcoming';
  }

  private loadRegistrationStatus(): void {
    // For each event, check if current user is registered
    const userId = this.currentUserId();
    if (!userId) return;

    this.events().forEach((event) => {
      this.eventService.getEventParticipants(event.id).subscribe({
        next: (response) => {
          if (response.success && response.data) {
            const isRegistered = response.data.some((p: any) => p.userId === userId);
            
            // Update the event in the array
            const updatedEvents = this.events().map(e => 
              e.id === event.id ? { ...e, registered: isRegistered } : e
            );
            this.events.set(updatedEvents);
            this.applyFilters();
          }
        },
        error: (error) => {
          console.error(`Error loading participants for event ${event.id}:`, error);
        }
      });
    });
  }

  applyFilters(): void {
    let filtered = [...this.events()];

    // Filter by status
    if (this.selectedFilter() !== 'all') {
      filtered = filtered.filter(event => event.status === this.selectedFilter());
    }

    // Filter by search query
    if (this.searchQuery().trim()) {
      const query = this.searchQuery().toLowerCase();
      filtered = filtered.filter(event =>
        event.name.toLowerCase().includes(query) ||
        event.description.toLowerCase().includes(query)
      );
    }

    this.filteredEvents.set(filtered);
  }

  onFilterChange(filter: 'all' | 'Upcoming' | 'Active' | 'Completed'): void {
    this.selectedFilter.set(filter);
    this.applyFilters();
  }

  onSearchChange(): void {
    this.applyFilters();
  }

  registerForEvent(event: DisplayEvent): void {
    const userId = this.currentUserId();
    if (!userId) {
      this.toast.error('User not logged in');
      return;
    }

    this.eventService.registerParticipant(event.id, { eventId: event.id, userId }).subscribe({
      next: (response) => {
        if (response.success) {
          // Update the event locally
          const updatedEvents = this.events().map(e =>
            e.id === event.id ? { ...e, registered: true, participantCount: e.participantCount + 1 } : e
          );
          this.events.set(updatedEvents);
          this.applyFilters();
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

  unregisterFromEvent(event: DisplayEvent): void {
    const userId = this.currentUserId();
    if (!userId) {
      this.toast.error('User not logged in');
      return;
    }

    if (confirm(`Are you sure you want to unregister from "${event.name}"?`)) {
      // Note: We need to add an unregister endpoint in the backend
      this.toast.warning('Unregister functionality coming soon');
      
      // For now, just update locally
      const updatedEvents = this.events().map(e =>
        e.id === event.id ? { ...e, registered: false, participantCount: Math.max(0, e.participantCount - 1) } : e
      );
      this.events.set(updatedEvents);
      this.applyFilters();
    }
  }

  openDetailsModal(event: DisplayEvent): void {
    this.selectedEvent.set(event);
    this.showDetailsModal.set(true);
  }

  closeDetailsModal(): void {
    this.showDetailsModal.set(false);
    this.selectedEvent.set(null);
  }

  getStatusBadgeClass(status: string): string {
    const classes: { [key: string]: string } = {
      'Upcoming': 'badge-upcoming',
      'Active': 'badge-ongoing',
      'Completed': 'badge-completed',
      'Cancelled': 'badge-cancelled'
    };
    return classes[status] || '';
  }

  getStatusLabel(status: string): string {
    return status || 'Unknown';
  }

  getAvailabilityPercentage(event: DisplayEvent): number {
    if (!event.maxParticipants) return 0;
    return (event.participantCount / event.maxParticipants) * 100;
  }

  isEventFull(event: DisplayEvent): boolean {
    if (!event.maxParticipants) return false;
    return event.participantCount >= event.maxParticipants;
  }

  refreshData(): void {
    this.loadEvents();
    this.toast.info('Events refreshed!');
  }
}
