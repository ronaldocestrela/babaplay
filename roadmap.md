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

## 🔐 Fase 4 — RBAC ✅ CONCLUÍDA

### Entregas

- Domain (RBAC por tenant):
  - `Role` entity (`Create`, `Rename`, `AddPermission`, `RemovePermission`, `Deactivate`)
  - `Permission` entity (`Create`, normalização de código)
  - `RolePermission` (N:N)
  - `UserRole` (atribuição de role para usuário no tenant)
- Infrastructure (tenant DB):
  - `TenantDbContext` com `DbSet<Role>`, `DbSet<Permission>`, `DbSet<RolePermission>`, `DbSet<UserRole>`
  - Índices: único em `(TenantId, NormalizedName)` para roles e único em `NormalizedCode` para permissions
  - EF Core migration: `AddRbac` (banco por-tenant)
- Application (CQRS + contratos):
  - Interfaces: `IRoleRepository`, `IPermissionRepository`, `IUserRoleRepository`
  - DTO: `RoleResponse`
  - Commands/Handlers:
    - `CreateRoleCommand`
    - `AssignRoleToUserCommand`
    - `AddPermissionToRoleCommand`
  - Query/Handler:
    - `GetRolesQuery`
- API + autorização:
  - `RoleController` (`POST /api/v1/role`, `GET /api/v1/role`, `POST /api/v1/role/{roleId}/users/{userId}`, `POST /api/v1/role/{roleId}/permissions`)
  - Policy `TenantMember` + `TenantMemberAuthorizationHandler` (valida usuário autenticado pertencente ao tenant resolvido)
  - Policy por permission com `PermissionRequirement` + `PermissionAuthorizationHandler`
  - Policies aplicadas por endpoint: `RbacRolesRead`, `RbacRolesWrite`, `RbacRolesAssign`, `RbacPermissionsWrite`
- Provisioning:
  - Seed idempotente de RBAC no `TenantProvisioningWorker`
  - Roles padrão: `Admin`, `Manager`, `Member`, `Viewer`
  - Matriz inicial de permissions via catálogo central `RbacCatalog`

### Testes

- Unit Domain (novos):
  - `RoleTests`
  - `PermissionTests`
  - `RolePermissionTests`
  - `UserRoleTests`
- Unit Application (novos):
  - `CreateRoleCommandHandlerTests`
  - `GetRolesQueryHandlerTests`
  - `AssignRoleToUserCommandHandlerTests`
  - `AddPermissionToRoleCommandHandlerTests`
- Integration RBAC (novos):
  - `RbacIntegrationTests`:
    - permitido (membro com permission)
    - negado por falta de permission
    - negado por isolamento de tenant
- Status atual da suíte backend: **111 testes, 100% passando**

---

## ⚽ Fase 5 — Posições ✅ CONCLUÍDA

### Entregas

- Domain:
  - `Position` entity (`Create`, `Update`, `Deactivate`) com `TenantId`, `Code`/`NormalizedCode`, `Name`, `Description`, `IsActive`
  - `PlayerPosition` (vínculo N:N Player↔Position)
  - `Player.SetPositions(...)` para atualização completa da lista de posições
- Application (CQRS + contratos):
  - Interface `IPositionRepository`
  - DTOs `PositionResponse` e `PlayerPositionsResponse`
  - Commands/Handlers:
    - `CreatePositionCommand`
    - `UpdatePositionCommand`
    - `DeletePositionCommand`
    - `UpdatePlayerPositionsCommand` (substitui lista completa de posições do jogador)
  - Queries/Handlers:
    - `GetPositionQuery`
    - `GetPositionsQuery`
- Infrastructure (tenant DB):
  - `TenantDbContext` com `DbSet<Position>` e `DbSet<PlayerPosition>`
  - Mapeamento EF para tabela de vínculo com PK composta `(PlayerId, PositionId)`
  - Índice único em posições por tenant: `(TenantId, NormalizedCode)`
  - `PositionRepository`
  - Migration tenant: `AddPositionsAndPlayerPositions`
- API:
  - `PositionController` com CRUD completo:
    - `POST /api/v1/position`
    - `GET /api/v1/position`
    - `GET /api/v1/position/{id}`
    - `PUT /api/v1/position/{id}`
    - `DELETE /api/v1/position/{id}`
  - `PlayerController`:
    - `PUT /api/v1/player/{id}/positions` (atualização completa da lista)

### Regras de negócio

- Máximo de 3 posições por jogador
- IDs de posições não podem ser vazios
- IDs de posições não podem ser duplicados
- Deleção de posição em uso é bloqueada (`409 POSITION_IN_USE`)

### Testes

- Unit Domain (novos):
  - `PositionTests`
  - `PlayerTests` (cenários de `SetPositions`)
- Unit Application (novos):
  - `CreatePositionCommandHandlerTests`
  - `GetPositionQueryHandlerTests`
  - `GetPositionsQueryHandlerTests`
  - `UpdatePositionCommandHandlerTests`
  - `DeletePositionCommandHandlerTests`
  - `UpdatePlayerPositionsCommandHandlerTests`
- Integration (novos/atualizados):
  - `PositionIntegrationTests`
  - `PlayerIntegrationTests` (cenários de `PUT /player/{id}/positions`)
- Regra validada:
  - Limite de 3 posições por jogador
  - Duplicidade e `Guid.Empty` em `positionIds` retornam 422
  - `DELETE /position/{id}` retorna 409 quando a posição está em uso

### Status atual

- Suíte backend executada após o fechamento da Fase 5: **154 testes, 100% passando**

---

## 📅 Fase 6 — Dias de Jogo ✅ CONCLUÍDA

### Entregas

- Domain:
  - `GameDay` entity (`Create`, `Update`, `ChangeStatus`, `Deactivate`) com `TenantId`, `Name`/`NormalizedName`, `ScheduledAt`, `Location`, `Description`, `MaxPlayers`, `Status`, `IsActive`
  - `GameDayStatus` enum: `Pending`, `Confirmed`, `Cancelled`, `Completed`
- Application (CQRS + contratos):
  - Interface `IGameDayRepository`
  - DTO `GameDayResponse`
  - Commands/Handlers:
    - `CreateGameDayCommand`
    - `UpdateGameDayCommand`
    - `ChangeGameDayStatusCommand`
    - `DeleteGameDayCommand`
  - Queries/Handlers:
    - `GetGameDayQuery`
    - `GetGameDaysQuery` (com filtro opcional por status)
- Infrastructure (tenant DB):
  - `TenantDbContext` com `DbSet<GameDay>`
  - Índice único por tenant: `(TenantId, NormalizedName, ScheduledAt)`
  - Índice de consulta: `(TenantId, ScheduledAt)`
  - `GameDayRepository`
  - Migration tenant: `AddGameDays`
- API:
  - `GameDayController` com endpoints:
    - `POST /api/v1/gameday`
    - `GET /api/v1/gameday`
    - `GET /api/v1/gameday/{id}`
    - `PUT /api/v1/gameday/{id}`
    - `PUT /api/v1/gameday/{id}/status`
    - `DELETE /api/v1/gameday/{id}`

### Regras de negócio

- `ScheduledAt` deve estar no futuro
- `MaxPlayers` deve ser maior que zero
- Soft delete obrigatório (`IsActive = false`)
- Fluxo de status permitido:
  - `Pending` → `Confirmed`/`Cancelled`
  - `Confirmed` → `Completed`/`Cancelled`
  - `Cancelled` e `Completed` são estados finais
- Duplicidade bloqueada por tenant para mesmo nome normalizado + data/hora (`409 GAMEDAY_ALREADY_EXISTS`)

### Testes

- Unit Domain (novos):
  - `GameDayTests`
- Unit Application (novos):
  - `CreateGameDayCommandHandlerTests`
  - `GetGameDayQueryHandlerTests`
  - `GetGameDaysQueryHandlerTests`
  - `UpdateGameDayCommandHandlerTests`
  - `ChangeGameDayStatusCommandHandlerTests`
  - `DeleteGameDayCommandHandlerTests`
- Integration (novos):
  - `GameDayIntegrationTests` (CRUD + status + validações)

### Status atual

- Suíte backend executada após o fechamento da Fase 6: **182 testes, 100% passando**

---

## 📍 Fase 7 — Check-in ✅ CONCLUÍDA (backend)

### Entregas

- Domain:
  - `Checkin` entity (`Create`, `Deactivate`) com `TenantId`, `PlayerId`, `GameDayId`, `CheckedInAtUtc`, `Latitude`, `Longitude`, `DistanceFromAssociationMeters`, `IsActive`, `CancelledAtUtc`
  - Value Object `GeoCoordinate` com validação de latitude/longitude e cálculo de distância (Haversine)
- Application (CQRS + contratos):
  - `CreateCheckinCommand` + `CreateCheckinCommandHandler`
  - `CancelCheckinCommand` + `CancelCheckinCommandHandler`
  - `GetCheckinsByGameDayQuery` + `GetCheckinsByGameDayQueryHandler`
  - `GetCheckinsByPlayerQuery` + `GetCheckinsByPlayerQueryHandler`
  - Interfaces: `ICheckinRepository`, `ITenantGeolocationSettingsRepository`, `ICheckinRealtimeNotifier`
  - DTOs: `CheckinResponse`, `TenantGeolocationSettingsDto`
- Infrastructure:
  - `CheckinRepository` (tenant DB)
  - `TenantGeolocationSettingsRepository` (master DB)
  - `CheckinHub` + `SignalRCheckinRealtimeNotifier`
  - `Tenant` (master) com `AssociationLatitude`, `AssociationLongitude`, `CheckinRadiusMeters`
  - `TenantDbContext` com `DbSet<Checkin>` e índices para consulta/duplicidade
- API:
  - `CheckinController`
  - `POST /api/v1/checkin`
  - `GET /api/v1/checkin/gameday/{gameDayId}`
  - `GET /api/v1/checkin/player/{playerId}`
  - `DELETE /api/v1/checkin/{id}`
  - `Program.cs` com `AddSignalR()` e `MapHub<CheckinHub>("/hubs/checkin")`
  - Migrations da fase:
    - tenant: `AddCheckins`
    - master: `AddTenantGeolocationSettings`

### Regras de negócio já aplicadas

- Check-in permitido apenas no dia do `GameDay`
- Jogador deve existir e estar ativo (`PLAYER_INACTIVE`)
- Validação de raio pela geolocalização da associação (`CHECKIN_OUTSIDE_ALLOWED_RADIUS`)
- Duplicidade bloqueada por jogador + game day (`CHECKIN_ALREADY_EXISTS`)
- Emissão de evento realtime para criação, contagem, tentativas negadas e undo

### Testes

- Unit Domain:
  - `CheckinTests`
- Unit Application:
  - `CreateCheckinCommandHandlerTests`
  - `CancelCheckinCommandHandlerTests`
  - `GetCheckinsByGameDayQueryHandlerTests`
  - `GetCheckinsByPlayerQueryHandlerTests`
- Integration:
  - `CheckinIntegrationTests` (HTTP)
  - `CheckinHubIntegrationTests` (SignalR)
- Regressão backend:
  - suíte completa executada com **206 testes, 100% passando**

---

## 🧩 Fase 8 — Times ✅ CONCLUÍDA (backend)

### Entregas

- Domain:
  - `Team` entity (`Create`, `Update`, `Deactivate`) com `TenantId`, `Name`/`NormalizedName`, `MaxPlayers`, `IsActive`
  - `TeamPlayer` (vínculo N:N Team↔Player)
  - `Team.SetPlayers(...)` para sincronização completa do elenco
- Application (CQRS + contratos):
  - Interface `ITeamRepository`
  - DTOs `TeamResponse` e `TeamPlayersResponse`
  - Commands/Handlers:
    - `CreateTeamCommand`
    - `UpdateTeamCommand`
    - `DeleteTeamCommand`
    - `UpdateTeamPlayersCommand`
  - Queries/Handlers:
    - `GetTeamQuery`
    - `GetTeamsQuery`
- Infrastructure (tenant DB):
  - `TeamRepository`
  - `TenantDbContext` com `DbSet<Team>` e `DbSet<TeamPlayer>`
  - Mapeamento EF da tabela de vínculo com PK composta `(TeamId, PlayerId)`
  - Índice único por tenant: `(TenantId, NormalizedName)`
  - Migration tenant: `AddTeamsAndTeamPlayers`
- API:
  - `TeamController` com endpoints:
    - `POST /api/v1/team`
    - `GET /api/v1/team`
    - `GET /api/v1/team/{id}`
    - `PUT /api/v1/team/{id}`
    - `PUT /api/v1/team/{id}/players`
    - `DELETE /api/v1/team/{id}`

### Regras

- Limite de jogadores por time (`TEAM_PLAYERS_LIMIT_EXCEEDED`)
- IDs duplicados no elenco bloqueados (`TEAM_DUPLICATE_PLAYERS`)
- IDs vazios no elenco bloqueados (`TEAM_INVALID_PLAYER_ID`)
- Jogadores inexistentes/inativos bloqueados (`TEAM_PLAYER_NOT_FOUND`)
- Goleiro obrigatório no elenco ativo (`TEAM_GOALKEEPER_REQUIRED`)

### Testes

- Unit Domain (novos):
  - `TeamTests`
  - `TeamPlayerTests`
- Unit Application (novos):
  - `CreateTeamCommandHandlerTests`
  - `GetTeamQueryHandlerTests`
  - `GetTeamsQueryHandlerTests`
  - `UpdateTeamCommandHandlerTests`
  - `DeleteTeamCommandHandlerTests`
  - `UpdateTeamPlayersCommandHandlerTests`
- Integration (novos):
  - `TeamIntegrationTests` (CRUD + validação de limite + goleiro obrigatório)

### Status atual

- Suíte backend executada após o fechamento da Fase 8: **243 testes, 100% passando**

---

## 🏆 Fase 9 — Partidas 🚧 EM IMPLEMENTAÇÃO

### Entregas concluídas (parcial)

- Domain:
  - `Match` entity (`Create`, `Update`, `ChangeStatus`, `Deactivate`) com `TenantId`, `GameDayId`, `HomeTeamId`, `AwayTeamId`, `Description`, `Status`, `IsActive`
  - `MatchStatus` enum: `Pending`, `Scheduled`, `InProgress`, `Completed`, `Cancelled`
- Application (CQRS + contratos):
  - Interface `IMatchRepository`
  - DTO `MatchResponse`
  - Commands/Handlers:
    - `CreateMatchCommand`
    - `UpdateMatchCommand`
    - `ChangeMatchStatusCommand`
    - `DeleteMatchCommand`
  - Queries/Handlers:
    - `GetMatchQuery`
    - `GetMatchesQuery`
- Infrastructure (tenant DB):
  - `MatchRepository`
  - `TenantDbContext` com `DbSet<Match>`
  - Índices de consulta por `(TenantId, Status)` e `(TenantId, GameDayId)`
  - Índice único para evitar duplicidade de confronto por game day
- API:
  - `MatchController` com endpoints:
    - `POST /api/v1/match`
    - `GET /api/v1/match`
    - `GET /api/v1/match/{id}`
    - `PUT /api/v1/match/{id}`
    - `PUT /api/v1/match/{id}/status`
    - `DELETE /api/v1/match/{id}`

### Regras já implementadas

- Partida exige dois times válidos e distintos (`TEAMS_MUST_BE_DIFFERENT`)
- `GameDay` deve existir (`GAMEDAY_NOT_FOUND`)
- Times devem existir/estar ativos (`TEAM_NOT_FOUND`)
- Duplicidade no mesmo game day é bloqueada (`MATCH_ALREADY_EXISTS`) incluindo ordem invertida dos times
- Fluxo de status permitido:
  - `Pending` → `Scheduled`/`Cancelled`
  - `Scheduled` → `InProgress`/`Cancelled`
  - `InProgress` → `Completed`
  - `Completed` e `Cancelled` são finais

### Testes (parcial)

- Unit Domain:
  - `MatchTests`
- Unit Application:
  - `CreateMatchCommandHandlerTests`
  - `UpdateMatchCommandHandlerTests`
  - `ChangeMatchStatusCommandHandlerTests`
  - `DeleteMatchCommandHandlerTests`
  - `GetMatchQueryHandlerTests`
  - `GetMatchesQueryHandlerTests`
- Integration:
  - `MatchIntegrationTests`

### Status atual da implementação da Fase 9

- Recorte de regressão executado com filtro de Match: **20 testes, 100% passando**

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
