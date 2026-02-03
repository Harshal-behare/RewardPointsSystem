import { Component, OnInit, signal, computed, DestroyRef, inject } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, NavigationEnd, ActivatedRoute } from '@angular/router';
import { filter } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CardComponent } from '../../../shared/components/card/card.component';
import { ButtonComponent } from '../../../shared/components/button/button.component';
import { BadgeComponent } from '../../../shared/components/badge/badge.component';
import { IconComponent } from '../../../shared/components/icon/icon.component';
import { EventService, CreateEventDto, UpdateEventDto } from '../../../core/services/event.service';
import { PointsService } from '../../../core/services/points.service';
import { UserService, UserDto } from '../../../core/services/user.service';
import { AdminService, BudgetValidationResult } from '../../../core/services/admin.service';
import { ToastService } from '../../../core/services/toast.service';
import { ConfirmDialogService } from '../../../core/services/confirm-dialog.service';

interface DisplayEvent {
  id: string;
  name: string;
  description: string;
  eventDate: string;
  eventEndDate?: string;
  status: string;
  pointsPool: number;
  remainingPoints: number;  // Track remaining points from pool
  participantCount: number;
  maxParticipants?: number;
  imageUrl?: string;
  location?: string;
  virtualLink?: string;
  registrationStartDate?: string;
  registrationEndDate?: string;
  firstPlacePoints?: number;
  secondPlacePoints?: number;
  thirdPlacePoints?: number;
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
    DatePipe,
    DecimalPipe,
    FormsModule,
    CardComponent,
    ButtonComponent,
    BadgeComponent,
    IconComponent
  ],
  templateUrl: './events.component.html',
  styleUrl: './events.component.scss'
})
export class AdminEventsComponent implements OnInit {
  private destroyRef = inject(DestroyRef);
  
  events = signal<DisplayEvent[]>([]);
  
  // Filter and Search - 4 statuses: Draft, Upcoming, Active, Completed
  filteredEvents = signal<DisplayEvent[]>([]);
  searchQuery = signal('');
  selectedFilter = signal<'all' | 'Draft' | 'Upcoming' | 'Active' | 'Completed' | 'PendingAwards'>('all');
  
  // Sorting
  sortField = signal<'name' | 'eventDate' | 'status' | 'pointsPool' | 'participantCount'>('eventDate');
  sortDirection = signal<'asc' | 'desc'>('asc');

  // Event Modal
  showModal = signal(false);
  modalMode = signal<'create' | 'edit'>('create');
  selectedEvent: Partial<DisplayEvent> = {
    name: '',
    description: '',
    eventDate: '',
    eventEndDate: '',
    pointsPool: 0,
    remainingPoints: 0,
    status: 'Draft',  // New events start as Draft
    imageUrl: '',
    maxParticipants: undefined,  // undefined means unlimited
    location: '',
    virtualLink: '',
    registrationStartDate: '',
    registrationEndDate: '',
    firstPlacePoints: 0,
    secondPlacePoints: 0,
    thirdPlacePoints: 0
  };
  
  // Track form validity for disabling save button
  isEventFormValid = signal(false);

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
  availableRanks = signal<number[]>([1, 2, 3]); // Track which ranks are still available
  
  // Computed: Check if all 3 ranks have been awarded
  allRanksAwarded = computed(() => {
    const awardedRanks = this.participants()
      .filter(p => p.eventRank && (p.pointsAwarded || p.status === 'Awarded'))
      .map(p => p.eventRank!);
    return [1, 2, 3].every(rank => awardedRanks.includes(rank));
  });

  // Direct Award Points Modal
  showDirectAwardModal = signal(false);
  allUsers = signal<UserDto[]>([]);
  filteredUsers = signal<UserDto[]>([]);
  userSearchQuery = signal('');
  selectedUserForAward = signal<UserDto | null>(null);
  directAwardPoints = signal(0);
  directAwardDescription = signal('');
  directAwardValidationError = signal('');
  isLoadingUsers = signal(false);
  isValidatingBudget = signal(false);
  budgetValidation = signal<BudgetValidationResult | null>(null);
  
  // Constants for validation
  readonly MAX_AWARD_POINTS = 10000;
  readonly MIN_DESCRIPTION_LENGTH = 10;
  readonly MAX_DESCRIPTION_LENGTH = 500;
  
  // Event Modal Validation
  eventModalValidationErrors: string[] = [];

  constructor(
    private router: Router,
    private route: ActivatedRoute,
    private eventService: EventService,
    private pointsService: PointsService,
    private userService: UserService,
    private adminService: AdminService,
    private toast: ToastService,
    private confirmDialog: ConfirmDialogService
  ) {
    // Subscribe to route changes with automatic cleanup
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(() => {
      this.loadEvents();
    });
  }

  ngOnInit(): void {
    this.loadEvents();
    
    // Check for search query from URL params (from dashboard)
    this.route.queryParams.pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(params => {
      if (params['search']) {
        this.searchQuery.set(params['search']);
        this.applyFilters();
        // Remove the query param from URL without reloading
        this.router.navigate([], { 
          relativeTo: this.route, 
          queryParams: {}, 
          replaceUrl: true 
        });
      }
    });
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
    const totalPool = event.totalPointsPool || event.pointsPool || event.pointsReward || 0;
    const remainingPoints = event.remainingPoints ?? totalPool;  // Use remaining if provided, else total
    return {
      id: event.id || event.eventId,
      name: event.name || event.title,
      description: event.description || '',
      eventDate: event.eventDate || event.startDate,
      eventEndDate: event.eventEndDate,
      status: this.mapEventStatus(event.status),
      pointsPool: totalPool,
      remainingPoints: remainingPoints,
      participantCount: event.participantsCount || event.participantCount || event.currentParticipants || 0,
      maxParticipants: event.maxParticipants,
      imageUrl: event.imageUrl || event.bannerImageUrl,
      location: event.location,
      virtualLink: event.virtualLink,
      registrationStartDate: event.registrationStartDate,
      registrationEndDate: event.registrationEndDate,
      firstPlacePoints: event.firstPlacePoints,
      secondPlacePoints: event.secondPlacePoints,
      thirdPlacePoints: event.thirdPlacePoints
    };
  }

  private mapEventStatus(status: string | number): string {
    if (typeof status === 'number') {
      // Backend enum: Draft=0, Upcoming=1, Active=2, Completed=3
      const statusMap: { [key: number]: string } = {
        0: 'Draft',
        1: 'Upcoming',
        2: 'Active',
        3: 'Completed'
      };
      return statusMap[status] || 'Draft';
    }
    // Normalize string status to one of 4 valid values
    const normalizedStatus = (status || '').toLowerCase();
    if (normalizedStatus === 'upcoming' || normalizedStatus === 'published') return 'Upcoming';
    if (normalizedStatus === 'active') return 'Active';
    if (normalizedStatus === 'completed') return 'Completed';
    return 'Draft';
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
        remainingPoints: 5000,
        participantCount: 24
      },
      {
        id: '2',
        name: 'Customer Service Excellence',
        description: 'Achieve 95% satisfaction rating',
        eventDate: '2026-06-20',
        status: 'Upcoming',
        pointsPool: 3000,
        remainingPoints: 3000,
        participantCount: 18
      }
    ]);
    this.toast.warning('Using demo data - API unavailable');
  }

  applyFilters(): void {
    let filtered = [...this.events()];

    // Filter by status
    if (this.selectedFilter() === 'PendingAwards') {
      // Show completed events that still have prize points to award
      filtered = filtered.filter(event => {
        if (event.status !== 'Completed') return false;
        // Check if any prize points are defined and remaining points > 0
        const totalPrizePoints = (event.firstPlacePoints || 0) + (event.secondPlacePoints || 0) + (event.thirdPlacePoints || 0);
        return totalPrizePoints > 0 && (event.remainingPoints ?? event.pointsPool) > 0;
      });
    } else if (this.selectedFilter() !== 'all') {
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
    
    // Apply sorting
    filtered.sort((a, b) => {
      let comparison = 0;
      const field = this.sortField();
      
      switch (field) {
        case 'name':
          comparison = a.name.localeCompare(b.name);
          break;
        case 'eventDate':
          comparison = new Date(a.eventDate).getTime() - new Date(b.eventDate).getTime();
          break;
        case 'status':
          comparison = a.status.localeCompare(b.status);
          break;
        case 'pointsPool':
          comparison = a.pointsPool - b.pointsPool;
          break;
        case 'participantCount':
          comparison = a.participantCount - b.participantCount;
          break;
      }
      
      return this.sortDirection() === 'asc' ? comparison : -comparison;
    });

    this.filteredEvents.set(filtered);
  }

  onFilterChange(filter: 'all' | 'Draft' | 'Upcoming' | 'Active' | 'Completed' | 'PendingAwards'): void {
    this.selectedFilter.set(filter);
    this.applyFilters();
  }

  onSearchChange(): void {
    this.applyFilters();
  }
  
  toggleSort(field: 'name' | 'eventDate' | 'status' | 'pointsPool' | 'participantCount'): void {
    if (this.sortField() === field) {
      this.sortDirection.set(this.sortDirection() === 'asc' ? 'desc' : 'asc');
    } else {
      this.sortField.set(field);
      this.sortDirection.set('asc');
    }
    this.applyFilters();
  }
  
  getSortIcon(field: string): string {
    if (this.sortField() !== field) return '↕️';
    return this.sortDirection() === 'asc' ? '↑' : '↓';
  }
  
  clearFilters(): void {
    this.searchQuery.set('');
    this.selectedFilter.set('all');
    this.sortField.set('eventDate');
    this.sortDirection.set('asc');
    this.applyFilters();
  }

  getStatusVariant(status: string): 'success' | 'warning' | 'info' | 'secondary' {
    const variantMap: { [key: string]: 'success' | 'warning' | 'info' | 'secondary' } = {
      'Draft': 'secondary',
      'Upcoming': 'info',
      'Active': 'warning',
      'Completed': 'success'
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
      'Awarded': 'success',  // Participants who have been awarded points
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
      eventEndDate: '',
      status: 'Draft',  // New events start as Draft
      pointsPool: 0,
      remainingPoints: 0,
      maxParticipants: undefined,  // undefined means unlimited
      imageUrl: '',
      location: '',
      virtualLink: '',
      registrationStartDate: '',
      registrationEndDate: '',
      firstPlacePoints: 0,
      secondPlacePoints: 0,
      thirdPlacePoints: 0
    };
    this.isEventFormValid.set(false);
    this.eventModalValidationErrors = [];
    this.showModal.set(true);
  }

  openEditModal(event: DisplayEvent): void {
    this.modalMode.set('edit');
    // Deep copy all event fields to ensure everything is editable
    this.selectedEvent = { 
      id: event.id,
      name: event.name,
      description: event.description,
      eventDate: this.formatDateForInput(event.eventDate),
      eventEndDate: event.eventEndDate ? this.formatDateForInput(event.eventEndDate) : '',
      status: event.status,
      pointsPool: event.pointsPool,
      remainingPoints: event.remainingPoints,
      participantCount: event.participantCount,
      maxParticipants: event.maxParticipants || undefined,  // undefined means unlimited
      imageUrl: event.imageUrl,
      location: event.location || '',
      virtualLink: event.virtualLink || '',
      registrationStartDate: event.registrationStartDate ? this.formatDateForInput(event.registrationStartDate) : '',
      registrationEndDate: event.registrationEndDate ? this.formatDateForInput(event.registrationEndDate) : '',
      firstPlacePoints: event.firstPlacePoints || 0,
      secondPlacePoints: event.secondPlacePoints || 0,
      thirdPlacePoints: event.thirdPlacePoints || 0
    };
    this.eventModalValidationErrors = [];
    this.validateEventModalRealtime();
    this.showModal.set(true);
  }

  closeModal(): void {
    this.showModal.set(false);
    this.selectedEvent = {};
    this.eventModalValidationErrors = [];
  }

  // Helper to format date for input[type="date"] (YYYY-MM-DD)
  private formatDateForInput(dateString: string): string {
    if (!dateString) return '';
    const date = new Date(dateString);
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  // Helper to convert date input to ISO string for API
  private formatDateForAPI(dateString: string): string {
    if (!dateString) return new Date().toISOString();
    // If already ISO format, return as is
    if (dateString.includes('T')) return dateString;
    // Convert YYYY-MM-DD to ISO format
    const date = new Date(dateString + 'T00:00:00Z');
    return date.toISOString();
  }

  // Status order for one-way flow validation
  private readonly statusOrder: string[] = ['Draft', 'Upcoming', 'Active', 'Completed'];

  /**
   * Check if status transition is valid (one-way only: Draft → Upcoming → Active → Completed)
   */
  private isValidStatusTransition(currentStatus: string, newStatus: string): boolean {
    const currentIndex = this.statusOrder.indexOf(currentStatus);
    const newIndex = this.statusOrder.indexOf(newStatus);

    // If status is the same, it's valid (no change)
    if (currentIndex === newIndex) return true;

    // Only forward transitions are allowed
    return newIndex > currentIndex;
  }

  /**
   * Get allowed next statuses for the current status
   * Admin can only manually change Draft -> Upcoming
   * Other transitions (Upcoming -> Active -> Completed) are automatic
   */
  getAvailableStatusTransitions(currentStatus: string): string[] {
    // Admin can only change Draft to Upcoming
    // Once Upcoming, status changes are automatic based on dates
    if (currentStatus === 'Draft') {
      return ['Draft', 'Upcoming'];
    }
    // For Upcoming, Active, Completed - admin cannot change, it's automatic
    return [currentStatus];
  }

  // Client-side validation matching backend rules
  validateEventModal(): boolean {
    this.eventModalValidationErrors = [];

    // Event name validation
    const name = (this.selectedEvent.name || '').trim();
    if (!name) {
      this.eventModalValidationErrors.push('Event name is required');
    } else {
      if (name.length < 3 || name.length > 200) {
        this.eventModalValidationErrors.push('Event name must be between 3 and 200 characters');
      }
    }

    // Description validation
    const description = (this.selectedEvent.description || '').trim();
    if (description && description.length > 1000) {
      this.eventModalValidationErrors.push('Description cannot exceed 1000 characters');
    }

    // Event date validation (must be in the future for new events)
    const eventDate = this.selectedEvent.eventDate;
    if (!eventDate) {
      this.eventModalValidationErrors.push('Event start date is required');
    } else {
      const selectedDate = new Date(eventDate);
      const now = new Date();
      now.setHours(0, 0, 0, 0); // Compare dates only
      if (this.modalMode() === 'create' && selectedDate <= now) {
        this.eventModalValidationErrors.push('Event start date must be in the future');
      }
    }

    // Event end date validation - MANDATORY
    const eventEndDate = this.selectedEvent.eventEndDate;
    if (!eventEndDate) {
      this.eventModalValidationErrors.push('Event end date is required');
    } else if (eventDate) {
      const startDate = new Date(eventDate);
      const endDate = new Date(eventEndDate);
      if (endDate < startDate) {
        this.eventModalValidationErrors.push('Event end date must be after start date');
      }
    }

    // Registration dates validation - MANDATORY
    const regStartDate = this.selectedEvent.registrationStartDate;
    const regEndDate = this.selectedEvent.registrationEndDate;
    
    if (!regStartDate) {
      this.eventModalValidationErrors.push('Registration start date is required');
    }
    if (!regEndDate) {
      this.eventModalValidationErrors.push('Registration end date is required');
    }
    
    if (regStartDate && regEndDate) {
      const regStart = new Date(regStartDate);
      const regEnd = new Date(regEndDate);
      if (regEnd < regStart) {
        this.eventModalValidationErrors.push('Registration end date must be after registration start date');
      }
    }
    // Registration should end before or on event start
    if (regEndDate && eventDate) {
      const regEnd = new Date(regEndDate);
      const evtStart = new Date(eventDate);
      if (regEnd > evtStart) {
        this.eventModalValidationErrors.push('Registration must close before or on event start date');
      }
    }

    // Points pool validation
    const pointsPool = this.selectedEvent.pointsPool || 0;
    if (pointsPool <= 0) {
      this.eventModalValidationErrors.push('Points pool must be greater than 0');
    }
    if (pointsPool > 1000000) {
      this.eventModalValidationErrors.push('Points pool cannot exceed 1,000,000');
    }

    // Prize distribution validation - MANDATORY and must equal points pool
    const first = this.selectedEvent.firstPlacePoints || 0;
    const second = this.selectedEvent.secondPlacePoints || 0;
    const third = this.selectedEvent.thirdPlacePoints || 0;

    // Prizes are mandatory
    if (first <= 0) {
      this.eventModalValidationErrors.push('1st place points are required and must be greater than 0');
    }
    if (second <= 0) {
      this.eventModalValidationErrors.push('2nd place points are required and must be greater than 0');
    }
    if (third <= 0) {
      this.eventModalValidationErrors.push('3rd place points are required and must be greater than 0');
    }

    // Validate descending order: 1st > 2nd > 3rd
    if (first > 0 && second > 0 && first <= second) {
      this.eventModalValidationErrors.push('1st place points must be greater than 2nd place points');
    }
    if (second > 0 && third > 0 && second <= third) {
      this.eventModalValidationErrors.push('2nd place points must be greater than 3rd place points');
    }
    if (first > 0 && third > 0 && first <= third) {
      this.eventModalValidationErrors.push('1st place points must be greater than 3rd place points');
    }

    // Total prize points must equal points pool
    const totalPrizePoints = first + second + third;
    if (pointsPool > 0 && first > 0 && second > 0 && third > 0) {
      if (totalPrizePoints !== pointsPool) {
        this.eventModalValidationErrors.push(
          `Total prize points (${totalPrizePoints}) must equal points pool (${pointsPool})`
        );
      }
    }

    // Status validation for create mode - only Draft or Upcoming allowed
    if (this.modalMode() === 'create') {
      const status = this.selectedEvent.status;
      if (status !== 'Draft' && status !== 'Upcoming') {
        this.eventModalValidationErrors.push('New events can only be created as Draft or Upcoming');
      }
    }

    // One-way status transition validation (only for edit mode)
    if (this.modalMode() === 'edit' && this.selectedEvent.id) {
      const originalEvent = this.events().find(e => e.id === this.selectedEvent.id);
      if (originalEvent && this.selectedEvent.status) {
        if (!this.isValidStatusTransition(originalEvent.status, this.selectedEvent.status)) {
          this.eventModalValidationErrors.push(
            `Invalid status transition. Events can only move forward: Draft → Upcoming → Active → Completed. ` +
            `Cannot change from "${originalEvent.status}" to "${this.selectedEvent.status}".`
          );
        }
      }
    }

    const isValid = this.eventModalValidationErrors.length === 0;
    this.isEventFormValid.set(isValid);
    return isValid;
  }

  // Real-time validation for form input changes
  validateEventModalRealtime(): void {
    this.validateEventModal();
  }

  // Get available statuses based on mode
  getAvailableStatuses(): string[] {
    if (this.modalMode() === 'create') {
      return ['Draft', 'Upcoming'];
    }
    // For edit mode, get allowed transitions
    const originalEvent = this.events().find(e => e.id === this.selectedEvent.id);
    if (originalEvent) {
      return this.getAvailableStatusTransitions(originalEvent.status);
    }
    return ['Draft', 'Upcoming', 'Active', 'Completed'];
  }

  saveEvent(): void {
    // Client-side validation
    if (!this.validateEventModal()) {
      return;
    }
    
    if (this.modalMode() === 'create') {
      const createData: CreateEventDto = {
        name: this.selectedEvent.name || '',
        description: this.selectedEvent.description || '',
        eventDate: this.formatDateForAPI(this.selectedEvent.eventDate || ''),
        totalPointsPool: this.selectedEvent.pointsPool || 0,
        eventEndDate: this.selectedEvent.eventEndDate ? this.formatDateForAPI(this.selectedEvent.eventEndDate) : undefined,
        maxParticipants: this.selectedEvent.maxParticipants || undefined,
        registrationStartDate: this.selectedEvent.registrationStartDate ? this.formatDateForAPI(this.selectedEvent.registrationStartDate) : undefined,
        registrationEndDate: this.selectedEvent.registrationEndDate ? this.formatDateForAPI(this.selectedEvent.registrationEndDate) : undefined,
        location: this.selectedEvent.location || undefined,
        virtualLink: this.selectedEvent.virtualLink || undefined,
        bannerImageUrl: this.selectedEvent.imageUrl || undefined,
        firstPlacePoints: this.selectedEvent.firstPlacePoints || undefined,
        secondPlacePoints: this.selectedEvent.secondPlacePoints || undefined,
        thirdPlacePoints: this.selectedEvent.thirdPlacePoints || undefined
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
          // Show backend validation errors
          this.toast.showValidationErrors(error);
        }
      });
    } else {
      const updateData: UpdateEventDto = {
        name: this.selectedEvent.name,
        description: this.selectedEvent.description,
        eventDate: this.formatDateForAPI(this.selectedEvent.eventDate || ''),
        totalPointsPool: this.selectedEvent.pointsPool,
        status: this.selectedEvent.status as 'Draft' | 'Upcoming' | 'Active' | 'Completed',
        eventEndDate: this.selectedEvent.eventEndDate ? this.formatDateForAPI(this.selectedEvent.eventEndDate) : undefined,
        maxParticipants: this.selectedEvent.maxParticipants || undefined,
        registrationStartDate: this.selectedEvent.registrationStartDate ? this.formatDateForAPI(this.selectedEvent.registrationStartDate) : undefined,
        registrationEndDate: this.selectedEvent.registrationEndDate ? this.formatDateForAPI(this.selectedEvent.registrationEndDate) : undefined,
        location: this.selectedEvent.location || undefined,
        virtualLink: this.selectedEvent.virtualLink || undefined,
        bannerImageUrl: this.selectedEvent.imageUrl || undefined,
        firstPlacePoints: this.selectedEvent.firstPlacePoints || undefined,
        secondPlacePoints: this.selectedEvent.secondPlacePoints || undefined,
        thirdPlacePoints: this.selectedEvent.thirdPlacePoints || undefined
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
          // Show backend validation errors
          this.toast.showValidationErrors(error);
        }
      });
    }
  }

  async deleteEvent(eventId: string): Promise<void> {
    const event = this.events().find(e => e.id === eventId);
    const confirmed = await this.confirmDialog.confirmDelete(`event "${event?.name}"`);
    if (confirmed) {
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
          this.toast.showApiError(error, 'Failed to delete event');
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
            status: p.pointsAwarded ? 'Awarded' : (p.status || 'Registered'),  // Mark as Awarded if points given
            pointsAwarded: p.pointsAwarded,
            eventRank: p.eventRank || p.rank
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
    // Check if event is completed before allowing award
    const event = this.selectedEventForParticipants();
    if (event && event.status !== 'Completed') {
      this.toast.warning('Points can only be awarded after the event is completed');
      return;
    }
    
    // Don't allow opening modal for already awarded participants
    if (participant.pointsAwarded || participant.status === 'Awarded') {
      this.toast.warning('Points have already been awarded to this participant');
      return;
    }
    
    // Calculate which ranks are still available (not yet awarded)
    const awardedRanks = this.participants()
      .filter(p => p.eventRank && (p.pointsAwarded || p.status === 'Awarded'))
      .map(p => p.eventRank!);
    
    const available = [1, 2, 3].filter(rank => !awardedRanks.includes(rank));
    
    // Check if all ranks are already awarded
    if (available.length === 0) {
      this.toast.warning('All prize positions (1st, 2nd, 3rd) have already been awarded for this event');
      return;
    }
    
    this.availableRanks.set(available);
    this.selectedParticipant.set(participant);
    
    // Set to first available rank
    const firstAvailableRank = available[0];
    this.winnerRank.set(firstAvailableRank);
    
    // Auto-set points based on first available rank
    if (event) {
      switch (firstAvailableRank) {
        case 1:
          this.pointsToAward.set(event.firstPlacePoints || 0);
          break;
        case 2:
          this.pointsToAward.set(event.secondPlacePoints || 0);
          break;
        case 3:
          this.pointsToAward.set(event.thirdPlacePoints || 0);
          break;
      }
    }
    
    this.winnerValidationError.set('');
    this.showWinnerModal.set(true);
  }

  // Handler for rank dropdown change - auto-updates points
  onRankChange(rank: number): void {
    this.winnerRank.set(rank);
    const event = this.selectedEventForParticipants();
    if (event) {
      switch (Number(rank)) {
        case 1:
          this.pointsToAward.set(event.firstPlacePoints || 0);
          break;
        case 2:
          this.pointsToAward.set(event.secondPlacePoints || 0);
          break;
        case 3:
          this.pointsToAward.set(event.thirdPlacePoints || 0);
          break;
        default:
          this.pointsToAward.set(0);
      }
    }
  }

  closeWinnerModal(): void {
    this.showWinnerModal.set(false);
    this.selectedParticipant.set(null);
    this.pointsToAward.set(0);
    this.winnerRank.set(1);
    this.winnerValidationError.set('');
  }

  confirmWinnerSelection(): void {
    // Validate points - Client-side validation matching backend rules
    this.winnerValidationError.set('');
    
    const points = this.pointsToAward();
    const rank = this.winnerRank();
    
    // Points validation
    if (!points || points <= 0) {
      this.winnerValidationError.set('Points must be greater than 0');
      return;
    }
    
    if (points > 100000) {
      this.winnerValidationError.set('Points cannot exceed 100,000 per award');
      return;
    }

    const currentEvent = this.selectedEventForParticipants();
    const remainingPoints = currentEvent?.remainingPoints ?? currentEvent?.pointsPool ?? 0;
    
    if (points > remainingPoints) {
      this.winnerValidationError.set(`Points cannot exceed remaining pool (${remainingPoints} points available)`);
      return;
    }
    
    // Rank validation
    if (!rank || rank < 1) {
      this.winnerValidationError.set('Position must be at least 1');
      return;
    }
    
    if (rank > 100) {
      this.winnerValidationError.set('Position cannot exceed 100');
      return;
    }

    // Call API to award points
    this.pointsService.awardPoints({
      userId: this.selectedParticipant()?.userId || '',
      points: points,
      description: `Winner of event: ${currentEvent?.name} (Rank #${rank})`,
      eventId: currentEvent?.id
    }).subscribe({
      next: (response) => {
        if (response.success) {
          // Update participant status locally to Awarded
          const currentParticipant = this.selectedParticipant();
          if (currentParticipant) {
            const updatedParticipant = {
              ...currentParticipant,
              status: 'Awarded' as const,
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

          // Update the remaining points in the event
          if (currentEvent) {
            const updatedEvent = {
              ...currentEvent,
              remainingPoints: (currentEvent.remainingPoints ?? currentEvent.pointsPool) - this.pointsToAward()
            };
            this.selectedEventForParticipants.set(updatedEvent);
            
            // Also update in the events list
            const eventsList = [...this.events()];
            const eventIndex = eventsList.findIndex(e => e.id === currentEvent.id);
            if (eventIndex !== -1) {
              eventsList[eventIndex] = updatedEvent;
              this.events.set(eventsList);
              this.applyFilters();
            }
          }

          this.toast.success(`Successfully awarded ${this.pointsToAward()} points to ${this.selectedParticipant()?.userName} (Rank #${this.winnerRank()})!`);
          this.closeWinnerModal();
          
          // Reload events to get fresh data from server
          this.loadEvents();
        } else {
          this.winnerValidationError.set(response.message || 'Failed to award points');
        }
      },
      error: (error) => {
        console.error('Error awarding points:', error);
        // Show backend validation errors in the modal
        const messages = error?.error?.errors 
          ? Object.values(error.error.errors).flat().join(' ')
          : error?.error?.message || error?.message || 'Failed to award points';
        this.winnerValidationError.set(messages as string);
      }
    });
  }

  refreshData(): void {
    this.loadEvents();
    this.toast.info('Data refreshed!');
  }

  // ==========================================
  // Direct Award Points Modal Methods
  // ==========================================

  openDirectAwardModal(): void {
    this.showDirectAwardModal.set(true);
    this.resetDirectAwardForm();
    this.loadAllUsers();
  }

  closeDirectAwardModal(): void {
    this.showDirectAwardModal.set(false);
    this.resetDirectAwardForm();
  }

  resetDirectAwardForm(): void {
    this.selectedUserForAward.set(null);
    this.directAwardPoints.set(0);
    this.directAwardDescription.set('');
    this.directAwardValidationError.set('');
    this.userSearchQuery.set('');
  }

  loadAllUsers(): void {
    this.isLoadingUsers.set(true);
    // Load a large page size to get all users for selection
    this.userService.getUsers(1, 500).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          // Filter only active users
          const activeUsers = response.data.items.filter(u => u.isActive);
          this.allUsers.set(activeUsers);
          this.filteredUsers.set(activeUsers);
        }
        this.isLoadingUsers.set(false);
      },
      error: (error) => {
        console.error('Error loading users:', error);
        this.toast.error('Failed to load users');
        this.isLoadingUsers.set(false);
      }
    });
  }

  onUserSearchChange(): void {
    const query = this.userSearchQuery().toLowerCase().trim();
    if (!query) {
      this.filteredUsers.set([...this.allUsers()]);
    } else {
      this.filteredUsers.set(this.allUsers().filter(u =>
        u.firstName.toLowerCase().includes(query) ||
        u.lastName.toLowerCase().includes(query) ||
        u.email.toLowerCase().includes(query) ||
        `${u.firstName} ${u.lastName}`.toLowerCase().includes(query)
      ));
    }
  }

  selectUserForAward(user: UserDto): void {
    this.selectedUserForAward.set(user);
    this.directAwardValidationError.set('');
    this.budgetValidation.set(null);
  }

  deselectUser(): void {
    this.selectedUserForAward.set(null);
    this.budgetValidation.set(null);
  }

  /**
   * Validate points input and check budget limits
   */
  onPointsInputChange(): void {
    this.directAwardValidationError.set('');
    this.budgetValidation.set(null);
    
    const points = this.directAwardPoints();
    
    // Basic validation first
    if (points < 0) {
      this.directAwardValidationError.set('Points cannot be negative');
      return;
    }
    
    if (points === 0) {
      this.directAwardValidationError.set('Points must be greater than 0');
      return;
    }
    
    if (points > this.MAX_AWARD_POINTS) {
      this.directAwardValidationError.set(`Points cannot exceed ${this.MAX_AWARD_POINTS.toLocaleString()} per award`);
      return;
    }
    
    // If valid, check budget
    if (points > 0 && points <= this.MAX_AWARD_POINTS) {
      this.validateBudget(points);
    }
  }

  /**
   * Validate budget for the given points
   */
  validateBudget(points: number): void {
    this.isValidatingBudget.set(true);
    
    this.adminService.validateBudget(points).subscribe({
      next: (response) => {
        this.isValidatingBudget.set(false);
        if (response.success && response.data) {
          this.budgetValidation.set(response.data);
          
          // Show warning or error based on validation result
          if (!response.data.isAllowed) {
            this.directAwardValidationError.set(response.data.message || 'Budget limit exceeded. Cannot award points.');
          } else if (response.data.isWarning && response.data.message) {
            // Warning is shown but award is still allowed
            this.directAwardValidationError.set('');
          }
        }
      },
      error: (error) => {
        this.isValidatingBudget.set(false);
        console.error('Error validating budget:', error);
      }
    });
  }

  validateDirectAward(): boolean {
    this.directAwardValidationError.set('');
    
    if (!this.selectedUserForAward()) {
      this.directAwardValidationError.set('Please select an employee to award points');
      return false;
    }

    const points = this.directAwardPoints();
    
    // Check for negative or zero
    if (points === null || points === undefined || points <= 0) {
      this.directAwardValidationError.set('Points must be greater than 0');
      return false;
    }

    // Check for maximum limit
    if (points > this.MAX_AWARD_POINTS) {
      this.directAwardValidationError.set(`Points cannot exceed ${this.MAX_AWARD_POINTS.toLocaleString()} per award`);
      return false;
    }

    // Check budget validation
    const budget = this.budgetValidation();
    if (budget && !budget.isAllowed) {
      this.directAwardValidationError.set(budget.message || 'Budget limit exceeded. Cannot award points.');
      return false;
    }

    const description = this.directAwardDescription().trim();
    if (!description) {
      this.directAwardValidationError.set('Please provide a reason for this award');
      return false;
    }

    if (description.length < this.MIN_DESCRIPTION_LENGTH) {
      this.directAwardValidationError.set(`Reason must be at least ${this.MIN_DESCRIPTION_LENGTH} characters`);
      return false;
    }

    if (description.length > this.MAX_DESCRIPTION_LENGTH) {
      this.directAwardValidationError.set(`Reason cannot exceed ${this.MAX_DESCRIPTION_LENGTH} characters`);
      return false;
    }

    return true;
  }

  /**
   * Check if the award button should be disabled
   */
  isAwardButtonDisabled(): boolean {
    const points = this.directAwardPoints();
    const description = this.directAwardDescription().trim();
    const budget = this.budgetValidation();
    
    // Disabled if no points, invalid points, no description, or budget not allowed
    return !points || 
           points <= 0 || 
           points > this.MAX_AWARD_POINTS ||
           !description || 
           description.length < this.MIN_DESCRIPTION_LENGTH ||
           this.isValidatingBudget() ||
           (budget !== null && !budget.isAllowed);
  }

  confirmDirectAward(): void {
    if (!this.validateDirectAward()) {
      return;
    }

    const user = this.selectedUserForAward();
    const points = this.directAwardPoints();
    const description = this.directAwardDescription().trim();
    const budget = this.budgetValidation();

    // Show confirmation if there's a budget warning
    if (budget?.isWarning && budget?.message) {
      this.confirmDialog.confirm({
        title: 'Budget Warning',
        message: `${budget.message}\n\nDo you want to proceed with awarding ${points.toLocaleString()} points?`,
        confirmText: 'Proceed',
        cancelText: 'Cancel',
        type: 'warning'
      }).then((confirmed) => {
        if (confirmed) {
          this.executeAwardPoints(user!, points, description);
        }
      });
    } else {
      this.executeAwardPoints(user!, points, description);
    }
  }

  private executeAwardPoints(user: UserDto, points: number, description: string): void {
    this.pointsService.awardPoints({
      userId: user.id,
      points: points,
      description: description
      // Note: No eventId - this is a direct award
    }).subscribe({
      next: (response) => {
        if (response.success) {
          this.toast.success(`Successfully awarded ${points.toLocaleString()} points to ${user.firstName} ${user.lastName}!`);
          this.closeDirectAwardModal();
        } else {
          this.directAwardValidationError.set(response.message || 'Failed to award points');
        }
      },
      error: (error) => {
        console.error('Error awarding points:', error);
        const errorMessage = error?.error?.message || error?.message || 'Failed to award points';
        
        // Check for budget-related errors
        if (errorMessage.toLowerCase().includes('budget') || errorMessage.toLowerCase().includes('limit')) {
          this.directAwardValidationError.set(`Budget limit reached: ${errorMessage}`);
        } else {
          const messages = error?.error?.errors 
            ? Object.values(error.error.errors).flat().join(' ')
            : errorMessage;
          this.directAwardValidationError.set(messages as string);
        }
      }
    });
  }
}
