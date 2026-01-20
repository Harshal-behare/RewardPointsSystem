import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { ApiResponse } from '../models/api-response.model';

// Event Interfaces - Status: Draft (admin only), Upcoming (employees can register), Completed (finished)
export interface EventDto {
  id: string;
  name: string;
  description: string;
  eventDate: string;
  status: 'Draft' | 'Upcoming' | 'Completed';
  totalPointsPool: number;
  remainingPoints?: number;
  participantCount?: number;
  createdAt: string;
  createdBy?: string;
}

export interface EventDetailsDto extends EventDto {
  participants?: EventParticipantDto[];
  pointsAwarded?: { userId: string; userName: string; points: number }[];
}

export interface EventParticipantDto {
  id: string;
  eventId: string;
  userId: string;
  userName: string;
  userEmail: string;
  registeredAt: string;
  status: 'Registered' | 'CheckedIn' | 'Attended' | 'Winner' | 'NoShow';
  pointsAwarded?: number;
  eventRank?: number;
}

export interface CreateEventDto {
  name: string;
  description: string;
  eventDate: string;
  totalPointsPool: number;
}

export interface UpdateEventDto {
  name?: string;
  description?: string;
  eventDate?: string;
  totalPointsPool?: number;
}

export interface ChangeEventStatusDto {
  status: 'Draft' | 'Upcoming' | 'Completed';
}

export interface RegisterParticipantDto {
  eventId: string;
  userId: string;
}

@Injectable({
  providedIn: 'root'
})
export class EventService {
  constructor(private api: ApiService) {}

  // Get all published events (public)
  getEvents(): Observable<ApiResponse<EventDto[]>> {
    return this.api.get<EventDto[]>('Events');
  }

  // Alias for getEvents() for consistency
  getAllEvents(): Observable<ApiResponse<EventDto[]>> {
    return this.getEvents();
  }

  // Get all events including drafts (admin)
  getAllEventsAdmin(): Observable<ApiResponse<EventDto[]>> {
    return this.api.get<EventDto[]>('Events/all');
  }

  // Get event by ID
  getEventById(id: string): Observable<ApiResponse<EventDetailsDto>> {
    return this.api.get<EventDetailsDto>(`Events/${id}`);
  }

  // Create new event
  createEvent(data: CreateEventDto): Observable<ApiResponse<EventDto>> {
    return this.api.post<EventDto>('Events', data);
  }

  // Update event
  updateEvent(id: string, data: UpdateEventDto): Observable<ApiResponse<EventDto>> {
    return this.api.put<EventDto>(`Events/${id}`, data);
  }

  // Delete (cancel) event
  deleteEvent(id: string): Observable<ApiResponse<void>> {
    return this.api.delete<void>(`Events/${id}`);
  }

  // Change event status
  changeEventStatus(id: string, status: ChangeEventStatusDto): Observable<ApiResponse<EventDto>> {
    return this.api.patch<EventDto>(`Events/${id}/status`, status);
  }

  // Register participant
  registerParticipant(eventId: string, data: RegisterParticipantDto): Observable<ApiResponse<EventParticipantDto>> {
    return this.api.post<EventParticipantDto>(`Events/${eventId}/participants`, data);
  }

  // Get event participants
  getEventParticipants(eventId: string): Observable<ApiResponse<EventParticipantDto[]>> {
    return this.api.get<EventParticipantDto[]>(`Events/${eventId}/participants`);
  }
}
