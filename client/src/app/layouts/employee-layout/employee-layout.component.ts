import { Component, OnInit } from '@angular/core';
import { RouterModule, Router } from '@angular/router';

@Component({
  selector: 'app-employee-layout',
  standalone: true,
  imports: [RouterModule],
  templateUrl: './employee-layout.component.html',
  styleUrl: './employee-layout.component.scss'
})
export class EmployeeLayoutComponent implements OnInit {
  isSidebarCollapsed = false;
  showProfileDropdown = false;
  isAdmin = false;
  userName = 'John Doe';
  userInitials = 'JD';

  menuItems = [
    { name: 'Dashboard', icon: '📊', route: '/employee/dashboard', active: true },
    { name: 'Events', icon: '🎉', route: '/employee/events', active: false },
    { name: 'Products', icon: '🎁', route: '/employee/products', active: false },
    { name: 'My Account', icon: '💰', route: '/employee/account', active: false },
    { name: 'Profile', icon: '👤', route: '/employee/profile', active: false }
  ];

  constructor(private router: Router) {}

  ngOnInit(): void {
    this.loadUserInfo();
  }

  loadUserInfo(): void {
    // Try to get stored user data
    const userDataStr = localStorage.getItem('user');
    if (userDataStr) {
      try {
        const userData = JSON.parse(userDataStr);
        this.userName = `${userData.firstName || ''} ${userData.lastName || ''}`.trim() || 'User';
        this.userInitials = this.getInitials(userData.firstName, userData.lastName);
        // Check if user is admin
        this.isAdmin = userData.role === 'Admin' || userData.role === 'Administrator' || 
                       userData.roles?.includes('Admin') || userData.roles?.includes('Administrator');
      } catch (e) {
        console.error('Error parsing user data', e);
      }
    }
    
    // Also try the token storage key
    const accessToken = localStorage.getItem('rp_access_token');
    if (accessToken) {
      try {
        const payload = JSON.parse(atob(accessToken.split('.')[1]));
        if (payload.role) {
          this.isAdmin = payload.role === 'Admin' || payload.role === 'Administrator';
        }
        if (payload.name || payload.unique_name) {
          this.userName = payload.name || payload.unique_name;
          const nameParts = this.userName.split(' ');
          this.userInitials = this.getInitials(nameParts[0], nameParts[1]);
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

  toggleSidebar(): void {
    this.isSidebarCollapsed = !this.isSidebarCollapsed;
  }

  toggleProfileDropdown(): void {
    this.showProfileDropdown = !this.showProfileDropdown;
  }

  setActiveMenu(clickedItem: any): void {
    this.menuItems.forEach(item => {
      item.active = item === clickedItem;
    });
  }

  switchToAdmin(): void {
    this.showProfileDropdown = false;
    this.router.navigate(['/admin/dashboard']);
  }

  logout(): void {
    // Clear any stored auth data
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    localStorage.removeItem('userRole');
    localStorage.removeItem('rp_access_token');
    localStorage.removeItem('rp_refresh_token');
    this.router.navigate(['/login']);
  }
}
