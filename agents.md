# AGENTS.md

## Visão Geral

Sistema SaaS para gestão de associações esportivas (futebol) com:

- Multi-tenant (1 banco por associação)
- Tempo real (SignalR)
- Notificações push (Firebase)
- Backend: .NET 10 (Clean Architecture + CQRS + TDD obrigatório)
- Banco: SQL Server
- Web: React + Vite + TypeScript
- Mobile: Flutter

Escala alvo inicial:
- ~500 tenants
- ~60 usuários por tenant

---

## PRINCÍPIOS OBRIGATÓRIOS (NÃO NEGOCIÁVEL)

### 1. TDD (Test Driven Development)

- Todo código deve ser iniciado por teste
- Fluxo obrigatório: Red → Green → Refactor
- Cobertura mínima: 80%

### 2. CQRS

- Commands → escrita
- Queries → leitura
- Proibido misturar responsabilidades

### 3. Clean Architecture

- Domain não depende de nada
- Application depende apenas de Domain
- Infrastructure depende de tudo
- API apenas orquestra

### 4. Identity

- Uso obrigatório do ASP.NET Identity
- Base para autenticação e autorização

### 5. Documentação

- Swagger obrigatório
- Toda alteração exige atualização da documentação

---

## Arquitetura Geral

### Master Database

- Autenticação global (Identity)
- Tenants
- Assinaturas
- UserTenants

### Tenant Database

- Jogadores
- Check-ins
- Partidas
- Times
- Financeiro
- RBAC

---

## Backend (.NET 10)

### Stack

- .NET 10
- ASP.NET Core
- Entity Framework Core
- SQL Server
- MediatR (CQRS)
- SignalR
- Firebase Admin SDK

### Estrutura

```
src/
 ├── Api
 ├── Application
 │    ├── Commands
 │    ├── Queries
 │    ├── Handlers
 │    ├── DTOs
 ├── Domain
 ├── Infrastructure
 ├── Tests
```

---

## Multi-Tenancy

### Estratégia

- 1 banco por tenant
- Connection string dinâmica

### Componentes

- ITenantResolver
- TenantMiddleware
- TenantDbContextFactory

---

## Tempo Real (SignalR)

### Hubs

- CheckinHub
- MatchHub

---

## Notificações (Firebase)

- Firebase Cloud Messaging (FCM)
- Tokens por dispositivo

---

## Frontend Web (React + Vite)

### Stack

- React 19 + Vite 8 + TypeScript 6
- TailwindCSS 4
- TanStack React Query v5 (server state)
- Zustand + persist (estado global)
- TanStack Router (roteamento + guards)
- Axios + interceptor queue (silent refresh)
- React Hook Form + Zod (formulários/validação)
- MSW 2 + Vitest + Testing Library (testes)

### Padrões

- Feature-based (`src/features/<feature>/`)
- Hooks (mutations via React Query, estado via Zustand)
- Erros de API: `ProblemDetails.title` → `getErrorCode()`
- TDD obrigatório: Red → Green → Refactor

### Estrutura

```
web/src/
 ├── core/
 │    ├── api/          # apiClient (Axios + interceptors)
 │    ├── components/   # AuthHeader, ErrorBoundary
 │    ├── constants/    # apiRoutes
 │    ├── providers/    # AppProviders (QueryClient, Router)
 │    └── utils/        # getErrorCode
 ├── features/
 │    └── auth/
 │         ├── components/  # LoginForm
 │         ├── hooks/       # useLogin, useLogout, useCurrentUser
 │         ├── schemas/     # loginSchema (Zod)
 │         ├── services/    # authService
 │         ├── store/       # authStore (Zustand)
 │         └── types/       # AuthResponse, UserProfile, LoginRequest...
 ├── layouts/          # ProtectedLayout, PublicLayout
 ├── pages/            # LoginPage, DashboardPage
 └── test/             # MSW handlers, setup, utils
```

---

## Mobile (Flutter)

### Stack

- Flutter (última versão estável)
- Dart
- Riverpod (state management)
- Dio (HTTP client)
- GoRouter (navegação)
- Firebase Messaging (push)
- Geolocator (GPS)

### Estrutura

```
lib/
 ├── core
 ├── features
 ├── services
 ├── widgets
```

---

## Modelagem (Resumo)

### Master DB

- Users (Identity)
- Tenants
- UserTenants

### Tenant DB

- Players
- Positions
- Matches
- Teams
- MatchEvents
- Checkins
- Financial

---

## API

### Padrão

- Controllers leves
- Lógica via Handlers (CQRS)

---

## Testes

- Unitários (Domain + Application)
- Integração (API)
- Falha de teste bloqueia deploy

---

## DevOps

- Docker
- CI/CD
- Redis (cache + SignalR backplane)
- Migrations por tenant

---

## Regras de Negócio

### Score

- Presença: +1
- Vitória: +3
- Empate: +1
- Gol: +2
- Amarelo: -1
- Vermelho: -3

---

## REGRA FINAL

Código sem:
- teste
- CQRS
- documentação

→ NÃO É ACEITO

---

## Estado de Implementação

### Fase 0 — Fundação ✅

- Solução `.NET 10` com 5 projetos (`Api`, `Application`, `Domain`, `Infrastructure`, `Tests`)
- Clean Architecture com dependências corretas entre camadas
- CQRS via contratos próprios (`ICommand<T>`, `ICommandHandler`, `IQuery<T>`, `IQueryHandler`)
- `Result<T>` / `Result` para fluxo sem exceções
- Vertical slice Ping (command + query + controller)
- `GlobalExceptionHandler` com `ProblemDetails`
- Swagger com docs XML
- 7 testes (unit + integração) — ciclo TDD Red-Green-Refactor

### Fase 1 — Identity + Auth ✅

#### JWT
- Algoritmo: HmacSha256; ClockSkew: `TimeSpan.Zero`
- Claims: `sub` (userId), `email`, `jti` (Guid), `iat`, `ClaimTypes.Role` por role
- Configuração via `JwtSettings` (seção `Jwt` no appsettings)

#### Refresh Token
- Persistido no Master DB (`RefreshTokens`); rotacionado a cada uso; revogável via `RevokedAt` (nullable)
- `IsRevoked` é propriedade computada (ignorada pelo EF via `e.Ignore`)
- Gerado com 64 bytes criptográficos (Base64)

#### Master DB — tabelas
| Tabela | Observação |
|---|---|
| `AspNetUsers` | `ApplicationUser` com `IsActive` e `CreatedAt` |
| `AspNetRoles` / tabelas Identity | padrão |
| `RefreshTokens` | índice único em `Token` |
| `Tenants` | slug único; `ConnectionString` `HasMaxLength(2000)` |
| `UserTenants` | PK composta (UserId, TenantId) |
| `Plans` | `Price` decimal(18,2) |
| `Subscriptions` | FK → Tenant + Plan; enum `SubscriptionStatus` (Active/Expired/Cancelled) |

#### Endpoints
| Método | Rota | Sucesso | Erros |
|---|---|---|---|
| POST | `/api/v1/auth/login` | 200 `AuthResponse` | 401 INVALID_CREDENTIALS · 422 USER_INACTIVE |
| POST | `/api/v1/auth/refresh-token` | 200 `AuthResponse` | 401 INVALID_TOKEN · 401 TOKEN_EXPIRED |

#### Testes — 27 total (100% passando)
- 5 unitários `LoginCommandHandler`
- 5 unitários `RefreshTokenCommandHandler`
- 7 integração `AuthIntegrationTests` (SQLite in-memory via `AuthWebApplicationFactory`)

#### Migration
- `InitialMaster` gerada em `src/BabaPlay.Infrastructure/Persistence/Migrations/`

---

### Fase 3 — Players ✅

#### Domínio
- `Player` entity (sealed, extends `EntityBase`): `UserId` (Guid, FK lógica), `Name`, `Nickname`, `Phone`, `DateOfBirth`, `IsActive`
- `static Create(...)` lança `ValidationException` em nome vazio/nulo ou `UserId` vazio
- `Update(...)` + `Deactivate()` (idempotente)

#### Application
- `IPlayerRepository`: GetByIdAsync, GetAllActiveAsync, ExistsByUserIdAsync, AddAsync, UpdateAsync, SaveChangesAsync
- `PlayerResponse` sealed record DTO
- 5 handlers CQRS: Create, GetPlayer, GetPlayers, Update, Delete

#### Infrastructure
- `PlayerRepository`: injeta `TenantDbContextFactory` + `ITenantContext`; cria contexto por operação
- `TenantDbContext`: adicionado `DbSet<Player>`, índice único `UserId`, max lengths
- `TenantDbContextDesignTimeFactory` para migrations
- Migration `AddPlayers` em `Persistence/Migrations/Tenant/`
- `TenantDbContextFactory`: `sealed` → `class`, `CreateAsync` → `virtual`

#### Endpoints
| Método | Rota | Sucesso | Erros |
|---|---|---|---|
| POST | `/api/v1/player` | 201 `PlayerResponse` | 404 USER_NOT_FOUND · 409 PLAYER_ALREADY_EXISTS · 422 INVALID_NAME |
| GET | `/api/v1/player` | 200 `IReadOnlyList<PlayerResponse>` | — |
| GET | `/api/v1/player/{id}` | 200 `PlayerResponse` | 404 PLAYER_NOT_FOUND |
| PUT | `/api/v1/player/{id}` | 200 `PlayerResponse` | 404 PLAYER_NOT_FOUND · 422 INVALID_NAME |
| DELETE | `/api/v1/player/{id}` | 204 | 404 PLAYER_NOT_FOUND |

#### Testes — 81 total (100% passando, +32)
- 10 unit `PlayerTests` (domínio)
- 5 unit `CreatePlayerCommandHandlerTests`
- 3 unit `GetPlayerQueryHandlerTests`
- 3 unit `GetPlayersQueryHandlerTests`
- 4 unit `UpdatePlayerCommandHandlerTests`
- 3 unit `DeletePlayerCommandHandlerTests`
- 10 integration `PlayerIntegrationTests` (`PlayerWebApplicationFactory` + SQLite in-memory dual)

---

### Fase 5 — Positions ✅

#### Domínio
- `Position` entity (sealed, extends `EntityBase`): `TenantId`, `Code`, `NormalizedCode`, `Name`, `Description`, `IsActive`
- `PlayerPosition` entity para vínculo N:N `Player` ↔ `Position`
- `Player.SetPositions(...)` para sincronização completa da lista de posições

#### Application
- `IPositionRepository`: `GetByIdAsync`, `GetAllActiveAsync`, `ExistsByNormalizedCodeAsync`, `GetByIdsAsync`, `IsInUseAsync`, `AddAsync`, `UpdateAsync`, `SaveChangesAsync`
- DTOs: `PositionResponse`, `PlayerPositionsResponse`
- Handlers CQRS:
	- Positions: Create, GetPosition, GetPositions, Update, Delete
	- Players: `UpdatePlayerPositions`

#### Infrastructure
- `PositionRepository` com contexto por operação via `TenantDbContextFactory` + `ITenantContext`
- `TenantDbContext`: `DbSet<Position>` e `DbSet<PlayerPosition>` com PK composta `(PlayerId, PositionId)`
- Índice único por tenant: `(TenantId, NormalizedCode)`
- Migration `AddPositionsAndPlayerPositions` em `Persistence/Migrations/Tenant/`

#### Endpoints
| Método | Rota | Sucesso | Erros |
|---|---|---|---|
| POST | `/api/v1/position` | 201 `PositionResponse` | 409 POSITION_ALREADY_EXISTS · 422 INVALID_CODE/INVALID_NAME |
| GET | `/api/v1/position` | 200 `IReadOnlyList<PositionResponse>` | — |
| GET | `/api/v1/position/{id}` | 200 `PositionResponse` | 404 POSITION_NOT_FOUND |
| PUT | `/api/v1/position/{id}` | 200 `PositionResponse` | 404 POSITION_NOT_FOUND · 409 POSITION_ALREADY_EXISTS · 422 INVALID_CODE/INVALID_NAME |
| DELETE | `/api/v1/position/{id}` | 204 | 404 POSITION_NOT_FOUND · 409 POSITION_IN_USE |
| PUT | `/api/v1/player/{id}/positions` | 200 `PlayerPositionsResponse` | 404 PLAYER_NOT_FOUND/POSITION_NOT_FOUND · 422 POSITIONS_LIMIT_EXCEEDED/DUPLICATE_POSITIONS/INVALID_POSITION_ID |

#### Regras de negócio
- Máximo de 3 posições por jogador
- IDs de posição duplicados são rejeitados
- IDs de posição vazios são rejeitados
- Posição vinculada a jogador não pode ser deletada (`POSITION_IN_USE`)

#### Testes — 154 total (100% passando)
- Unit Domain: `PositionTests`, `PlayerPositionTests`, `PlayerTests` (SetPositions)
- Unit Application: suítes de Commands/Queries de Positions e `UpdatePlayerPositionsCommandHandlerTests`
- Integration: `PositionIntegrationTests` + cenários de `PUT /player/{id}/positions` em `PlayerIntegrationTests`

---

### Fase 6 — GameDays ✅

#### Domínio
- `GameDay` entity (sealed, extends `EntityBase`): `TenantId`, `Name`, `NormalizedName`, `ScheduledAt`, `Location`, `Description`, `MaxPlayers`, `Status`, `IsActive`
- `GameDayStatus` enum: `Pending`, `Confirmed`, `Cancelled`, `Completed`
- Regras na entidade:
	- `ScheduledAt` no futuro
	- `MaxPlayers` > 0
	- transições de status válidas
	- `Deactivate()` idempotente

#### Application
- `IGameDayRepository`: `GetByIdAsync`, `GetAllActiveAsync`, `ExistsByNormalizedNameAndScheduledAtAsync`, `AddAsync`, `UpdateAsync`, `SaveChangesAsync`
- DTO: `GameDayResponse`
- Handlers CQRS:
	- Commands: `CreateGameDay`, `UpdateGameDay`, `ChangeGameDayStatus`, `DeleteGameDay`
	- Queries: `GetGameDay`, `GetGameDays`

#### Infrastructure
- `GameDayRepository` com contexto por operação via `TenantDbContextFactory` + `ITenantContext`
- `TenantDbContext`: adicionado `DbSet<GameDay>`
- Índices: único `(TenantId, NormalizedName, ScheduledAt)` + consulta `(TenantId, ScheduledAt)`
- Migration tenant: `AddGameDays`

#### Endpoints
| Método | Rota | Sucesso | Erros |
|---|---|---|---|
| POST | `/api/v1/gameday` | 201 `GameDayResponse` | 409 GAMEDAY_ALREADY_EXISTS · 422 INVALID_NAME/INVALID_SCHEDULED_AT/INVALID_MAX_PLAYERS |
| GET | `/api/v1/gameday` | 200 `IReadOnlyList<GameDayResponse>` | — |
| GET | `/api/v1/gameday/{id}` | 200 `GameDayResponse` | 404 GAMEDAY_NOT_FOUND |
| PUT | `/api/v1/gameday/{id}` | 200 `GameDayResponse` | 404 GAMEDAY_NOT_FOUND · 409 GAMEDAY_ALREADY_EXISTS · 422 INVALID_* |
| PUT | `/api/v1/gameday/{id}/status` | 200 `GameDayResponse` | 404 GAMEDAY_NOT_FOUND · 422 INVALID_STATUS_TRANSITION |
| DELETE | `/api/v1/gameday/{id}` | 204 | 404 GAMEDAY_NOT_FOUND |

#### Testes — 182 total (100% passando)
- Unit Domain: `GameDayTests`
- Unit Application: 6 suítes de Commands/Queries de GameDays
- Integration: `GameDayIntegrationTests`

---

### Fase 7 — Check-ins 🚧

#### Domínio
- `Checkin` entity (sealed, extends `EntityBase`): `TenantId`, `PlayerId`, `GameDayId`, `CheckedInAtUtc`, `Latitude`, `Longitude`, `DistanceFromAssociationMeters`, `IsActive`, `CancelledAtUtc`
- `GeoCoordinate` value object para validação de coordenadas e cálculo de distância em metros

#### Application
- Command implementado: `CreateCheckin`
- Interfaces: `ICheckinRepository`, `ITenantGeolocationSettingsRepository`, `ICheckinRealtimeNotifier`
- DTOs: `CheckinResponse`, `TenantGeolocationSettingsDto`

#### Infrastructure
- `CheckinRepository` com contexto por operação via `TenantDbContextFactory` + `ITenantContext`
- `TenantGeolocationSettingsRepository` para leitura de geolocalização da associação no master DB
- `TenantDbContext`: adicionado `DbSet<Checkin>` com índices de consulta e unicidade para duplicidade ativa
- SignalR: `CheckinHub` + `SignalRCheckinRealtimeNotifier`

#### API
- Endpoint implementado:
	- `POST /api/v1/checkin`

#### Regras aplicadas
- Check-in apenas no dia do jogo (`CHECKIN_DAY_INVALID`)
- Jogador precisa existir e estar ativo (`PLAYER_NOT_FOUND` / `PLAYER_INACTIVE`)
- Distância deve estar dentro do raio da associação (`CHECKIN_OUTSIDE_ALLOWED_RADIUS`)
- Duplicidade bloqueada por jogador + game day (`CHECKIN_ALREADY_EXISTS`)

#### Realtime (MVP parcial)
- Evento de check-in criado
- Evento de contagem atualizada
- Evento de tentativa negada com motivo

#### Testes
- Unit Domain: `CheckinTests`
- Unit Application: `CreateCheckinCommandHandlerTests`
- Suíte backend atual: 192 testes (100% passando)

#### Pendências da Fase 7
- `Undo/Cancel` de check-in
- Queries de listagem por game day e por jogador
- Testes de integração HTTP e SignalR da feature
- Migrations finais da fase

---

### Fase 16 — Frontend (React): 1. Auth ✅

#### Arquitetura de autenticação
- `authStore` (Zustand persist): `accessToken`, `refreshToken`, `isAuthenticated`, `currentUser: UserProfile | null`
- `apiClient` (Axios): injeta Bearer token via request interceptor; fila de retry em 401 com silent refresh; fallback para `clearTokens` + redirect
- `authService`: `login`, `refreshToken`, `logout` (POST com revogação), `getCurrentUser` (GET /me)

#### Hooks
| Hook | Descrição |
|---|---|
| `useLogin` | mutation; pré-fetcha perfil no cache após `setTokens` |
| `useLogout` | mutation; `onSettled` garante limpeza de store + cache + redirect mesmo em erro |
| `useCurrentUser` | query; `enabled: isAuthenticated`; persiste `UserProfile` no store |

#### Componentes
| Componente | Descrição |
|---|---|
| `AuthHeader` | Logo + email do usuário + botão Sair |
| `ErrorBoundary` | Class component; captura erros de render; fallback com reload |
| `ProtectedLayout` | Monta `AuthHeader` + aciona `useCurrentUser` |
| `LoginForm` | Formulário Zod + React Hook Form + exibe códigos de erro da API |

#### Testes — 52 total (100% passando, TDD)
- 10 suítes cobrindo: store, schema, interceptors, service, hooks, componentes

---

Fim.
