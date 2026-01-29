import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';
import { filter, Subscription } from 'rxjs';
import { AuthService } from '../../../auth/auth.service';

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
export class TopbarComponent implements OnInit, OnDestroy {
  pageTitle = 'Dashboard';
  userName = 'Admin User';
  userRole = 'Administrator';
  currentRole = 'Administrator';
  userInitials = 'AU';
  showDropdown = false;
  isAdmin = false;
  userRoles: string[] = [];
  
  private routerSubscription?: Subscription;
  
  // Route to page title mapping
  private readonly pageTitleMap: { [key: string]: string } = {
    '/admin/dashboard': 'Dashboard',
    '/admin/users': 'Users',
    '/admin/products': 'Products',
    '/admin/events': 'Events',
    '/admin/redemptions': 'Redemptions',
    '/admin/reports': 'Reports',
    '/admin/settings': 'Settings',
    '/employee/dashboard': 'Dashboard',
    '/employee/products': 'Products',
    '/employee/events': 'Events',
    '/employee/account': 'My Account',
    '/employee/profile': 'My Profile'
  };

  constructor(
    private router: Router,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    // Try to get user info from localStorage or API
    this.loadUserInfo();
    
    // Set initial page title based on current route
    this.updatePageTitle(this.router.url);
    
    // Subscribe to route changes to update page title
    this.routerSubscription = this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe((event: any) => {
      this.updatePageTitle(event.urlAfterRedirects || event.url);
    });
    
    // Close dropdown when clicking outside
    document.addEventListener('click', (event) => {
      const target = event.target as HTMLElement;
      if (!target.closest('.user-menu')) {
        this.showDropdown = false;
      }
    });
  }
  
  ngOnDestroy(): void {
    if (this.routerSubscription) {
      this.routerSubscription.unsubscribe();
    }
  }
  
  private updatePageTitle(url: string): void {
    // Remove query params and fragments
    const cleanUrl = url.split('?')[0].split('#')[0];
    
    // Check for exact match first
    if (this.pageTitleMap[cleanUrl]) {
      this.pageTitle = this.pageTitleMap[cleanUrl];
      return;
    }
    
    // Check for partial match (for nested routes)
    for (const [route, title] of Object.entries(this.pageTitleMap)) {
      if (cleanUrl.startsWith(route)) {
        this.pageTitle = title;
        return;
      }
    }
    
    // Default to Dashboard if no match
    this.pageTitle = 'Dashboard';
  }

  loadUserInfo(): void {
    // Get roles from AuthService (decoded from JWT)
    this.userRoles = this.authService.getUserRoles();
    this.isAdmin = this.authService.isAdmin();

    // Try to get stored user data for display info
    const userDataStr = localStorage.getItem('user');
    if (userDataStr) {
      try {
        const userData = JSON.parse(userDataStr);
        this.userName = `${userData.firstName || ''} ${userData.lastName || ''}`.trim() || 'User';
        this.userRole = userData.role || (this.userRoles.length > 0 ? this.userRoles[0] : 'User');
        this.userInitials = this.getInitials(userData.firstName, userData.lastName);
      } catch (e) {
        console.error('Error parsing user data', e);
      }
    }

    // Set current role based on current route
    if (this.router.url.includes('/admin')) {
      this.currentRole = 'Administrator';
    } else {
      this.currentRole = 'Employee';
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
      // Double-check admin permission before navigating
      if (!this.isAdmin) {
        console.warn('Access denied: User does not have Admin role');
        return;
      }
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
