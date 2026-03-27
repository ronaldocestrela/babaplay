# AGENTS — BabaPlay.Modules.Associates

## Domínio

**Associate**, **Position**, **AssociatePosition** (N:N com regra de negócio).

## Regras

- Entre **1 e 3** posições por associado; validação em `AssociateService.ValidatePositionsAsync`.
- **`IsActive`**: associado inativo não pode fazer login; verificação em `AuthService.LoginAsync` via `IAssociateStatusChecker` (implementação na Infrastructure, consulta `Associate` por `UserId`).

## Serviços

- `AssociateService` — CRUD com gestão de links `AssociatePosition`; `SetActiveAsync` para ativar/desativar. Na **criação**, e-mail é obrigatório; `IAssociateUserProvisioner` cria o utilizador Identity (role Associate) e preenche `Associate.UserId` antes de persistir. Respostas da API usam DTOs em `Dtos/` (`AssociateResponse`, `AssociatePositionInfo`) com projeção EF (`Select`) para evitar ciclos de serialização JSON nas navegações.
- `PositionService` — listar (ordem alfabética por nome), criar, atualizar e eliminar posições (eliminar falha com conflito se estiver atribuída a associados).

## Controllers

- `AssociatesController`, `PositionsController` — `/api/associates` (incl. `PATCH .../active`), `/api/positions`.
