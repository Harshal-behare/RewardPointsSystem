import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-badge',
  standalone: true,
  template: `
    <span [class]="getBadgeClasses()">
      <ng-content></ng-content>
    </span>
  `,
  styles: [`
    .badge {
      display: inline-flex;
      align-items: center;
      padding: 4px 12px;
      border-radius: 16px;
      font-size: 13px;
      font-weight: 600;
      white-space: nowrap;
    }

    .badge-success {
      background-color: #D5F4E6;
      color: #1B8A4B;
    }

    .badge-warning {
      background-color: #FEF5E7;
      color: #D68910;
    }

    .badge-info {
      background-color: #EBF5FB;
      color: #2874A6;
    }

    .badge-danger {
      background-color: #FADBD8;
      color: #C0392B;
    }

    .badge-secondary {
      background-color: #F4F6F7;
      color: #7A7A7A;
    }

    .badge-sm {
      padding: 2px 8px;
      font-size: 12px;
    }

    .badge-lg {
      padding: 6px 16px;
      font-size: 14px;
    }
  `]
})
export class BadgeComponent {
  @Input() variant: 'success' | 'warning' | 'info' | 'danger' | 'secondary' = 'secondary';
  @Input() size: 'sm' | 'md' | 'lg' = 'md';

  getBadgeClasses(): string {
    const classes = ['badge', `badge-${this.variant}`];
    
    if (this.size !== 'md') {
      classes.push(`badge-${this.size}`);
    }
    
    return classes.join(' ');
  }
}
