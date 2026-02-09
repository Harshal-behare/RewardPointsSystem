import { Injectable, signal, computed } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class LoadingService {
  private loadingCount = signal(0);

  isLoading = computed(() => this.loadingCount() > 0);

  show() {
    this.loadingCount.update(c => c + 1);
  }

  hide() {
    this.loadingCount.update(c => Math.max(0, c - 1));
  }

  // Wrapper for async operations
  async wrap<T>(operation: Promise<T>): Promise<T> {
    this.show();
    try {
      return await operation;
    } finally {
      this.hide();
    }
  }
}
