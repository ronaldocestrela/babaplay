# BabaPlay Backend (MVP)

Multitenant SaaS API for sports associations. Stack: **.NET 10**, **ASP.NET Core**, **SQL Server**, **EF Core (Code First)**, **ASP.NET Core Identity**, **JWT**, **per-tenant database**.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server (local or external service)
- Docker (for containerized production run)

## Configuration

Edit `src/BabaPlay.Api/appsettings.json`:

- `Database:PlatformConnectionString` – central catalog (tenants, plans, subscriptions, CORS origins).
- `Database:TenantTemplateConnectionString` – optional; used by EF tooling for tenant model design-time (`TenantDbContext` factory).
- `Jwt:SigningKey` – use a long random secret (32+ characters).
- `Email:ApiKey` – Resend API key.
- `Email:DefaultFromEmail` – validated sender email/domain configured in Resend.

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

## Docker (Production with external database)

The production compose runs only the API container. SQL Server must be provided by an external service.

1. Create your environment file from template:

```bash
cp .env.example .env
```

2. Edit `.env` and set real values, especially:

- `SQL_SERVER_HOST`
- `SQL_SA_PASSWORD`
- `JWT_SIGNING_KEY` (32+ chars)
- `RESEND_API_KEY`
- `RESEND_FROM_EMAIL`

3. Build and start:

```bash
docker compose --env-file .env -f docker-compose.prod.yml up -d --build
```

4. Stop:

```bash
docker compose --env-file .env -f docker-compose.prod.yml down
```

Notes:

- API is exposed on `API_PUBLIC_PORT` (default `5077`).
- API uses env vars (`Database__*`, `Jwt__*`) and does not require editing `appsettings.json` for production.
- Keep `.env` out of source control; `.env.example` is the committed template.

## Multitenancy

- **Subdomain**: `https://{tenant}.yourdomain.com`
- **Local/dev header**: `X-Tenant-Subdomain: {subdomain}`

**Platform (no tenant) routes** (also skip tenant resolution):

- `/api/platform/*`
- `/swagger`, `/health`

Create a tenant via `POST /api/platform/tenants`, then call `POST /api/platform/tenants/{id}/subscription` with a `planId` to create the tenant SQL database and apply migrations (tenant model).

**Associação (tenant):** `POST /api/associations` pode definir `playersPerTeam` (mínimo 2, default 5). Esse valor determina quantas equipas são criadas em `POST /api/teams/generate` a partir dos check-ins da sessão (ver [docs/frontend/teams-controller.md](docs/frontend/teams-controller.md)).

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

### Tenant migrations automáticas (startup)

Ao arrancar a API, após migrar a base **platform**, um serviço em background (`TenantMigrationsHostedService`) percorre todos os registos em `Platform.Tenants` e aplica `Database.MigrateAsync()` em cada base tenant (`DatabaseName`), com as migrations pendentes do `TenantDbContext`. Falhas num tenant não bloqueiam os restantes; consulta os logs para o resumo (`migrated` / `failed` / `skippedEmptyDb`).

Novos tenants criados via `POST .../subscription` continuam a ser migrados no provisionamento (`TenantDatabaseProvisioner`).

## Security notes (MVP)

- `/api/platform/*` is currently **`[AllowAnonymous]`** for ease of setup; lock this down (separate auth, API keys, or network rules) before production.
- Configure CORS origins in the `AllowedOrigins` table (platform DB); localhost origins are allowed by policy for development.

## Versionamento

- Politica de versionamento: `docs/VERSIONING.md`
- Processo de release: `docs/RELEASE_PROCESS.md`
- Historico de mudancas: `CHANGELOG.md`
