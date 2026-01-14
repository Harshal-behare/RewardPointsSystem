import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CardComponent } from '../../../shared/components/card/card.component';
import { ButtonComponent } from '../../../shared/components/button/button.component';
import { BadgeComponent } from '../../../shared/components/badge/badge.component';
import { Event, EventParticipant } from '../../../core/models/event.model';

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
  events: Event[] = [
    {
      id: 1,
      name: 'Summer Sales Challenge',
      description: 'Complete 10 sales to earn points',
      eventDate: '2026-07-15',
      status: 'Upcoming',
      pointsPool: 5000,
      participantCount: 24
    },
    {
      id: 2,
      name: 'Customer Service Excellence',
      description: 'Achieve 95% satisfaction rating',
      eventDate: '2026-06-20',
      status: 'Active',
      pointsPool: 3000,
      participantCount: 18
    },
    {
      id: 3,
      name: 'Q2 Training Completion',
      description: 'Complete all Q2 training modules',
      eventDate: '2026-05-30',
      status: 'Completed',
      pointsPool: 4000,
      participantCount: 45
    },
    {
      id: 4,
      name: 'Team Building Activity',
      description: 'Participate in team building event',
      eventDate: '2026-08-10',
      status: 'Upcoming',
      pointsPool: 2000,
      participantCount: 12
    },
  ];

  // Filter and Search
  filteredEvents: Event[] = [];
  searchQuery = '';
  selectedFilter: 'all' | 'Upcoming' | 'Active' | 'Completed' = 'all';

  // Event Modal
  showModal = false;
  modalMode: 'create' | 'edit' = 'create';
  selectedEvent: Partial<Event> = {
    name: '',
    description: '',
    eventDate: '',
    pointsPool: 0,
    status: 'Upcoming',
    imageUrl: '',
    maxParticipants: 0
  };

  // Participants Modal
  showParticipantsModal = false;
  selectedEventForParticipants: Event | null = null;
  participants: EventParticipant[] = [];
  filteredParticipants: EventParticipant[] = [];
  participantSearchQuery = '';

  // Winner Selection Modal
  showWinnerModal = false;
  selectedParticipant: EventParticipant | null = null;
  pointsToAward = 0;
  winnerRank = 1;
  winnerValidationError = '';

  // Mock participants data
  private mockParticipants: EventParticipant[] = [
    {
      id: 1,
      eventId: 1,
      userId: 101,
      userName: 'John Smith',
      userEmail: 'john.smith@agdata.com',
      registeredAt: '2026-06-15',
      status: 'Registered'
    },
    {
      id: 2,
      eventId: 1,
      userId: 102,
      userName: 'Sarah Johnson',
      userEmail: 'sarah.johnson@agdata.com',
      registeredAt: '2026-06-16',
      status: 'Winner',
      pointsAwarded: 1000,
      eventRank: 1
    },
    {
      id: 3,
      eventId: 1,
      userId: 103,
      userName: 'Michael Brown',
      userEmail: 'michael.brown@agdata.com',
      registeredAt: '2026-06-17',
      status: 'Registered'
    },
    {
      id: 4,
      eventId: 1,
      userId: 104,
      userName: 'Emily Davis',
      userEmail: 'emily.davis@agdata.com',
      registeredAt: '2026-06-18',
      status: 'CheckedIn'
    },
    {
      id: 5,
      eventId: 2,
      userId: 105,
      userName: 'David Wilson',
      userEmail: 'david.wilson@agdata.com',
      registeredAt: '2026-06-10',
      status: 'Attended'
    },
    {
      id: 6,
      eventId: 2,
      userId: 106,
      userName: 'Jessica Martinez',
      userEmail: 'jessica.martinez@agdata.com',
      registeredAt: '2026-06-11',
      status: 'Registered'
    },
  ];

  ngOnInit(): void {
    // Load events from API
    this.applyFilters();
  }

  applyFilters(): void {
    let filtered = [...this.events];

    // Filter by status
    if (this.selectedFilter !== 'all') {
      filtered = filtered.filter(event => event.status === this.selectedFilter);
    }

    // Filter by search query
    if (this.searchQuery.trim()) {
      const query = this.searchQuery.toLowerCase();
      filtered = filtered.filter(event =>
        event.name.toLowerCase().includes(query) ||
        event.description.toLowerCase().includes(query)
      );
    }

    this.filteredEvents = filtered;
  }

  onFilterChange(filter: 'all' | 'Upcoming' | 'Active' | 'Completed'): void {
    this.selectedFilter = filter;
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
    this.modalMode = 'create';
    this.selectedEvent = {
      name: '',
      description: '',
      eventDate: '',
      status: 'Upcoming',
      pointsPool: 0
    };
    this.showModal = true;
  }

  openEditModal(event: Event): void {
    this.modalMode = 'edit';
    this.selectedEvent = { ...event };
    this.showModal = true;
  }

  closeModal(): void {
    this.showModal = false;
    this.selectedEvent = {};
  }

  saveEvent(): void {
    if (this.modalMode === 'create') {
      console.log('Creating event:', this.selectedEvent);
      // TODO: Call API to create event
      const newEvent: Event = {
        id: Math.max(...this.events.map(e => e.id)) + 1,
        name: this.selectedEvent.name || '',
        description: this.selectedEvent.description || '',
        eventDate: this.selectedEvent.eventDate || '',
        status: this.selectedEvent.status || 'Upcoming',
        pointsPool: this.selectedEvent.pointsPool || 0,
        participantCount: 0
      };
      this.events.unshift(newEvent);
      alert('Event created successfully!');
    } else {
      console.log('Updating event:', this.selectedEvent);
      // TODO: Call API to update event
      const index = this.events.findIndex(e => e.id === this.selectedEvent.id);
      if (index !== -1) {
        this.events[index] = { ...this.selectedEvent } as Event;
      }
      alert('Event updated successfully!');
    }
    this.closeModal();
  }

  deleteEvent(eventId: number): void {
    if (confirm('Are you sure you want to delete this event?')) {
      console.log('Deleting event:', eventId);
      this.events = this.events.filter(e => e.id !== eventId);
    }
  }

  // Participants Modal Methods
  openParticipantsModal(event: Event): void {
    this.selectedEventForParticipants = event;
    // TODO: Load participants from API
    this.participants = this.mockParticipants.filter(p => p.eventId === event.id);
    this.filteredParticipants = [...this.participants];
    this.participantSearchQuery = '';
    this.showParticipantsModal = true;
  }

  closeParticipantsModal(): void {
    this.showParticipantsModal = false;
    this.selectedEventForParticipants = null;
    this.participants = [];
    this.filteredParticipants = [];
    this.participantSearchQuery = '';
  }

  filterParticipants(): void {
    const query = this.participantSearchQuery.toLowerCase().trim();
    if (!query) {
      this.filteredParticipants = [...this.participants];
    } else {
      this.filteredParticipants = this.participants.filter(p =>
        p.userName.toLowerCase().includes(query) ||
        p.userEmail.toLowerCase().includes(query)
      );
    }
  }

  // Winner Selection Modal Methods
  openWinnerSelectionModal(participant: EventParticipant): void {
    if (participant.status === 'Winner') {
      return;
    }
    this.selectedParticipant = participant;
    this.pointsToAward = 0;
    this.winnerRank = 1;
    this.winnerValidationError = '';
    this.showWinnerModal = true;
  }

  closeWinnerModal(): void {
    this.showWinnerModal = false;
    this.selectedParticipant = null;
    this.pointsToAward = 0;
    this.winnerRank = 1;
    this.winnerValidationError = '';
  }

  confirmWinnerSelection(): void {
    // Validate points
    this.winnerValidationError = '';
    
    if (!this.pointsToAward || this.pointsToAward <= 0) {
      this.winnerValidationError = 'Please enter a valid points amount (greater than 0)';
      return;
    }

    const maxPoints = this.selectedEventForParticipants?.pointsPool || 0;
    if (this.pointsToAward > maxPoints) {
      this.winnerValidationError = `Points cannot exceed available pool (${maxPoints} points)`;
      return;
    }

    // TODO: Call API to award points
    console.log('Awarding points:', {
      eventId: this.selectedEventForParticipants?.id,
      userId: this.selectedParticipant?.userId,
      points: this.pointsToAward,
      rank: this.winnerRank
    });

    // Update participant status locally
    if (this.selectedParticipant) {
      this.selectedParticipant.status = 'Winner';
      this.selectedParticipant.pointsAwarded = this.pointsToAward;
      this.selectedParticipant.eventRank = this.winnerRank;
      
      // Update the filtered list
      const index = this.filteredParticipants.findIndex(p => p.id === this.selectedParticipant?.id);
      if (index !== -1) {
        this.filteredParticipants[index] = { ...this.selectedParticipant };
      }
      
      // Update the main participants list
      const mainIndex = this.participants.findIndex(p => p.id === this.selectedParticipant?.id);
      if (mainIndex !== -1) {
        this.participants[mainIndex] = { ...this.selectedParticipant };
      }
    }

    // Show success message
    alert(`✓ Successfully awarded ${this.pointsToAward} points to ${this.selectedParticipant?.userName} (Rank #${this.winnerRank})!`);
    
    this.closeWinnerModal();
  }

  // Quick Action Methods
  exportEvents(): void {
    // TODO: Implement export functionality
    console.log('Exporting events...');
    alert('📥 Events export feature coming soon!');
  }

  refreshData(): void {
    // TODO: Reload events from API
    console.log('Refreshing events data...');
    this.applyFilters();
    alert('🔄 Data refreshed!');
  }
}
