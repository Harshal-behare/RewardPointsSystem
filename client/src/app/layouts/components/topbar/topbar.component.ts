import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

@Component({
  selector: 'app-topbar',
  standalone: true,
  imports: [CommonModule],
  template: `
    <header class="topbar">
      <div class="topbar-left">
        <h1>{{ pageTitle }}</h1>
      </div>
      
      <div class="topbar-right">
        <div class="user-menu">
          <div class="user-avatar">
            <span>{{ userInitials }}</span>
          </div>
          <div class="user-info">
            <div class="user-name">{{ userName }}</div>
            <div class="user-role">{{ userRole }}</div>
          </div>
          <button class="logout-btn" (click)="logout()">
            <span>🚪</span>
          </button>
        </div>
      </div>
    </header>
  `,
  styles: [`
    .topbar {
      position: fixed;
      top: 0;
      left: 260px;
      right: 0;
      height: 72px;
      background-color: #FFFFFF;
      border-bottom: 1px solid #F4F6F7;
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 0 32px;
      z-index: 90;
    }

    .topbar-left h1 {
      margin: 0;
      font-size: 24px;
      font-weight: 600;
      color: #2C3E50;
    }

    .topbar-right {
      display: flex;
      align-items: center;
      gap: 16px;
    }

    .user-menu {
      display: flex;
      align-items: center;
      gap: 12px;
      padding: 8px;
      border-radius: 8px;
      transition: background-color 0.2s ease;
    }

    .user-menu:hover {
      background-color: #F4F6F7;
    }

    .user-avatar {
      width: 40px;
      height: 40px;
      border-radius: 50%;
      background-color: #27AE60;
      color: white;
      display: flex;
      align-items: center;
      justify-content: center;
      font-weight: 600;
      font-size: 14px;
    }

    .user-info {
      display: flex;
      flex-direction: column;
    }

    .user-name {
      font-size: 14px;
      font-weight: 600;
      color: #2C3E50;
    }

    .user-role {
      font-size: 12px;
      color: #7A7A7A;
    }

    .logout-btn {
      background: none;
      border: none;
      padding: 8px;
      cursor: pointer;
      font-size: 18px;
      border-radius: 4px;
      transition: background-color 0.2s ease;
    }

    .logout-btn:hover {
      background-color: #FADBD8;
    }
  `]
})
export class TopbarComponent implements OnInit {
  pageTitle = 'Dashboard';
  userName = 'Admin User';
  userRole = 'Administrator';
  userInitials = 'AU';

  constructor(private router: Router) {}

  ngOnInit(): void {
    // Try to get user info from localStorage or API
    this.loadUserInfo();
  }

  loadUserInfo(): void {
    // Try to get stored user data
    const userDataStr = localStorage.getItem('user');
    if (userDataStr) {
      try {
        const userData = JSON.parse(userDataStr);
        this.userName = `${userData.firstName || ''} ${userData.lastName || ''}`.trim() || 'User';
        this.userRole = userData.role || 'User';
        this.userInitials = this.getInitials(userData.firstName, userData.lastName);
      } catch (e) {
        console.error('Error parsing user data', e);
      }
    }
  }

  getInitials(firstName?: string, lastName?: string): string {
    const first = firstName?.charAt(0)?.toUpperCase() || '';
    const last = lastName?.charAt(0)?.toUpperCase() || '';
    return first + last || 'U';
  }

  logout(): void {
    // Clear auth and redirect to login
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    this.router.navigate(['/login']);
  }
}
