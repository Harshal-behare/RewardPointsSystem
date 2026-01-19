import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../../core/services/api.service';
import { ToastService } from '../../../core/services/toast.service';

interface UserProfileResponse {
  userId: string;
  email: string;
  firstName: string;
  lastName: string;
  roles: string[];
}

interface UserProfile {
  name: string;
  email: string;
  phone: string;
  department: string;
  position: string;
  avatarUrl: string;
  joinDate: string;
}

@Component({
  selector: 'app-employee-profile',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.scss'
})
export class EmployeeProfileComponent implements OnInit {
  isLoading = true;
  
  profile: UserProfile = {
    name: '',
    email: '',
    phone: '',
    department: '',
    position: '',
    avatarUrl: 'https://i.pravatar.cc/200?img=1',
    joinDate: ''
  };

  // Password change
  currentPassword: string = '';
  newPassword: string = '';
  confirmPassword: string = '';

  // Edit mode
  isEditMode: boolean = false;
  editedProfile: UserProfile = { ...this.profile };

  // Avatar upload
  selectedFile: File | null = null;
  previewUrl: string | null = null;

  constructor(
    private api: ApiService,
    private toast: ToastService
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
          this.profile = {
            name: `${user.firstName || ''} ${user.lastName || ''}`.trim(),
            email: user.email || '',
            phone: '',
            department: '',
            position: user.roles?.[0] || 'Employee',
            avatarUrl: 'https://i.pravatar.cc/200?img=1',
            joinDate: ''
          };
          this.editedProfile = { ...this.profile };
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

  enableEditMode(): void {
    this.isEditMode = true;
    this.editedProfile = { ...this.profile };
  }

  cancelEdit(): void {
    this.isEditMode = false;
    this.editedProfile = { ...this.profile };
    this.previewUrl = null;
    this.selectedFile = null;
  }

  saveProfile(): void {
    // Validate
    if (!this.editedProfile.name.trim()) {
      this.toast.error('Name is required');
      return;
    }

    if (!this.editedProfile.email.trim()) {
      this.toast.error('Email is required');
      return;
    }

    // For now, just save locally - API update will be added later
    this.profile = { ...this.editedProfile };
    
    if (this.previewUrl) {
      this.profile.avatarUrl = this.previewUrl;
    }

    this.isEditMode = false;
    this.previewUrl = null;
    this.selectedFile = null;
    
    this.toast.success('Profile updated locally. Backend update coming soon.');
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files[0]) {
      this.selectedFile = input.files[0];

      // Preview image
      const reader = new FileReader();
      reader.onload = (e: any) => {
        this.previewUrl = e.target.result;
      };
      reader.readAsDataURL(this.selectedFile);
    }
  }

  changePassword(): void {
    // Validate
    if (!this.currentPassword || !this.newPassword || !this.confirmPassword) {
      this.toast.error('Please fill in all password fields');
      return;
    }

    if (this.newPassword.length < 8) {
      this.toast.error('New password must be at least 8 characters');
      return;
    }

    if (this.newPassword !== this.confirmPassword) {
      this.toast.error('New passwords do not match');
      return;
    }

    // Change password
    console.log('Password changed successfully');
    this.toast.info('Password change feature coming soon');
    
    // Reset fields
    this.currentPassword = '';
    this.newPassword = '';
    this.confirmPassword = '';
  }

  getPasswordStrength(): string {
    if (!this.newPassword) return '';
    
    const length = this.newPassword.length;
    const hasUpperCase = /[A-Z]/.test(this.newPassword);
    const hasLowerCase = /[a-z]/.test(this.newPassword);
    const hasNumbers = /\d/.test(this.newPassword);
    const hasSpecialChar = /[!@#$%^&*(),.?":{}|<>]/.test(this.newPassword);

    let strength = 0;
    if (length >= 8) strength++;
    if (length >= 12) strength++;
    if (hasUpperCase && hasLowerCase) strength++;
    if (hasNumbers) strength++;
    if (hasSpecialChar) strength++;

    if (strength <= 2) return 'weak';
    if (strength <= 4) return 'medium';
    return 'strong';
  }

  // Helper methods for password requirements
  hasMinLength(): boolean {
    return this.newPassword.length >= 8;
  }

  hasUpperCase(): boolean {
    return /[A-Z]/.test(this.newPassword);
  }

  hasLowerCase(): boolean {
    return /[a-z]/.test(this.newPassword);
  }

  hasNumber(): boolean {
    return /\d/.test(this.newPassword);
  }

  hasSpecialChar(): boolean {
    return /[!@#$%^&*(),.?":{}|<>]/.test(this.newPassword);
  }
}
