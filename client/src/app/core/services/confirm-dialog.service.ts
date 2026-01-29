import { Injectable, signal } from '@angular/core';
import { Subject } from 'rxjs';

export interface ConfirmDialogConfig {
  title: string;
  message: string;
  confirmText?: string;
  cancelText?: string;
  type?: 'danger' | 'warning' | 'info';
}

@Injectable({
  providedIn: 'root'
})
export class ConfirmDialogService {
  // Signal for dialog visibility and config
  isOpen = signal(false);
  config = signal<ConfirmDialogConfig | null>(null);
  
  // Subject to emit user's response
  private responseSubject = new Subject<boolean>();

  /**
   * Opens a confirmation dialog and returns a Promise that resolves to true (confirmed) or false (cancelled)
   */
  confirm(config: ConfirmDialogConfig): Promise<boolean> {
    this.config.set({
      title: config.title,
      message: config.message,
      confirmText: config.confirmText || 'Confirm',
      cancelText: config.cancelText || 'Cancel',
      type: config.type || 'warning'
    });
    this.isOpen.set(true);

    return new Promise<boolean>((resolve) => {
      const subscription = this.responseSubject.subscribe((result) => {
        subscription.unsubscribe();
        resolve(result);
      });
    });
  }

  /**
   * Called when user clicks confirm button
   */
  onConfirm(): void {
    this.isOpen.set(false);
    this.responseSubject.next(true);
  }

  /**
   * Called when user clicks cancel button or closes dialog
   */
  onCancel(): void {
    this.isOpen.set(false);
    this.responseSubject.next(false);
  }

  // Convenience methods for common confirmations
  
  /**
   * Confirm deletion action
   */
  confirmDelete(itemName: string = 'this item'): Promise<boolean> {
    return this.confirm({
      title: 'Confirm Delete',
      message: `Are you sure you want to delete ${itemName}? This action cannot be undone.`,
      confirmText: 'Delete',
      cancelText: 'Cancel',
      type: 'danger'
    });
  }

  /**
   * Confirm deactivation action
   */
  confirmDeactivate(itemName: string = 'this item'): Promise<boolean> {
    return this.confirm({
      title: 'Confirm Deactivate',
      message: `Are you sure you want to deactivate ${itemName}?`,
      confirmText: 'Deactivate',
      cancelText: 'Cancel',
      type: 'warning'
    });
  }

  /**
   * Confirm activation action
   */
  confirmActivate(itemName: string = 'this item'): Promise<boolean> {
    return this.confirm({
      title: 'Confirm Activate',
      message: `Are you sure you want to activate ${itemName}?`,
      confirmText: 'Activate',
      cancelText: 'Cancel',
      type: 'info'
    });
  }

  /**
   * Confirm cancellation action (e.g., cancel an order)
   */
  confirmCancellation(message: string): Promise<boolean> {
    return this.confirm({
      title: 'Confirm Cancellation',
      message: message,
      confirmText: 'Yes, Cancel',
      cancelText: 'No, Keep',
      type: 'warning'
    });
  }
}
