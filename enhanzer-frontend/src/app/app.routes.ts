import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { LoginComponent } from './features/login/login.component';
import { PurchaseBillComponent } from './features/purchase-bill/purchase-bill.component';
import { DashboardComponent } from './features/dashboard/dashboard.component';

export const routes: Routes = [
  { path: '', component: LoginComponent },
  { path: 'login', component: LoginComponent },
  { path: 'dashboard', component: DashboardComponent, canActivate: [authGuard] },
  { path: 'purchase-bill', component: PurchaseBillComponent, canActivate: [authGuard] },
  { path: '**', redirectTo: '' }
];
