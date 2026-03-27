# AGENTS — BabaPlay.Infrastructure

## Responsabilidade

Implementação técnica: persistência, segurança, multitenancy, integrações (SQL Server).

## Áreas

- `Persistence/` — `PlatformDbContext`, `TenantDbContext`, repositórios EF (`EfPlatformRepository`, `EfTenantRepository`), `TenantDatabaseProvisioner`, `AssociateUserProvisioner` (`IAssociateUserProvisioner`), `TenantConnectionStringFactory`, `ITenantDatabaseMigrator` / `TenantDatabaseMigrator`, `TenantMigrationOrchestrator`, `TenantMigrationsHostedService` (migra tenants em massa no arranque), **factories** de design-time, **Migrations/Platform** e **Migrations/Tenant**.
- `Multitenancy/` — `ITenantProvider`, `TenantProvider` (AsyncLocal), `TenantResolutionMiddleware`.
- `Security/` — JWT (`JwtSettings`, `JwtAccessTokenIssuer`), CORS (`DynamicCorsPolicyProvider`, `AllowedOriginsCache`, sync worker), autorização por permissão (`PermissionRequirement`, `PermissionAuthorizationHandler`, `PermissionPolicyProvider` — policies `perm:nome`).
- `DependencyInjection.cs` — registo de serviços, Identity (`AddIdentityCore` + roles), DbContext com connection string do tenant.

## Regras

- Ao alterar modelo: atualizar `OnModelCreating`, gerar migrações (`Platform` vs `Tenant`) e documentar no README se necessário.
- **Módulos** são referenciados aqui para tipos de entidade nas `DbSet`; evite lógica de negócio na Infrastructure (só infra + seed técnico do tenant em `TenantDatabaseProvisioner`).
- Não introduzir dependência circular: módulos não devem referenciar este projeto.
