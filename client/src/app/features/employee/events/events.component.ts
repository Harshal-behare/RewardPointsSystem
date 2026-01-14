import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

interface Event {
  id: number;
  name: string;
  description: string;
  date: string;
  endDate?: string;
  location: string;
  points: number;
  maxParticipants: number;
  currentParticipants: number;
  image: string;
  status: 'upcoming' | 'ongoing' | 'completed' | 'cancelled';
  registered: boolean;
  category: string;
}

@Component({
  selector: 'app-employee-events',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './events.component.html',
  styleUrl: './events.component.scss'
})
export class EmployeeEventsComponent implements OnInit {
  events: Event[] = [];
  filteredEvents: Event[] = [];
  selectedFilter: string = 'all';
  searchQuery: string = '';
  selectedEvent: Event | null = null;
  showDetailsModal: boolean = false;

  ngOnInit(): void {
    this.loadEvents();
    this.applyFilters();
  }

  loadEvents(): void {
    this.events = [
      {
        id: 1,
        name: 'Annual Sales Conference 2024',
        description: 'Join us for the biggest sales conference of the year. Network with industry leaders, learn new strategies, and celebrate our achievements.',
        date: '2024-02-15',
        endDate: '2024-02-17',
        location: 'Grand Hall, Building A',
        points: 500,
        maxParticipants: 200,
        currentParticipants: 145,
        image: 'https://images.unsplash.com/photo-1540575467063-178a50c2df87?w=600',
        status: 'upcoming',
        registered: true,
        category: 'Conference'
      },
      {
        id: 2,
        name: 'Team Building Workshop',
        description: 'Strengthen team bonds through collaborative activities and problem-solving challenges.',
        date: '2024-02-20',
        location: 'Conference Room 3',
        points: 300,
        maxParticipants: 50,
        currentParticipants: 32,
        image: 'https://images.unsplash.com/photo-1511578314322-379afb476865?w=600',
        status: 'upcoming',
        registered: false,
        category: 'Workshop'
      },
      {
        id: 3,
        name: 'Product Launch Event',
        description: 'Be the first to see our newest product line. Exclusive access for employees!',
        date: '2024-02-25',
        location: 'Virtual Event',
        points: 400,
        maxParticipants: 500,
        currentParticipants: 423,
        image: 'https://images.unsplash.com/photo-1505373877841-8d25f7d46678?w=600',
        status: 'upcoming',
        registered: false,
        category: 'Product Launch'
      },
      {
        id: 4,
        name: 'Health & Wellness Seminar',
        description: 'Learn about nutrition, fitness, and mental health from expert speakers.',
        date: '2024-03-01',
        location: 'Wellness Center',
        points: 250,
        maxParticipants: 100,
        currentParticipants: 67,
        image: 'https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b?w=600',
        status: 'upcoming',
        registered: true,
        category: 'Wellness'
      },
      {
        id: 5,
        name: 'Innovation Challenge Finals',
        description: 'Watch teams compete in the final round of our annual innovation challenge.',
        date: '2024-01-20',
        location: 'Main Auditorium',
        points: 350,
        maxParticipants: 300,
        currentParticipants: 298,
        image: 'https://images.unsplash.com/photo-1552664730-d307ca884978?w=600',
        status: 'completed',
        registered: true,
        category: 'Competition'
      },
      {
        id: 6,
        name: 'Diversity & Inclusion Training',
        description: 'Important training session on building an inclusive workplace culture.',
        date: '2024-02-10',
        location: 'Training Room B',
        points: 200,
        maxParticipants: 75,
        currentParticipants: 58,
        image: 'https://images.unsplash.com/photo-1591115765373-5207764f72e7?w=600',
        status: 'ongoing',
        registered: true,
        category: 'Training'
      },
      {
        id: 7,
        name: 'Company Picnic & BBQ',
        description: 'Relax and enjoy great food and games with your colleagues and their families.',
        date: '2024-03-15',
        location: 'City Park',
        points: 150,
        maxParticipants: 400,
        currentParticipants: 213,
        image: 'https://images.unsplash.com/photo-1555939594-58d7cb561ad1?w=600',
        status: 'upcoming',
        registered: false,
        category: 'Social'
      },
      {
        id: 8,
        name: 'Tech Talk: AI in Business',
        description: 'Explore how artificial intelligence is transforming modern business practices.',
        date: '2024-03-05',
        location: 'Virtual Event',
        points: 300,
        maxParticipants: 1000,
        currentParticipants: 734,
        image: 'https://images.unsplash.com/photo-1677442136019-21780ecad995?w=600',
        status: 'upcoming',
        registered: false,
        category: 'Tech Talk'
      },
      {
        id: 9,
        name: 'Quarterly Town Hall Meeting',
        description: 'Hear from leadership about company performance and future plans.',
        date: '2024-02-28',
        location: 'Main Auditorium',
        points: 100,
        maxParticipants: 500,
        currentParticipants: 412,
        image: 'https://images.unsplash.com/photo-1475721027785-f74eccf877e2?w=600',
        status: 'upcoming',
        registered: true,
        category: 'Meeting'
      }
    ];
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
        event.description.toLowerCase().includes(query) ||
        event.category.toLowerCase().includes(query)
      );
    }

    this.filteredEvents = filtered;
  }

  onFilterChange(filter: string): void {
    this.selectedFilter = filter;
    this.applyFilters();
  }

  onSearchChange(): void {
    this.applyFilters();
  }

  registerForEvent(event: Event): void {
    event.registered = true;
    event.currentParticipants++;
    console.log('Registered for event:', event.name);
  }

  unregisterFromEvent(event: Event): void {
    event.registered = false;
    event.currentParticipants--;
    console.log('Unregistered from event:', event.name);
  }

  openDetailsModal(event: Event): void {
    this.selectedEvent = event;
    this.showDetailsModal = true;
  }

  closeDetailsModal(): void {
    this.showDetailsModal = false;
    this.selectedEvent = null;
  }

  getStatusBadgeClass(status: string): string {
    const classes: { [key: string]: string } = {
      upcoming: 'badge-upcoming',
      ongoing: 'badge-ongoing',
      completed: 'badge-completed',
      cancelled: 'badge-cancelled'
    };
    return classes[status] || '';
  }

  getStatusLabel(status: string): string {
    const labels: { [key: string]: string } = {
      upcoming: 'Upcoming',
      ongoing: 'Ongoing',
      completed: 'Completed',
      cancelled: 'Cancelled'
    };
    return labels[status] || status;
  }

  getAvailabilityPercentage(event: Event): number {
    return (event.currentParticipants / event.maxParticipants) * 100;
  }

  isEventFull(event: Event): boolean {
    return event.currentParticipants >= event.maxParticipants;
  }
}
