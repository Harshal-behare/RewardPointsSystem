import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { ApiResponse } from '../models/api-response.model';

// Product Interfaces
export interface ProductDto {
  id: string;
  name: string;
  description: string;
  categoryId?: string;
  categoryName?: string;
  imageUrl?: string;
  currentPointsCost: number;
  isActive: boolean;
  isInStock: boolean;
  stockQuantity: number;
  createdAt: string;
}

export interface CategoryDto {
  id: string;
  name: string;
  description?: string;
  displayOrder: number;
  isActive: boolean;
  productCount?: number;
}

export interface CreateCategoryDto {
  name: string;
  description?: string;
  displayOrder: number;
}

export interface UpdateCategoryDto {
  name?: string;
  description?: string;
  displayOrder?: number;
  isActive?: boolean;
}

export interface CreateProductDto {
  name: string;
  description: string;
  categoryId?: string;
  imageUrl?: string;
  pointsPrice: number;
  stockQuantity: number;
}

export interface UpdateProductDto {
  name?: string;
  description?: string;
  categoryId?: string;
  imageUrl?: string;
  pointsPrice?: number;
  stockQuantity?: number;
  isActive?: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class ProductService {
  constructor(private api: ApiService) {}

  // Get all active products (for employees)
  getProducts(): Observable<ApiResponse<ProductDto[]>> {
    return this.api.get<ProductDto[]>('Products');
  }

  // Get all products including inactive (for admin)
  getAllProductsAdmin(): Observable<ApiResponse<ProductDto[]>> {
    return this.api.get<ProductDto[]>('Products/admin/all');
  }

  // Get product by ID
  getProductById(id: string): Observable<ApiResponse<ProductDto>> {
    return this.api.get<ProductDto>(`Products/${id}`);
  }

  // Create new product
  createProduct(data: CreateProductDto): Observable<ApiResponse<ProductDto>> {
    return this.api.post<ProductDto>('Products', data);
  }

  // Update product
  updateProduct(id: string, data: UpdateProductDto): Observable<ApiResponse<ProductDto>> {
    return this.api.put<ProductDto>(`Products/${id}`, data);
  }

  // Delete (deactivate) product
  deleteProduct(id: string): Observable<ApiResponse<void>> {
    return this.api.delete<void>(`Products/${id}`);
  }

  // Get products by category
  getProductsByCategory(categoryId: string): Observable<ApiResponse<ProductDto[]>> {
    return this.api.get<ProductDto[]>(`Products/category/${categoryId}`);
  }

  // Get all product categories (active only)
  getCategories(): Observable<ApiResponse<CategoryDto[]>> {
    return this.api.get<CategoryDto[]>('Products/categories');
  }

  // Get all categories including inactive (for admin)
  getAllCategoriesAdmin(): Observable<ApiResponse<CategoryDto[]>> {
    return this.api.get<CategoryDto[]>('Products/categories/admin/all');
  }

  // Create new category (admin only)
  createCategory(data: CreateCategoryDto): Observable<ApiResponse<CategoryDto>> {
    return this.api.post<CategoryDto>('Products/categories', data);
  }

  // Update category (admin only)
  updateCategory(id: string, data: UpdateCategoryDto): Observable<ApiResponse<CategoryDto>> {
    return this.api.put<CategoryDto>(`Products/categories/${id}`, data);
  }

  // Delete category (admin only)
  deleteCategory(id: string): Observable<ApiResponse<void>> {
    return this.api.delete<void>(`Products/categories/${id}`);
  }
}
