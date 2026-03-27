# AGENTS — BabaPlay.Modules.Platform

## Domínio

Backoffice: **Plan**, **Tenant**, **Subscription**, **AllowedOrigin** (CORS na BD central está em `PlatformDbContext`).

## Serviços

- `PlanService` — CRUD de planos (`IPlatformRepository<Plan>`).
- `TenantSubscriptionService` — CRUD tenants, `SubscribeTenantAsync` chama `ITenantProvisioningService` para criar DB e aplicar migrações tenant.

## Controllers

- `PlansController`, `TenantsController` — rota base **`/api/platform/...`**; atualmente `[AllowAnonymous]` (MVP); reforçar auth em produção.

## Notas

- Subdomain único por tenant; `DatabaseName` gerado automaticamente (`BabaPlay_{guid:N}`).
