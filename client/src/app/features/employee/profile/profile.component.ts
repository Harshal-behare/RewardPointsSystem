import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../../core/services/api.service';
import { UserService, UserDto, UpdateUserDto } from '../../../core/services/user.service';
import { AuthService } from '../../../auth/auth.service';
import { ToastService } from '../../../core/services/toast.service';

interface UserProfileResponse {
  userId: string;
  email: string;
  firstName: string;
  lastName: string;
  roles: string[];
}

interface UserProfile {
  id: string;
  firstName: string;
  lastName: string;
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
  isLoading = signal(true);
  isSaving = signal(false);
  
  profile = signal<UserProfile>({
    id: '',
    firstName: '',
    lastName: '',
    email: '',
    phone: '',
    department: '',
    position: '',
    avatarUrl: 'https://i.pravatar.cc/200?img=1',
    joinDate: ''
  });

  // Password change
  currentPassword: string = '';
  newPassword: string = '';
  confirmPassword: string = '';

  // Edit mode
  isEditMode: boolean = false;
  editedProfile: UserProfile = { ...this.profile() };

  // Avatar upload
  selectedFile: File | null = null;
  previewUrl: string | null = null;

  // Current user ID
  currentUserId: string = '';

  constructor(
    private api: ApiService,
    private userService: UserService,
    private authService: AuthService,
    private toast: ToastService
  ) {
    // Extract user ID from JWT token
    const token = this.authService.getToken();
    if (token) {
      try {
        const payload = JSON.parse(atob(token.split('.')[1]));
        this.currentUserId = payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] || 
                            payload.sub || 
                            payload.userId || '';
      } catch (e) {
        console.error('Error parsing token:', e);
      }
    }
  }

  ngOnInit(): void {
    this.loadProfile();
  }

  loadProfile(): void {
    this.isLoading.set(true);
    
    // First get basic auth info
    this.api.get<UserProfileResponse>('Auth/me').subscribe({
      next: (response) => {
        if (response.success && response.data) {
          const user = response.data;
          const newProfile: UserProfile = {
            id: user.userId || this.currentUserId,
            firstName: user.firstName || '',
            lastName: user.lastName || '',
            email: user.email || '',
            phone: '',
            department: '',
            position: user.roles?.[0] || 'Employee',
            avatarUrl: 'https://i.pravatar.cc/200?img=1',
            joinDate: ''
          };
          this.profile.set(newProfile);
          this.editedProfile = { ...newProfile };
          
          // Now try to get more detailed user info
          if (this.currentUserId) {
            this.loadUserDetails();
          }
        }
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading profile:', error);
        this.toast.error('Failed to load profile data');
        this.isLoading.set(false);
      }
    });
  }

  loadUserDetails(): void {
    this.userService.getUserById(this.currentUserId).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          const user = response.data;
          const updatedProfile: UserProfile = {
            ...this.profile(),
            id: user.id,
            firstName: user.firstName,
            lastName: user.lastName,
            email: user.email,
            joinDate: user.createdAt ? new Date(user.createdAt).toLocaleDateString() : ''
          };
          this.profile.set(updatedProfile);
          this.editedProfile = { ...updatedProfile };
        }
      },
      error: (error) => {
        console.error('Error loading user details:', error);
        // Don't show error - auth/me data is sufficient
      }
    });
  }

  enableEditMode(): void {
    this.isEditMode = true;
    this.editedProfile = { ...this.profile() };
  }

  cancelEdit(): void {
    this.isEditMode = false;
    this.editedProfile = { ...this.profile() };
    this.previewUrl = null;
    this.selectedFile = null;
  }

  saveProfile(): void {
    // Validate
    if (!this.editedProfile.firstName.trim()) {
      this.toast.error('First name is required');
      return;
    }

    if (!this.editedProfile.lastName.trim()) {
      this.toast.error('Last name is required');
      return;
    }

    if (!this.editedProfile.email.trim()) {
      this.toast.error('Email is required');
      return;
    }

    // Prepare update data
    const updateData: UpdateUserDto = {
      firstName: this.editedProfile.firstName.trim(),
      lastName: this.editedProfile.lastName.trim(),
      email: this.editedProfile.email.trim()
    };

    this.isSaving.set(true);

    this.userService.updateUser(this.currentUserId, updateData).subscribe({
      next: (response) => {
        if (response.success) {
          this.profile.set({ ...this.editedProfile });
          
          if (this.previewUrl) {
            const updated = this.profile();
            updated.avatarUrl = this.previewUrl;
            this.profile.set(updated);
          }

          this.isEditMode = false;
          this.previewUrl = null;
          this.selectedFile = null;
          
          this.toast.success('Profile updated successfully!');
        } else {
          this.toast.error(response.message || 'Failed to update profile');
        }
        this.isSaving.set(false);
      },
      error: (error) => {
        console.error('Error updating profile:', error);
        // Show backend validation errors
        this.toast.showValidationErrors(error);
        this.isSaving.set(false);
      }
    });
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

    // Call password change API
    this.api.post('Auth/change-password', {
      currentPassword: this.currentPassword,
      newPassword: this.newPassword
    }).subscribe({
      next: (response: any) => {
        if (response.success) {
          this.toast.success('Password changed successfully!');
          // Reset fields
          this.currentPassword = '';
          this.newPassword = '';
          this.confirmPassword = '';
        } else {
          this.toast.error(response.message || 'Failed to change password');
        }
      },
      error: (error) => {
        console.error('Error changing password:', error);
        // Show backend validation errors (like password requirements)
        this.toast.showValidationErrors(error);
      }
    });
  }

  getFullName(): string {
    const p = this.profile();
    return `${p.firstName} ${p.lastName}`.trim();
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
