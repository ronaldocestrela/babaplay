# Arquitetura do BabaPlay Backend
## API multitenant (.NET 10) | SQL Server | EF Core | Identity por tenant

## 1. Objetivo

Definir uma arquitetura clara, alinhada ao código e à especificação do MVP, compatível com desenvolvimento assistido por IA e com entregas incrementais.

Este documento descreve:

- visão do produto e do repositório
- arquitetura de software (monólito modular)
- multitenancy e persistência dupla (platform + tenant)
- módulos de domínio
- regras de negócio principais (síntese)
- estrutura física da solução
- fluxos prioritários
- decisões técnicas e riscos

A especificação detalhada de implementação para LLMs continua em [`llm_implementation_spec_saa_s_associacoes_esportivas.md`](llm_implementation_spec_saa_s_associacoes_esportivas.md). Em caso de divergência entre este documento, a spec e o código, prevalece o alinhamento explícito com o utilizador.

---

## 2. Resumo executivo

O **BabaPlay** é um **SaaS multitenant** para **gestão de associações esportivas** (MVP).

### Características principais

- **Isolamento forte:** banco **central (platform)** para catálogo SaaS + **um banco SQL Server por tenant** com dados da associação e Identity.
- **API REST** em ASP.NET Core; clientes web ou móveis consomem a API (o frontend não vive neste repositório).
- **Identificação de tenant:** subdomínio (produção) ou cabeçalho `X-Tenant-Subdomain` (desenvolvimento).
- **Rotas de plataforma** sem contexto de tenant: prefixo `/api/platform/*` (gestão de tenants, planos, subscriptions, provisionamento).
- **Autenticação:** JWT Bearer; **autorização** baseada em permissões (policies dinâmicas).
- **CORS** com origens permitidas persistidas na base **platform** (`AllowedOrigins`).
- **Domínio típico do MVP:** associação, associados, posições, mensalidades/pagamentos, sessões de check-in, geração de equipas a partir de check-ins, entradas financeiras simples.

---

## 3. Diretrizes arquiteturais

### 3.1. Estilo geral

**Monólito modular em camadas:** `BabaPlay.Api` expõe HTTP; `BabaPlay.Modules.*` concentra casos de uso e controllers; `BabaPlay.Infrastructure` concentra EF Core, multitenancy, Identity e integrações; `BabaPlay.SharedKernel` concentra primitivas transversais (`Result`, `BaseController`, contratos web).

Não há CQRS/MediatR como eixo obrigatório no código atual: a orquestração é feita por **serviços de módulo** com retornos **`Result` / `Result<T>`**.

### 3.2. Objetivos da arquitetura

- clareza de responsabilidades por módulo
- **isolamento de dados por tenant** como invariante
- manutenção simples e evolução incremental
- boa testabilidade dos serviços (unitários com mocks)
- compatibilidade com trabalho assistido por IA

### 3.3. Princípios

- **Tenant antes de funcionalidade:** qualquer leitura/escrita de negócio da associação passa pelo contexto de tenant resolvido.
- **Platform separado:** dados de catálogo SaaS não se misturam com dados de associação nos fluxos normais.
- **Contratos HTTP estáveis:** respostas de negócio encapsuladas em `ApiResponse<T>` (sucesso/erro coerentes).
- **Persistência explícita:** duas famílias de migrações EF (`Platform` e `Tenant`), pastas distintas sob `Persistence/Migrations/`.

---

## 4. Arquitetura lógica

```text
[ Clientes HTTP (SPA, mobile, ferramentas) ]
      |
      v
[ ASP.NET Core - BabaPlay.Api ]
      |
      +--> Autenticação JWT
      +--> CORS dinâmico (origens na base platform)
      +--> TenantResolutionMiddleware
      |         |
      |         +--> /api/platform/*  -->  PlatformDbContext (sem tenant)
      |         |
      |         +--> demais rotas      -->  resolve subdomain
      |                                   -->  TenantDbContext (por tenant)
      v
[ Módulos BabaPlay.Modules.* ]
      |
      v
[ BabaPlay.Infrastructure ]
      |
      +--> SQL Server (base PLATFORM: tenants, plans, subscriptions, CORS, ...)
      +--> SQL Server (bases TENANT: uma por associação, Identity, dados de negócio)
      +--> Repositórios IPlatformRepository<T> / ITenantRepository<T>
      +--> Provisionamento / migrações de bases tenant (startup e criação de tenant)
```

---

## 5. Stack tecnológica

### 5.1. Backend (este repositório)

- .NET 10
- ASP.NET Core Web API
- Entity Framework Core (Code First)
- SQL Server
- ASP.NET Core Identity (por tenant)
- JWT Bearer

### 5.2. Persistência

- **Platform:** `PlatformDbContext` — tenants, planos, subscrições, origens CORS, etc.
- **Tenant:** `TenantDbContext` — dados da associação, utilizadores, permissões de negócio, etc.

### 5.3. Integrações típicas

- **E-mail** transacional (ex.: Resend) via configuração (`appsettings` / variáveis de ambiente).
- **Docker** para empacotamento da API (ver `README.md` e `docker-compose`).

---

## 6. Multitenancy e resolução de pedidos

### 6.1. Identificação do tenant

O `TenantResolutionMiddleware` (Infrastructure):

- Para caminhos de **plataforma** (`/api/platform`, `/swagger`, `/health`): define contexto de plataforma **sem** tenant.
- Caso contrário: obtém o **slug** do tenant (subdomínio do host, cabeçalho `X-Tenant-Subdomain`, ou parâmetro `tenant` conforme implementação em `TenantSlugResolver`).
- Valida o tenant na base **platform** e define a **connection string** da base do tenant no `ITenantProvider` (escopo do pedido).

### 6.2. Connection string por tenant

A connection string efetiva do tenant deriva tipicamente da connection string “platform” alterando o catálogo inicial (`InitialCatalog`) para o `DatabaseName` registado no tenant.

### 6.3. Migrações

- Migrações EF em `src/BabaPlay.Infrastructure/Persistence/Migrations/` — pastas **Platform** e **Tenant**.
- Ao arranque, a base platform é migrada; um serviço em background pode aplicar migrações pendentes às bases tenant registadas (falhas num tenant não devem passar despercebidas em análise — ver logs).
- Novos tenants criados via fluxo de subscrição são provisionados com a base e migrações aplicadas no momento adequado.

---

## 7. Identidade, permissões e autorização

### 7.1. Modelo

- **Identity** vive no **contexto tenant** (utilizadores da associação).
- **JWT** emitido após autenticação; claims e **permissões** alimentam policies (`PermissionPolicyProvider`, `PermissionAuthorizationHandler`).
- **`IPermissionResolver`** centraliza resolução de permissões (acesso a `DbContext` apenas na Infrastructure, padrão já adoptado).

### 7.2. Plataforma

Rotas `/api/platform/*` são usadas para operação do SaaS (tenants, planos, subscriptions). **No MVP**, podem estar permissivas para facilitar setup — **endurecer antes de produção** (autenticação de serviço, API keys ou regras de rede). Ver notas em `README.md`.

### 7.3. Exemplos de perguntas de desenho

- O utilizador pertence ao tenant corretamente autenticado?
- A permissão exige apenas papel ou também estado do recurso (ex.: associado activo)?
- A operação é de plataforma ou de tenant?

---

## 8. Módulos do sistema (bounded contexts)

### 8.1. Platform (`BabaPlay.Modules.Platform`)

- gestão de **planos**, **tenants**, **subscriptions**
- fluxos que provisionam base tenant e aplicam modelo de dados tenant

### 8.2. Associations

- dados da **associação** no tenant (nome, morada, regulamento, **`PlayersPerTeam`**, etc.)

### 8.3. Identity (módulo)

- extensões e integração com perfis de utilizador no contexto do tenant (alinhado à spec: `UserType`, ligação a `AssociateId`, etc.)

### 8.4. Associates

- **associados**, **posições**, regras de quantidade de posições (ex.: mínimo 1, máximo 3 conforme spec)

### 8.5. Check-ins

- **sessões de check-in** e registos de **check-in** (ex.: 1 check-in por dia por associado — regra de negócio da spec)

### 8.6. Team generation

- **equipas** e **membros**; serviço de geração com algoritmo definido na spec (ordenação por primeira hora de check-in na sessão, `PlayersPerTeam`, round-robin, número mínimo de equipas)

### 8.7. Financial

- movimentos financeiros simples (ex.: entradas, categorias) conforme modelo do MVP

### 8.8. Dependências entre módulos

Preferência: cada módulo referencia `BabaPlay.SharedKernel`. Referências cruzadas entre módulos **só** quando o domínio o exige (ex.: `TeamGeneration` → `CheckIns`, `Associations`). `BabaPlay.Infrastructure` referencia SharedKernel e **todos** os módulos.

---

## 9. Modelagem de domínio (síntese)

### 9.1. Entidades principais na base **platform** (exemplos)

- `Plan`, `Tenant`, `Subscription`, `AllowedOrigin`, …

### 9.2. Entidades principais na base **tenant** (exemplos, conforme spec)

- `Association`
- `Associate`, `Position`, relação associado–posição
- `Membership`, `Payment`
- `CheckInSession`, `CheckIn`
- `Team`, `TeamMember`
- modelo de **roles/permissions** (roles configuráveis, seed de permissões)
- entidades financeiras (`CashEntry`, `Category`, …)

### 9.3. Regras de modelagem

- Todas as entidades de negócio seguem o padrão de entidade base adoptado no projeto (ex.: `BaseEntity` com `Id`, auditoria de criação/atualização, conforme spec).
- Alterações ao modelo exigem actualização dos `DbContext` correctos e nova migração na pasta certa.

---

## 10. Camada de aplicação e padrões de API

### 10.1. Serviços e `Result`

- Serviços nos módulos expõem operações que devolvem **`Result` / `Result<T>`** (erros de negócio explícitos).
- Controllers derivam de **`BaseController`** e mapeiam resultados para **`ApiResponse<T>`** (sucesso e falhas coerentes).

### 10.2. Controllers e descoberta

- Novos controllers em novos assemblies de módulo devem ser registados via **`AddApplicationPart`** em `Program.cs`.

### 10.3. Repositórios

- Acesso a dados via **`IPlatformRepository<T>`** e **`ITenantRepository<T>`**, não por exposição directa de `DbContext` aos módulos (excepto casos já existentes e justificados na Infrastructure).

---

## 11. Estrutura física do repositório (backend)

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

tests/
  BabaPlay.Tests.Unit/

docs/
  llm_implementation_spec_saa_s_associacoes_esportivas.md
  frontend/                 # contratos por controller (API para clientes)
  Arquitetura_BabaPlay_Backend.md
```

Organização interna de cada módulo segue o já adoptado (Controllers, Services, entidades, etc.) — ver código-fonte para pormenores.

---

## 12. Clientes HTTP e documentação de API

Este repositório **não** contém a aplicação SPA. Os contratos para equipas de frontend estão em **`docs/frontend/`** (tipicamente um ficheiro por controller).

Exemplos úteis:

- `associations-controller.md` — `playersPerTeam`
- `teams-controller.md` — geração de equipas com `sessionId`

---

## 13. Fluxos críticos de negócio

### 13.1. Criar tenant e subscrição (plataforma)

1. Operador chama API de plataforma (tenant/plan/subscription).
2. Sistema persiste metadados na base **platform**.
3. Provisionamento cria (ou prepara) a base do tenant e aplica migrações **Tenant**.
4. Associação pode então ser configurada no contexto do novo tenant.

### 13.2. Configurar associação

1. Utilizador autenticado no tenant define dados da associação, incluindo **`PlayersPerTeam`** (mínimo 2, default 5 na spec).
2. Dados persistidos apenas na base do **tenant**.

### 13.3. Check-in e geração de equipas

1. Abrir sessão de check-in; associados registam presença (regra: **1 check-in por dia por associado**).
2. Geração de equipas: ler participantes da **sessão**, ordenar pela **primeira** hora de check-in, aplicar **`PlayersPerTeam`** e algoritmo de equipas/número de equipas conforme spec (divisão inteira, mínimo de equipas, round-robin).

### 13.4. Autenticação e autorização típica

1. Pedido inclui tenant resolvido + JWT.
2. Handler de permissão avalia policy adequada ao recurso.

---

## 14. CORS e segurança operacional

- Origens permitidas carregadas da tabela **platform**; middleware/política de CORS dinâmica.
- **Segredos** (connection strings, JWT signing key, chaves de e-mail) apenas em configuração segura — nunca commitados.
- Validar endurecimento das rotas de plataforma antes de produção.

---

## 15. E-mail e notificações

Conforme configuração do MVP: envio transacional via fornecedor (ex.: Resend). Notificações in-app em tempo real **não** são o foco deste documento se não estiverem modeladas no código; evoluir conforme produto.

---

## 16. Persistência, backups e operações

- SQL Server como fonte de verdade.
- Estratégia de **backup** e **restauro** por base (platform + cada tenant) é responsabilidade da operação — documentar em runbooks à parte.
- Versionamento de API e release: ver `docs/VERSIONING.md`, `docs/RELEASE_PROCESS.md`, `CHANGELOG.md` quando aplicável.

---

## 17. Estratégia de testes

### 17.1. Backend

- **Unitários:** serviços com dependências substituídas (padrão em `BabaPlay.Tests.Unit`).
- **Integração:** quando existir projeto dedicado no futuro, cobrir pipelines EF, multitenancy e autorização; até lá, seguir o que o repositório já oferece.

### 17.2. Regra de ouro

Funcionalidades de negócio críticas (check-in, geração de equipas, permissões) devem ter **testes automatizados** antes de crescerem em complexidade.

---

## 18. Fases de implementação (referência da spec)

Ordem sugerida no documento de implementação (resumo):

1. **Fundação:** solução, BaseEntity/BaseController, EF, Identity, JWT, CORS dinâmico.
2. **Plataforma:** plans, tenants, subscriptions, provisionamento de base.
3. **Tenant setup:** associação.
4. **Identity** no tenant (utilizador ligado a associado quando aplicável).
5. **Permissões** (roles, seed).
6. **Associados** e posições.
7. **Mensalidades** (membership/payment).
8. **Check-ins.**
9. **Times** (geração).
10. **Financeiro** (entradas/categorias).

Esta ordem é **guia de produto**; o estado real do código pode estar noutra fase — validar no repositório.

---

## 19. Decisões importantes

1. **Multitenancy por base de dados** (isolamento forte).
2. **Dois DbContexts** e **duas famílias de migrações** (Platform / Tenant).
3. **Rotas de plataforma** sem resolução de tenant (`/api/platform/*`).
4. **JWT + permissões** por policy; `PermissionResolver` na Infrastructure.
5. **Sem microserviços** no MVP (monólito modular).
6. **Result + ApiResponse** como contrato uniforme de negócio na API.
7. **Geração de equipas** depende de **`PlayersPerTeam`** na associação e da sessão de check-in.
8. Frontend **fora** deste repo; contratos em `docs/frontend/`.

---

## 20. Riscos principais

| Risco | Mitigação |
|-------|-----------|
| Confusão entre dados platform e tenant | Convenções de rota, middleware explícito, code review |
| Falha parcial em migrações tenant | Monitorização de logs no arranque e no provisionamento |
| Rotas de plataforma expostas | Endurecer auth e rede antes de produção |
| Regra de negócio duplicada entre módulos | Serviços bem delimitados; testes nos pontos críticos |
| Connection strings e segredos vazados | Gestão por ambiente, rotação, nunca em repositório |

---

## 21. Conclusão

A arquitetura do BabaPlay Backend combina:

- **API ASP.NET Core** modular (`BabaPlay.Modules.*`)
- **multitenancy por banco** com middleware de resolução e `ITenantProvider`
- **EF Core** com migrações separadas para **platform** e **tenant**
- **Identity e JWT** no contexto do tenant
- **contratos HTTP** estáveis via `Result` / `ApiResponse<T>`

É adequada para **crescer por módulos**, manter **isolamento de dados** entre associações e suportar desenvolvimento assistido por IA com entregas **pequenas e verificáveis**.

---

## 22. Referências internas

- [`llm_implementation_spec_saa_s_associacoes_esportivas.md`](llm_implementation_spec_saa_s_associacoes_esportivas.md) — requisitos e fases detalhadas.
- [`../AGENTS.md`](../AGENTS.md) — regras operacionais para agentes e desenvolvedores.
- [`../README.md`](../README.md) — execução, Docker, migrações, multitenancy.
