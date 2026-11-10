import { Component, inject } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { catchError, finalize, throwError, timeout } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';
import { DashboardService, ItemQuantitySummary, PurchaseDashboard } from '../../core/services/dashboard.service';

@Component({
  selector: 'app-dashboard',
  imports: [DecimalPipe, RouterLink],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class DashboardComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly dashboardService = inject(DashboardService);

  dashboard: PurchaseDashboard = {
    latestOrders: [],
    oldestItems: [],
    itemQuantities: []
  };
  errorMessage = '';
  isLoading = true;

  constructor() {
    this.dashboardService.getPurchaseDashboard().pipe(
      timeout(20000),
      catchError(error => {
        this.errorMessage = error.name === 'TimeoutError'
          ? 'The dashboard request timed out. Please try again.'
          : 'Dashboard data could not be loaded.';
        return throwError(() => error);
      }),
      finalize(() => this.isLoading = false)
    ).subscribe({
      next: dashboard => {
        this.dashboard = dashboard;
      },
      error: () => undefined
    });
  }

  get totalChartQuantity(): number {
    return this.dashboard.itemQuantities.reduce((total, item) => total + item.quantity, 0);
  }

  get donutBackground(): string {
    if (this.totalChartQuantity === 0) {
      return 'conic-gradient(#e8edf4 0 100%)';
    }

    let current = 0;
    const colors = ['#2264d1', '#16835b', '#f59e0b', '#a855f7', '#ef476f', '#0ea5a8'];
    const segments = this.dashboard.itemQuantities.map((item, index) => {
      const start = current;
      current += item.quantity / this.totalChartQuantity * 100;
      return `${colors[index % colors.length]} ${start}% ${current}%`;
    });
    return `conic-gradient(${segments.join(', ')})`;
  }

  chartColor(index: number): string {
    return ['#2264d1', '#16835b', '#f59e0b', '#a855f7', '#ef476f', '#0ea5a8']
      [index % 6];
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
