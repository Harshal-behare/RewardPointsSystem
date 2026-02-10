export interface PointsTransaction {
  id: number;
  userId: number;
  userName?: string;
  points: number;
  type: 'Earned' | 'Redeemed';
  source: string;
  description?: string;
  createdAt: Date | string;
}

export interface PointsBalance {
  userId: number;
  balance: number;
  totalEarned: number;
  totalRedeemed: number;
}
