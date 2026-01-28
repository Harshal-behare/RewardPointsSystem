import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { AuthService } from '../auth.service';
import { Router, ActivatedRoute } from '@angular/router';
import { extractValidationErrors } from '../../core/models/api-response.model';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent implements OnInit {
  form!: FormGroup;
  loading = false;
  errorMsg: string | null = null;
  validationErrors: string[] = [];
  private returnUrl: string = '/dashboard';

  constructor(
    private fb: FormBuilder,
    private auth: AuthService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  // Static validators
  static agdataEmailValidator(control: import('@angular/forms').AbstractControl) {
    const value = control.value || '';
    return value.endsWith('@agdata.com') ? null : { agdataEmail: true };
  }

  ngOnInit(): void {
    this.form = this.fb.group({
      email: ['', [
        Validators.required,
        Validators.email,
        Validators.maxLength(255),
        LoginComponent.agdataEmailValidator
      ]],
      password: ['', [
        Validators.required,
        Validators.minLength(6)
      ]]
    });
    // Get return URL from route parameters or default to '/dashboard'
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/dashboard';
  }



  onSubmit(): void {
    if (this.form.invalid) return;
    this.loading = true;
    this.errorMsg = null;
    this.validationErrors = [];

    this.auth.login(this.form.value).subscribe({
      next: res => {
        console.log('login response', res);
        this.loading = false;
        if (res && res.data && res.data.accessToken) {
          // Token is already stored by AuthService.login() via tap()
          
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
            // Use returnUrl if available, otherwise employee dashboard
            this.router.navigateByUrl(this.returnUrl);
          }
        } else {
          this.errorMsg = res.message || 'Login failed';
        }
      },
      error: err => {
        console.error('login error', err);
        this.loading = false;
        // Extract and display all validation errors
        this.validationErrors = extractValidationErrors(err);
        this.errorMsg = this.validationErrors[0] || 'Login request failed';
      }
    });
  }
}
