import { Injectable, signal } from '@angular/core';
import { extractValidationErrors } from '../models/api-response.model';

export interface Toast {
  id: number;
  message: string;
  type: 'success' | 'error' | 'info' | 'warning';
  duration?: number;
}

@Injectable({
  providedIn: 'root'
})
export class ToastService {
  private toasts = signal<Toast[]>([]);
  private nextId = 1;

  getToasts() {
    return this.toasts;
  }

  show(message: string, type: 'success' | 'error' | 'info' | 'warning' = 'info', duration = 3000) {
    const toast: Toast = {
      id: this.nextId++,
      message,
      type,
      duration
    };

    this.toasts.update(toasts => [...toasts, toast]);

    if (duration > 0) {
      setTimeout(() => this.remove(toast.id), duration);
    }
  }

  success(message: string, duration?: number) {
    this.show(message, 'success', duration);
  }

  error(message: string, duration?: number) {
    this.show(message, 'error', duration ?? 5000); // Errors shown longer by default
  }

  info(message: string, duration?: number) {
    this.show(message, 'info', duration);
  }

  warning(message: string, duration?: number) {
    this.show(message, 'warning', duration);
  }

  /**
   * Shows validation errors from API response
   * Extracts and displays all validation messages
   */
  showValidationErrors(error: any, duration?: number) {
    const messages = extractValidationErrors(error);
    messages.forEach(msg => this.error(msg, duration ?? 5000));
  }

  /**
   * Shows a single error message extracted from API response
   */
  showApiError(error: any, fallbackMessage: string = 'An error occurred', duration?: number) {
    const messages = extractValidationErrors(error);
    const message = messages.length > 0 ? messages[0] : fallbackMessage;
    this.error(message, duration ?? 5000);
  }

  remove(id: number) {
    this.toasts.update(toasts => toasts.filter(t => t.id !== id));
  }
}
