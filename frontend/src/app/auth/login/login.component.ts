import { Component, OnInit, signal, ChangeDetectorRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { AuthService } from '../auth.service';
import { Router, ActivatedRoute } from '@angular/router';
import { extractValidationErrors } from '../../core/models/api-response.model';
import { IconComponent } from '../../shared/components/icon/icon.component';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, IconComponent],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent implements OnInit {
  form!: FormGroup;
  loading = signal(false);
  errorMsg = signal<string | null>(null);
  validationErrors = signal<string[]>([]);
  private returnUrl: string = '/dashboard';

  constructor(
    private fb: FormBuilder,
    private auth: AuthService,
    private router: Router,
    private route: ActivatedRoute,
    private cdr: ChangeDetectorRef
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
        Validators.minLength(8),
        Validators.maxLength(20),
        Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+\-=\[\]{};':"\\|,.<>\/?]).+$/)
      ]]
    });
    // Get return URL from route parameters or default to '/dashboard'
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/dashboard';
  }

showPassword = false;

togglePassword(): void {
  this.showPassword = !this.showPassword;
}

  onSubmit(): void {
    if (this.form.invalid) return;
    this.loading.set(true);
    this.errorMsg.set(null);
    this.validationErrors.set([]);

    this.auth.login(this.form.value).subscribe({
      next: res => {
        console.log('login response', res);
        this.loading.set(false);
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
          this.errorMsg.set(res.message || 'Login failed');
          this.cdr.detectChanges();
        }
      },
      error: err => {
        console.error('login error', err);
        this.loading.set(false);
        // Extract and display all validation errors immediately
        const errors = extractValidationErrors(err);
        this.validationErrors.set(errors);
        this.errorMsg.set(errors[0] || 'Login request failed');
        // Force change detection to show errors immediately
        this.cdr.detectChanges();
      }
    });
  }
}
