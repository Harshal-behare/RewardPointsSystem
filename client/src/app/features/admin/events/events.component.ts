import { Component, OnInit, signal, computed, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs';
import { CardComponent } from '../../../shared/components/card/card.component';
import { ButtonComponent } from '../../../shared/components/button/button.component';
import { BadgeComponent } from '../../../shared/components/badge/badge.component';
import { EventService, CreateEventDto, UpdateEventDto } from '../../../core/services/event.service';
import { PointsService } from '../../../core/services/points.service';
import { ToastService } from '../../../core/services/toast.service';

interface DisplayEvent {
  id: string;
  name: string;
  description: string;
  eventDate: string;
  status: string;
  pointsPool: number;
  participantCount: number;
  maxParticipants?: number;
  imageUrl?: string;
}

interface EventParticipant {
  id: number;
  eventId: string;
  userId: string;
  userName: string;
  userEmail: string;
  registeredAt: string;
  status: string;
  pointsAwarded?: number;
  eventRank?: number;
}

@Component({
  selector: 'app-admin-events',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    CardComponent,
    ButtonComponent,
    BadgeComponent
  ],
  templateUrl: './events.component.html',
  styleUrls: ['./events.component.scss']
})
export class AdminEventsComponent implements OnInit {
  events = signal<DisplayEvent[]>([]);
  
  // Filter and Search
  filteredEvents = signal<DisplayEvent[]>([]);
  searchQuery = signal('');
  selectedFilter = signal<'all' | 'Upcoming' | 'Active' | 'Completed'>('all');

  // Event Modal
  showModal = signal(false);
  modalMode = signal<'create' | 'edit'>('create');
  selectedEvent: Partial<DisplayEvent> = {
    name: '',
    description: '',
    eventDate: '',
    pointsPool: 0,
    status: 'Upcoming',
    imageUrl: '',
    maxParticipants: 0
  };

  // Participants Modal
  showParticipantsModal = signal(false);
  selectedEventForParticipants = signal<DisplayEvent | null>(null);
  participants = signal<EventParticipant[]>([]);
  filteredParticipants = signal<EventParticipant[]>([]);
  participantSearchQuery = signal('');

  // Winner Selection Modal
  showWinnerModal = signal(false);
  selectedParticipant = signal<EventParticipant | null>(null);
  pointsToAward = signal(0);
  winnerRank = signal(1);
  winnerValidationError = signal('');

  constructor(
    private router: Router,
    private eventService: EventService,
    private pointsService: PointsService,
    private toast: ToastService
  ) {
    // Use effect to reload data when navigating back to events
    effect(() => {
      this.router.events.pipe(
        filter(event => event instanceof NavigationEnd)
      ).subscribe(() => {
        this.loadEvents();
      });
    });
  }

  ngOnInit(): void {
    this.loadEvents();
  }

  loadEvents(): void {
    this.eventService.getAllEventsAdmin().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.events.set(response.data.map((event: any) => this.mapEventToDisplay(event)));
        } else {
          this.loadFallbackData();
        }
        this.applyFilters();
      },
      error: (error) => {
        console.error('Error loading events:', error);
        this.loadFallbackData();
        this.applyFilters();
      }
    });
  }

  private mapEventToDisplay(event: any): DisplayEvent {
    return {
      id: event.id || event.eventId,
      name: event.name || event.title,
      description: event.description || '',
      eventDate: event.eventDate || event.startDate,
      status: this.mapEventStatus(event.status),
      pointsPool: event.totalPointsPool || event.pointsPool || event.pointsReward || 0,
      participantCount: event.participantsCount || event.participantCount || event.currentParticipants || 0,
      maxParticipants: event.maxParticipants,
      imageUrl: event.imageUrl
    };
  }

  private mapEventStatus(status: string | number): string {
    if (typeof status === 'number') {
      const statusMap: { [key: number]: string } = {
        0: 'Upcoming',
        1: 'Active',
        2: 'Completed',
        3: 'Cancelled'
      };
      return statusMap[status] || 'Upcoming';
    }
    return status || 'Upcoming';
  }

  private loadFallbackData(): void {
    this.events.set([
      {
        id: '1',
        name: 'Summer Sales Challenge',
        description: 'Complete 10 sales to earn points',
        eventDate: '2026-07-15',
        status: 'Upcoming',
        pointsPool: 5000,
        participantCount: 24
      },
      {
        id: '2',
        name: 'Customer Service Excellence',
        description: 'Achieve 95% satisfaction rating',
        eventDate: '2026-06-20',
        status: 'Active',
        pointsPool: 3000,
        participantCount: 18
      }
    ]);
    this.toast.warning('Using demo data - API unavailable');
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

  getStatusVariant(status: string): 'success' | 'warning' | 'info' | 'secondary' {
    const variantMap: { [key: string]: 'success' | 'warning' | 'info' | 'secondary' } = {
      'Active': 'success',
      'Upcoming': 'info',
      'Completed': 'secondary',
      'Cancelled': 'warning'
    };
    return variantMap[status] || 'secondary';
  }

  getParticipantStatusVariant(status: string): 'success' | 'warning' | 'info' | 'secondary' {
    const variantMap: { [key: string]: 'success' | 'warning' | 'info' | 'secondary' } = {
      'Registered': 'info',
      'CheckedIn': 'warning',
      'Attended': 'secondary',
      'NoShow': 'warning',
      'Cancelled': 'secondary',
      'Winner': 'success'
    };
    return variantMap[status] || 'secondary';
  }

  // Event Modal Methods
  openCreateModal(): void {
    this.modalMode.set('create');
    this.selectedEvent = {
      name: '',
      description: '',
      eventDate: '',
      status: 'Upcoming',
      pointsPool: 0
    };
    this.showModal.set(true);
  }

  openEditModal(event: DisplayEvent): void {
    this.modalMode.set('edit');
    this.selectedEvent = { ...event };
    this.showModal.set(true);
  }

  closeModal(): void {
    this.showModal.set(false);
    this.selectedEvent = {};
  }

  saveEvent(): void {
    if (this.modalMode() === 'create') {
      const createData: CreateEventDto = {
        name: this.selectedEvent.name || '',
        description: this.selectedEvent.description || '',
        eventDate: this.selectedEvent.eventDate || new Date().toISOString(),
        totalPointsPool: this.selectedEvent.pointsPool || 0
      };

      this.eventService.createEvent(createData).subscribe({
        next: (response) => {
          if (response.success) {
            this.toast.success('Event created successfully!');
            this.closeModal();
            this.loadEvents();
          } else {
            this.toast.error(response.message || 'Failed to create event');
          }
        },
        error: (error) => {
          console.error('Error creating event:', error);
          this.toast.error('Failed to create event');
        }
      });
    } else {
      const updateData: UpdateEventDto = {
        name: this.selectedEvent.name,
        description: this.selectedEvent.description,
        eventDate: this.selectedEvent.eventDate,
        totalPointsPool: this.selectedEvent.pointsPool
      };

      this.eventService.updateEvent(this.selectedEvent.id!, updateData).subscribe({
        next: (response) => {
          if (response.success) {
            this.toast.success('Event updated successfully!');
            this.closeModal();
            this.loadEvents();
          } else {
            this.toast.error(response.message || 'Failed to update event');
          }
        },
        error: (error) => {
          console.error('Error updating event:', error);
          this.toast.error('Failed to update event');
        }
      });
    }
  }

  deleteEvent(eventId: string): void {
    if (confirm('Are you sure you want to delete this event?')) {
      this.eventService.deleteEvent(eventId).subscribe({
        next: (response) => {
          if (response.success) {
            this.toast.success('Event deleted successfully!');
            this.loadEvents();
          } else {
            this.toast.error(response.message || 'Failed to delete event');
          }
        },
        error: (error) => {
          console.error('Error deleting event:', error);
          this.toast.error('Failed to delete event');
        }
      });
    }
  }

  // Participants Modal Methods
  openParticipantsModal(event: DisplayEvent): void {
    this.selectedEventForParticipants.set(event);
    this.loadParticipants(event.id);
    this.showParticipantsModal.set(true);
  }

  loadParticipants(eventId: string): void {
    this.eventService.getEventParticipants(eventId).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          const participantsList = response.data.map((p: any) => ({
            id: p.id,
            eventId: eventId,
            userId: p.userId,
            userName: p.userName || p.fullName || 'Unknown',
            userEmail: p.userEmail || p.email || '',
            registeredAt: p.registeredAt || p.registrationDate,
            status: p.status || 'Registered',
            pointsAwarded: p.pointsAwarded,
            eventRank: p.rank
          }));
          this.participants.set(participantsList);
          this.filteredParticipants.set([...participantsList]);
        }
      },
      error: (error) => {
        console.error('Error loading participants:', error);
        this.participants.set([]);
        this.filteredParticipants.set([]);
      }
    });
  }

  closeParticipantsModal(): void {
    this.showParticipantsModal.set(false);
    this.selectedEventForParticipants.set(null);
    this.participants.set([]);
    this.filteredParticipants.set([]);
    this.participantSearchQuery.set('');
  }

  filterParticipants(): void {
    const query = this.participantSearchQuery().toLowerCase().trim();
    if (!query) {
      this.filteredParticipants.set([...this.participants()]);
    } else {
      this.filteredParticipants.set(this.participants().filter(p =>
        p.userName.toLowerCase().includes(query) ||
        p.userEmail.toLowerCase().includes(query)
      ));
    }
  }

  // Winner Selection Modal Methods
  openWinnerSelectionModal(participant: EventParticipant): void {
    if (participant.status === 'Winner') {
      return;
    }
    this.selectedParticipant.set(participant);
    this.pointsToAward.set(0);
    this.winnerRank.set(1);
    this.winnerValidationError.set('');
    this.showWinnerModal.set(true);
  }

  closeWinnerModal(): void {
    this.showWinnerModal.set(false);
    this.selectedParticipant.set(null);
    this.pointsToAward.set(0);
    this.winnerRank.set(1);
    this.winnerValidationError.set('');
  }

  confirmWinnerSelection(): void {
    // Validate points
    this.winnerValidationError.set('');
    
    if (!this.pointsToAward() || this.pointsToAward() <= 0) {
      this.winnerValidationError.set('Please enter a valid points amount (greater than 0)');
      return;
    }

    const maxPoints = this.selectedEventForParticipants()?.pointsPool || 0;
    if (this.pointsToAward() > maxPoints) {
      this.winnerValidationError.set(`Points cannot exceed available pool (${maxPoints} points)`);
      return;
    }

    // Call API to award points
    this.pointsService.awardPoints({
      userId: this.selectedParticipant()?.userId || '',
      points: this.pointsToAward(),
      reason: `Winner of event: ${this.selectedEventForParticipants()?.name} (Rank #${this.winnerRank()})`
    }).subscribe({
      next: (response) => {
        if (response.success) {
          // Update participant status locally
          const currentParticipant = this.selectedParticipant();
          if (currentParticipant) {
            const updatedParticipant = {
              ...currentParticipant,
              status: 'Winner' as const,
              pointsAwarded: this.pointsToAward(),
              eventRank: this.winnerRank()
            };
            
            // Update the filtered list
            const filteredList = [...this.filteredParticipants()];
            const index = filteredList.findIndex(p => p.id === currentParticipant.id);
            if (index !== -1) {
              filteredList[index] = updatedParticipant;
              this.filteredParticipants.set(filteredList);
            }
            
            // Update the main participants list
            const mainList = [...this.participants()];
            const mainIndex = mainList.findIndex(p => p.id === currentParticipant.id);
            if (mainIndex !== -1) {
              mainList[mainIndex] = updatedParticipant;
              this.participants.set(mainList);
            }
          }

          this.toast.success(`Successfully awarded ${this.pointsToAward()} points to ${this.selectedParticipant()?.userName} (Rank #${this.winnerRank()})!`);
          this.closeWinnerModal();
        } else {
          this.winnerValidationError.set(response.message || 'Failed to award points');
        }
      },
      error: (error) => {
        console.error('Error awarding points:', error);
        this.winnerValidationError.set('Failed to award points');
      }
    });
  }

  refreshData(): void {
    this.loadEvents();
    this.toast.info('Data refreshed!');
  }
}
