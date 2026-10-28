# AGENT GUIDELINES & SPECIFICATION MANUAL: AGENTS.MD
# Diretrizes para LLMs, Agentes Autônomos e Engenheiros de Software (BabaPlay)

Este documento define as regras inegociáveis, convenções arquiteturais, diretrizes de segurança, padrões de engenharia de software e processos operacionais que qualquer agente de IA ou desenvolvedor humano **DEVE** seguir estritamente ao criar, evoluir, refatorar ou testar o código do ecossistema **BabaPlay**.

---

## 1. VISÃO GERAL & STACK TECNOLÓGICA

O **BabaPlay** é uma plataforma SaaS multitenant voltada para a gestão integral de peladas (jogos amadores de futebol), associações esportivas, ligas e times. A solução abrange gestão de associados, organização de partidas/game days, check-in com geolocalização, estatísticas e rankings, gestão financeira (mensalidades e caixa), comunicação em tempo real e geração de súmulas.

### 1.1 Backend
* **Runtime / Framework:** .NET 10 (C# 13+, nullable reference types habilitados, file-scoped namespaces, implicit usings).
* **Persistência de Dados:** PostgreSQL (produção/deploy via EF Core 10) e SQL Server / In-Memory / SQLite (testes automatizados).
* **Autenticação & Autorização:** ASP.NET Core Identity integrado ao EF Core, com tokens JWT, refresh tokens e isolamento rígido por Tenant (RBAC granular).
* **Comunicação em Tempo Real:** SignalR hubs para atualizações de partidas, chat de times e notificações push.
* **Padrões de Comunicação:** Padrão Result (`Result<T>` e `Result`) para controle de fluxo de negócio sem propagação de exceções para regras esperadas.
* **Design Arquitetural:** Clean Architecture combinada a Monólito Modular, CQRS (Command Query Responsibility Segregation) via MediatR e princípios DDD (Domain-Driven Design).
* **Identificadores Únicos:** Chaves primárias e relacionais no formato **UUID (`Guid` v4 ou sequencial v7)** geradas preferencialmente no domínio.

### 1.2 Frontend
* **Frontend Principal (Novo / Alvo):** Blazor WebAssembly (.NET 10) desacoplado, localizado em `Backend/src/BabaPlay.Web`. Componentes isolados, Tailwind CSS / Scoped CSS e consumo da API via DTOs compartilhados.
* **Frontend Legado (Em migração):** React 18 + TypeScript + Vite, localizado no diretório raiz `web/`. Qualquer nova funcionalidade deve ser priorizada no projeto Blazor, mantendo o React apenas para compatibilidade legada conforme o roadmap de migração.
* **Comunicação:** Clientes HTTP tipados consumindo os endpoints REST da API através de contratos e DTOs bem definidos.

### 1.3 Engenharia de Qualidade
* **Metodologia:** Test-Driven Development (TDD) estrito (Red-Green-Refactor) com xUnit, FluentAssertions, Moq/NSubstitute e WebApplicationFactory.
* **Documentação Viva:** Living Documentation inegociável mantida no mesmo commit do código-fonte (`CHANGELOG.md`, `roadmap.md`, `blazor_functions_guide.md`, `functions.md`, `architecture.md` e diagramas Mermaid).

---

## 2. ARQUITETURA & PRINCÍPIOS FUNDAMENTAIS

### 2.1 Desacoplamento Estrito (Front-end vs Back-end)
* O Frontend (Blazor WebAssembly ou React) e o Backend (API REST) são sistemas desacoplados que se comunicam exclusivamente via rede (HTTP/REST e WebSockets/SignalR).
* O Frontend **NUNCA** acessa o banco de dados, `DbContext`, repositórios ou Identity diretamente.
* Modelos e entidades internas de domínio do Backend **NUNCA** devem vazar para o Frontend. A comunicação deve utilizar estritamente DTOs (Data Transfer Objects) e contratos tipados.

### 2.2 Clean Architecture & Monólito Modular
A solução organiza o código do Backend em camadas concêntricas com limites estritos de dependência:

1. **`BabaPlay.Domain` (Núcleo Puro):**
   * Contém Entidades, Agregados (`IAggregateRoot`), Value Objects, Eventos de Domínio (`IDomainEvent`), Enums e Interfaces de Repositório (`IRepository<T>`).
   * **Zero dependências externas:** Sem referências a EF Core, ASP.NET Core, HTTP ou bibliotecas de terceiros.
2. **`BabaPlay.Application` (Casos de Uso & Orquestração):**
   * Contém Commands, Queries, Handlers (MediatR), DTOs, Validadores (FluentValidation), Interfaces de Serviços e o catálogo de permissões (`RbacCatalog`).
   * Retorno obrigatório de operações de negócio encapsulado no padrão `Result<T>` ou `Result`.
3. **`BabaPlay.Infrastructure` (Mundo Externo & Adaptadores):**
   * Implementação de persistência com EF Core (`AppDbContext`), Migrations, Repositórios concretos, clientes de e-mail (Resend), armazenamento (Cloudinary), serviços de autenticação e workers em segundo plano.
4. **`BabaPlay.Api` (Entrada da Aplicação):**
   * Controllers REST / Minimal APIs, Middlewares (`TenantMiddleware`), filtros globais, documentação OpenAPI/Swagger e configuração do pipeline HTTP e SignalR hubs.

### 2.3 Padrão CQRS (Command Query Responsibility Segregation)
* **Controllers Finos (Skinny Controllers):** Controllers e Minimal APIs atuam unicamente como portas de entrada (Entrypoints). Eles recebem a requisição, associam o contexto e despacham via `IMediator`. Nenhuma regra de negócio deve residir nos controllers.
* **Commands (Escrita / Mutação):**
  * Operações que alteram o estado do sistema (Create, Update, Delete).
  * Validam regras de negócio e invariantes do domínio.
  * Utilizam repositórios e o padrão Unit of Work para persistência atômica.
  * Retornam `Result` ou `Result<T>` (geralmente contendo o ID ou DTO de confirmação).
* **Queries (Leitura / Consulta):**
  * Operações que apenas consultam o estado, sem gerar efeitos colaterais.
  * Devem utilizar `AsNoTracking()` no EF Core ou projeções otimizadas para garantir máxima performance.
  * Retornam DTOs de leitura dedicados.

### 2.4 Princípios SOLID & DRY
* **Single Responsibility (SRP):** Cada classe, command handler, query handler e componente Blazor deve ter um único motivo para mudar.
* **Open/Closed (OCP):** Comportamentos devem ser extensíveis por meio de abstrações e injeção de dependência, sem modificar código estável já testado.
* **Liskov Substitution (LSP):** Implementações devem satisfazer integralmente os contratos das interfaces base.
* **Interface Segregation (ISP):** Crie interfaces coesas e específicas para cada porta de comunicação.
* **Dependency Inversion (DIP):** As camadas de Domínio e Aplicação dependem exclusivamente de abstrações; a Infraestrutura implementa os contratos.
* **Don't Repeat Yourself (DRY):** Evite duplicações de lógica de validação e cálculo; reutilize contratos e regras no núcleo de negócio.

---

## 3. MULTI-TENANCY & SEGURANÇA EM PROFUNDIDADE

O BabaPlay é uma plataforma SaaS multitenant sob banco compartilhado (*Shared Database, Shared Schema*). A violação de isolamento entre inquilinos (Tenants) é classificada como **vulnerabilidade crítica gravíssima**.

### 3.1 Identificador de Tenant & Modelo de Dados
* Todas as entidades de negócio pertencentes a uma associação devem conter a propriedade `TenantId` do tipo `Guid`.
* O `TenantId` é obrigatório, indexado e nunca deve ser alterado após a criação do registro.

### 3.2 Resolução Segura de Tenant no Pipeline HTTP
* A API resolve o tenant ativo a cada requisição HTTP através do `TenantMiddleware`.
* Clientes HTTP (Blazor e React) devem enviar compulsoriamente o header `X-Tenant-Slug` com o slug da associação ativa.
* Em requisições autenticadas, a API valida adicionalmente a associação do usuário ao tenant através das claims do token JWT.
* **Regra Anti-Adulteração:** O `TenantId` **NUNCA** deve ser aceito cegamente do payload (JSON) ou de query params informados pelo cliente para entidades com escopo de tenant. O backend deve sempre forçar o `TenantId` resolvido a partir do contexto seguro validado (`ITenantContext`).

### 3.3 Global Query Filters Compulsórios no EF Core
* O `AppDbContext` **DEVE** aplicar Global Query Filters em **TODAS** as entidades tenant-scoped:
  ```csharp
  builder.Entity<Player>().HasQueryFilter(e => CurrentTenantId == Guid.Empty || e.TenantId == CurrentTenantId);
  builder.Entity<Match>().HasQueryFilter(e => CurrentTenantId == Guid.Empty || e.TenantId == CurrentTenantId);
  ```
* **Proibição:** É terminantemente proibido utilizar `.IgnoreQueryFilters()` em consultas normais de negócio. Seu uso é restrito a rotas exclusivas de superadministração global devidamente auditadas.

### 3.4 Gravação Segura (Injeção de TenantId)
* Nos Command Handlers e na camada de persistência, garanta que qualquer entidade recém-criada receba o `TenantId` corrente do contexto antes de ser persistida.
* Nunca permita que uma entidade seja gravada com `TenantId` nulo ou vazio quando ela for tenant-scoped.

### 3.5 Testes Obrigatórios contra Cross-Tenant Data Leak
* Qualquer novo endpoint, command ou query **DEVE** possuir testes automatizados de integração que validem explicitamente o isolamento de dados:
  1. Criação do recurso autenticado como **Tenant A**.
  2. Tentativa de leitura, alteração ou exclusão do mesmo recurso estando autenticado como **Tenant B**.
  3. O teste **DEVE** validar que o **Tenant B** recebe `404 Not Found` (ou resultado vazio), jamais vazando a existência ou dados do **Tenant A**.

### 3.6 Autenticação e Conexões SignalR em Tempo Real
* Conexões com hubs SignalR (em `/hubs/*`) devem enviar o header `X-Tenant-Slug` na negociação inicial.
* No Blazor WebAssembly / browser, como conexões WebSocket não suportam envio de headers arbitrários de autorização na abertura, o token JWT trafega via query string `?access_token=`. A API deve capturar e validar esse token no evento `JwtBearerEvents.OnMessageReceived`.

### 3.7 Escopo de Injeção de Dependência no Blazor WebAssembly
* No Blazor WASM, o `HttpClient` autenticado **DEVE** ser resolvido dentro do mesmo escopo da interface de usuário (UI scope), e **NÃO** através de instâncias desacopladas de `IHttpClientFactory` com `DelegatingHandler` sem escopo.
* O `IHttpClientFactory` padrão cria um escopo separado onde instâncias singleton/scoped como `UserSessionState` e `TenantState` são perdidas, resultando em cabeçalhos `Authorization: Bearer` e `X-Tenant-Slug` vazios, culminando em erros `401 Unauthorized` ou `403 Forbidden` generalizados.

### 3.8 RBAC (Role-Based Access Control) & Catálogo de Permissões
* **Defesa em Profundidade:** A segurança é garantida em múltiplos níveis:
  * **Frontend:** Controla visibilidade de menus e botões via `<AuthorizeView Policy="...">`, `@attribute [Authorize]` nas páginas sensíveis (ex.: `Pages/Financial/*`) e policies registradas em `AddAuthorizationCore` via `ClientAuthorizationPolicies`.
  * **Backend (Autoridade Real):** A API valida permissões tenant-scoped de forma inviolável via `PermissionAuthorizationHandler` e policies como `FinancialRead`, `TenantMember`, etc.
* **Status Codes HTTP Estritos:**
  * Usuário não autenticado ou token ausente/expirado $\rightarrow$ **`401 Unauthorized`**.
  * Usuário autenticado mas sem a permissão necessária para o recurso $\rightarrow$ **`403 Forbidden`**.
* **Sincronização do Catálogo RBAC:**
  * Todas as permissões e roles oficiais do sistema devem estar cadastradas em `BabaPlay.Application.Common.RbacCatalog`.
  * Ao criar um novo tenant, `EnsureOwnerAdminAccessAsync` provisiona as roles básicas e concede acesso de Admin ao criador.
  * Novas permissões introduzidas no `RbacCatalog` são propagadas automaticamente para todos os tenants pré-existentes na inicialização da API via `ITenantRbacCatalogSyncService.SyncAllTenantsAsync()`.

---

## 4. CONVENÇÃO DE IDENTIFICADORES: UUID / GUID

* **Identificadores Únicos:** Toda entidade de domínio, agregado e chave estrangeira deve utilizar identificadores do tipo `Guid` (UUID).
* **Geração no Domínio:** Os IDs devem ser gerados no momento da instanciação da entidade (`Guid.NewGuid()`), garantindo rastreabilidade, imutabilidade e identificação antes do commit no banco de dados.
* **Chaves de Banco:** O mapeamento no banco de dados deve utilizar o tipo nativo de identificador único (ex.: `uuid` no PostgreSQL, `uniqueidentifier` no SQL Server).

---

## 5. PADRÃO RESULT & TRATAMENTO DE ERROS

### 5.1 Proibição de Exceções para Controle de Fluxo
* **Regra Fundamental:** Exceções (`Exceptions`) **NÃO** devem ser utilizadas para controlar fluxo de negócio normal ou reportar falhas de validação.
* Exceções são restritas exclusivamente a falhas técnicas inesperadas e catastróficas (ex.: perda de conexão com o banco de dados, falha de I/O em disco).
* Todas as operações de Casos de Uso, Commands, Queries e regras de domínio devem retornar o encapsulador tipado `Result` ou `Result<T>` localizado em `BabaPlay.Application.Common`:
  ```csharp
  // Sucesso
  return Result<MatchDto>.Ok(matchDto);
  return Result.Ok();

  // Falha
  return Result<MatchDto>.Fail("Match.NotFound", "A partida solicitada não foi encontrada.");
  return Result.Fail("Checkin.Closed", "O período de check-in para este jogo já foi encerrado.");
  ```

### 5.2 Mapeamento de Status Codes na Camada de API
Os Controllers REST devem avaliar o `Result<T>` retornado pelo Mediator e convertê-lo no status code HTTP apropriado:
* `Result.Ok()` com carga útil $\rightarrow$ **`200 OK`** ou **`201 Created`**.
* `Result.Ok()` sem carga útil $\rightarrow$ **`204 NoContent`**.
* Falha com erro de recurso inexistente (`*.NotFound`) $\rightarrow$ **`404 NotFound`**.
* Falha por violação de regra de negócio ou validação (`*.Validation`, `*.Invalid`) $\rightarrow$ **`400 BadRequest`** ou **`422 UnprocessableEntity`**.
* Falha por conflito de estado concorrente (`*.Conflict`, `*.AlreadyExists`) $\rightarrow$ **`409 Conflict`**.
* Falha de autorização ou tenant inválido $\rightarrow$ **`403 Forbidden`**.

---

## 6. PADRÃO REPOSITORY & PERSISTÊNCIA

* **Abstração por Aggregate Root:** Repositórios operam sobre agregados de domínio (`IRepository<T>` onde `T : IAggregateRoot`).
* **Não Injetar DbContext em Handlers:** Handlers de aplicação devem interagir com o banco de dados exclusivamente por meio de interfaces de repositório (`IPlayerRepository`, `IMatchRepository`, etc.) ou abstrações de consulta, nunca injetando `AppDbContext` diretamente.
* **Proibição de Exposição de IQueryable:** Repositórios não devem vazar `IQueryable` para camadas superiores. As interfaces devem expor métodos com nomes expressivos orientados ao negócio, retornando `Task<T?>`, `Task<IReadOnlyList<T>>` ou DTOs de leitura.
* **Transações e Atomicidade:** Alterações que envolvem múltiplos agregados devem ser coordenadas pelo padrão `IUnitOfWork`, assegurando que o commit seja atômico.

---

## 7. ESTRUTURA DO PROJETO & MONOREPO

```text
/
├── Backend/                                # Solução .NET 10 (Clean Architecture + Monólito Modular)
│   ├── BabaPlay.slnx                       # Arquivo de solução
│   └── src/
│       ├── BabaPlay.Domain/                # Núcleo puro (Entidades, Agregados, Enums, Eventos)
│       ├── BabaPlay.Application/           # Casos de uso (Commands, Queries, DTOs, RbacCatalog, Result)
│       ├── BabaPlay.Infrastructure/        # Persistência (AppDbContext, Migrations, Repositórios, Workers)
│       ├── BabaPlay.Api/                   # Entrypoint HTTP (Controllers, Middlewares, SignalR Hubs)
│       ├── BabaPlay.Web/                   # Frontend moderno em Blazor WebAssembly (.NET 10)
│       └── BabaPlay.Tests/                 # Testes de Unidade, Integração e Arquitetura
├── web/                                    # Frontend legado em React 18 + TypeScript (Vite)
├── deploy/                                 # Configurações de Docker, Nginx e deploy
├── scripts/                                # Scripts de automação e validação local
├── docs/                                   # Documentação técnica complementar e ADRs
├── agents.md                               # Este manual de diretrizes para Agentes e LLMs
├── blazor_functions_guide.md               # Guia mestre da migração de telas para Blazor
├── functions.md                            # Catálogo funcional e mapeamento CQRS
├── architecture.md                         # Documento de arquitetura geral do sistema
├── roadmap.md                              # Roadmap e planejamento evolutivo de produto
└── CHANGELOG.md                            # Registro cronológico de mudanças e versões
```

---

## 8. ENGENHARIA DE QUALIDADE, TDD & TESTES AUTOMATIZADOS

O desenvolvimento de novas funcionalidades e correções de bugs deve adotar a disciplina de **Test-Driven Development (TDD)**:
1. **Red:** Escreva um teste que expresse a necessidade do negócio e observe o teste falhar.
2. **Green:** Implemente o código mínimo necessário para fazer o teste passar com sucesso.
3. **Refactor:** Elimine duplicações, aprimore o design e garanta que toda a suíte de testes continue verde.

### 8.1 Categorias de Testes Obrigatórias
* **Testes de Unidade (`BabaPlay.Tests/Unit`):**
  * Validam regras de domínio, cálculos estatísticos (artilharia, notas, MVP), balanceamento de times e command handlers isolados via mocks.
* **Testes de Integração (`BabaPlay.Tests/Integration`):**
  * Validam a interação real com o banco de dados (EF Core InMemory/SQLite/Postgres), pipeline de controllers, autenticação, middlewares e autorização.
  * **Testes de Isolamento Multi-tenant:** Testam compulsoriamente a ausência de vazamento de dados entre diferentes tenants.
* **Testes de Componentes Blazor:**
  * Validam renderização condicional, estados de carregamento e interação com formulários usando bUnit quando aplicável.
* **Critério de Validação:** Nenhuma tarefa de backend pode ser finalizada sem executar a suíte de testes com sucesso (`dotnet test Backend/`).

---

## 9. DOCUMENTAÇÃO VIVA (LIVING DOCUMENTATION) - INEGOVIÁVEL

A documentação do **BabaPlay** é um componente vivo, versionado e indissociável do código-fonte. **Nenhuma funcionalidade ou refatoração é considerada concluída sem que a documentação correspondente seja atualizada no mesmo commit**.

### 9.1 Sincronização Obrigatória de Arquivos
Ao alterar, criar ou refatorar funcionalidades, o agente deve sincronizar os seguintes artefatos:
* **`CHANGELOG.md`:** Registre as novas funcionalidades, melhorias de segurança ou correções sob a versão atual ou seção `[Unreleased]`, seguindo o padrão *Keep a Changelog*.
* **`roadmap.md`:** Atualize o status dos épicos, fases e tarefas concluídas com seus respectivos testes e datas.
* **`blazor_functions_guide.md`:** Mantenha atualizados o status e os contratos dos componentes Blazor em processo de migração.
* **`functions.md`:** Atualize os mapeamentos de Commands, Queries e regras de negócio de cada módulo.
* **`architecture.md`:** Atualize decisões de design caso haja alteração de topologia, infraestrutura ou padrões arquiteturais.

### 9.2 ADRs (Architecture Decision Records)
* Qualquer mudança substancial de tecnologia, introdução de nova biblioteca de infraestrutura, alteração no modelo de isolamento de tenants ou mudança no padrão de autorização exige a criação de um documento em `docs/architecture/ADR-xxxx.md`.

### 9.3 Diagramas Mermaid
* Casos de uso complexos (ex.: ciclo de vida de uma partida, fluxo de check-in com tolerância de geolocalização, algoritmo de sorteio de times e conciliação de mensalidades) devem incluir diagramas Mermaid (`flowchart`, `sequenceDiagram` ou `stateDiagram-v2`) no arquivo de documentação correspondente.

---

## 10. REGRAS DE ESTILO & CÓDIGO LIMPO

* Utilize recursos modernos e idiomáticos do **C# 13+ e .NET 10** (file-scoped namespaces, pattern matching, records para DTOs, primary constructors onde aumentarem a legibilidade).
* Mantenha nomes de classes, métodos, propriedades e variáveis em **inglês**, utilizando nomenclaturas expressivas e alinhadas à linguagem ubíqua do futebol/pelada (`GameDay`, `Match`, `Player`, `Team`, `Checkin`, `MonthlyFee`).
* Métodos e classes devem ser curtos e focados exclusivamente em sua responsabilidade.
* Elimine código morto, comentários obsoletos e dependências não utilizadas.

---

## 11. CHECKLIST OPERACIONAL PARA O AGENTE DE IA

Antes de considerar qualquer instrução ou tarefa como finalizada, o agente autônomo **DEVE** executar mentalmente e validar o seguinte checklist:

* [ ] **Multi-tenant Integrity:** A operação valida o `TenantId` a partir do contexto seguro (`ITenantContext` / `X-Tenant-Slug`)? Não há risco de cross-tenant data leak?
* [ ] **Query Filter Check:** Alguma consulta utilizou `.IgnoreQueryFilters()` sem justificativa explícita? As entidades tenant-scoped possuem filtro global?
* [ ] **UUID / Guid Check:** Todas as novas entidades e chaves relacionais utilizam `Guid` gerados no domínio?
* [ ] **Result Pattern:** O método retorna `Result` ou `Result<T>` para falhas de negócio esperadas em vez de lançar exceptions?
* [ ] **CQRS & Clean Architecture:** A regra reside no Handler/Domain em vez de no Controller? Os controllers apenas despacham para o Mediator?
* [ ] **Repository Abstraction:** O `AppDbContext` foi injetado indevidamente no Handler ou foi utilizada a abstração por repositório?
* [ ] **Frontend Decoupling:** O código Blazor reside estritamente em `Backend/src/BabaPlay.Web` e consome a API exclusivamente via HTTP/DTOs?
* [ ] **Blazor Scoped HttpClient:** O `HttpClient` autenticado do Blazor está registrado no escopo correto para não perder os estados de sessão/tenant?
* [ ] **RBAC & Authorization:** As rotas e handlers validam as devidas policies (`TenantMember`, `FinancialRead`, etc.) e retornam `401` ou `403` corretamente?
* [ ] **TDD & Test Execution:** Foi implementado o teste automatizado para a regra de negócio ou correção de bug? Os testes executam com sucesso (`dotnet test Backend/`)?
* [ ] **Cross-Tenant Test:** Há teste garantindo que o Tenant B não consegue ler nem modificar o recurso do Tenant A?
* [ ] **Living Documentation:** Os arquivos `CHANGELOG.md`, `roadmap.md`, `blazor_functions_guide.md` e `functions.md` foram devidamente atualizados e sincronizados?