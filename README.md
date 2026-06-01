# LIOS Care Doctor Dashboard — Portal

Production-ready ASP.NET Core MVC implementation aligned with **LIOSCare Dashboard TechSpec v1.0**.

## Scope implemented

This version is doctor-only. It intentionally excludes admin pricing configuration, doctor onboarding, billing/payouts, mobile dashboard, and in-dashboard voice/video calls because those are out of v1.0.

Implemented modules:

- Doctor login/logout
- JWT auth endpoints and refresh tokens
- Dashboard home with KPI cards, availability toggle, SLA watchlist, recent bookings, latest reports
- Quick Chat Queue with pool/direct visibility rules, filters, SLA urgency and concurrency-safe accept
- Quick Chat detail and reply workflow
- Direct booking requests with accept, decline + reason, and Diamond scheduling
- Doctor Profile editor with specializations, bio, certifications, education, availability and read-only pricing
- Session reports list, new report, detail, edit within 24h only
- Settings / notification preference mock UI
- API endpoints under `/api/v1`
- Code First EF Core PostgreSQL migrations
- Dapper optimized dashboard query service
- MessagingService client hook for `LioSocial.MessagingService`
- NotificationService client hook for user push/in-app notifications

## Tech stack

- .NET 10
- ASP.NET Core MVC / Razor Views
- EF Core Code First
- PostgreSQL / Npgsql
- Dapper for dashboard reporting queries
- Cookie auth for MVC pages
- JWT bearer auth for API endpoints
- Custom Material-inspired CSS design system in `wwwroot/css/site.css`

## Database approach

This is a **Code First EF Core migration** project.

The migration adds LIOS Care doctor dashboard tables to the existing PostgreSQL database using these schemas:

- `auth`
- `provider`

Tables created:

- `auth.doctor_dashboard_accounts`
- `auth.doctor_refresh_tokens`
- `provider.doctor_profiles`
- `provider.doctor_educations`
- `provider.service_tiers`
- `provider.quick_chat_requests`
- `provider.direct_booking_requests`
- `provider.doctor_session_reports`

## Configure PostgreSQL

Edit:

```json
"ConnectionStrings": {
  "DoctorPortalDb": "Host=localhost;Port=5432;Database=social_platform_db;Username=postgres;Password=postgres;Include Error Detail=true"
}
```

## Run migrations

From repository root:

```bash
cd src/LIOSCare.DoctorDashboard.Web
dotnet restore
dotnet ef database update --project ../LIOSCare.DoctorDashboard.Infrastructure --startup-project .
dotnet run
```

If `dotnet ef` is not installed:

```bash
dotnet tool install --global dotnet-ef
```

For local development, the app can run migrations and seed data on startup when:

```json
"Portal": {
  "AutoMigrateOnStartup": true,
  "SeedDemoData": true
}
```

For production, set `AutoMigrateOnStartup` to `false` and run migrations through your deployment pipeline.

## Default doctor login

```text
Email: doctor@lioscare.local
Password: Doctor@123
```

## API examples

All API endpoints use `/api/v1` and require JWT bearer auth except login/refresh.

```http
POST /api/v1/auth/login
GET  /api/v1/doctors/me
GET  /api/v1/doctors/me/stats
GET  /api/v1/quick-chats?status=pending
POST /api/v1/quick-chats/{id}/accept
POST /api/v1/quick-chats/{id}/reply
GET  /api/v1/bookings?doctor_id=me&status=pending
POST /api/v1/bookings/{id}/accept
POST /api/v1/bookings/{id}/decline
PATCH /api/v1/bookings/{id}/schedule
GET  /api/v1/session-reports?doctor_id=me
POST /api/v1/session-reports
PATCH /api/v1/session-reports/{id}
GET  /api/v1/service-tiers
```

## MessagingService integration

Configure:

```json
"MessagingService": {
  "BaseUrl": "https://localhost:7081",
  "Enabled": true,
  "ServiceToken": "PASTE_VALID_INTERNAL_OR_DOCTOR_JWT"
}
```

The portal uses `IMessagingServiceClient` and `HttpMessagingServiceClient`. When disabled, quick-chat workflow still works through local dashboard tables.

## NotificationService integration

Configure:

```json
"NotificationService": {
  "BaseUrl": "https://localhost:7181",
  "Enabled": true,
  "InternalApiKey": "PASTE_INTERNAL_API_KEY"
}
```

The service is used when doctors reply to quick chats or accept/decline/schedule bookings.

## Production notes

- Change `Jwt:SigningKey` before deployment.
- Disable startup auto-migration in production.
- Use HTTPS.
- Store secrets in environment variables or a secret manager.
- Replace demo photo handling with your media/CDN upload service.
- Keep service tier write endpoints admin-only outside this doctor portal.
