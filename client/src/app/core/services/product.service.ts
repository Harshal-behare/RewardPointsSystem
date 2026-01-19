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

  // Get all products
  getProducts(): Observable<ApiResponse<ProductDto[]>> {
    return this.api.get<ProductDto[]>('Products');
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

  // Get all product categories
  getCategories(): Observable<ApiResponse<CategoryDto[]>> {
    return this.api.get<CategoryDto[]>('Products/categories');
  }
}
