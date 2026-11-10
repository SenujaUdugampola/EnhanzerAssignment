import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {
  private readonly formBuilder = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly loginForm = this.formBuilder.nonNullable.group({
    Email: ['', [Validators.required, Validators.email]],
    Password: ['', Validators.required]
  });
  submitting = false;
  errorMessage = '';

  submit(): void {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.submitting = true;
    this.errorMessage = '';
    const { Email, Password } = this.loginForm.getRawValue();
    this.authService.login(Email, Password).subscribe({
      next: () => this.router.navigate(['/dashboard']),
      error: error => {
        this.submitting = false;
        this.errorMessage = error.error?.message ?? 'Unable to sign in. Please try again.';
      }
    });
  }
}
