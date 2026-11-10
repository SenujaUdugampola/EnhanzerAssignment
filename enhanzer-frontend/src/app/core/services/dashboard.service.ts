import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface LatestPurchaseOrder {
  id: number;
  netAmount: number;
  itemCount: number;
}

export interface OldestPurchaseItem {
  purchaseOrderId: number;
  itemName: string;
  quantity: number;
}

export interface ItemQuantitySummary {
  itemName: string;
  quantity: number;
}

export interface PurchaseDashboard {
  latestOrders: LatestPurchaseOrder[];
  oldestItems: OldestPurchaseItem[];
  itemQuantities: ItemQuantitySummary[];
}

@Injectable({ providedIn: 'root' })
export class DashboardService {
  private readonly http = inject(HttpClient);

  getPurchaseDashboard(): Observable<PurchaseDashboard> {
    return this.http.get<PurchaseDashboard>(`${environment.apiUrl}/purchases/dashboard`);
  }
}
