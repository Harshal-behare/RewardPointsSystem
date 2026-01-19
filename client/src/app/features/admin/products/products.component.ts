import { Component, OnInit, signal, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs';
import { CardComponent } from '../../../shared/components/card/card.component';
import { ButtonComponent } from '../../../shared/components/button/button.component';
import { BadgeComponent } from '../../../shared/components/badge/badge.component';
import { ProductService, ProductDto, CreateProductDto, UpdateProductDto, CategoryDto } from '../../../core/services/product.service';
import { ToastService } from '../../../core/services/toast.service';

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

@Component({
  selector: 'app-admin-products',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    CardComponent,
    ButtonComponent,
    BadgeComponent
  ],
  templateUrl: './products.component.html',
  styleUrls: ['./products.component.scss']
})
export class AdminProductsComponent implements OnInit {
  products = signal<DisplayProduct[]>([]);
  isLoading = signal(true);

  // Filter and Search
  filteredProducts = signal<DisplayProduct[]>([]);
  searchQuery = signal('');
  selectedCategory = signal<string>('all');
  categories = signal<string[]>(['all']);
  
  // Categories from database
  dbCategories = signal<CategoryDto[]>([]);

  showModal = signal(false);
  modalMode = signal<'create' | 'edit'>('create');
  selectedProduct: Partial<DisplayProduct> = {};

  viewMode = signal<'grid' | 'table'>('grid');

  constructor(
    private router: Router,
    private productService: ProductService,
    private toast: ToastService
  ) {
    // Use effect to reload data when navigating back to products
    effect(() => {
      this.router.events.pipe(
        filter(event => event instanceof NavigationEnd)
      ).subscribe(() => {
        this.loadProducts();
      });
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
    this.productService.getProducts().subscribe({
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

  onSearchChange(): void {
    this.applyFilters();
  }

  getStockStatus(stock: number): { label: string; variant: 'success' | 'warning' | 'danger' } {
    if (stock === 0) {
      return { label: 'Out of Stock', variant: 'danger' };
    } else if (stock < 5) {
      return { label: 'Low Stock', variant: 'warning' };
    }
    return { label: 'In Stock', variant: 'success' };
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
      pointsPrice: 0,
      stock: 0,
      status: 'Active'
    };
    this.showModal.set(true);
  }

  openEditModal(product: DisplayProduct): void {
    this.modalMode.set('edit');
    this.selectedProduct = { ...product };
    this.showModal.set(true);
  }

  closeModal(): void {
    this.showModal.set(false);
    this.selectedProduct = {};
  }

  // Helper to get categoryId from category name
  private getCategoryIdByName(categoryName: string): string | undefined {
    const cat = this.dbCategories().find(c => c.name === categoryName);
    return cat?.id;
  }

  saveProduct(): void {
    // Get categoryId from category name if not already set
    const categoryId = this.selectedProduct.categoryId || this.getCategoryIdByName(this.selectedProduct.category || '');
    
    if (this.modalMode() === 'create') {
      const createData: CreateProductDto = {
        name: this.selectedProduct.name || '',
        description: this.selectedProduct.description || '',
        categoryId: categoryId,
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
          this.toast.error('Failed to create product');
        }
      });
    } else {
      const updateData: UpdateProductDto = {
        name: this.selectedProduct.name,
        description: this.selectedProduct.description,
        categoryId: categoryId,
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
          this.toast.error('Failed to update product');
        }
      });
    }
  }

  deleteProduct(productId: string): void {
    if (confirm('Are you sure you want to delete this product?')) {
      this.productService.deleteProduct(productId).subscribe({
        next: (response) => {
          if (response.success) {
            this.toast.success('Product deleted successfully!');
            this.loadProducts();
          } else {
            this.toast.error(response.message || 'Failed to delete product');
          }
        },
        error: (error) => {
          console.error('Error deleting product:', error);
          this.toast.error('Failed to delete product');
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
}
