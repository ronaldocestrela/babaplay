# Instruções para LLMs e Agentes de Código (Baba Play)

## Visão Geral do Projeto
O **Baba Play** é uma plataforma SaaS multitenant voltada para a gestão completa de associações esportivas, ligas e times de futebol. A aplicação deve suportar alto isolamento de dados entre os inquilinos (Tenants) e englobar módulos de gestão administrativa, financeira, logística esportiva e comunicação.

## Stack Tecnológico
*   **Linguagem:** C#
*   **Framework:** .NET 10
*   **Front-end:** Blazor (Desacoplado do Back-end)
*   **Back-end:** ASP.NET Core Web API
*   **ORM:** Entity Framework Core (EF Core)
*   **Autenticação/Autorização:** ASP.NET Core Identity (JWT / Token-based)
*   **Mensageria / Real-time (Opcional/Planejado):** SignalR

## Padrões Arquiteturais e Metodologia
Ao gerar, alterar ou sugerir código, você deve obrigatoriamente seguir as seguintes diretrizes:

### 1. Separação Estrita (Desacoplamento)
*   O Front-end (Blazor) e o Back-end (API) são projetos distintos.
*   O Blazor nunca deve acessar o banco de dados, o EF Core ou o Identity diretamente. Toda a comunicação deve ser feita exclusivamente via requisições HTTP para a API REST.
*   Modelos de domínio do Back-end não devem vazar para o Front-end. Utilize DTOs (Data Transfer Objects) para a comunicação.

### 2. TDD (Test-Driven Development)
*   Sempre adote a mentalidade *Red-Green-Refactor*.
*   Ao implementar uma nova regra de negócio, comece gerando os testes de unidade (usando xUnit ou NUnit e Moq/NSubstitute).
*   Garanta que a cobertura de testes foque nos casos de sucesso e nas falhas de validação de domínio.

### 3. Arquitetura CQRS
*   O projeto utiliza **CQRS** (Command Query Responsibility Segregation).
*   A lógica de negócio não deve ficar nos Controllers ou Minimal APIs. Os endpoints devem apenas receber a requisição e despachá-la via `MediatR` (ou padrão equivalente).
*   **Commands:** Alteram o estado (Create, Update, Delete) e retornam confirmações ou IDs. Usam repositórios de escrita.
*   **Queries:** Apenas lêem o estado, sem efeitos colaterais. Podem usar repositórios de leitura ou projeções diretas (Dapper ou AsNoTracking do EF) para maior performance.

### 4. Padrão Repository e Unit of Work
*   O acesso aos dados via EF Core deve ser abstraído pelo padrão Repository.
*   Não injete o `DbContext` diretamente nos Handlers do CQRS.
*   Mantenha contratos (Interfaces) claros para os repositórios (ex: `IAtletaRepository`, `ITenantRepository`).
*   Utilize o padrão Unit of Work para coordenar transações e garantir atomicidade ao salvar alterações no banco de dados.

### 5. Multitenancy
*   Todo o sistema é estruturado em formato SaaS multitenant.
*   Sempre garanta que as consultas e comandos validem ou incluam o `TenantId` correspondente ao contexto atual do usuário.
*   Clientes HTTP (Blazor e React) devem enviar o header `X-Tenant-Slug` com o slug da associação ativa; a API resolve o tenant via `TenantMiddleware`.
*   Conexões SignalR (hubs em `/hubs/*`) também devem enviar `X-Tenant-Slug` nos headers da negociação; no browser/WASM o JWT vai em `?access_token=` no WebSocket e a API deve ler esse token em `JwtBearerEvents.OnMessageReceived`.
*   No Blazor WASM, o `HttpClient` autenticado deve ser criado no mesmo scope da UI (não via `IHttpClientFactory` + `DelegatingHandler`): o factory cria um scope separado e o handler perde `UserSessionState`/`TenantState` (Bearer/`X-Tenant-Slug` vazios → 401/403 em todas as APIs).
*   Policies usadas em `<AuthorizeView Policy="...">` devem ser registradas em `AddAuthorizationCore` no Web (`ClientAuthorizationPolicies` + handler client-side). Hoje `CommunicationWrite` usa `TenantState.IsOwner` como gate de UI; a API continua validando RBAC de verdade.
*   Páginas Blazor sensíveis (ex.: módulo Financial em `Pages/Financial/*`) devem usar `@attribute [Authorize]` para redirecionar usuários não autenticados antes de chamar a API.
*   Novas permissões adicionadas em `RbacCatalog` são sincronizadas para tenants existentes via `ITenantRbacCatalogSyncService.SyncAllTenantsAsync()` na subida da API (após migrations). O bootstrap do owner (`EnsureOwnerAdminAccessAsync`) roda apenas na criação do tenant.
*   Endpoints financeiros exigem policy `FinancialRead` (`financial.read`) além de `TenantMember`. JWT válido sem essa permissão retorna **403**; Bearer ausente/expirado retorna **401**.
*   O isolamento de dados é prioridade máxima de segurança.

### 6. Documentação Viva e Atualização Contínua (Obrigatória)
*   **Documentação é Viva:** A documentação do projeto (`roadmap.md`, `blazor_functions_guide.md`, `functions.md`, `CHANGELOG.md`, `architecture.md`) é um artefato vivo e **deve ser obrigatoriamente atualizada a cada nova funcionalidade implementada, alteração de contrato ou refatoração**.
*   **Critério de Aceite:** Nenhuma funcionalidade ou fase deve ser considerada concluída sem que os guias de implementação, o mapa de funções, o roadmap e o changelog estejam totalmente sincronizados com o código e os testes.

## Regras de Estilo e Código Limpo
*   Use as funcionalidades mais recentes e idiomáticas do C# e .NET 10.
*   Mantenha as classes pequenas e focadas no Princípio da Responsabilidade Única (SRP).
*   Nomeie variáveis, métodos e classes de forma descritiva e em inglês (para o código), mantendo a nomenclatura do domínio de negócio clara.
*   Sempre documente métodos complexos de negócio e retorne exceções customizadas do domínio para violações de regras.