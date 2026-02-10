import { Component, Input, Output, EventEmitter } from '@angular/core';

@Component({
  selector: 'app-button',
  standalone: true,
  template: `
    <button
      [type]="type"
      [disabled]="disabled"
      [class]="getButtonClasses()"
      (click)="handleClick($event)"
    >
      <ng-content></ng-content>
    </button>
  `,
  styles: [`
    button {
      padding: 12px 24px;
      border: none;
      border-radius: 8px;
      font-size: 15px;
      font-weight: 600;
      cursor: pointer;
      transition: all 0.2s ease;
      min-height: 44px;
      font-family: 'Inter', 'Poppins', sans-serif;
    }

    .btn-primary {
      background-color: #27AE60;
      color: white;
    }

    .btn-primary:hover:not(:disabled) {
      background-color: #1B8A4B;
      box-shadow: 0px 4px 16px rgba(39, 174, 96, 0.3);
    }

    .btn-secondary {
      background-color: #F4F6F7;
      color: #2C3E50;
    }

    .btn-secondary:hover:not(:disabled) {
      background-color: #E8EBED;
    }

    .btn-danger {
      background-color: #E74C3C;
      color: white;
    }

    .btn-danger:hover:not(:disabled) {
      background-color: #C0392B;
    }

    .btn-outline {
      background-color: transparent;
      color: #27AE60;
      border: 2px solid #27AE60;
    }

    .btn-outline:hover:not(:disabled) {
      background-color: #27AE60;
      color: white;
    }

    .btn-sm {
      padding: 8px 16px;
      font-size: 14px;
      min-height: 36px;
    }

    .btn-lg {
      padding: 14px 32px;
      font-size: 16px;
      min-height: 48px;
    }

    .btn-full {
      width: 100%;
    }

    button:disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }
  `]
})
export class ButtonComponent {
  @Input() type: 'button' | 'submit' | 'reset' = 'button';
  @Input() variant: 'primary' | 'secondary' | 'danger' | 'outline' = 'primary';
  @Input() size: 'sm' | 'md' | 'lg' = 'md';
  @Input() fullWidth = false;
  @Input() disabled = false;
  @Output() clicked = new EventEmitter<Event>();

  getButtonClasses(): string {
    const classes = [`btn-${this.variant}`];
    
    if (this.size !== 'md') {
      classes.push(`btn-${this.size}`);
    }
    
    if (this.fullWidth) {
      classes.push('btn-full');
    }
    
    return classes.join(' ');
  }

  handleClick(event: Event): void {
    if (!this.disabled) {
      this.clicked.emit(event);
    }
  }
}
