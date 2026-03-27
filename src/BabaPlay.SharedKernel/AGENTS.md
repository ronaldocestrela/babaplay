# AGENTS — BabaPlay.SharedKernel

## Responsabilidade

Código **compartilhado sem dependência de EF ou HTTP específicos além de `FrameworkReference` AspNetCore** para `BaseController`.

## Conteúdo principal

- `Entities/BaseEntity` — `Id` (string GUID sem hífens), `CreatedAt`, `UpdatedAt`.
- `Results/` — `Result`, `Result<T>`, `ResultStatus`, `ApiResponse<T>`.
- `Repositories/` — `IPlatformRepository<T>`, `ITenantRepository<T>`, unit of work interfaces.
- `Web/BaseController` — `FromResult`, `FromResult<T>`, `GetUserId`.
- `Security/` — `IAccessTokenIssuer`, `IPermissionResolver`.
- `Services/ITenantProvisioningService` — provisionar DB tenant sem acoplar módulo Platform à Infrastructure.

## Regras

- **Não** referenciar `BabaPlay.Infrastructure` nem módulos de domínio.
- Manter factories de `Result` estáveis; mudanças quebram todos os serviços.
- Novos contratos transversais: preferir interfaces aqui; implementação na Infrastructure.
