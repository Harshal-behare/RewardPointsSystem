import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CardComponent } from '../../../shared/components/card/card.component';
import { ButtonComponent } from '../../../shared/components/button/button.component';
import { BadgeComponent } from '../../../shared/components/badge/badge.component';
import { Event } from '../../../core/models/event.model';

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

  showModal = false;
  modalMode: 'create' | 'edit' = 'create';
  selectedEvent: Partial<Event> = {};

  ngOnInit(): void {
    // Load events from API
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
      // Call API to create event
    } else {
      console.log('Updating event:', this.selectedEvent);
      // Call API to update event
    }
    this.closeModal();
  }

  deleteEvent(eventId: number): void {
    if (confirm('Are you sure you want to delete this event?')) {
      console.log('Deleting event:', eventId);
      this.events = this.events.filter(e => e.id !== eventId);
    }
  }

  manageParticipants(event: Event): void {
    console.log('Managing participants for:', event.name);
    // Open participants modal
  }
}
