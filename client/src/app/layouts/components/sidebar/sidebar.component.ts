import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [RouterModule],
  template: `
    <aside class="sidebar">
      <div class="sidebar-header">
        <h2>{{ appName }}</h2>
      </div>
      
      <nav class="sidebar-nav">
        @for (item of menuItems; track item.route) {
          <a 
            [routerLink]="item.route"
            routerLinkActive="active"
            class="nav-item"
          >
            <span class="nav-icon">{{ item.icon }}</span>
            <span class="nav-label">{{ item.label }}</span>
          </a>
        }
      </nav>
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
    }

    .sidebar-header {
      padding: 24px;
      border-bottom: 1px solid #F4F6F7;
    }

    .sidebar-header h2 {
      margin: 0;
      font-size: 20px;
      font-weight: 700;
      color: #27AE60;
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
    }

    .nav-label {
      flex: 1;
    }
  `]
})
export class SidebarComponent {
  appName = 'RPS Admin';
  menuItems = [
    { icon: '📊', label: 'Dashboard', route: '/admin/dashboard' },
    { icon: '📅', label: 'Events', route: '/admin/events' },
    { icon: '🎁', label: 'Products', route: '/admin/products' },
    { icon: '🛒', label: 'Redemptions', route: '/admin/redemptions' },
    { icon: '👥', label: 'Users', route: '/admin/users' },
    { icon: '👤', label: 'Profile', route: '/admin/profile' },
  ];
}
