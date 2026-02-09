import { Injectable, signal } from '@angular/core';

export interface AppError {
  message: string;
  type: 'error' | 'warning' | 'info' | 'success';
  timestamp: Date;
}

@Injectable({ providedIn: 'root' })
export class ErrorService {
  private errorSignal = signal<AppError | null>(null);
  error = this.errorSignal.asReadonly();

  showError(message: string) {
    this.errorSignal.set({
      message,
      type: 'error',
      timestamp: new Date()
    });
    this.autoClear(5000);
  }

  showWarning(message: string) {
    this.errorSignal.set({
      message,
      type: 'warning',
      timestamp: new Date()
    });
    this.autoClear(5000);
  }

  showInfo(message: string) {
    this.errorSignal.set({
      message,
      type: 'info',
      timestamp: new Date()
    });
    this.autoClear(3000);
  }

  showSuccess(message: string) {
    this.errorSignal.set({
      message,
      type: 'success',
      timestamp: new Date()
    });
    this.autoClear(3000);
  }

  clear() {
    this.errorSignal.set(null);
  }

  private autoClear(ms: number) {
    setTimeout(() => this.clear(), ms);
  }

  // Extract message from API error
  extractMessage(error: any): string {
    if (error?.error?.message) return error.error.message;
    if (error?.message) return error.message;
    if (typeof error === 'string') return error;
    return 'An unexpected error occurred';
  }
}
