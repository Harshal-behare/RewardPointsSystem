import { Component, OnInit, signal, DestroyRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CardComponent } from '../../../shared/components/card/card.component';
import { ButtonComponent } from '../../../shared/components/button/button.component';
import { BadgeComponent } from '../../../shared/components/badge/badge.component';
import { UserService, UserDto, CreateUserDto, UpdateUserDto } from '../../../core/services/user.service';
import { ToastService } from '../../../core/services/toast.service';

interface DisplayUser {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  role: 'Admin' | 'Employee';
  status: 'Active' | 'Inactive';
  pointsBalance?: number;
  createdAt: string;
  password?: string; // Only for create mode
}

@Component({
  selector: 'app-admin-users',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    CardComponent,
    ButtonComponent,
    BadgeComponent
  ],
  templateUrl: './users.component.html',
  styleUrls: ['./users.component.scss']
})
export class AdminUsersComponent implements OnInit {
  private destroyRef = inject(DestroyRef);
  
  users = signal<DisplayUser[]>([]);
  isLoading = signal(true);
  currentPage = signal(1);
  pageSize = signal(10);
  totalUsers = signal(0);

  showModal = signal(false);
  modalMode = signal<'create' | 'edit'>('create');
  selectedUser: Partial<DisplayUser> = {};
  
  // Validation errors for modal
  modalValidationErrors: string[] = [];

  constructor(
    private router: Router,
    private userService: UserService,
    private toast: ToastService
  ) {
    // Subscribe to route changes with automatic cleanup
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(() => {
      this.loadUsers();
    });
  }

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.isLoading.set(true);
    this.userService.getUsers(this.currentPage(), this.pageSize()).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.users.set(this.mapUsersToDisplay(response.data.items || []));
          this.totalUsers.set(response.data.totalCount || 0);
        } else {
          this.useFallbackData();
        }
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading users:', error);
        this.useFallbackData();
        this.isLoading.set(false);
      }
    });
  }

  private mapUsersToDisplay(apiUsers: UserDto[]): DisplayUser[] {
    return apiUsers.map(u => ({
      id: u.id,
      firstName: u.firstName,
      lastName: u.lastName,
      email: u.email,
      role: u.roles?.includes('Admin') ? 'Admin' : 'Employee',
      status: u.isActive ? 'Active' : 'Inactive',
      pointsBalance: u.pointsBalance,
      createdAt: u.createdAt
    }));
  }

  private useFallbackData(): void {
    this.users.set([
      { id: '1', firstName: 'John', lastName: 'Doe', email: 'john.doe@company.com', role: 'Employee', status: 'Active', pointsBalance: 2500, createdAt: '2025-01-15' },
      { id: '2', firstName: 'Jane', lastName: 'Smith', email: 'jane.smith@company.com', role: 'Employee', status: 'Active', pointsBalance: 3200, createdAt: '2025-02-20' },
      { id: '3', firstName: 'Michael', lastName: 'Johnson', email: 'michael.j@company.com', role: 'Admin', status: 'Active', createdAt: '2024-11-10' },
      { id: '4', firstName: 'Sarah', lastName: 'Williams', email: 'sarah.w@company.com', role: 'Employee', status: 'Inactive', pointsBalance: 1800, createdAt: '2025-03-05' },
    ]);
  }

  getStatusVariant(status: string): 'success' | 'secondary' {
    return status === 'Active' ? 'success' : 'secondary';
  }

  getRoleBadgeVariant(role: string): 'info' | 'warning' {
    return role === 'Admin' ? 'warning' : 'info';
  }

  openCreateModal(): void {
    this.modalMode.set('create');
    this.selectedUser = {
      firstName: '',
      lastName: '',
      email: '',
      role: 'Employee',
      status: 'Active',
      password: ''
    };
    this.showModal.set(true);
  }

  openEditModal(user: DisplayUser): void {
    this.modalMode.set('edit');
    // Deep copy all fields to ensure everything is editable
    this.selectedUser = {
      id: user.id,
      firstName: user.firstName,
      lastName: user.lastName,
      email: user.email,
      role: user.role,
      status: user.status,
      pointsBalance: user.pointsBalance,
      createdAt: user.createdAt
    };
    this.showModal.set(true);
  }

  closeModal(): void {
    this.showModal.set(false);
    this.selectedUser = {};
    this.modalValidationErrors = [];
  }

  // Client-side validation matching backend rules
  validateUserModal(): boolean {
    this.modalValidationErrors = [];
    
    // First name validation
    const firstName = (this.selectedUser.firstName || '').trim();
    if (!firstName) {
      this.modalValidationErrors.push('First name is required');
    } else {
      if (!/^[a-zA-Z ]+$/.test(firstName)) {
        this.modalValidationErrors.push('First name can only contain letters and spaces');
      }
      if (firstName.length < 1 || firstName.length > 100) {
        this.modalValidationErrors.push('First name must be between 1 and 100 characters');
      }
    }
    
    // Last name validation
    const lastName = (this.selectedUser.lastName || '').trim();
    if (!lastName) {
      this.modalValidationErrors.push('Last name is required');
    } else {
      if (!/^[a-zA-Z ]+$/.test(lastName)) {
        this.modalValidationErrors.push('Last name can only contain letters and spaces');
      }
      if (lastName.length < 1 || lastName.length > 100) {
        this.modalValidationErrors.push('Last name must be between 1 and 100 characters');
      }
    }
    
    // Email validation
    const email = (this.selectedUser.email || '').trim();
    if (!email) {
      this.modalValidationErrors.push('Email is required');
    } else {
      const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
      if (!emailRegex.test(email)) {
        this.modalValidationErrors.push('Invalid email format');
      }
      if (email.length > 255) {
        this.modalValidationErrors.push('Email cannot exceed 255 characters');
      }
    }
    
    // Password validation (only for create mode)
    if (this.modalMode() === 'create') {
      const password = this.selectedUser.password || '';
      if (!password) {
        this.modalValidationErrors.push('Password is required');
      } else {
        if (password.length < 8) {
          this.modalValidationErrors.push('Password must be at least 8 characters');
        }
        if (!/[A-Z]/.test(password)) {
          this.modalValidationErrors.push('Password must contain at least one uppercase letter');
        }
        if (!/[a-z]/.test(password)) {
          this.modalValidationErrors.push('Password must contain at least one lowercase letter');
        }
        if (!/[0-9]/.test(password)) {
          this.modalValidationErrors.push('Password must contain at least one number');
        }
        if (!/[\W_]/.test(password)) {
          this.modalValidationErrors.push('Password must contain at least one special character');
        }
      }
    }
    
    return this.modalValidationErrors.length === 0;
  }

  saveUser(): void {
    // Client-side validation
    if (!this.validateUserModal()) {
      return;
    }
    
    if (this.modalMode() === 'create') {
      const createData: CreateUserDto = {
        email: this.selectedUser.email || '',
        firstName: this.selectedUser.firstName || '',
        lastName: this.selectedUser.lastName || '',
        password: this.selectedUser.password || '',
        role: this.selectedUser.role || 'Employee'
      };
      
      this.userService.createUser(createData).subscribe({
        next: (response) => {
          if (response.success) {
            this.toast.success('User created successfully!');
            this.closeModal();
            this.loadUsers();
          } else {
            this.toast.error(response.message || 'Failed to create user');
          }
        },
        error: (error) => {
          console.error('Error creating user:', error);
          // Show backend validation errors
          this.toast.showValidationErrors(error);
        }
      });
    } else {
      const updateData: UpdateUserDto = {
        firstName: this.selectedUser.firstName,
        lastName: this.selectedUser.lastName,
        email: this.selectedUser.email,
        isActive: this.selectedUser.status === 'Active'
      };
      
      this.userService.updateUser(this.selectedUser.id!, updateData).subscribe({
        next: (response) => {
          if (response.success) {
            this.toast.success('User updated successfully!');
            this.closeModal();
            this.loadUsers();
          } else {
            this.toast.error(response.message || 'Failed to update user');
          }
        },
        error: (error) => {
          console.error('Error updating user:', error);
          // Show backend validation errors
          this.toast.showValidationErrors(error);
        }
      });
    }
  }

  toggleUserStatus(user: DisplayUser): void {
    const action = user.status === 'Active' ? 'deactivate' : 'activate';
    if (confirm(`Are you sure you want to ${action} this user?`)) {
      const updateData: UpdateUserDto = {
        isActive: user.status !== 'Active'
      };
      
      this.userService.updateUser(user.id, updateData).subscribe({
        next: (response) => {
          if (response.success) {
            // Update the user in the signal
            const updatedUsers = this.users().map(u => 
              u.id === user.id ? { ...u, status: (u.status === 'Active' ? 'Inactive' : 'Active') as 'Active' | 'Inactive' } : u
            );
            this.users.set(updatedUsers);
            this.toast.success(`User ${action}d successfully!`);
          } else {
            this.toast.error(`Failed to ${action} user`);
          }
        },
        error: (error) => {
          console.error('Error toggling user status:', error);
          this.toast.showApiError(error, `Failed to ${action} user`);
        }
      });
    }
  }

  deleteUser(userId: string): void {
    if (confirm('Are you sure you want to delete this user? This action cannot be undone.')) {
      this.userService.deleteUser(userId).subscribe({
        next: (response) => {
          if (response.success) {
            this.toast.success('User deleted successfully!');
            this.loadUsers();
          } else {
            this.toast.error(response.message || 'Failed to delete user');
          }
        },
        error: (error) => {
          console.error('Error deleting user:', error);
          this.toast.showApiError(error, 'Failed to delete user');
        }
      });
    }
  }
}
