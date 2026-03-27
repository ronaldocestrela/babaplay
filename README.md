# BabaPlay Backend (MVP)

Multitenant SaaS API for sports associations. Stack: **.NET 10**, **ASP.NET Core**, **SQL Server**, **EF Core (Code First)**, **ASP.NET Core Identity**, **JWT**, **per-tenant database**.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server (local or Docker)

## Configuration

Edit `src/BabaPlay.Api/appsettings.json`:

- `Database:PlatformConnectionString` – central catalog (tenants, plans, subscriptions, CORS origins).
- `Database:TenantTemplateConnectionString` – optional; used by EF tooling for tenant model design-time (`TenantDbContext` factory).
- `Jwt:SigningKey` – use a long random secret (32+ characters).

## Run

Execute a partir da raiz do repositório:

```bash
# HTTP (porta 5077)
dotnet run --project src/BabaPlay.Api --launch-profile http

# HTTPS (portas 7166 / 5077)
dotnet run --project src/BabaPlay.Api --launch-profile https
```

- Swagger: `http://localhost:5077/swagger` (http) ou `https://localhost:7166/swagger` (https)
- Health: `http://localhost:5077/health`

## Multitenancy

- **Subdomain**: `https://{tenant}.yourdomain.com`
- **Local/dev header**: `X-Tenant-Subdomain: {subdomain}`

**Platform (no tenant) routes** (also skip tenant resolution):

- `/api/platform/*`
- `/swagger`, `/health`

Create a tenant via `POST /api/platform/tenants`, then call `POST /api/platform/tenants/{id}/subscription` with a `planId` to create the tenant SQL database and apply migrations (tenant model).

## API responses (Result)

All business results use `Result` / `Result<T>` in services. HTTP responses wrap payloads in:

```json
{ "success": true, "data": { } }
```

or on failure:

```json
{ "success": false, "error": "...", "errors": ["..."] }
```

## EF migrations

Migrations live in `src/BabaPlay.Infrastructure/Persistence/Migrations/` (`Platform` and `Tenant` folders).

```bash
# From repo root
dotnet ef migrations add Name --context PlatformDbContext --project src/BabaPlay.Infrastructure --startup-project src/BabaPlay.Api --output-dir Persistence/Migrations/Platform
dotnet ef migrations add Name --context TenantDbContext --project src/BabaPlay.Infrastructure --startup-project src/BabaPlay.Api --output-dir Persistence/Migrations/Tenant
```

## Security notes (MVP)

- `/api/platform/*` is currently **`[AllowAnonymous]`** for ease of setup; lock this down (separate auth, API keys, or network rules) before production.
- Configure CORS origins in the `AllowedOrigins` table (platform DB); localhost origins are allowed by policy for development.
