# AGENTS — BabaPlay Backend (raiz)

## Contexto

API multitenant (.NET 10) para gestão de associações esportivas: banco **central** (tenants, planos, subscriptions, CORS) e **um banco por tenant** (dados da associação + Identity).

## Regras gerais

- **Retornos de negócio:** serviços expõem `Result` / `Result<T>` ([SharedKernel](src/BabaPlay.SharedKernel)). Controllers usam `FromResult` em [BaseController](src/BabaPlay.SharedKernel/Web/BaseController.cs) e envelope `ApiResponse<T>`.
- **Dependências:** módulos referenciam apenas `BabaPlay.SharedKernel`. `BabaPlay.Infrastructure` referencia SharedKernel + todos os módulos (EF, Identity, multitenancy). `BabaPlay.Api` referencia Infrastructure + todos os módulos.
- **Multitenancy:** rotas de plataforma usam prefixo `/api/platform/*` (sem resolução de tenant). Demais rotas exigem subdomínio ou header `X-Tenant-Subdomain`.
- **Migrações EF:** em `src/BabaPlay.Infrastructure/Persistence/Migrations/` (`Platform` e `Tenant`). Novas propriedades em entidades de módulo exigem atualizar `TenantDbContext` / `PlatformDbContext` e gerar migração.

## Ao alterar o sistema

1. Manter serviços finos nos módulos; persistência via `IPlatformRepository<T>` / `ITenantRepository<T>`.
2. Evitar expor `DbContext` para fora da Infrastructure (exceto já existente para `PermissionResolver`, etc.).
3. Novos controllers: registrar assembly em [Program.cs](src/BabaPlay.Api/Program.cs) (`AddApplicationPart`).
4. Não editar o arquivo de plano em `.cursor/plans/` se o utilizador pedir apenas implementação.

## Documentação

- Especificação funcional: [docs/llm_implementation_spec_saa_s_associacoes_esportivas.md](docs/llm_implementation_spec_saa_s_associacoes_esportivas.md)
- Setup: [README.md](README.md)
