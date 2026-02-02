import { Component, OnInit, signal, DestroyRef, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CardComponent } from '../../../shared/components/card/card.component';
import { ButtonComponent } from '../../../shared/components/button/button.component';
import { BadgeComponent } from '../../../shared/components/badge/badge.component';
import { IconComponent } from '../../../shared/components/icon/icon.component';
import { ProductService, ProductDto, CreateProductDto, UpdateProductDto, CategoryDto, CreateCategoryDto, UpdateCategoryDto } from '../../../core/services/product.service';
import { ToastService } from '../../../core/services/toast.service';
import { ConfirmDialogService } from '../../../core/services/confirm-dialog.service';

interface DisplayProduct {
  id: string;
  name: string;
  description: string;
  category: string;
  categoryId?: string;
  pointsPrice: number;
  stock: number;
  imageUrl: string;
  status: 'Active' | 'Inactive';
}

interface DisplayCategory {
  id: string;
  name: string;
  description?: string;
  displayOrder: number;
  isActive: boolean;
  productCount: number;
}

@Component({
  selector: 'app-admin-products',
  standalone: true,
  imports: [
    FormsModule,
    CardComponent,
    ButtonComponent,
    BadgeComponent,
    IconComponent
  ],
  templateUrl: './products.component.html',
  styleUrl: './products.component.scss'
})
export class AdminProductsComponent implements OnInit {
    private destroyRef = inject(DestroyRef);
    
    // Validation errors for modal
    modalValidationErrors: string[] = [];

    // Client-side validators matching backend rules
    validateProductModal(): boolean {
      this.modalValidationErrors = [];
      
      // Product name: 2-200 chars
      const name = (this.selectedProduct.name || '').trim();
      if (!name) {
        this.modalValidationErrors.push('Product name is required');
      } else {
        if (name.length < 2 || name.length > 200) {
          this.modalValidationErrors.push('Product name must be between 2 and 200 characters');
        }
      }
      
      // Description: max 1000 chars
      const description = (this.selectedProduct.description || '').trim();
      if (description && description.length > 1000) {
        this.modalValidationErrors.push('Description cannot exceed 1000 characters');
      }
      
      // Points price: > 0
      if (!this.selectedProduct.pointsPrice || this.selectedProduct.pointsPrice <= 0) {
        this.modalValidationErrors.push('Points price must be greater than 0');
      }
      
      // Stock: >= 0
      if (this.selectedProduct.stock !== undefined && this.selectedProduct.stock < 0) {
        this.modalValidationErrors.push('Stock quantity cannot be negative');
      }
      
      // Image URL validation (if provided)
      const imageUrl = (this.selectedProduct.imageUrl || '').trim();
      if (imageUrl) {
        if (imageUrl.length > 500) {
          this.modalValidationErrors.push('Image URL cannot exceed 500 characters');
        }
        if (!imageUrl.startsWith('http://') && !imageUrl.startsWith('https://')) {
          this.modalValidationErrors.push('Image URL must start with http:// or https://');
        }
      }
      
      return this.modalValidationErrors.length === 0;
    }
  products = signal<DisplayProduct[]>([]);
  isLoading = signal(true);

  // Filter and Search
  filteredProducts = signal<DisplayProduct[]>([]);
  searchQuery = signal('');
  selectedCategory = signal<string>('all');
  selectedStatus = signal<string>('all'); // Status filter: 'all', 'Active', 'Inactive'
  categories = signal<string[]>(['all']);
  
  // Categories from database
  dbCategories = signal<CategoryDto[]>([]);

  // Category Management
  showCategoryModal = signal(false);
  showCategoryFormModal = signal(false);
  categoryModalMode = signal<'create' | 'edit'>('create');
  selectedCategoryItem: Partial<DisplayCategory> = {};
  categoryValidationErrors: string[] = [];
  allCategories = signal<DisplayCategory[]>([]);

  showModal = signal(false);
  modalMode = signal<'create' | 'edit'>('create');
  selectedProduct: Partial<DisplayProduct> = {};

  viewMode = signal<'grid' | 'table'>('grid');

  constructor(
    private router: Router,
    private productService: ProductService,
    private toast: ToastService,
    private confirmDialog: ConfirmDialogService
  ) {
    // Subscribe to route changes with automatic cleanup
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(() => {
      this.loadProducts();
    });
  }

  ngOnInit(): void {
    this.loadCategories();
    this.loadProducts();
  }

  loadCategories(): void {
    this.productService.getCategories().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.dbCategories.set(response.data);
          this.categories.set(['all', ...response.data.map(c => c.name)]);
        }
      },
      error: (error) => {
        console.error('Error loading categories:', error);
        // Keep default categories on error
      }
    });
  }

  loadProducts(): void {
    this.isLoading.set(true);
    // Use admin endpoint to get ALL products including inactive
    this.productService.getAllProductsAdmin().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.products.set(this.mapProductsToDisplay(response.data));
          // If categories haven't loaded from DB, extract from products
          if (this.dbCategories().length === 0) {
            const uniqueCategories = [...new Set(this.products().map(p => p.category).filter(c => c))];
            this.categories.set(['all', ...uniqueCategories]);
          }
        } else {
          this.useFallbackData();
        }
        this.applyFilters();
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading products:', error);
        this.useFallbackData();
        this.applyFilters();
        this.isLoading.set(false);
      }
    });
  }

  private mapProductsToDisplay(apiProducts: ProductDto[]): DisplayProduct[] {
    return apiProducts.map(p => ({
      id: p.id,
      name: p.name,
      description: p.description,
      category: p.categoryName || 'Uncategorized',
      categoryId: p.categoryId,
      pointsPrice: p.currentPointsCost,
      stock: p.stockQuantity,
      imageUrl: p.imageUrl || 'https://via.placeholder.com/150',
      status: p.isActive ? 'Active' : 'Inactive'
    }));
  }

  private useFallbackData(): void {
    this.products.set([
      { id: '1', name: 'Wireless Headphones', description: 'Premium noise-cancelling headphones', category: 'Electronics', pointsPrice: 5000, stock: 15, imageUrl: 'https://via.placeholder.com/150', status: 'Active' },
      { id: '2', name: 'Coffee Maker', description: 'Automatic coffee brewing machine', category: 'Appliances', pointsPrice: 3500, stock: 8, imageUrl: 'https://via.placeholder.com/150', status: 'Active' },
      { id: '3', name: 'Fitness Tracker', description: 'Smart fitness band', category: 'Wearables', pointsPrice: 2500, stock: 0, imageUrl: 'https://via.placeholder.com/150', status: 'Inactive' },
      { id: '4', name: 'Desk Lamp', description: 'LED desk lamp', category: 'Office', pointsPrice: 1500, stock: 25, imageUrl: 'https://via.placeholder.com/150', status: 'Active' },
    ]);
  }

  applyFilters(): void {
    let filtered = [...this.products()];

    // Filter by category
    if (this.selectedCategory() !== 'all') {
      filtered = filtered.filter(product => product.category === this.selectedCategory());
    }

    // Filter by status (Active/Inactive)
    if (this.selectedStatus() !== 'all') {
      filtered = filtered.filter(product => product.status === this.selectedStatus());
    }

    // Filter by search query
    if (this.searchQuery().trim()) {
      const query = this.searchQuery().toLowerCase();
      filtered = filtered.filter(product =>
        product.name.toLowerCase().includes(query) ||
        product.description.toLowerCase().includes(query) ||
        product.category.toLowerCase().includes(query)
      );
    }

    this.filteredProducts.set(filtered);
  }

  onCategoryChange(category: string): void {
    this.selectedCategory.set(category);
    this.applyFilters();
  }

  onStatusChange(status: string): void {
    this.selectedStatus.set(status);
    this.applyFilters();
  }

  onSearchChange(): void {
    this.applyFilters();
  }

  getStockStatus(stock: number): { label: string; variant: 'success' | 'warning' | 'danger' } {
    if (stock === 0) {
      return { label: 'Out of Stock', variant: 'danger' };
    } else if (stock < 5) {
      return { label: 'Low Stock', variant: 'warning' };
    }
    return { label: `${stock} units`, variant: 'success' };
  }

  handleImageError(event: Event): void {
    (event.target as HTMLImageElement).src = 'https://via.placeholder.com/150?text=No+Image';
  }

  getStatusVariant(status: string): 'success' | 'secondary' {
    return status === 'Active' ? 'success' : 'secondary';
  }

  openCreateModal(): void {
    this.modalMode.set('create');
    this.selectedProduct = {
      name: '',
      description: '',
      category: '',
      categoryId: undefined,
      pointsPrice: 1,
      stock: 0,
      imageUrl: '',
      status: 'Active'
    };
    this.showModal.set(true);
  }

  openEditModal(product: DisplayProduct): void {
    this.modalMode.set('edit');
    // Deep copy all product fields to ensure everything is editable
    this.selectedProduct = { 
      id: product.id,
      name: product.name,
      description: product.description,
      category: product.category,
      categoryId: product.categoryId,
      pointsPrice: product.pointsPrice,
      stock: product.stock,
      imageUrl: product.imageUrl,
      status: product.status
    };
    this.showModal.set(true);
  }

  closeModal(): void {
    this.showModal.set(false);
    this.selectedProduct = {};
  }

  saveProduct(): void {
    // Client-side validation
    if (!this.validateProductModal()) {
      return;
    }
    // CategoryId is now directly bound from dropdown, no need for name lookup
    if (this.modalMode() === 'create') {
      const createData: CreateProductDto = {
        name: this.selectedProduct.name || '',
        description: this.selectedProduct.description || '',
        categoryId: this.selectedProduct.categoryId,
        pointsPrice: this.selectedProduct.pointsPrice || 0,
        stockQuantity: this.selectedProduct.stock || 0,
        imageUrl: this.selectedProduct.imageUrl || 'https://via.placeholder.com/150'
      };
      this.productService.createProduct(createData).subscribe({
        next: (response) => {
          if (response.success) {
            this.toast.success('Product created successfully!');
            this.closeModal();
            this.loadProducts();
          } else {
            this.toast.error(response.message || 'Failed to create product');
          }
        },
        error: (error) => {
          console.error('Error creating product:', error);
          // Show backend validation errors
          this.toast.showValidationErrors(error);
        }
      });
    } else {
      const updateData: UpdateProductDto = {
        name: this.selectedProduct.name,
        description: this.selectedProduct.description,
        categoryId: this.selectedProduct.categoryId,
        pointsPrice: this.selectedProduct.pointsPrice,
        stockQuantity: this.selectedProduct.stock,
        imageUrl: this.selectedProduct.imageUrl,
        isActive: this.selectedProduct.status === 'Active'
      };
      this.productService.updateProduct(this.selectedProduct.id!, updateData).subscribe({
        next: (response) => {
          if (response.success) {
            this.toast.success('Product updated successfully!');
            this.closeModal();
            this.loadProducts();
          } else {
            this.toast.error(response.message || 'Failed to update product');
          }
        },
        error: (error) => {
          console.error('Error updating product:', error);
          // Show backend validation errors
          this.toast.showValidationErrors(error);
        }
      });
    }
  }

  toggleViewMode(): void {
    this.viewMode.set(this.viewMode() === 'grid' ? 'table' : 'grid');
  }

  refreshData(): void {
    this.loadProducts();
    this.toast.info('Data refreshed!');
  }

  // ============================================
  // CATEGORY MANAGEMENT METHODS
  // ============================================

  openCategoryModal(): void {
    this.loadAllCategories();
    this.showCategoryModal.set(true);
  }

  closeCategoryModal(): void {
    this.showCategoryModal.set(false);
  }

  loadAllCategories(): void {
    this.productService.getAllCategoriesAdmin().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.allCategories.set(response.data.map(c => ({
            id: c.id,
            name: c.name,
            description: c.description,
            displayOrder: c.displayOrder,
            isActive: c.isActive,
            productCount: c.productCount || 0
          })));
        }
      },
      error: (error) => {
        console.error('Error loading all categories:', error);
        this.toast.error('Failed to load categories');
      }
    });
  }

  openCreateCategoryForm(): void {
    this.categoryModalMode.set('create');
    this.selectedCategoryItem = {
      name: '',
      description: '',
      displayOrder: this.allCategories().length,
      isActive: true
    };
    this.categoryValidationErrors = [];
    this.showCategoryFormModal.set(true);
  }

  openEditCategoryForm(category: DisplayCategory): void {
    this.categoryModalMode.set('edit');
    this.selectedCategoryItem = {
      id: category.id,
      name: category.name,
      description: category.description,
      displayOrder: category.displayOrder,
      isActive: category.isActive,
      productCount: category.productCount
    };
    this.categoryValidationErrors = [];
    this.showCategoryFormModal.set(true);
  }

  closeCategoryFormModal(): void {
    this.showCategoryFormModal.set(false);
    this.selectedCategoryItem = {};
  }

  validateCategoryForm(): boolean {
    this.categoryValidationErrors = [];

    const name = (this.selectedCategoryItem.name || '').trim();
    if (!name) {
      this.categoryValidationErrors.push('Category name is required');
    } else if (name.length < 2 || name.length > 100) {
      this.categoryValidationErrors.push('Category name must be between 2 and 100 characters');
    }

    const description = (this.selectedCategoryItem.description || '').trim();
    if (description && description.length > 500) {
      this.categoryValidationErrors.push('Description cannot exceed 500 characters');
    }

    if (this.selectedCategoryItem.displayOrder !== undefined && this.selectedCategoryItem.displayOrder < 0) {
      this.categoryValidationErrors.push('Display order cannot be negative');
    }

    return this.categoryValidationErrors.length === 0;
  }

  saveCategory(): void {
    if (!this.validateCategoryForm()) {
      return;
    }

    if (this.categoryModalMode() === 'create') {
      const createData: CreateCategoryDto = {
        name: this.selectedCategoryItem.name || '',
        description: this.selectedCategoryItem.description,
        displayOrder: this.selectedCategoryItem.displayOrder || 0
      };

      this.productService.createCategory(createData).subscribe({
        next: (response) => {
          if (response.success) {
            this.toast.success('Category created successfully!');
            this.closeCategoryFormModal();
            this.loadAllCategories();
            this.loadCategories(); // Refresh the filter tabs
          } else {
            this.toast.error(response.message || 'Failed to create category');
          }
        },
        error: (error) => {
          console.error('Error creating category:', error);
          this.toast.showValidationErrors(error);
        }
      });
    } else {
      const updateData: UpdateCategoryDto = {
        name: this.selectedCategoryItem.name,
        description: this.selectedCategoryItem.description,
        displayOrder: this.selectedCategoryItem.displayOrder,
        isActive: this.selectedCategoryItem.isActive
      };

      this.productService.updateCategory(this.selectedCategoryItem.id!, updateData).subscribe({
        next: (response) => {
          if (response.success) {
            this.toast.success('Category updated successfully!');
            this.closeCategoryFormModal();
            this.loadAllCategories();
            this.loadCategories(); // Refresh the filter tabs
            this.loadProducts(); // Refresh products in case category name changed
          } else {
            this.toast.error(response.message || 'Failed to update category');
          }
        },
        error: (error) => {
          console.error('Error updating category:', error);
          this.toast.showValidationErrors(error);
        }
      });
    }
  }

  async deleteCategory(category: DisplayCategory): Promise<void> {
    const warningMessage = category.productCount > 0 
      ? `category "${category.name}" (${category.productCount} product(s) will become uncategorized)`
      : `category "${category.name}"`;
    
    const confirmed = await this.confirmDialog.confirmDelete(warningMessage);
    if (confirmed) {
      this.productService.deleteCategory(category.id).subscribe({
        next: (response) => {
          if (response.success) {
            this.toast.success(response.message || `Category '${category.name}' deleted successfully!`);
            this.loadAllCategories();
            this.loadCategories(); // Refresh the filter tabs
            this.loadProducts(); // Refresh products as some may now be uncategorized
          } else {
            this.toast.error(response.message || 'Failed to delete category');
          }
        },
        error: (error) => {
          console.error('Error deleting category:', error);
          this.toast.showApiError(error, 'Failed to delete category');
        }
      });
    }
  }

  toggleCategoryStatus(category: DisplayCategory): void {
    const updateData: UpdateCategoryDto = {
      isActive: !category.isActive
    };

    this.productService.updateCategory(category.id, updateData).subscribe({
      next: (response) => {
        if (response.success) {
          this.toast.success(`Category '${category.name}' ${category.isActive ? 'deactivated' : 'activated'} successfully!`);
          this.loadAllCategories();
          this.loadCategories();
        } else {
          this.toast.error(response.message || 'Failed to update category');
        }
      },
      error: (error) => {
        console.error('Error toggling category status:', error);
        this.toast.showApiError(error, 'Failed to update category');
      }
    });
  }
}
