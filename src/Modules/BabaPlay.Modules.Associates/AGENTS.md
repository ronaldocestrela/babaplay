# AGENTS — BabaPlay.Modules.Associates

## Domínio

**Associate**, **Position**, **AssociatePosition** (N:N com regra de negócio).

## Regras

- Entre **1 e 3** posições por associado; validação em `AssociateService.ValidatePositionsAsync`.

## Serviços

- `AssociateService` — CRUD com gestão de links `AssociatePosition`.
- `PositionService` — listar e criar posições.

## Controllers

- `AssociatesController`, `PositionsController` — `/api/associates`, `/api/positions`.
