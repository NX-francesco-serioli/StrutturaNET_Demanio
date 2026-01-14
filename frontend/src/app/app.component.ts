import { AsyncPipe, CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { AuthService, AuthResponse } from './auth.service';
import { NotificationService } from './notification.service';
import { environment } from '../environments/environment';
import { catchError, of, tap } from 'rxjs';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, AsyncPipe],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent implements OnInit {
  private readonly auth = inject(AuthService);
  private readonly fb = inject(FormBuilder);
  private readonly http = inject(HttpClient);
  private readonly notifications = inject(NotificationService);
  private readonly apiBase = environment.apiBaseUrl;

  readonly user$ = this.auth.currentUser$;
  readonly notifications$ = this.notifications.notifications$;

  statusMessage = '';
  adminUsers: { id: string; email: string; displayName?: string }[] = [];
  reportSummary: unknown = null;

  loginForm = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required]
  });

  registerForm = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
    displayName: [''],
    roles: ['User'],
    permissions: ['permissions.manage_users,permissions.view_reports']
  });

  ngOnInit(): void {
    this.auth.refreshCurrentUser().subscribe();
  }

  handleLogin(): void {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    const { email, password } = this.loginForm.getRawValue();
    this.statusMessage = 'Accesso in corso...';

    this.auth.login(email, password).subscribe({
      next: () => (this.statusMessage = 'Login eseguito'),
      error: (err) => (this.statusMessage = err?.error?.message ?? 'Credenziali non valide')
    });
  }

  handleRegister(): void {
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    const { email, password, displayName, roles, permissions } = this.registerForm.getRawValue();
    const payload = {
      email,
      password,
      displayName: displayName || undefined,
      roles: this.splitCsv(roles),
      permissions: this.splitCsv(permissions)
    };

    this.statusMessage = 'Creazione utente...';
    this.auth.register(payload).subscribe({
      next: () => (this.statusMessage = 'Utente creato (ricordati di fare login)'),
      error: (err) => (this.statusMessage = err?.error ?? 'Errore nella creazione utente')
    });
  }

  loadUsers(): void {
    this.http
      .get<{ id: string; email: string; displayName?: string }[]>(`${this.apiBase}/admin/users`)
      .pipe(
        tap(() => (this.statusMessage = '')),
        catchError((err) => {
          this.statusMessage = err?.error ?? 'Non autorizzato o errore nel caricamento utenti';
          return of([]);
        })
      )
      .subscribe((users) => (this.adminUsers = users));
  }

  loadReports(): void {
    this.http
      .get(`${this.apiBase}/reports/summary`)
      .pipe(
        tap(() => (this.statusMessage = '')),
        catchError((err) => {
          this.statusMessage = err?.error ?? 'Non autorizzato o errore report';
          return of(null);
        })
      )
      .subscribe((summary) => (this.reportSummary = summary));
  }

  logout(): void {
    this.auth.logout();
    this.adminUsers = [];
    this.reportSummary = null;
    this.statusMessage = 'Logout eseguito';
  }

  asAuth(response: AuthResponse | null): AuthResponse | null {
    return response;
  }

  private splitCsv(value?: string | null): string[] | undefined {
    if (!value) {
      return undefined;
    }

    const items = value
      .split(',')
      .map((v) => v.trim())
      .filter((v) => !!v);

    return items.length ? items : undefined;
  }
}
