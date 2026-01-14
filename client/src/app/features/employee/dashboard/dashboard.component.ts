import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

interface PointsBalance {
  total: number;
  available: number;
  pending: number;
  redeemed: number;
}

interface Event {
  id: number;
  name: string;
  date: string;
  location: string;
  points: number;
  image: string;
  status: 'upcoming' | 'ongoing' | 'completed';
  registered: boolean;
}

interface Product {
  id: number;
  name: string;
  points: number;
  image: string;
  stock: number;
  category: string;
}

interface Transaction {
  id: number;
  type: 'earned' | 'redeemed' | 'pending';
  description: string;
  points: number;
  date: string;
  status: string;
}

@Component({
  selector: 'app-employee-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class EmployeeDashboardComponent implements OnInit {
  pointsBalance: PointsBalance = {
    total: 2450,
    available: 1850,
    pending: 350,
    redeemed: 600
  };

  upcomingEvents: Event[] = [
    {
      id: 1,
      name: 'Annual Sales Conference 2024',
      date: '2024-02-15',
      location: 'Grand Hall, Building A',
      points: 500,
      image: 'https://images.unsplash.com/photo-1540575467063-178a50c2df87?w=400',
      status: 'upcoming',
      registered: true
    },
    {
      id: 2,
      name: 'Team Building Workshop',
      date: '2024-02-20',
      location: 'Conference Room 3',
      points: 300,
      image: 'https://images.unsplash.com/photo-1511578314322-379afb476865?w=400',
      status: 'upcoming',
      registered: false
    },
    {
      id: 3,
      name: 'Product Launch Event',
      date: '2024-02-25',
      location: 'Virtual Event',
      points: 400,
      image: 'https://images.unsplash.com/photo-1505373877841-8d25f7d46678?w=400',
      status: 'upcoming',
      registered: false
    }
  ];

  featuredProducts: Product[] = [
    {
      id: 1,
      name: 'Premium Wireless Headphones',
      points: 500,
      image: 'https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=400',
      stock: 15,
      category: 'Electronics'
    },
    {
      id: 2,
      name: 'Smart Watch Series 5',
      points: 800,
      image: 'https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=400',
      stock: 8,
      category: 'Electronics'
    },
    {
      id: 3,
      name: '$50 Amazon Gift Card',
      points: 400,
      image: 'https://images.unsplash.com/photo-1606318313036-a6706adfb249?w=400',
      stock: 50,
      category: 'Gift Cards'
    },
    {
      id: 4,
      name: 'Coffee Maker Deluxe',
      points: 350,
      image: 'https://images.unsplash.com/photo-1517668808822-9ebb02f2a0e6?w=400',
      stock: 12,
      category: 'Home & Kitchen'
    }
  ];

  recentTransactions: Transaction[] = [
    {
      id: 1,
      type: 'earned',
      description: 'Attended Q4 Sales Meeting',
      points: 250,
      date: '2024-01-28',
      status: 'Completed'
    },
    {
      id: 2,
      type: 'redeemed',
      description: 'Redeemed: Wireless Mouse',
      points: -150,
      date: '2024-01-25',
      status: 'Approved'
    },
    {
      id: 3,
      type: 'earned',
      description: 'Completed Training Module',
      points: 100,
      date: '2024-01-22',
      status: 'Completed'
    },
    {
      id: 4,
      type: 'pending',
      description: 'Pending: Team Building Event',
      points: 350,
      date: '2024-01-20',
      status: 'Pending'
    },
    {
      id: 5,
      type: 'earned',
      description: 'Won Innovation Challenge',
      points: 500,
      date: '2024-01-15',
      status: 'Completed'
    }
  ];

  constructor(private router: Router) {}

  ngOnInit(): void {
    // Load user data
  }

  navigateToEvents(): void {
    this.router.navigate(['/employee/events']);
  }

  navigateToProducts(): void {
    this.router.navigate(['/employee/products']);
  }

  navigateToAccount(): void {
    this.router.navigate(['/employee/account']);
  }

  registerForEvent(event: Event): void {
    event.registered = true;
    console.log('Registered for event:', event.name);
  }

  redeemProduct(product: Product): void {
    if (this.pointsBalance.available >= product.points) {
      console.log('Redeeming product:', product.name);
      this.router.navigate(['/employee/products']);
    } else {
      alert('Insufficient points!');
    }
  }
}
