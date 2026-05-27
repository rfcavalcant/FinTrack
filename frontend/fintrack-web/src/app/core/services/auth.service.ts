import { Injectable, signal, computed, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { AuthResponse, LoginRequest, RegisterRequest } from '../../models/auth.models';

// Trade-off de storage:
// - localStorage  → persiste entre abas/sessões, vulnerável a XSS
// - sessionStorage → fechou a aba, perdeu a sessão
// - httpOnly cookie → imune a XSS, exigiria CSRF token e mudanças no back
// Escolha: localStorage — padrão SPA, proteção contra XSS vem de CSP adequada.
const STORAGE_KEY = 'fintrack_session';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  private readonly _currentUser = signal<AuthResponse | null>(this.loadFromStorage());

  readonly currentUser = this._currentUser.asReadonly();
  readonly isAuthenticated = computed(() => this._currentUser() !== null);

  login(request: LoginRequest) {
    return this.http
      .post<AuthResponse>(`${environment.apiUrl}/api/v1/auth/login`, request)
      .pipe(tap(response => this.persist(response)));
  }

  register(request: RegisterRequest) {
    return this.http
      .post<AuthResponse>(`${environment.apiUrl}/api/v1/auth/register`, request)
      .pipe(tap(response => this.persist(response)));
  }

  logout(): void {
    localStorage.removeItem(STORAGE_KEY);
    this._currentUser.set(null);
    this.router.navigate(['/login']);
  }

  getToken(): string | null {
    return this._currentUser()?.token ?? null;
  }

  private persist(response: AuthResponse): void {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(response));
    this._currentUser.set(response);
  }

  private loadFromStorage(): AuthResponse | null {
    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      return raw ? (JSON.parse(raw) as AuthResponse) : null;
    } catch {
      return null;
    }
  }
}
