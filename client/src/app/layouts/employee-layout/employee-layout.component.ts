import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';

@Component({
  selector: 'app-employee-layout',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './employee-layout.component.html',
  styleUrl: './employee-layout.component.scss'
})
export class EmployeeLayoutComponent {
  isSidebarCollapsed = false;
  showProfileDropdown = false;

  menuItems = [
    { name: 'Dashboard', icon: '📊', route: '/employee/dashboard', active: true },
    { name: 'Events', icon: '🎉', route: '/employee/events', active: false },
    { name: 'Products', icon: '🎁', route: '/employee/products', active: false },
    { name: 'My Account', icon: '💰', route: '/employee/account', active: false },
    { name: 'Profile', icon: '👤', route: '/employee/profile', active: false }
  ];

  constructor(private router: Router) {}

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

  logout(): void {
    // Clear any stored auth data
    localStorage.removeItem('token');
    localStorage.removeItem('userRole');
    this.router.navigate(['/login']);
  }
}
