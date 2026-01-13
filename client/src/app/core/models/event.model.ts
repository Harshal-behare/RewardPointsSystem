export interface Event {
  id: number;
  name: string;
  description: string;
  eventDate: Date | string;
  status: 'Upcoming' | 'Active' | 'Completed' | 'Cancelled';
  pointsPool: number;
  createdBy?: number;
  createdAt?: Date | string;
  participantCount?: number;
}

export interface EventParticipant {
  id: number;
  eventId: number;
  userId: number;
  userName: string;
  pointsAwarded: number;
  awardedAt?: Date | string;
}
