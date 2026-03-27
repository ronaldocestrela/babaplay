# AGENTS — BabaPlay.Api

## Responsabilidade

**Composition root** da aplicação Web: pipeline, configuração, descoberta de controllers.

## Ficheiros-chave

- `Program.cs` — `AddInfrastructure`, `Add*Module`, `AddControllers` + **`AddApplicationPart` para cada assembly de módulos com controllers**, CORS `Dynamic`, ordem: `UseRouting` → `UseCors` → `UseAuthentication` → `TenantResolutionMiddleware` → `UseAuthorization` → `MapControllers`.
- `appsettings*.json` — `Database:PlatformConnectionString`, `Database:TenantTemplateConnectionString`, `Jwt:*`.

## Regras

- Adicionar **qualquer novo módulo com API**: referência no `.csproj`, `services.AddXxxModule()`, e **um** `AddApplicationPart(typeof(...Controller).Assembly)`.
- Migrações via startup: neste projeto usa-se `dotnet ef` com `--startup-project BabaPlay.Api`.
- Evitar lógica de negócio aqui; manter só bootstrap e extensões mínimas.
