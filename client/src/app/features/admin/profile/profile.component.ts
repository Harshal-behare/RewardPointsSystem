import { Component, OnInit, signal, ChangeDetectorRef } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CardComponent } from '../../../shared/components/card/card.component';
import { ButtonComponent } from '../../../shared/components/button/button.component';
import { ApiService } from '../../../core/services/api.service';
import { ToastService } from '../../../core/services/toast.service';
import { AuthService } from '../../../auth/auth.service';

interface UserProfileResponse {
  userId: string;
  email: string;
  firstName: string;
  lastName: string;
  roles: string[];
}

@Component({
  selector: 'app-admin-profile',
  standalone: true,
  imports: [
    DatePipe,
    FormsModule,
    CardComponent,
    ButtonComponent
  ],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.scss'
})
export class AdminProfileComponent implements OnInit {
  isLoading = signal(true);
  isSaving = signal(false);
  
  userId = '';
  
  profileData = signal({
    firstName: '',
    lastName: '',
    email: '',
    role: '',
    joinedDate: ''
  });

  passwordData = {
    currentPassword: '',
    newPassword: '',
    confirmPassword: ''
  };

  showCurrentPassword = false;
  showNewPassword = false;
  showConfirmPassword = false;

  constructor(
    private api: ApiService,
    private toast: ToastService,
    private authService: AuthService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadProfile();
  }

  loadProfile(): void {
    this.isLoading.set(true);
    this.api.get<UserProfileResponse>('Auth/me').subscribe({
      next: (response) => {
        if (response.success && response.data) {
          const user = response.data;
          this.userId = user.userId;
          this.profileData.set({
            firstName: user.firstName || '',
            lastName: user.lastName || '',
            email: user.email || '',
            role: user.roles?.[0] || 'Administrator',
            joinedDate: ''
          });
        }
        this.isLoading.set(false);
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error loading profile:', error);
        this.toast.error('Failed to load profile data');
        this.isLoading.set(false);
        this.cdr.detectChanges();
      }
    });
  }

  updateProfile(): void {
    const data = this.profileData();
    if (!data.firstName.trim() || !data.lastName.trim()) {
      this.toast.error('First name and last name are required');
      return;
    }

    this.isSaving.set(true);
    this.api.put(`Users/${this.userId}`, {
      firstName: data.firstName.trim(),
      lastName: data.lastName.trim()
    }).subscribe({
      next: (response: any) => {
        if (response.success) {
          this.toast.success('Profile updated successfully!');
          // Update auth service with new name
          this.authService.updateUserName(data.firstName, data.lastName);
          // Update localStorage user data
          const userDataStr = localStorage.getItem('user');
          if (userDataStr) {
            try {
              const userData = JSON.parse(userDataStr);
              userData.firstName = data.firstName;
              userData.lastName = data.lastName;
              localStorage.setItem('user', JSON.stringify(userData));
            } catch (e) {
              console.error('Error updating user data in localStorage', e);
            }
          }
        } else {
          this.toast.error(response.message || 'Failed to update profile');
        }
        this.isSaving.set(false);
      },
      error: (error) => {
        console.error('Error updating profile:', error);
        this.toast.showValidationErrors(error);
        this.isSaving.set(false);
      }
    });
  }

  // Helper methods to update profile data fields
  updateFirstName(value: string): void {
    this.profileData.update(data => ({ ...data, firstName: value }));
  }

  updateLastName(value: string): void {
    this.profileData.update(data => ({ ...data, lastName: value }));
  }

  changePassword(): void {
    if (this.passwordData.newPassword !== this.passwordData.confirmPassword) {
      this.toast.error('New passwords do not match!');
      return;
    }

    if (this.passwordData.newPassword.length < 8) {
      this.toast.error('Password must be at least 8 characters long!');
      return;
    }

    // Validate password strength
    if (!/[A-Z]/.test(this.passwordData.newPassword)) {
      this.toast.error('Password must contain at least one uppercase letter');
      return;
    }
    if (!/[a-z]/.test(this.passwordData.newPassword)) {
      this.toast.error('Password must contain at least one lowercase letter');
      return;
    }
    if (!/[0-9]/.test(this.passwordData.newPassword)) {
      this.toast.error('Password must contain at least one number');
      return;
    }

    this.api.post('Auth/change-password', {
      currentPassword: this.passwordData.currentPassword,
      newPassword: this.passwordData.newPassword
    }).subscribe({
      next: (response: any) => {
        if (response.success) {
          this.toast.success('Password changed successfully!');
          this.passwordData = {
            currentPassword: '',
            newPassword: '',
            confirmPassword: ''
          };
        } else {
          this.toast.error(response.message || 'Failed to change password');
        }
      },
      error: (error) => {
        console.error('Error changing password:', error);
        this.toast.showValidationErrors(error);
      }
    });
  }

  togglePasswordVisibility(field: 'current' | 'new' | 'confirm'): void {
    switch (field) {
      case 'current':
        this.showCurrentPassword = !this.showCurrentPassword;
        break;
      case 'new':
        this.showNewPassword = !this.showNewPassword;
        break;
      case 'confirm':
        this.showConfirmPassword = !this.showConfirmPassword;
        break;
    }
  }
}
