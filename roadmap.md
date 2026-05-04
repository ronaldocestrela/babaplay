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

## 🏆 Fase 9 — Partidas ✅ CONCLUÍDA

### Entregas

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
  - Migration tenant: `AddMatches`
- API:
  - `MatchController` com endpoints:
    - `POST /api/v1/match`
    - `GET /api/v1/match`
    - `GET /api/v1/match/{id}`
    - `PUT /api/v1/match/{id}`
    - `PUT /api/v1/match/{id}/status`
    - `DELETE /api/v1/match/{id}`
  - Hardening RBAC:
    - `MatchController` protegido com `TenantMember` + policies por endpoint (`MatchesRead`/`MatchesWrite`)
    - catálogo de permissões atualizado com `matches.read` e `matches.write`
    - matriz padrão de roles atualizada para incluir permissões de Match

### Regras já implementadas

- Partida exige dois times válidos e distintos (`TEAMS_MUST_BE_DIFFERENT`)
- `GameDay` deve existir (`GAMEDAY_NOT_FOUND`)
- Times devem existir/estar ativos (`TEAM_NOT_FOUND`)
- Duplicidade no mesmo game day é bloqueada (`MATCH_ALREADY_EXISTS`) incluindo ordem invertida dos times
- Partida não pode ser criada/atualizada em game day passado (`GAMEDAY_PAST`)
- Fluxo de status permitido:
  - `Pending` → `Scheduled`/`Cancelled`
  - `Scheduled` → `InProgress`/`Cancelled`
  - `InProgress` → `Completed`
  - `Completed` e `Cancelled` são finais

### Testes

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
  - cenário adicional: `POST /api/v1/match` com times iguais retorna `422 TEAMS_MUST_BE_DIFFERENT`
- Incremento TDD adicional:
  - validações de `Guid.Empty` para `GameDayId`, `HomeTeamId`, `AwayTeamId` nos handlers de create/update
  - validação de times iguais no update

### Status atual da implementação da Fase 9

- Recorte de regressão executado com filtro de Match (após hardening): **73 testes, 100% passando**
- Regressão backend completa mais recente: **315 testes, 100% passando**

---

## 📊 Fase 10 — Eventos

### Status

- ✅ Concluída (backend MVP)

### Entregas

- MatchEvents:
  - Gol
  - Amarelo
  - Vermelho

### Entregas iniciadas (slice 1)

- Domain:
  - `MatchEvent` entity (`Create`, `Update`, `Deactivate`) com validações de IDs e minuto (0-130)
  - `MatchEventType` entity configurável por tenant com `Code`, `Name`, `Points`, `IsSystemDefault`, `IsActive`
- Application (CQRS + contratos):
  - `IMatchEventRepository`, `IMatchEventTypeRepository`, `IMatchEventRealtimeNotifier`
  - DTOs `MatchEventResponse` e `MatchEventTypeResponse`
  - Commands/Handlers (iniciados):
    - MatchEventType: `Create`, `Update`, `Delete`
    - MatchEvent: `Create`, `Update`, `Delete`
  - Queries/Handlers (iniciados):
    - MatchEvent: `GetById`, `GetByMatch`, `GetByPlayer`
    - MatchEventType: `GetById`, `GetAll`
- Infrastructure:
  - `MatchEventRepository` e `MatchEventTypeRepository`
  - `TenantDbContext` com `DbSet<MatchEvent>` e `DbSet<MatchEventType>` + índices/mapeamentos
  - SignalR: `MatchHub` + `SignalRMatchEventRealtimeNotifier`
- API:
  - `MatchEventController` (CRUD + listagens por partida/jogador)
  - `MatchEventTypeController` (CRUD do catálogo de tipos com pontuação)
  - `Program.cs` com `MapHub<MatchHub>("/hubs/match")`
- Testes TDD (RED→GREEN do slice):
  - `MatchEventTests`
  - `MatchEventTypeTests`
  - `CreateMatchEventCommandHandlerTests`
  - `CreateMatchEventTypeCommandHandlerTests`

### Entregas iniciadas (slice 2)

- Banco (tenant):
  - Migration `AddMatchEventsAndTypes` criada em `Persistence/Migrations/Tenant/`
  - `TenantDbContextModelSnapshot` atualizado com `MatchEvents` e `MatchEventTypes`
- Segurança (RBAC/policies):
  - Novas permissões no catálogo: `matchevents.read`, `matchevents.write`, `matcheventtypes.read`, `matcheventtypes.write`
  - Novas policies: `MatchEventsRead`, `MatchEventsWrite`, `MatchEventTypesRead`, `MatchEventTypesWrite`
  - `MatchEventController` e `MatchEventTypeController` com autorização por policy (read/write)
- Provisioning tenant:
  - Seed idempotente de tipos padrão: `goal` (+2), `yellow_card` (-1), `red_card` (-3)
- Consistência de regra:
  - Reuso de código de tipo desativado bloqueado em validação de unicidade
- Testes TDD adicionais (unit):
  - MatchEvent handlers: `Update`, `Delete`, `GetById`, `GetByMatch`, `GetByPlayer`
  - MatchEventType handlers: `Update`, `Delete`, `GetById`, `GetAll`

### Status de testes após slices 1 e 2

- Filtro MatchEvents: **25 testes, 100% passando**
- Regressão backend completa: **307 testes, 100% passando**

---

## 📄 Fase 11 — Súmula

### Status

- ✅ Concluída (backend MVP)

### Entregas

- Geração de PDF
- Armazenamento

### Entregas iniciadas (slice 1)

- Domain:
  - `MatchSummary` entity (`Create`, `Deactivate`) com `TenantId`, `MatchId`, `StoragePath`, `FileName`, `ContentType`, `SizeBytes`, `GeneratedAtUtc`, `IsActive`
- Application (CQRS + contratos):
  - Interface `IMatchSummaryRepository`
  - Interfaces de serviço `IMatchSummaryPdfGenerator` e `IMatchSummaryStorageService`
  - DTOs: `MatchSummaryResponse` e `MatchSummaryFileResponse`
  - Command/Handler: `GenerateMatchSummary`
  - Queries/Handlers: `GetMatchSummaryByMatch` e `GetMatchSummaryFile`
- Infrastructure:
  - `MatchSummaryRepository`
  - `LocalMatchSummaryStorageService` (filesystem local)
  - `MinimalPdfMatchSummaryGenerator` (PDF mínimo válido para MVP)
  - `TenantDbContext` com `DbSet<MatchSummary>` e índice único `(TenantId, MatchId)`
- API:
  - `MatchSummaryController`
  - `POST /api/v1/match-summary`
  - `GET /api/v1/match-summary/{summaryId}`
  - `GET /api/v1/match-summary/match/{matchId}`
  - `GET /api/v1/match-summary/{summaryId}/file`
  - `DELETE /api/v1/match-summary/{summaryId}`

### Regras já aplicadas

- Geração permitida apenas para partida concluída (`MATCH_NOT_COMPLETED`)
- Duplicidade bloqueada por partida (`MATCH_SUMMARY_ALREADY_EXISTS`)
- Partida inexistente retorna `MATCH_NOT_FOUND`
- Download retorna 404 quando metadata/arquivo não existir (`MATCH_SUMMARY_NOT_FOUND` / `MATCH_SUMMARY_FILE_NOT_FOUND`)

### Testes (slice 1)

- Unit Domain:
  - `MatchSummaryTests`
- Unit Application:
  - `GenerateMatchSummaryCommandHandlerTests`
  - `GetMatchSummaryByMatchQueryHandlerTests`
- Integration:
  - `MatchSummaryIntegrationTests` (geração, consulta, duplicidade, download)

### Status atual dos testes

- Filtro `FullyQualifiedName~MatchSummary`: **15 testes, 100% passando**
- Regressão backend completa: **330 testes, 100% passando**

### Entregas iniciadas (slice 2)

- Banco (tenant):
  - Migration `AddMatchSummaries` criada em `Persistence/Migrations/Tenant/`
  - `TenantDbContextModelSnapshot` atualizado com `MatchSummaries`
- Testes TDD adicionais:
  - Unit Application: `GetMatchSummaryFileQueryHandlerTests`
  - Unit Infrastructure: `LocalMatchSummaryStorageServiceTests`, `MinimalPdfMatchSummaryGeneratorTests`
  - Integration: cenários negativos adicionados em `MatchSummaryIntegrationTests`
    - `POST /match-summary` com `matchId` inexistente
    - `POST /match-summary` com partida não concluída
    - `GET /match-summary/{summaryId}` inexistente
    - `GET /match-summary/match/{matchId}` inexistente
    - `GET /match-summary/{summaryId}/file` inexistente
    - `DELETE /match-summary/{summaryId}` inexistente
  - Novos endpoints de operação:
    - `GET /match-summary/{summaryId}` para metadata por id
    - `DELETE /match-summary/{summaryId}` para desativação da súmula e remoção do arquivo no storage
- Hardening operacional:
  - storage local configurável por `MatchSummaryStorage:RootPath`
  - proteção contra path traversal no read do storage local
  - isolamento/cleanup de storage em integração via `PlayerWebApplicationFactory`

### Status de testes após slice 2

- Filtro `FullyQualifiedName~MatchSummary`: **39 testes, 100% passando**
- Regressão backend completa: **354 testes, 100% passando**

### Entregas iniciadas (slice 3)

- Hardening de persistência na geração de súmula:
  - `GenerateMatchSummaryCommandHandler` agora faz cleanup do arquivo no storage quando falha ao persistir metadata no banco
  - novo erro de negócio: `MATCH_SUMMARY_PERSISTENCE_FAILED`
- Testes TDD adicionais:
  - Unit Application: `GenerateMatchSummaryCommandHandlerTests`
    - cenário novo: falha de persistência aciona `DeleteAsync` no storage para evitar arquivo órfão

### Status de testes após slice 3

- Filtro `FullyQualifiedName~MatchSummary`: **40 testes, 100% passando**

### Fechamento oficial da fase

- Escopo concluído nesta fase: backend MVP de súmula (geração, armazenamento local, consulta de metadata, download de arquivo, migração tenant, hardening e testes).
- Itens futuros não bloqueantes para esta fase:
  - melhoria visual/estrutural do PDF (template oficial)
  - versionamento e trilha de auditoria
  - interfaces Web/Mobile para operação da súmula

---

## 🧠 Fase 12 — Score e Ranking

### Status

- 🚧 Planejada (backend primeiro, frontend de consumo na Fase 16.7)

### Objetivo da fase

- Consolidar pontuação por jogador por tenant com rastreabilidade por origem de evento
- Expor consultas de ranking geral, artilharia e presença com filtros por período
- Garantir consistência do cálculo em operações de create/update/delete (eventos, check-ins e status de partidas)

### Regras de pontuação (fonte única)

- Presença: +1
- Vitória: +3
- Empate: +1
- Gol: +2
- Amarelo: -1
- Vermelho: -3

### Fórmula oficial

- `ScoreTotal = (Presencas*1) + (Vitorias*3) + (Empates*1) + (Gols*2) - (Amarelos*1) - (Vermelhos*3)`

### Escopo funcional

- Ranking geral por tenant com paginação e ordenação por `ScoreTotal` (desempate: gols, presenças, nome)
- Artilharia por tenant com total de gols e posição no período
- Presença por tenant com total de check-ins válidos e taxa de presença por período
- Filtro temporal em todas as consultas (`fromUtc`, `toUtc`) e filtro opcional por `gameDayId`

### Fora de escopo desta fase

- Cache distribuído de ranking (Redis) para leitura massiva
- Premiações/gamificação
- Ranking cross-tenant

### Arquitetura e modelagem (Clean + CQRS)

- Domain:
  - Novo agregado `PlayerScore` (snapshot materializado por jogador/tenant)
  - Value Objects: `ScoreBreakdown` e `RankingPeriod`
  - Regras de domínio para incremento/decremento idempotente por tipo de evento
- Application (somente CQRS):
  - Commands (write):
    - `RebuildTenantRankingCommand` (reprocessamento completo por tenant/período)
    - `ApplyScoreDeltaCommand` (atualização incremental por evento de origem)
  - Queries (read):
    - `GetRankingQuery`
    - `GetTopScorersQuery`
    - `GetAttendanceRankingQuery`
  - DTOs:
    - `RankingEntryResponse`
    - `TopScorerEntryResponse`
    - `AttendanceEntryResponse`
  - Interfaces:
    - `IPlayerScoreRepository`
    - `IScoreComputationService`
- Infrastructure:
  - `PlayerScoreRepository` (tenant DB)
  - `ScoreComputationService` com regra central de cálculo
  - Migração tenant: tabela `PlayerScores` + índices:
    - `(TenantId, PlayerId)` único
    - `(TenantId, ScoreTotal desc)` para ranking
    - `(TenantId, Goals desc)` para artilharia
    - `(TenantId, AttendanceCount desc)` para presença
- API:
  - `RankingController`
    - `GET /api/v1/ranking`
    - `GET /api/v1/ranking/top-scorers`
    - `GET /api/v1/ranking/attendance`
    - `POST /api/v1/ranking/rebuild` (operação administrativa)

### Integrações com fases anteriores

- Check-in (Fase 7): cria/remove presença no score
- MatchEvents (Fase 10): gols/cartões atualizam score
- Match (Fase 9): resultado final de partida aplica vitória/empate
- RBAC (Fase 4): novas permissões `ranking.read` e `ranking.write`

### Políticas e segurança

- `TenantMember` obrigatório em todos os endpoints
- Policies:
  - `RankingRead` para GETs
  - `RankingWrite` para rebuild/reprocessamento

### Estratégia de implementação (TDD por slices)

- Slice 1 — Domínio + cálculo puro:
  - Red: testes de `ScoreComputationService` e `PlayerScore` cobrindo todas as regras
  - Green: implementação mínima da fórmula e validações
  - Refactor: eliminar duplicidade e centralizar constantes de pontuação
- Slice 2 — Persistência + CQRS de leitura:
  - Red: testes de handlers de queries com filtros/período/paginação
  - Green: repositório + handlers de leitura
  - Refactor: otimizar projeções e ordenações
- Slice 3 — Atualização incremental:
  - Red: testes de `ApplyScoreDeltaCommandHandler` (create/update/delete de origem)
  - Green: aplicação de deltas idempotente
  - Refactor: proteção contra dupla aplicação por `SourceEventId`
- Slice 4 — Rebuild administrativo + API:
  - Red: integração HTTP + autorização + multi-tenant isolation
  - Green: endpoint de rebuild e wiring completo
  - Refactor: hardening de erros e observabilidade

### Entregas concluídas (implementação atual)

- Slice 1 — Domínio + cálculo puro ✅
  - Domain:
    - `PlayerScore` entity com `Create`, `ApplyDelta`, `ReplaceBreakdown`, `Deactivate`
    - value objects `ScoreBreakdown` e `RankingPeriod`
  - Application:
    - `IScoreComputationService` + `ScoreComputationService`
  - Testes TDD:
    - `PlayerScoreTests`, `ScoreBreakdownTests`, `RankingPeriodTests`, `ScoreComputationServiceTests`
  - Status: 13 testes passando (filtro das suítes da slice)

- Slice 2 — Persistência + CQRS de leitura ✅
  - Application (Queries):
    - `GetRankingQuery` / `GetRankingQueryHandler`
    - `GetTopScorersQuery` / `GetTopScorersQueryHandler`
    - `GetAttendanceRankingQuery` / `GetAttendanceRankingQueryHandler`
    - DTOs: `RankingEntryResponse`, `TopScorerEntryResponse`, `AttendanceEntryResponse`
    - contrato: `IPlayerScoreRepository`
  - Infrastructure:
    - `PlayerScoreRepository`
    - `TenantDbContext` com `DbSet<PlayerScore>` + mapeamento e índices de ranking
    - Migration tenant: `AddPlayerScores`
  - Regras aplicadas na leitura:
    - validação de período (`INVALID_PERIOD`)
    - paginação (`Page`, `PageSize`) com rank calculado por offset
    - ordenações específicas por tipo de ranking
  - Testes TDD:
    - `GetRankingQueryHandlerTests`
    - `GetTopScorersQueryHandlerTests`
    - `GetAttendanceRankingQueryHandlerTests`
  - Status: 7 testes da slice passando; regressão combinada slices 1+2 com 20 testes passando

- Slice 3 — Atualização incremental ✅
  - Application (Command):
    - `ApplyScoreDeltaCommand` / `ApplyScoreDeltaCommandHandler`
  - Regras aplicadas:
    - idempotência por `SourceEventId` com bloqueio de duplicidade (`DUPLICATE_SCORE_EVENT`)
    - criação automática de `PlayerScore` para primeiro delta positivo
    - delta negativo sem score prévio retorna `PLAYER_SCORE_NOT_FOUND`
    - delta inválido que leva contador abaixo de zero retorna `INVALID_SCORE_DELTA`
  - Infrastructure:
    - `PlayerScoreSourceEvent` para trilha de eventos processados
    - `PlayerScoreRepository` expandido com:
      - `HasProcessedSourceEventAsync`
      - `AddProcessedSourceEventAsync`
    - `TenantDbContext` com `DbSet<PlayerScoreSourceEvent>` + índice único `(TenantId, SourceEventId)`
    - Migration tenant: `AddPlayerScoreSourceEvents`
  - Testes TDD:
    - `ApplyScoreDeltaCommandHandlerTests`
  - Status: 5 testes da slice passando; regressão combinada slices 1+2+3 com 25 testes passando

### Cenários obrigatórios de teste

- Unit Domain:
  - fórmula completa de score com combinações mistas
  - limites e idempotência de atualização incremental
- Unit Application:
  - handlers de query (ordenação, paginação, período)
  - handlers de command (delta + rebuild)
- Integration API:
  - `GET /ranking`, `GET /ranking/top-scorers`, `GET /ranking/attendance`
  - `POST /ranking/rebuild` com sucesso, sem permissão e tenant inválido
  - isolamento entre tenants (dados nunca cruzam)
- Meta da fase:
  - Cobertura mínima >= 80%
  - Regressão backend 100% passando

### Códigos de erro planejados

- `INVALID_PERIOD`
- `PLAYER_SCORE_NOT_FOUND`
- `RANKING_REBUILD_FAILED`
- `DUPLICATE_SCORE_EVENT`

### Documentação obrigatória

- Swagger atualizado com contratos de ranking
- Tabela de regras de pontuação em docs de API
- Changelog com escopo da fase e impactos em integrações (Check-in, Match, MatchEvents)

### Critérios de aceite (Definition of Done)

- Queries de ranking, artilharia e presença publicadas e protegidas por RBAC
- Cálculo validado por TDD com rastreabilidade de origem
- Rebuild administrativo funcional para recomputar inconsistências
- Migração tenant aplicada e validada em testes de integração
- Documentação (Swagger + roadmap + changelog) atualizada

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
