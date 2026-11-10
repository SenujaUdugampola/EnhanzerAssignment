import { Component, DestroyRef, inject, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { LocationDetail, LocationService } from '../../core/services/location.service';
import { AuthService } from '../../core/services/auth.service';
import { PurchaseService } from '../../core/services/purchase.service';

interface BillItem {
  item: string; batch: string; standardCost: number; standardPrice: number; margin: number;
  qty: number; freeQty: number; discount: number; totalCost: number; totalSelling: number;
}

@Component({
  selector: 'app-purchase-bill',
  imports: [ReactiveFormsModule, DecimalPipe],
  templateUrl: './purchase-bill.component.html',
  styleUrl: './purchase-bill.component.css'
})
export class PurchaseBillComponent {
  private readonly formBuilder = inject(FormBuilder);
  private readonly locationService = inject(LocationService);
  private readonly authService = inject(AuthService);
  private readonly purchaseService = inject(PurchaseService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  readonly itemNames = ['Mango', 'Apple', 'Banana', 'Orange', 'Grapes', 'Kiwi', 'Strawberry'];
  readonly itemForm = this.formBuilder.nonNullable.group({
    item: ['', Validators.required],
    batch: ['', Validators.required],
    standardCost: [0, [Validators.required, Validators.min(0)]],
    standardPrice: [0, [Validators.required, Validators.min(0)]],
    margin: [0, [Validators.required, Validators.min(0)]],
    qty: [1, [Validators.required, Validators.min(1)]],
    freeQty: [0, [Validators.required, Validators.min(0)]],
    discount: [0, [Validators.required, Validators.min(0)]],
    totalCost: [{ value: 0, disabled: true }],
    totalSelling: [{ value: 0, disabled: true }]
  });
  readonly locations = signal<LocationDetail[]>([]);
  itemsTable: BillItem[] = [];
  isSaving = false;
  message = '';
  errorMessage = '';

  constructor() {
    this.itemForm.valueChanges.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(values => {
      const standardCost = values.standardCost ?? 0;
      const qty = values.qty ?? 0;
      const discount = values.discount ?? 0;
      const standardPrice = values.standardPrice ?? 0;
      const cost = standardCost * qty;
      this.itemForm.patchValue({
        totalCost: cost - cost * (discount / 100),
        totalSelling: standardPrice * qty
      }, { emitEvent: false });
    });
    this.locationService.getLocations().subscribe({
      next: locations => this.locations.set(locations),
      error: () => this.locations.set([])
    });
  }

  get totalQuantity(): number {
    return this.itemsTable.reduce((sum, item) => sum + item.qty, 0);
  }

  addItem(): void {
    if (this.itemForm.invalid) {
      this.itemForm.markAllAsTouched();
      return;
    }
    const values = this.itemForm.getRawValue();
    this.itemsTable.push(values);
    this.itemForm.reset({
      item: '', batch: '', standardCost: 0, standardPrice: 0, margin: 0, qty: 1,
      freeQty: 0, discount: 0, totalCost: 0, totalSelling: 0
    });
  }

  savePurchase(): void {
    if (this.itemsTable.length === 0 || this.isSaving) {
      return;
    }

    this.isSaving = true;
    this.message = '';
    this.errorMessage = '';
    this.purchaseService.createPurchase(this.itemsTable.map(item => ({
      itemName: item.item,
      batchCode: item.batch,
      standardCost: item.standardCost,
      standardPrice: item.standardPrice,
      marginPercent: item.margin,
      quantity: item.qty,
      freeQuantity: item.freeQty,
      discountPercent: item.discount
    }))).subscribe({
      next: response => {
        this.message = `Purchase ${response.billNo} saved successfully.`;
        this.itemsTable = [];
        this.isSaving = false;
      },
      error: error => {
        this.errorMessage = error.error?.message ?? 'Could not save the purchase.';
        this.isSaving = false;
      }
    });
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
