export interface Event {
  id: number;
  name: string;
  description: string;
  eventDate: Date | string;
  status: 'Upcoming' | 'Active' | 'Completed' | 'Cancelled';
  pointsPool: number;
  imageUrl?: string;
  maxParticipants?: number;
  createdBy?: number;
  createdAt?: Date | string;
  participantCount?: number;
}

export interface EventParticipant {
  id: number;
  eventId: number;
  userId: number;
  userName: string;
  userEmail: string;
  registeredAt: Date | string;
  status: 'Registered' | 'CheckedIn' | 'Attended' | 'NoShow' | 'Cancelled' | 'Winner';
  pointsAwarded?: number;
  awardedAt?: Date | string;
  eventRank?: number;
}
