import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { BehaviorSubject, EMPTY, Observable, catchError, tap } from 'rxjs';
import { environment } from '../environments/environment';

export interface UserDto {
  id: string;
  email: string;
  displayName?: string;
  roles: string[];
  permissions: string[];
}

export interface AuthResponse {
  token: string;
  expiresAt: string | Date;
  user: UserDto;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly tokenKey = 'adsp_mds_demaniodigitale_accesstoken';
  private readonly apiBase = environment.apiBaseUrl;

  private readonly currentUserSubject = new BehaviorSubject<AuthResponse | null>(null);
  readonly currentUser$ = this.currentUserSubject.asObservable();

  constructor() {
    const token = this.token;
    if (token) {
      this.refreshCurrentUser().subscribe();
    }
  }

  get token(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  login(email: string, password: string): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.apiBase}/auth/login`, { email, password })
      .pipe(tap((resp) => this.setAuth(resp)));
  }

  register(payload: {
    email: string;
    password: string;
    displayName?: string;
    roles?: string[];
    permissions?: string[];
  }): Observable<unknown> {
    return this.http.post(`${this.apiBase}/auth/register`, payload);
  }

  refreshCurrentUser(): Observable<AuthResponse> {
    return this.http.get<AuthResponse>(`${this.apiBase}/auth/me`).pipe(
      tap((resp) => this.setAuth(resp)),
      catchError(() => {
        this.logout();
        return EMPTY;
      })
    );
  }

  logout(): void {
    localStorage.removeItem(this.tokenKey);
    this.currentUserSubject.next(null);
  }

  private setAuth(resp: AuthResponse): void {
    localStorage.setItem(this.tokenKey, resp.token);
    this.currentUserSubject.next(resp);
  }
}
