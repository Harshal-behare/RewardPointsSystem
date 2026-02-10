import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-card',
  standalone: true,
  template: `
    <div [class]="getCardClasses()">
      @if (title || hasHeaderContent) {
        <div class="card-header">
          @if (title) {
            <h3>{{ title }}</h3>
          }
          <ng-content select="[header]"></ng-content>
        </div>
      }
      <div class="card-body">
        <ng-content></ng-content>
      </div>
      @if (hasFooterContent) {
        <div class="card-footer">
          <ng-content select="[footer]"></ng-content>
        </div>
      }
    </div>
  `,
  styles: [`
    .card {
      background-color: #FFFFFF;
      border-radius: 12px;
      box-shadow: 0px 4px 12px rgba(0, 0, 0, 0.05);
      overflow: hidden;
      transition: box-shadow 0.2s ease;
    }

    .card-hoverable:hover {
      box-shadow: 0px 6px 16px rgba(0, 0, 0, 0.1);
    }

    .card-header {
      padding: 24px;
      border-bottom: 1px solid #F4F6F7;
      display: flex;
      justify-content: space-between;
      align-items: center;
    }

    .card-header h3 {
      margin: 0;
      font-size: 20px;
      font-weight: 600;
      color: #2C3E50;
    }

    .card-body {
      padding: 24px;
    }

    .card-footer {
      padding: 16px 24px;
      background-color: #F4F6F7;
      border-top: 1px solid #E8EBED;
    }

    .card-compact .card-body {
      padding: 16px;
    }

    .card-compact .card-header {
      padding: 16px;
    }
  `]
})
export class CardComponent {
  @Input() title?: string;
  @Input() hoverable = false;
  @Input() compact = false;
  @Input() hasHeaderContent = false;
  @Input() hasFooterContent = false;

  getCardClasses(): string {
    const classes = ['card'];
    
    if (this.hoverable) {
      classes.push('card-hoverable');
    }
    
    if (this.compact) {
      classes.push('card-compact');
    }
    
    return classes.join(' ');
  }
}
