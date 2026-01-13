import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-kpi-card',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="kpi-card">
      <div class="kpi-icon">
        <span>{{ icon }}</span>
      </div>
      <div class="kpi-content">
        <div class="kpi-label">{{ label }}</div>
        <div class="kpi-value">{{ value }}</div>
        <div class="kpi-trend" *ngIf="trend">
          <span [class]="getTrendClass()">
            {{ trend > 0 ? '↑' : '↓' }} {{ Math.abs(trend) }}%
          </span>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .kpi-card {
      background: white;
      border-radius: 12px;
      padding: 24px;
      display: flex;
      align-items: center;
      gap: 16px;
      box-shadow: 0px 4px 12px rgba(0, 0, 0, 0.05);
      transition: transform 0.2s ease, box-shadow 0.2s ease;
    }

    .kpi-card:hover {
      transform: translateY(-2px);
      box-shadow: 0px 6px 16px rgba(0, 0, 0, 0.1);
    }

    .kpi-icon {
      width: 56px;
      height: 56px;
      border-radius: 12px;
      background-color: #D5F4E6;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 28px;
    }

    .kpi-content {
      flex: 1;
    }

    .kpi-label {
      font-size: 14px;
      color: #7A7A7A;
      margin-bottom: 4px;
    }

    .kpi-value {
      font-size: 28px;
      font-weight: 700;
      color: #2C3E50;
    }

    .kpi-trend {
      margin-top: 4px;
      font-size: 13px;
      font-weight: 600;
    }

    .trend-positive {
      color: #27AE60;
    }

    .trend-negative {
      color: #E74C3C;
    }
  `]
})
export class KpiCardComponent {
  @Input() icon = '📊';
  @Input() label = '';
  @Input() value: string | number = 0;
  @Input() trend?: number;

  Math = Math;

  getTrendClass(): string {
    return this.trend && this.trend > 0 ? 'trend-positive' : 'trend-negative';
  }
}
