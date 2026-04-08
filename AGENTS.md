# AGENTS.md — BabaPlay Backend (raiz)

Este arquivo orienta agentes de IA e desenvolvedores ao trabalhar neste repositório.

## 1. Objetivo do projeto

API **multitenant** (.NET 10) para gestão de **associações esportivas** (SaaS MVP):

- **Banco central (platform):** tenants, planos, subscriptions, CORS, catálogo operacional.
- **Um banco por tenant:** dados da associação + ASP.NET Core Identity.

Não é um monólito de frontend: este repositório é o **backend**. Requisitos de produto e contratos para clientes estão documentados em `docs/` (ver secção [Documentação](#19-documentação)).

### Regras funcionais centrais (visão de sistema)

- Resolução de tenant por **subdomínio** ou header `X-Tenant-Subdomain`.
- Rotas de plataforma **sem** tenant: prefixo `/api/platform/*`.
- Negócio e permissões são **por associação (tenant)**; dados de plataforma não se misturam com dados de tenant nos fluxos normais de API de associação.

Para detalhes de domínio (entidades, fluxos, regras), usar a especificação funcional em `docs/` — não assumir que o código reflete sempre a spec; em caso de divergência, alinhar com o utilizador.

---

## 2. Como trabalhar neste projeto

Abordagem de pair programming com IA:

- o humano define o **quê** e o **porquê**
- o agente ajuda no **como**
- alterações **pequenas**, validáveis e reversíveis
- mudanças importantes entram com **testes** quando fizer sentido para o risco
- **nenhum commit** deve deixar o projeto quebrado (build verde na raiz do que foi tocado)
- **multitenancy**, **isolamento de dados** e **contratos de API** têm prioridade sobre velocidade bruta

### Regras obrigatórias

1. Não tentar implementar “o sistema inteiro” numa única sessão.
2. Trabalhar por **caso de uso** ou tarefa pequena.
3. Toda mudança deve considerar: **domínio**, **tenant correto**, **persistência** (Platform vs Tenant), **autorização**, **migrações** se o modelo mudar.
4. Não simplificar isolamento multitenant nem rotas de plataforma sem instrução explícita.
5. Não ignorar impacto em **migrações EF** quando entidades/DbContexts mudam.
6. Não trocar stack, padrão arquitetural macro ou convenções do repo sem autorização explícita.

---

## 3. Stack oficial

### Backend (este repositório)

- .NET 10
- ASP.NET Core Web API
- Entity Framework Core (Code First)
- SQL Server
- ASP.NET Core Identity (por tenant)
- JWT (autenticação)

### Padrões de código

- Módulos por domínio (`BabaPlay.Modules.*`) + `BabaPlay.SharedKernel` + `BabaPlay.Infrastructure` + `BabaPlay.Api`.
- Retornos de negócio: `Result` / `Result<T>` no SharedKernel; controllers alinham com `BaseController` e envelope `ApiResponse<T>`.

### Infra (conforme configuração)

- Email (ex.: Resend) via configuração em `appsettings` / variáveis de ambiente
- Docker para execução containerizada (ver `README.md`)

---

## 4. Estrutura esperada do repositório

```text
src/
  BabaPlay.Api/
  BabaPlay.Infrastructure/
  BabaPlay.SharedKernel/
  Modules/
    BabaPlay.Modules.Platform/
    BabaPlay.Modules.Associations/
    BabaPlay.Modules.Associates/
    BabaPlay.Modules.CheckIns/
    BabaPlay.Modules.TeamGeneration/
    BabaPlay.Modules.Financial/
    BabaPlay.Modules.Identity/
  ...

tests/
  BabaPlay.Tests.Unit/

docs/
  llm_implementation_spec_saa_s_associacoes_esportivas.md
  frontend/                    # contratos por controller (API consumida pelo cliente)
  ...
```

### Responsabilidades (resumo)

- **`BabaPlay.Api`:** composição, pipeline, Swagger, health, registo de parts de controllers.
- **`BabaPlay.SharedKernel`:** primitivas partilhadas (ex.: `Result`, `BaseController`, utilitários web).
- **`BabaPlay.Modules.*`:** serviços de domínio, controllers do módulo, contratos do módulo — com dependência preferencial no SharedKernel; referências cruzadas entre módulos só quando o domínio o exige (ex.: `TeamGeneration` → `CheckIns`, `Associations`).
- **`BabaPlay.Infrastructure`:** EF Core, `PlatformDbContext` / `TenantDbContext`, repositórios, Identity, multitenancy, migrações em `Persistence/Migrations/` (**Platform** e **Tenant**).

---

## 5. Comandos importantes

### Backend (raiz do repositório)

```bash
dotnet restore
dotnet build
dotnet test
dotnet run --project src/BabaPlay.Api --launch-profile http
```

### Entity Framework (migrações)

Migrações em `src/BabaPlay.Infrastructure/Persistence/Migrations/` (`Platform` e `Tenant`). Exemplos:

```bash
dotnet ef migrations add NomeDaMigracao --context PlatformDbContext --project src/BabaPlay.Infrastructure --startup-project src/BabaPlay.Api --output-dir Persistence/Migrations/Platform

dotnet ef migrations add NomeDaMigracao --context TenantDbContext --project src/BabaPlay.Infrastructure --startup-project src/BabaPlay.Api --output-dir Persistence/Migrations/Tenant
```

### Regra

Antes de concluir uma tarefa, o agente deve, no mínimo:

- **`dotnet build`** (e **`dotnet test`** quando a alteração afetar comportamento testável)
- rever **migrações** quando houver mudança de modelo/persistência
- confirmar que novos controllers estão registados (ver secção [Ao alterar o sistema](#7-ao-alterar-o-sistema))

---

## 6. Arquitetura de backend

### Padrões obrigatórios neste repo

- **Serviços finos** nos módulos; persistência via abstrações (`IPlatformRepository<T>` / `ITenantRepository<T>`) — não expor `DbContext` fora da Infrastructure (exceto casos já existentes e justificados, ex.: `PermissionResolver`).
- **Controllers** consistentes com `Result` / `ApiResponse<T>` e `FromResult` no `BaseController`.
- **Multitenancy:** rotas `/api/platform/*` sem resolução de tenant; restantes rotas com subdomínio ou `X-Tenant-Subdomain`.
- **Migrações:** alterações em entidades refletidas nos `DbContext` certos e nova migração quando aplicável.

### Organização por módulo

Cada `BabaPlay.Modules.*` concentra o que é daquele bounded context (serviços, controllers, DTOs conforme já adoptado no projeto). Novos controllers: registar o assembly em `Program.cs` (`AddApplicationPart`).

### Regra de ouro

Preferir **clareza e testabilidade** a camadas genéricas desnecessárias. O que importa é **tenant isolado**, **API previsível** e **persistência correcta**.

---

## 7. Ao alterar o sistema

1. Manter serviços finos nos módulos; persistência via `IPlatformRepository<T>` / `ITenantRepository<T>`.
2. Evitar expor `DbContext` para fora da Infrastructure (exceto já existente para `PermissionResolver`, etc.).
3. Novos controllers: registar assembly em [`Program.cs`](src/BabaPlay.Api/Program.cs) (`AddApplicationPart`).
4. Não editar o arquivo de plano em `.cursor/plans/` se o utilizador pedir **apenas** implementação.

---

## 8. Frontend e contratos de API

Este repositório não contém a app web; a documentação para consumidores da API está em **`docs/frontend/`** (tipicamente um ficheiro por **controller**, com payloads e `ApiResponse`).

- Configuração de equipas: ver `docs/frontend/associations-controller.md` (`playersPerTeam`) e `docs/frontend/teams-controller.md` (geração com `sessionId`).

Regra: não colocar código-fonte de aplicação dentro de `docs/` sem pedido explícito.

---

## 9. Segurança e segredos

Nunca expor segredos em saída de terminal, logs, commits, capturas de ecrã ou respostas do agente.

### Regras obrigatórias

- não executar comandos que despejem segredos completos (ex.: `env`, `printenv`, `cat .env`)
- ao verificar variáveis de ambiente, preferir indicar apenas **definida / indefinida**, não valores
- nunca imprimir **connection strings** completas nem **JWT signing keys** completas
- nunca exibir tokens de autenticação completos

Se algum segredo aparecer acidentalmente, recomendar **rotação** imediata.

Nota de produto (MVP): rotas `/api/platform/*` podem estar permissivas para facilitar setup; endurecer antes de produção (ver `README.md`).

---

## 10. Padrões importantes do projeto

- **Result / ApiResponse:** erros de negócio previsíveis via `Result`; HTTP alinhado com o envelope comum.
- **Dependências entre projetos:** módulos → SharedKernel; Infrastructure → SharedKernel + todos os módulos; Api → Infrastructure + todos os módulos.
- **CORS:** origens permitidas geridas no contexto de plataforma (tabela `AllowedOrigins`), não hardcoded sem critério.
- **Migrações tenant:** há serviços de arranque/provisionamento que aplicam migrações — falhas num tenant não devem ser ignoradas em silêncio na análise; ver logs.

---

## 11. Fluxo obrigatório por tarefa técnica

Ordem recomendada:

1. Perceber o objetivo exacto e o **tenant vs platform** afectados.
2. Mapear regra de negócio e entidades.
3. Identificar **autorização** e **persistência** (qual `DbContext` / repositório).
4. Implementar o mínimo necessário + ajustar testes quando aplicável.
5. Rever **nomes**, duplicação e complexidade.
6. Garantir **build** (e testes relevantes) verdes.
7. Actualizar documentação viva apenas se surgir convenção nova ou armadilha importante (não expandir `docs/` sem necessidade).

---

## 12. Formato recomendado para solicitar trabalho ao agente

```text
Contexto:
Objetivo:
Regras de negócio:
Restrições (incl. multitenancy / platform):
Critérios de aceite:
Arquivos / áreas prováveis:
Testes esperados:
Riscos:
```

---

## 13. Critérios mínimos de aceite por alteração relevante

Quando aplicável, a entrega deve deixar claro:

- **quem** pode executar (papéis / utilizador autenticado / plataforma)?
- **em que scope** (tenant vs platform)?
- **que dados** persistem e em que base?
- **que testes** cobrem o comportamento?
- há **migração** e foi revista?

---

## 14. Estratégia de testes

- **Unitários:** serviços e regras com dependências substituíveis (padrão actual em `tests/BabaPlay.Tests.Unit`).
- **Integração:** quando existir projeto dedicado no futuro, preferir para pipelines EF, auth e multitenancy; até lá, seguir o que o repo já oferece.

Regra: código novo sem teste só é aceitável para **wiring trivial** e baixo risco; caso contrário, preferir pelo menos um teste que falhe se a regra quebrar.

---

## 15. Convenções de commit

Preferir commits pequenos e com mensagem clara (estilo convencional ajuda):

- `feat(associates): ...`
- `fix(platform): ...`
- `test(associations): ...`
- `chore(infra): ...`

Nenhum commit deve depender de outro commit quebrado para “fazer sentido”.

---

## 16. Checklist antes de concluir uma entrega

- `dotnet build` ok
- `dotnet test` ok quando a mudança for comportamental
- migrações revistas se o modelo mudou
- controllers novos registados no `Program.cs` quando aplicável
- sem vazamento de segredos em logs ou outputs
- documentação actualizada só se necessário (spec/contratos)

---

## 17. O que actualizar neste ficheiro ao longo do projecto

Actualizar `AGENTS.md` quando surgir:

- nova convenção transversal
- decisão arquitectural importante (ex.: novo módulo, novo padrão de auth em plataforma)
- armadilha recorrente (multitenancy, migrações, Identity)
- comando operacional novo obrigatório

---

## 18. Regra final

**Velocidade** neste projecto significa:

- mudar pouco de cada vez
- validar rápido (build/test)
- corrigir cedo
- preservar isolamento de tenant e integridade da API

---

## 19. Documentação

### Especificação e produto

- **Especificação funcional (MVP SaaS associações):** [docs/llm_implementation_spec_saa_s_associacoes_esportivas.md](docs/llm_implementation_spec_saa_s_associacoes_esportivas.md) — tratar como **requisitos de produto**; se divergir do código, confirmar com o utilizador ou alinhar a implementação.
- **Setup e execução:** [README.md](README.md)

### Documentação frontend (API consumida pelo cliente)

- Pasta [docs/frontend/](docs/frontend/) — um ficheiro por **controller** com payloads e respostas esperadas (`ApiResponse`).

### Evitar

- Não colocar código-fonte em `docs/` sem pedido explícito.
- Não editar ficheiros em `.cursor/plans/` por iniciativa própria quando o pedido for só implementação.
