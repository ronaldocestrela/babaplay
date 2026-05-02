# 🚀 Roadmap de Implementação — BabaPlay SaaS Associações Esportivas

## 🎯 Objetivo

Construir um sistema SaaS escalável, com:

- Multi-tenancy (1 DB por associação)
- TDD obrigatório
- CQRS obrigatório
- ASP.NET Identity
- Tempo real (SignalR)
- Notificações (Firebase)

---

## 🧱 Fase 0 — Fundação ✅ CONCLUÍDA

### Entregas

- Estrutura:
  - Api
  - Application
  - Domain
  - Infrastructure
  - Tests

- Configuração:
  - Clean Architecture
  - CQRS
  - xUnit + FluentAssertions
  - Swagger

### Testes (7 total — 100% passando)

- Unitários: `PingCommandHandler`, `PingQueryHandler`, `EntityBase`
- Integração: `PingIntegrationTests`

---

## 🔐 Fase 1 — Identity + Auth ✅ CONCLUÍDA

### Entregas

- ASP.NET Identity (`ApplicationUser : IdentityUser` + `IsActive`)
- JWT com HmacSha256 (claims: sub, email, jti, iat, role); ClockSkew = Zero
- Refresh Token persistido no DB, rotacionado a cada uso, revogável
- Master DB: Tenants, UserTenants, Plans, Subscriptions (SaaS full)
- EF Core migration: `InitialMaster`
- `POST /api/v1/auth/login` — retorna `AuthResponse` (accessToken + refreshToken)
- `POST /api/v1/auth/refresh-token` — rotação de token
- Swagger com Bearer JWT security definition

### Testes (27 total — 100% passando)

- 10 testes unitários (LoginCommandHandler × 5, RefreshTokenCommandHandler × 5)
- 7 testes de integração (login válido, inválido, email desconhecido, refresh válido, expirado, inválido, token revogado)

---

## 🏢 Fase 2 — Multi-Tenancy ✅ CONCLUÍDA

### Entregas

- `ProvisioningStatus` enum (Pending/InProgress/Ready/Failed) — Domain
- `ITenantRepository`, `IUserTenantRepository`, `ITenantContext`, `ITenantProvisioningQueue` — Application interfaces
- `CreateTenantCommand` + `CreateTenantCommandHandler` (valida slug único, enfileira provisioning)
- `GetTenantStatusQuery` + `GetTenantStatusQueryHandler`
- `TenantRepository`, `UserTenantRepository` — Infrastructure, acesso ao MasterDb
- `RequestTenantContext` — contexto escoped por request (sem HTTP direto)
- `TenantProvisioningQueue` — `System.Threading.Channels` singleton
- `TenantProvisioningWorker` — `BackgroundService`; cria DB isolado + roda migrations do TenantDbContext
- `TenantMiddleware` — resolve `X-Tenant-Slug` header e popula `ITenantContext`
- `TenantController` — `POST /api/v1/tenant` (201 / 409 / 422) · `GET /api/v1/tenant/{id}/status` (200 / 404)
- EF Core migration: `AddTenantProvisioningStatus`
- Swagger: `TenantSlugHeaderOperationFilter` documenta `X-Tenant-Slug` em todas as operações
- Frontend: `tenantService.ts` com `parseTenantSlug` + `getTenantFromUrl` (subdomain-first, fallback `?tenant=`)
- Frontend: `router.tsx` resolve tenant no `beforeLoad` da rota raiz
- Frontend: `TenantResponse` / `CreateTenantRequest` adicionados aos tipos
- Frontend: MSW handlers para `POST /api/v1/tenant` e `GET /api/v1/tenant/:id/status`

### Testes (49 total — 100% passando)

- 5 unit: `CreateTenantCommandHandlerTests`
- 3 unit: `GetTenantStatusQueryHandlerTests`
- 7 integration: `TenantIntegrationTests` (via `TenantWebApplicationFactory` + `TestAuthHandler`)
- 14 frontend unit: `tenantService.test.ts`

---

## 👤 Fase 3 — Players ✅ CONCLUÍDA

### Entregas

- `Player` entity (Domain): `Create()`, `Update()`, `Deactivate()` — soft delete via `IsActive`
- `IPlayerRepository` — interface de Application (GetById, GetAllActive, ExistsByUserId, Add, Update, SaveChanges)
- `PlayerResponse` DTO
- `CreatePlayerCommand` + `CreatePlayerCommandHandler` — valida nome, verifica UserId no Master DB, impede duplicidade por tenant
- `GetPlayerQuery` + `GetPlayerQueryHandler` — busca por Id
- `GetPlayersQuery` + `GetPlayersQueryHandler` — lista todos os ativos
- `UpdatePlayerCommand` + `UpdatePlayerCommandHandler` — atualiza nome/apelido/telefone/nascimento
- `DeletePlayerCommand` + `DeletePlayerCommandHandler` — soft delete (idempotente)
- `PlayerRepository` — implementação Infrastructure; usa `TenantDbContextFactory` + `ITenantContext` por operação
- `TenantDbContext` atualizado: `DbSet<Player>` + índice único em `UserId` + `OnModelCreating` com max lengths
- `TenantDbContextDesignTimeFactory` — factory para EF Migrations
- EF Core migration: `AddPlayers` (tabela `Players` no banco por-tenant)
- `TenantDbContextFactory` — `sealed` → `class`, `CreateAsync` → `virtual` (permite override em testes)
- `PlayerController` — `[Authorize]` · POST 201 · GET 200 (lista) · GET {id} 200 · PUT {id} 200 · DELETE {id} 204
- Códigos de erro: `INVALID_NAME` 422 · `USER_NOT_FOUND` 404 · `PLAYER_ALREADY_EXISTS` 409 · `PLAYER_NOT_FOUND` 404
- `PlayerWebApplicationFactory` — SQLite in-memory para Master + Tenant; `TestTenantDbContextFactory` override

### Testes (32 novos — 100% passando · total acumulado: 81)

- 10 unit: `PlayerTests` (domínio — Create, Update, Deactivate, trim, validações)
- 5 unit: `CreatePlayerCommandHandlerTests`
- 3 unit: `GetPlayerQueryHandlerTests`
- 3 unit: `GetPlayersQueryHandlerTests`
- 4 unit: `UpdatePlayerCommandHandlerTests`
- 3 unit: `DeletePlayerCommandHandlerTests`
- 10 integration: `PlayerIntegrationTests` (POST válido, POST duplicado, POST usuário desconhecido, POST nome vazio, GET lista, GET por id, GET id desconhecido, PUT, PUT id desconhecido, DELETE, DELETE id desconhecido)

---

## 🔐 Fase 4 — RBAC

### Entregas

- Roles
- Permissions
- Middleware de autorização

### Testes

- Permissão válida
- Permissão negada

---

## ⚽ Fase 5 — Posições

### Entregas

- Positions
- PlayerPositions

### Testes

- Limite de 3 posições

---

## 📅 Fase 6 — Dias de Jogo

### Entregas

- GameDays

---

## 📍 Fase 7 — Check-in

### Entregas

- Check-in com geolocalização
- Validação de raio
- SignalR

### Testes

- Dentro do raio
- Fora do raio
- Duplicidade

---

## 🧩 Fase 8 — Times

### Entregas

- Teams
- TeamPlayers

### Regras

- Limite de jogadores
- Goleiro obrigatório

---

## 🏆 Fase 9 — Partidas

### Entregas

- Matches
- Status

---

## 📊 Fase 10 — Eventos

### Entregas

- MatchEvents:
  - Gol
  - Amarelo
  - Vermelho

---

## 📄 Fase 11 — Súmula

### Entregas

- Geração de PDF
- Armazenamento

---

## 🧠 Fase 12 — Score e Ranking

### Regras

- Presença: +1
- Vitória: +3
- Empate: +1
- Gol: +2
- Amarelo: -1
- Vermelho: -3

### Entregas

- Ranking geral
- Artilharia
- Presença

---

## 🔔 Fase 13 — Notificações

### Entregas

- Firebase
- Push por evento

---

## 💰 Fase 14 — Financeiro

### Entregas

- Entradas e saídas
- Mensalidades

### Relatórios

- Caixa
- Inadimplência

---

## 📊 Fase 15 — Relatórios

### Entregas

- Presença
- Ranking
- Financeiro

---

## 🌐 Fase 16 — Frontend (React)

### 1. Auth ✅ CONCLUÍDA

#### Stack
- React 19 + Vite 8 + TypeScript 6
- TailwindCSS 4
- Zustand + persist (auth state)
- TanStack React Query v5
- TanStack Router (guards via `beforeLoad`)
- Axios + interceptor queue (silent refresh)
- Zod (validação de formulários)
- MSW 2 + Vitest + Testing Library (TDD)

#### Entregas
- `authService`: `login`, `refreshToken`, `logout` (revogação server-side), `getCurrentUser`
- `authStore` (Zustand): `accessToken`, `refreshToken`, `isAuthenticated`, `currentUser`; `setTokens`, `setCurrentUser`, `clearTokens` atômico
- `useLogin`: mutation com pré-fetch de perfil após login
- `useLogout`: mutation com `onSettled` — limpa store + cache + navega independente de erro
- `useCurrentUser`: query habilitada por `isAuthenticated`, popula `currentUser` no store
- `AuthHeader`: exibe email do usuário + botão Sair
- `ErrorBoundary`: captura erros de renderização, exibe fallback + reload
- `ProtectedLayout`: integra `AuthHeader` + aciona `useCurrentUser`
- Router guards: redirecionamento automático autenticado/não-autenticado
- Contrato de erros: `ProblemDetails.title` → `getErrorCode()`

#### Testes — 52 total (100% passando)
| Suíte | Testes |
|---|---|
| authStore | 6 |
| loginSchema | 5 |
| apiClient (interceptors) | 4 |
| authService | 10 |
| useLogin | 5 |
| useLogout | 3 |
| useCurrentUser | 4 |
| LoginForm | 7 |
| AuthHeader | 4 |
| ErrorBoundary | 4 |

### 2. Dashboard — pendente
### 3. Players — pendente
### 4. Check-in — pendente
### 5. Times — pendente
### 6. Partidas — pendente
### 7. Ranking — pendente
### 8. Financeiro — pendente

---

## 📱 Fase 17 — Mobile (Flutter)

### Ordem

1. Login
2. Check-in
3. Notificações
4. Jogos
5. Ranking

---

## ⚠️ Regras obrigatórias

- TDD sempre
- CQRS sempre
- Documentação sempre atualizada
- Testes obrigatórios (mínimo 80%)
- Falha de teste bloqueia deploy

---

## 🧠 Estratégia

- Fases críticas: 1 semana
- Fases simples: 2-3 dias

### MVP

Parar na Fase 11 já gera valor real.

---

Fim do documento.
