# AGENTS — BabaPlay.Modules.Associates

## Domínio

**Associate**, **Position**, **AssociatePosition** (N:N com regra de negócio).

## Regras

- Entre **1 e 3** posições por associado; validação em `AssociateService.ValidatePositionsAsync`.

## Serviços

- `AssociateService` — CRUD com gestão de links `AssociatePosition`.
- `PositionService` — listar, criar, atualizar e eliminar posições (eliminar falha com conflito se estiver atribuída a associados).

## Controllers

- `AssociatesController`, `PositionsController` — `/api/associates`, `/api/positions`.
