# AGENTS — BabaPlay.Modules.Identity

## Domínio

**ApplicationUser** (`UserType`, `AssociateId`), **ApplicationRole**, entidades **Permission** e **RolePermission** (tenant DB).

## Serviços

- `AuthService` — registo e login; JWT via `IAccessTokenIssuer`; permissões via `IPermissionResolver` (implementação na Infrastructure).
- `RoleAdminService` — listar roles, atribuir role a utilizador, listar permissions.

## Controllers

- `AuthController` — `/api/auth/register`, `/api/auth/login` (`[AllowAnonymous]`).
- `RolesController`, `PermissionsController` — autenticados.

## Notas

- Seed de roles/permissions posicionado na Infrastructure (`TenantDatabaseProvisioner`).
- Policies de autorização: `perm:nome_da_permissao` (ex.: `perm:associates.read`).
