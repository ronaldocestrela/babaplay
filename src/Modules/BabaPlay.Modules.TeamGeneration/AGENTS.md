# AGENTS — BabaPlay.Modules.TeamGeneration

## Domínio

**Team**, **TeamMember**.

## Dependência

- Referência ao projeto **BabaPlay.Modules.CheckIns** para usar entidade `CheckIn` na ordenação por chegada.

## Algoritmo (MVP)

- Ordenar associados pela **primeira** hora de check-in na sessão; distribuição **round-robin** em N equipas (`GenerateFromSessionAsync`); remove equipas anteriores da mesma sessão antes de regenerar.

## Controllers

- `TeamsController` — `/api/teams/generate`, `/api/teams/by-session/{sessionId}`.
