# AdSP MdS - Demanio Digitale

Questo repository contiene una soluzione full-stack con backend .NET, frontend Angular, messaging su RabbitMQ e notifiche realtime via SignalR.

## Struttura e layer

### Backend
- Api: espone le API REST, autenticazione/autorità, Swagger/OpenAPI.
- Worker: servizio background per elaborazioni asincrone e consumatori MassTransit.
- Application: logica applicativa, eventi, opzioni e servizi di dominio.
- Domain: entita', value object e regole di business.
- Persistence: accesso dati (EF Core, Postgres/PostGIS).
- ServiceDefaults: configurazioni comuni (telemetria, health checks, service discovery).

### Frontend
- Client Angular (SPA) con login, gestione utenti e schermate demo.
- Proxy locale per chiamare l'API e il SignalR hub.

### Messaging e notifiche
- RabbitMQ come broker per eventi asincroni (MassTransit).
- Notifiche realtime via SignalR: gli eventi backend vengono inoltrati ai client connessi.

## Ambienti di esecuzione

### Locale con Aspire (AppHost)
- AppHost avvia i servizi backend e i container infrastrutturali (Postgres/PostGIS, RabbitMQ).
- Usato per sviluppo locale in Visual Studio.
- File: `AppHost/Program.cs`.

### Docker Compose
- Avvio completo in container (db, rabbitmq, api, worker, frontend, nginx).
- File: `docker-compose.yml`.

## Avvio rapido

### AppHost (locale)
1. Apri la soluzione in Visual Studio.
2. Imposta `AppHost` come progetto di avvio.
3. Avvia in debug.

### Docker Compose
```bash
docker compose up -d --build
```

Per fermare i servizi:
```bash
docker compose down
```

## Componenti principali
- Database: Postgres/PostGIS.
- Broker: RabbitMQ.
- Auth: ASP.NET Identity + JWT.
- Realtime: SignalR Hub `/hubs/notifications`.