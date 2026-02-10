export interface User {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  role: 'Admin' | 'Employee';
  status: 'Active' | 'Inactive';
  pointsBalance?: number;
  createdAt: Date | string;
  updatedAt?: Date | string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  user: User;
}
