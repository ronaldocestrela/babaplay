# AGENTS — Modules

## Padrão de cada módulo

- `Entities/` — POCOs que herdam `BaseEntity` (quando aplicável); **não** herdade de Identity aqui salvo `ApplicationUser`/`ApplicationRole` no módulo Identity.
- `Services/` — orquestração e regras; retorno **`Result` / `Result<T>`**; uso de `IPlatformRepository` ou `ITenantRepository` + `IPlatformUnitOfWork` / `ITenantUnitOfWork`.
- `Controllers/` — herdam `BabaPlay.SharedKernel.Web.BaseController`; retornam `FromResult(...)`; rotas tenant em `/api/...`, plataforma em `/api/platform/...` (só módulo Platform).
- `DependencyInjection.cs` — `AddXxxModule(this IServiceCollection)` registando serviços `Scoped` típicos.

## Dependências

- Cada `BabaPlay.Modules.*.csproj`: `SharedKernel` + `FrameworkReference` AspNetCore; pacotes extra só quando necessários (ex.: `Microsoft.EntityFrameworkCore` para `Include`/`ToListAsync` nos serviços).

## Referências cruzadas

- `BabaPlay.Modules.TeamGeneration` referencia `BabaPlay.Modules.CheckIns` por usar entidade `CheckIn` no serviço. Evite novas referências entre módulos; prefira eventos ou contratos no SharedKernel se o domínio crescer.
