# Changelog

All notable changes to BabaPlay Backend will be documented in this file.

The format follows [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

> **Beta policy:** versions `0.x.y` are pre-release. Breaking changes may happen without a major-version bump until `1.0.0`.

---

## [Unreleased]

### Added

- **Association `PlayersPerTeam`**: campo no tenant (`Associations`) — alvo de jogadores por equipa na geração de times (default 5, mínimo 2). `POST /api/associations` aceita `playersPerTeam`; migração tenant `AddPlayersPerTeamToAssociation`.
- **Tenant migrations on startup**: `TenantMigrationsHostedService` iterates `Platform.Tenants` and applies pending `TenantDbContext` migrations to each tenant database; failures are logged per tenant without blocking others.
- **Associate `IsActive`**: column on tenant `Associates`; inactive associates cannot log in (`403` on `/api/auth/login`). `PATCH /api/associates/{id}/active` toggles status. `IAssociateStatusChecker` in SharedKernel, implemented in Infrastructure.
- **Associate user provisioning**: `POST /api/associates` requires `email` and creates an Identity user with role **Associate**, linking `Associate.UserId` and `ApplicationUser.AssociateId`. `IAssociateUserProvisioner` (SharedKernel) implemented as `AssociateUserProvisioner` (Infrastructure).
- **Associate invitations by link**: tenant feature to generate invitation links with expiry (default 7 days), validate invitation token, and register associates via `/api/auth/register-with-invitation`.
- **Shared invitation links (multi-use)**: managers/admins can create one reusable link that allows multiple associates to self-register.
- **Invitation model for future email dispatch**: invitation contract supports single-use email-bound invites (reserved for upcoming email dispatch flow).

### Changed

- **Teams generation**: `POST /api/teams/generate` deixa de aceitar `teamCount`; o número de equipas é `max(2, checkedInAssociates / playersPerTeam)` com `playersPerTeam` lido da associação no tenant.
- **Associates API**: list/get/create/update/PATCH active return `AssociateResponse` with `positions[]` as `{ positionId, positionName }` (no nested EF entities; avoids JSON reference cycles).
- **Position** (tenant): removed `SortOrder`; list ordering is alphabetical by `Name`. Tenant migration `RemovePositionSortOrder` drops column `Positions.SortOrder`. API payloads for `/api/positions` no longer accept or return `sortOrder`.
- **Invitation registration payload**: `/api/auth/register-with-invitation` now accepts optional `email`; required for shared links and ignored for single-use email-bound invitations.

---

## [0.1.0-beta.1] — 2026-03-26

### Added

#### Architecture & Infrastructure
- Multi-tenant architecture with per-tenant SQL Server database isolation.
- `Directory.Build.props` with shared versioning (`VersionPrefix` / `VersionSuffix`) across all projects.
- `SharedKernel` building block: `BaseEntity`, `Result<T>` / `Result` pattern, `ApiResponse<T>` HTTP envelope, repository interfaces (`IPlatformRepository<T>`, `ITenantRepository<T>`, `IPlatformUnitOfWork`, `ITenantUnitOfWork`), `BaseController` with `FromResult` helper.
- `BabaPlay.Infrastructure`: `PlatformDbContext`, `TenantDbContext`, EF Core Code First migrations (Platform and Tenant), generic EF repository implementations, `TenantDatabaseProvisioner` for on-demand tenant DB creation.
- Tenant resolution via subdomain or `X-Tenant-Subdomain` request header (`TenantResolutionMiddleware`).
- Dynamic CORS policy driven by `AllowedOrigins` table in the platform database (`DynamicCorsPolicyProvider`, `AllowedOriginsSyncWorker`).
- JWT authentication with `JwtAccessTokenIssuer` and configurable `Jwt:SigningKey`.
- Permission-based authorization with custom `PermissionAuthorizationHandler`, `PermissionPolicyProvider`, and `PermissionResolver`.

#### Module — Platform (`/api/platform/*`)
- `Tenant` entity with subdomain, plan, and subscription tracking.
- `Plan` entity with name and description.
- `Subscription` entity linking tenants to plans.
- `AllowedOrigin` entity for dynamic CORS management.
- `TenantsController`: CRUD for tenants + `POST /api/platform/tenants/{id}/subscription` to provision tenant database and apply migrations.
- `PlansController`: CRUD for subscription plans.

#### Module — Identity
- `ApplicationUser` and `ApplicationRole` via ASP.NET Core Identity.
- `Permission` and `RolePermission` entities (custom RBAC layer).
- `AuthController`: `POST /api/auth/register`, `POST /api/auth/login` (returns JWT).
- `RolesController`: CRUD for roles, assign/remove users, manage role permissions.
- `PermissionsController`: list all permissions.

#### Module — Associates
- `Associate` entity (linked to `ApplicationUser`) with profile data.
- `Position` entity for sports positions.
- `AssociatePosition` join entity (many-to-many).
- `AssociatesController`: CRUD for associates, list associates with positions.
- `PositionsController`: CRUD for positions.

#### Module — Associations
- `Association` entity with name, logo URL, and address.
- `AssociationsController`: CRUD for association profile.

#### Module — Check-ins
- `CheckInSession` entity (date-based sessions).
- `CheckIn` entity linking associates to sessions.
- `CheckInsController`: open/close sessions, record check-ins, list session attendees.

#### Module — Financial
- `Category` entity for income/expense classification.
- `CashEntry` entity for manual cash-flow records.
- `Membership` and `Payment` entities for associate fee tracking.
- `CategoriesController`: CRUD for categories.
- `CashEntriesController`: CRUD for cash entries.
- `MembershipsController`: create memberships, record payments, list overdue memberships.

#### Module — Team Generation
- `Team` and `TeamMember` entities.
- `TeamsController`: generate balanced teams from a pool of present associates, list generated teams, delete teams.

---

[Unreleased]: https://github.com/your-org/babaplay-backend/compare/v0.1.0-beta.1...HEAD
[0.1.0-beta.1]: https://github.com/your-org/babaplay-backend/releases/tag/v0.1.0-beta.1
