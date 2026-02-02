import { Component, signal } from '@angular/core';
import { RouterModule } from '@angular/router';
import { SidebarComponent } from '../components/sidebar/sidebar.component';
import { TopbarComponent } from '../components/topbar/topbar.component';

@Component({
  selector: 'app-admin-layout',
  standalone: true,
  imports: [RouterModule, SidebarComponent, TopbarComponent],
  template: `
    <div class="admin-layout">
      <app-sidebar (collapsedChange)="onSidebarCollapse($event)"></app-sidebar>
      <app-topbar [sidebarCollapsed]="isSidebarCollapsed()"></app-topbar>

      <main class="main-content" [class.sidebar-collapsed]="isSidebarCollapsed()">
        <router-outlet></router-outlet>
      </main>
    </div>
  `,
  styles: [`
    .admin-layout {
      min-height: 100vh;
      background-color: #F4F6F7;
      overflow-x: hidden;
    }

    .main-content {
      position: relative;
      margin-left: 260px;
      margin-top: 72px;
      padding: 32px;
      min-height: calc(100vh - 72px);
      transition: margin-left 0.3s ease;
      width: calc(100% - 260px);
      box-sizing: border-box;
      overflow-x: auto;
    }

    .main-content.sidebar-collapsed {
      margin-left: 70px;
      width: calc(100% - 70px);
    }

    @media (max-width: 768px) {
      .main-content {
        margin-left: 0;
        padding: 16px;
        width: 100%;
      }

      .main-content.sidebar-collapsed {
        margin-left: 0;
        width: 100%;
      }
    }
  `]
})
export class AdminLayoutComponent {
  isSidebarCollapsed = signal(false);

  onSidebarCollapse(isCollapsed: boolean): void {
    this.isSidebarCollapsed.set(isCollapsed);
  }
}
