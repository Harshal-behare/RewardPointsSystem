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

  showModal = false;
  modalMode: 'create' | 'edit' = 'create';
  selectedProduct: Partial<Product> = {};

  viewMode: 'grid' | 'table' = 'grid';

  ngOnInit(): void {
    // Load products from API
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
    } else {
      console.log('Updating product:', this.selectedProduct);
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
}
