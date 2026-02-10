import { Component, inject } from '@angular/core';
import { ConfirmDialogService } from '../../../core/services/confirm-dialog.service';

@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  imports: [],
  templateUrl: './confirm-dialog.component.html',
  styleUrl: './confirm-dialog.component.scss'
})
export class ConfirmDialogComponent {
  protected dialogService = inject(ConfirmDialogService);

  onConfirm(): void {
    this.dialogService.onConfirm();
  }

  onCancel(): void {
    this.dialogService.onCancel();
  }

  onOverlayClick(event: MouseEvent): void {
    // Only close if clicking directly on the overlay, not the dialog content
    if (event.target === event.currentTarget) {
      this.onCancel();
    }
  }

  // Handle escape key to close
  onKeydown(event: KeyboardEvent): void {
    if (event.key === 'Escape') {
      this.onCancel();
    }
  }
}
