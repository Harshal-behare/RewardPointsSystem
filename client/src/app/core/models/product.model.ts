export interface Product {
  id: number;
  name: string;
  description: string;
  category: string;
  pointsPrice: number;
  stock: number;
  imageUrl?: string;
  status: 'Active' | 'Inactive';
  createdAt?: Date | string;
}

export interface ProductRedemption {
  id: number;
  productId: number;
  productName: string;
  userId: number;
  userName: string;
  pointsSpent: number;
  status: 'Pending' | 'Approved' | 'Rejected' | 'Completed';
  redeemedAt: Date | string;
}
