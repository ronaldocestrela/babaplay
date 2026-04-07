# AGENTS — BabaPlay.Modules.TeamGeneration

## Domínio

**Team**, **TeamMember**.

## Dependência

- Referência ao projeto **BabaPlay.Modules.CheckIns** para usar entidade `CheckIn` na ordenação por chegada.
- Referência ao projeto **BabaPlay.Modules.Associations** para ler `Association.PlayersPerTeam` e calcular a quantidade de times.

## Algoritmo (MVP)

- Ordenar associados pela **primeira** hora de check-in na sessão; número de equipas = `max(2, totalCheckedIn / PlayersPerTeam)` (configurado na associação); distribuição **round-robin** (`GenerateFromSessionAsync`); remove equipas anteriores da mesma sessão antes de regenerar.

## Controllers

- `TeamsController` — `/api/teams/generate`, `/api/teams/by-session/{sessionId}`.
