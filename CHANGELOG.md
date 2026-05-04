# Changelog

Todas as mudanças relevantes deste projeto serão documentadas aqui.

Formato baseado em [Keep a Changelog](https://keepachangelog.com/pt-BR/1.0.0/).

---

## [Unreleased]

### Added — Fase 8: Times (backend concluído)

- Domain:
	- `Team` entity com `Create`, `Update`, `Deactivate` e sincronização de elenco via `SetPlayers(...)`
	- `TeamPlayer` para vínculo N:N entre `Team` e `Player`
- Application:
	- `ITeamRepository`
	- DTOs `TeamResponse` e `TeamPlayersResponse`
	- `CreateTeamCommand` / `CreateTeamCommandHandler`
	- `UpdateTeamCommand` / `UpdateTeamCommandHandler`
	- `DeleteTeamCommand` / `DeleteTeamCommandHandler`
	- `GetTeamQuery` / `GetTeamQueryHandler`
	- `GetTeamsQuery` / `GetTeamsQueryHandler`
	- `UpdateTeamPlayersCommand` / `UpdateTeamPlayersCommandHandler`
- Regras de negócio aplicadas:
	- limite de jogadores por time (`TEAM_PLAYERS_LIMIT_EXCEEDED`)
	- bloqueio de `playerIds` duplicados (`TEAM_DUPLICATE_PLAYERS`)
	- bloqueio de `playerIds` com `Guid.Empty` (`TEAM_INVALID_PLAYER_ID`)
	- bloqueio de jogadores inexistentes/inativos (`TEAM_PLAYER_NOT_FOUND`)
	- goleiro obrigatório no elenco ativo (`TEAM_GOALKEEPER_REQUIRED`)
- Infrastructure:
	- `TeamRepository`
	- `TenantDbContext` com `DbSet<Team>` e `DbSet<TeamPlayer>`
	- índice único `(TenantId, NormalizedName)` em `Teams`
	- migration tenant `AddTeamsAndTeamPlayers`
- API:
	- novo `TeamController` com CRUD completo em `/api/v1/team`
	- endpoint `PUT /api/v1/team/{id}/players` para sincronização completa do elenco
- Testes novos:
	- Unit Domain: `TeamTests`, `TeamPlayerTests`
	- Unit Application: suítes de commands/queries de Team e `UpdateTeamPlayersCommandHandlerTests`
	- Integration: `TeamIntegrationTests`
- Regressão executada: 243 testes backend, 100% passando

### Added — Fase 7: Check-in (backend concluído)

- Domain:
	- `Checkin` entity com suporte a criação e cancelamento lógico (`Deactivate`)
	- `GeoCoordinate` value object para validação de latitude/longitude e cálculo de distância em metros
- Application:
	- `CreateCheckinCommand` / `CreateCheckinCommandHandler`
	- `CancelCheckinCommand` / `CancelCheckinCommandHandler`
	- `GetCheckinsByGameDayQuery` / `GetCheckinsByGameDayQueryHandler`
	- `GetCheckinsByPlayerQuery` / `GetCheckinsByPlayerQueryHandler`
	- contratos `ICheckinRepository`, `ITenantGeolocationSettingsRepository`, `ICheckinRealtimeNotifier`
	- DTOs `CheckinResponse` e `TenantGeolocationSettingsDto`
- Regras de negócio aplicadas:
	- check-in somente no dia do `GameDay` (`CHECKIN_DAY_INVALID`)
	- jogador ativo obrigatório (`PLAYER_INACTIVE`)
	- validação de raio por geolocalização da associação (`CHECKIN_OUTSIDE_ALLOWED_RADIUS`)
	- duplicidade bloqueada por jogador + game day (`CHECKIN_ALREADY_EXISTS`)
- Infrastructure:
	- `CheckinRepository` (tenant DB)
	- `TenantGeolocationSettingsRepository` (master DB)
	- `Tenant` com campos de geolocalização (`AssociationLatitude`, `AssociationLongitude`, `CheckinRadiusMeters`)
	- `TenantDbContext` com `DbSet<Checkin>` e índices para consulta/duplicidade ativa
	- SignalR com `CheckinHub` e `SignalRCheckinRealtimeNotifier`
- API:
	- endpoint `POST /api/v1/checkin`
	- endpoint `GET /api/v1/checkin/gameday/{gameDayId}`
	- endpoint `GET /api/v1/checkin/player/{playerId}`
	- endpoint `DELETE /api/v1/checkin/{id}`
	- `Program.cs` com `AddSignalR()` + `MapHub<CheckinHub>("/hubs/checkin")`
- Realtime:
	- eventos de criação, contagem, tentativa negada e undo de check-in
- Migrations:
	- tenant: `AddCheckins`
	- master: `AddTenantGeolocationSettings`
- Testes:
	- Unit Domain: `CheckinTests`
	- Unit Application: `CreateCheckinCommandHandlerTests`, `CancelCheckinCommandHandlerTests`, `GetCheckinsByGameDayQueryHandlerTests`, `GetCheckinsByPlayerQueryHandlerTests`
	- Integration: `CheckinIntegrationTests` e `CheckinHubIntegrationTests`
	- Regressão executada: 206 testes backend, 100% passando

### Changed

- Documentação de roadmap e estado de implementação atualizados para refletir a Fase 7 backend concluída.

### Added — Fase 5: Positions

- `Position` entity (Domain): `Create()`, `Update()`, `Deactivate()` com `TenantId`, `Code`, `NormalizedCode`, `Name`, `Description`, `IsActive`
- `PlayerPosition` entity (Domain): vínculo N:N entre `Player` e `Position`
- `Player.SetPositions(...)` no domínio para sincronização completa da lista de posições
- `IPositionRepository` (Application): operações de consulta, persistência e verificação de uso (`IsInUseAsync`)
- `PositionResponse` e `PlayerPositionsResponse` DTOs
- CQRS Positions:
	- `CreatePositionCommand` / `CreatePositionCommandHandler`
	- `GetPositionQuery` / `GetPositionQueryHandler`
	- `GetPositionsQuery` / `GetPositionsQueryHandler`
	- `UpdatePositionCommand` / `UpdatePositionCommandHandler`
	- `DeletePositionCommand` / `DeletePositionCommandHandler`
- CQRS Players:
	- `UpdatePlayerPositionsCommand` / `UpdatePlayerPositionsCommandHandler`
- Regra de negócio:
	- máximo de 3 posições por jogador
	- bloqueio de `positionIds` duplicados (`DUPLICATE_POSITIONS`)
	- bloqueio de `positionIds` com `Guid.Empty` (`INVALID_POSITION_ID`)
	- bloqueio de deleção de posição em uso (`POSITION_IN_USE`)
- Infrastructure:
	- `PositionRepository`
	- `TenantDbContext` com `DbSet<Position>` e `DbSet<PlayerPosition>`
	- índice único `(TenantId, NormalizedCode)`
	- migration tenant `AddPositionsAndPlayerPositions`
- API:
	- novo `PositionController` com CRUD completo em `/api/v1/position`
	- novo endpoint `PUT /api/v1/player/{id}/positions`
	- `DELETE /api/v1/position/{id}` retorna 409 quando posição está em uso
- Testes novos/atualizados:
	- Unit Domain: `PositionTests`, `PlayerPositionTests`, extensão de `PlayerTests`
	- Unit Application: suítes de commands/queries de Positions e `UpdatePlayerPositionsCommandHandlerTests`
	- Integration: `PositionIntegrationTests` e cenários de `PUT /player/{id}/positions` em `PlayerIntegrationTests`
- Status atual backend: **154 testes, 100% passando**

### Added — Fase 3: Players

- `Player` entity (Domain): `Create()`, `Update()`, `Deactivate()` (soft delete via `IsActive`); `UserId` como FK lógica cross-DB ao `ApplicationUser`
- `IPlayerRepository` — interface de Application com GetByIdAsync, GetAllActiveAsync, ExistsByUserIdAsync, AddAsync, UpdateAsync, SaveChangesAsync
- `PlayerResponse` sealed record DTO
- `CreatePlayerCommand` / `CreatePlayerCommandHandler` — valida nome, verifica UserId no Master DB via `IUserRepository`, impede duplicidade por tenant via `ExistsByUserIdAsync`
- `GetPlayerQuery` / `GetPlayerQueryHandler` — retorna player por Id ou `PLAYER_NOT_FOUND`
- `GetPlayersQuery` / `GetPlayersQueryHandler` — retorna todos os players ativos, ordenados por nome
- `UpdatePlayerCommand` / `UpdatePlayerCommandHandler` — atualiza name/nickname/phone/dateOfBirth
- `DeletePlayerCommand` / `DeletePlayerCommandHandler` — soft delete idempotente (`Deactivate()`)
- `PlayerRepository` — Infrastructure; cria `TenantDbContext` por operação via `TenantDbContextFactory` + `ITenantContext`
- `TenantDbContext` atualizado: `DbSet<Player>`, índice único em `UserId`, `HasMaxLength` em Name(100)/Nickname(50)/Phone(20)
- `TenantDbContextDesignTimeFactory` — `IDesignTimeDbContextFactory` para suporte a `dotnet ef migrations`
- EF Core migration `AddPlayers` — cria tabela `Players` no banco por-tenant (Persistence/Migrations/Tenant/)
- `TenantDbContextFactory` refatorado para `class virtual CreateAsync` (permite override em testes de integração)
- `POST /api/v1/player` → 201 `PlayerResponse` | 404 USER_NOT_FOUND | 409 PLAYER_ALREADY_EXISTS | 422 INVALID_NAME
- `GET /api/v1/player` → 200 `IReadOnlyList<PlayerResponse>`
- `GET /api/v1/player/{id}` → 200 `PlayerResponse` | 404 PLAYER_NOT_FOUND
- `PUT /api/v1/player/{id}` → 200 `PlayerResponse` | 404 PLAYER_NOT_FOUND | 422 INVALID_NAME
- `DELETE /api/v1/player/{id}` → 204 | 404 PLAYER_NOT_FOUND
- `PlayerWebApplicationFactory` + `PlayerIntegrationTests` — SQLite in-memory para Master e Tenant; 5 usuários pré-seedados (um por teste de escrita); `TestTenantDbContextFactory` ignora tenantId
- 32 novos testes (28 unit + 10 integration); total acumulado: 81 testes (100% passando)


### Added — Fase 2: Multi-Tenancy

- `ProvisioningStatus` enum (Pending/InProgress/Ready/Failed) no domínio
- `ITenantContext`, `ITenantRepository`, `IUserTenantRepository`, `ITenantProvisioningQueue` — interfaces de Application
- `CreateTenantCommand` / `CreateTenantCommandHandler` — cria tenant, valida slug único, enfileira provisionamento
- `GetTenantStatusQuery` / `GetTenantStatusQueryHandler` — retorna status de provisionamento
- `TenantRepository` / `UserTenantRepository` — acesso ao Master DB via EF Core
- `RequestTenantContext` — implementação de `ITenantContext` escopada por request
- `TenantProvisioningQueue` — fila interna com `System.Threading.Channels` (singleton)
- `TenantProvisioningWorker` — `BackgroundService` que cria banco SQL Server isolado + migra `TenantDbContext`
- `TenantMiddleware` — lê `X-Tenant-Slug` e popula `ITenantContext`; erros 404 via `NotFoundException`
- `POST /api/v1/tenant` → 201 `TenantResponse` (Pending) | 409 TENANT_SLUG_TAKEN | 422 TENANT_NAME_REQUIRED / TENANT_SLUG_REQUIRED
- `GET /api/v1/tenant/{id}/status` → 200 `TenantResponse` | 404 TENANT_NOT_FOUND
- EF Core migration `AddTenantProvisioningStatus` — adiciona coluna `ProvisioningStatus int NOT NULL DEFAULT 0` na tabela `Tenants`
- `TenantSlugHeaderOperationFilter` — documenta `X-Tenant-Slug` no Swagger para todos os endpoints
- `TestAuthHandler` + `TenantWebApplicationFactory` — infraestrutura de testes de integração com SQLite in-memory e bypass de JWT
- Frontend: `tenantService.ts` com `parseTenantSlug` (subdomain extractor) e `getTenantFromUrl` (subdomain → `?tenant=`)
- Frontend: `router.tsx` com resolução de tenant no `beforeLoad` da rota raiz
- Frontend: `TenantResponse` e `CreateTenantRequest` adicionados a `features/auth/types/index.ts`
- Frontend: MSW handlers para `POST /api/v1/tenant` e `GET /api/v1/tenant/:id/status`
- 49 novos testes (8 unit + 7 integration backend; 14 frontend); total acumulado: 111 testes (100% passando)


### Added — Fase 1: Identity + Auth

- `ApplicationUser : IdentityUser` com propriedades `IsActive` e `CreatedAt`
- `RefreshToken` entity com rotação e revogação (`RevokedAt` nullable)
- Master DB SaaS completo: `Tenant`, `UserTenant`, `Plan`, `Subscription` (enum `SubscriptionStatus`)
- `MasterDbContext : IdentityDbContext<ApplicationUser>` com todas as tabelas e índices
- EF Core migration: `InitialMaster`
- `JwtSettings` com suporte a `SecretKey`, `Issuer`, `Audience`, `AccessTokenExpiresInMinutes`, `RefreshTokenExpiresInDays`
- `JwtTokenService` (HmacSha256, claims: sub/email/jti/iat/role, ClockSkew=Zero)
- `UserRepository` e `RefreshTokenRepository` abstraídos via interfaces de Application
- `LoginCommand` / `LoginCommandHandler` — valida credenciais, gera e persiste token par
- `RefreshTokenCommand` / `RefreshTokenCommandHandler` — valida, revoga e rotaciona refresh token
- `POST /api/v1/auth/login` → 200 `AuthResponse` | 401 INVALID_CREDENTIALS | 422 USER_INACTIVE
- `POST /api/v1/auth/refresh-token` → 200 `AuthResponse` | 401 INVALID_TOKEN/TOKEN_EXPIRED
- Swagger Bearer JWT security definition + requirement globais
- `appsettings.json` / `appsettings.Development.json` com seções `ConnectionStrings:MasterDb` e `Jwt`
- Pacotes: `Microsoft.AspNetCore.Identity.EntityFrameworkCore 10.0.0`, `Microsoft.AspNetCore.Authentication.JwtBearer 10.0.0`, `Microsoft.EntityFrameworkCore.SqlServer 10.0.0`, `System.IdentityModel.Tokens.Jwt 8.9.0`, `Microsoft.EntityFrameworkCore.Sqlite 10.0.0` (Tests)
- 27 testes passando (20 unit + 7 integration)


### Added — Fase 0: Fundação

- Solução `.NET 10` com 5 projetos (`Api`, `Application`, `Domain`, `Infrastructure`, `Tests`)
- Clean Architecture com dependências corretas entre camadas
- Contratos CQRS: `ICommand<T>`, `ICommandHandler<T, R>`, `IQuery<T>`, `IQueryHandler<T, R>`
- `Result<T>` e `Result` para encapsular resultados de operações sem exceção de controle de fluxo
- Primitivos de domínio: `EntityBase`, `IAggregateRoot`, `IRepository<T>`, `IUnitOfWork`, `IDomainEvent`
- Exceções de domínio: `DomainException`, `NotFoundException`, `ValidationException`
- Vertical slice Ping: `PingCommand` + `PingCommandHandler`, `PingQuery` + `PingQueryHandler`, `PingController`
- `GlobalExceptionHandler` com mapeamento de exceções de domínio para `ProblemDetails`
- Swagger/OpenAPI configurado com docs XML
- Testes unitários e de integração para o vertical slice Ping (ciclo TDD Red-Green-Refactor)
- `coverlet` configurado para cobertura de código
- `Directory.Build.props` com configurações compartilhadas de projeto
- GitHub Actions CI: build + testes + cobertura >= 80%
