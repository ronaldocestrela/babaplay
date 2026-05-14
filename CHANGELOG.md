# Changelog

Todas as mudanças relevantes deste projeto serão documentadas aqui.

Formato baseado em [Keep a Changelog](https://keepachangelog.com/pt-BR/1.0.0/).

---

## [Unreleased]

### Added — Infra: Deploy manual com Docker (sem CD)

- Estrutura de deploy manual adicionada:
	- `deploy/docker/docker-compose.manual.yml` com `api`, `web` e `sqlserver`
	- `deploy/docker/.env.manual.example` com variaveis de ambiente necessarias para operacao
- Containerizacao do backend:
	- `Backend/Dockerfile` multi-stage (.NET 10 SDK -> ASP.NET runtime)
	- `Backend/.dockerignore` para reduzir contexto de build
- Containerizacao do frontend:
	- `web/Dockerfile` multi-stage (build Vite + runtime Nginx)
	- `web/nginx/default.conf` com fallback SPA (`try_files ... /index.html`)
	- `web/.dockerignore` para reduzir contexto de build
- Documentacao operacional:
	- `docs/deploy-manual-docker.md` com fluxo completo de build, subida, validacao, update, rollback e troubleshooting
- Seguranca operacional:
	- `.gitignore` atualizado para ignorar `deploy/docker/.env.manual` (arquivo local com segredos)

### Added — Fase 16.6: Frontend Partidas (CRUD + status)

- Frontend (Matches base):
	- nova feature `web/src/features/matches/` com:
		- `types/index.ts`
		- `schemas/matchFormSchema.ts`
		- `services/matchService.ts`
		- `hooks/index.ts`
		- `store/matchStore.ts`
		- `components/MatchForm.tsx`, `components/MatchList.tsx`
	- `API_ROUTES` expandido para operações completas de Match:
		- `GET/POST /api/v1/match`
		- `GET/PUT/DELETE /api/v1/match/{id}`
		- `PUT /api/v1/match/{id}/status`
	- `ERROR_CODES` expandido com códigos da feature Match
	- MSW (`src/test/handlers.ts`) ampliado com cenários de sucesso/erro para todos os endpoints de Match
- Frontend (Matches UI):
	- nova `MatchesPage` com:
		- listagem + filtro por confronto/status
		- modal de create/edit
		- mudança de status por linha
		- tratamento de erro por `ProblemDetails.title`
	- rota protegida adicionada: `/matches`
	- header autenticado com navegação para `Partidas`
- Testes TDD adicionados/atualizados:
	- `src/features/matches/services/__tests__/matchService.test.ts`
	- `src/features/matches/hooks/__tests__/matchHooks.test.ts`
	- `src/features/matches/schemas/__tests__/matchFormSchema.test.ts`
	- `src/features/matches/store/__tests__/matchStore.test.ts`
	- `src/features/matches/components/__tests__/MatchForm.test.tsx`
	- `src/features/matches/components/__tests__/MatchList.test.tsx`
	- `src/pages/__tests__/MatchesPage.test.tsx`
	- `src/core/components/__tests__/AuthHeader.test.tsx`
- Validação executada:
	- testes focados (matches + página + header): 42 testes passando
	- suíte web completa: 219 testes passando
	- lint frontend: passando

### Added — Fase 16.5: Frontend Times (CRUD + elenco)

- Frontend (Teams base):
	- nova feature `web/src/features/teams/` com:
		- `types/index.ts`
		- `schemas/teamFormSchema.ts`
		- `services/teamService.ts`
		- `hooks/index.ts`
		- `store/teamStore.ts`
		- `components/TeamForm.tsx`, `components/TeamList.tsx`, `components/TeamRosterEditor.tsx`
	- `API_ROUTES` expandido para operações completas de Teams:
		- `GET/POST /api/v1/team`
		- `GET/PUT/DELETE /api/v1/team/{id}`
		- `PUT /api/v1/team/{id}/players`
	- `ERROR_CODES` expandido com códigos de negócio da feature Teams
	- MSW (`src/test/handlers.ts`) ampliado com cenários de sucesso/erro para todos os endpoints de Teams
- Frontend (Teams UI):
	- nova `TeamsPage` com:
		- listagem + filtro
		- modal de create/edit de time
		- modal de gestão de elenco
		- tratamento de erro por `ProblemDetails.title`
	- rota protegida adicionada: `/teams`
	- header autenticado com navegação para `Times`
- Testes TDD adicionados/atualizados:
	- `src/features/teams/services/__tests__/teamService.test.ts`
	- `src/features/teams/hooks/__tests__/teamHooks.test.ts`
	- `src/features/teams/schemas/__tests__/teamFormSchema.test.ts`
	- `src/features/teams/store/__tests__/teamStore.test.ts`
	- `src/features/teams/components/__tests__/TeamForm.test.tsx`
	- `src/features/teams/components/__tests__/TeamList.test.tsx`
	- `src/features/teams/components/__tests__/TeamRosterEditor.test.tsx`
	- `src/pages/__tests__/TeamsPage.test.tsx`
	- `src/core/components/__tests__/AuthHeader.test.tsx`
- Validação executada:
	- suíte web completa: 185 testes passando
	- lint frontend: passando

### Added — Fase 16.4: Frontend Check-in (slices iniciais)

- Frontend (Check-in base):
	- nova feature `web/src/features/checkin/` com:
		- `types/index.ts`
		- `services/checkinService.ts`
		- `hooks/index.ts`
		- `schemas/checkinFormSchema.ts`
		- `store/checkinStore.ts`
	- `API_ROUTES` expandido para operações completas de Check-in:
		- `POST /api/v1/checkin`
		- `GET /api/v1/checkin/gameday/{gameDayId}`
		- `GET /api/v1/checkin/player/{playerId}`
		- `DELETE /api/v1/checkin/{id}`
	- `ERROR_CODES` expandido com códigos de negócio de check-in
	- MSW (`src/test/handlers.ts`) ampliado com cenários de sucesso e erro para create/list/cancel
- Frontend (Check-in UI inicial):
	- nova `CheckinsPage` com:
		- formulário de criação
		- captura de geolocalização automática via navegador com fallback manual
		- listagem de check-ins e cancelamento
		- tratamento de erro por código (`ProblemDetails.title`)
		- visualização geográfica inicial (base para evolução do mapa)
	- rota protegida adicionada: `/checkins`
	- header autenticado com navegação para `Check-ins`
- Testes TDD adicionados/atualizados:
	- `src/features/checkin/services/__tests__/checkinService.test.ts`
	- `src/features/checkin/hooks/__tests__/checkinHooks.test.ts`
	- `src/features/checkin/schemas/__tests__/checkinFormSchema.test.ts`
	- `src/features/checkin/store/__tests__/checkinStore.test.ts`
	- `src/pages/__tests__/CheckinsPage.test.tsx`
	- `src/core/components/__tests__/AuthHeader.test.tsx`
- Validação executada:
	- testes focados (checkin + navegação): 29 testes passando
	- suíte web completa: 139 testes passando
	- lint frontend: passando

### Changed — Fase 16.4: Frontend Check-in (slice 3 componentização)

- Refatoração da página de Check-ins para componentes dedicados:
	- `src/features/checkin/components/CheckinForm.tsx`
	- `src/features/checkin/components/CheckinList.tsx`
	- `src/features/checkin/components/CheckinMap.tsx`
- Página `CheckinsPage` simplificada para orquestração de estado e mutações, mantendo fluxo funcional existente.
- Testes TDD adicionados para os novos componentes:
	- `src/features/checkin/components/__tests__/CheckinForm.test.tsx`
	- `src/features/checkin/components/__tests__/CheckinList.test.tsx`
- Validação executada:
	- testes focados (checkin + página): 26 testes passando
	- suíte web completa: 143 testes passando
	- lint frontend: passando

### Changed — Fase 16.4: Frontend Check-in (slice 4 seleção guiada)

- Formulário de Check-in evoluído para seleção guiada (sem entrada manual de UUID):
	- seleção de jogador
	- seleção de dia de jogo
- Data layer da feature ampliado:
	- `checkinService.getPlayersForCheckin()`
	- `checkinService.getGameDaysForCheckin()`
	- hooks `useCheckinPlayers()` e `useCheckinGameDays()`
- Validação do schema ajustada para fluxo guiado com IDs obrigatórios não vazios.
- Testes TDD atualizados/expandidos:
	- `src/features/checkin/services/__tests__/checkinService.test.ts`
	- `src/features/checkin/hooks/__tests__/checkinHooks.test.ts`
	- `src/features/checkin/schemas/__tests__/checkinFormSchema.test.ts`
	- `src/features/checkin/components/__tests__/CheckinForm.test.tsx`
	- `src/pages/__tests__/CheckinsPage.test.tsx`
- Validação executada:
	- testes focados de checkin: 30 testes passando
	- suíte web completa: 147 testes passando
	- lint frontend: passando

### Changed — Fase 16.4: Frontend Check-in (slice 5 hardening de UX)

- Feedback operacional adicionado no fluxo de check-in:
	- mensagem de sucesso ao registrar check-in
	- mensagem de sucesso ao cancelar check-in
	- mensagens de erro de ação resolvidas por código de domínio
- Pending state de cancelamento refinado por item na listagem:
	- apenas o item em progresso entra em estado `Cancelando...`
	- bloqueio granular de ação por item
- Testes TDD atualizados/expandidos:
	- `src/features/checkin/components/__tests__/CheckinList.test.tsx`
	- `src/pages/__tests__/CheckinsPage.test.tsx`
- Validação executada:
	- testes focados de checkin: 32 testes passando
	- suíte web completa: 149 testes passando
	- lint frontend: passando

### Changed — Fase 16.4: Frontend Check-in (slice 6 mapa real)

- Integração de provider real de mapa no componente de check-in:
	- `react-leaflet` + `leaflet`
	- `TileLayer` com OpenStreetMap
	- `CircleMarker` na posição atual capturada (latitude/longitude)
	- fallback visual quando coordenadas não estão disponíveis ou são inválidas
- Testes TDD adicionados/expandidos:
	- `src/features/checkin/components/__tests__/CheckinMap.test.tsx`
- Validação executada:
	- testes focados de checkin: 34 testes passando
	- suíte web completa: 151 testes passando
	- lint frontend: passando

### Added — Fase 16.3: Frontend Players (slices 1 e 2)

- Frontend (Players base):
	- nova feature `web/src/features/players/` com:
		- `types/index.ts`
		- `services/playerService.ts`
		- `hooks/index.ts`
		- `schemas/playerFormSchema.ts`
		- `store/playerStore.ts`
	- `API_ROUTES` expandido para operações de Players e Positions:
		- `GET/POST /api/v1/player`
		- `GET/PUT/DELETE /api/v1/player/{id}`
		- `PUT /api/v1/player/{id}/positions`
		- `GET /api/v1/position`
	- `ERROR_CODES` expandido com códigos da feature Players/Positions
	- MSW (`src/test/handlers.ts`) expandido com cenários de listagem, detalhe, create, update, delete e update positions
- Frontend (Players UI):
	- nova `PlayersPage` com:
		- listagem com filtro por nome/apelido
		- modal único para criar/editar
		- ações de excluir
		- atualização de posições no fluxo de salvar
		- fallback para `FORBIDDEN`
	- rota protegida adicionada: `/players`
- Testes TDD adicionados:
	- `src/features/players/services/__tests__/playerService.test.ts`
	- `src/features/players/hooks/__tests__/playersHooks.test.ts`
	- `src/features/players/schemas/__tests__/playerFormSchema.test.ts`
	- `src/features/players/store/__tests__/playerStore.test.ts`
	- `src/pages/__tests__/PlayersPage.test.tsx`
- Validação executada:
	- testes focados da fase 16.3: 27 testes passando
	- suíte web completa: 107 testes passando
	- lint frontend: passando

### Changed — Fase 16.3: Frontend Players (slice 3 hardening)

- Header autenticado:
	- adicionado acesso rápido para `Dashboard` e `Jogadores`
	- navegação para `/players` integrada no header principal
- PlayersPage:
	- estados pending para ações de salvar/excluir
	- botões de ação desabilitados durante mutações para evitar concorrência de comandos
	- mapeamento de mensagens para erros avançados de posições:
		- `POSITION_NOT_FOUND`
		- `DUPLICATE_POSITIONS`
		- `POSITIONS_LIMIT_EXCEEDED`
- MSW:
	- endpoint `PUT /api/v1/player/{id}/positions` ampliado com cenários de erro avançados
- Testes TDD adicionados/atualizados:
	- `src/core/components/__tests__/AuthHeader.test.tsx`
	- `src/features/players/hooks/__tests__/playersHooks.test.ts`
	- `src/pages/__tests__/PlayersPage.test.tsx`
- Validação executada:
	- testes focados (header + hooks + players page): 25 testes passando
	- suíte web completa: 116 testes passando
	- lint frontend: passando

### Added — Fase 16: Frontend Dashboard (slice 1 e 2)

- Frontend (Dashboard MVP):
	- nova feature `web/src/features/dashboard/` com:
		- `types/index.ts`
		- `services/dashboardService.ts`
		- `hooks/useDashboardData.ts`
	- `DashboardPage` evoluída de placeholder para tela funcional com:
		- KPIs operacionais
		- widget de ranking (melhor score, top artilharia, top presença)
		- widget financeiro (saldo, aberto, mensalidades pagas)
		- tratamento de loading/erro e degradação por permissão em blocos (`FORBIDDEN`)
	- `API_ROUTES` expandido para endpoints de leitura usados pelo dashboard
	- MSW (`src/test/handlers.ts`) expandido com mocks de players/teams/gamedays/matches/checkins/ranking/financeiro
- Frontend (Filtro de período):
	- seleção entre `Mês atual` e `Personalizado`
	- inputs `De` e `Até` com aplicação explícita do intervalo
	- propagação de `fromUtc`/`toUtc` para ranking e financeiro
	- cache key do React Query parametrizada por período
- Testes TDD adicionados/atualizados:
	- `src/features/dashboard/services/__tests__/dashboardService.test.ts`
	- `src/features/dashboard/hooks/__tests__/useDashboardData.test.ts`
	- `src/pages/__tests__/DashboardPage.test.tsx`
- Validação executada:
	- testes focados dashboard: 9 passando (slice 1) + 7 passando (slice 2)
	- suíte web completa: 78 testes passando
	- lint frontend: passando

### Added — Fase 14: Financeiro (slice 4 hardening)

- Application hardening:
	- `ReverseMonthlyFeePaymentCommandHandler` com validação explícita de tenant (`TENANT_NOT_RESOLVED`)
	- `GetMonthlySummaryQueryHandler` com validação de ano (`1..9999`) para evitar exceções de data inválida
- Testes unitários adicionados:
	- `ReverseMonthlyFeePaymentCommandHandlerTests`
	- `GetCashFlowQueryHandlerTests`
	- `GetDelinquencyQueryHandlerTests`
	- `GetMonthlySummaryQueryHandlerTests`
	- `GetPlayerStatementQueryHandlerTests`
- Validação executada:
	- filtro `FullyQualifiedName~Unit.Application.Financial|FullyQualifiedName~FinancialIntegrationTests|FullyQualifiedName~RbacIntegrationTests`: 28 testes passando
	- suíte completa backend: 451 testes passando

### Added — Fase 14: Financeiro (slice 3 API + RBAC + leituras)

- Application:
	- command/handler de estorno: `ReverseMonthlyFeePaymentCommand` / `ReverseMonthlyFeePaymentCommandHandler`
	- queries/handlers:
		- `GetCashFlowQuery` / `GetCashFlowQueryHandler`
		- `GetDelinquencyQuery` / `GetDelinquencyQueryHandler`
		- `GetMonthlySummaryQuery` / `GetMonthlySummaryQueryHandler`
		- `GetPlayerStatementQuery` / `GetPlayerStatementQueryHandler`
	- DTOs de relatório e extrato financeiro adicionados
- API:
	- novo `FinancialController` com endpoints de write/read/approve para caixa, mensalidades, pagamentos, estorno e relatórios
- RBAC:
	- policies `FinancialRead`, `FinancialWrite`, `FinancialApprove`
	- permissões `financial.read`, `financial.write`, `financial.approve` no `RbacCatalog`
	- matriz default atualizada (`Admin`: read/write/approve, `Manager`: read/write)
- Infrastructure:
	- `MonthlyFeePaymentRepository` expandido com `GetByIdAsync` e `UpdateAsync`
	- repositórios financeiros expandidos para consultas de período/competência/extrato
- Testes adicionados/atualizados:
	- `FinancialIntegrationTests`
	- `RbacIntegrationTests` com cenários financeiros
- Validação executada:
	- filtro `FullyQualifiedName~FinancialIntegrationTests|FullyQualifiedName~RbacIntegrationTests|FullyQualifiedName~Financial`: 15 testes passando
	- suíte completa backend: 438 testes passando

### Added — Fase 14: Financeiro (slice 1 e write inicial)

- Domain:
	- `CashTransaction` entity com `Create` e `Deactivate`
	- `PlayerMonthlyFee` entity com `Create`, `ApplyPayment`, `MarkOverdue`, `Cancel`, `Deactivate`
	- `MonthlyFeePayment` entity com `Create`, `Reverse`, `Deactivate`
	- enums `CashTransactionType` e `MonthlyFeeStatus`
	- value object `BillingCompetence`
- Application:
	- contratos `ICashTransactionRepository`, `IPlayerMonthlyFeeRepository`, `IMonthlyFeePaymentRepository`
	- DTOs `CashTransactionResponse`, `PlayerMonthlyFeeResponse`, `MonthlyFeePaymentResponse`
	- CQRS (write):
		- `CreateCashTransactionCommand` / `CreateCashTransactionCommandHandler`
		- `CreatePlayerMonthlyFeeCommand` / `CreatePlayerMonthlyFeeCommandHandler`
		- `RegisterMonthlyFeePaymentCommand` / `RegisterMonthlyFeePaymentCommandHandler`
- Regras aplicadas:
	- validação de valor > 0 para transações e pagamentos
	- validação UTC para datas operacionais
	- cálculo de `SignedAmount` para caixa (despesa negativa)
	- pagamento parcial/total de mensalidade com transição para `Paid` no total
	- mensalidade paga não pode ser cancelada
	- estorno idempotente em `MonthlyFeePayment`
- Testes TDD adicionados:
	- Unit Domain: `CashTransactionTests`, `PlayerMonthlyFeeTests`, `MonthlyFeePaymentTests`, `BillingCompetenceTests`
	- Unit Application: `CreateCashTransactionCommandHandlerTests`, `CreatePlayerMonthlyFeeCommandHandlerTests`, `RegisterMonthlyFeePaymentCommandHandlerTests`
- Validação executada:
	- filtro `FullyQualifiedName~CashTransactionTests|FullyQualifiedName~PlayerMonthlyFeeTests|FullyQualifiedName~MonthlyFeePaymentTests|FullyQualifiedName~BillingCompetenceTests|FullyQualifiedName~CreateCashTransactionCommandHandlerTests|FullyQualifiedName~CreatePlayerMonthlyFeeCommandHandlerTests|FullyQualifiedName~RegisterMonthlyFeePaymentCommandHandlerTests`: 24 testes passando

### Added — Fase 14: Financeiro (slice 2 de infraestrutura)

- Infrastructure:
	- `TenantDbContext` atualizado com `DbSet<CashTransaction>`, `DbSet<PlayerMonthlyFee>`, `DbSet<MonthlyFeePayment>`
	- mapeamentos EF e índices financeiros por tenant para caixa, mensalidades e pagamentos
	- repositórios adicionados:
		- `CashTransactionRepository`
		- `PlayerMonthlyFeeRepository`
		- `MonthlyFeePaymentRepository`
	- `ServiceRegistration` atualizado para registrar contratos financeiros da camada Application
- Banco (tenant):
	- migration `AddFinancialCore` criada em `Persistence/Migrations/Tenant/`
	- `TenantDbContextModelSnapshot` atualizado
- Validação executada:
	- filtro `FullyQualifiedName~CreateCashTransactionCommandHandlerTests|FullyQualifiedName~CreatePlayerMonthlyFeeCommandHandlerTests|FullyQualifiedName~RegisterMonthlyFeePaymentCommandHandlerTests`: 6 testes passando

### Added — Fase 13: Notificações (slice 1 inicial)

- Domain:
	- `NotificationType` enum
	- `UserDeviceToken` entity com `Create`, `RotateToken`, `Deactivate`
	- `UserNotificationPreferences` entity com `CreateDefault`, `Update`, `Deactivate`
	- `Notification` entity com `Create`, `MarkAsRead`, `Deactivate`
- Application:
	- DTOs `DeviceTokenResponse` e `UserNotificationPreferencesResponse`
	- contratos `IUserDeviceTokenRepository` e `IUserNotificationPreferencesRepository`
	- CQRS:
		- `RegisterDeviceTokenCommand` / `RegisterDeviceTokenCommandHandler`
		- `UpdateNotificationPreferencesCommand` / `UpdateNotificationPreferencesCommandHandler`
- Regras aplicadas:
	- validação de `UserId`, `DeviceId`, `Token`, `Platform` no registro de token
	- rotação de token quando o device já existe
	- criação padrão de preferências com atualização no mesmo comando
	- idempotência de desativação e marcação de leitura no domínio
- Testes TDD adicionados:
	- Unit Domain: `UserDeviceTokenTests`, `UserNotificationPreferencesTests`, `NotificationTests`
	- Unit Application: `RegisterDeviceTokenCommandHandlerTests`, `UpdateNotificationPreferencesCommandHandlerTests`
- Validação executada:
	- filtro `FullyQualifiedName~RegisterDeviceTokenCommandHandlerTests|FullyQualifiedName~UpdateNotificationPreferencesCommandHandlerTests|FullyQualifiedName~UserDeviceTokenTests|FullyQualifiedName~UserNotificationPreferencesTests|FullyQualifiedName~NotificationTests`: 18 testes passando

### Added — Fase 12: Score e Ranking (slices 1 e 2)

- Slice 1 — Domínio + cálculo:
	- `PlayerScore` entity com contadores, score total e operações `ApplyDelta`/`ReplaceBreakdown`
	- value objects `ScoreBreakdown` (fórmula oficial) e `RankingPeriod` (validação de UTC e intervalo)
	- `IScoreComputationService` + `ScoreComputationService`
	- testes TDD: `PlayerScoreTests`, `ScoreBreakdownTests`, `RankingPeriodTests`, `ScoreComputationServiceTests`
- Slice 2 — Persistência + CQRS leitura:
	- contrato `IPlayerScoreRepository`
	- queries/handlers:
		- `GetRankingQuery` / `GetRankingQueryHandler`
		- `GetTopScorersQuery` / `GetTopScorersQueryHandler`
		- `GetAttendanceRankingQuery` / `GetAttendanceRankingQueryHandler`
	- DTOs: `RankingEntryResponse`, `TopScorerEntryResponse`, `AttendanceEntryResponse`
	- `PlayerScoreRepository` com ordenações específicas (ranking, artilharia, presença), filtro de período e paginação
	- `TenantDbContext` atualizado com `DbSet<PlayerScore>` + índices de consulta
	- migration tenant `AddPlayerScores`
- Regras/validações adicionadas:
	- `INVALID_PERIOD` para combinação inválida de `FromUtc`/`ToUtc`
	- rank calculado por offset de página
- Validação executada:
	- testes da Slice 2: 7 passando
	- regressão focada Slices 1+2: 20 passando

### Added — Fase 12: Score e Ranking (slice 3)

- Application:
	- `ApplyScoreDeltaCommand` / `ApplyScoreDeltaCommandHandler`
- Regras de atualização incremental:
	- deduplicação por `SourceEventId` (`DUPLICATE_SCORE_EVENT`)
	- criação automática de `PlayerScore` para primeiro evento positivo
	- retorno `PLAYER_SCORE_NOT_FOUND` para delta negativo sem score prévio
	- retorno `INVALID_SCORE_DELTA` para deltas que invalidam contadores
- Domain:
	- nova entidade `PlayerScoreSourceEvent` para registrar origem processada
- Infrastructure:
	- `IPlayerScoreRepository` expandido com `HasProcessedSourceEventAsync` e `AddProcessedSourceEventAsync`
	- `PlayerScoreRepository` com suporte à trilha de eventos processados
	- `TenantDbContext` com `DbSet<PlayerScoreSourceEvent>` e índice único `(TenantId, SourceEventId)`
	- migration tenant `AddPlayerScoreSourceEvents`
- Testes TDD adicionados:
	- `ApplyScoreDeltaCommandHandlerTests`
- Validação executada:
	- testes da Slice 3: 5 passando
	- regressão focada Slices 1+2+3: 25 passando

### Added — Fase 12: Score e Ranking (slice 4)

- Application:
	- `RebuildTenantRankingCommand` / `RebuildTenantRankingCommandHandler`
	- DTO `RebuildRankingResponse`
	- validação de período (`INVALID_PERIOD`) e falha operacional (`RANKING_REBUILD_FAILED`)
- API:
	- novo `RankingController` com endpoints:
		- `GET /api/v1/ranking`
		- `GET /api/v1/ranking/top-scorers`
		- `GET /api/v1/ranking/attendance`
		- `POST /api/v1/ranking/rebuild`
- RBAC/Autorização:
	- policies adicionadas: `RankingRead`, `RankingWrite`
	- permissões adicionadas: `ranking.read`, `ranking.write`
	- `RbacCatalog` atualizado com matriz padrão para ranking
- Infrastructure:
	- `IPlayerScoreRepository` expandido com `GetAllActiveForRebuildAsync`
	- `PlayerScoreRepository` atualizado para rebuild administrativo
	- `PlayerScoreSourceEvent` mapeado no `TenantDbContext`
	- migration tenant `AddPlayerScoreSourceEvents`
- Testes TDD adicionados:
	- Unit: `RebuildTenantRankingCommandHandlerTests`
	- Integration: `RankingIntegrationTests` (sucesso, sem permissão e tenant inválido)
- Validação executada:
	- testes da Slice 4: 9 passando
	- regressão focada (Fase 12 + RBAC): 24 passando

### Added — Fase 11: Súmula (backend MVP concluído)

- Domain:
	- `MatchSummary` entity com `Create` e `Deactivate`
	- validações de `TenantId`, `MatchId`, `StoragePath`, `FileName`, `ContentType`, `SizeBytes`
- Application:
	- contratos `IMatchSummaryRepository`, `IMatchSummaryPdfGenerator`, `IMatchSummaryStorageService`
	- DTOs `MatchSummaryResponse` e `MatchSummaryFileResponse`
	- CQRS: `GenerateMatchSummaryCommand`, `GetMatchSummaryByMatchQuery`, `GetMatchSummaryFileQuery`
- Regras de negócio aplicadas:
	- geração apenas para partida `Completed` (`MATCH_NOT_COMPLETED`)
	- bloqueio de duplicidade por partida (`MATCH_SUMMARY_ALREADY_EXISTS`)
	- validações de inexistência de partida/arquivo (`MATCH_NOT_FOUND`, `MATCH_SUMMARY_FILE_NOT_FOUND`)
- Infrastructure:
	- `MatchSummaryRepository`
	- `LocalMatchSummaryStorageService` (filesystem local MVP)
	- `MinimalPdfMatchSummaryGenerator` (PDF mínimo válido sem dependência externa)
	- `TenantDbContext` com `DbSet<MatchSummary>` e índice único `(TenantId, MatchId)`
- API:
	- novo `MatchSummaryController`
	- `POST /api/v1/match-summary`
	- `GET /api/v1/match-summary/match/{matchId}`
	- `GET /api/v1/match-summary/{summaryId}/file`
- Testes TDD adicionados:
	- Unit Domain: `MatchSummaryTests`
	- Unit Application: `GenerateMatchSummaryCommandHandlerTests`, `GetMatchSummaryByMatchQueryHandlerTests`
	- Integration: `MatchSummaryIntegrationTests`
- Execução de validação:
	- filtro `FullyQualifiedName~MatchSummary`: 15 testes passando
	- regressão backend completa: 330 testes passando

### Changed — Fase 11: Súmula (complementação do slice)

- Banco (tenant):
	- migration `AddMatchSummaries` adicionada em `Persistence/Migrations/Tenant`
	- `TenantDbContextModelSnapshot` atualizado para incluir `MatchSummaries`
- Testes:
	- novo `GetMatchSummaryFileQueryHandlerTests`
	- novos testes de infraestrutura para `LocalMatchSummaryStorageService` e `MinimalPdfMatchSummaryGenerator`
	- `MatchSummaryIntegrationTests` expandido com cenários negativos (match inexistente, match não concluída, summary/arquivo inexistentes)
- Hardening:
	- `LocalMatchSummaryStorageService` com root configurável via `MatchSummaryStorage:RootPath`
	- validação de path traversal na leitura de arquivo
	- isolamento de storage em integração com cleanup automático no `PlayerWebApplicationFactory`
- Configuração:
	- `appsettings.json` e `appsettings.Development.json` com seção `MatchSummaryStorage`
- Validação atualizada:
	- filtro `FullyQualifiedName~MatchSummary`: 27 testes passando
	- regressão backend completa: 342 testes passando

### Changed — Fase 11: fechamento oficial

- Status da fase atualizado para concluída no roadmap com escopo explícito de backend MVP.
- Delimitação de escopo futuro adicionada (template PDF avançado, versionamento/auditoria e UI Web/Mobile) como incrementos não bloqueantes.

### Added — Fase 11: endpoints operacionais adicionais

- API:
	- `GET /api/v1/match-summary/{summaryId}` para metadata por id
	- `DELETE /api/v1/match-summary/{summaryId}` para desativação da súmula
- Application (CQRS):
	- `GetMatchSummaryQuery` / `GetMatchSummaryQueryHandler`
	- `DeleteMatchSummaryCommand` / `DeleteMatchSummaryCommandHandler`
- Storage:
	- `IMatchSummaryStorageService.DeleteAsync(...)`
	- implementação em `LocalMatchSummaryStorageService` com validação de path traversal
- Testes:
	- Unit Application: `GetMatchSummaryQueryHandlerTests`, `DeleteMatchSummaryCommandHandlerTests`
	- Unit Infrastructure: cenários de `DeleteAsync` em `LocalMatchSummaryStorageServiceTests`
	- Integration: cenários de GET por id e DELETE em `MatchSummaryIntegrationTests`
- Validação atualizada:
	- filtro `FullyQualifiedName~MatchSummary`: 39 testes passando
	- regressão backend completa: 354 testes passando

### Changed — Fase 11: hardening de persistência

- Application:
	- `GenerateMatchSummaryCommandHandler` agora realiza cleanup do arquivo no storage quando ocorre exceção ao persistir metadata no banco.
	- novo erro retornado no fluxo de geração: `MATCH_SUMMARY_PERSISTENCE_FAILED`.
- Testes TDD:
	- `GenerateMatchSummaryCommandHandlerTests` recebeu cenário de falha de persistência com validação explícita de chamada ao `DeleteAsync` do storage.
- Validação atualizada:
	- filtro `FullyQualifiedName~MatchSummary`: 40 testes passando

### Changed — Fase 9: Partidas (hardening RBAC + TDD)

- Segurança/RBAC:
	- novas permissões em `RbacCatalog`: `matches.read`, `matches.write`
	- novas policies: `MatchesRead` e `MatchesWrite`
	- `MatchController` atualizado para exigir `TenantMember` e policies por endpoint (read/write)
- Testes:
	- adicionados cenários unitários para `CreateMatchCommandHandler` e `UpdateMatchCommandHandler` cobrindo IDs vazios (`GameDayId`, `HomeTeamId`, `AwayTeamId`)
	- adicionado cenário unitário para times iguais no update (`TEAMS_MUST_BE_DIFFERENT`)
	- adicionado cenário de integração em `MatchIntegrationTests` para `POST /api/v1/match` com times iguais retornando `422`
	- `PlayerWebApplicationFactory` atualizado para seed RBAC padrão no tenant de testes e atribuição de role aos usuários de integração
- Validação:
	- regressão filtrada de Match: 73 testes passando
	- regressão completa backend com cobertura: 315 testes passando

### Added — Fase 10: MatchEvents (backend em andamento)

- Domain:
	- `MatchEvent` entity com `Create`, `Update`, `Deactivate` e validação de minuto (0-130)
	- `MatchEventType` entity configurável por tenant com pontuação (`Points`) e flag `IsSystemDefault`
- Application:
	- contratos `IMatchEventRepository`, `IMatchEventTypeRepository`, `IMatchEventRealtimeNotifier`
	- DTOs `MatchEventResponse` e `MatchEventTypeResponse`
	- MatchEventType CQRS: `Create`, `Update`, `Delete`, `GetById`, `GetAll`
	- MatchEvent CQRS: `Create`, `Update`, `Delete`, `GetById`, `GetByMatch`, `GetByPlayer`
- Infrastructure:
	- `MatchEventRepository` e `MatchEventTypeRepository`
	- `TenantDbContext` com `DbSet<MatchEvent>` e `DbSet<MatchEventType>`
	- mapeamentos e índices para consulta por partida, jogador e código normalizado por tenant
	- SignalR com `MatchHub` e `SignalRMatchEventRealtimeNotifier`
	- migration tenant `AddMatchEventsAndTypes`
	- seed idempotente de tipos padrão no provisioning (`goal`, `yellow_card`, `red_card`)
- API:
	- `MatchEventController` com CRUD e listagens por partida/jogador
	- `MatchEventTypeController` com CRUD de catálogo configurável por tenant
	- `Program.cs` com `MapHub<MatchHub>("/hubs/match")`
	- autorização por policy aplicada para leitura/escrita de MatchEvents e MatchEventTypes
	- novas policies/padrões RBAC adicionados para fase 10
- Testes TDD adicionados:
	- Unit Domain: `MatchEventTests`, `MatchEventTypeTests`
	- Unit Application:
		- MatchEvent: `CreateMatchEventCommandHandlerTests`, `UpdateMatchEventCommandHandlerTests`, `DeleteMatchEventCommandHandlerTests`, `GetMatchEventQueryHandlerTests`, `GetMatchEventsByMatchQueryHandlerTests`, `GetMatchEventsByPlayerQueryHandlerTests`
		- MatchEventType: `CreateMatchEventTypeCommandHandlerTests`, `UpdateMatchEventTypeCommandHandlerTests`, `DeleteMatchEventTypeCommandHandlerTests`, `GetMatchEventTypeQueryHandlerTests`, `GetMatchEventTypesQueryHandlerTests`
- Execução de validação:
	- filtro `FullyQualifiedName~MatchEvents`: 25 testes passando
	- regressão backend completa: 307 testes passando

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
