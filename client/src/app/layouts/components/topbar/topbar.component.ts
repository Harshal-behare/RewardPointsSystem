import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-topbar',
  standalone: true,
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
            <div class="user-role">{{ currentRole }}</div>
          </div>
          
          <!-- Dropdown Toggle Button -->
          <button class="dropdown-toggle" (click)="toggleDropdown()">
            <span>▼</span>
          </button>
          
          <!-- Dropdown Menu -->
          <div class="dropdown-menu" [class.show]="showDropdown">
            <div class="dropdown-header">
              <strong>Switch Role</strong>
            </div>
            @if (isAdmin) {
              <button 
                class="dropdown-item" 
                [class.active]="currentRole === 'Administrator'"
                (click)="switchRole('admin')">
                <span class="role-icon">👨‍💼</span>
                <span>Admin Dashboard</span>
              </button>
            }
            <button 
              class="dropdown-item"
              [class.active]="currentRole === 'Employee'"
              (click)="switchRole('employee')">
              <span class="role-icon">👤</span>
              <span>Employee Dashboard</span>
            </button>
            <div class="dropdown-divider"></div>
            <button class="dropdown-item logout-item" (click)="logout()">
              <span class="role-icon">🚪</span>
              <span>Log out</span>
            </button>
          </div>
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
      position: relative;
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
    background-color: #E74C3C; /* Red shade */
    color: white;             /* Text color for contrast */
    border: none;
    padding: 8px;
    cursor: pointer;
    font-size: 18px;
    border-radius: 4px;
    transition: background-color 0.2s ease;
}

.logout-btn:hover {
    background-color: #C0392B; /* Darker red on hover */
}

    /* Dropdown Toggle */
    .dropdown-toggle {
      background: none;
      border: none;
      cursor: pointer;
      padding: 4px 8px;
      color: #7A7A7A;
      font-size: 12px;
      border-radius: 4px;
      transition: all 0.2s ease;
    }

    .dropdown-toggle:hover {
      background-color: #E8EBED;
      color: #2C3E50;
    }

    /* Dropdown Menu */
    .dropdown-menu {
      position: absolute;
      top: 100%;
      right: 0;
      margin-top: 8px;
      background-color: white;
      border-radius: 8px;
      box-shadow: 0 4px 16px rgba(0, 0, 0, 0.1);
      min-width: 220px;
      opacity: 0;
      visibility: hidden;
      transform: translateY(-10px);
      transition: all 0.2s ease;
      z-index: 1000;
    }

    .dropdown-menu.show {
      opacity: 1;
      visibility: visible;
      transform: translateY(0);
    }

    .dropdown-header {
      padding: 12px 16px;
      border-bottom: 1px solid #F4F6F7;
      color: #7A7A7A;
      font-size: 12px;
    }

    .dropdown-item {
      display: flex;
      align-items: center;
      gap: 12px;
      width: 100%;
      padding: 12px 16px;
      border: none;
      background: none;
      cursor: pointer;
      font-size: 14px;
      color: #2C3E50;
      text-align: left;
      transition: background-color 0.2s ease;
    }

    .dropdown-item:hover {
      background-color: #F4F6F7;
    }

    .dropdown-item.active {
      background-color: #D5F4E6;
      color: #27AE60;
    }

    .dropdown-item.logout-item {
      border-top: 1px solid #F4F6F7;
      color: #E74C3C;
    }

    .dropdown-item.logout-item:hover {
      background-color: #FADBD8;
    }

    .role-icon {
      font-size: 16px;
    }

    .dropdown-divider {
      height: 1px;
      background-color: #F4F6F7;
      margin: 4px 0;
    }

  `]
})
export class TopbarComponent implements OnInit {
  pageTitle = 'Dashboard';
  userName = 'Admin User';
  userRole = 'Administrator';
  currentRole = 'Administrator';
  userInitials = 'AU';
  showDropdown = false;
  isAdmin = false;

  constructor(
    private router: Router
  ) {}

  ngOnInit(): void {
    // Try to get user info from localStorage or API
    this.loadUserInfo();
    
    // Close dropdown when clicking outside
    document.addEventListener('click', (event) => {
      const target = event.target as HTMLElement;
      if (!target.closest('.user-menu')) {
        this.showDropdown = false;
      }
    });
  }

  loadUserInfo(): void {
    // Try to get stored user data
    const userDataStr = localStorage.getItem('user');
    if (userDataStr) {
      try {
        const userData = JSON.parse(userDataStr);
        this.userName = `${userData.firstName || ''} ${userData.lastName || ''}`.trim() || 'User';
        this.userRole = userData.role || 'User';
        // Check if user is admin
        this.isAdmin = userData.role === 'Admin' || userData.role === 'Administrator' || 
                       userData.roles?.includes('Admin') || userData.roles?.includes('Administrator');
        
        // Set current role based on current route
        if (this.router.url.includes('/admin')) {
          this.currentRole = 'Administrator';
        } else {
          this.currentRole = 'Employee';
        }
        this.userInitials = this.getInitials(userData.firstName, userData.lastName);
      } catch (e) {
        console.error('Error parsing user data', e);
      }
    }
    
    // Also try the token storage key
    const accessToken = localStorage.getItem('rp_access_token');
    if (accessToken) {
      // Decode JWT to get user info
      try {
        const payload = JSON.parse(atob(accessToken.split('.')[1]));
        if (payload.role) {
          this.isAdmin = payload.role === 'Admin' || payload.role === 'Administrator';
        }
      } catch (e) {
        console.error('Error decoding token', e);
      }
    }
  }

  getInitials(firstName?: string, lastName?: string): string {
    const first = firstName?.charAt(0)?.toUpperCase() || '';
    const last = lastName?.charAt(0)?.toUpperCase() || '';
    return first + last || 'U';
  }

  toggleDropdown(): void {
    this.showDropdown = !this.showDropdown;
  }

  switchRole(role: 'admin' | 'employee'): void {
    this.showDropdown = false;
    
    if (role === 'admin') {
      this.currentRole = 'Administrator';
      this.router.navigate(['/admin/dashboard']);
    } else {
      this.currentRole = 'Employee';
      this.router.navigate(['/employee/dashboard']);
    }
  }

  logout(): void {
    this.showDropdown = false;
    // Clear auth and redirect to login
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    localStorage.removeItem('rp_access_token');
    localStorage.removeItem('rp_refresh_token');
    this.router.navigate(['/login']);
  }
}
