import { Component, OnInit, signal, computed } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ProductService, ProductDto, CategoryDto } from '../../../core/services/product.service';
import { RedemptionService, CreateRedemptionDto, RedemptionDto } from '../../../core/services/redemption.service';
import { PointsService, PointsAccountDto } from '../../../core/services/points.service';
import { AuthService } from '../../../auth/auth.service';
import { ToastService } from '../../../core/services/toast.service';
import { IconComponent } from '../../../shared/components/icon/icon.component';

interface Product {
  id: string;
  name: string;
  description: string;
  points: number;
  image: string;
  stock: number;
  category: string;
  featured: boolean;
}

@Component({
  selector: 'app-employee-products',
  standalone: true,
  imports: [FormsModule, IconComponent],
  templateUrl: './products.component.html',
  styleUrl: './products.component.scss'
})
export class EmployeeProductsComponent implements OnInit {
  // Expose Math for template use
  Math = Math;
  
  products = signal<Product[]>([]);
  filteredProducts = signal<Product[]>([]);
  selectedCategory = signal<string>('all');
  searchQuery = signal<string>('');
  
  // Sorting filters (same as admin)
  sortBy = signal<'none' | 'stock' | 'points'>('none');
  sortOrder = signal<'asc' | 'desc'>('asc');
  userPoints = signal<number>(0);
  selectedProduct = signal<Product | null>(null);
  showRedeemModal = signal<boolean>(false);
  redemptionQuantity = signal<number>(1);
  redemptionValidationErrors = signal<string[]>([]);
  isLoading = signal<boolean>(true);
  currentUserId: string = '';
  
  // Pagination
  currentPage = signal(1);
  pageSize = signal(12);
  
  // Computed paginated products
  paginatedProducts = computed(() => {
    const filtered = this.filteredProducts();
    const start = (this.currentPage() - 1) * this.pageSize();
    const end = start + this.pageSize();
    return filtered.slice(start, end);
  });
  
  totalPages = computed(() => Math.ceil(this.filteredProducts().length / this.pageSize()));
  
  // Track pending redemption product IDs
  pendingRedemptionProductIds = signal<Set<string>>(new Set());

  // Dynamic categories loaded from database
  categories = signal<{ value: string; label: string }[]>([
    { value: 'all', label: 'All Products' }
  ]);

  constructor(
    private productService: ProductService,
    private redemptionService: RedemptionService,
    private pointsService: PointsService,
    private authService: AuthService,
    private toast: ToastService
  ) {
    // Get current user ID from JWT token
    const token = this.authService.getToken();
    if (token) {
      try {
        const payload = JSON.parse(atob(token.split('.')[1]));
        this.currentUserId = payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] || 
                            payload.sub || 
                            payload.userId || '';
      } catch (e) {
        console.error('Error parsing token:', e);
      }
    }
  }

  ngOnInit(): void {
    this.loadCategories();
    this.loadProducts();
    this.loadUserPoints();
    this.loadPendingRedemptions();
  }

  loadPendingRedemptions(): void {
    this.redemptionService.getMyRedemptions().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          // Get all product IDs that have pending redemptions
          const pendingIds = new Set<string>();
          const redemptions = Array.isArray(response.data) ? response.data : 
                              (response.data as any).items || [];
          redemptions.forEach((r: RedemptionDto) => {
            if (r.status === 'Pending') {
              pendingIds.add(r.productId);
            }
          });
          this.pendingRedemptionProductIds.set(pendingIds);
        }
      },
      error: (error) => {
        console.error('Error loading pending redemptions:', error);
      }
    });
  }

  loadCategories(): void {
    this.productService.getCategories().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          const dynamicCategories = response.data.map((c: CategoryDto) => ({
            value: c.name,
            label: c.name
          }));
          this.categories.set([
            { value: 'all', label: 'All Products' },
            ...dynamicCategories
          ]);
        }
      },
      error: (error) => {
        console.error('Error loading categories:', error);
        // Keep default 'All Products' category on error
      }
    });
  }

  loadProducts(): void {
    this.isLoading.set(true);
    this.productService.getProducts().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          const productsData = Array.isArray(response.data) ? response.data : 
                               (response.data as any).items || [];
          const mappedProducts = productsData
            .filter((p: ProductDto) => p.isActive && p.isInStock)
            .map((p: ProductDto) => ({
              id: p.id,
              name: p.name,
              description: p.description || '',
              points: p.currentPointsCost,
              image: p.imageUrl || 'https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=600',
              stock: p.stockQuantity || 0,
              category: p.categoryName || 'General',
              featured: false
            }));
          this.products.set(mappedProducts);
          this.applyFilters();
        }
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading products:', error);
        this.toast.error('Failed to load products');
        this.isLoading.set(false);
      }
    });
  }

  loadUserPoints(): void {
    if (!this.currentUserId) return;

    this.pointsService.getPointsAccount(this.currentUserId).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.userPoints.set(response.data.currentBalance);
        }
      },
      error: (error) => {
        console.error('Error loading user points:', error);
      }
    });
  }

  applyFilters(): void {
    let filtered = [...this.products()];

    // Filter by category
    if (this.selectedCategory() !== 'all') {
      filtered = filtered.filter(product => product.category === this.selectedCategory());
    }

    // Filter by search query
    const query = this.searchQuery().trim();
    if (query) {
      const lowerQuery = query.toLowerCase();
      filtered = filtered.filter(product =>
        product.name.toLowerCase().includes(lowerQuery) ||
        product.description.toLowerCase().includes(lowerQuery) ||
        product.category.toLowerCase().includes(lowerQuery)
      );
    }

    // Apply sorting
    const sortBy = this.sortBy();
    const sortOrder = this.sortOrder();
    
    if (sortBy !== 'none') {
      filtered.sort((a, b) => {
        let comparison = 0;
        if (sortBy === 'stock') {
          comparison = a.stock - b.stock;
        } else if (sortBy === 'points') {
          comparison = a.points - b.points;
        }
        return sortOrder === 'asc' ? comparison : -comparison;
      });
    }

    this.filteredProducts.set(filtered);
    // Reset to first page when filters change
    this.currentPage.set(1);
  }

  onSortChange(sortBy: 'none' | 'stock' | 'points'): void {
    if (this.sortBy() === sortBy) {
      // Toggle order if same sort field clicked
      this.sortOrder.set(this.sortOrder() === 'asc' ? 'desc' : 'asc');
    } else {
      this.sortBy.set(sortBy);
      this.sortOrder.set('asc');
    }
    this.applyFilters();
  }

  clearSort(): void {
    this.sortBy.set('none');
    this.sortOrder.set('asc');
    this.currentPage.set(1);
    this.applyFilters();
  }

  onCategoryChange(category: string): void {
    this.selectedCategory.set(category);
    this.applyFilters();
  }

  onSearchChange(value: string): void {
    this.searchQuery.set(value);
    this.applyFilters();
  }
  
  // Pagination methods
  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages()) {
      this.currentPage.set(page);
    }
  }
  
  nextPage(): void {
    if (this.currentPage() < this.totalPages()) {
      this.currentPage.set(this.currentPage() + 1);
    }
  }
  
  previousPage(): void {
    if (this.currentPage() > 1) {
      this.currentPage.set(this.currentPage() - 1);
    }
  }
  
  onPageSizeChange(size: number): void {
    this.pageSize.set(size);
    this.currentPage.set(1);
  }
  
  getPageNumbers(): number[] {
    const total = this.totalPages();
    const current = this.currentPage();
    const pages: number[] = [];
    
    if (total <= 7) {
      for (let i = 1; i <= total; i++) pages.push(i);
    } else {
      pages.push(1);
      if (current > 3) pages.push(-1); // ellipsis
      for (let i = Math.max(2, current - 1); i <= Math.min(total - 1, current + 1); i++) {
        pages.push(i);
      }
      if (current < total - 2) pages.push(-1); // ellipsis
      pages.push(total);
    }
    return pages;
  }

  openRedeemModal(product: Product): void {
    this.selectedProduct.set(product);
    this.showRedeemModal.set(true);
    this.redemptionQuantity.set(1);
    this.redemptionValidationErrors.set([]);
  }

  closeRedeemModal(): void {
    this.showRedeemModal.set(false);
    this.selectedProduct.set(null);
    this.redemptionQuantity.set(1);
    this.redemptionValidationErrors.set([]);
  }
  /**
 * Always keep quantity as integer and between 1 and 10
 */
onQuantityChange(value: any): void {
  // If input is cleared or invalid → default to 1
  if (value === '' || value === null || value === undefined) {
    this.redemptionQuantity.set(1);
    return;
  }

  let qty = Number(value);

  // NaN / Infinity protection
  if (!Number.isFinite(qty)) {
    this.redemptionQuantity.set(1);
    return;
  }

  // Force integer (no decimals)
  qty = Math.trunc(qty);

  // Clamp between 1 and 10
  qty = Math.max(1, Math.min(10, qty));

  this.redemptionQuantity.set(qty);
}

/**
 * Prevent typing invalid characters and prevent typing > 10
 */
restrictQuantityKeys(event: KeyboardEvent): void {
  const blocked = ['e', 'E', '+', '-', '.', ',', ' '];

  // Allow navigation/edit keys
  const allowedControl = [
    'Backspace', 'Delete', 'Tab', 'Escape', 'Enter',
    'ArrowLeft', 'ArrowRight', 'Home', 'End'
  ];

  if (allowedControl.includes(event.key)) return;

  // Block invalid characters
  if (blocked.includes(event.key)) {
    event.preventDefault();
    return;
  }

  // Allow only digits
  if (!/^\d$/.test(event.key)) {
    event.preventDefault();
    return;
  }

  // Prevent typing a value > 10 (example: block 11)
  const input = event.target as HTMLInputElement;
  const selectionStart = input.selectionStart ?? input.value.length;
  const selectionEnd = input.selectionEnd ?? input.value.length;

  const nextValue =
    input.value.slice(0, selectionStart) +
    event.key +
    input.value.slice(selectionEnd);

  const nextNum = Number(nextValue);

  if (Number.isFinite(nextNum) && nextNum > 10) {
    event.preventDefault();
  }
}

/**
 * Prevent pasting values outside 1..10 or non-numeric
 */
restrictQuantityPaste(event: ClipboardEvent): void {
  const pasted = event.clipboardData?.getData('text') ?? '';

  // Only digits
  if (!/^\d+$/.test(pasted)) {
    event.preventDefault();
    return;
  }

  const num = Number(pasted);
  if (!Number.isFinite(num) || num < 1 || num > 10) {
    event.preventDefault();
  }
}

/**
 * Prevent mouse wheel changing number input
 */
disableNumberWheel(event: WheelEvent): void {
  (event.target as HTMLInputElement).blur();
  event.preventDefault();
}

  // Client-side validation for redemption
  validateRedemption(): boolean {
    const errors: string[] = [];
    const product = this.selectedProduct();
    const quantity = this.redemptionQuantity();
    
    // Quantity validation - must be integer
    if (!Number.isInteger(quantity)) {
      errors.push('Quantity must be a whole number');
    }
    if (quantity < 1) {
      errors.push('Quantity must be at least 1');
    }
    if (quantity > 10) {
      errors.push('Quantity cannot exceed 10 items per redemption');
    }
    
    // Points validation
    if (product && this.userPoints() < product.points * quantity) {
      errors.push('Insufficient points for this redemption');
    }
    
    // Stock validation
    if (product && product.stock < quantity) {
      errors.push('Not enough stock available');
    }
    
    this.redemptionValidationErrors.set(errors);
    return errors.length === 0;
  }

  confirmRedeem(): void {
    // Client-side validation
    if (!this.validateRedemption()) {
      return;
    }
    
    const product = this.selectedProduct();
    if (!product) return;

    if (!this.currentUserId) {
      this.toast.error('User not authenticated');
      return;
    }

    const redemptionData: CreateRedemptionDto = {
      userId: this.currentUserId,
      productId: product.id,
      quantity: this.redemptionQuantity()
    };

    this.redemptionService.createRedemption(redemptionData).subscribe({
      next: (response) => {
        if (response.success) {
          this.closeRedeemModal();
          this.toast.success('Redemption request submitted! Your request is pending admin approval. You will be notified once it is processed.');
          this.loadProducts();
          this.loadUserPoints();
          this.loadPendingRedemptions(); // Reload pending redemptions
        } else {
          this.toast.error(response.message || 'Failed to redeem product');
        }
      },
      error: (error) => {
        console.error('Error redeeming product:', error);
        // Show backend validation errors (e.g., insufficient points, out of stock, quantity limits)
        this.toast.showValidationErrors(error);
      }
    });
  }

  canRedeem(product: Product): boolean {
    return this.userPoints() >= product.points && 
           product.stock > 0 && 
           !this.hasPendingRedemption(product);
  }

  hasPendingRedemption(product: Product): boolean {
    return this.pendingRedemptionProductIds().has(product.id);
  }

  isOutOfStock(product: Product): boolean {
    return product.stock === 0;
  }

  isLowStock(product: Product): boolean {
    return product.stock > 0 && product.stock < 10;
  }

  // Prevent decimal input in quantity field
  preventDecimalInput(event: KeyboardEvent): void {
    // Block decimal point and 'e' (scientific notation)
    if (event.key === '.' || event.key === ',' || event.key === 'e' || event.key === 'E') {
      event.preventDefault();
    }
  }
}
