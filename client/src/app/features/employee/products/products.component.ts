import { Component, OnInit, signal } from '@angular/core';
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

  // Client-side validation for redemption
  validateRedemption(): boolean {
    const errors: string[] = [];
    const product = this.selectedProduct();
    const quantity = this.redemptionQuantity();
    
    // Quantity validation
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
}
