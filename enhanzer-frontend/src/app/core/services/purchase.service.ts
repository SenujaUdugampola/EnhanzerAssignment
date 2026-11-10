import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface PurchaseItem {
  itemName: string;
  batchCode: string;
  standardCost: number;
  standardPrice: number;
  marginPercent: number;
  quantity: number;
  freeQuantity: number;
  discountPercent: number;
}

export interface PurchaseResponse {
  id: number;
  billNo: string;
  purchaseDate: string;
  totalQuantity: number;
  totalFreeQuantity: number;
  totalCost: number;
  totalSelling: number;
}

@Injectable({ providedIn: 'root' })
export class PurchaseService {
  private readonly http = inject(HttpClient);

  createPurchase(items: PurchaseItem[]): Observable<PurchaseResponse> {
    return this.http.post<PurchaseResponse>(`${environment.apiUrl}/purchases`, { items });
  }
}
