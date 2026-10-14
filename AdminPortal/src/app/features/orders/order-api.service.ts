/** Order feature API adapter. Encapsulates order CRUD HTTP calls and leaves
 * order form/table rendering to components. */
import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import type { Order } from '../../shared/models';

@Injectable({ providedIn: 'root' })
export class OrderApiService {
  private readonly http = inject(HttpClient); private readonly url = 'http://localhost:6200/api/orders';
  list() { return this.http.get<Order[]>(this.url); }
  save(id: number, body: unknown) { return id ? this.http.put<Order>(`${this.url}/${id}`, body) : this.http.post<Order>(this.url, body); }
  delete(id: number) { return this.http.delete(`${this.url}/${id}`); }
}
