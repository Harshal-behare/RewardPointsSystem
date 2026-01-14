import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

interface Product {
  id: number;
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
  imports: [CommonModule, FormsModule],
  templateUrl: './products.component.html',
  styleUrl: './products.component.scss'
})
export class EmployeeProductsComponent implements OnInit {
  products: Product[] = [];
  filteredProducts: Product[] = [];
  selectedCategory: string = 'all';
  searchQuery: string = '';
  userPoints: number = 1850;
  selectedProduct: Product | null = null;
  showRedeemModal: boolean = false;
  deliveryAddress: string = '';

  categories = [
    { value: 'all', label: 'All Products' },
    { value: 'Electronics', label: 'Electronics' },
    { value: 'Gift Cards', label: 'Gift Cards' },
    { value: 'Home & Kitchen', label: 'Home & Kitchen' },
    { value: 'Fashion', label: 'Fashion' },
    { value: 'Books', label: 'Books' },
    { value: 'Sports', label: 'Sports & Outdoors' }
  ];

  ngOnInit(): void {
    this.loadProducts();
    this.applyFilters();
  }

  loadProducts(): void {
    this.products = [
      {
        id: 1,
        name: 'Premium Wireless Headphones',
        description: 'High-quality noise-cancelling wireless headphones with 30-hour battery life.',
        points: 500,
        image: 'https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=600',
        stock: 15,
        category: 'Electronics',
        featured: true
      },
      {
        id: 2,
        name: 'Smart Watch Series 5',
        description: 'Advanced smartwatch with fitness tracking, heart rate monitor, and GPS.',
        points: 800,
        image: 'https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=600',
        stock: 8,
        category: 'Electronics',
        featured: true
      },
      {
        id: 3,
        name: '$50 Amazon Gift Card',
        description: 'Redeem for anything on Amazon - millions of products to choose from.',
        points: 400,
        image: 'https://images.unsplash.com/photo-1606318313036-a6706adfb249?w=600',
        stock: 50,
        category: 'Gift Cards',
        featured: false
      },
      {
        id: 4,
        name: 'Coffee Maker Deluxe',
        description: 'Premium coffee maker with programmable features and thermal carafe.',
        points: 350,
        image: 'https://images.unsplash.com/photo-1517668808822-9ebb02f2a0e6?w=600',
        stock: 12,
        category: 'Home & Kitchen',
        featured: false
      },
      {
        id: 5,
        name: 'Wireless Bluetooth Speaker',
        description: '360-degree sound, waterproof design, 24-hour battery life.',
        points: 300,
        image: 'https://images.unsplash.com/photo-1608043152269-423dbba4e7e1?w=600',
        stock: 20,
        category: 'Electronics',
        featured: true
      },
      {
        id: 6,
        name: 'Professional Chef Knife Set',
        description: 'High-carbon stainless steel knife set with wooden block.',
        points: 450,
        image: 'https://images.unsplash.com/photo-1593618998160-e34014e67546?w=600',
        stock: 10,
        category: 'Home & Kitchen',
        featured: false
      },
      {
        id: 7,
        name: '$100 Visa Gift Card',
        description: 'Use anywhere Visa is accepted - the ultimate flexible reward.',
        points: 850,
        image: 'https://images.unsplash.com/photo-1580519542036-c47de6196ba5?w=600',
        stock: 30,
        category: 'Gift Cards',
        featured: true
      },
      {
        id: 8,
        name: 'Fitness Tracker Band',
        description: 'Track steps, calories, sleep, and heart rate with smartphone sync.',
        points: 250,
        image: 'https://images.unsplash.com/photo-1557935728-e6d1eae2a92f?w=600',
        stock: 25,
        category: 'Electronics',
        featured: false
      },
      {
        id: 9,
        name: 'Designer Backpack',
        description: 'Water-resistant laptop backpack with USB charging port.',
        points: 400,
        image: 'https://images.unsplash.com/photo-1553062407-98eeb64c6a62?w=600',
        stock: 18,
        category: 'Fashion',
        featured: false
      },
      {
        id: 10,
        name: 'Bestseller Book Collection',
        description: 'Set of 5 current bestselling novels across various genres.',
        points: 200,
        image: 'https://images.unsplash.com/photo-1512820790803-83ca734da794?w=600',
        stock: 40,
        category: 'Books',
        featured: false
      },
      {
        id: 11,
        name: 'Yoga Mat Premium Set',
        description: 'Eco-friendly yoga mat with carrying strap and blocks.',
        points: 180,
        image: 'https://images.unsplash.com/photo-1601925260368-ae2f83cf8b7f?w=600',
        stock: 22,
        category: 'Sports',
        featured: false
      },
      {
        id: 12,
        name: 'Mechanical Gaming Keyboard',
        description: 'RGB backlit mechanical keyboard with customizable keys.',
        points: 550,
        image: 'https://images.unsplash.com/photo-1587829741301-dc798b83add3?w=600',
        stock: 14,
        category: 'Electronics',
        featured: true
      }
    ];
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

  openRedeemModal(product: Product): void {
    this.selectedProduct = product;
    this.showRedeemModal = true;
    this.deliveryAddress = '';
  }

  closeRedeemModal(): void {
    this.showRedeemModal = false;
    this.selectedProduct = null;
    this.deliveryAddress = '';
  }

  confirmRedeem(): void {
    if (!this.selectedProduct || !this.deliveryAddress.trim()) {
      alert('Please enter a delivery address');
      return;
    }

    if (this.userPoints < this.selectedProduct.points) {
      alert('Insufficient points!');
      return;
    }

    // Process redemption
    this.userPoints -= this.selectedProduct.points;
    this.selectedProduct.stock--;
    
    console.log('Product redeemed:', this.selectedProduct.name);
    console.log('Delivery address:', this.deliveryAddress);
    
    alert('Product redeemed successfully! Check your email for confirmation.');
    this.closeRedeemModal();
  }

  canRedeem(product: Product): boolean {
    return this.userPoints >= product.points && product.stock > 0;
  }

  isOutOfStock(product: Product): boolean {
    return product.stock === 0;
  }

  isLowStock(product: Product): boolean {
    return product.stock > 0 && product.stock < 10;
  }
}
