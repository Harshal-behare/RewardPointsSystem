import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CardComponent } from '../../../shared/components/card/card.component';
import { ButtonComponent } from '../../../shared/components/button/button.component';
import { BadgeComponent } from '../../../shared/components/badge/badge.component';
import { User } from '../../../core/models/user.model';

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
  users: User[] = [
    {
      id: 1,
      firstName: 'John',
      lastName: 'Doe',
      email: 'john.doe@company.com',
      role: 'Employee',
      status: 'Active',
      pointsBalance: 2500,
      createdAt: '2025-01-15'
    },
    {
      id: 2,
      firstName: 'Jane',
      lastName: 'Smith',
      email: 'jane.smith@company.com',
      role: 'Employee',
      status: 'Active',
      pointsBalance: 3200,
      createdAt: '2025-02-20'
    },
    {
      id: 3,
      firstName: 'Michael',
      lastName: 'Johnson',
      email: 'michael.j@company.com',
      role: 'Admin',
      status: 'Active',
      createdAt: '2024-11-10'
    },
    {
      id: 4,
      firstName: 'Sarah',
      lastName: 'Williams',
      email: 'sarah.w@company.com',
      role: 'Employee',
      status: 'Inactive',
      pointsBalance: 1800,
      createdAt: '2025-03-05'
    },
  ];

  showModal = false;
  modalMode: 'create' | 'edit' = 'create';
  selectedUser: Partial<User> = {};

  ngOnInit(): void {
    // Load users from API
  }

  getStatusVariant(status: string): 'success' | 'secondary' {
    return status === 'Active' ? 'success' : 'secondary';
  }

  getRoleBadgeVariant(role: string): 'info' | 'warning' {
    return role === 'Admin' ? 'warning' : 'info';
  }

  openCreateModal(): void {
    this.modalMode = 'create';
    this.selectedUser = {
      firstName: '',
      lastName: '',
      email: '',
      role: 'Employee',
      status: 'Active'
    };
    this.showModal = true;
  }

  openEditModal(user: User): void {
    this.modalMode = 'edit';
    this.selectedUser = { ...user };
    this.showModal = true;
  }

  closeModal(): void {
    this.showModal = false;
    this.selectedUser = {};
  }

  saveUser(): void {
    if (this.modalMode === 'create') {
      console.log('Creating user:', this.selectedUser);
    } else {
      console.log('Updating user:', this.selectedUser);
    }
    this.closeModal();
  }

  toggleUserStatus(user: User): void {
    const action = user.status === 'Active' ? 'deactivate' : 'activate';
    if (confirm(`Are you sure you want to ${action} this user?`)) {
      user.status = user.status === 'Active' ? 'Inactive' : 'Active';
      console.log(`User ${action}d:`, user.id);
    }
  }

  deleteUser(userId: number): void {
    if (confirm('Are you sure you want to delete this user? This action cannot be undone.')) {
      console.log('Deleting user:', userId);
      this.users = this.users.filter(u => u.id !== userId);
    }
  }
}
