# Changelog

Todas as mudanças relevantes deste projeto serão documentadas aqui.

Formato baseado em [Keep a Changelog](https://keepachangelog.com/pt-BR/1.0.0/).

---

## [Unreleased]

### Added — Fase 2: Multi-Tenancy

- `ProvisioningStatus` enum (Pending/InProgress/Ready/Failed) no domínio
- `ITenantContext`, `ITenantRepository`, `IUserTenantRepository`, `ITenantProvisioningQueue` — interfaces de Application
- `CreateTenantCommand` / `CreateTenantCommandHandler` — cria tenant, valida slug único, enfileira provisionamento
- `GetTenantStatusQuery` / `GetTenantStatusQueryHandler` — retorna status de provisionamento
- `TenantRepository` / `UserTenantRepository` — acesso ao Master DB via EF Core
- `RequestTenantContext` — implementação de `ITenantContext` escopada por request
- `TenantProvisioningQueue` — fila interna com `System.Threading.Channels` (singleton)
- `TenantProvisioningWorker` — `BackgroundService` que cria banco SQL Server isolado + migra `TenantDbContext`
- `TenantMiddleware` — lê `X-Tenant-Slug` e popula `ITenantContext`; erros 404 via `NotFoundException`
- `POST /api/v1/tenant` → 201 `TenantResponse` (Pending) | 409 TENANT_SLUG_TAKEN | 422 TENANT_NAME_REQUIRED / TENANT_SLUG_REQUIRED
- `GET /api/v1/tenant/{id}/status` → 200 `TenantResponse` | 404 TENANT_NOT_FOUND
- EF Core migration `AddTenantProvisioningStatus` — adiciona coluna `ProvisioningStatus int NOT NULL DEFAULT 0` na tabela `Tenants`
- `TenantSlugHeaderOperationFilter` — documenta `X-Tenant-Slug` no Swagger para todos os endpoints
- `TestAuthHandler` + `TenantWebApplicationFactory` — infraestrutura de testes de integração com SQLite in-memory e bypass de JWT
- Frontend: `tenantService.ts` com `parseTenantSlug` (subdomain extractor) e `getTenantFromUrl` (subdomain → `?tenant=`)
- Frontend: `router.tsx` com resolução de tenant no `beforeLoad` da rota raiz
- Frontend: `TenantResponse` e `CreateTenantRequest` adicionados a `features/auth/types/index.ts`
- Frontend: MSW handlers para `POST /api/v1/tenant` e `GET /api/v1/tenant/:id/status`
- 49 novos testes (8 unit + 7 integration backend; 14 frontend); total acumulado: 111 testes (100% passando)


### Added — Fase 1: Identity + Auth

- `ApplicationUser : IdentityUser` com propriedades `IsActive` e `CreatedAt`
- `RefreshToken` entity com rotação e revogação (`RevokedAt` nullable)
- Master DB SaaS completo: `Tenant`, `UserTenant`, `Plan`, `Subscription` (enum `SubscriptionStatus`)
- `MasterDbContext : IdentityDbContext<ApplicationUser>` com todas as tabelas e índices
- EF Core migration: `InitialMaster`
- `JwtSettings` com suporte a `SecretKey`, `Issuer`, `Audience`, `AccessTokenExpiresInMinutes`, `RefreshTokenExpiresInDays`
- `JwtTokenService` (HmacSha256, claims: sub/email/jti/iat/role, ClockSkew=Zero)
- `UserRepository` e `RefreshTokenRepository` abstraídos via interfaces de Application
- `LoginCommand` / `LoginCommandHandler` — valida credenciais, gera e persiste token par
- `RefreshTokenCommand` / `RefreshTokenCommandHandler` — valida, revoga e rotaciona refresh token
- `POST /api/v1/auth/login` → 200 `AuthResponse` | 401 INVALID_CREDENTIALS | 422 USER_INACTIVE
- `POST /api/v1/auth/refresh-token` → 200 `AuthResponse` | 401 INVALID_TOKEN/TOKEN_EXPIRED
- Swagger Bearer JWT security definition + requirement globais
- `appsettings.json` / `appsettings.Development.json` com seções `ConnectionStrings:MasterDb` e `Jwt`
- Pacotes: `Microsoft.AspNetCore.Identity.EntityFrameworkCore 10.0.0`, `Microsoft.AspNetCore.Authentication.JwtBearer 10.0.0`, `Microsoft.EntityFrameworkCore.SqlServer 10.0.0`, `System.IdentityModel.Tokens.Jwt 8.9.0`, `Microsoft.EntityFrameworkCore.Sqlite 10.0.0` (Tests)
- 27 testes passando (20 unit + 7 integration)


### Added — Fase 0: Fundação

- Solução `.NET 10` com 5 projetos (`Api`, `Application`, `Domain`, `Infrastructure`, `Tests`)
- Clean Architecture com dependências corretas entre camadas
- Contratos CQRS: `ICommand<T>`, `ICommandHandler<T, R>`, `IQuery<T>`, `IQueryHandler<T, R>`
- `Result<T>` e `Result` para encapsular resultados de operações sem exceção de controle de fluxo
- Primitivos de domínio: `EntityBase`, `IAggregateRoot`, `IRepository<T>`, `IUnitOfWork`, `IDomainEvent`
- Exceções de domínio: `DomainException`, `NotFoundException`, `ValidationException`
- Vertical slice Ping: `PingCommand` + `PingCommandHandler`, `PingQuery` + `PingQueryHandler`, `PingController`
- `GlobalExceptionHandler` com mapeamento de exceções de domínio para `ProblemDetails`
- Swagger/OpenAPI configurado com docs XML
- Testes unitários e de integração para o vertical slice Ping (ciclo TDD Red-Green-Refactor)
- `coverlet` configurado para cobertura de código
- `Directory.Build.props` com configurações compartilhadas de projeto
- GitHub Actions CI: build + testes + cobertura >= 80%
