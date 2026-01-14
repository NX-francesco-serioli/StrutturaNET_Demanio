import { Injectable, inject } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { HubConnection, HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import { environment } from '../environments/environment';
import { AuthService } from './auth.service';

export interface NotificationMessage {
  type: string;
  message: string;
  occurredAtUtc: string;
  userId?: string | null;
}

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private readonly auth = inject(AuthService);
  private readonly hubUrl = environment.notificationsHubUrl;
  private readonly notificationsSubject = new BehaviorSubject<NotificationMessage[]>([]);
  private connection: HubConnection | null = null;

  readonly notifications$ = this.notificationsSubject.asObservable();

  constructor() {
    this.auth.currentUser$.subscribe((auth) => {
      if (auth?.token) {
        this.startConnection();
      } else {
        this.stopConnection();
      }
    });
  }

  private startConnection(): void {
    if (this.connection && this.connection.state !== HubConnectionState.Disconnected) {
      return;
    }

    this.connection = new HubConnectionBuilder()
      .withUrl(this.hubUrl, {
        accessTokenFactory: () => this.auth.token ?? ''
      })
      .withAutomaticReconnect()
      .build();

    this.connection.on('notification', (notification: NotificationMessage) => {
      const next = [notification, ...this.notificationsSubject.value].slice(0, 20);
      this.notificationsSubject.next(next);
    });

    this.connection.start().catch(() => {
      this.connection?.stop().catch(() => undefined);
    });
  }

  private stopConnection(): void {
    if (!this.connection) {
      return;
    }

    const current = this.connection;
    this.connection = null;
    current.stop().catch(() => undefined);
    this.notificationsSubject.next([]);
  }
}
