import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CardComponent } from '../../../shared/components/card/card.component';
import { ButtonComponent } from '../../../shared/components/button/button.component';
import { BadgeComponent } from '../../../shared/components/badge/badge.component';
import { Product } from '../../../core/models/product.model';

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
  products: Product[] = [
    {
      id: 1,
      name: 'Wireless Headphones',
      description: 'Premium noise-cancelling headphones',
      category: 'Electronics',
      pointsPrice: 5000,
      stock: 15,
      imageUrl: 'https://via.placeholder.com/150',
      status: 'Active'
    },
    {
      id: 2,
      name: 'Coffee Maker',
      description: 'Automatic coffee brewing machine',
      category: 'Appliances',
      pointsPrice: 3500,
      stock: 8,
      imageUrl: 'https://via.placeholder.com/150',
      status: 'Active'
    },
    {
      id: 3,
      name: 'Fitness Tracker',
      description: 'Smart fitness band with heart rate monitor',
      category: 'Wearables',
      pointsPrice: 2500,
      stock: 0,
      imageUrl: 'https://via.placeholder.com/150',
      status: 'Inactive'
    },
    {
      id: 4,
      name: 'Desk Lamp',
      description: 'LED desk lamp with adjustable brightness',
      category: 'Office',
      pointsPrice: 1500,
      stock: 25,
      imageUrl: 'https://via.placeholder.com/150',
      status: 'Active'
    },
    {
      id: 5,
      name: 'Backpack',
      description: 'Water-resistant laptop backpack',
      category: 'Accessories',
      pointsPrice: 2000,
      stock: 12,
      imageUrl: 'https://via.placeholder.com/150',
      status: 'Active'
    },
    {
      id: 6,
      name: 'Bluetooth Speaker',
      description: 'Portable wireless speaker',
      category: 'Electronics',
      pointsPrice: 3000,
      stock: 3,
      imageUrl: 'https://via.placeholder.com/150',
      status: 'Active'
    },
  ];

  // Filter and Search
  filteredProducts: Product[] = [];
  searchQuery = '';
  selectedCategory: string = 'all';
  categories = ['all', 'Electronics', 'Appliances', 'Wearables', 'Office', 'Accessories'];

  showModal = false;
  modalMode: 'create' | 'edit' = 'create';
  selectedProduct: Partial<Product> = {};

  viewMode: 'grid' | 'table' = 'grid';

  ngOnInit(): void {
    // Load products from API
    this.applyFilters();
  }

  applyFilters(): void {
    let filtered = [...this.products];

    // Filter by category
    if (this.selectedCategory !== 'all') {
      filtered = filtered.filter(product => product.category === this.selectedCategory);
    }

    // Filter by search query
    if (this.searchQuery.trim()) {
      const query = this.searchQuery.toLowerCase();
      filtered = filtered.filter(product =>
        product.name.toLowerCase().includes(query) ||
        product.description.toLowerCase().includes(query) ||
        product.category.toLowerCase().includes(query)
      );
    }

    this.filteredProducts = filtered;
  }

  onCategoryChange(category: string): void {
    this.selectedCategory = category;
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
    this.modalMode = 'create';
    this.selectedProduct = {
      name: '',
      description: '',
      category: '',
      pointsPrice: 0,
      stock: 0,
      status: 'Active'
    };
    this.showModal = true;
  }

  openEditModal(product: Product): void {
    this.modalMode = 'edit';
    this.selectedProduct = { ...product };
    this.showModal = true;
  }

  closeModal(): void {
    this.showModal = false;
    this.selectedProduct = {};
  }

  saveProduct(): void {
    if (this.modalMode === 'create') {
      console.log('Creating product:', this.selectedProduct);
      // TODO: Call API to create product
      // Add new product to the list with a generated ID
      const newProduct: Product = {
        id: Math.max(...this.products.map(p => p.id)) + 1,
        name: this.selectedProduct.name || '',
        description: this.selectedProduct.description || '',
        category: this.selectedProduct.category || '',
        pointsPrice: this.selectedProduct.pointsPrice || 0,
        stock: this.selectedProduct.stock || 0,
        imageUrl: this.selectedProduct.imageUrl || 'https://via.placeholder.com/150',
        status: this.selectedProduct.status || 'Active'
      };
      this.products.unshift(newProduct);
      alert('Product added successfully!');
    } else {
      console.log('Updating product:', this.selectedProduct);
      // TODO: Call API to update product
      const index = this.products.findIndex(p => p.id === this.selectedProduct.id);
      if (index !== -1) {
        this.products[index] = { ...this.selectedProduct } as Product;
      }
      alert('Product updated successfully!');
    }
    this.closeModal();
  }

  deleteProduct(productId: number): void {
    if (confirm('Are you sure you want to delete this product?')) {
      console.log('Deleting product:', productId);
      this.products = this.products.filter(p => p.id !== productId);
    }
  }

  toggleViewMode(): void {
    this.viewMode = this.viewMode === 'grid' ? 'table' : 'grid';
  }

  // Quick Action Methods
  importProducts(): void {
    // TODO: Implement import functionality
    console.log('Importing products...');
    alert('📤 Products import feature coming soon!');
  }

  exportProducts(): void {
    // TODO: Implement export functionality
    console.log('Exporting products...');
    alert('📥 Products export feature coming soon!');
  }

  refreshData(): void {
    // TODO: Reload products from API
    console.log('Refreshing products data...');
    this.applyFilters();
    alert('🔄 Data refreshed!');
  }
}
