import { Component, OnInit, signal, DestroyRef, inject, computed } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, NavigationEnd } from '@angular/router';
import { filter, forkJoin, Observable } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CardComponent } from '../../../shared/components/card/card.component';
import { ButtonComponent } from '../../../shared/components/button/button.component';
import { BadgeComponent } from '../../../shared/components/badge/badge.component';
import { IconComponent } from '../../../shared/components/icon/icon.component';
import { UserService, UserDto, CreateUserDto, UpdateUserDto } from '../../../core/services/user.service';
import { AdminService } from '../../../core/services/admin.service';
import { ToastService } from '../../../core/services/toast.service';
import { ConfirmDialogService } from '../../../core/services/confirm-dialog.service';
import { AuthService } from '../../../auth/auth.service';
import { ApiService } from '../../../core/services/api.service';

// Role interface for dynamic roles
export interface RoleDto {
  id: string;
  name: string;
  description?: string;
}

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
    DatePipe,
    FormsModule,
    CardComponent,
    ButtonComponent,
    BadgeComponent,
    IconComponent
  ],
  templateUrl: './users.component.html',
  styleUrl: './users.component.scss'
})
export class AdminUsersComponent implements OnInit {
  private destroyRef = inject(DestroyRef);
  
  users = signal<DisplayUser[]>([]);
  isLoading = signal(true);
  currentPage = signal(1);
  pageSize = signal(10);
  totalUsers = signal(0);
  
  // Filter and Search state
  searchQuery = signal('');
  statusFilter = signal<'all' | 'Active' | 'Inactive'>('all');
  roleFilter = signal<'all' | 'Admin' | 'Employee'>('all');
  sortField = signal<'name' | 'email' | 'role' | 'status' | 'createdAt' | 'pointsBalance'>('name');
  sortDirection = signal<'asc' | 'desc'>('asc');
  
  // Computed filtered and sorted users
  filteredUsers = computed(() => {
    let result = [...this.users()];
    
    // Apply search filter
    const query = this.searchQuery().toLowerCase().trim();
    if (query) {
      result = result.filter(u => 
        u.firstName.toLowerCase().includes(query) ||
        u.lastName.toLowerCase().includes(query) ||
        u.email.toLowerCase().includes(query) ||
        `${u.firstName} ${u.lastName}`.toLowerCase().includes(query)
      );
    }
    
    // Apply status filter
    if (this.statusFilter() !== 'all') {
      result = result.filter(u => u.status === this.statusFilter());
    }
    
    // Apply role filter
    if (this.roleFilter() !== 'all') {
      result = result.filter(u => u.role === this.roleFilter());
    }
    
    // Apply sorting
    result.sort((a, b) => {
      let comparison = 0;
      const field = this.sortField();
      
      switch (field) {
        case 'name':
          comparison = `${a.firstName} ${a.lastName}`.localeCompare(`${b.firstName} ${b.lastName}`);
          break;
        case 'email':
          comparison = a.email.localeCompare(b.email);
          break;
        case 'role':
          comparison = a.role.localeCompare(b.role);
          break;
        case 'status':
          comparison = a.status.localeCompare(b.status);
          break;
        case 'createdAt':
          comparison = new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime();
          break;
        case 'pointsBalance':
          comparison = (a.pointsBalance || 0) - (b.pointsBalance || 0);
          break;
      }
      
      return this.sortDirection() === 'asc' ? comparison : -comparison;
    });
    
    return result;
  });
  
  // Admin protection
  adminCount = signal(0);
  currentUserId = signal<string>('');
  
  // Dynamic roles from database
  roles = signal<RoleDto[]>([]);

  showModal = signal(false);
  modalMode = signal<'create' | 'edit'>('create');
  selectedUser: Partial<DisplayUser> = {};
  
  // Validation errors for modal
  modalValidationErrors: string[] = [];

  constructor(
    private router: Router,
    private userService: UserService,
    private adminService: AdminService,
    private authService: AuthService,
    private toast: ToastService,
    private apiService: ApiService,
    private confirmDialog: ConfirmDialogService
  ) {
    // Get current user ID from token
    const decoded = this.authService.getDecodedToken();
    if (decoded) {
      this.currentUserId.set(decoded.sub || decoded.nameid || '');
    }
    
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
    
    // Load users, admin count, and roles in parallel
    forkJoin({
      users: this.userService.getUsers(this.currentPage(), this.pageSize()),
      adminCount: this.adminService.getAdminCount(),
      roles: this.apiService.get<RoleDto[]>('Roles')
    }).subscribe({
      next: ({ users, adminCount, roles }) => {
        if (users.success && users.data) {
          this.users.set(this.mapUsersToDisplay(users.data.items || []));
          this.totalUsers.set(users.data.totalCount || 0);
        } else {
          this.useFallbackData();
        }
        
        if (adminCount.success && adminCount.data) {
          this.adminCount.set(adminCount.data.count);
        }
        
        // Set roles from database
        if (roles.success && roles.data) {
          this.roles.set(roles.data);
        } else {
          // Fallback roles if API fails
          this.roles.set([
            { id: '1', name: 'Admin' },
            { id: '2', name: 'Employee' }
          ]);
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

  // Filter and Sort methods
  onSearchChange(): void {
    // Search is automatically applied via computed signal
  }
  
  onStatusFilterChange(status: 'all' | 'Active' | 'Inactive'): void {
    this.statusFilter.set(status);
  }
  
  onRoleFilterChange(role: 'all' | 'Admin' | 'Employee'): void {
    this.roleFilter.set(role);
  }
  
  toggleSort(field: 'name' | 'email' | 'role' | 'status' | 'createdAt' | 'pointsBalance'): void {
    if (this.sortField() === field) {
      // Toggle direction
      this.sortDirection.set(this.sortDirection() === 'asc' ? 'desc' : 'asc');
    } else {
      // New field, default to ascending
      this.sortField.set(field);
      this.sortDirection.set('asc');
    }
  }
  
  getSortIcon(field: string): string {
    if (this.sortField() !== field) return '↕️';
    return this.sortDirection() === 'asc' ? '↑' : '↓';
  }
  
  clearFilters(): void {
    this.searchQuery.set('');
    this.statusFilter.set('all');
    this.roleFilter.set('all');
    this.sortField.set('name');
    this.sortDirection.set('asc');
  }

  /**
   * Check if admin actions (deactivate/delete) should be disabled for a user
   * This protects the last admin from being removed from the system
   */
  isAdminActionDisabled(user: DisplayUser): boolean {
    // Only protect admins
    if (user.role !== 'Admin') {
      return false;
    }
    
    // If there's only one admin, disable actions for that admin
    return this.adminCount() <= 1;
  }

  /**
   * Check if the current user being edited is the last admin and should have role protected
   */
  isLastAdminRoleProtected(): boolean {
    if (this.modalMode() !== 'edit') return false;
    if (!this.selectedUser.id) return false;
    
    // Get the original user (not the edited version)
    const originalUser = this.users().find(u => u.id === this.selectedUser.id);
    if (!originalUser || originalUser.role !== 'Admin') return false;
    
    // If there's only 1 admin, protect role change
    return this.adminCount() <= 1;
  }

  /**
   * Check if the current user being edited is the last admin and should have status protected
   */
  isLastAdminStatusProtected(): boolean {
    if (this.modalMode() !== 'edit') return false;
    if (!this.selectedUser.id) return false;
    
    // Get the original user (not the edited version)
    const originalUser = this.users().find(u => u.id === this.selectedUser.id);
    if (!originalUser || originalUser.role !== 'Admin') return false;
    if (originalUser.status !== 'Active') return false; // Already inactive, can be activated
    
    // If there's only 1 admin and they are active, protect status change
    return this.adminCount() <= 1;
  }

  /**
   * Get tooltip message for disabled admin actions
   */
  getAdminActionTooltip(user: DisplayUser): string {
    if (this.isAdminActionDisabled(user)) {
      return 'Cannot deactivate or delete the last admin in the system';
    }
    return user.status === 'Active' ? 'Deactivate' : 'Activate';
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
    const firstNameRaw = this.selectedUser.firstName || '';
    const firstName = firstNameRaw.trim();
    if (!firstName) {
      this.modalValidationErrors.push('First name is required');
    } else {
      // Check for leading whitespace (before trim)
      if (firstNameRaw.startsWith(' ')) {
        this.modalValidationErrors.push('First name cannot start with a space');
      }
      // Must start with a letter, then letters and spaces only
      if (!/^[a-zA-Z][a-zA-Z ]*$/.test(firstName)) {
        this.modalValidationErrors.push('First name must start with a letter and contain only letters and spaces');
      }
      if (firstName.length > 20) {
        this.modalValidationErrors.push('First name cannot exceed 20 characters');
      }
    }
    
    // Last name validation
    const lastNameRaw = this.selectedUser.lastName || '';
    const lastName = lastNameRaw.trim();
    if (!lastName) {
      this.modalValidationErrors.push('Last name is required');
    } else {
      // Check for leading whitespace (before trim)
      if (lastNameRaw.startsWith(' ')) {
        this.modalValidationErrors.push('Last name cannot start with a space');
      }
      // Must start with a letter, then letters and spaces only
      if (!/^[a-zA-Z][a-zA-Z ]*$/.test(lastName)) {
        this.modalValidationErrors.push('Last name must start with a letter and contain only letters and spaces');
      }
      if (lastName.length > 20) {
        this.modalValidationErrors.push('Last name cannot exceed 20 characters');
      }
    }
    
    // Email validation - must be @agdata.com domain
    const email = (this.selectedUser.email || '').trim().toLowerCase();
    if (!email) {
      this.modalValidationErrors.push('Email is required');
    } else {
      const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
      if (!emailRegex.test(email)) {
        this.modalValidationErrors.push('Invalid email format');
      }
      // Must be @agdata.com domain
      if (!email.endsWith('@agdata.com')) {
        this.modalValidationErrors.push('Email must be an @agdata.com address');
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
        if (password.length > 20) {
          this.modalValidationErrors.push('Password cannot exceed 20 characters');
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
        if (!/[!@#$%^&*()_+\-=\[\]{};':"\\|,.<>\/?]/.test(password)) {
          this.modalValidationErrors.push('Password must contain at least one special character (!@#$%^&*()_+-=[]{};\':"|,.<>/?)');
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
            // Display the error message in the modal
            this.modalValidationErrors = [response.message || 'Failed to create user'];
          }
        },
        error: (error) => {
          // Handle 409 Conflict (duplicate user) and other errors
          const errorMessage = this.extractErrorMessage(error);
          this.modalValidationErrors = [errorMessage];
          // Show toast for visibility
          this.toast.error(errorMessage);
        }
      });
    } else {
      // Get original user to check if role changed
      const originalUser = this.users().find(u => u.id === this.selectedUser.id);
      const roleChanged = originalUser && originalUser.role !== this.selectedUser.role;
      
      const updateData: UpdateUserDto = {
        firstName: this.selectedUser.firstName,
        lastName: this.selectedUser.lastName,
        email: this.selectedUser.email,
        isActive: this.selectedUser.status === 'Active'
      };
      
      this.userService.updateUser(this.selectedUser.id!, updateData).subscribe({
        next: (response) => {
          if (response.success) {
            // Check if role changed - handle role update separately
            if (roleChanged && this.selectedUser.id) {
              this.updateUserRole(this.selectedUser.id, this.selectedUser.role || 'Employee').subscribe({
                next: () => {
                  this.toast.success('User updated successfully!');
                  this.closeModal();
                  this.loadUsers();
                },
                error: (roleError) => {
                  console.error('Error updating role:', roleError);
                  this.toast.warning('User updated but role change failed. Please try again.');
                  this.closeModal();
                  this.loadUsers();
                }
              });
            } else {
              this.toast.success('User updated successfully!');
              this.closeModal();
              this.loadUsers();
            }
          } else {
            this.modalValidationErrors = [response.message || 'Failed to update user'];
          }
        },
        error: (error) => {
          const errorMessage = this.extractErrorMessage(error);
          this.modalValidationErrors = [errorMessage];
          this.toast.error(errorMessage);
        }
      });
    }
  }
  
  /**
   * Extract error message from API error response
   */
  private extractErrorMessage(error: any): string {
    // Handle 409 Conflict specifically
    if (error.status === 409) {
      return error.error?.message || error.error?.Message || 'A user with this email already exists';
    }
    
    // Check for validation errors
    const errors = error?.error?.errors || error?.errors;
    if (errors && typeof errors === 'object') {
      const messages: string[] = [];
      Object.keys(errors).forEach(field => {
        const fieldErrors = errors[field];
        if (Array.isArray(fieldErrors)) {
          fieldErrors.forEach(msg => messages.push(msg));
        } else if (typeof fieldErrors === 'string') {
          messages.push(fieldErrors);
        }
      });
      if (messages.length > 0) return messages.join('. ');
    }
    
    // Check for single message
    return error?.error?.message || error?.error?.Message || error?.message || 'An unexpected error occurred';
  }
  
  /**
   * Update user role via the roles API
   */
  private updateUserRole(userId: string, newRoleName: string): Observable<any> {
    // First, get the role ID for the new role
    const newRole = this.roles().find(r => r.name === newRoleName);
    if (!newRole) {
      return new Observable(subscriber => {
        subscriber.error(new Error(`Role ${newRoleName} not found`));
      });
    }
    
    // Get current user's roles and remove them, then assign the new role
    return new Observable(subscriber => {
      // Get current roles
      this.userService.getUserRoles(userId).subscribe({
        next: (rolesResponse) => {
          const currentRoles = rolesResponse.data || [];
          // Remove all current roles
          const removePromises = currentRoles.map((role: any) => 
            this.userService.removeRole(userId, role.id).toPromise()
          );
          
          Promise.all(removePromises).then(() => {
            // Assign new role
            this.userService.assignRole(userId, newRole.id).subscribe({
              next: () => subscriber.next(true),
              error: (err) => subscriber.error(err)
            });
          }).catch(err => subscriber.error(err));
        },
        error: (err) => {
          // If we can't get roles, just try to assign the new one
          this.userService.assignRole(userId, newRole.id).subscribe({
            next: () => subscriber.next(true),
            error: (assignErr) => subscriber.error(assignErr)
          });
        }
      });
    });
  }

  async toggleUserStatus(user: DisplayUser): Promise<void> {
    // Check if this is the last admin
    if (this.isAdminActionDisabled(user) && user.status === 'Active') {
      this.toast.error('Cannot deactivate the last admin in the system');
      return;
    }
    
    const action = user.status === 'Active' ? 'deactivate' : 'activate';
    const confirmed = user.status === 'Active' 
      ? await this.confirmDialog.confirmDeactivate(`user ${user.firstName} ${user.lastName}`)
      : await this.confirmDialog.confirmActivate(`user ${user.firstName} ${user.lastName}`);
    
    if (confirmed) {
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
            
            // Reload admin count if an admin was activated/deactivated
            if (user.role === 'Admin') {
              this.adminService.getAdminCount().subscribe(res => {
                if (res.success && res.data) {
                  this.adminCount.set(res.data.count);
                }
              });
            }
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
}
