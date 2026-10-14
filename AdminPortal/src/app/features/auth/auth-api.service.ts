/** Auth API adapter for the admin portal.
 * Keeps token storage and login calls out of the root component.
 */
import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import type { AuthResponse } from '../../shared/models';

@Injectable({ providedIn: 'root' })
export class AuthApiService {
  private readonly http = inject(HttpClient);
  private readonly url = 'http://localhost:5078/api/auth';
  private readonly tokenKey = 'orderflow.admin.token';
  get token() { return localStorage.getItem(this.tokenKey); }
  login(email: string, password: string) { return this.http.post<AuthResponse>(`${this.url}/login`, { email, password }); }
  store(token: string) { localStorage.setItem(this.tokenKey, token); }
  clear() { localStorage.removeItem(this.tokenKey); }
}
