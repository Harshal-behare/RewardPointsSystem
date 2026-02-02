import { Component, OnInit, signal, ChangeDetectorRef } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../../core/services/api.service';
import { ToastService } from '../../../core/services/toast.service';
import { AuthService } from '../../../auth/auth.service';
import { IconComponent } from '../../../shared/components/icon/icon.component';

interface UserProfileResponse {
  userId: string;
  email: string;
  firstName: string;
  lastName: string;
  roles: string[];
}

@Component({
  selector: 'app-employee-profile',
  standalone: true,
  imports: [FormsModule, IconComponent],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.scss'
})
export class EmployeeProfileComponent implements OnInit {
  // Use signals for reactive state
  isLoading = signal(true);
  isSaving = signal(false);
  isPasswordFormValid = signal(false);

  userId = '';

  profileData = signal({
    firstName: '',
    lastName: '',
    email: '',
    role: ''
  });

  // Validation errors
  firstNameError = '';
  lastNameError = '';

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
            role: user.roles?.[0] || 'Employee'
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

  // Validation: Letters and spaces only, max 20 characters
  validateName(value: string): string {
    const trimmed = value.trim();
    
    if (!trimmed) {
      return 'This field is required';
    }
    
    if (trimmed.length > 20) {
      return 'Maximum 20 characters allowed';
    }
    
    // Letters and spaces only (including accented characters)
    const namePattern = /^[A-Za-zÀ-ÿ\s]+$/;
    if (!namePattern.test(trimmed)) {
      return 'Only letters and spaces allowed';
    }
    
    return '';
  }

  validateFirstName(): void {
    this.firstNameError = this.validateName(this.profileData().firstName);
  }

  validateLastName(): void {
    this.lastNameError = this.validateName(this.profileData().lastName);
  }

  // Helper methods to update signal fields
  updateFirstName(value: string): void {
    this.profileData.update(data => ({ ...data, firstName: value }));
  }

  updateLastName(value: string): void {
    this.profileData.update(data => ({ ...data, lastName: value }));
  }

  updateProfile(): void {
    // Validate both fields
    this.validateFirstName();
    this.validateLastName();

    if (this.firstNameError || this.lastNameError) {
      this.toast.error('Please fix the validation errors');
      return;
    }

    this.isSaving.set(true);
    this.api.put(`Users/${this.userId}`, {
      firstName: this.profileData().firstName.trim(),
      lastName: this.profileData().lastName.trim()
    }).subscribe({
      next: (response: any) => {
        if (response.success) {
          this.toast.success('Profile updated successfully!');
          // Update auth service with new name
          this.authService.updateUserName(
            this.profileData().firstName.trim(),
            this.profileData().lastName.trim()
          );
        } else {
          this.toast.error(response.message || 'Failed to update profile');
        }
        this.isSaving.set(false);
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error updating profile:', error);
        this.toast.showValidationErrors(error);
        this.isSaving.set(false);
        this.cdr.detectChanges();
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

    if (this.passwordData.newPassword.length > 20) {
      this.toast.error('Password cannot exceed 20 characters!');
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
    if (!/[!@#$%^&*()_+\-=\[\]{};':"\\|,.<>\/?]/.test(this.passwordData.newPassword)) {
      this.toast.error('Password must contain at least one special character (!@#$%^&*()_+-=[]{};\':"|,.<>/?)');
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

  getInitials(): string {
    const first = this.profileData().firstName?.charAt(0) || '';
    const last = this.profileData().lastName?.charAt(0) || '';
    return (first + last).toUpperCase();
  }

  // Password validation helper methods
  hasUppercase(password: string): boolean {
    return /[A-Z]/.test(password);
  }

  hasLowercase(password: string): boolean {
    return /[a-z]/.test(password);
  }

  hasNumber(password: string): boolean {
    return /[0-9]/.test(password);
  }

  hasSpecialChar(password: string): boolean {
    return /[!@#$%^&*()_+\-=\[\]{};':"\\|,.<>\/?]/.test(password);
  }

  validateNewPassword(): void {
    const pwd = this.passwordData.newPassword;
    const isValid = 
      this.passwordData.currentPassword.length > 0 &&
      pwd.length >= 8 &&
      pwd.length <= 20 &&
      this.hasUppercase(pwd) &&
      this.hasLowercase(pwd) &&
      this.hasNumber(pwd) &&
      this.hasSpecialChar(pwd) &&
      this.passwordData.confirmPassword === pwd &&
      this.passwordData.confirmPassword.length > 0;
    
    this.isPasswordFormValid.set(isValid);
  }
}
