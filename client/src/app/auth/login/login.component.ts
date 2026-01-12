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
          this.router.navigateByUrl('/dashboard');
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
