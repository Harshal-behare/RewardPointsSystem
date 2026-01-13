import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { AuthService } from '../auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {
  form!: FormGroup;
  loading = false;
  errorMsg: string | null = null;

  constructor(
    private fb: FormBuilder,
    private auth: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required]]
    });
  }

  onSubmit(): void {
    if (this.form.invalid) return;
    this.loading = true;
    this.errorMsg = null;

    this.auth.login(this.form.value).subscribe({
      next: res => {
        console.log('login response', res);
        this.loading = false;
        if (res && res.data && res.data.accessToken) {
          // Store token and user data
          localStorage.setItem('token', res.data.accessToken);
          
          // Store user data for profile display
          if (res.data.user) {
            localStorage.setItem('user', JSON.stringify(res.data.user));
          }
          
          // Redirect based on user role
          const userRoles = res.data.user?.roles || [];
          const primaryRole = res.data.user?.role || res.data.role;
          
          // Check if user has Admin role (case-insensitive)
          const isAdmin = userRoles.some((r: string) => r.toLowerCase() === 'admin') || 
                         (primaryRole && primaryRole.toLowerCase() === 'admin');
          
          if (isAdmin) {
            this.router.navigateByUrl('/admin/dashboard');
          } else {
            // Employee or other roles go to employee dashboard
            this.router.navigateByUrl('/dashboard');
          }
        } else {
          this.errorMsg = res.message || 'Login failed';
        }
      },
      error: err => {
        console.error('login error', err);
        this.loading = false;
        this.errorMsg = err?.error?.message || err?.message || 'Login request failed';
      }
    });
  }
}
