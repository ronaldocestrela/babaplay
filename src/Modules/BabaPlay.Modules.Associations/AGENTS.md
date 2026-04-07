# AGENTS — BabaPlay.Modules.Associations

## Domínio

**Association** — dados institucionais da associação no tenant (nome, morada, regulamento opcional, **PlayersPerTeam** para geração de times).

## Serviços

- `AssociationService` — listagem, obtenção, upsert (criar ou atualizar conforme `Id` nulo ou não).

## Controllers

- `AssociationsController` — `/api/associations`, autenticado.

## Notas

- Um tenant = tipicamente uma associação; o MVP permite o modelo genérico para evolução futura.
