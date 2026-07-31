# Arquitetura do Sistema - Baba Play

## 1. Visão Geral
O **Baba Play** é uma plataforma SaaS (Software as a Service) multitenant para gestão integral de associações esportivas, ligas e times de futebol. 

A arquitetura escolhida é o **Monólito Modular (Modular Monolith)** com separação estrita entre Front-end e Back-end. Essa abordagem evita a complexidade prematura de microsserviços, garantindo forte coesão, limites bem definidos de domínio e facilidade de manutenção e deploy.

## 2. Stack Tecnológico Base
*   **Back-end:** ASP.NET Core Web API (.NET 10)
*   **Front-end:** Blazor WebAssembly / Auto (Desacoplado)
*   **ORM:** Entity Framework Core 10
*   **Autenticação:** ASP.NET Core Identity + JWT (JSON Web Tokens)
*   **Real-time:** SignalR
*   **Background Jobs:** Hangfire ou Quartz.NET (a definir)

---

## 3. Topologia de Aplicação

A solução física está dividida em duas grandes camadas que se comunicam exclusivamente via rede:

1.  **Frontend (Client - Blazor):** Responsável apenas pela apresentação (UI) e gerenciamento de estado local. Não possui referências diretas ao banco de dados ou regras de negócio centrais. Comunica-se com o backend via HTTP/REST utilizando DTOs.
2.  **Backend (Server - API):** Centraliza as regras de negócio, persistência de dados, segurança e processamento em segundo plano. 

---

## 4. Arquitetura do Back-end (Monólito Modular)

O Back-end é dividido em Bounded Contexts (Módulos de Domínio) lógicos. Cada módulo encapsula suas próprias entidades, regras e casos de uso, minimizando o acoplamento.

### 4.1. Módulos de Domínio (Bounded Contexts)
*   **`BabaPlay.Identity`:** Gestão de Tenants (Inquilinos), usuários, autenticação, autorização (RBAC) e perfis (Admin, Atleta, Técnico).
*   **`BabaPlay.Sports`:** Motor principal. Gerencia times, estatísticas, convocações (RSVP), escalações, prancheta tática e algoritmo de sorteio de equipes.
*   **`BabaPlay.Financial`:** Módulo de cobranças, gestão de mensalidades, integração com gateways de pagamento (Pix, Cartão), controle de inadimplência e prestação de contas.
*   **`BabaPlay.Communication`:** Envio de notificações push, e-mails, murais de avisos e chat (SignalR).

### 4.2. Padrões de Projeto e Fluxo de Dados
A API implementa a arquitetura **CQRS (Command Query Responsibility Segregation)** combinada com o padrão **Repository / Unit of Work**.

*   **Controllers / Minimal APIs:** Atuam apenas como portas de entrada (Entrypoints). Recebem as requisições HTTP e despacham objetos para os Handlers via `MediatR`.
*   **Commands (Escrita):** Operações que alteram estado (Create, Update, Delete). Validam regras de negócio, utilizam Repositórios para persistir os dados no EF Core e acionam o Unit of Work para *commit*.
*   **Queries (Leitura):** Operações que apenas retornam dados. Podem contornar o tracking do EF Core (`AsNoTracking`) ou utilizar micro-ORMs (como Dapper) para otimização de performance em relatórios complexos.

---

## 5. Estratégia de Multitenancy (Isolamento de Dados)

O isolamento de dados entre diferentes times e associações é o pilar de segurança do SaaS.

*   **Abordagem:** *Shared Database, Shared Schema* (Banco e esquemas compartilhados).
*   **Identificação:** Cada registro no banco de dados possuirá uma coluna `TenantId`.
*   **Implementação Segura:** Utilização de **Global Query Filters** no Entity Framework Core (`modelBuilder.Entity<Entidade>().HasQueryFilter(e => e.TenantId == _currentTenantId)`). O ID do Tenant atual é resolvido a cada requisição HTTP através do Token JWT do usuário ou do cabeçalho da requisição, sendo injetado via Dependency Injection no contexto de banco de dados.

### 5.1. RBAC e sincronização de catálogo
*   Permissões e roles default vivem em `RbacCatalog` (Application). Na **criação** do tenant, `EnsureOwnerAdminAccessAsync` provisiona o catálogo e atribui a role Admin ao owner.
*   Tenants já existentes recebem novas permissões (ex.: `financial.read`, `financial.write`, `financial.approve`) via `ITenantRbacCatalogSyncService.SyncAllTenantsAsync()` executado na subida da API, após migrations.
*   Autorização de API combina membership (`TenantMember`) com permissões tenant-scoped (`PermissionAuthorizationHandler`). Ausência de permissão → HTTP 403; JWT inválido/ausente → HTTP 401.

### 5.2. Módulo Financial (API + Blazor)
*   Controller: `FinancialController` (`/api/v1/financial/*`) com policy `FinancialRead` para leituras (overview, invoices, statement, defaulters, fundraisers).
*   Blazor: páginas em `Pages/Financial/*` protegidas com `@attribute [Authorize]`; `FinancialApiService.GetFinancialOverviewAsync` expõe mensagens distintas para 401 (sessão) e 403 (permissão).

---

## 6. Comunicação e Assincronicidade

*   **Eventos de Domínio (Domain Events):** A comunicação entre os módulos do monólito ocorre através de eventos em memória (ex: `MediatR` notifications). 
    *   *Exemplo:* O `BabaPlay.Sports` emite um evento `MatchScheduledEvent`. O `BabaPlay.Communication` escuta esse evento e dispara notificações push para os atletas.
*   **Tempo Real (Real-time):** O SignalR será utilizado para funcionalidades que exigem resposta imediata, como chats do time e atualização em tempo real de estatísticas durante uma partida.
*   **Background Tasks:** Tarefas agendadas, como geração de boletos mensais no dia 1º ou lembretes de cobrança automática, serão orquestradas via processos em segundo plano dissociados do fluxo principal de requisição HTTP.

---

## 7. Metodologia de Desenvolvimento (TDD)
O desenvolvimento das regras de negócio centrais seguirá a abordagem de **Test-Driven Development (TDD)**. 

*   O foco inicial de testes (Unitários) recairá sobre o domínio puro e os *Command Handlers*.
*   Regras complexas (ex: Algoritmo de sorteio de times balanceados ou cálculo de estatísticas) devem ser validadas extensivamente por testes antes da integração com o banco de dados.

---
*Documento mantido pela equipe de engenharia do Baba Play. Atualizado em: 29 de Julho de 2026.*