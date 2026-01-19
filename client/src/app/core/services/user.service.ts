import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { ApiResponse } from '../models/api-response.model';

// User Interfaces
export interface UserDto {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
  roles?: string[];
  pointsBalance?: number;
}

export interface UserBalanceDto {
  userId: string;
  firstName: string;
  lastName: string;
  email: string;
  currentBalance: number;
  totalEarned: number;
  totalRedeemed: number;
  lastTransaction?: string;
}

export interface CreateUserDto {
  email: string;
  firstName: string;
  lastName: string;
  password?: string;
}

export interface UpdateUserDto {
  firstName?: string;
  lastName?: string;
  email?: string;
  isActive?: boolean;
}

export interface PaginatedUsers {
  items: UserDto[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

@Injectable({
  providedIn: 'root'
})
export class UserService {
  constructor(private api: ApiService) {}

  // Get all users (paginated)
  getUsers(page: number = 1, pageSize: number = 10): Observable<ApiResponse<PaginatedUsers>> {
    return this.api.get<PaginatedUsers>('Users', { page, pageSize });
  }

  // Get user by ID
  getUserById(id: string): Observable<ApiResponse<UserDto>> {
    return this.api.get<UserDto>(`Users/${id}`);
  }

  // Create new user
  createUser(data: CreateUserDto): Observable<ApiResponse<UserDto>> {
    return this.api.post<UserDto>('Users', data);
  }

  // Update user
  updateUser(id: string, data: UpdateUserDto): Observable<ApiResponse<UserDto>> {
    return this.api.put<UserDto>(`Users/${id}`, data);
  }

  // Delete (deactivate) user
  deleteUser(id: string): Observable<ApiResponse<void>> {
    return this.api.delete<void>(`Users/${id}`);
  }

  // Get user balance
  getUserBalance(id: string): Observable<ApiResponse<UserBalanceDto>> {
    return this.api.get<UserBalanceDto>(`Users/${id}/balance`);
  }

  // Assign role to user
  assignRole(userId: string, roleId: string): Observable<ApiResponse<void>> {
    return this.api.post<void>(`Roles/users/${userId}/assign`, { roleId });
  }

  // Remove role from user
  removeRole(userId: string, roleId: string): Observable<ApiResponse<void>> {
    return this.api.delete<void>(`Roles/users/${userId}/roles/${roleId}`);
  }

  // Get user roles
  getUserRoles(userId: string): Observable<ApiResponse<any[]>> {
    return this.api.get<any[]>(`Roles/users/${userId}/roles`);
  }
}
