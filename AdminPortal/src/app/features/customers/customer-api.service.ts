/** Customer feature API adapter. Keeps HTTP details out of the root component;
 * an enterprise app would usually pair this with customer pages and forms. */
import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import type { Customer } from '../../shared/models';

@Injectable({ providedIn: 'root' })
export class CustomerApiService {
  private readonly http = inject(HttpClient); private readonly url = 'http://localhost:6200/api/customers';
  list() { return this.http.get<Customer[]>(this.url); }
  save(id: number, body: { name: string; email: string }) { return id ? this.http.put<Customer>(`${this.url}/${id}`, body) : this.http.post<Customer>(this.url, body); }
  delete(id: number) { return this.http.delete(`${this.url}/${id}`); }
}
