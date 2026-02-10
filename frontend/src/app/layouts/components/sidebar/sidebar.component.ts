import { Component, signal, output } from '@angular/core';
import { RouterModule } from '@angular/router';
import { IconComponent } from '../../../shared/components/icon/icon.component';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [RouterModule, IconComponent],
  template: `
    <aside class="sidebar" [class.collapsed]="isCollapsed()">
      <!-- Sidebar Header with Logo -->
      <div class="sidebar-header">
        @if (!isCollapsed()) {
          <img
            src="assets/Agdata_logo.png"
            alt="AGDATA Logo"
            class="sidebar-logo"
          />
        }
      </div>

      <!-- Navigation -->
      <nav class="sidebar-nav">
        @for (item of menuItems; track item.route) {
          <a
            [routerLink]="item.route"
            routerLinkActive="active"
            class="nav-item"
            [title]="item.label"
          >
            <span class="nav-icon"><app-icon [name]="item.icon" [size]="20"></app-icon></span>
            <span class="nav-label" [class.hidden]="isCollapsed()">{{ item.label }}</span>
          </a>
        }
      </nav>

      <!-- Sidebar Footer with Toggle Button -->
      <div class="sidebar-footer">
        <button
          class="toggle-btn"
          (click)="toggleSidebar()"
          [attr.aria-label]="isCollapsed() ? 'Expand sidebar' : 'Collapse sidebar'"
          [title]="isCollapsed() ? 'Expand sidebar' : 'Collapse sidebar'"
        >
          <app-icon [name]="isCollapsed() ? 'chevron-right' : 'chevron-left'" [size]="18"></app-icon>
        </button>
      </div>
    </aside>
  `,
  styles: [`
    .sidebar {
      position: fixed;
      left: 0;
      top: 0;
      bottom: 0;
      width: 260px;
      background-color: #FFFFFF;
      border-right: 1px solid #F4F6F7;
      display: flex;
      flex-direction: column;
      z-index: 100;
      transition: width 0.3s ease;
    }

    .sidebar.collapsed {
      width: 70px;
    }

    .sidebar-header {
      height: 90px;
      display: flex;
      align-items: center;
      justify-content: center;
      border-bottom: 1px solid #F4F6F7;
      padding: 12px;
    }

    .sidebar.collapsed .sidebar-header {
      height: 70px;
    }

    .sidebar-logo {
      max-height: 60px;
      max-width: 100%;
      object-fit: contain;
    }

    .sidebar-nav {
      flex: 1;
      padding: 16px 0;
      overflow-y: auto;
    }

    .nav-item {
      display: flex;
      align-items: center;
      padding: 12px 24px;
      color: #2C3E50;
      text-decoration: none;
      transition: all 0.2s ease;
      font-size: 15px;
      font-weight: 500;
    }

    .sidebar.collapsed .nav-item {
      padding: 12px;
      justify-content: center;
    }

    .nav-item:hover {
      background-color: #F4F6F7;
      color: #27AE60;
    }

    .nav-item.active {
      background-color: #D5F4E6;
      color: #27AE60;
      border-right: 3px solid #27AE60;
    }

    .nav-icon {
      margin-right: 12px;
      font-size: 18px;
      display: flex;
      align-items: center;
      justify-content: center;
    }

    .sidebar.collapsed .nav-icon {
      margin-right: 0;
    }

    .nav-label {
      flex: 1;
      white-space: nowrap;
      overflow: hidden;
      transition: opacity 0.2s ease;
    }

    .nav-label.hidden {
      opacity: 0;
      width: 0;
      visibility: hidden;
    }

    .sidebar-footer {
      padding: 16px;
      border-top: 1px solid #F4F6F7;
    }

    .toggle-btn {
      width: 100%;
      padding: 10px;
      background-color: #F4F6F7;
      border: 1px solid #E8EBED;
      border-radius: 8px;
      display: flex;
      align-items: center;
      justify-content: center;
      cursor: pointer;
      color: #7A7A7A;
      transition: all 0.2s ease;
    }

    .toggle-btn:hover {
      background-color: #E8EBED;
      color: #27AE60;
      border-color: #27AE60;
    }
  `]
})
export class SidebarComponent {
  isCollapsed = signal(false);
  collapsedChange = output<boolean>();

  menuItems = [
    { icon: 'dashboard', label: 'Dashboard', route: '/admin/dashboard' },
    { icon: 'events', label: 'Events', route: '/admin/events' },
    { icon: 'gift', label: 'Products', route: '/admin/products' },
    { icon: 'shopping-cart', label: 'Redemptions', route: '/admin/redemptions' },
    { icon: 'users', label: 'Users', route: '/admin/users' },
    { icon: 'user', label: 'Profile', route: '/admin/profile' },
  ];

  toggleSidebar(): void {
    this.isCollapsed.update(v => !v);
    this.collapsedChange.emit(this.isCollapsed());
  }
}
