import { Component, OnInit, signal, computed } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { EventService, EventWinnerDto } from '../../../core/services/event.service';
import { AuthService } from '../../../auth/auth.service';
import { ToastService } from '../../../core/services/toast.service';

interface DisplayEvent {
  id: string;
  name: string;
  description: string;
  eventDate: string;
  eventEndDate?: string;
  status: 'Upcoming' | 'Active' | 'Completed';
  pointsPool: number;
  remainingPoints: number;
  participantCount: number;
  maxParticipants?: number;
  registrationStartDate?: string;
  registrationEndDate?: string;
  location?: string;
  virtualLink?: string;
  imageUrl?: string;
  registered: boolean;
  
  // Prize distribution
  firstPlacePoints?: number;
  secondPlacePoints?: number;
  thirdPlacePoints?: number;
  
  // Winners (for completed events)
  winners?: EventWinnerDto[];
}

@Component({
  selector: 'app-employee-events',
  standalone: true,
  imports: [FormsModule, CommonModule],
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
  isLoading = signal(false);

  // Computed statistics
  stats = computed(() => {
    const allEvents = this.events();
    return {
      total: allEvents.length,
      upcoming: allEvents.filter(e => e.status === 'Upcoming').length,
      active: allEvents.filter(e => e.status === 'Active').length,
      completed: allEvents.filter(e => e.status === 'Completed').length,
      registered: allEvents.filter(e => e.registered).length
    };
  });

  constructor(
    private eventService: EventService,
    private authService: AuthService,
    private toast: ToastService
  ) {}

  ngOnInit(): void {
    // Get current user ID from token
    const token = this.authService.getToken();
    if (token) {
      try {
        const payload = JSON.parse(atob(token.split('.')[1]));
        const userId = payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] || 
                      payload.sub || 
                      payload.userId || 
                      payload.nameid || '';
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
    this.isLoading.set(true);
    this.eventService.getAllEvents().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          // Filter out Draft events - employees should only see Upcoming, Active, and Completed
          const visibleEvents = response.data.filter((event: any) => {
            const status = (event.status || '').toLowerCase();
            return status !== 'draft';
          });
          
          const mappedEvents = visibleEvents.map((event: any) => this.mapEventToDisplay(event));
          this.events.set(mappedEvents);
          
          // Load registration status for each event
          this.loadRegistrationStatus();
        } else {
          this.toast.error('Failed to load events');
        }
        this.applyFilters();
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading events:', error);
        this.toast.error('Failed to load events');
        this.applyFilters();
        this.isLoading.set(false);
      }
    });
  }

  private mapEventToDisplay(event: any): DisplayEvent {
    return {
      id: event.id || event.eventId,
      name: event.name || event.title || '',
      description: event.description || '',
      eventDate: event.eventDate || event.startDate || '',
      eventEndDate: event.eventEndDate,
      status: this.normalizeStatus(event.status),
      pointsPool: event.totalPointsPool || event.pointsPool || 0,
      remainingPoints: event.remainingPoints ?? event.totalPointsPool ?? 0,
      participantCount: event.participantsCount || event.participantCount || 0,
      maxParticipants: event.maxParticipants,
      registrationStartDate: event.registrationStartDate,
      registrationEndDate: event.registrationEndDate,
      location: event.location,
      virtualLink: event.virtualLink,
      imageUrl: event.imageUrl || event.bannerImageUrl,
      registered: false,
      firstPlacePoints: event.firstPlacePoints,
      secondPlacePoints: event.secondPlacePoints,
      thirdPlacePoints: event.thirdPlacePoints,
      winners: event.winners || []
    };
  }

  private normalizeStatus(status: string): 'Upcoming' | 'Active' | 'Completed' {
    const statusLower = (status || '').toLowerCase();
    if (statusLower === 'upcoming' || statusLower === 'published') return 'Upcoming';
    if (statusLower === 'active') return 'Active';
    if (statusLower === 'completed') return 'Completed';
    return 'Upcoming';
  }

  private loadRegistrationStatus(): void {
    const userId = this.currentUserId();
    if (!userId) return;

    this.events().forEach((event) => {
      this.eventService.getEventParticipants(event.id).subscribe({
        next: (response) => {
          if (response.success && response.data) {
            const isRegistered = response.data.some((p: any) => p.userId === userId);
            
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
        event.description.toLowerCase().includes(query) ||
        (event.location && event.location.toLowerCase().includes(query))
      );
    }

    // Sort: Upcoming first, then Active, then Completed; within status sort by date
    filtered.sort((a, b) => {
      const statusOrder = { 'Upcoming': 0, 'Active': 1, 'Completed': 2 };
      if (statusOrder[a.status] !== statusOrder[b.status]) {
        return statusOrder[a.status] - statusOrder[b.status];
      }
      return new Date(a.eventDate).getTime() - new Date(b.eventDate).getTime();
    });

    this.filteredEvents.set(filtered);
  }

  onFilterChange(filter: 'all' | 'Upcoming' | 'Active' | 'Completed'): void {
    this.selectedFilter.set(filter);
    this.applyFilters();
  }

  onSearchChange(): void {
    this.applyFilters();
  }

  clearSearch(): void {
    this.searchQuery.set('');
    this.applyFilters();
  }

  registerForEvent(event: DisplayEvent): void {
    const userId = this.currentUserId();
    if (!userId) {
      this.toast.error('Please log in to register for events');
      return;
    }

    if (event.status !== 'Upcoming') {
      this.toast.warning('Registration is only available for upcoming events');
      return;
    }

    // Check if registration period is valid
    if (!this.isRegistrationOpen(event)) {
      this.toast.warning('Registration is not currently open for this event');
      return;
    }

    this.eventService.registerParticipant(event.id, { eventId: event.id, userId }).subscribe({
      next: (response) => {
        if (response.success) {
          const updatedEvents = this.events().map(e =>
            e.id === event.id ? { ...e, registered: true, participantCount: e.participantCount + 1 } : e
          );
          this.events.set(updatedEvents);
          this.applyFilters();
          this.toast.success(`Successfully registered for "${event.name}"!`);
          
          // Update modal if open
          if (this.selectedEvent()?.id === event.id) {
            this.selectedEvent.set({ ...event, registered: true, participantCount: event.participantCount + 1 });
          }
        } else {
          this.toast.error(response.message || 'Failed to register for event');
        }
      },
      error: (error) => {
        console.error('Error registering for event:', error);
        this.toast.showValidationErrors(error);
      }
    });
  }

  openDetailsModal(event: DisplayEvent): void {
    this.selectedEvent.set(event);
    this.showDetailsModal.set(true);
  }

  closeDetailsModal(): void {
    this.showDetailsModal.set(false);
    this.selectedEvent.set(null);
  }

  // Helper methods
  getStatusBadgeClass(status: string): string {
    const classes: { [key: string]: string } = {
      'Upcoming': 'badge-upcoming',
      'Active': 'badge-active',
      'Completed': 'badge-completed'
    };
    return classes[status] || '';
  }

  getStatusIcon(status: string): string {
    const icons: { [key: string]: string } = {
      'Upcoming': '📅',
      'Active': '🔥',
      'Completed': '✅'
    };
    return icons[status] || '📌';
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

  isRegistrationOpen(event: DisplayEvent): boolean {
    if (event.status !== 'Upcoming') return false;
    
    const now = new Date();
    const today = new Date(now.getFullYear(), now.getMonth(), now.getDate());
    
    if (event.registrationStartDate) {
      const regStart = new Date(event.registrationStartDate);
      const startDate = new Date(regStart.getFullYear(), regStart.getMonth(), regStart.getDate());
      if (today < startDate) return false;
    }
    
    if (event.registrationEndDate) {
      const regEnd = new Date(event.registrationEndDate);
      // Compare dates only (inclusive of end date - registration open until end of that day)
      const endDate = new Date(regEnd.getFullYear(), regEnd.getMonth(), regEnd.getDate());
      if (today > endDate) return false;
    }
    
    return !this.isEventFull(event);
  }

  getRegistrationStatus(event: DisplayEvent): string {
    if (event.registered) return 'registered';
    if (event.status !== 'Upcoming') return 'closed';
    if (this.isEventFull(event)) return 'full';
    
    const now = new Date();
    const today = new Date(now.getFullYear(), now.getMonth(), now.getDate());
    
    if (event.registrationStartDate) {
      const regStart = new Date(event.registrationStartDate);
      const startDate = new Date(regStart.getFullYear(), regStart.getMonth(), regStart.getDate());
      if (today < startDate) return 'not-started';
    }
    
    if (event.registrationEndDate) {
      const regEnd = new Date(event.registrationEndDate);
      // Compare dates only (inclusive of end date)
      const endDate = new Date(regEnd.getFullYear(), regEnd.getMonth(), regEnd.getDate());
      if (today > endDate) return 'ended';
    }
    
    return 'open';
  }

  getRankIcon(rank: number): string {
    switch (rank) {
      case 1: return '🥇';
      case 2: return '🥈';
      case 3: return '🥉';
      default: return '🏅';
    }
  }

  getRankLabel(rank: number): string {
    switch (rank) {
      case 1: return '1st Place';
      case 2: return '2nd Place';
      case 3: return '3rd Place';
      default: return `${rank}th Place`;
    }
  }

  formatDate(dateString: string | undefined): string {
    if (!dateString) return 'TBD';
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', { 
      weekday: 'short', 
      year: 'numeric', 
      month: 'short', 
      day: 'numeric' 
    });
  }

  formatTime(dateString: string | undefined): string {
    if (!dateString) return '';
    const date = new Date(dateString);
    return date.toLocaleTimeString('en-US', { 
      hour: '2-digit', 
      minute: '2-digit' 
    });
  }

  getDaysUntilEvent(event: DisplayEvent): number {
    const now = new Date();
    const eventDate = new Date(event.eventDate);
    const diffTime = eventDate.getTime() - now.getTime();
    return Math.ceil(diffTime / (1000 * 60 * 60 * 24));
  }

  getDaysUntilText(event: DisplayEvent): string {
    const days = this.getDaysUntilEvent(event);
    if (days < 0) return 'Event passed';
    if (days === 0) return 'Today!';
    if (days === 1) return 'Tomorrow';
    return `In ${days} days`;
  }

  hasPrizeDistribution(event: DisplayEvent): boolean {
    return !!(event.firstPlacePoints || event.secondPlacePoints || event.thirdPlacePoints);
  }

  refreshData(): void {
    this.loadEvents();
    this.toast.info('Events refreshed!');
  }
}
