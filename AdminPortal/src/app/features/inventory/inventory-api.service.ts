/** Inventory feature API adapter for stock management. */
import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import type { Inventory } from '../../shared/models';

@Injectable({ providedIn: 'root' })
export class InventoryApiService {
  private readonly http = inject(HttpClient); private readonly url = 'http://localhost:6200/api/inventory';
  list() { return this.http.get<Inventory[]>(this.url); }
  save(body: { productId: number; availableQuantity: number }) { return this.http.put<Inventory>(this.url, body); }
}
