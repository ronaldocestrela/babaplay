# AGENTS — src

## Layout

| Projeto | Função |
|---------|--------|
| `BabaPlay.SharedKernel` | Result, ApiResponse, BaseEntity, repositórios abstratos, BaseController, contratos (JWT, permissions, provisioning) |
| `BabaPlay.Infrastructure` | EF Core (`PlatformDbContext`, `TenantDbContext`), Identity, JWT, CORS dinâmico, multitenancy, implementações dos repositórios, migrações |
| `BabaPlay.Api` | Host: `Program.cs`, Swagger, registo de módulos (`AddApplicationPart`), pipeline HTTP |
| `Modules/*` | Bounded contexts: entidades, serviços (`Result`), controllers, `DependencyInjection.cs` |

## Convenções

- Novo módulo: pasta em `Modules/`, `.csproj`, referência a `SharedKernel` apenas; registar serviços e `AddApplicationPart` na API.
- Código gerado `bin/` e `obj/` — ignorar em alterações.

## Multitenancy (lembrete)

- `TenantResolutionMiddleware` resolve tenant antes dos endpoints que usam `TenantDbContext`.
- Plataforma: `/api/platform/*`, `/swagger`, `/health`.
