import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

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
  profile: UserProfile = {
    name: 'John Doe',
    email: 'john.doe@company.com',
    phone: '+1 (555) 123-4567',
    department: 'Sales',
    position: 'Senior Sales Representative',
    avatarUrl: 'https://i.pravatar.cc/200?img=1',
    joinDate: '2022-03-15'
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

  ngOnInit(): void {
    // Load user profile
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
      alert('Name is required');
      return;
    }

    if (!this.editedProfile.email.trim()) {
      alert('Email is required');
      return;
    }

    // Save profile
    this.profile = { ...this.editedProfile };
    
    if (this.previewUrl) {
      this.profile.avatarUrl = this.previewUrl;
    }

    this.isEditMode = false;
    this.previewUrl = null;
    this.selectedFile = null;
    
    alert('Profile updated successfully!');
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
      alert('Please fill in all password fields');
      return;
    }

    if (this.newPassword.length < 8) {
      alert('New password must be at least 8 characters');
      return;
    }

    if (this.newPassword !== this.confirmPassword) {
      alert('New passwords do not match');
      return;
    }

    // Change password
    console.log('Password changed successfully');
    alert('Password changed successfully!');
    
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
