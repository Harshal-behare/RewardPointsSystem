import { Component, computed } from '@angular/core';
import { ToastService } from '../../../core/services/toast.service';
import { IconComponent } from '../icon/icon.component';

@Component({
  selector: 'app-toast',
  standalone: true,
  imports: [IconComponent],
  template: `
    <div class="toast-container">
      @for (toast of toasts(); track toast.id) {
        <div class="toast toast-{{toast.type}}">
          <span class="toast-icon"><app-icon [name]="getIcon(toast.type)" [size]="20"></app-icon></span>
          <span class="toast-message">{{ toast.message }}</span>
          <button class="toast-close" (click)="close(toast.id)"><app-icon name="close" [size]="16"></app-icon></button>
        </div>
      }
    </div>
  `,
  styles: [`
    .toast-container {
      position: fixed;
      top: 80px;
      right: 24px;
      z-index: 9999;
      display: flex;
      flex-direction: column;
      gap: 12px;
    }

    .toast {
      display: flex;
      align-items: center;
      gap: 12px;
      min-width: 300px;
      padding: 16px;
      background: white;
      border-radius: 8px;
      box-shadow: 0 4px 16px rgba(0, 0, 0, 0.15);
      animation: slideIn 0.3s ease;
    }

    @keyframes slideIn {
      from {
        transform: translateX(400px);
        opacity: 0;
      }
      to {
        transform: translateX(0);
        opacity: 1;
      }
    }

    .toast-icon {
      font-size: 20px;
      flex-shrink: 0;
    }

    .toast-message {
      flex: 1;
      font-size: 14px;
      color: #2C3E50;
      font-weight: 500;
    }

    .toast-close {
      background: none;
      border: none;
      font-size: 18px;
      color: #7A7A7A;
      cursor: pointer;
      padding: 0;
      width: 20px;
      height: 20px;
      display: flex;
      align-items: center;
      justify-content: center;
      border-radius: 4px;
      transition: background-color 0.2s ease;

      &:hover {
        background-color: #F4F6F7;
      }
    }

    .toast-success {
      border-left: 4px solid #27AE60;
    }

    .toast-error {
      border-left: 4px solid #E74C3C;
    }

    .toast-warning {
      border-left: 4px solid #F39C12;
    }

    .toast-info {
      border-left: 4px solid #3498DB;
    }

    @media (max-width: 768px) {
      .toast-container {
        right: 12px;
        left: 12px;
      }

      .toast {
        min-width: auto;
      }
    }
  `]
})
export class ToastComponent {
  toasts = computed(() => this.toastService.getToasts()());

  constructor(private toastService: ToastService) {}

  getIcon(type: string): string {
    const icons = {
      success: 'check',
      error: 'close',
      warning: 'warning',
      info: 'info'
    };
    return icons[type as keyof typeof icons] || 'info';
  }

  close(id: number) {
    this.toastService.remove(id);
  }
}
