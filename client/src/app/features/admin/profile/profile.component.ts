import { Component, OnInit } from '@angular/core';
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
  isLoading = true;
  isSaving = false;
  
  userId = '';
  
  profileData = {
    firstName: '',
    lastName: '',
    email: '',
    role: '',
    joinedDate: ''
  };

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
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.loadProfile();
  }

  loadProfile(): void {
    this.isLoading = true;
    this.api.get<UserProfileResponse>('Auth/me').subscribe({
      next: (response) => {
        if (response.success && response.data) {
          const user = response.data;
          this.userId = user.userId;
          this.profileData = {
            firstName: user.firstName || '',
            lastName: user.lastName || '',
            email: user.email || '',
            role: user.roles?.[0] || 'Administrator',
            joinedDate: ''
          };
        }
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading profile:', error);
        this.toast.error('Failed to load profile data');
        this.isLoading = false;
      }
    });
  }

  updateProfile(): void {
    if (!this.profileData.firstName.trim() || !this.profileData.lastName.trim()) {
      this.toast.error('First name and last name are required');
      return;
    }

    this.isSaving = true;
    this.api.put(`Users/${this.userId}`, {
      firstName: this.profileData.firstName.trim(),
      lastName: this.profileData.lastName.trim()
    }).subscribe({
      next: (response: any) => {
        if (response.success) {
          this.toast.success('Profile updated successfully!');
          // Update auth service with new name
          this.authService.updateUserName(this.profileData.firstName, this.profileData.lastName);
        } else {
          this.toast.error(response.message || 'Failed to update profile');
        }
        this.isSaving = false;
      },
      error: (error) => {
        console.error('Error updating profile:', error);
        this.toast.showValidationErrors(error);
        this.isSaving = false;
      }
    });
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
