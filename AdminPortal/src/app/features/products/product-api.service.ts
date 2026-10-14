/** Product feature API adapter. Centralizes product endpoint URLs and keeps
 * the component focused on presentation and user interaction. */
import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import type { Product } from '../../shared/models';

@Injectable({ providedIn: 'root' })
export class ProductApiService {
  private readonly http = inject(HttpClient); private readonly url = 'http://localhost:6200/api/products';
  list() { return this.http.get<Product[]>(this.url); }
  save(id: number, body: { name: string; price: number }) { return id ? this.http.put<Product>(`${this.url}/${id}`, body) : this.http.post<Product>(this.url, body); }
  delete(id: number) { return this.http.delete(`${this.url}/${id}`); }
}
