# 📁 FOLDER_CONTEXT — BabaPlay.Application

## Instrução para LLM

Você está no projeto **BabaPlay.Application**. Esta é a camada de **casos de uso** da Clean Architecture — o orquestrador que coordena o domínio para atender às necessidades do usuário.

## Responsabilidade deste projeto

Orquestra a execução dos casos de uso do sistema usando CQRS (Command Query Responsibility Segregation):

- **Commands/** — Operações de escrita/mutação agrupadas por domínio de negócio
- **Queries/** — Operações de leitura/consulta agrupadas por domínio de negócio
- **DTOs/** — Data Transfer Objects para entrada e saída de dados (desacoplados das Entities)
- **Interfaces/** — Contratos que a Application define para dependências externas (repositórios, serviços de email, storage, etc.)
- **Services/** — Serviços de aplicação para lógica que não pertence a um único Command/Query
- **Common/** — Classes e utilitários transversais (ex: paginação, resultado genérico)

## Padrão CQRS adotado

Cada operação segue o padrão:

```
ICommand<TResult> / IQuery<TResult>   ← define a intenção
ICommandHandler<TCommand, TResult>    ← processa a intenção
IQueryHandler<TQuery, TResult>        ← processa a consulta
```

As pastas dentro de `Commands/` e `Queries/` são organizadas por **domínio de negócio**:

| Domínio               | Exemplos de Commands/Queries                                        |
|-----------------------|---------------------------------------------------------------------|
| `Auth`                | Login, Logout, RefreshToken, ForgotPassword                         |
| `Players`             | CreatePlayer, UpdatePlayer, GetPlayerById, ListPlayers              |
| `Teams`               | CreateTeam, AddPlayerToTeam, GetTeamById                            |
| `Matches`             | CreateMatch, UpdateMatchScore, FinishMatch                          |
| `MatchEvents`         | RegisterMatchEvent, DeleteMatchEvent                                |
| `Checkins`            | CheckinPlayer, ListCheckins                                         |
| `Financial`           | CreateCashTransaction, GetFinancialSummary, RegisterFeePayment      |
| `GameDays`            | CreateGameDay, GetGameDayById, ListGameDays                         |
| `Tenants`             | CreateTenant, UpdateTenant, GetTenantSettings                       |
| `Notifications`       | SendNotification, MarkNotificationAsRead                            |
| `Scores`              | ComputePlayerScore, GetPlayerRanking                                |

## Regras para LLM ao atuar neste projeto

1. **Application não conhece Infrastructure** — use apenas interfaces definidas em `Interfaces/` para acessar repositórios e serviços externos.
2. **Toda operação de escrita** deve ser um `ICommand` com seu `ICommandHandler`. Toda consulta deve ser um `IQuery` com seu `IQueryHandler`.
3. **DTOs são a fronteira** — nunca retorne `Entity` diretamente aos Controllers ou à UI; mapeie para DTO.
4. **Nunca acessar** banco de dados diretamente — sempre via repositório injetado (`IPlayerRepository`, etc.).
5. O arquivo `ServiceRegistration.cs` registra todos os handlers e serviços de aplicação no container DI.
6. Validações de entrada do usuário pertencem a esta camada (ex: FluentValidation nos Commands).
